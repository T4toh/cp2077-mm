using NexusMods.Sdk;
using NexusMods.Sdk.Games;
using System.Collections.Immutable;
using NexusMods.Paths;
using Microsoft.Extensions.Logging;

namespace NexusMods.Backend;

public class ManualLinuxCompatabilityDataProvider : ILinuxCompatabilityDataProvider
{
    private readonly AbsolutePath _winePrefix;
    private readonly ILogger _logger;

    public AbsolutePath WinePrefixDirectoryPath => _winePrefix;

    public ManualLinuxCompatabilityDataProvider(AbsolutePath winePrefix, ILogger logger)
    {
        _winePrefix = winePrefix;
        _logger = logger;
    }

    public async ValueTask<ImmutableHashSet<string>> GetInstalledWinetricksComponents(CancellationToken cancellationToken = default)
    {
        var winetricksLog = _winePrefix.Combine("winetricks.log");
        if (!winetricksLog.FileExists)
        {
            _logger.LogWarning("Winetricks log not found at {Path}", winetricksLog);
            return ImmutableHashSet<string>.Empty;
        }

        try
        {
            var lines = await File.ReadAllLinesAsync(winetricksLog.ToString(), cancellationToken);
            return lines.Select(l => l.Trim()).ToImmutableHashSet();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read winetricks log at {Path}", winetricksLog);
            return ImmutableHashSet<string>.Empty;
        }
    }

    public async ValueTask<ImmutableArray<WineDllOverride>> GetWineDllOverrides(CancellationToken cancellationToken = default)
    {
        var userReg = _winePrefix.Combine("user.reg");
        if (!userReg.FileExists)
        {
            _logger.LogWarning("WINE registry (user.reg) not found at {Path}", userReg);
            return ImmutableArray<WineDllOverride>.Empty;
        }

        try
        {
            var content = await File.ReadAllTextAsync(userReg.ToString(), cancellationToken);
            return WineParser.ParseDllOverridesFromRegistry(content).ToImmutableArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse WINE registry at {Path}", userReg);
            return ImmutableArray<WineDllOverride>.Empty;
        }
    }
}
