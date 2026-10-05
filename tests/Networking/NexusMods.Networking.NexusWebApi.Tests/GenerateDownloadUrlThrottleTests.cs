using FluentAssertions;

namespace NexusMods.Networking.NexusWebApi.Tests;

public class GenerateDownloadUrlThrottleTests
{
    private const string Json = """{"url":"https://files.nexus-cdn.com/x.zip"}""";
    private const string Challenge = "<!DOCTYPE html><html><head><title>Just a moment...</title>";

    private sealed class FakeClock
    {
        public DateTimeOffset Now = new(2026, 10, 4, 0, 0, 0, TimeSpan.Zero);
        public readonly List<TimeSpan> Waits = [];

        public GenerateDownloadUrlThrottle Throttle() => new((wait, _) =>
        {
            Waits.Add(wait);
            Now += wait;
            return Task.CompletedTask;
        }, () => Now);
    }

    private static Func<CancellationToken, Task<string?>> Responses(params string?[] responses)
    {
        var queue = new Queue<string?>(responses);
        return _ => Task.FromResult(queue.Dequeue());
    }

    [Fact]
    public async Task BackToBackCalls_AreSpacedOneSecondApart()
    {
        var clock = new FakeClock();
        var throttle = clock.Throttle();

        await throttle.RunAsync(Responses(Json), CancellationToken.None);
        clock.Now += TimeSpan.FromMilliseconds(100);
        await throttle.RunAsync(Responses(Json), CancellationToken.None);

        clock.Waits.Should().Equal(TimeSpan.FromMilliseconds(900));
    }

    [Fact]
    public async Task ChallengePage_IsRetriedWithGrowingWaits()
    {
        var clock = new FakeClock();

        var result = await clock.Throttle().RunAsync(Responses(Challenge, Challenge, Json), CancellationToken.None);

        result.Should().Be(Json);
        clock.Waits.Should().Equal(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4));
    }

    [Fact]
    public async Task ChallengeEveryTime_GivesUpAfterThreeRetriesAndReturnsThePage()
    {
        var clock = new FakeClock();

        var result = await clock.Throttle().RunAsync(Responses(Challenge, Challenge, Challenge, Challenge), CancellationToken.None);

        result.Should().Be(Challenge);
        clock.Waits.Should().Equal(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8));
    }

    [Fact]
    public async Task CurlFailure_IsNotRetried()
    {
        var clock = new FakeClock();

        var result = await clock.Throttle().RunAsync(Responses((string?)null), CancellationToken.None);

        result.Should().BeNull();
        clock.Waits.Should().BeEmpty();
    }
}
