using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Games.TestFramework;
using NexusMods.Hashing.xxHash3;
using NexusMods.MnemonicDB.Abstractions.TxFunctions;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Loadouts;
using NexusMods.StandardGameLocators.TestHelpers;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

/// <summary>
/// The vanilla file list of an installation (<see cref="GameBaselineFile"/>): built from the Nexus hash database when
/// it knows the version, from the disk when it doesn't, and the only thing the synchronizer treats as original.
/// </summary>
public class VanillaBaselineTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<VanillaBaselineTests>(helper)
{
    [Fact]
    public async Task TryGetVanillaFiles_IsFalseUntilTheListIsBuilt()
    {
        await LoadoutManager.ManageInstallation(GameInstallation);
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        GameBaselineFile.TryGetVanillaFiles(metadata, out _).Should().BeFalse();

        var path = new GamePath(LocationId.Game, "bin/x64/original.exe");
        using (var tx = Connection.BeginTransaction())
        {
            _ = new GameBaselineFile.New(tx)
            {
                Path = path.ToGamePathParentTuple(metadata.Id),
                Hash = "original".xxHash3AsUtf8(),
                Size = Size.FromLong(8),
                GameId = metadata.Id,
            };
            tx.Add(metadata.Id, GameInstallMetadata.BaselineFromDisk, true);
            await tx.Commit();
        }

        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files).Should().BeTrue();
        files.Select(f => (GamePath)f.Path).Should().Equal(path);
    }

    private AbsolutePath GameFile(string path) => GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, path));

    private async Task<Loadout.ReadOnly> ManagedLoadoutWith(params (string Path, string Content)[] files)
    {
        foreach (var (path, content) in files)
        {
            GameFile(path).Parent.CreateDirectory();
            await GameFile(path).WriteAllTextAsync(content);
        }
        await LoadoutManager.ManageInstallation(GameInstallation);
        return await Synchronizer.Synchronize(await CreateLoadout());
    }

    private GamePath[] ListPaths() =>
        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files)
            ? files.Select(f => (GamePath)f.Path).Order().ToArray()
            : throw new InvalidOperationException("no list");

    [Fact]
    public async Task FirstSync_UnknownVersion_KeepsEveryFileAlreadyThere()
    {
        // A manually added game (or any version Nexus doesn't know): today the first sync deletes these
        await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"), ("r6/config/settings.ini", "vanilla"));

        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
        GameFile("r6/config/settings.ini").FileExists.Should().BeTrue();
        ListPaths().Should().Equal(new GamePath(LocationId.Game, "bin/x64/original.exe"), new GamePath(LocationId.Game, "r6/config/settings.ini"));
        GameInstallMetadata.BaselineFromDisk.Get(GameRegistry.ForceGetMetadata(GameInstallation)).Should().BeTrue();
    }

    [Fact]
    public async Task FileAddedAfterManaging_IsAnExternalChangeNotAnOriginal()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        GameFile("bin/x64/dropped-by-hand.dll").Parent.CreateDirectory();
        await GameFile("bin/x64/dropped-by-hand.dll").WriteAllTextAsync("mod");

        await Synchronizer.Synchronize(loadout.Rebase());

        ListPaths().Should().Equal(new GamePath(LocationId.Game, "bin/x64/original.exe"));
    }

    [Fact]
    public async Task LocatorIdsChange_RebuildsTheListAndAdoptsThePatch()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "v1"));
        var locator = ServiceProvider.GetServices<IGameLocator>().OfType<UniversalStubbedGameLocator<Cyberpunk2077Game>>().Single();
        locator.LocatorIds = [LocatorId.From("unknown-v2")];
        await GameFile("bin/x64/original.exe").WriteAllTextAsync("v2");

        await Synchronizer.Synchronize(loadout.Rebase());

        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files).Should().BeTrue();
        files.Single().Hash.Should().Be("v2".xxHash3AsUtf8());
    }

    [Fact]
    public async Task SteamPatchOverModdedFile_DeletesNothingAndAdoptsNewVanilla()
    {
        var loadout = await ManagedLoadoutWith(("r6/config/settings.ini", "v1"));
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [(RelativePath)"r6/config/settings.ini"], loadout, "ConfigMod");
            await tx.Commit();
        }
        loadout = await Synchronizer.Synchronize(loadout.Rebase());

        var locator = ServiceProvider.GetServices<IGameLocator>().OfType<UniversalStubbedGameLocator<Cyberpunk2077Game>>().Single();
        locator.LocatorIds = [LocatorId.From("unknown-v2")];
        await GameFile("r6/config/settings.ini").WriteAllTextAsync("v2");
        await Synchronizer.Synchronize(loadout.Rebase());

        GameFile("r6/config/settings.ini").FileExists.Should().BeTrue();
        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files).Should().BeTrue();
        files.Single().Hash.Should().Be("v2".xxHash3AsUtf8());
    }

    [Fact]
    public async Task ExistingInstallWithoutList_GetsOneOnNextSync()
    {
        // Installs managed before this change: no list, no marker
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        using (var tx = Connection.BeginTransaction())
        {
            foreach (var file in GameBaselineFile.FindByGame(metadata.Db, metadata)) tx.Delete(file, recursive: false);
            tx.Retract(metadata.Id, GameInstallMetadata.BaselineFromDisk, true);
            await tx.Commit();
        }

        await Synchronizer.Synchronize(loadout.Rebase());

        ListPaths().Should().Equal(new GamePath(LocationId.Game, "bin/x64/original.exe"));
    }

    [Fact]
    public async Task UnManage_ClearsListAndMarker()
    {
        await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));

        await LoadoutManager.UnManage(GameInstallation, runGc: false, cleanGameFolder: false);

        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        metadata.Contains(GameInstallMetadata.BaselineFromDisk).Should().BeFalse();
        GameBaselineFile.FindByGame(metadata.Db, metadata).Should().BeEmpty();
    }

    [Fact]
    public async Task ListRebuiltWhileSwitchingLoadouts_UsesTheLoadoutOnDisk()
    {
        // Synchronizing B while A is applied: the disk still holds A's mod files, which must not become originals
        var loadoutA = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [(RelativePath)"bin/x64/mod-of-a.dll"], loadoutA, "ModOfA");
            await tx.Commit();
        }
        await Synchronizer.Synchronize(loadoutA.Rebase());
        GameFile("bin/x64/mod-of-a.dll").FileExists.Should().BeTrue();

        var loadoutB = await CreateLoadout();
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        using (var tx = Connection.BeginTransaction())
        {
            // No list yet (install managed before this change), so switching to B builds it
            foreach (var file in GameBaselineFile.FindByGame(metadata.Db, metadata)) tx.Delete(file, recursive: false);
            tx.Retract(metadata.Id, GameInstallMetadata.BaselineFromDisk, true);
            await tx.Commit();
        }

        await Synchronizer.Synchronize(loadoutB.Rebase());

        ListPaths().Should().Equal(new GamePath(LocationId.Game, "bin/x64/original.exe"));
    }
}
