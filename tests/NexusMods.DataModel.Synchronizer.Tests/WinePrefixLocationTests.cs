using FluentAssertions;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.TestFramework;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.IO;
using NexusMods.Sdk.Loadouts;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

/// <summary>
/// The Wine prefix is a location with a whitelist: only the listed files are indexed, written or deleted, never
/// through a symlink, and nothing else inside the prefix is ever touched.
/// </summary>
public class WinePrefixLocationTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<WinePrefixLocationTests>(helper)
{
    protected override bool WithWinePrefix => true;

    private const string SettingsFolder = "drive_c/users/steamuser/AppData/Local/CD Projekt Red/Cyberpunk 2077";
    private const string Settings = SettingsFolder + "/UserSettings.json";
    private static readonly GamePath SettingsPath = new(LocationId.WinePrefix, Settings);

    private AbsolutePath Prefix => GameInstallation.Locations[LocationId.WinePrefix].Path;
    private AbsolutePath PrefixFile(string relative) => Prefix.Combine(relative);

    private async Task WritePrefixFile(string relative, string content)
    {
        var file = PrefixFile(relative);
        file.Parent.CreateDirectory();
        await file.WriteAllTextAsync(content);
    }

    private async Task<Loadout.ReadOnly> ManagedLoadout()
    {
        await LoadoutManager.ManageInstallation(GameInstallation);
        await Synchronizer.ReindexState(GameInstallation);
        return await Synchronizer.Synchronize(await CreateLoadout());
    }

    private GamePath[] IndexedPrefixPaths()
    {
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        return DiskStateEntry.FindByGame(metadata.Db, metadata)
            .Select(e => (GamePath)e.Path)
            .Where(p => p.LocationId == LocationId.WinePrefix)
            .ToArray();
    }

    [Fact]
    public void ThePrefixIsALocationWithAWhitelist()
    {
        GameInstallation.Locations[LocationId.WinePrefix].Path.ToString().Should().EndWith("/pfx");
        GameInstallation.Locations[LocationId.WinePrefix].ManagedFiles.Should().BeEquivalentTo([(RelativePath)Settings]);
        GameInstallation.Locations[LocationId.Game].ManagedFiles.Should().BeNull();
    }

    [Fact]
    public async Task IndexesOnlyTheWhitelist()
    {
        await WritePrefixFile(Settings, "{}");
        await WritePrefixFile(SettingsFolder + "/CrashInfo.json", "crash");
        await WritePrefixFile(SettingsFolder + "/cache/x", "cache");
        await WritePrefixFile("drive_c/windows/system32/a.dll", "dll");

        await ManagedLoadout();

        IndexedPrefixPaths().Should().Equal(SettingsPath);
    }

    [Fact]
    public async Task PrefixDeletedAfterManaging_SyncDoesNotThrow()
    {
        await WritePrefixFile(Settings, "{}");
        var loadout = await ManagedLoadout();
        IndexedPrefixPaths().Should().Equal(SettingsPath);

        // Storage Manager "Borrar prefix de Proton", or Steam recreating it
        Prefix.DeleteDirectoryNoFollow();

        loadout = await Synchronizer.Synchronize(loadout.Rebase());

        IndexedPrefixPaths().Should().BeEmpty();
        loadout.IsValid().Should().BeTrue();
    }
}
