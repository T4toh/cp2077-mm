using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.Loadouts;
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

    public CyberpunkDeepCleanTool(IFileSystem fileSystem, ILogger<CyberpunkDeepCleanTool> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public IEnumerable<GameId> GameIds => [Cyberpunk2077Game.GameId];
    public string Name => "Deep Clean (Disable all mods)";

    public async Task Execute(Loadout.ReadOnly loadout, CancellationToken cancellationToken)
    {
        var gamePath = loadout.InstallationInstance.Locations[LocationId.Game].Path;
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupDir = gamePath.Combine(RelativePath.FromUnsanitizedInput($"_MOD_REMOVER_BACKUP_{timestamp}"));

        var pathsToMove = new[]
        {
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
            "r6/input"
        };

        var pathsToDelete = new[]
        {
            "V2077"
        };

        _logger.LogInformation("Starting deep clean for Cyberpunk 2077 at {Path}", gamePath);

        bool backupCreated = false;

        foreach (var relativePath in pathsToMove)
        {
            var fullPath = gamePath.Combine(RelativePath.FromUnsanitizedInput(relativePath));
            if (fullPath.DirectoryExists() || fullPath.FileExists)
            {
                if (!backupCreated)
                {
                    if (!backupDir.DirectoryExists())
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
        }

        foreach (var relativePath in pathsToDelete)
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
            _logger.LogInformation("Deep clean completed. Backup created at {Path}", backupDir);
        else
            _logger.LogInformation("Deep clean completed. No mod files were found to backup.");
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
