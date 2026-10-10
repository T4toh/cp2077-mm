# Wine Prefix Location (pieza 2) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let a game declare files inside its Wine/Proton prefix as a location with a whitelist, so the app indexes, backs up, writes and deletes only those files and never anything else in the prefix. CP2077 declares `UserSettings.json`.

**Architecture:** A new `LocationId.WinePrefix` points at the prefix root (`.../compatdata/1091500/pfx`). `IGameData.GetManagedFiles` (default empty) gives a per-location whitelist that `GameLocations` carries in `GameLocationDescriptor.ManagedFiles`. The core enforces it in three places: the scan (`GameLocationsService.IndexGame`) stats only the whitelisted files instead of enumerating; `ALoadoutSynchronizer.EnsureDiskChangesStayInside` throws before any write or delete outside the whitelist or through a symlink; `CleanDirectories` skips whitelisted locations. `UpdateBaseline` always takes non-`Game` locations from the disk, since the Nexus hash DB only describes the game folder.

**Tech Stack:** C#/.NET 10, NexusMods.Paths (`AbsolutePath`, `RelativePath`, `GamePath`), MnemonicDB, xUnit v3 (`dotnet test`) for the Synchronizer tests, TUnit (`dotnet run`) for `NexusMods.Sdk.Tests`, FluentAssertions.

**Spec:** `docs/superpowers/specs/2026-10-09-wine-prefix-location-design.md`

## Global Constraints

- Branch `feat/wine-prefix-location` (already created from `main`, the spec is its first commit).
- dotnet lives in `~/.dotnet`; export `DOTNET_ROLL_FORWARD=Major` before any `dotnet` command.
- Build gate: `dotnet build -p:TreatWarningsAsErrors=true` must stay at 0 compiler warnings (only `NU19xx` NuGet audit warnings are tolerated). `.globalconfig` analyzer errors (`CS4014`, `CS8509`, `CA1069`, `CA2211`, `CA2021`) are never suppressed.
- Never run `dotnet test` on the whole solution. One project at a time. TUnit projects (`NexusMods.Sdk.Tests`, `NexusMods.Backend.Tests`) run with `dotnet run --project`, not `dotnet test`.
- Nothing CP2077-specific outside `src/NexusMods.Games.RedEngine`. Nothing in the core assumes Nexus Mods.
- Never `AbsolutePath.DeleteDirectory(recursive: true)`; use `DeleteDirectoryNoFollow()` from `NexusMods.Sdk.IO`. Every destructive path gets a test with a symlink pointing outside.
- Commit messages: Conventional Commits, no AI co-author line, no "Generated with" footer (org rule).
- Log and exception messages in Spanish, as the surrounding code does. Code comments in English.
- `.verified.` snapshot files are never deleted or edited by hand. If one changes, the plan is wrong; stop and report.

## Review Focus

Inputs the spec implies but did not list as tests. Each has a test pinned to the task that owns the code:

1. The game folder sits inside the prefix (`drive_c/Games/Cyberpunk 2077`, common with Lutris): `Game` must stay a top-level location and keep being enumerated, or no game file would ever be indexed. Test in Task 1 (`GameInsideAWhitelistedLocation_StaysTopLevel`).
2. The whitelisted file itself is a symlink to a file outside the prefix: it must not be indexed, and a mod replacing it must fail the sync instead of writing through the link. Test in Task 4 (`WhitelistedFileThatIsASymlink_IsNeverIndexedNorWritten`).
3. The prefix is deleted after managing (Storage Manager "Borrar prefix"): the next sync must not throw, the entry just disappears. Test in Task 3 (`PrefixDeletedAfterManaging_SyncDoesNotThrow`).
4. The game edits `UserSettings.json` between syncs: the reset must restore the original content, not the edit. Test in Task 4 (`ResetRestoresTheSettingsAndLeavesTheRest` edits the file before the reset).
5. No prefix at all (`LinuxCompatabilityDataProvider` null): the only location is `Game` and existing tests keep their snapshots. Covered by running the full `NexusMods.DataModel.Synchronizer.Tests` and `NexusMods.Games.RedEngine.Tests` projects unchanged in Task 2 and Task 6.

---

### Task 1: Whitelist in `GameLocations` (Sdk)

**Files:**
- Modify: `src/NexusMods.Sdk/Games/Locators/LocationId.cs:50-52` (add `WinePrefix` after `AppDataRoaming`)
- Modify: `src/NexusMods.Sdk/Games/Locators/GameLocationDescriptor.cs`
- Modify: `src/NexusMods.Sdk/Games/Locators/GameLocations.cs:28-60` (`Create`), add `IsManaged`
- Modify: `src/NexusMods.Sdk/Games/IGameData.cs:42` (add `GetManagedFiles` next to `GetLocations`)
- Modify: `src/NexusMods.Backend/Games/GameRegistry.cs:55-56`
- Test: `tests/NexusMods.Sdk.Tests/GameLocationsTests.cs`

**Interfaces:**
- Produces: `LocationId.WinePrefix`; `GameLocationDescriptor.ManagedFiles` (`ImmutableHashSet<RelativePath>?`, null = whole location managed); `GameLocations.Create(ImmutableDictionary<LocationId, AbsolutePath> resolvedLocations, ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>? managedFiles = null)`; `bool GameLocations.IsManaged(GamePath)`; `IGameData.GetManagedFiles(IFileSystem, GameLocatorResult)` returning `ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>` (default empty).

- [ ] **Step 1: Write the failing TUnit tests**

Append to `tests/NexusMods.Sdk.Tests/GameLocationsTests.cs` inside the class:

```csharp
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
```

- [ ] **Step 2: Run the tests to verify they fail to compile**

Run: `cd ~/Repos/tModManager && DOTNET_ROLL_FORWARD=Major dotnet build tests/NexusMods.Sdk.Tests 2>&1 | grep -E 'error|Warn|Error' | head`
Expected: errors `'LocationId' does not contain a definition for 'WinePrefix'`, `No overload for method 'Create' takes 2 arguments`, `'GameLocations' does not contain a definition for 'IsManaged'`.

- [ ] **Step 3: Add `LocationId.WinePrefix`**

In `src/NexusMods.Sdk/Games/Locators/LocationId.cs`, after `AppDataRoaming`:

