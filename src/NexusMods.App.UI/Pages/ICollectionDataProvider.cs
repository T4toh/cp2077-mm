using DynamicData;
using DynamicData.Kernel;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.NexusModsLibrary.Models;
using NexusMods.App.UI.Controls;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Sdk.Loadouts;

namespace NexusMods.App.UI.Pages;

/// <summary>
/// Data provider interface for collection downloads that transforms collection data
/// into CompositeItemModel collections for TreeDataGrid display.
/// </summary>
public interface ICollectionDataProvider
{
    /// <summary>
    /// Observes collection download items based on revision metadata and filter.
    /// </summary>
    IObservable<IChangeSet<CompositeItemModel<EntityId>, EntityId>> ObserveCollectionItems(
        CollectionRevisionMetadata.ReadOnly revisionMetadata,
        IObservable<CollectionDownloadsFilter> filterObservable,
        Optional<LoadoutId> loadoutId);
}
