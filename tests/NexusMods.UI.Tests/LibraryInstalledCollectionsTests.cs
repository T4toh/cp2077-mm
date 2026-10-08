using DynamicData;
using FluentAssertions;
using NexusMods.Abstractions.Loadouts;
using NexusMods.App.UI.Pages;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Sdk.Loadouts;

namespace NexusMods.UI.Tests;

public class LibraryInstalledCollectionsTests : AUiTest
{
    public LibraryInstalledCollectionsTests(IServiceProvider provider) : base(provider) { }

    [Fact]
    public async Task TwoItemsInOneCollection_ListTheCollectionOnce()
    {
        // No loadout entity is needed: the pipeline only follows Parent
        var loadoutId = LoadoutId.From(EntityId.From(0xDEADBEEF));

        using var tx = Connection.BeginTransaction();
        var collection = new CollectionGroup.New(tx, out var collectionId)
        {
            IsReadOnly = true,
            LoadoutItemGroup = new LoadoutItemGroup.New(tx, collectionId)
            {
                IsGroup = true,
                LoadoutItem = new LoadoutItem.New(tx, collectionId) { Name = "Welcome to Night City", LoadoutId = loadoutId },
            },
        };
        var first = new LoadoutItem.New(tx) { Name = "file A", LoadoutId = loadoutId, ParentId = collection.Id };
        var second = new LoadoutItem.New(tx) { Name = "file B", LoadoutId = loadoutId, ParentId = collection.Id };
        var result = await tx.Commit();

        using var linkedItems = new SourceCache<LoadoutItem.ReadOnly, EntityId>(item => item.Id);
        linkedItems.AddOrUpdate([
            LoadoutItem.Load(result.Db, result[first.Id]),
            LoadoutItem.Load(result.Db, result[second.Id]),
        ]);

        using var _ = LibraryDataProviderHelper.ObserveInstalledCollectionNames(linkedItems.Connect())
            .Bind(out var names)
            .Subscribe();

        names.Should().Equal("Welcome to Night City");

        linkedItems.RemoveKey(result[first.Id]);
        names.Should().Equal("Welcome to Night City");

        linkedItems.RemoveKey(result[second.Id]);
        names.Should().BeEmpty();
    }
}