```csharp
    /// <summary>
    /// Root of the Wine/Proton prefix the game runs in (<c>.../compatdata/&lt;appid&gt;/pfx</c>). Always declared with a
    /// whitelist (<see cref="IGameData.GetManagedFiles"/>): the app only ever touches the listed files inside it.
    /// </summary>
    public static readonly LocationId WinePrefix = From("WinePrefix");
```

- [ ] **Step 4: Add `ManagedFiles` to the descriptor**

Replace the class body of `src/NexusMods.Sdk/Games/Locators/GameLocationDescriptor.cs`:

```csharp
public class GameLocationDescriptor
{
    public LocationId LocationId { get; }
    public AbsolutePath Path { get; }
    public ImmutableArray<LocationId> NestedLocations { get; }
    public Optional<LocationId> TopLevelParent { get; }

    /// <summary>
    /// Files the app manages in this location, relative to <see cref="Path"/>. Null means the whole location is
    /// managed (the game folder). A location with a whitelist is never enumerated and nothing outside the list
    /// is read, written, backed up or deleted.
    /// </summary>
    public ImmutableHashSet<RelativePath>? ManagedFiles { get; }

    public bool IsTopLevel => !TopLevelParent.HasValue;

    public GameLocationDescriptor(LocationId locatorId, AbsolutePath path, ImmutableArray<LocationId> nestedLocations, Optional<LocationId> topLevelParent, ImmutableHashSet<RelativePath>? managedFiles = null)
    {
        LocationId = locatorId;
        Path = path;
        NestedLocations = nestedLocations;
        TopLevelParent = topLevelParent;
        ManagedFiles = managedFiles;
    }
}
```

- [ ] **Step 5: Thread the whitelist through `GameLocations.Create` and add `IsManaged`**

In `src/NexusMods.Sdk/Games/Locators/GameLocations.cs`, replace `Create` with:

```csharp
    public static GameLocations Create(
        ImmutableDictionary<LocationId, AbsolutePath> resolvedLocations,
        ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>? managedFiles = null)
    {
        managedFiles ??= ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>.Empty;
        var results = new Dictionary<LocationId, GameLocationDescriptor>(capacity: resolvedLocations.Count);

        var nestedLocations = new List<LocationId>(capacity: resolvedLocations.Count);
        foreach (var (currentLocation, currentPath) in resolvedLocations)
        {
            var topLevelParent = Optional<(LocationId, AbsolutePath)>.None;
            nestedLocations.Clear();

            foreach (var (otherLocation, otherPath) in resolvedLocations)
            {
                if (otherLocation == currentLocation) continue;

                // A whitelisted location (the Wine prefix) never swallows another: a game installed inside the
                // prefix stays top level and keeps being scanned
                if (otherPath.InFolder(currentPath) && !managedFiles.ContainsKey(currentLocation))
                {
                    nestedLocations.Add(otherLocation);
                } else if (currentPath.InFolder(otherPath) && !managedFiles.ContainsKey(otherLocation))
                {
                    if (!topLevelParent.HasValue) topLevelParent = (otherLocation, otherPath);
                    else
                    {
                        topLevelParent = topLevelParent.Value.Item2.InFolder(otherPath) ? (otherLocation, otherPath) : topLevelParent;
                    }
                }
            }

            var descriptor = new GameLocationDescriptor(
                currentLocation,
                currentPath,
                nestedLocations: [..nestedLocations],
                topLevelParent: topLevelParent.Convert(tuple => tuple.Item1),
                managedFiles: managedFiles.GetValueOrDefault(currentLocation)
            );

            results.Add(currentLocation, descriptor);
        }

        return new GameLocations(results.ToFrozenDictionary());
    }

    /// <summary>
    /// False when the path's location has a whitelist and the path is not in it: the app must not read, write,
    /// back up or delete it.
    /// </summary>
    public bool IsManaged(GamePath gamePath)
    {
        var managed = _locations[gamePath.LocationId].ManagedFiles;
        return managed is null || managed.Contains(gamePath.Path);
    }
```

- [ ] **Step 6: Add `GetManagedFiles` to `IGameData` and wire `GameRegistry`**

In `src/NexusMods.Sdk/Games/IGameData.cs`, right after the `GetLocations` declaration (line 42):

```csharp
    /// <summary>
    /// Per-location whitelist of the files the app manages. A location missing here is managed whole (the game
    /// folder); a location listed here (the Wine prefix) is only ever touched at the listed paths.
    /// </summary>
    ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>> GetManagedFiles(IFileSystem fileSystem, GameLocatorResult gameLocatorResult)
        => ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>.Empty;
```

In `src/NexusMods.Backend/Games/GameRegistry.cs` replace lines 55-56:

```csharp
                            var resolvedLocations = locatorResult.Game.GetLocations(locatorResult.Path.FileSystem, locatorResult);
                            var managedFiles = locatorResult.Game.GetManagedFiles(locatorResult.Path.FileSystem, locatorResult);
                            var gameLocations = GameLocations.Create(resolvedLocations, managedFiles);
```

- [ ] **Step 7: Run the Sdk tests**

Run: `cd ~/Repos/tModManager && DOTNET_ROLL_FORWARD=Major dotnet run --project tests/NexusMods.Sdk.Tests -- --treenode-filter "/*/*/GameLocationsTests/*" 2>&1 | tail -15`
Expected: all `GameLocationsTests` pass (the 3 new ones plus the existing `ToAbsolutePath_*`).

- [ ] **Step 8: Build the solution with warnings as errors**

Run: `cd ~/Repos/tModManager && DOTNET_ROLL_FORWARD=Major dotnet build -p:TreatWarningsAsErrors=true 2>&1 | grep -E 'error|Warn' | grep -v NU19 | head`
Expected: no output (0 errors, 0 compiler warnings).

- [ ] **Step 9: Commit**

```bash
git add src/NexusMods.Sdk/Games/Locators/LocationId.cs src/NexusMods.Sdk/Games/Locators/GameLocationDescriptor.cs src/NexusMods.Sdk/Games/Locators/GameLocations.cs src/NexusMods.Sdk/Games/IGameData.cs src/NexusMods.Backend/Games/GameRegistry.cs tests/NexusMods.Sdk.Tests/GameLocationsTests.cs
git commit -m "feat(sdk): per-location managed-files whitelist and LocationId.WinePrefix"
```

---

### Task 2: Test fixture with a stub Wine prefix

