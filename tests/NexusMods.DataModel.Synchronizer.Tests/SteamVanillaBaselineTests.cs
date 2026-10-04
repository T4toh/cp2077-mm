using System.IO.Compression;
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
using NexusMods.Sdk.Loadouts;
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

    private GamePath[] Overrides(Loadout.ReadOnly loadout) =>
        LoadoutOverridesGroup.FindByOverridesFor(Connection.Db, loadout.Id)
            .SelectMany(g => g.AsLoadoutItemGroup().Children.OfTypeLoadoutItemWithTargetPath())
            .Select(i => (GamePath)i.TargetPath).ToArray();

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

    private AbsolutePath GameFile(string path) => GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, path));

    private async Task WriteGameFile(string path, string content)
    {
        GameFile(path).Parent.CreateDirectory();
        await GameFile(path).WriteAllTextAsync(content);
    }

    /// <summary>Writes the files the stubbed hash database lists for <paramref name="zip"/>, with their real content.</summary>
    private async Task WriteStubbedGameFiles(string zip, Func<string, bool>? only = null)
    {
        using var archive = ZipFile.OpenRead((FileSystem.GetKnownPath(KnownPath.EntryDirectory) / "Resources" / zip).ToString());
        foreach (var entry in archive.Entries.Where(e => e.FullName.StartsWith("game/") && e.Length > 0))
        {
            // The stubbed database lists them relative to the game folder, without the zip's `game/`
            var path = entry.FullName["game/".Length..];
            if (only is not null && !only(path)) continue;
            GameFile(path).Parent.CreateDirectory();
            await using var source = entry.Open();
            await using var target = GameFile(path).Create();
            await source.CopyToAsync(target);
        }
    }

    [Fact]
    public async Task ListFromDisk_HashDatabaseLearnsTheVersion_RebuildsFromNexusAndDeletesNothing()
    {
        // A Steam version the hash database doesn't know yet: the list comes from the disk
        Locator.LocatorIds = [LocatorId.From("not-in-the-db-yet")];
        await WriteGameFile("bin/x64/Cyberpunk2077.exe", "primary file");
        await LoadoutManager.ManageInstallation(GameInstallation);
        var loadout = await Synchronizer.Synchronize(await CreateLoadout());
        GameInstallMetadata.BaselineFromDisk.Get(GameRegistry.ForceGetMetadata(GameInstallation)).Should().BeTrue();

        // A file of that version shows up after the list was taken: it goes to External Changes
        await WriteStubbedGameFiles("StubbedGameState.zip", only: path => path == "config.ini");
        loadout = await Synchronizer.Synchronize(loadout.Rebase());
        Overrides(loadout).Should().Contain(new GamePath(LocationId.Game, "config.ini"));

        // A hash database update learns the installed version: same IDs, now known
        Hashes.LearnedLocatorIds.Add(LocatorId.From("not-in-the-db-yet"));
        loadout = await Synchronizer.Synchronize(loadout.Rebase());
        await Synchronizer.Synchronize(loadout.Rebase());

        GameFile("config.ini").FileExists.Should().BeTrue();
        GameInstallMetadata.BaselineFromDisk.Get(GameRegistry.ForceGetMetadata(GameInstallation)).Should().BeFalse();
        ListPaths().Should().Equal(NexusPaths("not-in-the-db-yet"));
    }
}
