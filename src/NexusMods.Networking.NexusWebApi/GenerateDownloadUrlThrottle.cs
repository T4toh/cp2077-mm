namespace NexusMods.Networking.NexusWebApi;

/// <summary>
/// Spaces out calls to the website's GenerateDownloadUrl endpoint and retries Cloudflare's challenge page. Calls made
/// ~100 ms after the previous one get "Just a moment..." (HTML) instead of JSON; calls ≥0.7 s apart almost always pass.
/// </summary>
/// <remarks>Not thread-safe: callers serialize (NexusApiClient holds its curl semaphore).</remarks>
internal sealed class GenerateDownloadUrlThrottle(Func<TimeSpan, CancellationToken, Task> delay, Func<DateTimeOffset> now)
{
    internal static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(1);
    internal static readonly TimeSpan[] RetryDelays = [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8)];

    public static GenerateDownloadUrlThrottle Default { get; } = new(Task.Delay, () => DateTimeOffset.UtcNow);

    private DateTimeOffset _lastCall = DateTimeOffset.MinValue;

    /// <summary>
    /// Runs <paramref name="call"/>, retrying while it returns the challenge page. Returns the last response: JSON,
    /// null (curl failed, not retried) or the challenge page once the retries run out.
    /// </summary>
    public async Task<string?> RunAsync(Func<CancellationToken, Task<string?>> call, CancellationToken cancellationToken)
    {
        string? response = null;
        for (var attempt = 0; attempt <= RetryDelays.Length; attempt++)
        {
            var wait = attempt == 0 ? MinInterval - (now() - _lastCall) : RetryDelays[attempt - 1];
            if (wait > TimeSpan.Zero) await delay(wait, cancellationToken);

            response = await call(cancellationToken);
            _lastCall = now();
            if (!IsChallenge(response)) return response;
        }

        return response;
    }

    internal static bool IsChallenge(string? response) => response is not null && response.TrimStart().StartsWith('<');
}