**Files:**
- Modify: `tests/NexusMods.StandardGameLocators.TestHelpers/UniversalStubbedGameLocator.cs`
- Modify: `tests/NexusMods.StandardGameLocators.TestHelpers/StubbedTestHarness.cs`
- Create: `tests/NexusMods.StandardGameLocators.TestHelpers/StubbedLinuxCompatabilityDataProvider.cs`
- Modify: `tests/Games/NexusMods.Games.TestFramework/ACyberpunkIsolatedGameTest.cs`
- Modify: `tests/Games/NexusMods.Games.TestFramework/AIsolatedGameTest.cs:297-345` (`AddModAsync`)

**Interfaces:**
- Produces: `AddUniversalGameLocator<TGame>(version, gameFiles = null, stores = null, withWinePrefix = false)`; `UniversalStubbedGameLocator<TGame>.WinePrefix` (`AbsolutePath?`, `<temp>/pfx` when enabled); `ACyberpunkIsolatedGameTest<TTest>.WithWinePrefix` (`protected virtual bool`, default false); `AIsolatedGameTest.AddModAsync(ITransaction, IEnumerable<GamePath>, LoadoutId, string, LibraryArchive.ReadOnly? = null, LoadoutItemGroupId? = null)` with the existing `IEnumerable<RelativePath>` overload delegating to it (content of each file = its relative path as UTF-8, unchanged).

No new test in this task: it is fixture work. The check is that every existing Synchronizer and RedEngine test still passes with identical snapshots.

- [ ] **Step 1: Add the stub provider**

Create `tests/NexusMods.StandardGameLocators.TestHelpers/StubbedLinuxCompatabilityDataProvider.cs`:

```csharp
using System.Collections.Immutable;
using NexusMods.Paths;
using NexusMods.Sdk;
using NexusMods.Sdk.Games;

namespace NexusMods.StandardGameLocators.TestHelpers;

/// <summary>
/// A Wine prefix for tests: just a folder, no DLL overrides, no winetricks packages.
/// </summary>
public sealed class StubbedLinuxCompatabilityDataProvider(AbsolutePath winePrefix) : ILinuxCompatabilityDataProvider
{
    public AbsolutePath WinePrefixDirectoryPath => winePrefix;

    public ValueTask<ImmutableArray<WineDllOverride>> GetWineDllOverrides(CancellationToken cancellationToken)
        => ValueTask.FromResult(ImmutableArray<WineDllOverride>.Empty);

    public ValueTask<ImmutableHashSet<string>> GetInstalledWinetricksComponents(CancellationToken cancellationToken)
        => ValueTask.FromResult(ImmutableHashSet<string>.Empty);
}
```

(`WineDllOverride` lives in `NexusMods.Sdk`; if the build says otherwise, use the namespace `ILinuxCompatabilityDataProvider.cs` imports.)

- [ ] **Step 2: Let the stub locator create `pfx/` on request**

In `tests/NexusMods.StandardGameLocators.TestHelpers/UniversalStubbedGameLocator.cs`, add a field and property, a constructor parameter, and set the provider on the result:

```csharp
    public LocatorId[] LocatorIds { get; set; } = [LocatorId.From("StubbedGameState.zip")];

    /// <summary>The stub Wine prefix (<c>&lt;temp&gt;/pfx</c>), or null when the locator was created without one.</summary>
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
            WinePrefix = _path.Path.Combine("pfx");
            WinePrefix.Value.CreateDirectory();
        }

        if (gameFiles is null) return;
        ...
```

and in `Locate()`:

```csharp
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
```

In `tests/NexusMods.StandardGameLocators.TestHelpers/StubbedTestHarness.cs` add the parameter and pass it through:

```csharp
    public static IServiceCollection AddUniversalGameLocator<TGame>(
        this IServiceCollection services,
        Version version,
        Dictionary<RelativePath, byte[]>? gameFiles = null,
        GameStore[]? stores = null,
        bool withWinePrefix = false)
        where TGame : IGame
    {
        services
            .AddSingleton<IGameLocator, UniversalStubbedGameLocator<TGame>>(s =>
                new UniversalStubbedGameLocator<TGame>(
                    s,
                    s.GetRequiredService<IFileSystem>(),
                    s.GetRequiredService<TemporaryFileManager>(),
                    gameFiles,
                    stores,
                    withWinePrefix));

        return services;
    }
```

- [ ] **Step 3: Opt-in flag on the CP2077 fixture**

In `tests/Games/NexusMods.Games.TestFramework/ACyberpunkIsolatedGameTest.cs`:

```csharp
public class ACyberpunkIsolatedGameTest<TTest>(ITestOutputHelper helper) : AIsolatedGameTest<TTest, Cyberpunk2077Game>(helper)
{
    public static Dictionary<RelativePath, byte[]> PrimaryFile => new() { [(RelativePath)"bin/x64/Cyberpunk2077.exe"] = "Cyberpunk2077.exe"u8.ToArray() };

    /// <summary>True gives the stub installation a Wine prefix (<c>pfx/</c>), and with it the <c>WinePrefix</c> location.</summary>
    protected virtual bool WithWinePrefix => false;

    protected override IServiceCollection AddServices(IServiceCollection services)
    {
        return base.AddServices(services)
            .AddOSInterop()
            .AddRuntimeDependencies()
            .AddGenericGameSupport()
            .AddUniversalGameLocator<Cyberpunk2077Game>(new Version("1.61"), PrimaryFile, withWinePrefix: WithWinePrefix)
            .AddRedEngineGames();
    }
}
```

- [ ] **Step 4: `AddModAsync` with `GamePath`**

In `tests/Games/NexusMods.Games.TestFramework/AIsolatedGameTest.cs`, rename the existing method body to take `IEnumerable<GamePath>` and add the old signature as a delegating overload. The existing method (line 297) becomes:

```csharp
    public Task<List<Hash>> AddModAsync(
        ITransaction tx,
        IEnumerable<RelativePath> paths,
        LoadoutId loadoutId,
        string modName,
        LibraryArchive.ReadOnly? libraryArchive = null,
        LoadoutItemGroupId? parentGroup = null)
        => AddModAsync(tx, paths.Select(path => new GamePath(LocationId.Game, path)), loadoutId, modName, libraryArchive, parentGroup);

    /// <summary>
    /// Same as above for any location: the file content is the UTF-8 of the relative path (without the location).
    /// </summary>
    public async Task<List<Hash>> AddModAsync(
        ITransaction tx,
        IEnumerable<GamePath> paths,
        LoadoutId loadoutId,
        string modName,
        LibraryArchive.ReadOnly? libraryArchive = null,
        LoadoutItemGroupId? parentGroup = null)
    {
        var records = new List<ArchivedFileEntry>();
        var hashes = new List<Hash>();
        var modGroup = AddEmptyGroup(tx, loadoutId, modName, parentGroup);
        foreach (var gamePath in paths)
        {
            var path = gamePath.Path;
            var data = Encoding.UTF8.GetBytes(path);
            var hash = data.xxHash3();
            var size = Size.FromLong(path.Path.Length);

            // Create the LoadoutFile in DB
            AddFileInternal(tx, loadoutId, modGroup, gamePath, hash, size);
            ...   // rest of the loop body unchanged: it only uses `path` (RelativePath)
```

