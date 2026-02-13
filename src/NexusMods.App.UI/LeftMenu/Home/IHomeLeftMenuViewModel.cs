using System.Collections.ObjectModel;
using NexusMods.App.UI.LeftMenu.Items;

namespace NexusMods.App.UI.LeftMenu.Home;

public interface IHomeLeftMenuViewModel : ILeftMenuViewModel
{
    public ILeftMenuItemViewModel LeftMenuItemMyGames { get; }
    
    public ILeftMenuItemViewModel LeftMenuItemMyLoadouts { get; }

    public ILeftMenuItemViewModel LeftMenuItemDownloads { get; }

    public ILeftMenuItemViewModel LeftMenuItemCollections { get; }

    public ReadOnlyObservableCollection<ILeftMenuItemViewModel> LeftMenuCollectionItems { get; }
}
