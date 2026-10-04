using JetBrains.Annotations;
using NexusMods.Paths;
using NexusMods.Sdk.Settings;

namespace NexusMods.Sdk.Library;

/// <summary>
/// Where original downloads (the archives exactly as Nexus serves them) are kept.
/// </summary>
[PublicAPI]
public record DownloadsSettings : ISettings
{
    public ConfigurablePath Folder { get; init; }

    public static ISettingsBuilder Configure(ISettingsBuilder settingsBuilder) => settingsBuilder
        .ConfigureDefault(CreateDefault)
        .ConfigureBackend(StorageBackendOptions.Use(StorageBackends.Json));

    /// <summary>tModManager's own downloads folder: nothing but downloads lives there.</summary>
    public static ConfigurablePath DefaultFolder => new(KnownPath.XDG_DATA_HOME, $"{ApplicationConstants.DataDirectoryName}/Downloads");

    public static DownloadsSettings CreateDefault(IServiceProvider serviceProvider) => new()
    {
        Folder = DefaultFolder,
    };
}
