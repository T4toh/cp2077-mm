using System.Collections.Immutable;
using NexusMods.Paths;
using NexusMods.Sdk.Games;

namespace NexusMods.Sdk.Tests;

public class GameLocationsTests
{
    private static readonly GameLocations Locations = GameLocations.Create(ImmutableDictionary<LocationId, AbsolutePath>.Empty
        .Add(LocationId.Game, FileSystem.Shared.FromUnsanitizedFullPath("/games/Cyberpunk 2077")));

    [Test]
    public async Task ToAbsolutePath_InsideTheGame_Resolves()
    {
        var path = Locations.ToAbsolutePath(new GamePath(LocationId.Game, "r6/scripts/mod.reds"));
        await Assert.That(path.ToString()).IsEqualTo("/games/Cyberpunk 2077/r6/scripts/mod.reds");
    }

    [Test]
    [Arguments("../../.bashrc")]
    [Arguments("r6/../../outside.txt")]
    [Arguments("..\\..\\.bashrc")]
    public async Task ToAbsolutePath_WithParentSegment_Throws(string relative)
    {
        // FOMOD destinations and collection.json paths reach here unchanged
        await Assert.That(() => Locations.ToAbsolutePath(new GamePath(LocationId.Game, relative))).Throws<InvalidOperationException>();
    }

    private static readonly AbsolutePath Prefix = FileSystem.Shared.FromUnsanitizedFullPath("/steam/compatdata/1091500/pfx");
    private static readonly RelativePath Settings = (RelativePath)"drive_c/users/steamuser/AppData/Local/CD Projekt Red/Cyberpunk 2077/UserSettings.json";

    private static GameLocations WithPrefix(AbsolutePath gamePath) => GameLocations.Create(
        ImmutableDictionary<LocationId, AbsolutePath>.Empty
            .Add(LocationId.Game, gamePath)
            .Add(LocationId.WinePrefix, Prefix),
        ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>.Empty
            .Add(LocationId.WinePrefix, [Settings]));

    [Test]
    public async Task IsManaged_WholeLocationWithoutWhitelist_IsTrue()
    {
        await Assert.That(Locations.IsManaged(new GamePath(LocationId.Game, "r6/scripts/mod.reds"))).IsTrue();
    }

    [Test]
    public async Task IsManaged_WhitelistedLocation_OnlyListedFiles()
    {
        var locations = WithPrefix(FileSystem.Shared.FromUnsanitizedFullPath("/games/Cyberpunk 2077"));
        await Assert.That(locations.IsManaged(new GamePath(LocationId.WinePrefix, Settings))).IsTrue();
        await Assert.That(locations.IsManaged(new GamePath(LocationId.WinePrefix, "drive_c/users/steamuser/Desktop/x.txt"))).IsFalse();
        await Assert.That(locations[LocationId.WinePrefix].ManagedFiles).IsNotNull();
        await Assert.That(locations[LocationId.Game].ManagedFiles).IsNull();
    }

    [Test]
    public async Task GameInsideAWhitelistedLocation_StaysTopLevel()
    {
        // Lutris installs the game inside the prefix: the prefix must not swallow the game folder, or it would never be scanned
        var locations = WithPrefix(Prefix.Combine("drive_c/Games/Cyberpunk 2077"));
        await Assert.That(locations[LocationId.Game].IsTopLevel).IsTrue();
        await Assert.That(locations[LocationId.WinePrefix].NestedLocations).IsEmpty();
        var topLevel = locations.GetTopLevelLocations().Select(kv => kv.Key).ToArray();
        await Assert.That(topLevel).Contains(LocationId.Game);
        await Assert.That(topLevel).Contains(LocationId.WinePrefix);
        // A game file still maps to Game, not to the prefix
        var mapped = locations.ToGamePath(Prefix.Combine("drive_c/Games/Cyberpunk 2077/bin/x64/Cyberpunk2077.exe"));
        await Assert.That(mapped.LocationId).IsEqualTo(LocationId.Game);
    }
}
