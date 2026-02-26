using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.Collections;
using NexusMods.Abstractions.Loadouts;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.TxFunctions;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Jobs;
using NexusMods.Sdk.Loadouts;
using R3;

namespace NexusMods.Games.RedEngine.Cyberpunk2077;

public class CyberpunkDeepCleanTool : ITool
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<CyberpunkDeepCleanTool> _logger;
    private readonly ISynchronizerService _synchronizerService;
    private readonly IConnection _connection;

    public CyberpunkDeepCleanTool(IFileSystem fileSystem, ILogger<CyberpunkDeepCleanTool> logger, ISynchronizerService synchronizerService, IConnection connection)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _synchronizerService = synchronizerService;
        _connection = connection;
    }

    public IEnumerable<GameId> GameIds => [Cyberpunk2077Game.GameId];
    public string Name => "Deep Clean (Disable all mods)";

    // Paths to move to a timestamped backup directory (mirrors the bash script by manavortex).
    // IMPORTANT: Only mod-specific paths are included here. The original bash script also moves
    // engine/config/base, engine/config/galaxy, engine/config/platform/pc, r6/cache, r6/config,
    // and r6/input — but those are base game files. Steam users can restore them via "Verify game
    // files", but we have no Steam, so we leave them untouched.
    private static readonly string[] PathsToMove =
    [
        "archive/pc/mod",
        "mods",
        "bin/x64/plugins",
        "r6/scripts",
        "r6/tweaks",
        "red4ext",
        "engine/tools",
        "bin/x64/d3d11.dll",
        "bin/x64/global.ini",
        "bin/x64/powrprof.dll",
        "bin/x64/winmm.dll",
        "bin/x64/version.dll",
    ];

    private static readonly string[] PathsToDelete = ["V2077"];

    public async Task Execute(Loadout.ReadOnly loadout, CancellationToken cancellationToken)
    {
        var gamePath = loadout.InstallationInstance.Locations[LocationId.Game].Path;
        _logger.LogInformation("Starting deep clean for Cyberpunk 2077 at {Path}", gamePath);

        // Step 1: Move mod files to a timestamped backup directory outside the game folder.
        // Keeping backups outside the game folder prevents the sync from tracking or trying to restore them.
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupsRoot = _fileSystem.GetKnownPath(KnownPath.XDG_DATA_HOME)
            .Combine("NexusMods.App")
            .Combine("CyberpunkBackups");
        var backupDir = backupsRoot.Combine(RelativePath.FromUnsanitizedInput(timestamp));
        var backupCreated = false;

        foreach (var relativePath in PathsToMove)
        {
            var fullPath = gamePath.Combine(RelativePath.FromUnsanitizedInput(relativePath));
            if (!fullPath.DirectoryExists() && !fullPath.FileExists) continue;

            if (!backupCreated)
            {
                backupDir.CreateDirectory();
                backupCreated = true;
            }

            var destination = backupDir.Combine(RelativePath.FromUnsanitizedInput(relativePath));
            if (!destination.Parent.DirectoryExists())
                destination.Parent.CreateDirectory();

            try
            {
                if (fullPath.DirectoryExists())
                    System.IO.Directory.Move(fullPath.ToString(), destination.ToString());
                else
                    System.IO.File.Move(fullPath.ToString(), destination.ToString());

                _logger.LogInformation("Moved {Path} to backup", relativePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to move {Path} to backup", relativePath);
            }
        }

        foreach (var relativePath in PathsToDelete)
        {
            var fullPath = gamePath.Combine(RelativePath.FromUnsanitizedInput(relativePath));
            try
            {
                if (fullPath.DirectoryExists())
                {
                    fullPath.DeleteDirectory(true);
                    _logger.LogInformation("Deleted directory {Path}", relativePath);
                }
                else if (fullPath.FileExists)
                {
                    fullPath.Delete();
                    _logger.LogInformation("Deleted file {Path}", relativePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete {Path}", relativePath);
            }
        }

        if (backupCreated)
            _logger.LogInformation("Mod files backed up to {Path}", backupDir);
        else
            _logger.LogInformation("No mod files found to back up");

        // Step 2: Delete previous backup folders created by earlier deep cleans.
        // This keeps the CyberpunkBackups directory clean over time.
        try
        {
            if (backupsRoot.DirectoryExists())
            {
                var deletedBackups = 0;
                foreach (var oldBackupDir in System.IO.Directory.GetDirectories(backupsRoot.ToString()))
                {
                    var dirName = System.IO.Path.GetFileName(oldBackupDir);
                    // Skip the backup we just created
                    if (dirName == timestamp) continue;
                    try
                    {
                        System.IO.Directory.Delete(oldBackupDir, true);
                        deletedBackups++;
                        _logger.LogInformation("Deleted old backup: {Dir}", dirName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old backup: {Dir}", dirName);
                    }
                }
                if (deletedBackups > 0)
                    _logger.LogInformation("Deleted {Count} old backup folder(s)", deletedBackups);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clean old backups");
        }

        // Step 3: Remove all mod groups and collections from the loadout database.
        // This ensures the app state is fully reset, not just disabled.
        // Library items and archives are preserved so mods can be re-installed without re-downloading.
        try
        {
            var db = _connection.Db;
            using var tx = _connection.BeginTransaction();
            var removedCount = 0;

            // Find all top-level LoadoutItemGroups (direct children of the loadout, not nested mod groups)
            // and delete them recursively. This removes:
            //   - Regular mod groups (LoadoutItemGroup)
            //   - Collection groups (NexusCollectionLoadoutGroup via CollectionGroup → LoadoutItemGroup)
            //   - Their nested mod groups and all LoadoutFiles within
            foreach (var item in LoadoutItem.FindByLoadout(db, loadout.Id).OfTypeLoadoutItemGroup())
            {
                // Only delete top-level groups (those directly under the Loadout, not nested under another group)
                var loadoutItem = item.AsLoadoutItem();
                if (loadoutItem.Contains(LoadoutItem.Parent)) continue; // skip nested groups

                // Skip the overrides group — it tracks vanilla game files that shouldn't be removed
                if (new[] { item }.OfTypeLoadoutOverridesGroup().Any()) continue;

                tx.Delete(item.Id, recursive: true);
                removedCount++;
            }

            if (removedCount > 0)
            {
                await tx.Commit();
                _logger.LogInformation("Removed {Count} top-level mod group(s) from the loadout database", removedCount);
            }
            else
            {
                _logger.LogInformation("No mod groups found to remove from the database");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove mods from the database");
        }

        // Step 4: Rescan the game folder so the app knows the disk state has changed.
        // This avoids the app trying to re-apply (or delete) files it no longer tracks.
        _logger.LogInformation("Rescanning game folder to update disk state...");
        await _synchronizerService.RescanFiles(loadout.InstallationInstance);
    }

    public IJobTask<ITool, Unit> StartJob(Loadout.ReadOnly loadout, IJobMonitor monitor, CancellationToken cancellationToken)
    {
        return monitor.Begin<ITool, Unit>(this, async _ =>
        {
            await Execute(loadout, cancellationToken);
            return Unit.Default;
        });
    }
}
