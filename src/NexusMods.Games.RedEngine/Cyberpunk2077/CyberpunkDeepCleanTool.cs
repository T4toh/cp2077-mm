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
    // Note: the bash script has a bug where directories are skipped; this C# version handles both files and dirs.
    private static readonly string[] PathsToMove =
    [
        "archive/pc/mod",
        "mods",
        "bin/x64/plugins",
        "r6/scripts",
        "r6/tweaks",
        "red4ext",
        "engine/tools",
        "engine/config/platform/pc",
        "bin/x64/d3d11.dll",
        "bin/x64/global.ini",
        "bin/x64/powrprof.dll",
        "bin/x64/winmm.dll",
        "bin/x64/version.dll",
        "engine/config/base",
        "engine/config/galaxy",
        "r6/cache",
        "r6/config",
        "r6/input",
    ];

    private static readonly string[] PathsToDelete = ["V2077"];

    public async Task Execute(Loadout.ReadOnly loadout, CancellationToken cancellationToken)
    {
        var gamePath = loadout.InstallationInstance.Locations[LocationId.Game].Path;
        _logger.LogInformation("Starting deep clean for Cyberpunk 2077 at {Path}", gamePath);

        // Step 1: Move mod files to a timestamped backup directory
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupDir = gamePath.Combine(RelativePath.FromUnsanitizedInput($"_MOD_REMOVER_BACKUP_{timestamp}"));
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

        // Step 2: Disable all mods in the database so the app state matches the cleaned disk
        try
        {
            var db = loadout.Db;
            using var tx = db.Connection.BeginTransaction();
            var disabledCount = 0;
            foreach (var item in LoadoutItem.FindByLoadout(db, loadout.Id))
            {
                if (item.Contains(LoadoutItem.Disabled)) continue;
                tx.Add(item.Id, LoadoutItem.Disabled, NexusMods.MnemonicDB.Abstractions.ElementComparers.Null.Instance);
                disabledCount++;
            }
            if (disabledCount > 0)
            {
                await tx.Commit();
                _logger.LogInformation("Disabled {Count} mods in the database", disabledCount);
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
