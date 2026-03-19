using System.Runtime.CompilerServices;
using NexusMods.Abstractions.Diagnostics;
using NexusMods.Abstractions.Diagnostics.Emitters;
using NexusMods.Abstractions.Diagnostics.Values;

using NexusMods.Abstractions.Loadouts;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Loadouts;
using static NexusMods.Games.RedEngine.Constants;
namespace NexusMods.Games.RedEngine.Cyberpunk2077.Emitters;

public class MissingRedModEmitter : ILoadoutDiagnosticEmitter
{
    public static readonly NamedLink RedmodGenericLink = new("official website", new Uri("https://www.cyberpunk.net/en/modding-support"));
    public static readonly NamedLink RedmodSteamLink = new("Steam", new Uri("steam://store/2060310"));

    internal static bool HasRedMods(Loadout.ReadOnly loadout, out AbsolutePath redModInstallFolder, out int numRedModDirs)
    {
        redModInstallFolder = loadout.InstallationInstance.Locations.ToAbsolutePath(RedModInstallFolder);

        if (!redModInstallFolder.DirectoryExists())
        {
            numRedModDirs = 0;
            return false;
        }

        var redModDirs = redModInstallFolder
            .EnumerateDirectories("*", false)
            .Where(x => x.EnumerateFiles(pattern: "*", recursive: false).Any());

        numRedModDirs = redModDirs.Count();
        return numRedModDirs > 0;
    }

    internal static bool HasRedModToolInstalled(Loadout.ReadOnly loadout, out AbsolutePath redModPath)
    {
        redModPath = loadout.InstallationInstance.Locations.ToAbsolutePath(RedModPath);
        return redModPath.FileExists;
    }

    public async IAsyncEnumerable<Diagnostic> Diagnose(
        Loadout.ReadOnly loadout,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!HasRedMods(loadout, out var redModInstallFolder, out var numRedModDirs)) yield break;
        if (HasRedModToolInstalled(loadout, out var redModPath)) yield break;

        var store = loadout.Installation.Store;

        var link = store == GameStore.Steam ? RedmodSteamLink : RedmodGenericLink;

        yield return Diagnostics.CreateMissingRedModDependency(
            RedmodLink: link,
            GenericLink: RedmodGenericLink,
            ModCount: numRedModDirs,
            RedModFolder: redModInstallFolder.ToString(),
            RedModEXE: redModPath.ToString()
        );

        await Task.Yield();
    }
}
