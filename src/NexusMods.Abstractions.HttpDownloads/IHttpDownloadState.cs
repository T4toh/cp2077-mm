using System.ComponentModel;
using DynamicData.Kernel;
using NexusMods.Paths;
using NexusMods.Sdk.Jobs;
using NexusMods.Sdk.Library;

namespace NexusMods.Abstractions.HttpDownloads;

/// <summary>
/// Public interface for external access to download information
/// </summary>
public interface IHttpDownloadState : IPublicJobStateData, INotifyPropertyChanged
{
    /// <summary>
    /// Content length from HTTP headers
    /// </summary>
    Optional<Size> ContentLength { get; }
    
    /// <summary>
    /// Total bytes downloaded so far
    /// </summary>
    Size TotalBytesDownloaded { get; }

    /// <summary>
    /// Meaningful filename from Content-Disposition header.
    /// </summary>
    Optional<RelativePath> FileName { get; }
}
