using FluentAssertions;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Loadouts;
using Xunit;

namespace NexusMods.DataModel.SchemaVersions.Tests.MigrationSpecificTests;

public class TestsFor_0011_GameIdFromNexusModsGameId(ITestOutputHelper helper) : ALegacyDatabaseTest(helper)
{
    [Theory]
    [InlineData("Migration-8.rocksdb.zip")]
    [InlineData("Issue-2608.rocksdb.zip")]
    public async Task InstallsOfARegisteredGameGetItsGameId(string database)
    {
        await using var tempConnection = await ConnectionFor(database);
        var db = tempConnection.Connection.Db;

        var installs = db.Datoms(GameInstallMetadata.LegacyNexusModsGameId).AsModels<GameInstallMetadata.ReadOnly>(db).ToArray();
        installs.Should().NotBeEmpty();
        foreach (var install in installs)
        {
            GameInstallMetadata.LegacyNexusModsGameId.TryGetValue(install, out var nexusModsGameId).Should().BeTrue();
            if (nexusModsGameId == Cyberpunk2077Game.NexusModsGameId.Value)
                GameInstallMetadata.GameId.TryGetValue(install, out var gameId).Should().BeTrue() ;
            else
                GameInstallMetadata.GameId.Contains(install).Should().BeFalse("only Cyberpunk is registered in these tests");
        }

        foreach (var loadout in Loadout.All(db).Where(loadout => GameInstallMetadata.GameId.Contains(loadout.Installation)))
            loadout.InstallationInstance.Game.GameId.Should().Be(Cyberpunk2077Game.GameId);
    }
}
