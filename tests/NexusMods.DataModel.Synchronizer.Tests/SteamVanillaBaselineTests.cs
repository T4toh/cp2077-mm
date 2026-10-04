using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
}
