# Lista de archivos originales sin la base de Nexus (pieza 1) — Plan de implementación

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Que cada instalación tenga su propia lista de archivos originales (`GameBaselineFile`), armada desde la base de Nexus cuando conoce la versión y desde el disco cuando no, y que el sincronizador, el reset, Deep Clean y la UI lean solo esa lista.

**Architecture:** Modelo MnemonicDB nuevo `GameBaselineFile` (ruta, hash, tamaño por instalación) más una marca `GameInstallMetadata.BaselineFromDisk`. `ALoadoutSynchronizer.Synchronize` arma o rearma la lista antes de construir el árbol (sin marca, o cuando cambian los IDs de manifest). La regla de la foto es una función pura (`BaselineRule.Apply`). El layer 0 de `Synchronizer.sql` pasa a leer `GameBaselineFile`. Un botón "Actualicé el juego" en Mis juegos rearma la lista a pedido.

**Tech Stack:** C# / .NET 10, MnemonicDB (modelos `IModelDefinition` con source generator), DuckDB SQL (`Synchronizer.sql`), Avalonia + ReactiveUI, xUnit v3 + FluentAssertions.

**Spec:** `docs/superpowers/specs/2026-10-04-vanilla-baseline-design.md` (incluye "Ajustes del plan").

## Global Constraints

- Repo `/home/tatoh/Repos/tModManager`, rama `feat/vanilla-baseline` (ya creada, con la spec commiteada). Un PR al final; el merge lo hace Tatoh.
- Entorno: `export PATH=$HOME/.dotnet:$PATH DOTNET_ROLL_FORWARD=Major`.
- Build con `dotnet build <proyecto> -p:TreatWarningsAsErrors=true`: 0 warnings. `.globalconfig` hace error de `CS4014`, `CS8509`, `CA1069`, `CA2211`, `CA2021`: no suprimir.
- Tests: un proyecto por vez (`dotnet test tests/<Proyecto> --filter "..."`). **Nunca** `dotnet test` sobre la solución entera. La suite completa es `printf '4\n\n0\n' | ./dev.sh` (secuencial, warnings como errores).
- Snapshots de Verify (`*.verified.*`): nunca borrarlos a mano. Si un test genera `*.received.*`, revisar el diff; si es esperado, copiar el received sobre el verified.
- Commits en inglés (`feat:`/`fix:`/`test:`/`docs:`), **sin** `Co-Authored-By` y sin pie de Claude (regla de la organización). PR sin pie de Claude.
- Logs y strings de UI nuevos en español.
- Nada específico de Cyberpunk fuera de `NexusMods.Games.RedEngine`; nada que asuma que Nexus es la única fuente.
- Destructivo: nunca `DeleteDirectory(recursive: true)`; todo cambio que borre archivos lleva un test con symlink apuntando afuera.
- "Nexus conoce la versión" = `store == GameStore.Steam && fileHashes.UnknownLocatorIds(store, ids).Length == 0`. Es la única definición; no inventar otra.

## Review Focus

1. **Parche de Steam que pisa un archivo que un mod también pisa** (Steam restaura el original nuevo encima del mod): lo esperable es que no se borre nada y que la lista adopte el original nuevo; el mod se vuelve a desplegar o, como mucho, termina en External Changes. → Test `SteamPatchOverModdedFile_DeletesNothingAndAdoptsNewVanilla` en Task 3.
2. **Dejar de gestionar y volver a gestionar**: la lista y la marca vieja no pueden sobrevivir (si no, el juego re-gestionado nunca rearma su lista). → Test `UnManage_ClearsListAndMarker` en Task 3.
3. **Loadout que borra a propósito un original** (`Deleted` en layer 1/2): rearmar la lista no puede olvidar ese original, o el reset ya no lo restaura. → Caso `DeletedOnPurpose_KeepsPreviousEntry` en Task 2.
4. **Archivo intrínseco que genera la app** (layer 3, p. ej. el `modlist` de REDmod): no puede entrar a la lista como original. → Caso `IntrinsicPath_KeepsPreviousEntry` en Task 2 (el sincronizador lo marca como propio en Task 3).
5. **Reset con una carpeta symlink dentro del juego**: el reset ahora lee la lista; no debe seguir el link ni borrar lo de afuera. → Test `Reset_WithSymlinkedFolder_LeavesOutsideUntouched` en Task 4.

---

### Task 1: Modelo `GameBaselineFile` + marca en la instalación

**Files:**
- Create: `src/NexusMods.Abstractions.Loadouts/Models/GameBaselineFile.cs`
- Modify: `src/NexusMods.Sdk/Games/Models/GameInstallMetadata.cs` (atributo nuevo al final)
- Modify: `src/NexusMods.DataModel/Services.cs:108` (registrar el modelo)
- Test: `tests/NexusMods.DataModel.Synchronizer.Tests/VanillaBaselineTests.cs` (nuevo)
- Modify (snapshot): `tests/NexusMods.DataModel.SchemaVersions.Tests/Schema.verified.md`

**Interfaces:**
- Produces:
  - `GameBaselineFile` (namespace `NexusMods.Abstractions.Loadouts`) con atributos `Game` (`ReferenceAttribute<GameInstallMetadata>`, propiedad generada `GameId`), `Path` (`GamePathParentAttribute`), `Hash` (`HashAttribute`), `Size` (`SizeAttribute`); `GameBaselineFile.New`, `GameBaselineFile.FindByGame(IDb, EntityId)`.
  - `public static bool GameBaselineFile.TryGetVanillaFiles(GameInstallMetadata.ReadOnly metadata, out IReadOnlyList<GameBaselineFile.ReadOnly> files)`: `false` si la instalación no tiene la marca.
  - `GameInstallMetadata.BaselineFromDisk` (`BooleanAttribute`, opcional): presente = lista armada; `true` = salió del disco.
  - Extensión DI `AddGameBaselineFileModel()` (generada).

- [ ] **Step 1: Escribir el test que falla**

`tests/NexusMods.DataModel.Synchronizer.Tests/VanillaBaselineTests.cs`:

```csharp
using FluentAssertions;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Games.TestFramework;
using NexusMods.Hashing.xxHash3;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

/// <summary>
/// The vanilla file list of an installation (<see cref="GameBaselineFile"/>): built from the Nexus hash database when
/// it knows the version, from the disk when it doesn't, and the only thing the synchronizer treats as original.
/// </summary>
public class VanillaBaselineTests(ITestOutputHelper helper) : ACyberpunkIsolatedGameTest<VanillaBaselineTests>(helper)
{
    [Fact]
    public async Task TryGetVanillaFiles_IsFalseUntilTheListIsBuilt()
    {
        await LoadoutManager.ManageInstallation(GameInstallation);
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        GameBaselineFile.TryGetVanillaFiles(metadata, out _).Should().BeFalse();

        var path = new GamePath(LocationId.Game, "bin/x64/original.exe");
        using (var tx = Connection.BeginTransaction())
        {
            _ = new GameBaselineFile.New(tx)
            {
                Path = path.ToGamePathParentTuple(metadata.Id),
                Hash = "original".xxHash3AsUtf8(),
                Size = Size.FromLong(8),
                GameId = metadata.Id,
            };
            tx.Add(metadata.Id, GameInstallMetadata.BaselineFromDisk, true);
            await tx.Commit();
        }

        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files).Should().BeTrue();
        files.Select(f => (GamePath)f.Path).Should().Equal(path);
    }
}
```