Keep the rest of the loop (`hashes.Add`, `records.Add`, the `libraryArchive` block using `path`) exactly as it is.

- [ ] **Step 5: Build and run the unchanged suites**

Run, one at a time:

```bash
cd ~/Repos/tModManager && export DOTNET_ROLL_FORWARD=Major
dotnet build -p:TreatWarningsAsErrors=true 2>&1 | grep -E 'error|Warn' | grep -v NU19 | head
dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True" 2>&1 | tail -5
dotnet test tests/Games/NexusMods.Games.RedEngine.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True" 2>&1 | tail -5
```

Expected: build clean; both test runs report `Failed: 0`. `git status` shows no new or modified `.verified.` or `.received.` files.

- [ ] **Step 6: Commit**

```bash
git add tests/NexusMods.StandardGameLocators.TestHelpers tests/Games/NexusMods.Games.TestFramework
git commit -m "test: stub Wine prefix in the universal locator and AddModAsync with GamePath"
```

---

### Task 3: CP2077 declares the prefix; the scan stats only the whitelist

**Files:**
- Modify: `src/NexusMods.Games.RedEngine/Cyberpunk2077/Cyberpunk2077Game.cs:84-98`
- Modify: `src/NexusMods.Backend/Games/GameLocationsService.cs:32-41`
- Create: `tests/NexusMods.DataModel.Synchronizer.Tests/WinePrefixLocationTests.cs`

**Interfaces:**
- Consumes: `LocationId.WinePrefix`, `GameLocationDescriptor.ManagedFiles`, `IGameData.GetManagedFiles` (Task 1); `WithWinePrefix`, `AddModAsync(GamePath)` (Task 2).
- Produces: `Cyberpunk2077Game.GetLocations` adds `WinePrefix → WinePrefixDirectoryPath` when the locator result has a `LinuxCompatabilityDataProvider`; `Cyberpunk2077Game.GetManagedFiles` returns `{ WinePrefix: ["drive_c/users/<user>/AppData/Local/CD Projekt Red/Cyberpunk 2077/UserSettings.json"] }`; the test class with helpers `Settings`, `SettingsPath`, `Prefix`, `PrefixFile`, `WritePrefixFile`, `ManagedLoadout`, `IndexedPrefixPaths` that Task 4 extends.

- [ ] **Step 1: Write the failing tests**

Create `tests/NexusMods.DataModel.Synchronizer.Tests/WinePrefixLocationTests.cs`:

```csharp
using FluentAssertions;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.TestFramework;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.IO;
using NexusMods.Sdk.Loadouts;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

/// <summary>
/// The Wine prefix is a location with a whitelist: only the listed files are indexed, written or deleted, never
/// through a symlink, and nothing else inside the prefix is ever touched.
/// </summary>
public class WinePrefixLocationTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<WinePrefixLocationTests>(helper)
{
    protected override bool WithWinePrefix => true;

    private const string SettingsFolder = "drive_c/users/steamuser/AppData/Local/CD Projekt Red/Cyberpunk 2077";
    private const string Settings = SettingsFolder + "/UserSettings.json";
    private static readonly GamePath SettingsPath = new(LocationId.WinePrefix, Settings);

    private AbsolutePath Prefix => GameInstallation.Locations[LocationId.WinePrefix].Path;
    private AbsolutePath PrefixFile(string relative) => Prefix.Combine(relative);

    private async Task WritePrefixFile(string relative, string content)
    {
        var file = PrefixFile(relative);
        file.Parent.CreateDirectory();
        await file.WriteAllTextAsync(content);
    }

    private async Task<Loadout.ReadOnly> ManagedLoadout()
    {
        await LoadoutManager.ManageInstallation(GameInstallation);
        await Synchronizer.ReindexState(GameInstallation);
        return await Synchronizer.Synchronize(await CreateLoadout());
    }

    private GamePath[] IndexedPrefixPaths()
    {
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        return DiskStateEntry.FindByGame(metadata.Db, metadata)
            .Select(e => (GamePath)e.Path)
            .Where(p => p.LocationId == LocationId.WinePrefix)
            .ToArray();
    }

    [Fact]
    public void ThePrefixIsALocationWithAWhitelist()
    {
        GameInstallation.Locations[LocationId.WinePrefix].Path.ToString().Should().EndWith("/pfx");
        GameInstallation.Locations[LocationId.WinePrefix].ManagedFiles.Should().BeEquivalentTo([(RelativePath)Settings]);
        GameInstallation.Locations[LocationId.Game].ManagedFiles.Should().BeNull();
    }

    [Fact]
    public async Task IndexesOnlyTheWhitelist()
    {
        await WritePrefixFile(Settings, "{}");
        await WritePrefixFile(SettingsFolder + "/CrashInfo.json", "crash");
        await WritePrefixFile(SettingsFolder + "/cache/x", "cache");
        await WritePrefixFile("drive_c/windows/system32/a.dll", "dll");

        await ManagedLoadout();

        IndexedPrefixPaths().Should().Equal(SettingsPath);
    }

    [Fact]
    public async Task PrefixDeletedAfterManaging_SyncDoesNotThrow()
    {
        await WritePrefixFile(Settings, "{}");
        var loadout = await ManagedLoadout();
        IndexedPrefixPaths().Should().Equal(SettingsPath);

        // Storage Manager "Borrar prefix de Proton", or Steam recreating it
        Prefix.DeleteDirectoryNoFollow();

        loadout = await Synchronizer.Synchronize(loadout.Rebase());

        IndexedPrefixPaths().Should().BeEmpty();
        loadout.IsValid().Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run the tests to verify they fail**

Run: `cd ~/Repos/tModManager && DOTNET_ROLL_FORWARD=Major dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~WinePrefixLocationTests" 2>&1 | grep -E 'Failed|Passed|error' | head`
Expected: the 3 tests fail. `ThePrefixIsALocationWithAWhitelist` with `KeyNotFoundException` (no `WinePrefix` location yet); `IndexesOnlyTheWhitelist` with the same.

