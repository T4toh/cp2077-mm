using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Games.FileHashes;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Backend;
using NexusMods.CrossPlatform;
using NexusMods.Games.Generic;
using NexusMods.Games.RedEngine;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Games.TestFramework;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.StandardGameLocators.TestHelpers;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

public class SteamVanillaBaselineTests(ITestOutputHelper helper) : AIsolatedGameTest<SteamVanillaBaselineTests, Cyberpunk2077Game>(helper)
{
    protected override IServiceCollection AddServices(IServiceCollection services) => base.AddServices(services)
        .AddOSInterop()
        .AddRuntimeDependencies()
        .AddGenericGameSupport()
        .AddUniversalGameLocator<Cyberpunk2077Game>(new Version("1.61"), stores: [GameStore.Steam])
        .AddRedEngineGames();

    [Fact]
    public async Task KnownVersion_ListIsTheNexusList()
    {
        var preexisting = GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, "bin/x64/preexisting-mod.dll"));
        preexisting.Parent.CreateDirectory();
        await preexisting.WriteAllTextAsync("mod");

        await LoadoutManager.ManageInstallation(GameInstallation);
        await Synchronizer.Synchronize(await CreateLoadout());

        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        GameInstallMetadata.BaselineFromDisk.Get(metadata).Should().BeFalse();
        GameBaselineFile.TryGetVanillaFiles(metadata, out var files).Should().BeTrue();
        var nexus = ServiceProvider.GetRequiredService<NexusMods.Abstractions.Games.FileHashes.IFileHashesService>()
            .GetGameFiles((GameStore.Steam, [LocatorId.From("StubbedGameState.zip")])).Select(f => f.Path);
        files.Select(f => (GamePath)f.Path).Should().BeEquivalentTo(nexus);
        files.Select(f => (GamePath)f.Path).Should().NotContain(new GamePath(LocationId.Game, "bin/x64/preexisting-mod.dll"));
    }

    private UniversalStubbedGameLocator<Cyberpunk2077Game> Locator =>
        ServiceProvider.GetServices<IGameLocator>().OfType<UniversalStubbedGameLocator<Cyberpunk2077Game>>().Single();

    private StubbedFileHasherService Hashes => (StubbedFileHasherService)ServiceProvider.GetRequiredService<IFileHashesService>();

    private GamePath[] NexusPaths(string locatorId) =>
        Hashes.GetGameFiles((GameStore.Steam, [LocatorId.From(locatorId)])).Select(f => f.Path).Order().ToArray();

    private GamePath[] ListPaths() =>
        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files)
            ? files.Select(f => (GamePath)f.Path).Order().ToArray()
            : throw new InvalidOperationException("no list");

    [Fact]
    public async Task RebuildFailsAfterTheNewIdsAreSaved_NextSyncRebuilds()
    {
        await LoadoutManager.ManageInstallation(GameInstallation);
        var loadout = await Synchronizer.Synchronize(await CreateLoadout());
        ListPaths().Should().Equal(NexusPaths("StubbedGameState.zip"));

        // Steam updated the game; reading the new list fails once (I/O while Steam writes)
        Locator.LocatorIds = [LocatorId.From("StubbedGameState_game_v2.zip")];
        Hashes.FailNextGetGameFiles = true;
        var failed = () => Synchronizer.Synchronize(loadout.Rebase());
        await failed.Should().ThrowAsync<IOException>();

        // The new IDs are saved but the list is the old version's: it must not count as built
        GameRegistry.ForceGetMetadata(GameInstallation).Contains(GameInstallMetadata.BaselineFromDisk).Should().BeFalse();

        await Synchronizer.Synchronize(loadout.Rebase());
        ListPaths().Should().Equal(NexusPaths("StubbedGameState_game_v2.zip"));
    }
}
