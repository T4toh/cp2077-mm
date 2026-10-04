using Microsoft.Extensions.DependencyInjection;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Loadouts;

namespace NexusMods.App.UI.Helpers;

/// <summary>Shared lookup for the managed games that Steam installed.</summary>
public static class SteamPaths
{
    /// <summary>The Steam installations behind the visible loadouts, one per install folder.</summary>
    public static GameInstallation[] Installations(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<IConnection>().Db;
        return Loadout.All(db)
            .Where(loadout => loadout.IsVisible())
            .Select(loadout => loadout.InstallationInstance)
            .Where(installation => installation.LocatorResult.Store == GameStore.Steam)
            .DistinctBy(installation => installation.LocatorResult.Path)
            .ToArray();
    }
}