- [ ] **Step 3: Declare the location and the whitelist in CP2077**

In `src/NexusMods.Games.RedEngine/Cyberpunk2077/Cyberpunk2077Game.cs`, replace `GetLocations` (lines 84-98) with:

```csharp
    public ImmutableDictionary<LocationId, AbsolutePath> GetLocations(IFileSystem fileSystem, GameLocatorResult gameLocatorResult)
    {
        var locations = new Dictionary<LocationId, AbsolutePath>
        {
            { LocationId.Game, gameLocatorResult.Path },
            // Saves stay out until a mod needs them: they would go through the same whitelist as the settings
        };
        // The prefix root, not the settings folder: SafePath.IsUnderSymlink from the root covers every folder in
        // between (Wine links Documents/Desktop to $HOME; people link the saves folder to a sync folder)
        if (gameLocatorResult.LinuxCompatabilityDataProvider is { } linux)
            locations[LocationId.WinePrefix] = linux.WinePrefixDirectoryPath;
        return locations.ToImmutableDictionary();
    }

    public ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>> GetManagedFiles(IFileSystem fileSystem, GameLocatorResult gameLocatorResult)
    {
        if (gameLocatorResult.LinuxCompatabilityDataProvider is not { } linux)
            return ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>.Empty;

        var user = WineUserName(linux.WinePrefixDirectoryPath);
        var settings = (RelativePath)$"drive_c/users/{user}/AppData/Local/CD Projekt Red/Cyberpunk 2077/UserSettings.json";
        return ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>.Empty.Add(LocationId.WinePrefix, [settings]);
    }

    /// <summary>
    /// Proton runs every game as <c>steamuser</c>; a Wine/Lutris prefix uses the real user name. Before the first
    /// launch neither folder exists and the Proton name is assumed.
    /// </summary>
    private static string WineUserName(AbsolutePath prefix)
    {
        var users = prefix.Combine("drive_c/users");
        if (users.Combine("steamuser").DirectoryExists()) return "steamuser";
        var current = Environment.UserName;
        return users.Combine(current).DirectoryExists() ? current : "steamuser";
    }
```

- [ ] **Step 4: Stat the whitelist instead of enumerating**

In `src/NexusMods.Backend/Games/GameLocationsService.cs`, replace lines 38-41 (the `topLevelLocations` / `enumerable` block) with:

```csharp
        var enumerable = installation.Locations.GetTopLevelLocations()
            .Where(kv => kv.Value.DirectoryExists())
            .SelectMany(kv => FilesToIndex(installation.Locations[kv.Key]));
```

and add the helper to the class:

```csharp
    /// <summary>
    /// Never enter symlinked folders: files behind a link live elsewhere, and once indexed the synchronizer would
    /// back them up and delete them on clean/unmanage/loadout switch. A location with a whitelist (the Wine prefix)
    /// is not enumerated at all: only the listed files are looked at, and none of them through a symlink.
    /// </summary>
    private static IEnumerable<AbsolutePath> FilesToIndex(GameLocationDescriptor location)
    {
        var root = location.Path;
        if (location.ManagedFiles is { } managed)
        {
            return managed
                .Select(relative => root.Combine(relative))
                .Where(file => file.FileExists
                               && !SafePath.IsUnderSymlink(root.ToString(), file.ToString())
                               && !SafePath.IsSymlink(file));
        }
        return SafePath.EnumerateFilesNoFollow(root).Where(file => SafePath.IsStrictlyInside(root, file));
    }
```

- [ ] **Step 5: Run the tests**

Run: `cd ~/Repos/tModManager && DOTNET_ROLL_FORWARD=Major dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~WinePrefixLocationTests" 2>&1 | grep -E 'Failed|Passed|error' | head`
Expected: `Passed: 3`.

- [ ] **Step 6: Run the whole Synchronizer and RedEngine projects**

```bash
cd ~/Repos/tModManager && export DOTNET_ROLL_FORWARD=Major
dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True" 2>&1 | tail -3
dotnet test tests/Games/NexusMods.Games.RedEngine.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True" 2>&1 | tail -3
```

Expected: `Failed: 0` in both; no `.received.` files in `git status`.

- [ ] **Step 7: Commit**

```bash
git add src/NexusMods.Games.RedEngine/Cyberpunk2077/Cyberpunk2077Game.cs src/NexusMods.Backend/Games/GameLocationsService.cs tests/NexusMods.DataModel.Synchronizer.Tests/WinePrefixLocationTests.cs
git commit -m "feat(cp2077): UserSettings.json in the Wine prefix as a whitelisted location"
```

---

### Task 4: Writes, deletes and the reset stay inside the whitelist

**Files:**
- Modify: `src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs:115-116` (`CleanDirectories`) and `:580-590` (`EnsureDiskChangesStayInside`)
- Test: `tests/NexusMods.DataModel.Synchronizer.Tests/WinePrefixLocationTests.cs`

**Interfaces:**
- Consumes: `GameLocations.IsManaged`, `GameLocationDescriptor.ManagedFiles` (Task 1); the test helpers from Task 3.

- [ ] **Step 1: Write the failing tests**

Append inside `WinePrefixLocationTests`:

