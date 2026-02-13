using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.Downloads;
using NexusMods.Abstractions.HttpDownloads;
using NexusMods.Abstractions.Library.Jobs;
using NexusMods.Abstractions.NexusModsLibrary;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Networking.HttpDownloader;
using NexusMods.Paths;
using NexusMods.Sdk.Jobs;
using NexusMods.Sdk.Library;

namespace NexusMods.Library;

internal class AddDownloadJob : IJobDefinitionWithStart<AddDownloadJob, LibraryFile.ReadOnly>, IAddDownloadJob
{
    public required IJobTask<IDownloadJob, AbsolutePath> DownloadJob { get; init; }
    internal required IConnection Connection { get; set; }
    internal required IServiceProvider ServiceProvider { get; set; }

    public static IJobTask<AddDownloadJob, LibraryFile.ReadOnly> Create(IServiceProvider provider, IJobTask<IDownloadJob, AbsolutePath> downloadJob)
    {
        var monitor = provider.GetRequiredService<IJobMonitor>();
        var job = new AddDownloadJob
        {
            DownloadJob = downloadJob,
            Connection = provider.GetRequiredService<IConnection>(),
            ServiceProvider = provider,
        };
        return monitor.Begin<AddDownloadJob, LibraryFile.ReadOnly>(job);
    }

    public async ValueTask<LibraryFile.ReadOnly> StartAsync(IJobContext<AddDownloadJob> context)
    {
        await context.YieldAsync();

        // This throws if download cancelled, will be caught downstream.
        await DownloadJob;

        // Preserve a copy of the original downloaded file
        await PreserveOriginalFile(context.CancellationToken);

        await context.YieldAsync();
        using var tx = Connection.BeginTransaction();

        var libraryFile = await AddLibraryFileJob.Create(ServiceProvider, tx, DownloadJob.Result);
        await DownloadJob.JobDefinition.AddMetadata(tx, libraryFile);

        var transactionResult = await tx.Commit();
        return transactionResult.Remap(libraryFile);
    }

    private async Task PreserveOriginalFile(CancellationToken ct)
    {
        try
        {
            var downloadedFilePath = DownloadJob.Result;
            if (!downloadedFilePath.FileExists) return;

            var fs = ServiceProvider.GetRequiredService<IFileSystem>();
            var downloadsFolder = GetDownloadsFolder(fs);

            if (!downloadsFolder.DirectoryExists())
                downloadsFolder.CreateDirectory();

            var destFileName = GetMeaningfulFileName(downloadedFilePath);
            var destPath = downloadsFolder.Combine(destFileName);

            // Avoid overwriting existing files
            if (destPath.FileExists)
            {
                var stem = destPath.GetFileNameWithoutExtension();
                var extStr = destPath.Extension.ToString();
                var counter = 1;
                do
                {
                    destPath = downloadsFolder.Combine($"{stem}_{counter}{extStr}");
                    counter++;
                } while (destPath.FileExists);
            }

            await using var source = downloadedFilePath.Read();
            await using var dest = destPath.Create();
            await source.CopyToAsync(dest, ct);
        }
        catch (Exception ex)
        {
            var logger = ServiceProvider.GetService<ILogger<AddDownloadJob>>();
            logger?.LogWarning(ex, "No se pudo preservar el archivo original descargado");
        }
    }

    /// <summary>
    /// Tries to get a meaningful filename from NexusMods metadata, the download URI,
    /// or falls back to the temp file name.
    /// </summary>
    private string GetMeaningfulFileName(AbsolutePath tempFilePath)
    {
        var filename = string.Empty;

        // 1. Try to get filename from HTTP headers (Content-Disposition)
        // Check if the job is an HTTP job directly
        if (DownloadJob.JobDefinition is IHttpDownloadJob httpJob && httpJob.GetJobStateData() is IHttpDownloadState state && state.FileName.HasValue)
        {
            filename = state.FileName.Value.ToString();
        }
        // Check if the job is a Nexus job that contains an HTTP job
        else if (DownloadJob.JobDefinition is INexusModsDownloadJob nxmJob && nxmJob.HttpDownloadJob.JobDefinition.GetJobStateData() is IHttpDownloadState state2 && state2.FileName.HasValue)
        {
            filename = state2.FileName.Value.ToString();
        }

        // 2. Prioritize NexusMods file metadata name if we still don't have a filename
        if (string.IsNullOrEmpty(filename) && DownloadJob.JobDefinition is INexusModsDownloadJob nxmJob2)
        {
            try
            {
                var metadataName = nxmJob2.FileMetadata.Name;
                if (!string.IsNullOrEmpty(metadataName))
                    filename = metadataName;
            }
            catch
            {
                // Metadata might not be available
            }
        }

        // 3. Fall back to extracting filename from HTTP URI
        if (string.IsNullOrEmpty(filename))
        {
            IHttpDownloadJob? httpJobFallback = DownloadJob.JobDefinition switch
            {
                IHttpDownloadJob h => h,
                INexusModsDownloadJob n => n.HttpDownloadJob.JobDefinition,
                _ => null
            };

            if (httpJobFallback != null)
            {
                try
                {
                    var uriPath = httpJobFallback.Uri.AbsolutePath;
                    var lastSegment = uriPath.Split('/').LastOrDefault(s => !string.IsNullOrEmpty(s));
                    if (!string.IsNullOrEmpty(lastSegment) && lastSegment.Contains('.'))
                    {
                        filename = Uri.UnescapeDataString(lastSegment);
                    }
                }
                catch
                {
                    // Ignore URI parsing errors
                }
            }
        }

        if (string.IsNullOrEmpty(filename))
            filename = tempFilePath.FileName;

        // Ensure we have an extension if the temp file has one, but AVOID .tmp
        var ext = tempFilePath.Extension;
        var extStr = ext.ToString();
        if (ext != Extension.None && !extStr.Equals(".tmp", StringComparison.OrdinalIgnoreCase) && !filename.EndsWith(extStr, StringComparison.OrdinalIgnoreCase))
        {
            if (!Path.HasExtension(filename))
                filename += extStr;
        }

        return filename;
    }

    private static AbsolutePath GetDownloadsFolder(IFileSystem fs)
    {
        var basePath = fs.OS.MatchPlatform(
            onWindows: () => KnownPath.LocalApplicationDataDirectory,
            onLinux: () => KnownPath.XDG_DATA_HOME,
            onOSX: () => KnownPath.LocalApplicationDataDirectory
        );

        var dirName = fs.OS.IsOSX ? "NexusMods_App" : "NexusMods.App";
        return fs.GetKnownPath(basePath).Combine(dirName).Combine("Downloads");
    }
}
