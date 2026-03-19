using NexusMods.Abstractions.Downloads;
using NexusMods.App.UI.Controls;
using NexusMods.App.UI.Controls.Filters;
using NexusMods.App.UI.Controls.TreeDataGrid.Filters;
using NexusMods.Sdk.Jobs;
using NexusMods.UI.Sdk;
using R3;

namespace NexusMods.App.UI.Pages.Downloads;

/// <summary>
/// DownloadRef: Reference holder for DownloadInfo objects
/// - This component is static and never changes
/// </summary>
public sealed class DownloadRef(DownloadInfo download) : ReactiveR3Object, IItemModelComponent<DownloadRef>, IComparable<DownloadRef>
{
    public DownloadId DownloadId { get; } = download.Id;
    public DownloadInfo Download { get; } = download;

    public int CompareTo(DownloadRef? other)
    {
        if (other is null) return 1;
        return DownloadId.CompareTo(other.DownloadId);
    }

    public FilterResult MatchesFilter(Filter filter) => FilterResult.Indeterminate;
}

/// <summary>
    /// Components for Downloads data display.
    /// </summary>
public static class DownloadComponents
{
    /// <summary>
    /// GAME COLUMN COMPONENT
    /// - Shows game name and game icon
    /// - This is static, never changes.
    /// </summary>
    public sealed class GameComponent(string gameName) : ReactiveR3Object, IItemModelComponent<GameComponent>, IComparable<GameComponent>
    {
        public IReadOnlyBindableReactiveProperty<string> GameName { get; } = new BindableReactiveProperty<string>(gameName);

        public int CompareTo(GameComponent? other)
        {
            if (other is null) return 1;
            return string.Compare(GameName.Value, other.GameName.Value, StringComparison.OrdinalIgnoreCase);
        }

        public FilterResult MatchesFilter(Filter filter)
        {
            return filter switch
            {
                Filter.NameFilter nameFilter => GameName.Value.Contains(
                    nameFilter.SearchText, 
                    nameFilter.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)
                    ? FilterResult.Pass : FilterResult.Fail,
                Filter.TextFilter textFilter => GameName.Value.Contains(
                    textFilter.SearchText, 
                    textFilter.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)
                    ? FilterResult.Pass : FilterResult.Fail,
                _ => FilterResult.Indeterminate,
            };
        }

        private bool _isDisposed;
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing) GameName.Dispose();
                _isDisposed = true;
            }
            base.Dispose(disposing);
        }
    }

    // SizeProgressComponent and SpeedComponent have been extracted to SharedProgressComponents
    // in NexusMods.App.UI.Controls for reuse by both regular downloads and collection downloads.

    /// <summary>
    /// STATUS COLUMN COMPONENT
    /// - Contains embedded controls: progress bar, pause/resume button, cancel button, kebab menu
    /// - All download actions consolidated into this single column
    /// </summary>
    public sealed class StatusComponent : ReactiveR3Object, IItemModelComponent<StatusComponent>, IComparable<StatusComponent>
    {
        public IReadOnlyBindableReactiveProperty<double> Progress { get; }
        public IReadOnlyBindableReactiveProperty<JobStatus> Status { get; }
        public IReadOnlyBindableReactiveProperty<bool> IsPaused { get; }
        
        // Commands
        public ReactiveCommand<Unit> PauseCommand { get; } = new();
        public ReactiveCommand<Unit> ResumeCommand { get; } = new();
        public ReactiveCommand<Unit> CancelCommand { get; } = new();
        
        // Visibility based on JobStatus
        public IReadOnlyBindableReactiveProperty<bool> CanPause { get; }
        public IReadOnlyBindableReactiveProperty<bool> CanResume { get; }
        public IReadOnlyBindableReactiveProperty<bool> CanCancel { get; }
        public IReadOnlyBindableReactiveProperty<bool> IsCompleted { get; }

        public StatusComponent(
            Percent initialProgress,
            JobStatus initialStatus,
            Observable<Percent> progressObservable,
            Observable<JobStatus> statusObservable)
        {
            Progress = progressObservable.Select(p => p.Value).ToBindableReactiveProperty(initialProgress.Value);
            Status = statusObservable.ToBindableReactiveProperty(initialStatus);
            IsPaused = statusObservable.Select(status => status == JobStatus.Paused).ToBindableReactiveProperty(initialStatus == JobStatus.Paused);

            // Set up can-execute properties based on status  
            CanPause = statusObservable
                .Select(static status => status == JobStatus.Running)
                .ToBindableReactiveProperty(initialStatus == JobStatus.Running);

            CanResume = statusObservable
                .Select(static status => status == JobStatus.Paused)
                .ToBindableReactiveProperty(initialStatus == JobStatus.Paused);

            CanCancel = statusObservable
                .Select(static status => status is JobStatus.Created or JobStatus.Running or JobStatus.Paused)
                .ToBindableReactiveProperty(initialStatus is JobStatus.Created or JobStatus.Running or JobStatus.Paused);

            IsCompleted = statusObservable
                .Select(static status => status == JobStatus.Completed)
                .ToBindableReactiveProperty(initialStatus == JobStatus.Completed);
        }

        public int CompareTo(StatusComponent? other)
        {
            if (other is null) return 1;
            return Status.Value.CompareTo(other.Status.Value);
        }

        public FilterResult MatchesFilter(Filter filter)
        {
            return filter switch
            {
                Filter.TextFilter textFilter => Status.Value.ToString().Contains(
                    textFilter.SearchText, 
                    textFilter.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)
                    ? FilterResult.Pass : FilterResult.Fail,
                _ => FilterResult.Indeterminate,
            };
        }

        private bool _isDisposed;
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    Disposable.Dispose(Progress, Status, PauseCommand, ResumeCommand, CancelCommand, CanPause, CanResume, CanCancel, IsCompleted);

                _isDisposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
