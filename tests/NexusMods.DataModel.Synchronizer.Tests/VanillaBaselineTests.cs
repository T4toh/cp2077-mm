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

    // The fixture's game folder starts with the executable: a disk list without it is not saved
    private static readonly GamePath Primary = new(LocationId.Game, "bin/x64/Cyberpunk2077.exe");

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
        ListPaths().Should().BeEquivalentTo([Primary, new GamePath(LocationId.Game, "bin/x64/original.exe"), new GamePath(LocationId.Game, "r6/config/settings.ini")]);
        GameInstallMetadata.BaselineFromDisk.Get(GameRegistry.ForceGetMetadata(GameInstallation)).Should().BeTrue();
    }

    [Fact]
    public async Task FileAddedAfterManaging_IsAnExternalChangeNotAnOriginal()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        GameFile("bin/x64/dropped-by-hand.dll").Parent.CreateDirectory();
        await GameFile("bin/x64/dropped-by-hand.dll").WriteAllTextAsync("mod");

        await Synchronizer.Synchronize(loadout.Rebase());

        ListPaths().Should().BeEquivalentTo([Primary, new GamePath(LocationId.Game, "bin/x64/original.exe")]);
    }

    [Fact]
    public async Task SynchronizerServiceUpdateBaseline_PicksUpFilesAddedByThePatch()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        GameFile("bin/x64/new-in-patch.dll").Parent.CreateDirectory();
        await GameFile("bin/x64/new-in-patch.dll").WriteAllTextAsync("patched");

        var metadata = await SynchronizerService.UpdateBaseline(loadout.LoadoutId);

        GameBaselineFile.TryGetVanillaFiles(metadata, out var files).Should().BeTrue();
        files.Select(f => (GamePath)f.Path).Order().Should().Contain(new GamePath(LocationId.Game, "bin/x64/new-in-patch.dll"));
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
        files.Single(f => (GamePath)f.Path != Primary).Hash.Should().Be("v2".xxHash3AsUtf8());
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
        files.Single(f => (GamePath)f.Path != Primary).Hash.Should().Be("v2".xxHash3AsUtf8());
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

        ListPaths().Should().BeEquivalentTo([Primary, new GamePath(LocationId.Game, "bin/x64/original.exe")]);
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

        ListPaths().Should().BeEquivalentTo([Primary, new GamePath(LocationId.Game, "bin/x64/original.exe")]);
    }

    [Fact]
    public async Task ModDisabledBeforeAPatch_IsNotAnOriginalAndGetsRemoved()
    {
        // The disk holds the loadout as last applied: a mod disabled since then is still deployed and still the loadout's
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "v1"));
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [(RelativePath)"bin/x64/mod.dll"], loadout, "DisabledLater");
            await tx.Commit();
        }
        loadout = await Synchronizer.Synchronize(loadout.Rebase());
        GameFile("bin/x64/mod.dll").FileExists.Should().BeTrue();

        await DisableItem(LoadoutItem.FindByLoadout(Connection.Db, loadout).Single(i => i.Name == "DisabledLater").Id);
        var locator = ServiceProvider.GetServices<IGameLocator>().OfType<UniversalStubbedGameLocator<Cyberpunk2077Game>>().Single();
        locator.LocatorIds = [LocatorId.From("unknown-v2")];
        await Synchronizer.Synchronize(loadout.Rebase());

        ListPaths().Should().BeEquivalentTo([Primary, new GamePath(LocationId.Game, "bin/x64/original.exe")]);
        GameFile("bin/x64/mod.dll").FileExists.Should().BeFalse();
    }

    [Fact]
    public async Task UnknownVersion_ModOverOriginal_RemovingItRestoresTheOriginal_AndResetKeepsOriginals()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla exe"), ("r6/config/settings.ini", "vanilla ini"));
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [(RelativePath)"r6/config/settings.ini", (RelativePath)"archive/pc/mod/new.archive"], loadout, "ConfigMod");
            await tx.Commit();
        }
        loadout = await Synchronizer.Synchronize(loadout.Rebase());
        (await GameFile("r6/config/settings.ini").ReadAllTextAsync()).Should().Be("r6/config/settings.ini"); // AddModAsync content = path

        var mod = LoadoutItem.FindByLoadout(loadout.Db, loadout).OfTypeLoadoutItemGroup().Single(g => g.AsLoadoutItem().Name == "ConfigMod");
        using (var tx = Connection.BeginTransaction())
        {
            tx.Delete(mod, recursive: true);
            await tx.Commit();
        }
        await Synchronizer.Synchronize(loadout.Rebase());

        (await GameFile("r6/config/settings.ini").ReadAllTextAsync()).Should().Be("vanilla ini");
        GameFile("archive/pc/mod/new.archive").FileExists.Should().BeFalse();

        await Synchronizer.ResetToOriginalGameState(GameInstallation);
        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task Reset_WithSymlinkedFolder_LeavesOutsideUntouched()
    {
        await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        var outside = TemporaryFileManager.CreateFolder().Path;
        var canary = outside.Combine("precious.txt");
        await canary.WriteAllTextAsync("user data");
        File.CreateSymbolicLink(GameFile("bin/linked").ToString(), outside.ToString());

        await Synchronizer.ResetToOriginalGameState(GameInstallation);

        canary.FileExists.Should().BeTrue();
        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task RebuildWithoutThePrimaryFile_KeepsThePreviousList()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/Cyberpunk2077.exe", "exe"), ("bin/x64/original.exe", "vanilla"));
        var before = ListPaths();

        // The game folder reads as empty (drive not mounted, folder half gone): a list from it makes every original a leftover
        GameFile("bin/x64/Cyberpunk2077.exe").Delete();
        GameFile("bin/x64/original.exe").Delete();
        // The button says so instead of reporting success with the old list
        var press = () => SynchronizerService.UpdateBaseline(loadout.LoadoutId);
        await press.Should().ThrowAsync<InvalidOperationException>().WithMessage("*no se actualizó*Cyberpunk2077.exe*");

        ListPaths().Should().Equal(before);
        GameInstallMetadata.BaselineFromDisk.Get(GameRegistry.ForceGetMetadata(GameInstallation)).Should().BeTrue();
    }

    [Fact]
    public async Task FirstListWithoutThePrimaryFile_IsNotSaved_AndNothingIsDeleted()
    {
        GameFile("bin/x64/Cyberpunk2077.exe").Delete();

        // No list, so the apply guard refuses the first sync's deletions
        var manage = () => ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        await manage.Should().ThrowAsync<InvalidOperationException>().WithMessage("No se puede borrar archivos del juego*");

        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        metadata.Contains(GameInstallMetadata.BaselineFromDisk).Should().BeFalse();
        GameBaselineFile.FindByGame(metadata.Db, metadata).Should().BeEmpty();
    }

    private GamePath[] Overrides(Loadout.ReadOnly loadout) =>
        LoadoutOverridesGroup.FindByOverridesFor(Connection.Db, loadout.Id)
            .SelectMany(g => g.AsLoadoutItemGroup().Children.OfTypeLoadoutItemWithTargetPath())
            .Select(i => (GamePath)i.TargetPath).ToArray();

    private async Task WriteGameFile(string path, string content)
    {
        GameFile(path).Parent.CreateDirectory();
        await GameFile(path).WriteAllTextAsync(content);
    }

    [Fact]
    public async Task UpdateBaselineButton_AdoptsAPatchAlreadyInExternalChanges()
    {
        // A patch with no signal (manual install, GOG): it changes an original and adds a file
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "v1"));
        await WriteGameFile("bin/x64/original.exe", "v2");
        await WriteGameFile("bin/x64/new-in-patch.dll", "patched");
        loadout = await Synchronizer.Synchronize(loadout.Rebase());
        var original = new GamePath(LocationId.Game, "bin/x64/original.exe");
        var added = new GamePath(LocationId.Game, "bin/x64/new-in-patch.dll");
        Overrides(loadout).Should().Contain([original, added]);

        await SynchronizerService.UpdateBaseline(loadout.LoadoutId);

        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files).Should().BeTrue();
        files.Single(f => (GamePath)f.Path == original).Hash.Should().Be("v2".xxHash3AsUtf8());
        files.Single(f => (GamePath)f.Path == added).Hash.Should().Be("patched".xxHash3AsUtf8());
        Overrides(loadout).Should().NotContain(original).And.NotContain(added);

        // The patch's files are originals now: the reset keeps them
        await Synchronizer.ResetToOriginalGameState(GameInstallation);
        (await GameFile("bin/x64/new-in-patch.dll").ReadAllTextAsync()).Should().Be("patched");
        (await GameFile("bin/x64/original.exe").ReadAllTextAsync()).Should().Be("v2");
    }

    [Fact]
    public async Task UpdateBaselineButton_KeepsProtectingDeletionsChangedFilesAndModFiles()
    {
        var loadout = await ManagedLoadoutWith(("r6/config/settings.ini", "vanilla"));
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [(RelativePath)"r6/config/settings.ini", (RelativePath)"r6/config/mod.ini"], loadout, "ConfigMod");
            await tx.Commit();
        }
        loadout = await Synchronizer.Synchronize(loadout.Rebase());

        GameFile("r6/config/settings.ini").Delete();                     // a deletion (over an original)
        await WriteGameFile("r6/config/mod.ini", "edited by the game");  // a mod's file changed at runtime
        await WriteGameFile("bin/x64/edited-later.ini", "first");        // changes again before the button
        loadout = await Synchronizer.Synchronize(loadout.Rebase());
        await WriteGameFile("bin/x64/edited-later.ini", "second");
        var before = GameBaselineFile.FindByGame(Connection.Db, GameRegistry.ForceGetMetadata(GameInstallation))
            .Select(f => ((GamePath)f.Path, f.Hash)).OrderBy(f => f.Item1).ToArray();
        Overrides(loadout).Should().BeEquivalentTo([
            new GamePath(LocationId.Game, "r6/config/settings.ini"),
            new GamePath(LocationId.Game, "r6/config/mod.ini"),
            new GamePath(LocationId.Game, "bin/x64/edited-later.ini"),
        ]);

        await SynchronizerService.UpdateBaseline(loadout.LoadoutId);

        GameBaselineFile.FindByGame(Connection.Db, GameRegistry.ForceGetMetadata(GameInstallation))
            .Select(f => ((GamePath)f.Path, f.Hash)).OrderBy(f => f.Item1).Should().Equal(before);
        Overrides(loadout).Should().HaveCount(3);
    }
}
