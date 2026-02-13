using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using JetBrains.Annotations;
using NexusMods.Abstractions.NexusModsLibrary.Models;
using NexusMods.App.UI.Controls;
using NexusMods.App.UI.Controls.Navigation;
using NexusMods.App.UI.LeftMenu.Items;
using NexusMods.App.UI.Pages.CollectionDownload;
using NexusMods.App.UI.Pages.Downloads;
using NexusMods.App.UI.Pages.MyGames;
using NexusMods.App.UI.Pages.MyLoadouts;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Sdk.Loadouts;
using NexusMods.Sdk.NexusModsApi;
using NexusMods.UI.Sdk;
using NexusMods.UI.Sdk.Icons;
using ReactiveUI;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Kernel;
using Microsoft.Extensions.DependencyInjection;

namespace NexusMods.App.UI.LeftMenu.Home;

[UsedImplicitly]
public class HomeLeftMenuViewModel : AViewModel<IHomeLeftMenuViewModel>, IHomeLeftMenuViewModel
{
    public WorkspaceId WorkspaceId { get; }
    public ILeftMenuItemViewModel LeftMenuItemMyGames { get; }
    public ILeftMenuItemViewModel LeftMenuItemMyLoadouts { get; }
    public ILeftMenuItemViewModel LeftMenuItemDownloads { get; }
    public ILeftMenuItemViewModel LeftMenuItemCollections { get; }

    public ReadOnlyObservableCollection<ILeftMenuItemViewModel> LeftMenuCollectionItems => _leftMenuCollectionItems;
    private ReadOnlyObservableCollection<ILeftMenuItemViewModel> _leftMenuCollectionItems = new([]);

    public HomeLeftMenuViewModel(
        IMyGamesViewModel myGamesViewModel,
        WorkspaceId workspaceId,
        IWorkspaceController workspaceController,
        IServiceProvider serviceProvider)
    {
        WorkspaceId = workspaceId;
        var conn = serviceProvider.GetRequiredService<IConnection>();

        LeftMenuItemMyGames = new LeftMenuItemViewModel(
            workspaceController,
            WorkspaceId,
            new PageData
            {
                FactoryId = MyGamesPageFactory.StaticId,
                Context = new MyGamesPageContext(),
            }
        )
        {
            Text = new StringComponent(Language.MyGames),
            Icon = IconValues.GamepadOutline,
        };
        
        LeftMenuItemMyLoadouts = new LeftMenuItemViewModel(
            workspaceController,
            WorkspaceId,
            new PageData
            {
                FactoryId = MyLoadoutsPageFactory.StaticId,
                Context = new MyLoadoutsPageContext(),
            }
        )
        {
            Text = new StringComponent(Language.MyLoadoutsPageTitle),
            Icon = IconValues.Package,
        };

        LeftMenuItemDownloads = new LeftMenuItemViewModel(
            workspaceController,
            WorkspaceId,
            new PageData
            {
                FactoryId = DownloadsPageFactory.StaticId,
                Context = new DownloadsPageContext { GameScope = Optional<NexusModsGameId>.None },
            }
        )
        {
            Text = new StringComponent(Language.Downloads_WorkspaceTitle),
            Icon = IconValues.Download,
        };

        LeftMenuItemCollections = new LeftMenuItemViewModel(
            workspaceController,
            WorkspaceId,
            new PageData
            {
                // We don't have a generic collections page, but we can list them here?
                // Or just show "All Collections" if such page exists.
                // For now, let's keep it as a placeholder or remove it if not needed.
                // Actually the user wants "My Collections" (Downloads menu).
                FactoryId = DownloadsPageFactory.StaticId,
                Context = new DownloadsPageContext { GameScope = Optional<NexusModsGameId>.None },
            }
        )
        {
            Text = new StringComponent("My Collections"), // User specifically asked for this name
            Icon = IconValues.CollectionsOutline,
        };

        this.WhenActivated(d =>
        {
            // Observe all collection revisions being downloaded/present
            CollectionRevisionMetadata.ObserveAll(conn)
                .Transform(revision =>
                {
                    var pageData = new PageData
                    {
                        FactoryId = CollectionDownloadPageFactory.StaticId,
                        Context = new CollectionDownloadPageContext
                        {
                            TargetLoadout = Optional<LoadoutId>.None, 
                            CollectionRevisionMetadataId = revision,
                        },
                    };

                    return (ILeftMenuItemViewModel)new CollectionRevisionLeftMenuItemViewModel(
                        workspaceController,
                        workspaceId,
                        pageData,
                        revision,
                        serviceProvider)
                    {
                        Text = new StringComponent(initialValue: revision.Collection.Name, CollectionMetadata.Observe(conn, revision.Collection).Select(x => x.Name)),
                        Icon = IconValues.CollectionsOutline,
                        RightIcon = IconValues.Downloading,
                    };
                })
                .OnUI()
                .Bind(out _leftMenuCollectionItems)
                .Subscribe()
                .DisposeWith(d);
        });
    }
}
