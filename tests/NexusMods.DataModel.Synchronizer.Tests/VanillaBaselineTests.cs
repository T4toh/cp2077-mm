using FluentAssertions;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.TestFramework;
using NexusMods.Hashing.xxHash3;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

/// <summary>
/// The vanilla file list of an installation (<see cref="GameBaselineFile"/>): built from the Nexus hash database when
/// it knows the version, from the disk when it doesn't, and the only thing the synchronizer treats as original.
/// </summary>
public class VanillaBaselineTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<VanillaBaselineTests>(helper)
{
    [Fact]
    public async Task TryGetVanillaFiles_IsFalseUntilTheListIsBuilt()
    {
        await LoadoutManager.ManageInstallation(GameInstallation);
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        GameBaselineFile.TryGetVanillaFiles(metadata, out _).Should().BeFalse();

        var path = new GamePath(LocationId.Game, "bin/x64/original.exe");
        using (var tx = Connection.BeginTransaction())
        {
            _ = new GameBaselineFile.New(tx)
            {
                Path = path.ToGamePathParentTuple(metadata.Id),
                Hash = "original".xxHash3AsUtf8(),
                Size = Size.FromLong(8),
                GameId = metadata.Id,
            };
            tx.Add(metadata.Id, GameInstallMetadata.BaselineFromDisk, true);
            await tx.Commit();
        }

        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files).Should().BeTrue();
        files.Select(f => (GamePath)f.Path).Should().Equal(path);
    }
}
