using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusMods.Abstractions.Downloads;
using NexusMods.Abstractions.HttpDownloads;
using NexusMods.Abstractions.Library.Jobs;
using NexusMods.Abstractions.NexusModsLibrary;
using NexusMods.MnemonicDB.Abstractions;
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
                var ext = destPath.Extension;
                var counter = 1;
                do
                {
                    destPath = downloadsFolder.Combine($"{stem}_{counter}{ext}");
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
        // Prioritize NexusMods file metadata name (e.g. "Enhanced Police v1.7z")
        if (DownloadJob.JobDefinition is INexusModsDownloadJob nxmJob)
        {
            try
            {
                var metadataName = nxmJob.FileMetadata.Name;
                if (!string.IsNullOrEmpty(metadataName))
                    return metadataName;
            }
            catch
            {
                // Metadata might not be available
            }
        }

        // Fall back to extracting filename from HTTP URI
        if (DownloadJob.JobDefinition is IHttpDownloadJob httpJob)
        {
            try
            {
                var uriPath = httpJob.Uri.AbsolutePath;
                var lastSegment = uriPath.Split('/').LastOrDefault(s => !string.IsNullOrEmpty(s));
                if (!string.IsNullOrEmpty(lastSegment) && lastSegment.Contains('.'))
                {
                    return Uri.UnescapeDataString(lastSegment);
                }
            }
            catch
            {
                // Ignore URI parsing errors
            }
        }

        return tempFilePath.FileName;
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
