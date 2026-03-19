using System.Collections.Immutable;
using JetBrains.Annotations;

namespace NexusMods.Sdk.Games;

/// <summary>
/// Record containing all store identifiers for a game.
/// </summary>
[PublicAPI]
public record StoreIdentifiers(GameId GameId)
{
    /// <summary>
    /// All Steam App IDs for the game.
    /// </summary>
    /// <remarks>
    /// Use https://steamdb.info/ to get the IDs. Look up a game, something like https://steamdb.info/app/489830/
    /// and you'll find the App ID in the table at the top as well as in the URL.
    /// </remarks>
    public ImmutableArray<uint> SteamAppIds { get; init; } = ImmutableArray<uint>.Empty;
}
