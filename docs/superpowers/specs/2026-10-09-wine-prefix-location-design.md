# Ubicaciones dentro del prefix con whitelist de archivos gestionados (pieza 2)

Fecha: 2026-10-09 · Estado: diseño aprobado, pendiente de plan e implementación

## Objetivo

Que un juego pueda declarar archivos que viven dentro del prefix de Proton/Wine (settings, saves,
archivos de orden) sin que la app vea, respalde, borre o escriba nada más dentro del prefix. Es la
pieza 2 de "Piezas genéricas para el segundo juego" en `TODO.md`. Hoy la única ubicación es `Game`
y el synchronizer trata igual a toda ubicación top-level: escanea todo, el reset borra todo lo no
vanilla. Con eso, cualquier ubicación dentro del prefix sería peligrosa.

### Qué se decidió (2026-10-09)

- **Alcance CP2077:** solo `UserSettings.json`
  (`pfx/drive_c/users/steamuser/AppData/Local/CD Projekt Red/Cyberpunk 2077/UserSettings.json`).
  Ningún instalador lo escribe todavía; la pieza es infraestructura y CP2077 es el caso de prueba.
- **La ubicación es la raíz del prefix (`pfx`), no la carpeta de settings.** Así el guard existente
  `SafePath.IsUnderSymlink(raíz, archivo)` cubre toda la cadena `drive_c/users/.../Cyberpunk 2077`
  con código que ya existe. Si `pfx` mismo es un symlink (prefix movido de disco) se sigue, igual
  que hoy con `Game`.
- **Whitelist declarada por el juego, aplicada por el core.** El core solo conoce "ubicación con
  lista de archivos gestionados"; nada de CP2077 sale de `Games.RedEngine`.
- **Ante la duda, no tocar:** fuera de la whitelist no se lee, no se escribe, no se borra. Un mod que
  apunte fuera de la whitelist en esa ubicación falla el sync antes de tocar el disco.

### Criterios de éxito

1. Con el prefix presente, al gestionar CP2077 la app indexa `UserSettings.json` y nada más del
   prefix (ni `CrashInfo.json`, ni `cache/`, ni `drive_c/windows`).
2. Un cambio de settings hecho desde el juego aparece como External Change; el reset (desactivar,
   dejar de gestionar con limpieza) restaura el `UserSettings.json` original y no toca otro archivo
   del prefix.
3. Un mod con un path fuera de la whitelist en esa ubicación hace fallar el sync sin escribir.
4. Si cualquier carpeta entre `pfx` y el archivo es un symlink, la app no escribe, no borra y no
   indexa a través de él. El test con symlink a una carpeta externa falla sin el fix.
5. Sin prefix (sin `LinuxCompatabilityDataProvider`, como en los tests existentes) no cambia nada:
   la única ubicación sigue siendo `Game` y los snapshots `.verified.` no cambian.
6. Nada del core asume CP2077 ni Nexus Mods.

### Fuera de alcance

- Saves (`Saved Games/CD Projekt Red/Cyberpunk 2077`): se agregan a la whitelist cuando haga falta.
- `IIntrinsicFile` para `UserSettings.json` (pieza 4): hasta entonces los cambios del juego van a
  External Changes como cualquier otro archivo.
- `modlist.txt` de REDmod (`RedModDeployTool` lo escribe directo en `pfx/drive_c`): candidato a
  whitelist más adelante.
- UI nueva. El árbol de archivos del loadout muestra la ubicación nueva con los controles de hoy.

## Diseño

### 1. Ubicación nueva: `LocationId.WinePrefix`

- `LocationId.WinePrefix = From("WinePrefix")` en `NexusMods.Sdk/Games/Locators/LocationId.cs`.
  El nombre se persiste en `GamePath` dentro de la base; queda fijo.
- `Cyberpunk2077Game.GetLocations` agrega `{ WinePrefix → LinuxCompatabilityDataProvider.WinePrefixDirectoryPath }`
  cuando el provider existe. Sin provider (tests, juego sin prefix) no se declara.
- `GameLocations.Create` la trata como top-level (no está dentro de `Game`).

### 2. Whitelist por ubicación

- `IGameData` suma un método con implementación por defecto vacía:

  ```csharp
  ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>> GetManagedFiles(IFileSystem fileSystem, GameLocatorResult gameLocatorResult)
      => ImmutableDictionary<LocationId, ImmutableHashSet<RelativePath>>.Empty;
  ```

  Una ubicación ausente del diccionario está gestionada entera (`Game` sigue igual). Una presente
  gestiona solo los paths listados.
- `GameLocationDescriptor.ManagedFiles` (`ImmutableHashSet<RelativePath>?`, null = entera) y
  `GameLocations.IsManaged(GamePath)`: true si la ubicación no tiene whitelist o el path está en ella.
  `GameRegistry` pasa el resultado de `GetManagedFiles` a `GameLocations.Create`.
- CP2077 devuelve `{ WinePrefix: ["drive_c/users/<user>/AppData/Local/CD Projekt Red/Cyberpunk 2077/UserSettings.json"] }`.
  `<user>` = `steamuser` si existe `drive_c/users/steamuser`; si no, `Environment.UserName` si existe
  esa carpeta (prefix de Wine/Lutris); si no existe ninguna, `steamuser` (el prefix de Proton lo crea
  al primer lanzamiento).

### 3. Enforcement en el core

Tres puntos, todos en el core, para que ningún juego pueda olvidarse:

