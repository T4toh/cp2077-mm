# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

NexusMods.App is a cross-platform (Windows, Linux, macOS) mod manager for PC games, built with C#/.NET 9 and Avalonia UI. It manages mod installation, load orders, file conflicts, and synchronization with game directories.

## Build & Run Commands

```bash
dotnet build                           # Build entire solution
dotnet run --project src/NexusMods.App/NexusMods.App.csproj  # Run the app

dotnet test                            # Run all tests
dotnet test --filter "RequiresNetworking!=True&FlakeyTest!=True"  # Skip network/flakey tests (CI default)
dotnet test --filter "FullyQualifiedName~SomeTestClass.SomeMethod"  # Run a single test
dotnet test tests/NexusMods.DataModel.Tests  # Run one test project

dotnet build -p:UseSystemExtractor=true  # Linux: use system 7z for extraction
```

Test traits used for filtering: `RequiresNetworking`, `FlakeyTest`, `RequiresApiKey`.

## Architecture

### Solution Structure

The solution (`NexusMods.App.sln`) has ~63 projects in `src/` and ~34 in `tests/`, organized into layers:

- **`NexusMods.App`** — Entry point (WinExe). Wires up DI, starts Avalonia UI or CLI.
- **`NexusMods.App.UI`** — Avalonia views and ViewModels (MVVM with ReactiveUI/R3).
- **`NexusMods.App.Cli`** — CLI commands using `[Verb]`/`[Option]`/`[Injected]` attributes.
- **`NexusMods.Backend`** — Core services: OS interop, file extraction, IPC.
- **`NexusMods.DataModel`** — MnemonicDB-based persistence, synchronizer service, loadout manager.
- **`NexusMods.Library`** — Mod library management (add/remove/install from library).
- **`NexusMods.Collections`** — Nexus Mods collection download and installation.
- **`NexusMods.Sdk`** — Shared utilities and settings infrastructure.
- **`NexusMods.Abstractions.*`** (~20 projects) — Interfaces and contracts for all subsystems.
- **`NexusMods.Games.*`** — Game-specific implementations (one project per game).
- **`NexusMods.Networking.*`** — HTTP downloads, Nexus API, Steam/GOG/Epic store integration.

### Dependency Injection Pattern

Every subsystem registers services through static extension methods in its own `Services.cs`:

```csharp
public static IServiceCollection AddStardewValley(this IServiceCollection services)
{
    return services
        .AddGame<StardewValley>()
        .AddSingleton<SMAPIInstaller>()
        .AddSMAPILoadoutItemModel()
        .AddSettings<StardewValleySettings>();
}
```

The main `Services.cs` in `NexusMods.App` chains all of these together. Startup has two modes: `RunAsMain` (full app with UI, database, games) and client mode (minimal services for IPC).

### MnemonicDB Data Models

Data persistence uses MnemonicDB — an immutable, entity-attribute-value database. Models are defined as partial classes implementing `IModelDefinition` with static attribute fields:

```csharp
public partial class LoadoutItem : IModelDefinition
{
    private const string Namespace = "NexusMods.Loadouts.LoadoutItem";
    public static readonly StringAttribute Name = new(Namespace, nameof(Name));
    public static readonly MarkerAttribute Disabled = new(Namespace, nameof(Disabled)) { IsIndexed = true };
    public static readonly ReferenceAttribute<Loadout> Loadout = new(Namespace, nameof(Loadout)) { IsIndexed = true };
}
```

Key concepts:
- **Attribute types**: `StringAttribute`, `MarkerAttribute`, `ReferenceAttribute<T>`, `HashAttribute`, `SizeAttribute`, `GamePathParentAttribute`
- **`[Include<T>]`** on a model inherits all attributes from T (model inheritance)
- **`ReadOnly` partial struct** inside model definitions for typed query results
- **`ITxFunction`** implementations for complex transactional updates (read from `IDb basis`, write to `ITransaction tx`)
- Each model has a generated `.New()` constructor and `FindBy*` static methods

### Game Plugin System

Games implement `IGame` and `IGameData<T>` and register via `AddGame<T>()`. Each game provides:
- `GameId`, `DisplayName`, `NexusModsGameId`
- `StoreIdentifiers` (Steam, GOG, Xbox, Epic app IDs)
- `LibraryItemInstallers` — ordered chain of installers to try
- `DiagnosticEmitters` — health checks and warnings
- `Synchronizer` — game-specific `ILoadoutSynchronizer`
- `GetLocations()` — maps `LocationId` → `AbsolutePath` for game directories

`GamePath` combines a `LocationId` (Game, SaveData, Config, etc.) with a relative path for portable file references.

### Loadout & Synchronization

The core mod management loop:
1. **Loadout** — immutable snapshot of all installed mods, their files, and load order
2. **Synchronizer** — three-way diff: previous disk state vs. current game folder vs. desired loadout
3. **Apply** — writes the diff to disk (backs up originals, deploys mod files)
4. **SynchronizerService** — serializes sync operations via semaphore, exposes observable status

Loadout data hierarchy: `Loadout` → `LoadoutItemGroup` (mod) → `LoadoutItem` → `LoadoutFile` (individual file with hash/size/path).

### UI Architecture

Avalonia MVVM with interface-based ViewModels:
- ViewModels inherit `AViewModel<TInterface>` and implement `IXxxViewModel`
- Registration: `AddViewModel<Impl, IInterface>()` + `AddView<View, IInterface>()`
- `IPageFactory` implementations create pages dynamically
- `IViewLocator` resolves Views from ViewModel types
- TreeDataGrid for hierarchical file displays
- R3 observables and DynamicData for reactive collections

### CLI Commands

Defined in `NexusMods.App.Cli` using attributes:
- `[Verb("name", "description")]` on static methods
- `[Option("short", "long", "description")]` for arguments
- `[Injected]` for DI-resolved parameters
- Custom `IOptionParser<T>` implementations for type conversion

## Test Framework

- **xUnit** with `Xunit.DependencyInjection` for constructor-injected services
- **NSubstitute** for mocking, **FluentAssertions** for assertions, **AutoFixture** for test data
- **Verify** (snapshot testing) with `.verified.` files checked into source
- **`AGameTest<TGame>`** base class in `NexusMods.Games.TestFramework` provides pre-configured DI with game installations, file stores, loadout managers
- `NexusMods.StandardGameLocators.TestHelpers` stubs game detection for CI environments

## Code Style

- .NET 9, C# with nullable reference types enabled, implicit usings
- UTF-8, LF line endings, 4-space indentation (see `.editorconfig`)
- Centralized NuGet versions in `Directory.Packages.props`
- Global analyzer rules in `.globalconfig`: un-awaited tasks are errors (`CS4014`), missing switch cases are errors (`CS8509`)
