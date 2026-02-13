using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Abstractions.Serialization.Attributes;
using NexusMods.App.UI.Windows;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Sdk.Loadouts;
using NexusMods.UI.Sdk.Icons;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.MnemonicDB.Abstractions;

namespace NexusMods.App.UI.Pages.EssentialMods;

[JsonName("NexusMods.App.UI.Pages.EssentialMods.EssentialModsPageContext")]
public record EssentialModsPageContext : IPageFactoryContext
{
    public required LoadoutId LoadoutId { get; init; }
}

[UsedImplicitly]
public class EssentialModsPageFactory : APageFactory<IEssentialModsViewModel, EssentialModsPageContext>
{
    public EssentialModsPageFactory(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public static readonly PageFactoryId StaticId = PageFactoryId.From(Guid.Parse("7a8b9c0d-1e2f-4a5b-8c9d-0e1f2a3b4c5d"));
    public override PageFactoryId Id => StaticId;

    public override IEssentialModsViewModel CreateViewModel(EssentialModsPageContext context)
    {
        var loadout = Loadout.Load(ServiceProvider.GetRequiredService<IConnection>().Db, context.LoadoutId);
        return new EssentialModsViewModel(
            ServiceProvider.GetRequiredService<IWindowManager>(),
            ServiceProvider,
            context.LoadoutId,
            loadout.InstallationInstance.Game.NexusModsGameId.Value);
    }

    public override IEnumerable<PageDiscoveryDetails?> GetDiscoveryDetails(IWorkspaceContext workspaceContext)
    {
        if (workspaceContext is not LoadoutContext loadoutContext) yield break;
        
        var loadout = Loadout.Load(ServiceProvider.GetRequiredService<IConnection>().Db, loadoutContext.LoadoutId);
        if (loadout.InstallationInstance.Game.GameId != Cyberpunk2077Game.GameId) yield break;

        yield return new PageDiscoveryDetails
        {
            SectionName = "Mods",
            ItemName = "Essential Mods",
            Icon = IconValues.Star,
            PageData = new PageData
            {
                FactoryId = Id,
                Context = new EssentialModsPageContext
                {
                    LoadoutId = loadoutContext.LoadoutId,
                },
            },
        };
    }
}
