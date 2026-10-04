using Avalonia.Platform.Storage;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.UI.Sdk;
using R3;

namespace NexusMods.App.UI.Overlays;

public class ManualAddGameOverlayViewModel : AOverlayViewModel<IManualAddGameOverlayViewModel, ManualAddGameOverlayResult>, IManualAddGameOverlayViewModel
{
    public IReadOnlyList<IGameData> Games { get; }
    public BindableReactiveProperty<IGameData?> SelectedGame { get; }
    public BindableReactiveProperty<string> GamePath { get; } = new(value: string.Empty);
    public BindableReactiveProperty<string> WinePrefix { get; } = new(value: string.Empty);
    
    public ReactiveCommand<Unit> CommandBrowseGamePath { get; }
    public ReactiveCommand<Unit> CommandBrowseWinePrefix { get; }
    public ReactiveCommand<Unit> CommandCancel { get; }
    public ReactiveCommand<Unit> CommandAdd { get; }

    public ManualAddGameOverlayViewModel(IAvaloniaInterop avaloniaInterop, IEnumerable<IGameData> games)
    {
        Games = games.OrderBy(game => game.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();
        SelectedGame = new BindableReactiveProperty<IGameData?>(Games.Count == 1 ? Games[0] : null);

        CommandBrowseGamePath = new ReactiveCommand(async (_, _) =>
        {
            var options = new FolderPickerOpenOptions
            {
                Title = SelectedGame.Value is { } game ? $"Select the {game.DisplayName} installation folder" : "Select the game installation folder",
                AllowMultiple = false
            };
            var result = await avaloniaInterop.OpenFolderPickerAsync(options);
            if (result.Length > 0)
            {
                GamePath.Value = result[0].ToString();
            }
        });

        CommandBrowseWinePrefix = new ReactiveCommand(async (_, _) =>
        {
            var options = new FolderPickerOpenOptions
            {
                Title = "Select WINE Prefix Folder",
                AllowMultiple = false
            };
            var result = await avaloniaInterop.OpenFolderPickerAsync(options);
            if (result.Length > 0)
            {
                WinePrefix.Value = result[0].ToString();
            }
        });

        CommandCancel = new ReactiveCommand(_ => Complete(result: ManualAddGameOverlayResult.Cancel));
        
        CommandAdd = new ReactiveCommand(_ => 
        {
            if (SelectedGame.Value is not { } game || string.IsNullOrWhiteSpace(GamePath.Value)) return;
            Complete(result: new ManualAddGameOverlayResult(Confirmed: true, Game: game, GamePath: GamePath.Value, WinePrefix: WinePrefix.Value));
        });
    }
}
