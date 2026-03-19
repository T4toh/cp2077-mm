using Avalonia.Media.Imaging;
using NexusMods.Abstractions.Games;
using NexusMods.Sdk.Games;
using NexusMods.UI.Sdk;

namespace NexusMods.App.UI.Controls.MiniGameWidget.Standard;

public interface IMiniGameWidgetViewModel : IViewModelInterface
{
    public IGame? Game { get; set; }
    public GameInstallation[]? GameInstallations { get; set; }
    public string Name { get; set; }
    public bool IsFound { get; set; }
    public Bitmap Image { get; }
}
