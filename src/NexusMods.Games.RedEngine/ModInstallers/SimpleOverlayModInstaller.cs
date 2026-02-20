using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NexusMods.Abstractions.Library.Installers;
using NexusMods.Abstractions.Loadouts;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Paths;
using NexusMods.Paths.Trees.Traits;
using NexusMods.Sdk.Library;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Loadouts;

namespace NexusMods.Games.RedEngine.ModInstallers;

public class SimpleOverlayModInstaller : ALibraryArchiveInstaller
{
    
    public SimpleOverlayModInstaller(IServiceProvider serviceProvider) : 
        base(serviceProvider, serviceProvider.GetRequiredService<ILogger<SimpleOverlayModInstaller>>())
    {
    }

    private static readonly RelativePath[] RootPaths =
    [
        "bin/x64",
        "engine",
        "r6",
        "red4ext",
        "archive/pc/mod",
        "plugins",
    ];

    public override ValueTask<InstallerResult> ExecuteAsync(
        LibraryArchive.ReadOnly libraryArchive,
        LoadoutItemGroup.New loadoutGroup,
        ITransaction tx,
        Loadout.ReadOnly loadout,
        CancellationToken cancellationToken)
    {
        var tree = LibraryArchiveTreeExtensions.GetTree(libraryArchive);
        
        // 1. Find the best root candidate
        // We look for where the core folders (bin, r6, etc.) start in the archive.
        var allFiles = tree.EnumerateFilesBfs().ToArray();
        if (allFiles.Length == 0) return ValueTask.FromResult<InstallerResult>(new NotSupported(Reason: "Archive is empty"));

        RelativePath? archiveRoot = null;
        foreach (var rootPath in RootPaths)
        {
            var match = allFiles.Where(f => f.Value.Item.Path.ToString().Contains(rootPath.ToString(), StringComparison.OrdinalIgnoreCase))
                .Select(f => f.Value.Item.Path)
                .Cast<RelativePath?>()
                .FirstOrDefault();

            if (match is not null)
            {
                var pathStr = match.Value.ToString();
                var index = pathStr.IndexOf(rootPath.ToString(), StringComparison.OrdinalIgnoreCase);
                archiveRoot = RelativePath.FromUnsanitizedInput(pathStr.AsSpan(0, index).ToString());
                break;
            }
        }

        // 2. If no core folders found, this might be a simple archive mod or we don't support its structure here
        if (archiveRoot is null) return ValueTask.FromResult<InstallerResult>(new NotSupported(Reason: "Archive contains no recognized Cyberpunk 2077 root folders"));

        var root = archiveRoot.Value;
        var newFiles = 0;
        foreach (var file in allFiles)
        {
            if (!file.Value.Item.Path.InFolder(root)) continue;

            var relativePath = file.Value.Item.Path.RelativeTo(root);
            
            // Heuristic: if 'plugins' is at the root of the mod, it usually belongs in 'bin/x64/plugins'
            if (relativePath.ToString().StartsWith("plugins", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = RelativePath.FromUnsanitizedInput("bin/x64").Join(relativePath);
            }

            Logger.LogDebug("Installing file {File} to {TargetPath}", file.Value.Item.Path, relativePath);

            _ = file.Value.ToLoadoutFile(loadout.Id, loadoutGroup.Id, tx, new GamePath(LocationId.Game, relativePath));
            newFiles++;
        }

        return newFiles == 0
            ? ValueTask.FromResult<InstallerResult>(new NotSupported(Reason: "Found no matching files after root resolution"))
            : ValueTask.FromResult<InstallerResult>(new Success());
    }
}