- [ ] **Step 2: Correr y ver que no compila**

Run: `dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~VanillaBaselineTests"`
Expected: error de compilación `GameBaselineFile` / `BaselineFromDisk` no existen.

- [ ] **Step 3: Implementar**

`src/NexusMods.Abstractions.Loadouts/Models/GameBaselineFile.cs`:

```csharp
using NexusMods.MnemonicDB.Abstractions;
using NexusMods.MnemonicDB.Abstractions.Attributes;
using NexusMods.MnemonicDB.Abstractions.Models;
using NexusMods.Sdk.Games;
using NexusMods.Sdk.Hashes;

namespace NexusMods.Abstractions.Loadouts;

/// <summary>
/// One original file of a game installation: what the synchronizer keeps (layer 0), the reset restores and Deep Clean
/// never touches. Filled from the Nexus hash database when it knows the installed version, otherwise from the disk
/// (see <see cref="GameInstallMetadata.BaselineFromDisk"/>), so no part of the app needs Nexus to know the game.
/// </summary>
public partial class GameBaselineFile : IModelDefinition
{
    private const string Namespace = "NexusMods.Loadouts.GameBaselineFile";

    /// <summary>The installation this file belongs to.</summary>
    public static readonly ReferenceAttribute<GameInstallMetadata> Game = new(Namespace, nameof(Game));

    /// <summary>Where the file lives.</summary>
    public static readonly GamePathParentAttribute Path = new(Namespace, nameof(Path));

    /// <summary>The original content's hash.</summary>
    public static readonly HashAttribute Hash = new(Namespace, nameof(Hash));

    /// <summary>The original content's size.</summary>
    public static readonly SizeAttribute Size = new(Namespace, nameof(Size));

    /// <summary>
    /// The installation's original files, or false when the list hasn't been built yet (never synchronized since this
    /// existed). Without a list every game file looks like a leftover, so callers must not delete anything.
    /// </summary>
    public static bool TryGetVanillaFiles(GameInstallMetadata.ReadOnly metadata, out IReadOnlyList<ReadOnly> files)
    {
        if (!metadata.Contains(GameInstallMetadata.BaselineFromDisk))
        {
            files = [];
            return false;
        }

        files = FindByGame(metadata.Db, metadata).ToArray();
        return true;
    }
}
```

Al final de la clase en `src/NexusMods.Sdk/Games/Models/GameInstallMetadata.cs`:

```csharp
    /// <summary>
    /// Set once the installation's original file list (GameBaselineFile) has been built: true when it came from the
    /// disk because the Nexus hash database doesn't know the installed version, false when it came from that database.
    /// </summary>
    public static readonly BooleanAttribute BaselineFromDisk = new(Namespace, nameof(BaselineFromDisk)) { IsOptional = true };
```

En `src/NexusMods.DataModel/Services.cs`, debajo de `coll.AddDiskStateEntryModel();`:

```csharp
        coll.AddGameBaselineFileModel();
```

- [ ] **Step 4: Correr el test**

Run: `dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~VanillaBaselineTests"`
Expected: PASS. Si `FindByGame` no se genera (el generador solo crea `FindBy*` para atributos indexados o referencias), agregar `{ IsIndexed = true }` a `Game` y volver a correr.

- [ ] **Step 5: Actualizar el snapshot de esquema**

Run: `dotnet test tests/NexusMods.DataModel.SchemaVersions.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
Expected: falla el test de esquema con un `Schema.received.md`. Revisar el diff: solo tienen que aparecer `NexusMods.Loadouts.GameBaselineFile/{Game,Path,Hash,Size}` y `NexusMods.Loadouts.GameMetadata/BaselineFromDisk`. Copiar el received sobre `Schema.verified.md` y volver a correr: PASS. Si algún test pide subir la versión de esquema, **parar y avisar** (la spec dice que no hace falta).

- [ ] **Step 6: Commit**

```bash
git add src/NexusMods.Abstractions.Loadouts/Models/GameBaselineFile.cs src/NexusMods.Sdk/Games/Models/GameInstallMetadata.cs src/NexusMods.DataModel/Services.cs tests/NexusMods.DataModel.Synchronizer.Tests/VanillaBaselineTests.cs tests/NexusMods.DataModel.SchemaVersions.Tests/Schema.verified.md
git commit -m "feat: add the per-installation vanilla file list model"
```

---

### Task 2: Regla de la foto (`BaselineRule.Apply`)

**Files:**
- Create: `src/NexusMods.Abstractions.Loadouts.Synchronizers/BaselineRule.cs`
- Test: `tests/NexusMods.DataModel.Synchronizer.Tests/BaselineRuleTests.cs` (nuevo)

**Interfaces:**
- Consumes: nada de Task 1.
- Produces: `public static Dictionary<GamePath, (Hash Hash, Size Size)> BaselineRule.Apply(IReadOnlyDictionary<GamePath, (Hash Hash, Size Size)> previous, IEnumerable<(GamePath Path, Hash Hash, Size Size)> disk, IReadOnlyDictionary<GamePath, Hash?> owned)` (namespace `NexusMods.Abstractions.Loadouts.Synchronizers`). `owned`: rutas que pertenecen al loadout; valor = hash del archivo de mod, o `null` para External Changes, borrados a propósito e intrínsecos (se conserva la entrada anterior sin mirar el hash).

- [ ] **Step 1: Escribir los tests que fallan**

`tests/NexusMods.DataModel.Synchronizer.Tests/BaselineRuleTests.cs`:

```csharp
using FluentAssertions;
using NexusMods.Abstractions.Loadouts.Synchronizers;
using NexusMods.Hashing.xxHash3;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

public class BaselineRuleTests
{
    private static GamePath P(string path) => new(LocationId.Game, path);
    private static (Hash, Size) F(string content) => (content.xxHash3AsUtf8(), Size.FromLong(content.Length));

    private static Dictionary<GamePath, (Hash Hash, Size Size)> Apply(
        Dictionary<GamePath, (Hash Hash, Size Size)> previous,
        (string Path, string Content)[] disk,
        Dictionary<GamePath, Hash?> owned) =>
        BaselineRule.Apply(previous, disk.Select(d => (P(d.Path), d.Content.xxHash3AsUtf8(), Size.FromLong(d.Content.Length))), owned);

