using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData;
using NexusMods.App.UI.Windows;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Sdk.Loadouts;
using NexusMods.Sdk.NexusModsApi;
using NexusMods.UI.Sdk;
using NexusMods.UI.Sdk.Icons;
using ReactiveUI;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;

namespace NexusMods.App.UI.Pages.EssentialMods;

public class EssentialModsViewModel : APageViewModel<IEssentialModsViewModel>, IEssentialModsViewModel
{
    private readonly SourceList<IEssentialModEntryViewModel> _essentialModsSource = new();
    private readonly ReadOnlyObservableCollection<IEssentialModEntryViewModel> _essentialMods;
    public ReadOnlyObservableCollection<IEssentialModEntryViewModel> EssentialMods => _essentialMods;

    public ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> InstallAllCommand { get; }

    public EssentialModsViewModel(
        IWindowManager windowManager,
        IServiceProvider serviceProvider,
        LoadoutId loadoutId,
        NexusModsGameId nexusModsGameId) : base(windowManager)
    {
        TabTitle = "Essential Mods";
        TabIcon = IconValues.Star;

        foreach (var mod in NexusMods.Games.RedEngine.Cyberpunk2077.EssentialMods.Cyberpunk2077Essentials)
        {
            _essentialModsSource.Add(new EssentialModEntryViewModel(
                serviceProvider,
                loadoutId,
                nexusModsGameId,
                mod.Name,
                mod.ModId,
                mod.Description));
        }

        _essentialModsSource.Connect()
            .Bind(out _essentialMods)
            .Subscribe();

        InstallAllCommand = ReactiveUI.ReactiveCommand.CreateFromTask(async () =>
        {
            foreach (var mod in EssentialMods)
            {
                if (mod.Status != EssentialModStatus.Installed)
                {
                    await mod.InstallCommand.Execute(Unit.Default).FirstAsync();
                }
            }
        });
    }
}
