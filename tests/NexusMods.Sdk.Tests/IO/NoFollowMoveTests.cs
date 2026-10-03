using NexusMods.Sdk.IO;

namespace NexusMods.Sdk.Tests.IO;

public class NoFollowMoveTests
{
    // /tmp and /dev/shm are separate tmpfs mounts, so a rename between them fails with EXDEV: the same thing
    // Deep Clean hits when the game and tModManager/Backups are on different disks or btrfs subvolumes.
    private const string OtherDevice = "/dev/shm";

    [Test]
    public async Task MoveDirectoryNoFollow_AcrossDevices_CopiesLinksAsLinksAndKeepsTheirTargets()
    {
        var source = Directory.CreateTempSubdirectory("nofollow-move-").FullName;
        var outside = Directory.CreateTempSubdirectory("nofollow-outside-").FullName;
        var destRoot = Path.Combine(OtherDevice, $"nofollow-dest-{Guid.NewGuid():N}");
        var dest = Path.Combine(destRoot, "mods");
        try
        {
            Directory.CreateDirectory(Path.Combine(source, "sub"));
            File.WriteAllText(Path.Combine(source, "sub/mod.archive"), "mod");
            File.WriteAllText(Path.Combine(outside, "precious.txt"), "keep me");
            File.CreateSymbolicLink(Path.Combine(source, "linked-dir"), outside);
            File.CreateSymbolicLink(Path.Combine(source, "linked-file"), Path.Combine(outside, "precious.txt"));
            File.CreateSymbolicLink(Path.Combine(source, "z:"), "/");
            Directory.CreateDirectory(destRoot);

            NoFollowMove.MoveDirectoryNoFollow(source, dest);

            await Assert.That(Directory.Exists(source)).IsFalse();
            await Assert.That(File.ReadAllText(Path.Combine(dest, "sub/mod.archive"))).IsEqualTo("mod");
            await Assert.That(new DirectoryInfo(Path.Combine(dest, "linked-dir")).LinkTarget).IsEqualTo(outside);
            await Assert.That(new FileInfo(Path.Combine(dest, "linked-file")).LinkTarget).IsEqualTo(Path.Combine(outside, "precious.txt"));
            await Assert.That(new DirectoryInfo(Path.Combine(dest, "z:")).LinkTarget).IsEqualTo("/");
            await Assert.That(File.ReadAllText(Path.Combine(outside, "precious.txt"))).IsEqualTo("keep me");
        }
        finally
        {
            if (Directory.Exists(source)) Directory.Delete(source, recursive: true);
            if (Directory.Exists(destRoot)) Directory.Delete(destRoot, recursive: true);
            Directory.Delete(outside, recursive: true);
        }
    }

    [Test]
    public async Task MoveDirectoryNoFollow_DestinationExists_ThrowsAndLeavesSourceAlone()
    {
        var source = Directory.CreateTempSubdirectory("nofollow-move-").FullName;
        var dest = Path.Combine(OtherDevice, $"nofollow-dest-{Guid.NewGuid():N}");
        try
        {
            File.WriteAllText(Path.Combine(source, "mod.archive"), "mod");
            Directory.CreateDirectory(dest);

            await Assert.That(() => NoFollowMove.MoveDirectoryNoFollow(source, dest)).Throws<IOException>();

            await Assert.That(File.ReadAllText(Path.Combine(source, "mod.archive"))).IsEqualTo("mod");
            await Assert.That(Directory.EnumerateFileSystemEntries(dest)).IsEmpty();
        }
        finally
        {
            Directory.Delete(source, recursive: true);
            Directory.Delete(dest, recursive: true);
        }
    }
}
