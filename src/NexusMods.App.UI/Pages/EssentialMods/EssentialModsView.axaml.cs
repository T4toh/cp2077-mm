using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;

namespace NexusMods.App.UI.Pages.EssentialMods;

public partial class EssentialModsView : ReactiveUserControl<IEssentialModsViewModel>
{
    public EssentialModsView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.EssentialMods, v => v.EssentialModsList.ItemsSource)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.InstallAllCommand, v => v.InstallAllButton)
                .DisposeWith(disposables);
        });
    }
}
