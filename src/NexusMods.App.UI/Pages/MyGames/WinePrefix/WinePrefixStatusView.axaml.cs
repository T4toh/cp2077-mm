using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using NexusMods.UI.Sdk.Icons;
using ReactiveUI;

namespace NexusMods.App.UI.Pages.MyGames.WinePrefix;

public partial class WinePrefixStatusView : ReactiveUserControl<IWinePrefixStatusViewModel>
{
    private static readonly IconValue CheckIcon = IconValues.CheckCircleOutline;
    private static readonly IconValue WarningIcon = IconValues.WarningAmber;

    public WinePrefixStatusView()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            this.WhenAnyValue(v => v.ViewModel!.IsVisible)
                .Subscribe(visible => IsVisible = visible)
                .DisposeWith(d);

            this.WhenAnyValue(v => v.ViewModel!.IsExpanded)
                .BindTo(this, v => v.WinePrefixExpander.IsExpanded)
                .DisposeWith(d);

            // Header icon
            this.WhenAnyValue(v => v.ViewModel!.HasIssues)
                .Subscribe(hasIssues =>
                {
                    StatusIcon.Value = hasIssues ? WarningIcon : CheckIcon;
                })
                .DisposeWith(d);

            // Protontricks status
            this.WhenAnyValue(v => v.ViewModel!.IsProtontricksInstalled)
                .Subscribe(installed =>
                {
                    ProtontricksIcon.Value = installed ? CheckIcon : WarningIcon;
                    ProtontricksText.Text = installed
                        ? "Protontricks: Instalado"
                        : "Protontricks: No encontrado";
                })
                .DisposeWith(d);

            // d3dcompiler_47 status
            this.WhenAnyValue(v => v.ViewModel!.IsD3dCompiler47Installed)
                .Subscribe(installed =>
                {
                    D3dCompilerIcon.Value = installed ? CheckIcon : WarningIcon;
                    D3dCompilerText.Text = installed
                        ? "d3dcompiler_47: Instalado"
                        : "d3dcompiler_47: Faltante";
                })
                .DisposeWith(d);

            // vcrun2022 status
            this.WhenAnyValue(v => v.ViewModel!.IsVcRun2022Installed)
                .Subscribe(installed =>
                {
                    VcRunIcon.Value = installed ? CheckIcon : WarningIcon;
                    VcRunText.Text = installed
                        ? "vcrun2022: Instalado"
                        : "vcrun2022: Faltante";
                })
                .DisposeWith(d);

            // DLL overrides status
            this.WhenAnyValue(v => v.ViewModel!.HasCorrectDllOverrides)
                .Subscribe(correct =>
                {
                    DllOverridesIcon.Value = correct ? CheckIcon : WarningIcon;
                    DllOverridesText.Text = correct
                        ? "DLL Overrides: Correctos"
                        : "DLL Overrides: Configuracion incorrecta";
                })
                .DisposeWith(d);

            // Instructions visibility
            this.WhenAnyValue(
                    v => v.ViewModel!.DllOverridesInstructions,
                    v => v.ViewModel!.WinetricksInstructions)
                .Subscribe(tuple =>
                {
                    var (dllInstructions, wtInstructions) = tuple;
                    DllOverridesInstructionsText.Text = dllInstructions ?? "";
                    DllOverridesInstructionsText.IsVisible = dllInstructions is not null;
                    WinetricksInstructionsText.Text = wtInstructions ?? "";
                    WinetricksInstructionsText.IsVisible = wtInstructions is not null;
                    InstructionsBorder.IsVisible = dllInstructions is not null || wtInstructions is not null;
                })
                .DisposeWith(d);

            // Refresh button
            this.BindCommand(ViewModel!, vm => vm.RefreshCommand, v => v.RefreshButton)
                .DisposeWith(d);
        });
    }
}
