using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NexusMods.Backend.FileExtractor.Extractors;
using NexusMods.Hashing.xxHash3;
using NexusMods.Hashing.xxHash3.Paths;
using NexusMods.Paths;
using TUnit.Assertions.Enums;

namespace NexusMods.Backend.Tests.FileExtractor;

public class GenericExtractionTests : AFileExtractorTest
{
    [Test]
    [ArchiveDataSource]
    public async Task CanExtractAll(AbsolutePath path)
    {
        await using var tempFolder = TemporaryFileManager.CreateFolder();
        await FileExtractor.ExtractAllAsync(path, tempFolder, CancellationToken.None);

        var files = tempFolder.Path.EnumerateFiles();
        var results = new List<(RelativePath, Hash)>();
        foreach (var f in files)
            results.Add((f.RelativeTo(tempFolder.Path), await f.XxHash3Async()));
        var actual = results.ToArray();

        (RelativePath, Hash)[] expected = [
            ("deepFolder/deepFolder2/deepFolder3/deepFolder4/deepFile.txt", (Hash)0x3F0AB4D495E35A9A),
            ("folder1/folder1file.txt", (Hash)0x8520436F06348939),
            ("rootFile.txt", (Hash)0x818A82701BC1CC30),
        ];

        await Assert.That(actual).IsEquivalentTo(expected, ordering: CollectionOrdering.Any, comparer: EqualityComparer<(RelativePath, Hash)>.Default);
    }
}

internal class ArchiveDataSource : DataSourceGeneratorAttribute<AbsolutePath>
{
    protected override IEnumerable<Func<AbsolutePath>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        return FileSystem.Shared.GetKnownPath(KnownPath.CurrentDirectory)
            .Combine("Resources")
            .EnumerateFiles()
            .Where(file => file.FileName is "data_7zip_lzma2.7z" or "data_zip_lzma.zip")
            .Select<AbsolutePath, Func<AbsolutePath>>(file => () => file);
    }
}

[System.Runtime.Versioning.UnsupportedOSPlatform("windows")]
public class ExtractedPermissionsTests : AFileExtractorTest
{
    [Test]
    public async Task EntriesWithUnixModeZero_AreReadableAfterExtraction()
    {
        await using var tempFolder = TemporaryFileManager.CreateFolder();
        var zip = tempFolder.Path.Combine("mode0.zip");
        await using (var fs = zip.Create())
        using (var archive = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Create))
        {
            foreach (var name in new[] { "b.reds", "locked/a.reds" })
            {
                var entry = archive.CreateEntry(name);
                entry.ExternalAttributes = 0x8000 << 16; // regular file, permissions 000
                await using var w = new StreamWriter(entry.Open());
                await w.WriteAsync("contenido");
            }
        }

        // 7z restores unix modes (the managed zip extractor ignores them), as with the real .7z that had mode 0
        var sevenZip = ServiceProvider.GetServices<IExtractor>().OfType<SevenZipExtractor>().Single();
        var extractor = new NexusMods.Backend.FileExtractor.FileExtractor(NullLogger<NexusMods.Backend.FileExtractor.FileExtractor>.Instance, [sevenZip]);
        var dest = tempFolder.Path.Combine("out");
        await extractor.ExtractAllAsync(zip, dest, CancellationToken.None);

        foreach (var name in new[] { "b.reds", "locked/a.reds" })
            await Assert.That(await File.ReadAllTextAsync(dest.Combine(name).ToString())).IsEqualTo("contenido");
    }

    [Test]
    public async Task MakeOwnerAccessible_DoesNotFollowSymlinks()
    {
        await using var tempFolder = TemporaryFileManager.CreateFolder();
        var outside = tempFolder.Path.Combine("outside").ToString();
        var root = tempFolder.Path.Combine("root").ToString();
        Directory.CreateDirectory(outside);
        Directory.CreateDirectory(root);
        var target = Path.Combine(outside, "secret");
        await File.WriteAllTextAsync(target, "x");
        File.SetUnixFileMode(target, UnixFileMode.None);
        File.CreateSymbolicLink(Path.Combine(root, "link-file"), target);
        Directory.CreateSymbolicLink(Path.Combine(root, "link-dir"), outside);

        NexusMods.Backend.FileExtractor.FileExtractor.MakeOwnerAccessible(root);

        await Assert.That(File.GetUnixFileMode(target)).IsEqualTo(UnixFileMode.None);
        File.SetUnixFileMode(target, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }
}
