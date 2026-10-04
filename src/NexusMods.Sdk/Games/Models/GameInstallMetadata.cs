using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.BuiltInEntities;
using NexusMods.MnemonicDB.Abstractions.Models;
using NexusMods.Sdk.Loadouts;
using NexusMods.Sdk.NexusModsApi;

namespace NexusMods.Sdk.Games;

/// <summary>
/// Metadata about a game installation. This model exists so that parts of the system can reference a single
/// MnemonicDb model, instead of a tuple of domain/store/path
/// </summary>
public partial class GameInstallMetadata : IModelDefinition
{
    private const string Namespace = "NexusMods.Loadouts.GameMetadata";

    /// <summary>
    /// The game's unique id. Stored as "Game": "GameId" is the attribute it replaced.
    /// </summary>
    public static readonly GameIdAttribute GameId = new(Namespace, "Game") { IsIndexed = true };

    /// <summary>
    /// What identified the game before <see cref="GameId"/>: its id on Nexus Mods. Only kept so the migration that
    /// fills <see cref="GameId"/> can read older databases; nothing else reads or writes it.
    /// </summary>
    public static readonly NexusModsGameIdAttribute LegacyNexusModsGameId = new(Namespace, "GameId") { IsIndexed = true, IsOptional = true };

    /// <summary>
    /// The name of the store the game is from
    /// </summary>
    public static readonly GameStoreAttribute Store = new(Namespace, nameof(Store));

    /// <summary>
    /// The path to the game's installation directory.
    /// </summary>
    public static readonly StringAttribute Path = new(Namespace, nameof(Path)) { IsIndexed = true };

    /// <summary>
    /// User friendly name for the game.
    /// May be referred to from diagnostics or otherwise.
    /// </summary>
    public static readonly StringAttribute Name = new(Namespace, nameof(Name));

    /// <summary>
    /// The last applied loadout to this game state
    /// </summary>
    public static readonly ReferenceAttribute<Loadout> LastSyncedLoadout = new(Namespace, nameof(LastSyncedLoadout)) { IsOptional = true };

    /// <summary>
    /// The 'AsOf' transaction ID of the last applied loadout
    /// </summary>
    public static readonly ReferenceAttribute<Transaction> LastSyncedLoadoutTransaction = new(Namespace, nameof(LastSyncedLoadoutTransaction)) { IsOptional = true };

    /// <summary>
    /// The 'AsOf' transaction ID of the initial disk state when the game folder was first indexed
    /// </summary>
    public static readonly ReferenceAttribute<Transaction> InitialDiskStateTransaction = new(Namespace, nameof(InitialDiskStateTransaction)) { IsOptional = true };

    /// <summary>
    /// The last scanned disk state transaction ID
    /// </summary>
    public static readonly ReferenceAttribute<Transaction> LastScannedDiskStateTransaction = new(Namespace, nameof(LastScannedDiskStateTransaction)) { IsOptional = true };
}
