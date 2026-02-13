using NexusMods.UI.Sdk;
using R3;

namespace NexusMods.App.UI.Overlays;

public record ManualAddGameOverlayResult(bool Confirmed, string GamePath, string WinePrefix)
{
    public static readonly ManualAddGameOverlayResult Cancel = new(false, string.Empty, string.Empty);
}

public interface IManualAddGameOverlayViewModel : IOverlayViewModel
{
    BindableReactiveProperty<string> GamePath { get; }
    BindableReactiveProperty<string> WinePrefix { get; }
    ReactiveCommand<Unit> CommandBrowseGamePath { get; }
    ReactiveCommand<Unit> CommandBrowseWinePrefix { get; }
    ReactiveCommand<Unit> CommandCancel { get; }
    ReactiveCommand<Unit> CommandAdd { get; }
}