    [Fact]
    public void FirstTime_EverythingOnDiskIsOriginal()
    {
        var result = Apply([], [("bin/a.exe", "a"), ("r6/b.ini", "b")], []);
        result.Should().BeEquivalentTo(new Dictionary<GamePath, (Hash, Size)> { [P("bin/a.exe")] = F("a"), [P("r6/b.ini")] = F("b") });
    }

    [Fact]
    public void ModOwnedPathWithModContent_KeepsPreviousEntry()
    {
        var result = Apply(new() { [P("r6/b.ini")] = F("vanilla") }, [("r6/b.ini", "mod")], new() { [P("r6/b.ini")] = "mod".xxHash3AsUtf8() });
        result[P("r6/b.ini")].Should().Be(F("vanilla"));
    }

    [Fact]
    public void ModOwnedPathWithoutPreviousEntry_StaysOut()
    {
        var result = Apply([], [("archive/pc/mod/a.archive", "mod")], new() { [P("archive/pc/mod/a.archive")] = "mod".xxHash3AsUtf8() });
        result.Should().BeEmpty();
    }

    [Fact]
    public void ModOwnedPathOverwrittenByAPatch_AdoptsTheNewContent()
    {
        var result = Apply(new() { [P("r6/b.ini")] = F("v1") }, [("r6/b.ini", "v2")], new() { [P("r6/b.ini")] = "mod".xxHash3AsUtf8() });
        result[P("r6/b.ini")].Should().Be(F("v2"));
    }

    [Fact]
    public void ExternalChange_KeepsPreviousEntryWhateverTheDiskHas()
    {
        var result = Apply(new() { [P("r6/b.ini")] = F("vanilla") }, [("r6/b.ini", "edited by hand")], new() { [P("r6/b.ini")] = null });
        result[P("r6/b.ini")].Should().Be(F("vanilla"));
    }

    [Fact]
    public void DeletedOnPurpose_KeepsPreviousEntry()
    {
        var result = Apply(new() { [P("bin/intro.bk2")] = F("intro") }, [], new() { [P("bin/intro.bk2")] = null });
        result[P("bin/intro.bk2")].Should().Be(F("intro"));
    }

    [Fact]
    public void IntrinsicPath_KeepsPreviousEntry()
    {
        var result = Apply([], [("r6/cache/modded/mods.json", "generated")], new() { [P("r6/cache/modded/mods.json")] = null });
        result.Should().BeEmpty();
    }

    [Fact]
    public void PatchedOriginal_IsAdopted()
    {
        var result = Apply(new() { [P("bin/a.exe")] = F("v1") }, [("bin/a.exe", "v2")], []);
        result[P("bin/a.exe")].Should().Be(F("v2"));
    }

