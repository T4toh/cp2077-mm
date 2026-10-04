using System.IO.Compression;
using FluentAssertions;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.TestFramework;
using NexusMods.MnemonicDB.Abstractions.TxFunctions;
using NexusMods.Paths;
using NexusMods.Sdk.Library;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

public class InstallWithoutMyModsTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<InstallWithoutMyModsTests>(helper)
{
    [Fact]
    public async Task ParallelInstalls_WithNoEditableCollection_CreateASingleMyMods()
    {
        var loadout = await CreateLoadout();
        using (var tx = Connection.BeginTransaction())
        {
            tx.Delete(loadout.MutableCollections().Single().CollectionId, recursive: true);
            await tx.Commit();
        }
        loadout = loadout.Rebase();
        loadout.MutableCollections().Should().BeEmpty();

        var items = new List<LibraryItem.ReadOnly>();
        for (var i = 0; i < 4; i++)
        {
            var zip = FileSystem.GetKnownPath(KnownPath.TempDirectory).Combine($"no-my-mods-{Guid.NewGuid():N}.zip");
            await using (var fs = zip.Create())
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                await using var w = new StreamWriter(archive.CreateEntry($"archive/pc/mod/mod{i}.archive").Open());
                await w.WriteAsync($"mod {i}");
            }
            items.Add((await LibraryService.AddLocalFile(zip)).AsLibraryFile().AsLibraryItem());
        }

        // What the library page does when no editable collection is left: no parent, all at once
        await Task.WhenAll(items.Select(async item => await LoadoutManager.InstallItem(item, loadout.LoadoutId)));

        loadout = loadout.Rebase();
        var myMods = loadout.MutableCollections().Should().ContainSingle().Subject;
        LoadoutItem.FindByParent(Connection.Db, myMods.CollectionId).Should().HaveCount(4);
    }
}