```csharp
    private async Task<Loadout.ReadOnly> WithSettingsMod(Loadout.ReadOnly loadout)
    {
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [SettingsPath], loadout, "SettingsMod");
            await tx.Commit();
        }
        Refresh(ref loadout);
        return loadout;
    }

    /// <summary>Canary folder outside the prefix with the settings chain inside it, and a symlink at <paramref name="linkRelative"/> pointing to it.</summary>
    private async Task<(AbsolutePath Outside, AbsolutePath Settings, AbsolutePath Canary)> LinkedOutside(string linkRelative, string insideSettings)
    {
        var outside = TemporaryFileManager.CreateFolder().Path;
        var settings = outside.Combine(insideSettings);
        settings.Parent.CreateDirectory();
        await settings.WriteAllTextAsync("user data");
        var canary = outside.Combine("precious.txt");
        await canary.WriteAllTextAsync("precious");

        var link = PrefixFile(linkRelative);
        link.Parent.CreateDirectory();
        File.CreateSymbolicLink(link.ToString(), outside.ToString());
        return (outside, settings, canary);
    }

    [Fact]
    public async Task ResetRestoresTheSettingsAndLeavesTheRest()
    {
        await WritePrefixFile(Settings, "original");
        await WritePrefixFile(SettingsFolder + "/CrashInfo.json", "crash");
        var loadout = await ManagedLoadout();
        loadout = await WithSettingsMod(loadout);

        loadout = await Synchronizer.Synchronize(loadout);
        (await PrefixFile(Settings).ReadAllTextAsync()).Should().Be(Settings, "AddModAsync writes the relative path as content");

        // The game edits its settings between syncs
        await PrefixFile(Settings).WriteAllTextAsync("edited by the game");

        await LoadoutManager.UnManage(GameInstallation);

        (await PrefixFile(Settings).ReadAllTextAsync()).Should().Be("original");
        (await PrefixFile(SettingsFolder + "/CrashInfo.json").ReadAllTextAsync()).Should().Be("crash");
        PrefixFile(SettingsFolder).DirectoryExists().Should().BeTrue("the app never removes folders inside the prefix");
    }

    [Fact]
    public async Task ModFileOutsideTheWhitelist_FailsTheSyncWithoutWriting()
    {
        var loadout = await ManagedLoadout();
        var outsideWhitelist = new GamePath(LocationId.WinePrefix, "drive_c/users/steamuser/Desktop/x.txt");
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [outsideWhitelist], loadout, "DesktopMod");
            await tx.Commit();
        }
        Refresh(ref loadout);

        var act = () => Synchronizer.Synchronize(loadout);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*no está entre los archivos*");
        PrefixFile("drive_c/users/steamuser/Desktop/x.txt").FileExists.Should().BeFalse();
    }

    [Fact]
    public async Task SymlinkedSettingsFolder_IsNeverWrittenThroughNorIndexed()
    {
        var (_, outsideSettings, canary) = await LinkedOutside(SettingsFolder, "UserSettings.json");
        var loadout = await ManagedLoadout();
        IndexedPrefixPaths().Should().BeEmpty();

        loadout = await WithSettingsMod(loadout);
        var act = () => Synchronizer.Synchronize(loadout);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*symlink*");
        (await outsideSettings.ReadAllTextAsync()).Should().Be("user data");
        canary.FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task SymlinkedUserFolder_IsNeverIndexedNorDeleted()
    {
        // A Wine prefix can link the whole user folder, or Documents/Desktop, to the real $HOME
        var (_, outsideSettings, canary) = await LinkedOutside("drive_c/users/steamuser", "AppData/Local/CD Projekt Red/Cyberpunk 2077/UserSettings.json");

        await ManagedLoadout();
        IndexedPrefixPaths().Should().BeEmpty();

        await LoadoutManager.UnManage(GameInstallation);
        (await outsideSettings.ReadAllTextAsync()).Should().Be("user data");
        canary.FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task WhitelistedFileThatIsASymlink_IsNeverIndexedNorWritten()
    {
        var outside = TemporaryFileManager.CreateFolder().Path.Combine("UserSettings.json");
        await outside.WriteAllTextAsync("user data");
        PrefixFile(Settings).Parent.CreateDirectory();
        File.CreateSymbolicLink(PrefixFile(Settings).ToString(), outside.ToString());

        var loadout = await ManagedLoadout();
        IndexedPrefixPaths().Should().BeEmpty();

        loadout = await WithSettingsMod(loadout);
        var act = () => Synchronizer.Synchronize(loadout);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*symlink*");
        (await outside.ReadAllTextAsync()).Should().Be("user data");
    }
```

(`Refresh(ref loadout)` is the helper `DiskSafetyTests` already uses from the base fixture.)

- [ ] **Step 2: Run the tests to verify they fail**

Run: `cd ~/Repos/tModManager && DOTNET_ROLL_FORWARD=Major dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~WinePrefixLocationTests" 2>&1 | grep -E 'Failed|Passed|\[FAIL\]' | head -20`
Expected: `ModFileOutsideTheWhitelist_FailsTheSyncWithoutWriting` fails (no exception, `x.txt` written); `WhitelistedFileThatIsASymlink_IsNeverIndexedNorWritten` fails (the sync writes through the file link; `outside` content changes). `SymlinkedSettingsFolder_*` passes already through `IsUnderSymlink` from the prefix root and `SymlinkedUserFolder_*` through the scan: keep them, they pin the choice of `pfx` as the location root. `ResetRestoresTheSettingsAndLeavesTheRest` may pass or fail on the folder assertion depending on `CleanDirectories`; either way it is green after Step 3.

- [ ] **Step 3: Enforce the whitelist before any disk change, and keep folders**

In `src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs` replace `EnsureDiskChangesStayInside` (lines 576-590):

```csharp
    /// <summary>
    /// Throws before anything touches the disk when a write or delete would land outside its location: a <c>..</c>
    /// segment (<see cref="GameLocations.ToAbsolutePath"/> throws), a folder in between that is a symlink, or, in a
    /// location with a whitelist (the Wine prefix), a path that is not on the list or is itself a symlink.
    /// </summary>
    private static void EnsureDiskChangesStayInside(Dictionary<GamePath, SyncNode> syncTree, GameLocations locations)
    {
        const Actions diskChanges = Actions.DeleteFromDisk | Actions.ExtractToDisk | Actions.WriteIntrinsic;
        foreach (var (path, node) in syncTree)
        {
            if ((node.Actions & diskChanges) == 0) continue;
            if (!locations.IsManaged(path))
                throw new InvalidOperationException($"`{path}` no está entre los archivos que tModManager gestiona en esa ubicación; no se escribe ni se borra nada ahí");
            var resolved = locations.ToAbsolutePath(path);
            if (SafePath.IsUnderSymlink(locations[path.LocationId].Path.ToString(), resolved.ToString()))
                throw new InvalidOperationException($"`{path}` está dentro de una carpeta que es un symlink; tModManager no escribe ni borra a través de links");
            if (locations[path.LocationId].ManagedFiles is not null && SafePath.IsSymlink(resolved))
                throw new InvalidOperationException($"`{path}` es un symlink; tModManager no escribe ni borra a través de links");
        }
    }
```

At the top of `CleanDirectories` (line 115), before the three `HashSet`s:

