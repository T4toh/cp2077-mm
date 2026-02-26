using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.App.UI.Controls;
using NexusMods.App.UI.LeftMenu.Items;
using NexusMods.App.UI.Pages.Downloads;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Sdk.NexusModsApi;
using NexusMods.UI.Sdk;
using NexusMods.UI.Sdk.Icons;

namespace NexusMods.App.UI.LeftMenu.Downloads;

[UsedImplicitly]
public class DownloadsLeftMenuViewModel : AViewModel<IDownloadsLeftMenuViewModel>, IDownloadsLeftMenuViewModel
{
    public WorkspaceId WorkspaceId { get; }
    public ILeftMenuItemViewModel LeftMenuItemAllDownloads { get; }

    public DownloadsLeftMenuViewModel(
        WorkspaceId workspaceId,
        IWorkspaceController workspaceController,
        IServiceProvider serviceProvider)
    {
        WorkspaceId = workspaceId;

        LeftMenuItemAllDownloads = new LeftMenuItemViewModel(
            workspaceController,
            WorkspaceId,
            new PageData
            {
                FactoryId = DownloadsPageFactory.StaticId,
                Context = new DownloadsPageContext { GameScope = DynamicData.Kernel.Optional<NexusModsGameId>.None },
            }
        )
        {
            Text = new StringComponent(Language.DownloadsLeftMenu_AllDownloads),
            Icon = IconValues.Download,
        };
    }
}
