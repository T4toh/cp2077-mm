# Cyberpunk 2077 Mod Manager (Linux/Steam)

Specialized fork of NexusMods.App for managing **Cyberpunk 2077** mods on **Linux** via **Steam (Proton)**. The upstream repository was discontinued; this fork continues development focused on a single game and platform.

## Project Overview

- **Primary Goal:** Native Linux mod manager for Cyberpunk 2077 that handles Proton/Wine complexities.
- **Main Technologies:**
    - **Language/Runtime:** C# / .NET 10.
    - **UI Framework:** Avalonia UI (MVVM with ReactiveUI/R3).
    - **Database:** MnemonicDB (an immutable, entity-attribute-value database).
    - **App Packaging:** PupNet (for AppImage generation).
- **App ID:** `com.cyberpunk2077.modmanager`
- **Data Directory:** `~/.local/share/NexusMods.App.Cyberpunk/` (isolated from official app, shared downloads)
- **Key Constraints:**
    - Supports **Cyberpunk 2077** only (Steam App ID `1091500`).
    - Supports **Steam** on **Linux** only. Game locators: `SteamLocator` + `ManuallyAddedLocator`.
    - Removed support for GOG, EGS, Windows/macOS, and all other games.
    - **No telemetry** — Matomo, Mixpanel, and OpenTelemetry completely removed.

## Architecture (81 projects: 49 src + 29 test + 3 other)

The solution (`NexusMods.App.sln`) is organized into a modular, layered architecture:

- **`NexusMods.App`**: Entry point. Wires up Dependency Injection and starts the Avalonia UI or CLI.
- **`NexusMods.App.UI`**: UI layer with Avalonia views and ViewModels. Includes Storage Manager page with Deep Clean UI.
- **`NexusMods.Backend`**: Linux interop, file extraction (`SignatureChecker` for magic bytes), game locators (Steam + manual).
- **`NexusMods.DataModel`**: MnemonicDB persistence, synchronizer, loadout manager, `StorageAnalyzer` (Deep Clean + storage cleanup).
- **`NexusMods.Games.RedEngine`**: Cyberpunk 2077 implementation — `EssentialMods` (7 tracked mods), `CyberpunkDeepCleanTool`, diagnostic emitters (core mods, redundant folders, Wine prefix, REDmod, pattern-based dependencies).
- **`NexusMods.Collections`**: Collection download/install with MD5 rescan and free-user download via curl.
- **`NexusMods.Networking.NexusWebApi`**: Nexus Mods API + `FirefoxCookieReader` for cookie-based direct downloads.
- **`NexusMods.Sdk`**: Shared utilities, `WineParser` (Lutris/WINEDLLOVERRIDES support), `Md5Value`, settings.
- **`NexusMods.Abstractions.*`**: Interfaces and contracts (21 projects).
- **`NexusMods.Library`**: Mod library management with empty file detection on download.

### Fork-Specific Features

1. **Cookie-based free downloads**: Firefox cookies + curl bypass for Cloudflare TLS fingerprinting
2. **Deep Clean tool**: Backup mod directories, clean DB, rescan game folder
3. **Storage Manager**: Granular cleanup (archives, backups, downloads, mod groups)
4. **Essential mods diagnostics**: Tracks Redscript, RED4ext, CET, ArchiveXL, TweakXL, Codeware, Equipment-EX
5. **MD5 rescan**: Detect already-downloaded files to avoid re-downloads
6. **Wine/Lutris support**: Parse Lutris configs, DLL overrides, winetricks.log
7. **Manual game locator**: User-specified game path and Wine prefix
8. **Magic bytes detection**: Archive type detection by file headers
9. **Collection resilience**: Auto re-download missing archives, partial apply, crash-free loading

## Building and Running

Common development tasks are managed via the `dev.sh` script (Spanish menu) or standard .NET CLI commands.

| Task | Command |
| :--- | :--- |
| **Build** | `dotnet build` |
| **Run App** | `dotnet run --project src/NexusMods.App/NexusMods.App.csproj` |
| **Run All Tests** | `dotnet test` |
| **Run Safe Tests** | `dotnet test --filter "RequiresNetworking!=True&FlakeyTest!=True"` |
| **Run Game Tests** | `dotnet test tests/Games/NexusMods.Games.RedEngine.Tests` |
| **Clean Project** | `dotnet clean` |
| **Generate AppImage** | `./dev.sh` (Option 10) - Requires `pupnet` tool. |

## Development Conventions

- **Dependency Injection**: Every subsystem registers its own services using a static `Add*` extension method in a `Services.cs` file. The root chain is in `src/NexusMods.App/Services.cs`.
- **Data Models**: Defined as partial classes implementing `IModelDefinition` with static attribute fields (e.g., `StringAttribute`, `ReferenceAttribute`). Processed by source generators.
- **Transactions**: Complex database updates must implement `ITxFunction` for transactional safety.
- **Reactive Programming**: Uses **R3** and **DynamicData** for reactive UI state and collection management.
- **Testing**:
    - Uses **xUnit** with `Xunit.DependencyInjection` and **NSubstitute** for mocking.
    - Snapshot testing with **Verify** (`.verified.*` files committed to source).
    - Game-specific tests inherit from `AGameTest<TGame>`.
- **Code Style**:
    - Nullable reference types enabled, implicit usings.
    - Adheres to `.editorconfig` (4-space indentation, LF endings, UTF-8).
    - Strict compiler rules: Un-awaited tasks (`CS4014`) and missing switch cases (`CS8509`) are errors.
    - Log messages and some UI strings are in Spanish (personal fork).

## Key Files & Directories

- `src/`: Main source code projects (49 projects).
- `tests/`: Unit and integration tests (29 projects).
- `Directory.Packages.props`: Centralized NuGet package version management.
- `global.json`: Defines the required .NET SDK version (10.0.0).
- `dev.sh`: Interactive utility script with Spanish menu for common development tasks.
- `CLAUDE.md`: Detailed guidance for AI assistants working on this codebase.
- `src/NexusMods.Games.RedEngine/Cyberpunk2077/`: Game-specific implementation, diagnostics, essential mods, deep clean.
- `src/NexusMods.Networking.NexusWebApi/FirefoxCookieReader.cs`: Cookie extraction for direct downloads.
- `src/NexusMods.DataModel/Storage/StorageAnalyzer.cs`: Deep Clean and storage management logic.
