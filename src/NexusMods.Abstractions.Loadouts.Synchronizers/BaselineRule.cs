using NexusMods.Hashing.xxHash3;
using NexusMods.Paths;
using NexusMods.Sdk.Games;

namespace NexusMods.Abstractions.Loadouts.Synchronizers;

/// <summary>
/// Builds an installation's original file list from the disk when the Nexus hash database doesn't know the version.
/// Errs on the side of "original": a file wrongly kept as original is never deleted, a file wrongly dropped would be.
/// </summary>
public static class BaselineRule
{
    /// <param name="previous">The list before this run (empty the first time).</param>
    /// <param name="disk">The freshly indexed disk state.</param>
    /// <param name="owned">
    /// Paths the loadout owns. The value is the mod file's hash, or null for paths whose previous entry is kept whatever
    /// the disk holds (External Changes, files deleted on purpose, files the app generates).
    /// </param>
    public static Dictionary<GamePath, (Hash Hash, Size Size)> Apply(
        IReadOnlyDictionary<GamePath, (Hash Hash, Size Size)> previous,
        IEnumerable<(GamePath Path, Hash Hash, Size Size)> disk,
        IReadOnlyDictionary<GamePath, Hash?> owned)
    {
        var result = new Dictionary<GamePath, (Hash Hash, Size Size)>();

        foreach (var (path, hash, size) in disk)
        {
            // What's on disk is the loadout's, not the game's: the original (if known) is the previous entry
            if (owned.TryGetValue(path, out var ownedHash) && (ownedHash is null || ownedHash == hash))
            {
                if (previous.TryGetValue(path, out var kept)) result[path] = kept;
                continue;
            }

            result[path] = (hash, size);
        }

        // Owned paths missing from disk (mod not deployed yet, original deleted on purpose) keep their original
        foreach (var (path, entry) in previous)
        {
            if (!result.ContainsKey(path) && owned.ContainsKey(path))
                result[path] = entry;
        }

        return result;
    }
}
