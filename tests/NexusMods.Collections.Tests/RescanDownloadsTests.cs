using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.NexusModsLibrary.Models;
using NexusMods.Abstractions.NexusWebApi.Types;
using NexusMods.Games.TestFramework;
using NexusMods.Paths;
using NexusMods.Sdk.Hashes;
using NexusMods.Sdk.Library;
using NexusMods.Sdk.NexusModsApi;
using NexusMods.Sdk.Settings;
using Xunit;

namespace NexusMods.Collections.Tests;

/// <summary>
/// <see cref="CollectionDownloader.RescanDownloads"/> must link a download that is already on disk, however small
/// (a 447-byte mod was skipped and downloaded again after every reset).
/// </summary>
public class RescanDownloadsTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<RescanDownloadsTests>(helper)
{
    [Fact]
    public async Task TinyDownloadOnDisk_IsLinked()
    {
        var downloads = ServiceProvider.GetRequiredService<ISettingsManager>().Get<DownloadsSettings>().Folder.ToPath(FileSystem);
        downloads.CreateDirectory();
        var content = new byte[447];
        Random.Shared.NextBytes(content);
        var file = downloads.Combine("tiny.7z");
        await file.WriteAllBytesAsync(content);

        using var tx = Connection.BeginTransaction();
        var collection = new CollectionMetadata.New(tx)
        {
            CollectionId = CollectionId.From(1),
            Slug = CollectionSlug.From("tiny"),
            Name = "Tiny",
            GameId = NexusModsGameId.From(3333),
            AuthorId = tx.TempId(),
        };
        var revision = new CollectionRevisionMetadata.New(tx)
        {
            RevisionId = RevisionId.From(1),
            RevisionNumber = RevisionNumber.From(1),
            CollectionId = collection.Id,
        };
        _ = new CollectionDownloadExternal.New(tx, out var downloadId)
        {
            CollectionDownload = new CollectionDownload.New(tx, downloadId)
            {
                CollectionRevisionId = revision.Id,
                Name = "tiny",
                IsOptional = false,
                ArrayIndex = 0,
            },
            Md5 = Md5Value.From(MD5.HashData(content)),
            Size = Size.From((ulong)content.Length),
            Uri = new Uri("https://example.com/tiny.7z"),
        };
        var result = await tx.Commit();

        var downloader = ServiceProvider.GetRequiredService<CollectionDownloader>();
        var matched = await downloader.RescanDownloads(CollectionRevisionMetadata.Load(result.Db, result[revision.Id]), CancellationToken.None);

        matched.Should().Be(1);
    }
}
