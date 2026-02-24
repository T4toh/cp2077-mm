using NexusMods.UI.Sdk;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Pages.StorageManager.Dialogs;

public interface IDeepCleanDialogContentViewModel : IViewModelInterface
{
    bool NukeMode { get; set; }
}

public class DeepCleanDialogContentViewModel : AViewModel<IDeepCleanDialogContentViewModel>, IDeepCleanDialogContentViewModel
{
    [Reactive] public bool NukeMode { get; set; }
}
