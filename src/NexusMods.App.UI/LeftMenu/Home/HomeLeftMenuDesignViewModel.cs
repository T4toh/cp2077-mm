using System.Collections.ObjectModel;
using NexusMods.App.UI.Controls;
using NexusMods.App.UI.LeftMenu.Items;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.UI.Sdk;
using NexusMods.UI.Sdk.Icons;

namespace NexusMods.App.UI.LeftMenu.Home;

public class HomeLeftMenuDesignViewModel : AViewModel<IHomeLeftMenuViewModel>, IHomeLeftMenuViewModel
{
    public WorkspaceId WorkspaceId { get; } = WorkspaceId.NewId();
    
    public ILeftMenuItemViewModel LeftMenuItemMyGames { get; } = new LeftMenuItemDesignViewModel
    {
        Text = new StringComponent(Language.MyGames),
        Icon = IconValues.GamepadOutline,
    };
    public ILeftMenuItemViewModel LeftMenuItemMyLoadouts { get; } = new LeftMenuItemDesignViewModel
    {
        Text = new StringComponent(Language.MyLoadoutsPageTitle),
        Icon = IconValues.Package,
    };

    public ILeftMenuItemViewModel LeftMenuItemDownloads { get; } = new LeftMenuItemDesignViewModel
    {
        Text = new StringComponent(Language.Downloads_WorkspaceTitle),
        Icon = IconValues.Download,
    };

    public ILeftMenuItemViewModel LeftMenuItemCollections { get; } = new LeftMenuItemDesignViewModel
    {
        Text = new StringComponent("My Collections"),
        Icon = IconValues.CollectionsOutline,
    };

    public ReadOnlyObservableCollection<ILeftMenuItemViewModel> LeftMenuCollectionItems { get; } = new(new ObservableCollection<ILeftMenuItemViewModel>());
}


    
