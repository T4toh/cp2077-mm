using NexusMods.Abstractions.Serialization.ExpressionGenerator;
using NexusMods.App.UI.Pages;
using NexusMods.App.UI.Pages.Changelog;
using NexusMods.App.UI.Pages.CollectionDownload;
#if DEBUG
using NexusMods.App.UI.Pages.DebugControls;
#endif
using NexusMods.App.UI.Pages.Diagnostics;
using NexusMods.App.UI.Pages.Diff.ApplyDiff;
using NexusMods.App.UI.Pages.Downloads;
using NexusMods.App.UI.Pages.EssentialMods;
using NexusMods.App.UI.Pages.LibraryPage;
using NexusMods.App.UI.Pages.LoadoutGroupFilesPage;
using NexusMods.App.UI.Pages.LoadoutPage;
using NexusMods.App.UI.Pages.MyGames;
using NexusMods.App.UI.Pages.MyLoadouts;
#if DEBUG
using NexusMods.App.UI.Pages.ObservableInfo;
#endif
using NexusMods.App.UI.Pages.Settings;
using NexusMods.App.UI.Pages.StorageManager;
using NexusMods.App.UI.Pages.TextEdit;
using NexusMods.App.UI.WorkspaceSystem;

namespace NexusMods.App.UI;

internal class TypeFinder : ITypeFinder
{
    public IEnumerable<Type> DescendentsOf(Type type)
    {
        return AllTypes.Where(t => t.IsAssignableTo(type));
    }

    private static IEnumerable<Type> AllTypes => new[]
    {
        // factory context
        typeof(MyGamesPageContext),
        typeof(DiagnosticListPageContext),
        typeof(ApplyDiffPageContext),
        typeof(SettingsPageContext),
        typeof(ChangelogPageContext),
        typeof(TextEditorPageContext),
        typeof(MyLoadoutsPageContext),
        typeof(LoadoutGroupFilesPageContext),
        typeof(LibraryPageContext),
        typeof(LoadoutPageContext),
        typeof(CollectionLoadoutPageContext),
#if DEBUG
        typeof(ProtocolRegistrationTestPageContext),
#endif
        // Kept for backward compat: persisted window state may reference these
        typeof(DownloadsPageContext),
        typeof(EssentialModsPageContext),
        typeof(StorageManagerPageContext),

        // workspace context
        typeof(EmptyContext),
        typeof(HomeContext),
        typeof(LoadoutContext),
        // Kept for backward compat: persisted window state may reference this
        typeof(DownloadsContext),
        typeof(CollectionDownloadPageContext),
#if DEBUG
        typeof(ProtocolRegistrationTestPageContext),
        typeof(ObservableInfoPageContext),
        typeof(DebugControlsPageContext),
#endif

        // other
        typeof(WindowData),
    };
}