- **Scan** (`GameLocationsService.IndexGame`): una ubicación con whitelist no se enumera. Se hace
  `stat` de cada path de la whitelist y se indexa solo si existe, no está bajo un symlink
  (`SafePath.IsUnderSymlink(raíz, path)`) y no es él mismo un symlink (`SafePath.IsSymlink`).
  Enumerar `pfx` serían miles de archivos de Windows y es justo lo que no queremos ver.
- **Escritura y borrado** (`ALoadoutSynchronizer.EnsureDiskChangesStayInside`): además de `..` y
  symlinks, un path con acción `DeleteFromDisk`, `ExtractToDisk` o `WriteIntrinsic` que no sea
  `IsManaged` lanza `InvalidOperationException` nombrando el path, antes de tocar el disco. Mismo
  patrón que hoy: el sync falla completo y no escribe nada.
- **`CleanDirectories`** no corre en ubicaciones con whitelist: no borramos carpetas vacías dentro
  del prefix aunque sean nuestras.

### 4. Lista vanilla

- La base de hashes de Nexus solo devuelve paths `Game`. En `UpdateBaseline`, para toda ubicación
  distinta de `Game` se aplica siempre `BaselineRule` (foto del disco), también cuando Nexus conoce
  la versión. El marcador `BaselineFromDisk` sigue reflejando solo la ubicación `Game`.
- Efecto: `UserSettings.json` presente al gestionar es original; el reset lo restaura del backup.
- Caso borde aceptado: prefix recién creado sin `UserSettings.json` al gestionar. El que genere el
  juego entra como External Change y el reset lo borra (el juego lo regenera con defaults).
- `ReprocessOverrides` y `AdoptableExternalChanges` no cambian (solo paths que la base de Nexus
  lista, o fuera de mods e intrínsecos).

### 5. Tests

Fixture: `UniversalStubbedGameLocator` crea `pfx/` en su carpeta temporal y setea
`LinuxCompatabilityDataProvider` con un provider stub que apunta ahí (sin overrides ni winetricks).
Los tests que no crean `drive_c/users/steamuser` ven el prefix vacío: `GetLocations` lo declara igual
(path determinista) y el scan no encuentra nada, así que los snapshots existentes no cambian.

En `tests/NexusMods.DataModel.Synchronizer.Tests` (xUnit, fixture `ACyberpunkIsolatedGameTest`):

1. `PrefixLocation_IndexesOnlyTheWhitelist`: con `UserSettings.json`, `CrashInfo.json`, `cache/x`
   y `drive_c/windows/system32/a.dll` en el prefix, tras gestionar y sincronizar `DiskStateEntry`
   contiene solo `UserSettings.json`.
2. `PrefixLocation_ResetRestoresTheSettingsAndLeavesTheRest`: mod que reemplaza `UserSettings.json`
   (necesita que el helper `AddModAsync` acepte `GamePath`), aplicar, dejar de gestionar con
   limpieza: contenido original restaurado, `CrashInfo.json` y `cache/x` intactos.
3. `ModFileOutsideTheWhitelist_FailsTheSyncWithoutWriting`: mod con `(WinePrefix, "drive_c/users/steamuser/Desktop/x.txt")`
   → el sync lanza y el archivo no existe.
4. `PrefixLocation_SymlinkedFolder_IsNeverWrittenNorDeleted` (falla sin el fix): la carpeta
   `Cyberpunk 2077/` es un symlink a una carpeta externa con `UserSettings.json` + canario. El sync con
   un mod que reemplaza `UserSettings.json` rechaza; dejar de gestionar con limpieza deja el
   `UserSettings.json` externo y el canario intactos.
5. `PrefixLocation_SymlinkedUserFolder_IsNeverIndexed`: `users/steamuser` es symlink a una carpeta
   externa → nada indexado, nada tocado al dejar de gestionar.
6. En `SteamVanillaBaselineTests`: versión conocida por Nexus + `UserSettings.json` en el prefix → el
   archivo está en `GameBaselineFile`.

Prueba real (`./dev.sh` opción 11, dentro de la jaula): gestionar, cambiar un setting en el juego,
ver el External Change, reset, confirmar que el setting volvió y que `cache/` y `CrashInfo.json`
siguen ahí.

### Archivos a tocar

- `src/NexusMods.Sdk/Games/Locators/LocationId.cs`, `GameLocationDescriptor.cs`, `GameLocations.cs`
- `src/NexusMods.Sdk/Games/IGameData.cs` (`GetManagedFiles`)
- `src/NexusMods.Backend/Games/GameRegistry.cs`, `GameLocationsService.cs`
- `src/NexusMods.Abstractions.Loadouts.Synchronizers/ALoadoutSynchronizer.cs`
  (`EnsureDiskChangesStayInside`, `CleanDirectories`, `UpdateBaseline`)
- `src/NexusMods.Games.RedEngine/Cyberpunk2077/Cyberpunk2077Game.cs`
- `tests/NexusMods.StandardGameLocators.TestHelpers/UniversalStubbedGameLocator.cs`
- `tests/Games/NexusMods.Games.TestFramework/AIsolatedGameTest.cs` (`AddModAsync` con `GamePath`)
- `tests/NexusMods.DataModel.Synchronizer.Tests/` (tests nuevos), `SteamVanillaBaselineTests.cs`
- `TODO.md` (pieza 2 hecha, saves y `modlist.txt` como pendientes), `CLAUDE.md` (ubicaciones)
