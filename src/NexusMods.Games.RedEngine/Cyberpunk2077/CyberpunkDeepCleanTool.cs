using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.Loadouts;
using NexusMods.MnemonicDB.Abstractions;
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

    public CyberpunkDeepCleanTool(IFileSystem fileSystem, ILogger<CyberpunkDeepCleanTool> logger, ISynchronizerService synchronizerService)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _synchronizerService = synchronizerService;
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

        // Step 2: Disable all mod groups in the database so the app state matches the cleaned disk.
        // We only disable the group entities (not individual file items), because the SQL sync
        // checks the group's disabled status to determine if files should be deployed.
        try
        {
            var db = loadout.Db;
            using var tx = db.Connection.BeginTransaction();
            var disabledCount = 0;
            foreach (var item in LoadoutItem.FindByLoadout(db, loadout.Id).OfTypeLoadoutItemGroup())
            {
                if (item.AsLoadoutItem().Contains(LoadoutItem.Disabled)) continue;
                tx.Add(item.Id, LoadoutItem.Disabled, NexusMods.MnemonicDB.Abstractions.ElementComparers.Null.Instance);
                disabledCount++;
            }
            if (disabledCount > 0)
            {
                await tx.Commit();
                _logger.LogInformation("Disabled {Count} mod groups in the database", disabledCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable mods in the database");
        }

        // Step 3: Rescan the game folder so the app knows the disk state has changed.
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
