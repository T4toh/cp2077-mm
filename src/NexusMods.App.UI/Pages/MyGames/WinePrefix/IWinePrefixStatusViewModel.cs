using System.Reactive;
using NexusMods.UI.Sdk;
using ReactiveUI;

namespace NexusMods.App.UI.Pages.MyGames.WinePrefix;

public interface IWinePrefixStatusViewModel : IViewModelInterface
{
    bool IsVisible { get; }
    bool IsExpanded { get; set; }
    bool HasIssues { get; }
    bool IsProtontricksInstalled { get; }
    bool IsD3dCompiler47Installed { get; }
    bool IsVcRun2022Installed { get; }
    bool HasCorrectDllOverrides { get; }
    string? DllOverridesInstructions { get; }
    string? WinetricksInstructions { get; }
    ReactiveCommand<Unit, Unit> RefreshCommand { get; }
}
