using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.Models;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Hashes;

namespace NexusMods.Abstractions.Loadouts;

/// <summary>
/// One original file of a game installation: what the synchronizer keeps (layer 0), the reset restores and Deep Clean
/// never touches. Filled from the Nexus hash database when it knows the installed version, otherwise from the disk
/// (see <see cref="GameInstallMetadata.BaselineFromDisk"/>), so no part of the app needs Nexus to know the game.
/// </summary>
public partial class GameBaselineFile : IModelDefinition
{
    private const string Namespace = "NexusMods.Loadouts.GameBaselineFile";

    /// <summary>The installation this file belongs to.</summary>
    public static readonly ReferenceAttribute<GameInstallMetadata> Game = new(Namespace, nameof(Game));

    /// <summary>Where the file lives.</summary>
    public static readonly GamePathParentAttribute Path = new(Namespace, nameof(Path));

    /// <summary>The original content's hash.</summary>
    public static readonly HashAttribute Hash = new(Namespace, nameof(Hash));

    /// <summary>The original content's size.</summary>
    public static readonly SizeAttribute Size = new(Namespace, nameof(Size));

    /// <summary>
    /// The installation's original files, or false when the list hasn't been built yet (never synchronized since this
    /// existed). Without a list every game file looks like a leftover, so callers must not delete anything.
    /// </summary>
    public static bool TryGetVanillaFiles(GameInstallMetadata.ReadOnly metadata, out IReadOnlyList<ReadOnly> files)
    {
        if (!metadata.Contains(GameInstallMetadata.BaselineFromDisk))
        {
            files = [];
            return false;
        }

        files = FindByGame(metadata.Db, metadata).ToArray();
        return true;
    }
}
