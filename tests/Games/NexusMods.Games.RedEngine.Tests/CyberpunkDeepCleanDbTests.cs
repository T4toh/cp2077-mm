using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Games.TestFramework;

namespace NexusMods.Games.RedEngine.Tests;

public class CyberpunkDeepCleanDbTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<CyberpunkDeepCleanDbTests>(helper)
{
    [Fact]
    public async Task RemoveModGroups_EmptiesMyModsInsteadOfDeletingIt()
    {
        var loadout = await CreateLoadout();
        var myMods = LoadoutItemGroupId.From(loadout.MutableCollections().Single().CollectionId);

        using (var tx = Connection.BeginTransaction())
        {
            AddEmptyGroup(tx, loadout.LoadoutId, "mod in My Mods", myMods);
            AddEmptyGroup(tx, loadout.LoadoutId, "top-level mod");
            _ = new CollectionGroup.New(tx, out var nexusId)
            {
                IsReadOnly = true,
                LoadoutItemGroup = new LoadoutItemGroup.New(tx, nexusId)
                {
                    IsGroup = true,
                    LoadoutItem = new LoadoutItem.New(tx, nexusId) { Name = "Nexus collection", LoadoutId = loadout.LoadoutId },
                },
            };
            AddEmptyGroup(tx, loadout.LoadoutId, "mod in Nexus collection", LoadoutItemGroupId.From(nexusId));
            await tx.Commit();
        }

        await CyberpunkDeepCleanTool.RemoveModGroups(Connection, loadout.LoadoutId, NullLogger.Instance);

        loadout = Refresh(loadout);
        loadout.MutableCollections().Select(c => c.CollectionId).Should().Equal(myMods.Value);
        var remaining = LoadoutItem.FindByLoadout(Connection.Db, loadout.LoadoutId)
            .OfTypeLoadoutItemGroup()
            .Where(g => !new[] { g }.OfTypeLoadoutOverridesGroup().Any())
            .Select(g => g.AsLoadoutItem().Name);
        remaining.Should().Equal("My Mods");
    }
}
