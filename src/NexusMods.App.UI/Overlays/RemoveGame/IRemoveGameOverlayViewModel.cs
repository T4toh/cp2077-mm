using NexusMods.Paths;
using R3;

namespace NexusMods.App.UI.Overlays;

public record struct RemoveGameOverlayResult(bool ShouldRemoveGame, bool ShouldDeleteDownloads, bool ShouldCleanGameFolder)
{
    public static readonly RemoveGameOverlayResult Cancel = new(ShouldRemoveGame: false, ShouldDeleteDownloads: false, ShouldCleanGameFolder: false);
}

public interface IRemoveGameOverlayViewModel : IOverlayViewModel<RemoveGameOverlayResult>
{
    string GameName { get; }

    int NumDownloads { get; }

    Size SumDownloadsSize { get; }

    int NumCollections { get; }

    BindableReactiveProperty<bool> ShouldDeleteDownloads { get; }
    BindableReactiveProperty<bool> ShouldCleanGameFolder { get; }

    ReactiveCommand<Unit> CommandCancel { get; }

    ReactiveCommand<Unit> CommandRemove { get; }
}
