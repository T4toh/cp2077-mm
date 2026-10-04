using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Library;
using NexusMods.DataModel.Storage;
using NexusMods.Games.TestFramework;
using NexusMods.Paths;
using NexusMods.Sdk.IO;
using NexusMods.Sdk.Library;
using NexusMods.Sdk.Settings;
using Xunit;

namespace NexusMods.DataModel.Tests;

public class StorageAnalyzerTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<StorageAnalyzerTests>(helper)
{
    [Fact]
    public async Task DeleteDownloadsAsync_DeletesOnlyDownloadsTheLibraryRecorded()
    {
        // The downloads folder is configurable: pointed at ~/Downloads it holds the user's own files too
        var storageAnalyzer = ServiceProvider.GetRequiredService<IStorageAnalyzer>();
        var libraryService = ServiceProvider.GetRequiredService<ILibraryService>();
        var downloads = ServiceProvider.GetRequiredService<ISettingsManager>().Get<DownloadsSettings>().Folder.ToPath(FileSystem);
        downloads.CreateDirectory();

        var recorded = downloads.Combine("mod.txt");
        await File.WriteAllTextAsync(recorded.ToString(), "mod contents");
        await libraryService.AddLocalFile(recorded);

        var usersOwn = downloads.Combine("tax-return.pdf");
        await File.WriteAllTextAsync(usersOwn.ToString(), "not a mod");
        var nestedFile = downloads.Combine("subdir/nested.txt");
        nestedFile.Parent.CreateDirectory();
        await File.WriteAllTextAsync(nestedFile.ToString(), "should survive");

        await storageAnalyzer.DeleteDownloadsAsync();

        recorded.FileExists.Should().BeFalse();
        usersOwn.FileExists.Should().BeTrue();
        nestedFile.FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDownloadsAsync_InOwnFolder_AlsoDeletesUnrecordedDownloads()
    {
        // After a database reset the old downloads are no longer recorded but still fill tModManager's folder
        var analyzer = (StorageAnalyzer)ServiceProvider.GetRequiredService<IStorageAnalyzer>();
        var downloads = ServiceProvider.GetRequiredService<ISettingsManager>().Get<DownloadsSettings>().Folder.ToPath(FileSystem);
        downloads.CreateDirectory();
        analyzer.DefaultDownloadsFolderProvider = () => downloads;

        var unrecorded = downloads.Combine("Running Man.zip");
        await File.WriteAllTextAsync(unrecorded.ToString(), "old download");
        var partial = downloads.Combine($"mod.zip.tmp-{Guid.NewGuid():N}");
        await File.WriteAllTextAsync(partial.ToString(), "half written");
        var outside = TemporaryFileManager.CreateFolder().Path.Combine("canary.txt");
        await File.WriteAllTextAsync(outside.ToString(), "must survive");
        var link = downloads.Combine("linked.zip");
        File.CreateSymbolicLink(link.ToString(), outside.ToString());

        await analyzer.DeleteDownloadsAsync();

        unrecorded.FileExists.Should().BeFalse();
        File.Exists(link.ToString()).Should().BeFalse();
        partial.FileExists.Should().BeTrue();
        outside.FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDownloadsAsync_OwnFolderLinkedElsewhere_DeletesOnlyRecorded()
    {
        // The default folder made a symlink to ~/Downloads must not empty ~/Downloads
        var analyzer = (StorageAnalyzer)ServiceProvider.GetRequiredService<IStorageAnalyzer>();
        var downloads = ServiceProvider.GetRequiredService<ISettingsManager>().Get<DownloadsSettings>().Folder.ToPath(FileSystem);
        var usersFolder = TemporaryFileManager.CreateFolder().Path;
        if (downloads.DirectoryExists()) downloads.DeleteDirectoryNoFollow();
        downloads.Parent.CreateDirectory();
        Directory.CreateSymbolicLink(downloads.ToString(), usersFolder.ToString());
        analyzer.DefaultDownloadsFolderProvider = () => downloads;

        var usersOwn = usersFolder.Combine("tax-return.pdf");
        await File.WriteAllTextAsync(usersOwn.ToString(), "not a mod");

        await analyzer.DeleteDownloadsAsync();

        usersOwn.FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDownloadsAsync_SkipsInProgressPartials()
    {
        var storageAnalyzer = ServiceProvider.GetRequiredService<IStorageAnalyzer>();
        var downloads = ServiceProvider.GetRequiredService<ISettingsManager>().Get<DownloadsSettings>().Folder.ToPath(FileSystem);
        downloads.CreateDirectory();
        var partial = downloads.Combine($"mod.zip.tmp-{Guid.NewGuid():N}");
        await File.WriteAllTextAsync(partial.ToString(), "half written");

        await storageAnalyzer.DeleteDownloadsAsync();

        partial.FileExists.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeletePhysicalFilesAsync_KeepNewest_KeepsEachGamesLatestSnapshot(bool keepNewest)
    {
        // A Super Clean keeps the snapshot it just made; another game's only backup must survive it too.
        var analyzer = (StorageAnalyzer)ServiceProvider.GetRequiredService<IStorageAnalyzer>();
        var backups = TemporaryFileManager.CreateFolder().Path;
        analyzer.BackupsFolderProvider = () => backups;
        string[] snapshots = ["GameA/20260101_000000", "GameA/20260103_000000", "GameA/20260102_000000", "GameB/20250101_000000"];
        foreach (var snapshot in snapshots)
        {
            backups.Combine(snapshot).CreateDirectory();
            await File.WriteAllTextAsync(backups.Combine(snapshot).Combine("mod.archive").ToString(), snapshot);
        }

        await analyzer.DeletePhysicalFilesAsync(keepNewest);

        snapshots.Where(snapshot => backups.Combine(snapshot).DirectoryExists())
            .Should().BeEquivalentTo(keepNewest ? ["GameA/20260103_000000", "GameB/20250101_000000"] : Array.Empty<string>());
        if (keepNewest) backups.Combine("GameA/20260103_000000/mod.archive").FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePhysicalFilesAsync_DoesNotFollowSymlinksOutOfTheBackup()
    {
        // Deep Clean moves mod folders as-is, so a backup can hold a symlink to a folder elsewhere.
        var analyzer = (StorageAnalyzer)ServiceProvider.GetRequiredService<IStorageAnalyzer>();
        var backups = TemporaryFileManager.CreateFolder().Path;
        var outside = TemporaryFileManager.CreateFolder().Path;
        var canary = outside.Combine("sub/canary.txt");
        canary.Parent.CreateDirectory();
        await File.WriteAllTextAsync(canary.ToString(), "must survive");
        analyzer.BackupsFolderProvider = () => backups;
        backups.Combine("GameA/20260101_000000").CreateDirectory();
        File.CreateSymbolicLink(backups.Combine("GameA/20260101_000000/linked-mod").ToString(), outside.ToString());
        // A linked game folder: its "snapshots" are the outside folder's own subfolders
        File.CreateSymbolicLink(backups.Combine("GameB").ToString(), outside.ToString());

        var stats = await analyzer.GetStorageStatsAsync();
        await analyzer.DeletePhysicalFilesAsync();

        backups.Combine("GameA/20260101_000000").DirectoryExists().Should().BeFalse();
        canary.FileExists.Should().BeTrue();
        stats.CyberpunkBackupsSize.Value.Should().Be(0, "the size walk must not count files behind a symlink");
    }

    [Fact]
    public async Task RunDeepCleanOnAllLoadouts_WithSeveralLoadouts_RefusesBeforeTouchingAnything()
    {
        // Never run this without the guard: Deep Clean writes its backups to the real XDG data folder
        var analyzer = ServiceProvider.GetRequiredService<IStorageAnalyzer>();
        await CreateLoadout();
        await CreateLoadout();

        var act = () => analyzer.RunDeepCleanOnAllLoadoutsAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*loadouts*");
    }
}
