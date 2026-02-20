# Copilot Instructions

## Context

This is a **fork** of NexusMods.App supporting only **Cyberpunk 2077 via Steam on Linux**. Support for other games (Stardew Valley, BG3, Skyrim/Fallout, Bannerlord), stores (GOG, EGS, Xbox), and platforms (Windows, macOS) has been removed from the solution. Some source directories for removed games still exist on disk but are not included in `NexusMods.App.sln`.

**Stack:** C# / .NET 10, Avalonia UI, ReactiveUI/R3, MnemonicDB, xUnit.

## Build & Run

```bash
dotnet build                                                        # build solution
dotnet run --project src/NexusMods.App/NexusMods.App.csproj        # run app
dotnet test                                                         # all tests
dotnet test --filter "RequiresNetworking!=True&FlakeyTest!=True"    # CI-safe (skip network/flakey)
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"      # single test
dotnet test tests/Games/NexusMods.Games.RedEngine.Tests             # CP2077-specific tests
dotnet build -p:UseSystemExtractor=true                             # use system 7z
```

Test filter traits: `RequiresNetworking`, `FlakeyTest`, `RequiresApiKey`.

## Solution Layers

| Project(s) | Purpose |
|---|---|
| `NexusMods.App` | Entry point; wires all DI, starts Avalonia UI or CLI |
| `NexusMods.App.UI` | Avalonia views + ViewModels (MVVM) |
| `NexusMods.App.Cli` | CLI verbs |
| `NexusMods.Backend` | Linux interop, file extraction, Steam game locator |
| `NexusMods.DataModel` | MnemonicDB persistence, synchronizer, loadout manager |
| `NexusMods.Library` | Mod library (add/remove/install) |
| `NexusMods.Collections` | Nexus Mods collection download/install |
| `NexusMods.Sdk` | Shared utilities, settings infrastructure |
| `NexusMods.Abstractions.*` | Interfaces/contracts between subsystems |
| `NexusMods.Games.RedEngine` | Cyberpunk 2077 implementation (only supported game) |
| `NexusMods.Games.FileHashes` | File hash DB for game version detection (Steam only) |
| `NexusMods.Networking.Steam` | Steam store integration |
| `NexusMods.Networking.NexusWebApi` | Nexus Mods API |

## Key Conventions

### Dependency Injection

Every subsystem exposes a static `Add*` extension method on `IServiceCollection` in its own `Services.cs`. The root `src/NexusMods.App/Services.cs` chains them all. Never register services anywhere else.

```csharp
// Example: src/NexusMods.Games.RedEngine/Services.cs
public static IServiceCollection AddRedEngineGames(this IServiceCollection services)
    => services.AddGame<Cyberpunk2077Game>()...;
```

### MnemonicDB Data Models

Models are `partial class` types implementing `IModelDefinition` with **static attribute fields**. Source generators derive query/transaction helpers.

```csharp
public partial class LoadoutItem : IModelDefinition
{
    private const string Namespace = "NexusMods.Loadouts.LoadoutItem";
    public static readonly StringAttribute Name = new(Namespace, nameof(Name));
    public static readonly MarkerAttribute Disabled = new(Namespace, nameof(Disabled)) { IsIndexed = true };
    public static readonly ReferenceAttribute<Loadout> Loadout = new(Namespace, nameof(Loadout)) { IsIndexed = true };

    public partial struct ReadOnly { /* extend generated struct here */ }
}
```

- Use `[Include<T>]` on a model to inherit all attributes from `T`.
- Complex transactional writes implement `ITxFunction` (read from `IDb basis`, write to `ITransaction tx`).
- Generated methods: `.New()`, `FindBy*`.

### Avalonia MVVM

- ViewModels inherit `AViewModel<TInterface>` and implement their `IXxxViewModel` interface.
- Reactive properties use `[Reactive]` (ReactiveUI.Fody) on auto-props.
- Register in the subsystem's `Services.cs`:

```csharp
services.AddViewModel<MyViewModel, IMyViewModel>();
services.AddView<MyView, IMyViewModel>();
```

- Design-time ViewModels (`*DesignViewModel`) exist alongside real VMs for Avalonia previewer.

### CLI Commands

```csharp
[Verb("verb-name", "description")]
public static async Task<int> MyVerb(
    [Option("s", "long-name", "description")] string arg,
    [Injected] IMyService service,
    CancellationToken cancel) { ... }
```

### Testing

Each test project needs a `Startup.cs` with `ConfigureServices` (xUnit.DependencyInjection pattern):

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
        => services.AddDefaultServicesForTesting().AddLogging(b => b.AddXUnit());
}
```

- Game-level tests inherit `AGameTest<TGame>` from `NexusMods.Games.TestFramework`.
- Snapshot tests use **Verify** — `.verified.*` files are committed to source; never delete them manually.
- `NexusMods.StandardGameLocators.TestHelpers` stubs Steam game detection in CI.

### Strict Compiler Rules (`.globalconfig`)

These are treated as **errors** — do not suppress:
- `CS4014`: un-awaited `Task`
- `CS8509`: non-exhaustive switch on named enum values
- `CA1069`: duplicate enum values
- `CA2211`: non-constant static fields that are externally visible

### Code Style

- LF line endings, 4-space indentation (enforced by `.editorconfig`).
- All NuGet versions are centralized in `Directory.Packages.props` — never add a `Version=` attribute directly to a `<PackageReference>`.
- Nullable reference types and implicit usings are enabled globally.

### Loadout & Sync Flow

The core loop: **Loadout** (immutable DB snapshot) → **Synchronizer** (3-way diff: DB vs. disk vs. desired) → **Apply** (write to disk). Hierarchy: `Loadout` → `LoadoutItemGroup` (mod) → `LoadoutItem` → `LoadoutFile`. `SynchronizerService` serializes all sync operations via a semaphore.

### GamePath

`GamePath` = `LocationId` + relative path. `LocationId` enumerates well-known game directories (Game, SaveData, Config, etc.). Always use `GamePath` for portable file references — never raw absolute paths.
