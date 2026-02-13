using System.Collections.ObjectModel;
using System.Reactive;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.UI.Sdk;
using ReactiveUI;

namespace NexusMods.App.UI.Pages.EssentialMods;

public interface IEssentialModsViewModel : IPageViewModelInterface
{
    ReadOnlyObservableCollection<IEssentialModEntryViewModel> EssentialMods { get; }
    ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> InstallAllCommand { get; }
}

public interface IEssentialModEntryViewModel : IViewModelInterface
{
    string Name { get; }
    string Description { get; }
    EssentialModStatus Status { get; }
    ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> InstallCommand { get; }
}

public enum EssentialModStatus
{
    NotDownloaded,
    InLibrary,
    Installed,
    Downloading,
    Installing
}