```csharp
    private void CleanDirectories(IEnumerable<GamePath> directoriesWithDeletions, DiskState newDiskState, GameInstallation installation)
    {
        // Folders inside a whitelisted location (the Wine prefix) are the game's or Wine's, never ours to remove
        directoriesWithDeletions = directoriesWithDeletions.Where(dir => installation.Locations[dir.LocationId].ManagedFiles is null);
```

- [ ] **Step 4: Run the tests**

Run: `cd ~/Repos/tModManager && DOTNET_ROLL_FORWARD=Major dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~WinePrefixLocationTests" 2>&1 | grep -E 'Failed|Passed' | head`
Expected: `Passed: 8, Failed: 0`.

- [ ] **Step 5: Prove the symlink tests fail without the fix**

Temporarily revert only the `EnsureDiskChangesStayInside` change (`git stash push src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs`), run the filter again, and confirm `ModFileOutsideTheWhitelist_*` and `WhitelistedFileThatIsASymlink_*` fail. Then `git stash pop`. Then edit `Cyberpunk2077Game.GetLocations` to point `WinePrefix` at `linux.WinePrefixDirectoryPath.Combine(SettingsFolder)` with the whitelist `UserSettings.json` (a 2-line local experiment), run, and confirm `SymlinkedSettingsFolder_*` fails (the location root itself is the link). Revert that experiment with `git checkout src/NexusMods.Games.RedEngine/Cyberpunk2077/Cyberpunk2077Game.cs`. Write the result of both checks in the commit body.

- [ ] **Step 6: Run the full Synchronizer, DataModel and RedEngine projects**

```bash
cd ~/Repos/tModManager && export DOTNET_ROLL_FORWARD=Major
dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True" 2>&1 | tail -3
dotnet test tests/NexusMods.DataModel.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True" 2>&1 | tail -3
dotnet test tests/Games/NexusMods.Games.RedEngine.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True" 2>&1 | tail -3
```

Expected: `Failed: 0` in all three; no `.received.` files.

- [ ] **Step 7: Commit**

```bash
git add src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs tests/NexusMods.DataModel.Synchronizer.Tests/WinePrefixLocationTests.cs
git commit -m "feat(sync): never write, delete or clean outside a location's whitelist

Without the whitelist check a mod file at drive_c/users/steamuser/Desktop/x.txt
was written; without the leaf symlink check a mod replacing a linked
UserSettings.json wrote through the link. With the location rooted at the
settings folder instead of pfx, a linked settings folder was written through."
```

---

### Task 5: Non-`Game` locations always take their baseline from the disk

**Files:**
- Modify: `src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs:1380-1397` (`UpdateBaseline`, Nexus branch)
- Test: `tests/NexusMods.DataModel.Synchronizer.Tests/SteamVanillaBaselineTests.cs`

**Interfaces:**
- Consumes: `BaselineRule.Apply(previous, disk, owned)` (exists), `LocationId.WinePrefix` (Task 1), `withWinePrefix` (Task 2).

- [ ] **Step 1: Write the failing test**

In `tests/NexusMods.DataModel.Synchronizer.Tests/SteamVanillaBaselineTests.cs`, change the locator registration to `.AddUniversalGameLocator<Cyberpunk2077Game>(new Version("1.61"), stores: [GameStore.Steam], withWinePrefix: true)` and add:

```csharp
    [Fact]
    public async Task KnownVersion_PrefixFileComesFromTheDisk()
    {
        // The Nexus list only describes the game folder: whitelisted prefix files are originals when they are there
        var settings = new GamePath(LocationId.WinePrefix, "drive_c/users/steamuser/AppData/Local/CD Projekt Red/Cyberpunk 2077/UserSettings.json");
        var onDisk = GameInstallation.Locations.ToAbsolutePath(settings);
        onDisk.Parent.CreateDirectory();
        await onDisk.WriteAllTextAsync("{}");

        await LoadoutManager.ManageInstallation(GameInstallation);
        await Synchronizer.Synchronize(await CreateLoadout());

        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        GameInstallMetadata.BaselineFromDisk.Get(metadata).Should().BeFalse();
        GameBaselineFile.TryGetVanillaFiles(metadata, out var files).Should().BeTrue();
        files.Select(f => (GamePath)f.Path).Should().Contain(settings);
        files.Select(f => (GamePath)f.Path).Where(p => p.LocationId == LocationId.Game).Should().BeEquivalentTo(NexusPaths("StubbedGameState.zip"));
    }
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `cd ~/Repos/tModManager && DOTNET_ROLL_FORWARD=Major dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~SteamVanillaBaselineTests" 2>&1 | grep -E 'Failed|Passed|\[FAIL\]' | head`
Expected: `KnownVersion_PrefixFileComesFromTheDisk` fails on `Should().Contain(settings)`; every other test in the class still passes.

- [ ] **Step 3: Apply `BaselineRule` to everything outside `Game` in the Nexus branch**

In `UpdateBaseline`, inside `if (nexusKnows) { ... }`, right after the `foreach (var (path, entry) in previous)` loop and before `files = nexus.Select(...)`:

```csharp
            // The hash DB only describes the game folder: every other location (the Wine prefix) follows the disk,
            // like a disk-made list does. Entries kept above win, so an edit since the last sync stays an External Change
            var outsideGame = disk.Where(d => d.Item1.LocationId != LocationId.Game).ToList();
            if (outsideGame.Count > 0)
            {
                var previousOutside = previous.Where(kv => kv.Key.LocationId != LocationId.Game).ToDictionary();
                foreach (var (path, entry) in BaselineRule.Apply(previousOutside, outsideGame, owned))
                    nexus.TryAdd(path, entry);
            }
```

- [ ] **Step 4: Run the baseline test classes**

```bash
cd ~/Repos/tModManager && export DOTNET_ROLL_FORWARD=Major
dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~VanillaBaselineTests|FullyQualifiedName~BaselineRuleTests|FullyQualifiedName~WinePrefixLocationTests" 2>&1 | grep -E 'Failed|Passed' | head
```

Expected: `Failed: 0`.

- [ ] **Step 5: Commit**

```bash
git add src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs tests/NexusMods.DataModel.Synchronizer.Tests/SteamVanillaBaselineTests.cs
git commit -m "feat(sync): baseline outside the game folder always comes from the disk"
```

---

### Task 6: Docs, full verification, PR

**Files:**
- Modify: `TODO.md` (pieza 2 row in the table at line 173; "Pendiente" items; the "Próximo" block at lines 7-9)
- Modify: `CLAUDE.md` ("Supported Game & Store" / "Game Plugin System" sections)
- Modify: `docs/superpowers/specs/2026-10-09-wine-prefix-location-design.md` (status line)