    [Fact]
    public void OriginalGoneFromDisk_LeavesTheList()
    {
        var result = Apply(new() { [P("bin/old.dll")] = F("old") }, [], []);
        result.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Correr y ver que no compila**

Run: `dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~BaselineRuleTests"`
Expected: error de compilación, `BaselineRule` no existe.

- [ ] **Step 3: Implementar**

`src/NexusMods.Abstractions.Loadouts.Synchronizers/BaselineRule.cs`:

```csharp
using NexusMods.Hashing.xxHash3;
using NexusMods.Paths;
using NexusMods.Sdk.Games;

namespace NexusMods.Abstractions.Loadouts.Synchronizers;

/// <summary>
/// Builds an installation's original file list from the disk when the Nexus hash database doesn't know the version.
/// Errs on the side of "original": a file wrongly kept as original is never deleted, a file wrongly dropped would be.
/// </summary>
public static class BaselineRule
{
    /// <param name="previous">The list before this run (empty the first time).</param>
    /// <param name="disk">The freshly indexed disk state.</param>
    /// <param name="owned">
    /// Paths the loadout owns. The value is the mod file's hash, or null for paths whose previous entry is kept whatever
    /// the disk holds (External Changes, files deleted on purpose, files the app generates).
    /// </param>
    public static Dictionary<GamePath, (Hash Hash, Size Size)> Apply(
        IReadOnlyDictionary<GamePath, (Hash Hash, Size Size)> previous,
        IEnumerable<(GamePath Path, Hash Hash, Size Size)> disk,
        IReadOnlyDictionary<GamePath, Hash?> owned)
    {
        var result = new Dictionary<GamePath, (Hash Hash, Size Size)>();

        foreach (var (path, hash, size) in disk)
        {
            // What's on disk is the loadout's, not the game's: the original (if known) is the previous entry
            if (owned.TryGetValue(path, out var ownedHash) && (ownedHash is null || ownedHash == hash))
            {
                if (previous.TryGetValue(path, out var kept)) result[path] = kept;
                continue;
            }

            result[path] = (hash, size);
        }

        // Owned paths missing from disk (mod not deployed yet, original deleted on purpose) keep their original
        foreach (var (path, entry) in previous)
        {
            if (!result.ContainsKey(path) && owned.ContainsKey(path))
                result[path] = entry;
        }

        return result;
    }
}
```

- [ ] **Step 4: Correr los tests**

Run: `dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~BaselineRuleTests"`
Expected: PASS (9 tests).

- [ ] **Step 5: Commit**

```bash
git add src/NexusMods.Abstractions.Loadouts.Synchronizers/BaselineRule.cs tests/NexusMods.DataModel.Synchronizer.Tests/BaselineRuleTests.cs
git commit -m "feat: add the rule that builds the vanilla list from the disk"
```

---

### Task 3: El sincronizador arma la lista y el layer 0 la lee

**Files:**
- Modify: `src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs` (`Synchronize` ~`:1084`, método nuevo `UpdateBaseline`, helper `OwnedPaths`)
- Modify: `src/NexusMods.Abstractions.Loadouts.Synchronizers/ILoadoutSynchronizer.cs` (declarar `UpdateBaseline`)
- Modify: `src/NexusMods.DataModel/Synchronizer/Synchronizer.sql:67-76` (layer 0)
- Modify: `src/NexusMods.Games.FileHashes/FileHashesQueries.sql:25-48` (borrar macros sin uso)
- Modify: `src/NexusMods.DataModel/LoadoutManager.cs` (`UnManage`, ~`:341-366`)
- Test: `tests/NexusMods.DataModel.Synchronizer.Tests/VanillaBaselineTests.cs` (agregar), nuevo `tests/NexusMods.DataModel.Synchronizer.Tests/SteamVanillaBaselineTests.cs`

**Interfaces:**
- Consumes: `GameBaselineFile`, `GameInstallMetadata.BaselineFromDisk`, `GameBaselineFile.TryGetVanillaFiles` (Task 1); `BaselineRule.Apply` (Task 2).
- Produces: `Task<GameInstallMetadata.ReadOnly> ILoadoutSynchronizer.UpdateBaseline(Loadout.ReadOnly loadout)` (reindexa y rearma la lista; la usa el botón de Task 5).

- [ ] **Step 1: Escribir los tests que fallan**

Agregar a `VanillaBaselineTests` (la tienda de los tests es `Unknown`, así que Nexus nunca "conoce" la versión y vale la regla de la foto):

```csharp
    private AbsolutePath GameFile(string path) => GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, path));

    private async Task<Loadout.ReadOnly> ManagedLoadoutWith(params (string Path, string Content)[] files)
    {
        foreach (var (path, content) in files)
        {
            GameFile(path).Parent.CreateDirectory();
            await GameFile(path).WriteAllTextAsync(content);
        }
        await LoadoutManager.ManageInstallation(GameInstallation);
        return await Synchronizer.Synchronize(await CreateLoadout());
    }

    private GamePath[] ListPaths() =>
        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files)
            ? files.Select(f => (GamePath)f.Path).Order().ToArray()
            : throw new InvalidOperationException("no list");

    [Fact]
    public async Task FirstSync_UnknownVersion_KeepsEveryFileAlreadyThere()
    {
        // A manually added game (or any version Nexus doesn't know): today the first sync deletes these
        await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"), ("r6/config/settings.ini", "vanilla"));

        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
        GameFile("r6/config/settings.ini").FileExists.Should().BeTrue();
        ListPaths().Should().Equal(new GamePath(LocationId.Game, "bin/x64/original.exe"), new GamePath(LocationId.Game, "r6/config/settings.ini"));
        GameInstallMetadata.BaselineFromDisk.Get(GameRegistry.ForceGetMetadata(GameInstallation)).Should().BeTrue();
    }

    [Fact]
    public async Task FileAddedAfterManaging_IsAnExternalChangeNotAnOriginal()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        GameFile("bin/x64/dropped-by-hand.dll").Parent.CreateDirectory();
        await GameFile("bin/x64/dropped-by-hand.dll").WriteAllTextAsync("mod");

        await Synchronizer.Synchronize(loadout.Rebase());

        ListPaths().Should().Equal(new GamePath(LocationId.Game, "bin/x64/original.exe"));
    }

    [Fact]
    public async Task LocatorIdsChange_RebuildsTheListAndAdoptsThePatch()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "v1"));
        var locator = ServiceProvider.GetServices<IGameLocator>().OfType<UniversalStubbedGameLocator<Cyberpunk2077Game>>().Single();
        locator.LocatorIds = [LocatorId.From("unknown-v2")];
        await GameFile("bin/x64/original.exe").WriteAllTextAsync("v2");

        await Synchronizer.Synchronize(loadout.Rebase());

        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files).Should().BeTrue();
        files.Single().Hash.Should().Be("v2".xxHash3AsUtf8());
    }

    [Fact]
    public async Task SteamPatchOverModdedFile_DeletesNothingAndAdoptsNewVanilla()
    {
        var loadout = await ManagedLoadoutWith(("r6/config/settings.ini", "v1"));
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [(RelativePath)"r6/config/settings.ini"], loadout, "ConfigMod");
            await tx.Commit();
        }
        loadout = await Synchronizer.Synchronize(loadout.Rebase());

        var locator = ServiceProvider.GetServices<IGameLocator>().OfType<UniversalStubbedGameLocator<Cyberpunk2077Game>>().Single();
        locator.LocatorIds = [LocatorId.From("unknown-v2")];
        await GameFile("r6/config/settings.ini").WriteAllTextAsync("v2");
        await Synchronizer.Synchronize(loadout.Rebase());

        GameFile("r6/config/settings.ini").FileExists.Should().BeTrue();
        GameBaselineFile.TryGetVanillaFiles(GameRegistry.ForceGetMetadata(GameInstallation), out var files).Should().BeTrue();
        files.Single().Hash.Should().Be("v2".xxHash3AsUtf8());
    }

    [Fact]
    public async Task ExistingInstallWithoutList_GetsOneOnNextSync()
    {
        // Installs managed before this change: no list, no marker
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        using (var tx = Connection.BeginTransaction())
        {
            foreach (var file in GameBaselineFile.FindByGame(metadata.Db, metadata)) tx.Delete(file, recursive: false);
            tx.Retract(metadata.Id, GameInstallMetadata.BaselineFromDisk, true);
            await tx.Commit();
        }

        await Synchronizer.Synchronize(loadout.Rebase());

        ListPaths().Should().Equal(new GamePath(LocationId.Game, "bin/x64/original.exe"));
    }

    [Fact]
    public async Task UnManage_ClearsListAndMarker()
    {
        await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));

        await LoadoutManager.UnManage(GameInstallation, runGc: false, cleanGameFolder: false);

        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        metadata.Contains(GameInstallMetadata.BaselineFromDisk).Should().BeFalse();
        GameBaselineFile.FindByGame(metadata.Db, metadata).Should().BeEmpty();
    }
```

Usings a agregar arriba: `Microsoft.Extensions.DependencyInjection`, `NexusMods.Games.RedEngine.Cyberpunk2077`, `NexusMods.Sdk.Loadouts`, `NexusMods.StandardGameLocators.TestHelpers`, `NexusMods.MnemonicDB.Abstractions.TxFunctions`.

Nuevo `tests/NexusMods.DataModel.Synchronizer.Tests/SteamVanillaBaselineTests.cs` (tienda Steam: el stub de hashes conoce `StubbedGameState.zip`):

```csharp
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Loadouts;
using NexusMods.Backend;
using NexusMods.CrossPlatform;
using NexusMods.Games.Generic;
using NexusMods.Games.RedEngine;
using NexusMods.Games.RedEngine.Cyberpunk2077;
using NexusMods.Games.TestFramework;
using NexusMods.Paths;
using NexusMods.Sdk.Games;
using NexusMods.StandardGameLocators.TestHelpers;
using Xunit;

namespace NexusMods.DataModel.Synchronizer.Tests;

public class SteamVanillaBaselineTests(ITestOutputHelper helper) : AIsolatedGameTest<SteamVanillaBaselineTests, Cyberpunk2077Game>(helper)
{
    protected override IServiceCollection AddServices(IServiceCollection services) => base.AddServices(services)
        .AddOSInterop()
        .AddRuntimeDependencies()
        .AddGenericGameSupport()
        .AddUniversalGameLocator<Cyberpunk2077Game>(new Version("1.61"), stores: [GameStore.Steam])
        .AddRedEngineGames();

    [Fact]
    public async Task KnownVersion_ListIsTheNexusList()
    {
        var preexisting = GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, "bin/x64/preexisting-mod.dll"));
        preexisting.Parent.CreateDirectory();
        await preexisting.WriteAllTextAsync("mod");

        await LoadoutManager.ManageInstallation(GameInstallation);
        await Synchronizer.Synchronize(await CreateLoadout());

        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        GameInstallMetadata.BaselineFromDisk.Get(metadata).Should().BeFalse();
        GameBaselineFile.TryGetVanillaFiles(metadata, out var files).Should().BeTrue();
        var nexus = ServiceProvider.GetRequiredService<NexusMods.Abstractions.Games.FileHashes.IFileHashesService>()
            .GetGameFiles((GameStore.Steam, [LocatorId.From("StubbedGameState.zip")])).Select(f => f.Path);
        files.Select(f => (GamePath)f.Path).Should().BeEquivalentTo(nexus);
        files.Select(f => (GamePath)f.Path).Should().NotContain(new GamePath(LocationId.Game, "bin/x64/preexisting-mod.dll"));
    }
}
```

(Si `AIsolatedGameTest.AddServices` no es `protected virtual`, copiar el patrón exacto de `ACyberpunkIsolatedGameTest`, que lo sobreescribe.)

- [ ] **Step 2: Correr y ver que fallan**

Run: `dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~VanillaBaselineTests|FullyQualifiedName~SteamVanillaBaselineTests"`
Expected: compilan y fallan (todavía nadie arma la lista: `ListPaths()` tira "no list" y los `TryGetVanillaFiles` dan `false`). Para confirmar el riesgo de la spec, comentar temporalmente las líneas de `ListPaths()`/`BaselineFromDisk` de `FirstSync_UnknownVersion_KeepsEveryFileAlreadyThere` y correrlo solo: si falla en `FileExists`, **hoy la primera sincronización borra los archivos** (riesgo confirmado); si pasa, el riesgo no existe tal como lo describe la spec. Restaurar las líneas y anotar el resultado para el PR.

- [ ] **Step 3: Implementar `UpdateBaseline` y los disparadores**

En `ILoadoutSynchronizer.cs`, junto a `ReindexState`:

```csharp
    /// <summary>
    /// Reindexes the game and rebuilds the installation's original file list (GameBaselineFile): from the Nexus hash
    /// database when it knows the installed version, otherwise from the disk (BaselineRule).
    /// </summary>
    Task<Sdk.Games.GameInstallMetadata.ReadOnly> UpdateBaseline(Loadout.ReadOnly loadout);
```

En `ALoadoutSynchronizer.cs`, reemplazar el comienzo de `Synchronize`:

```csharp
    public virtual async Task<Loadout.ReadOnly> Synchronize(Loadout.ReadOnly loadout, SynchronizeLoadoutJob? job = null)
    {
        loadout = loadout.Rebase();
        var previousLocatorIds = loadout.LocatorIds.ToHashSet();

        // Update locator IDs before building the sync tree
        loadout = await UpdateLocatorIds(loadout);

        // No list yet (new or pre-existing install) or a new game version: rebuild it before anything is compared
        if (!loadout.Installation.Contains(Sdk.Games.GameInstallMetadata.BaselineFromDisk) || !previousLocatorIds.SetEquals(loadout.LocatorIds))
        {
            await UpdateBaseline(loadout);
            loadout = loadout.Rebase();
        }
```

(el resto del método queda igual). Agregar en la misma clase, cerca de `ReindexState`:

```csharp
    /// <inheritdoc />
    public async Task<Sdk.Games.GameInstallMetadata.ReadOnly> UpdateBaseline(Loadout.ReadOnly loadout)
    {
        var metadata = await ReindexState(loadout.InstallationInstance);
        var store = metadata.Store;
        var locatorIds = loadout.LocatorIds.Distinct().ToArray();
        var nexusKnows = store == GameStore.Steam && _fileHashService.UnknownLocatorIds(store, locatorIds).Length == 0;

        IEnumerable<(GamePath Path, Hash Hash, Size Size)> files;
        if (nexusKnows)
        {
            files = _fileHashService.GetGameFiles((store, locatorIds)).Select(f => (f.Path, f.Hash, f.Size));
        }
        else
        {
            var previous = new Dictionary<GamePath, (Hash Hash, Size Size)>();
            foreach (var entry in GameBaselineFile.FindByGame(metadata.Db, metadata))
                previous[entry.Path] = (entry.Hash, entry.Size);
            var disk = DiskStateEntry.FindByGame(metadata.Db, metadata).Select(e => ((GamePath)e.Path, e.Hash, e.Size));
            files = BaselineRule.Apply(previous, disk, OwnedPaths(loadout.Rebase())).Select(kv => (kv.Key, kv.Value.Hash, kv.Value.Size));
        }

        using var tx = Connection.BeginTransaction();
        foreach (var old in GameBaselineFile.FindByGame(metadata.Db, metadata))
            tx.Delete(old, recursive: false);
        var count = 0;
        foreach (var (path, hash, size) in files)
        {
            _ = new GameBaselineFile.New(tx)
            {
                Path = path.ToGamePathParentTuple(metadata.Id),
                Hash = hash,
                Size = size,
                GameId = metadata.Id,
            };
            count++;
        }
        tx.Add(metadata.Id, Sdk.Games.GameInstallMetadata.BaselineFromDisk, !nexusKnows);
        await tx.Commit();

        Logger.LogInformation("Lista de archivos originales de {Game}: {Count} archivos, desde {Source}",
            loadout.InstallationInstance.Game.DisplayName, count, nexusKnows ? "la base de Nexus" : "el disco");
        return Sdk.Games.GameInstallMetadata.Load(Connection.Db, metadata.Id);
    }

    /// <summary>
    /// Paths the loadout owns, for <see cref="BaselineRule"/>: mod files with their hash; External Changes, files
    /// deleted on purpose and files the app generates with null (their previous original is kept as is).
    /// </summary>
    private Dictionary<GamePath, Hash?> OwnedPaths(Loadout.ReadOnly loadout)
    {
        var owned = new Dictionary<GamePath, Hash?>();
        foreach (var row in WinningFilesQuery(loadout.Db, loadout))
        {
            var path = new GamePath(row.Location, row.Path);
            switch (ToItemType(row.ItemType))
            {
                case LoadoutSourceItemType.Loadout: owned[path] = row.Hash; break;
                case LoadoutSourceItemType.Deleted:
                case LoadoutSourceItemType.Intrinsic: owned[path] = null; break;
                case LoadoutSourceItemType.Game: break;
            }
        }

        foreach (var overrides in LoadoutOverridesGroup.FindByOverridesFor(loadout.Db, loadout.Id))
        {
            foreach (var item in overrides.AsLoadoutItemGroup().Children.OfTypeLoadoutItemWithTargetPath())
                owned[item.TargetPath] = null;
        }

        return owned;
    }
```

Notas para el implementador:
- `WinningFilesQuery` devuelve la tupla `(Id, Hash, Size, Location, Path, ItemType)`; si el nombre del campo de ubicación es `LocationId`, usar ese.
- Si `LoadoutSourceItemType` tiene más miembros que estos cuatro, el `switch` dará `CS8509`: agregarlos sin hacer nada.
- `item.TargetPath` es la tupla de `GamePathParentAttribute`; si no convierte implícito a `GamePath`, usar `(GamePath)item.TargetPath`.

- [ ] **Step 4: Layer 0 desde la lista**

En `Synchronizer.sql`, reemplazar el primer bloque de `all_files` (comentario `-- Game files on Layer 0` hasta antes del primer `UNION`) por:

```sql
  -- Game files on Layer 0: the installation's original file list (GameBaselineFile)
  SELECT
    loadout.Id Loadout,
    NULL Id,
    {Location: baseline.Path.Item2, Path: baseline.Path.Item3} Path,
    baseline.Hash,
    baseline.Size,
    'Game'::synchronizer.ItemType ItemType,
    0 Layer
  FROM MDB_GAMEBASELINEFILE(Db => db) baseline
  INNER JOIN MDB_LOADOUT(Db => db) loadout ON loadout.Installation = baseline.Game
```

Luego `git grep -n 'loadout_files\|steam_loadout_files\|loadout_locatorids' -- src tests`: si el único uso era `Synchronizer.sql`, borrar las tres macros de `FileHashesQueries.sql` (`loadout_locatorids`, `steam_loadout_files`, `loadout_files`, líneas ~25-48). Si hay otro uso, dejarlas.

- [ ] **Step 5: Borrar la lista al dejar de gestionar**

En `LoadoutManager.UnManage`, dentro de la misma transacción que borra las `DiskStateEntry` (después del `foreach` de `DiskStateEntry.FindByGame`):

```csharp
            foreach (var file in GameBaselineFile.FindByGame(metadata.Db, metadata))
                tx.Delete(file, recursive: false);
            if (metadata.Contains(GameInstallMetadata.BaselineFromDisk))
                tx.Retract(metadata, GameInstallMetadata.BaselineFromDisk, GameInstallMetadata.BaselineFromDisk.Get(metadata));
```

- [ ] **Step 6: Correr los tests nuevos**

Run: `dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~VanillaBaselineTests|FullyQualifiedName~SteamVanillaBaselineTests|FullyQualifiedName~BaselineRuleTests"`
Expected: PASS.

- [ ] **Step 7: Correr los proyectos que tocan el sincronizador y revisar snapshots**

Run, uno por vez:
`dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
`dotnet test tests/NexusMods.DataModel.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
`dotnet test tests/Games/NexusMods.Games.RedEngine.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
`dotnet test tests/NexusMods.Collections.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
Expected: PASS. Si falla algún Verify, leer el `.received` contra el `.verified`: es aceptable que cambie **solo** porque archivos del juego que antes se borraban o iban a External Changes ahora se quedan como originales. Cualquier otro diff es un bug: arreglar antes de seguir. Anotar en el PR qué snapshots cambiaron y por qué.

- [ ] **Step 8: Build sin warnings y commit**

Run: `dotnet build src/NexusMods.App -p:TreatWarningsAsErrors=true` → 0 warnings.

```bash
git add -A
git commit -m "feat: synchronizer builds the vanilla list and layer 0 reads it

Layer 0 used to be a SQL join on the Nexus hash database, empty for any version it doesn't know (and always for a
manually added game), so the first sync could back up and delete the game. Each installation now has its own list,
filled from that database when it knows the version and from the disk otherwise, rebuilt when the manifest IDs change."
```

---

### Task 4: Freno, reset, Deep Clean y "¿hay lista?" leen la lista

**Files:**
- Modify: `src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs` (`RunActions` ~`:482-485`, `EnsureVanillaDataKnown` ~`:568-575`, `ResetToOriginalGameState` ~`:1461-1512`)
- Modify: `src/NexusMods.Abstractions.Loadouts.Synchronizers/ILoadoutSynchronizer.cs:124`
- Modify: `src/NexusMods.DataModel/LoadoutManager.cs:282-301` (`DeactivateCurrentLoadout`)
- Modify: `src/NexusMods.Games.RedEngine/Cyberpunk2077/CyberpunkDeepCleanTool.cs` (`ResolveVanilla` `:111-142` se borra; uso en `Execute` ~`:253-277`)
- Modify: `tests/Games/NexusMods.Games.RedEngine.Tests/CyberpunkDeepCleanToolTests.cs` (borrar los dos tests de `ResolveVanilla`, `:96-125`)
- Modify: `src/NexusMods.App.UI/Pages/MyGames/MyGamesViewModel.cs:614-615` (`HasVanillaData`)
- Modify: `tests/NexusMods.DataModel.Synchronizer.Tests/DiskSafetyTests.cs:91-104`
- Test: `tests/NexusMods.DataModel.Synchronizer.Tests/VanillaBaselineTests.cs` (agregar)

**Interfaces:**
- Consumes: `GameBaselineFile.TryGetVanillaFiles` (Task 1), `UpdateBaseline` (Task 3).
- Produces: `Task ILoadoutSynchronizer.ResetToOriginalGameState(GameInstallation installation)` (sin parámetro de IDs).

- [ ] **Step 1: Escribir los tests que fallan**

Agregar a `VanillaBaselineTests`:

```csharp
    [Fact]
    public async Task UnknownVersion_ModOverOriginal_RemovingItRestoresTheOriginal_AndResetKeepsOriginals()
    {
        var loadout = await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla exe"), ("r6/config/settings.ini", "vanilla ini"));
        using (var tx = Connection.BeginTransaction())
        {
            await AddModAsync(tx, [(RelativePath)"r6/config/settings.ini", (RelativePath)"archive/pc/mod/new.archive"], loadout, "ConfigMod");
            await tx.Commit();
        }
        loadout = await Synchronizer.Synchronize(loadout.Rebase());
        (await GameFile("r6/config/settings.ini").ReadAllTextAsync()).Should().Be("r6/config/settings.ini"); // AddModAsync content = path

        var mod = LoadoutItem.FindByLoadout(loadout.Db, loadout).OfTypeLoadoutItemGroup().Single(g => g.AsLoadoutItem().Name == "ConfigMod");
        using (var tx = Connection.BeginTransaction())
        {
            tx.Delete(mod, recursive: true);
            await tx.Commit();
        }
        await Synchronizer.Synchronize(loadout.Rebase());

        (await GameFile("r6/config/settings.ini").ReadAllTextAsync()).Should().Be("vanilla ini");
        GameFile("archive/pc/mod/new.archive").FileExists.Should().BeFalse();

        await Synchronizer.ResetToOriginalGameState(GameInstallation);
        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task Reset_WithSymlinkedFolder_LeavesOutsideUntouched()
    {
        await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        var outside = TemporaryFileManager.CreateFolder().Path;
        var canary = outside.Combine("precious.txt");
        await canary.WriteAllTextAsync("user data");
        File.CreateSymbolicLink(GameFile("bin/linked").ToString(), outside.ToString());

        await Synchronizer.ResetToOriginalGameState(GameInstallation);

        canary.FileExists.Should().BeTrue();
        GameFile("bin/x64/original.exe").FileExists.Should().BeTrue();
    }

    [Fact]
    public async Task DeepCleanAndUi_SeeTheList()
    {
        await ManagedLoadoutWith(("bin/x64/original.exe", "vanilla"));
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        GameBaselineFile.TryGetVanillaFiles(metadata, out var files).Should().BeTrue();
        files.Select(f => (GamePath)f.Path).Should().Contain(new GamePath(LocationId.Game, "bin/x64/original.exe"));
    }
```

Reemplazar `ResetWithUnknownLocatorIds_RefusesAndDeletesNothing` en `DiskSafetyTests.cs` por:

```csharp
    [Fact]
    public async Task ResetWithoutVanillaList_RefusesAndDeletesNothing()
    {
        // No list (never synchronized since the list existed): resetting would delete the whole game
        var loadout = await ManagedLoadout();
        var original = GameInstallation.Locations.ToAbsolutePath(new GamePath(LocationId.Game, "bin/x64/original.exe"));
        original.Parent.CreateDirectory();
        await original.WriteAllTextAsync("vanilla");
        await Synchronizer.Synchronize(loadout);
        var metadata = GameRegistry.ForceGetMetadata(GameInstallation);
        using (var tx = Connection.BeginTransaction())
        {
            tx.Retract(metadata.Id, GameInstallMetadata.BaselineFromDisk, GameInstallMetadata.BaselineFromDisk.Get(metadata));
            await tx.Commit();
        }

        var act = () => Synchronizer.ResetToOriginalGameState(GameInstallation);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*lista de archivos originales*");
        original.FileExists.Should().BeTrue();
    }
```

- [ ] **Step 2: Correr y ver que no compila**

Run: `dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "FullyQualifiedName~VanillaBaselineTests|FullyQualifiedName~DiskSafetyTests"`
Expected: error de compilación (`ResetToOriginalGameState` pide dos argumentos).

- [ ] **Step 3: Implementar**

`EnsureVanillaDataKnown` pasa a mirar la lista:

```csharp
    private static void EnsureVanillaDataKnown(Sdk.Games.GameInstallMetadata.ReadOnly metadata, string operation)
    {
        if (GameBaselineFile.TryGetVanillaFiles(metadata, out _)) return;
        throw new InvalidOperationException(
            $"No se puede {operation}: todavía no hay lista de archivos originales para {metadata.Name}. " +
            "Sin esa lista tModManager borraría archivos del juego, así que no hace nada.");
    }
```

En `RunActions`, reemplazar el freno Steam-only (`:482-485`, con su comentario) por:

```csharp
        // Without the vanilla list every original game file looks like a leftover: deleting waits for the list
        if (syncTree.Values.Any(node => node.Actions.HasFlag(Actions.DeleteFromDisk)))
            EnsureVanillaDataKnown(Sdk.Games.GameInstallMetadata.Load(Connection.Db, gameMetadataId), "borrar archivos del juego");
```

`ResetToOriginalGameState`:

```csharp
    public async Task ResetToOriginalGameState(GameInstallation installation)
    {
        var metadata = await ReindexState(installation);
        // The reset deletes everything that isn't in the vanilla list: with no list, that's the whole game
        EnsureVanillaDataKnown(metadata, "restaurar la carpeta del juego");
        GameBaselineFile.TryGetVanillaFiles(metadata, out var gameState);

        var diskStateEntries = DiskStateEntry.FindByGame(metadata.Db, metadata);
```

y en el `foreach (var gameFile in gameState)` usar `gameFile.Path` como `GamePath` (`desiredState.Add(gameFile.Path, syncNode)`; si no convierte implícito, `(GamePath)gameFile.Path`). El resto del método queda igual. Actualizar la firma en `ILoadoutSynchronizer.cs:124` a `Task ResetToOriginalGameState(GameInstallation installation);`.

En `LoadoutManager.DeactivateCurrentLoadout`, borrar el cálculo de `locatorIds` y su warning, y llamar `await synchronizer.ResetToOriginalGameState(installation);`.

`git grep -n 'ResetToOriginalGameState' -- src tests` y ajustar cualquier otro llamador.

Deep Clean (`CyberpunkDeepCleanTool.Execute`): reemplazar el bloque `try { var resolved = ResolveVanilla(...) ... } catch ...` por:

```csharp
        IReadOnlySet<GamePath> vanilla = new HashSet<GamePath>();
        if (GameBaselineFile.TryGetVanillaFiles(loadout.Installation, out var vanillaFiles))
            vanilla = vanillaFiles.Select(f => (GamePath)f.Path).ToHashSet();
        else
            _logger.LogWarning("Todavía no hay lista de archivos originales; se omite la búsqueda de archivos sueltos de mods");
```

Borrar el método `ResolveVanilla` con su comentario, el campo `_fileHashes` y su parámetro de constructor si quedan sin uso (y los `using` que sobren). Borrar los tests `ResolveVanilla_AllIdsKnown_ReturnsUnionOfFiles` y `ResolveVanilla_OneIdUnknown_ReturnsEmptyAndNamesIt` de `CyberpunkDeepCleanToolTests.cs`.

`MyGamesViewModel.HasVanillaData`:

```csharp
    private bool HasVanillaData(GameInstallation installation) =>
        _gameRegistry.TryGetMetadata(installation, out var metadata) && GameBaselineFile.TryGetVanillaFiles(metadata, out _);
```

(quitar `_fileHashesService` del VM si queda sin uso).

- [ ] **Step 4: Correr los tests**

Run, uno por vez:
`dotnet test tests/NexusMods.DataModel.Synchronizer.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
`dotnet test tests/NexusMods.DataModel.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
`dotnet test tests/Games/NexusMods.Games.RedEngine.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
`dotnet test tests/NexusMods.UI.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"`
Expected: PASS. Mismo criterio de snapshots que en Task 3 Step 7 (los tests de cambio de loadout ahora resetean contra la lista y no contra el stub de Nexus: un diff solo es aceptable si lo explica eso).

- [ ] **Step 5: Verificar que cada test nuevo falla sin su arreglo**

Uno por vez, revertir temporalmente y correr el test correspondiente; debe fallar; restaurar:
- `ResetWithoutVanillaList_RefusesAndDeletesNothing`: en `EnsureVanillaDataKnown`, cambiar el `return` condicional por `return;` incondicional → falla.
- `Reset_WithSymlinkedFolder_LeavesOutsideUntouched`: este test cuida un invariante que ya garantiza el indexado (no entra en carpetas symlink); anotar en el PR que pasa también sin cambios y por qué.

- [ ] **Step 6: Build y commit**

Run: `dotnet build src/NexusMods.App -p:TreatWarningsAsErrors=true` → 0 warnings.

```bash
git add -A
git commit -m "feat: reset, Deep Clean and the apply guard read the vanilla list

The guard now covers every store (it was Steam-only, so a manually added game had none), the reset restores the
installation's list instead of asking the Nexus database again, and Deep Clean drops its own all-or-nothing lookup."
```

---

### Task 5: Botón "Actualicé el juego" y aviso en el widget

**Files:**
- Modify: `src/NexusMods.App.UI/Controls/GameWidget/IGameWidgetViewModel.cs` (comando nuevo)
- Modify: `src/NexusMods.App.UI/Controls/GameWidget/GameWidgetViewModel.cs` (comando por defecto, texto de versión, `IGameRegistry`)
- Modify: `src/NexusMods.App.UI/Controls/GameWidget/GameWidget.axaml` (botón en `ManagedGameGrid`)
- Modify: el `GameWidgetDesignViewModel` si implementa la interfaz (`git grep -n 'IGameWidgetViewModel' -- src`)
- Modify: `src/NexusMods.App.UI/Pages/MyGames/MyGamesViewModel.cs` (cablear el comando, junto a `DeepCleanCommand` ~`:242`)

**Interfaces:**
- Consumes: `ILoadoutSynchronizer.UpdateBaseline(Loadout.ReadOnly)` (Task 3), `GameInstallMetadata.BaselineFromDisk` (Task 1).
- Produces: `ReactiveCommand<Unit, Unit> IGameWidgetViewModel.UpdateBaselineCommand { get; set; }`.

- [ ] **Step 1: Comando en la interfaz y el VM**

`IGameWidgetViewModel.cs`, junto a `DeepCleanCommand`:

```csharp
    public ReactiveCommand<Unit, Unit> UpdateBaselineCommand { get; set; }
```

`GameWidgetViewModel.cs`: en el constructor, junto a los demás comandos vacíos, `UpdateBaselineCommand = ReactiveCommand.Create(() => { });`; propiedad `[Reactive] public ReactiveCommand<Unit, Unit> UpdateBaselineCommand { get; set; }`. Agregar `IGameRegistry gameRegistry` al constructor y cambiar el texto de versión desconocida:

```csharp
                        if (!fileHashesService.TryGetVanityVersion((installation.LocatorResult.Store, locatorIds), out var vanityVersion))
                        {
                            var fromDisk = gameRegistry.TryGetMetadata(installation, out var metadata)
                                && GameInstallMetadata.BaselineFromDisk.TryGetValue(metadata, out var value) && value;
                            return fromDisk ? "Versión desconocida: originales tomados del disco" : Language.GameWidget_VersionUnknown;
                        }
                        return $"Version: {vanityVersion.Value}";
```

Si el design VM implementa `IGameWidgetViewModel`, agregarle `public ReactiveCommand<Unit, Unit> UpdateBaselineCommand { get; set; } = ReactiveCommand.Create(() => { });`.

- [ ] **Step 2: Botón**

En `GameWidget.axaml`, `ManagedGameGrid`: cambiar `ColumnDefinitions="*, 6, Auto, 6, Auto"` por `"*, 6, Auto, 6, Auto, 6, Auto"`, mover `RemoveGameButton` a `Grid.Column="6"` e insertar antes:

```xml
                    <controls:StandardButton Grid.Column="4" x:Name="UpdateBaselineButton"
                                             LeftIcon="{x:Static icons:IconValues.Update}"
                                             ShowLabel="False"
                                             ShowIcon="Left"
                                             Type="Tertiary"
                                             Fill="Weak"
                                             Command="{CompiledBinding UpdateBaselineCommand}"
                                             ToolTip.Tip="Actualicé el juego: vuelve a tomar la lista de archivos originales del disco. Si tenías mods puestos a mano fuera de tModManager, sacalos antes."/>
```

- [ ] **Step 3: Cablear en Mis juegos**

En `MyGamesViewModel`, después de `vm.DeepCleanCommand = ...;`:

```csharp
                            vm.UpdateBaselineCommand = ReactiveCommand.CreateFromTask(async () =>
                            {
                                try
                                {
                                    var loadoutId = GetLoadout(conn, installation);
                                    if (!loadoutId.HasValue) return;
                                    var loadout = Loadout.Load(conn.Db, loadoutId.Value);
                                    var metadata = await Task.Run(() => installation.GetGame().Synchronizer.UpdateBaseline(loadout));
                                    GameBaselineFile.TryGetVanillaFiles(metadata, out var files);
                                    _notificationService.ShowToast($"Lista de archivos originales actualizada: {files.Count} archivos", ToastNotificationVariant.Success);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error al actualizar la lista de archivos originales");
                                    _notificationService.ShowToast("No se pudo actualizar la lista de archivos originales", ToastNotificationVariant.Failure);
                                }
                                finally
                                {
                                    _refreshSignal.OnNext(Unit.Default);
                                }
                            });
```

- [ ] **Step 4: Build y tests de UI**

Run: `dotnet build src/NexusMods.App -p:TreatWarningsAsErrors=true` → 0 warnings.
Run: `dotnet test tests/NexusMods.UI.Tests --filter "RequiresNetworking!=True&FlakeyTest!=True"` → PASS.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: \"Actualicé el juego\" button and unknown-version notice in My Games"
```

---

### Task 6: Documentación y suite completa

**Files:**
- Modify: `TODO.md` (pieza 1 de la tabla; "Lista de archivos originales" de la fase 2)
- Modify: `CLAUDE.md` (Loadout & Synchronization; `NexusMods.Games.FileHashes`)
- Modify: `docs/superpowers/specs/2026-10-04-vanilla-baseline-design.md` (estado)

- [ ] **Step 1: Docs**

- `TODO.md`: en la fase 2, marcar `[x]` "Lista de archivos originales" con una línea: "hecho (PR de esta rama): `GameBaselineFile` por instalación, de Nexus si conoce la versión, del disco si no; botón 'Actualicé el juego'". En la tabla de piezas, fila 1: agregar "(hecha)" al nombre.
- `CLAUDE.md`, sección "Loadout & Synchronization", agregar después del punto 2: "Layer 0 (game files) is the installation's `GameBaselineFile` list: filled from the Nexus hash DB when it knows the installed Steam version, otherwise from the disk (`BaselineRule`), rebuilt when manifest IDs change or with the 'Actualicé el juego' button. Nothing outside `UpdateBaseline` asks the hash DB what is vanilla." En la línea de `NexusMods.Games.FileHashes`, agregar "(version names and the known-version vanilla list; optional)".
- Spec: cambiar "Estado" a "implementado (PR #N)".

- [ ] **Step 2: Suite completa**

Run: `printf '4\n\n0\n' | ./dev.sh > /tmp/claude-1000/suite.log 2>&1` (en segundo plano; tarda) y después `grep -E 'Passed!|Failed!|failed:|succeeded:|Warn' /tmp/claude-1000/suite.log`.
Expected: todos los proyectos `Passed!`, `0 Warning(s)`.

- [ ] **Step 3: Commit, push y PR**

```bash
git add -A
git commit -m "docs: vanilla list in TODO and CLAUDE.md"
git push -u origin feat/vanilla-baseline
gh pr create --base main --title "Vanilla file list without the Nexus hash database (piece 1)" --body "<resumen: qué cambia, resultado del test de riesgo de Task 3 Step 2, snapshots que cambiaron y por qué, verificación, qué falta probar en la app real (./dev.sh opción 11)>"
```

(El cuerpo se escribe con los datos reales al llegar acá; sin pie de Claude.)
