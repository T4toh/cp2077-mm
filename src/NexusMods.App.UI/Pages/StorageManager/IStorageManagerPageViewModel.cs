using NexusMods.App.UI.WorkspaceSystem;
using R3;

namespace NexusMods.App.UI.Pages.StorageManager;

public interface IStorageManagerPageViewModel : IPageViewModelInterface
{
    /// <summary>Human-readable total size of all .nx archive files.</summary>
    string ArchivesSizeText { get; }

    /// <summary>Number of original game files currently backed up.</summary>
    int BackedUpFilesCount { get; }

    /// <summary>Human-readable total size of the downloads folder.</summary>
    string DownloadsFolderSizeText { get; }

    /// <summary>Human-readable total size of the CyberpunkBackups folder.</summary>
    string CyberpunkBackupsSizeText { get; }

    /// <summary>True while GC or Deep Clean is running.</summary>
    bool IsBusy { get; }

    /// <summary>Runs the garbage collector to reclaim unused archive chunks.</summary>
    ReactiveCommand<Unit> RunGarbageCollectionCommand { get; }

    /// <summary>Deletes all game file backups then runs GC (frees maximum space).</summary>
    ReactiveCommand<Unit> DeepCleanCommand { get; }

    /// <summary>Refreshes the storage stats from disk.</summary>
    ReactiveCommand<Unit> RefreshCommand { get; }
}
