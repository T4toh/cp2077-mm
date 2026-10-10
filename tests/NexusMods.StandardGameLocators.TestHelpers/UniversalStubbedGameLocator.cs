using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Games;
using NexusMods.Paths;
using NexusMods.Sdk.Games;

namespace NexusMods.StandardGameLocators.TestHelpers;

public class UniversalStubbedGameLocator<TGame> : IGameLocator, IDisposable
    where TGame : IGame
{
    private readonly TemporaryPath _path;
    private readonly TemporaryPath? _prefix;
    private readonly TGame _game;
    private readonly GameStore[] _stores;

    public LocatorId[] LocatorIds { get; set; } = [LocatorId.From("StubbedGameState.zip")];

    /// <summary>The stub Wine prefix (a sibling temp folder ending in <c>/pfx</c>, never inside the game folder), or null when the locator was created without one.</summary>
    public AbsolutePath? WinePrefix { get; }

    public UniversalStubbedGameLocator(
        IServiceProvider serviceProvider,
        IFileSystem fileSystem,
        TemporaryFileManager fileManager,
        Dictionary<RelativePath, byte[]>? gameFiles = null,
        GameStore[]? stores = null,
        bool withWinePrefix = false)
    {
        _stores = stores ?? [GameStore.Unknown];
        _path = fileManager.CreateFolder(typeof(TGame).Name);
        _game = serviceProvider.GetRequiredService<TGame>();

        if (withWinePrefix)
        {
            // A real prefix lives under steamapps/compatdata, never inside the game folder
            _prefix = fileManager.CreateFolder($"{typeof(TGame).Name}-compatdata");
            WinePrefix = _prefix.Value.Path.Combine("pfx");
            WinePrefix.Value.CreateDirectory();
        }

        if (gameFiles is null) return;
        foreach (var gameFile in gameFiles)
        {
            var gameFilePath = _path.Path.Combine(gameFile.Key);
            gameFilePath.Parent.CreateDirectory();
            using var stream = fileSystem.CreateFile(gameFilePath);
            stream.Write(gameFile.Value, 0, gameFile.Value.Length);
        }
    }

    public IEnumerable<GameLocatorResult> Locate()
    {
        foreach (var store in _stores)
        {
            yield return new GameLocatorResult
            {
                Game = _game,
                Locator = this,
                LocatorIds = [..LocatorIds],
                Store = store,
                Path = _path,
                StoreIdentifier = LocatorIds[0].Value,
                LinuxCompatabilityDataProvider = WinePrefix is { } prefix ? new StubbedLinuxCompatabilityDataProvider(prefix) : null,
            };
        }
    }

    public void Dispose()
    {
        _path.Dispose();
        _prefix?.Dispose();
    }
}
