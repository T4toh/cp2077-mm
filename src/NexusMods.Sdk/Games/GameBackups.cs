using NexusMods.Paths;

namespace NexusMods.Sdk.Games;

/// <summary>
/// Where Deep Clean snapshots live: <c>Backups/&lt;GameId&gt;/&lt;yyyyMMdd_HHmmss&gt;</c>. One folder per game, so pruning
/// one game's snapshots never deletes another game's only backup of its mods.
/// </summary>
public static class GameBackups
{
    public const string SnapshotNameFormat = "yyyyMMdd_HHmmss";

    public static AbsolutePath Root(IFileSystem fs) =>
        fs.GetKnownPath(KnownPath.XDG_DATA_HOME).Combine(ApplicationConstants.DataDirectoryName).Combine("Backups");

    public static AbsolutePath ForGame(IFileSystem fs, GameId game) => Root(fs).Combine(game.ToString());

    public static bool IsSnapshotName(string name) =>
        DateTime.TryParseExact(name, SnapshotNameFormat, null, System.Globalization.DateTimeStyles.None, out _);
}
