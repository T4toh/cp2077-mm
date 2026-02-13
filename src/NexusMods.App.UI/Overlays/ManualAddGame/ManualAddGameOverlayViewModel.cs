using Avalonia.Platform.Storage;
using NexusMods.Paths;
using NexusMods.UI.Sdk;
using R3;

namespace NexusMods.App.UI.Overlays;

public class ManualAddGameOverlayViewModel : AOverlayViewModel<IManualAddGameOverlayViewModel, ManualAddGameOverlayResult>, IManualAddGameOverlayViewModel
{
    public BindableReactiveProperty<string> GamePath { get; } = new(value: string.Empty);
    public BindableReactiveProperty<string> WinePrefix { get; } = new(value: string.Empty);
    
    public ReactiveCommand<Unit> CommandBrowseGamePath { get; }
    public ReactiveCommand<Unit> CommandBrowseWinePrefix { get; }
    public ReactiveCommand<Unit> CommandCancel { get; }
    public ReactiveCommand<Unit> CommandAdd { get; }

    public ManualAddGameOverlayViewModel(IAvaloniaInterop avaloniaInterop)
    {
        CommandBrowseGamePath = new ReactiveCommand(async (_, _) =>
        {
            var options = new FolderPickerOpenOptions
            {
                Title = "Select Cyberpunk 2077 Installation Folder",
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
            if (string.IsNullOrWhiteSpace(GamePath.Value)) return;
            Complete(result: new ManualAddGameOverlayResult(Confirmed: true, GamePath: GamePath.Value, WinePrefix: WinePrefix.Value));
        });
    }
}
