# NexusMods.App - Cyberpunk 2077 (Linux/Steam)

This is a specialized fork of the NexusMods.App, focused exclusively on managing mods for **Cyberpunk 2077** on **Linux** via **Steam**. It provides mod installation, load order management, conflict resolution, and synchronization with the game directory.

## Project Overview

- **Primary Goal:** A modern, cross-platform (but Linux-focused in this fork) mod manager for Cyberpunk 2077.
- **Main Technologies:**
    - **Language/Runtime:** C# / .NET 10.
    - **UI Framework:** Avalonia UI (MVVM with ReactiveUI/R3).
    - **Database:** MnemonicDB (an immutable, entity-attribute-value database).
    - **App Packaging:** PupNet (for AppImage generation).
- **Key Constraints:**
    - Supports **Cyberpunk 2077** only.
    - Supports **Steam** on **Linux** only.
    - Removed support for GOG, EGS, and Windows/macOS.

## Architecture

The solution (`NexusMods.App.sln`) is organized into a modular, layered architecture:

- **`NexusMods.App`**: Entry point. Wires up Dependency Injection and starts the Avalonia UI or CLI.
- **`NexusMods.App.UI`**: UI layer containing Avalonia views and ViewModels.
- **`NexusMods.Backend`**: Core Linux-specific services, file extraction, and Steam game localization.
- **`NexusMods.DataModel`**: Persistence layer using MnemonicDB, handling synchronization and loadouts.
- **`NexusMods.Games.RedEngine`**: The Cyberpunk 2077 game implementation.
- **`NexusMods.Abstractions.*`**: Interfaces and contracts for loose coupling between layers.
- **`NexusMods.Sdk`**: Shared utilities and configuration infrastructure.

## Building and Running

Common development tasks are managed via the `dev.sh` script or standard .NET CLI commands.

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

- **Dependency Injection**: Every subsystem registers its own services using a static `Add*` extension method in a `Services.cs` file.
- **Data Models**: Defined as partial classes implementing `IModelDefinition` with static attribute fields (e.g., `StringAttribute`, `ReferenceAttribute`). These are processed by source generators.
- **Transactions**: Complex database updates must implement `ITxFunction` for transactional safety.
- **Reactive Programming**: Uses **R3** and **DynamicData** for reactive UI state and collection management.
- **Testing**:
    - Uses **xUnit** with **NSubstitute** for mocking.
    - Snapshot testing is performed with **Verify**.
    - Game-specific tests should inherit from `AGameTest<TGame>`.
- **Code Style**:
    - Nullable reference types are enabled.
    - Implicit usings are used throughout.
    - Adheres to `.editorconfig` (4-space indentation, LF endings).
    - Strict compiler rules: Un-awaited tasks and missing switch cases are treated as errors.

## Key Files & Directories

- `src/`: Main source code projects.
- `tests/`: Unit and integration tests.
- `Directory.Packages.props`: Centralized NuGet package version management.
- `global.json`: Defines the required .NET SDK version (10.0.0).
- `dev.sh`: Interactive utility script for common development tasks.
- `CLAUDE.md`: Detailed guidance for AI assistants working on this codebase.
