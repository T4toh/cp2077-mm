using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Games;
using NexusMods.Games.RedEngine.Cyberpunk2077.SortOrder;
using NexusMods.Games.TestFramework;

namespace NexusMods.Games.RedEngine.Tests;

public class SortOrderManagerPerGameTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<SortOrderManagerPerGameTests>(helper)
{
    [Fact]
    public void AnotherGameRegisteringVarietiesKeepsRedModOrder()
    {
        var cyberpunk = InitAndGetSortOrderManager();

        // What a second game wired like Cyberpunk2077Game does: take a SortOrderManager and register its own varieties.
        var other = ServiceProvider.GetService<SortOrderManager>() ?? new SortOrderManager(ServiceProvider);
        other.RegisterSortOrderVarieties([], Game);

        cyberpunk.GetSortOrderVarieties().Should().ContainSingle(variety => variety is RedModSortOrderVariety);
    }
}
