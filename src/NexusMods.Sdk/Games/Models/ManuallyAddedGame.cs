using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.Models;
using NexusMods.Sdk.NexusModsApi;

namespace NexusMods.Sdk.Games;

/// <summary>
/// Used to store information about manually added games.
/// Upstream marked this obsolete; this fork relies on it for the manual game locator.
/// </summary>
public partial class ManuallyAddedGame : IModelDefinition
{
    private const string Namespace = "NexusMods.StandardGameLocators.ManuallyAddedGame";

    /// <summary>
    /// The game this install belongs to. Stored as "Game": "GameId" is the attribute it replaced.
    /// </summary>
    public static readonly GameIdAttribute GameId = new(Namespace, "Game") { IsIndexed = true };

    /// <summary>
    /// What identified the game before <see cref="GameId"/>: its id on Nexus Mods. Only kept so the migration that
    /// fills <see cref="GameId"/> can read older databases; nothing else reads or writes it.
    /// </summary>
    public static readonly NexusModsGameIdAttribute LegacyNexusModsGameId = new(Namespace, "GameId") { IsIndexed = true, IsOptional = true };

    /// <summary>
    /// The version of the game.
    /// </summary>
    public static readonly StringAttribute Version = new(Namespace, nameof(Version));

    /// <summary>
    /// The path to the game install.
    /// </summary>
    public static readonly StringAttribute Path = new(Namespace, nameof(Path)) { IsIndexed = true };

    /// <summary>
    /// The path to the WINE prefix.
    /// </summary>
    public static readonly StringAttribute WinePrefix = new(Namespace, nameof(WinePrefix));
}