- [ ] **Step 1: Update `TODO.md`**

In the "Piezas genéricas" table, row 2 becomes (keep the table shape):

```markdown
| 2 | Ubicaciones dentro del prefix, con whitelist de archivos gestionados (hecha 2026-10-09, `LocationId.WinePrefix` + `IGameData.GetManagedFiles`; spec en `docs/superpowers/specs/2026-10-09-wine-prefix-location-design.md`; pendiente de prueba real) | `UserSettings.json` (saves y `modlist.txt` de REDmod cuando haga falta) | saves, `Documents/The Witcher 3/user.settings`, `mods.settings` | cualquier juego con prefix |
```

Under "### Pendiente de las pruebas" add:

```markdown
- [ ] **Pieza 2 en el juego real** (2026-10-09): gestionar con el prefix presente, cambiar un setting en el juego, ver el External Change de `UserSettings.json`, reset, confirmar que el setting volvió y que `cache/` y `CrashInfo.json` siguen. En la jaula (`./dev.sh` opción 11)
```

Under "### Otros TODO relevantes en código" (or the bug list) add:

```markdown
- [ ] **Symlinks de archivo en la carpeta del juego:** el scan los lista y un mod que los reemplace escribe a través del link. El prefix ya lo rechaza (`EnsureDiskChangesStayInside`, solo ubicaciones con whitelist); extender a `Game` cuando se decida qué hacer con links legítimos
```

In the "Próximo" block, point 2 becomes "Pieza 3 (mods locales de primera clase) o pieza 4 (`IIntrinsicFile`, primer uso: `UserSettings.json`)".

- [ ] **Step 2: Update `CLAUDE.md`**

In "Game Plugin System", after the `GetLocations()` bullet add:

```markdown
- `GetManagedFiles()` — optional per-location whitelist (`GameLocationDescriptor.ManagedFiles`). A whitelisted location (`LocationId.WinePrefix`, the Proton prefix root) is never enumerated: the scan stats only the listed files, `EnsureDiskChangesStayInside` refuses any write or delete outside the list or through a symlink, `CleanDirectories` leaves its folders alone, and its baseline always comes from the disk. CP2077 lists only `drive_c/users/steamuser/AppData/Local/CD Projekt Red/Cyberpunk 2077/UserSettings.json`
```

- [ ] **Step 3: Mark the spec as implemented**

Change the spec's status line to `Estado: implementado en la rama feat/wine-prefix-location (PR pendiente), prueba real pendiente`.

- [ ] **Step 4: Full verification, sequentially**

```bash
cd ~/Repos/tModManager && export DOTNET_ROLL_FORWARD=Major
dotnet build -p:TreatWarningsAsErrors=true 2>&1 | grep -E 'error|Warn' | grep -v NU19 | head
dotnet run --project tests/NexusMods.Sdk.Tests 2>&1 | tail -3
dotnet run --project tests/NexusMods.Backend.Tests 2>&1 | tail -3
for p in tests/NexusMods.DataModel.Synchronizer.Tests tests/NexusMods.DataModel.Tests tests/Games/NexusMods.Games.RedEngine.Tests tests/NexusMods.Library.Tests tests/NexusMods.Collections.Tests tests/NexusMods.StandardGameLocators.Tests tests/NexusMods.UI.Tests; do
  [ -d "$p" ] && echo "== $p" && dotnet test "$p" --filter "RequiresNetworking!=True&FlakeyTest!=True" 2>&1 | tail -2
done
git status --short | grep -E 'received|verified' && echo "SNAPSHOT CHANGED: stop" || echo "snapshots untouched"
```

Expected: build clean, every project `Failed: 0`, "snapshots untouched". If a project path does not exist, skip it (the loop does). Then run `./dev.sh` option 4 for the complete sequential suite and paste its last lines in the PR.

- [ ] **Step 5: Commit docs and open the PR**

```bash
git add TODO.md CLAUDE.md docs/superpowers/specs/2026-10-09-wine-prefix-location-design.md
git commit -m "docs: piece 2 (Wine prefix location) done, real-game test pending"
git push -u origin feat/wine-prefix-location
gh pr create --title "feat: Wine prefix as a whitelisted location (piece 2)" --body-file - <<'EOF'
## Qué

Pieza 2 de "Piezas genéricas para el segundo juego": `LocationId.WinePrefix` apunta a la raíz del prefix de Proton y cada juego declara, con `IGameData.GetManagedFiles`, qué archivos gestiona ahí. El core aplica la whitelist en el scan (stat de la lista, nunca se enumera `pfx`), antes de escribir o borrar (`EnsureDiskChangesStayInside`), en `CleanDirectories` y en la lista vanilla (fuera de `Game`, siempre del disco). CP2077 gestiona solo `UserSettings.json`.

Spec: `docs/superpowers/specs/2026-10-09-wine-prefix-location-design.md`.

## Seguridad

- Nada fuera de la whitelist se lee, respalda, escribe ni borra.
- Symlinks: la ubicación es `pfx`, así que `IsUnderSymlink` cubre toda la cadena hasta el archivo; el archivo mismo como symlink también se rechaza.
- Tests con symlink a una carpeta externa (`WinePrefixLocationTests`): fallan sin el fix (detalle en el commit de `feat(sync)`).

## Pendiente

Prueba real con el juego (`./dev.sh` opción 11): gestionar, cambiar un setting, ver el External Change, reset.
EOF
```

Do not merge. Report the PR URL and the test totals.

---

## Self-review

- **Spec coverage:** §1 location → Task 1 + Task 3; §2 whitelist → Task 1 (+ CP2077 in Task 3); §3 scan → Task 3, write/delete + CleanDirectories → Task 4; §4 baseline → Task 5; §5 tests 1-6 → Tasks 3, 4, 5; fixture → Task 2; docs → Task 6. Real-game test is the user's, noted in TODO.
- **Placeholders:** none. Every code step shows the code.
- **Type consistency:** `GetManagedFiles` returns `ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>` everywhere; `GameLocations.Create` second parameter is nullable of that type; `ManagedFiles` is `ImmutableHashSet<RelativePath>?`; `IsManaged(GamePath)` returns bool; the `AddModAsync(GamePath)` overload keeps the parameter order of the original.
- **Review Focus:** items 1-5 each have a test or a run pinned in Tasks 1, 3, 4, 6.
