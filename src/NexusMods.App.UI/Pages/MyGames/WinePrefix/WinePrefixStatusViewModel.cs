using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using NexusMods.Sdk;
using NexusMods.Sdk.Games;
using NexusMods.UI.Sdk;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Pages.MyGames.WinePrefix;

public class WinePrefixStatusViewModel : AViewModel<IWinePrefixStatusViewModel>, IWinePrefixStatusViewModel
{
    private static readonly WineDllOverride[] RequiredOverrides =
    [
        new("winmm", [WineDllOverrideType.Native, WineDllOverrideType.BuiltIn]),
        new("version", [WineDllOverrideType.Native, WineDllOverrideType.BuiltIn]),
    ];

    private static readonly ImmutableHashSet<string> RequiredWinetricksPackages = ["d3dcompiler_47", "vcrun2022"];

    private readonly ILinuxCompatabilityDataProvider? _linuxCompat;
    private readonly IRuntimeDependency? _protontricks;

    [Reactive] public bool IsVisible { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool HasIssues { get; set; }
    [Reactive] public bool IsProtontricksInstalled { get; set; }
    [Reactive] public bool IsD3dCompiler47Installed { get; set; }
    [Reactive] public bool IsVcRun2022Installed { get; set; }
    [Reactive] public bool HasCorrectDllOverrides { get; set; }
    [Reactive] public string? DllOverridesInstructions { get; set; }
    [Reactive] public string? WinetricksInstructions { get; set; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    public WinePrefixStatusViewModel(
        GameInstallation installation,
        IEnumerable<IRuntimeDependency> runtimeDependencies)
    {
        _linuxCompat = installation.LocatorResult.LinuxCompatabilityDataProvider;
        _protontricks = runtimeDependencies
            .FirstOrDefault(d => d.DisplayName == "Protontricks");

        RefreshCommand = ReactiveCommand.CreateFromTask(RunChecksAsync);

        this.WhenActivated(d =>
        {
            if (_linuxCompat is null)
            {
                IsVisible = false;
                return;
            }

            IsVisible = true;

            Observable.StartAsync(RunChecksAsync)
                .Subscribe()
                .DisposeWith(d);
        });
    }

    private async Task RunChecksAsync(CancellationToken ct = default)
    {
        if (_linuxCompat is null) return;

        // Check protontricks
        if (_protontricks is not null)
        {
            var info = await _protontricks.QueryInstallationInformation(ct);
            IsProtontricksInstalled = info.HasValue;
        }
        else
        {
            IsProtontricksInstalled = false;
        }

        // Check winetricks packages
        var installedPackages = await _linuxCompat.GetInstalledWinetricksComponents(cancellationToken: ct);
        IsD3dCompiler47Installed = installedPackages.Contains("d3dcompiler_47");
        IsVcRun2022Installed = installedPackages.Contains("vcrun2022");

        var missingPackages = RequiredWinetricksPackages.Except(installedPackages);
        if (missingPackages.Count > 0)
        {
            var missingList = missingPackages.Select(x => $"* `{x}`").Aggregate((a, b) => $"{a}\n{b}");
            WinetricksInstructions = $"""
Usa [protontricks](https://github.com/Matoking/protontricks) para instalar los paquetes faltantes:

{missingList}
""";
        }
        else
        {
            WinetricksInstructions = null;
        }

        // Check DLL overrides
        var existingOverrides = await _linuxCompat.GetWineDllOverrides(cancellationToken: ct);
        var allOverridesCorrect = true;

        foreach (var required in RequiredOverrides)
        {
            var found = existingOverrides.FirstOrDefault(o =>
                o.DllName.Equals(required.DllName, StringComparison.OrdinalIgnoreCase));

            if (found.DllName is null || !found.OverrideTypes.SequenceEqual(required.OverrideTypes))
            {
                allOverridesCorrect = false;
                break;
            }
        }

        HasCorrectDllOverrides = allOverridesCorrect;

        if (!allOverridesCorrect)
        {
            var dllOverridesString = RequiredOverrides
                .Select(x => x.ToString())
                .Aggregate((a, b) => $"{a};{b}");

            DllOverridesInstructions = $"""
* Abrir Steam
* Click derecho en el juego
* Click en "Propiedades..."
* Ir a la seccion "General"
* Actualizar "Opciones de lanzamiento" con:

```
WINEDLLOVERRIDES="{dllOverridesString}" %command%
```
""";
        }
        else
        {
            DllOverridesInstructions = null;
        }

        HasIssues = !IsProtontricksInstalled || !IsD3dCompiler47Installed || !IsVcRun2022Installed || !HasCorrectDllOverrides;
        IsExpanded = HasIssues;
    }
}
