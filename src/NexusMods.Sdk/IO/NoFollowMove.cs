namespace NexusMods.Sdk.IO;

/// <summary>
/// Directory move that works across filesystems and never follows symbolic links.
/// </summary>
public static class NoFollowMove
{
    /// <summary>
    /// Moves <paramref name="from"/> to <paramref name="to"/> (which must not exist). When a plain rename fails
    /// because they are on different filesystems or btrfs subvolumes, copies the tree (symlinks recreated as links,
    /// never entered) and then deletes the source; like <see cref="NoFollowDelete.DeleteDirectoryNoFollow"/>, the
    /// recursive delete removes links as links.
    /// If the copy fails, the partial copy is removed and the source is left untouched.
    /// </summary>
    public static void MoveDirectoryNoFollow(string from, string to)
    {
        try
        {
            Directory.Move(from, to);
            return;
        }
        // .NET reports EXDEV as a plain IOException; whatever else failed, a copy into an existing destination
        // would merge into it, so only fall back when the destination is still free
        catch (IOException) when (!Path.Exists(to) && Directory.Exists(from))
        {
        }

        try
        {
            CopyTree(new DirectoryInfo(from), to);
        }
        catch
        {
            if (Directory.Exists(to)) Directory.Delete(to, recursive: true);
            throw;
        }

        Directory.Delete(from, recursive: true);
    }

    private static void CopyTree(DirectoryInfo source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var entry in source.EnumerateFileSystemInfos("*", new EnumerationOptions { AttributesToSkip = 0 }))
        {
            var target = Path.Combine(dest, entry.Name);
            if (entry.LinkTarget is { } linkTarget)
                File.CreateSymbolicLink(target, linkTarget);
            else if (entry is DirectoryInfo dir)
                CopyTree(dir, target);
            else
                File.Copy(entry.FullName, target);
        }
    }
}
