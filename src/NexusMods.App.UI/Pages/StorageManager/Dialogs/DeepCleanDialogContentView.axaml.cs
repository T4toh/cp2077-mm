using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using JetBrains.Annotations;
using ReactiveUI;

namespace NexusMods.App.UI.Pages.StorageManager.Dialogs;

[UsedImplicitly]
public partial class DeepCleanDialogContentView : ReactiveUserControl<IDeepCleanDialogContentViewModel>
{
    public DeepCleanDialogContentView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.Bind(ViewModel, vm => vm.NukeMode, view => view.NukeModeCheckBox.IsChecked,
                    b => b, nb => nb ?? false)
                .DisposeWith(d);
        });
    }
}
