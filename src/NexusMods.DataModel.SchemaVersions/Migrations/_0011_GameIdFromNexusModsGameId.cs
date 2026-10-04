using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.NexusModsApi;

namespace NexusMods.DataModel.SchemaVersions.Migrations;

/// <summary>
/// Game installs used to be identified by the game's Nexus Mods id, which a game without a Nexus page doesn't have.
/// Fills the new <see cref="GameId"/> attributes of <see cref="GameInstallMetadata"/> and <see cref="ManuallyAddedGame"/>.
/// An install of any other game (e.g. from upstream's multi-game days) is left as it was: no locator found it before
/// this migration either.
/// </summary>
internal class _0011_GameIdFromNexusModsGameId : ITransactionalMigration
{
    public static (MigrationId Id, string Name) IdAndName => MigrationId.ParseNameAndId(nameof(_0011_GameIdFromNexusModsGameId));

    // History, not configuration: until this migration Cyberpunk 2077 was the only game this app registered. Asking
    // DI for the games instead would build them, and their constructors need services a migration can't count on.
    private static readonly Dictionary<NexusModsGameId, GameId> GameIds = new()
    {
        [NexusModsGameId.From(3333)] = GameId.From("RedEngine.Cyberpunk2077"),
    };

    private readonly ILogger _logger;

    public _0011_GameIdFromNexusModsGameId(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<_0011_GameIdFromNexusModsGameId>>();
    }

    public Task Prepare(IDb db) => Task.CompletedTask;

    public void Migrate(ITransaction tx, IDb db)
    {
        foreach (var install in db.Datoms(GameInstallMetadata.LegacyNexusModsGameId).AsModels<GameInstallMetadata.ReadOnly>(db))
        {
            if (GameInstallMetadata.GameId.Contains(install) || !GameInstallMetadata.LegacyNexusModsGameId.TryGetValue(install, out var nexusModsGameId)) continue;
            if (TryMap(install.Id, nexusModsGameId, out var gameId)) tx.Add(install.Id, GameInstallMetadata.GameId, gameId);
        }

        foreach (var added in db.Datoms(ManuallyAddedGame.LegacyNexusModsGameId).AsModels<ManuallyAddedGame.ReadOnly>(db))
        {
            if (ManuallyAddedGame.GameId.Contains(added) || !ManuallyAddedGame.LegacyNexusModsGameId.TryGetValue(added, out var nexusModsGameId)) continue;
            if (TryMap(added.Id, nexusModsGameId, out var gameId)) tx.Add(added.Id, ManuallyAddedGame.GameId, gameId);
        }
    }

    private bool TryMap(EntityId entity, NexusModsGameId nexusModsGameId, out GameId gameId)
    {
        if (GameIds.TryGetValue(nexusModsGameId, out gameId)) return true;
        _logger.LogWarning("Entity {Entity}: no known game has Nexus Mods id {NexusModsGameId}, left without a GameId", entity, nexusModsGameId);
        return false;
    }
}
