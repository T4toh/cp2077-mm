using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;

namespace NexusMods.App.UI.Overlays;

public partial class ManualAddGameOverlayView : ReactiveUserControl<IManualAddGameOverlayViewModel>
{
    public ManualAddGameOverlayView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.BindCommand(ViewModel, vm => vm.CommandBrowseGamePath, v => v.ButtonBrowseGamePath)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CommandBrowseWinePrefix, v => v.ButtonBrowseWinePrefix)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CommandCancel, v => v.ButtonCancel)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CommandAdd, v => v.ButtonAdd)
                .DisposeWith(disposables);
        });
    }
}
