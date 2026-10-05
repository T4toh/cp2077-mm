# TODO

## 📍 Estado y próximos pasos (2026-09-25)

Mergeado: eliminación de `.nx` en 3 PRs (#42 store por hash + GC, #43 descargas propias, #45 asistente de limpieza + Deep Clean reforzado) y #44 (snapshot de RedMod). Sin CI: los workflows de GitHub se sacaron, **la verificación es local** (`./dev.sh` opción 4, un proyecto por vez). Nada de esto se probó todavía con la app abierta: solo build + tests.

### Prueba real del 2026-09-25 (rama `fix/delete-no-follow-symlinks`)

Hecho: asistente completo (Deep Clean, prefix de Proton borrado sin tocar nada afuera, verificación de Steam, reset), juego gestionado de cero, "Welcome to Night City" bajada (283 mods) e instalada, juego lanzado con RED4ext + 5 plugins y REDScript sin errores. En el camino se arreglaron: el handler `nxm://` que los tests reescribían (login roto), descargas sin extensión, carrera en la barra de progreso de la colección, y el health check del prefix que nunca corría para Steam (tras recrear el prefix faltaban `vcrun2022`/`d3dcompiler_47`: `protontricks 1091500 -q vcrun2022 d3dcompiler_47`).

### Prueba real del 2026-10-03 (rama `chore/sandbox-run`): plan completo

Pasos 6-8 hechos dentro de la jaula (`./dev.sh` opción 11): store borrado a mano y reaplicado (con la app cerrada y abierta), colección reinstalada sin el store, Storage Manager (Deep Clean, borrar descargas cancelado, borrar prefix). Después, prefix recreado por Steam y health check de Proton verificado: marca `vcrun2022`/`d3dcompiler_47` sin prefix y con prefix nuevo, desaparece tras `protontricks`. Único error en el log: el `.desktop` del handler `nxm://`, esperado dentro de la jaula. En el camino: Deep Clean no movía carpetas entre subvolúmenes btrfs / discos (`EXDEV`); arreglado con `NoFollowMove.MoveDirectoryNoFollow` (copia sin seguir links si el rename falla), probado con los mods reales. Avisos `Unable to extract` de 3 archivos generados en runtime (`red4ext/config.ini`, `final.redscripts.bk`, `.bin` de address_library) con el store borrado: no vienen de ninguna descarga y se regeneran solos.

- [x] **Duplicados en `Downloads/` de esta corrida:** resuelto solo el 2026-10-04: "Borrar descargas" se llevó las copias sin extensión (las que tenía registradas la biblioteca) y quedaron las `.zip` viejas. Ver "Borrar descargas deja lo que la biblioteca no registró"
- [ ] **El rescan MD5 no es automático** (solo el botón "Rescan downloads"): con el reset se bajó todo de nuevo aunque las descargas estaban. Correrlo solo antes de bajar una colección
- [ ] **Avisar al borrar el prefix de Proton** que después hacen falta `vcrun2022` y `d3dcompiler_47` (o instalarlos con protontricks desde la app); ahora el health check lo detecta, pero hay que ir a buscarlo

### Plan de prueba original

Pruebas con datos reales: `./dev.sh` opción 11 (compila Release, verifica la jaula, saca snapshot de `/home` y abre la app con todo el disco en solo lectura salvo `tModManager`, el juego y el prefix 1091500; ver `sandbox-run.sh`). Antes de arrancar: backup de saves (`steamapps/compatdata/1091500/pfx/drive_c/users/steamuser/Saved Games/CD Projekt Red/Cyberpunk 2077/`) y de `~/.local/share/tModManager/` (el asistente borra la base y los `.nx`).

1. **Build y suite local.** `git pull && ./dev.sh` → opción 4 (tests sin red/flakey, proyecto por proyecto). Esperado: todo verde. Después `dotnet build -c Release` o AppImage (opción 10).
2. **Asistente de datos viejos** (hay 285 `.nx` en `DataModel/Archives`, tiene que aparecer al abrir):
   - Paso 1 aviso → Siguiente.
   - Paso 2: muestra cantidad/tamaño de `NexusMods.App/Downloads` → Siguiente las **mueve** a `tModManager/Downloads` (chequear que el origen quede vacío y los nombres se conserven).
   - Paso 3: Deep Clean (backup en `tModManager/Backups/<timestamp>/`). Probar sin tildar el prefix de Proton. Si falla, anotar el error y probar "Continuar sin limpiar".
   - Paso 4: "Verificar integridad en Steam" (abre Steam) → esperar que termine → "Reiniciar".
   - Al volver: sin asistente, app vacía (base nueva), no quedan `*.nx`.
3. **Gestionar el juego e instalar "Welcome to Night City".** Anotar cuántos mods reutiliza el MD5 rescan (descargas rescatadas) y cuántos baja. Nada debería bajarse dos veces.
4. **Aplicar y lanzar el juego** desde la app: Redscript, CET y RED4ext sin errores, jugable.
5. **Disco:** `find ~/.local/share/tModManager -name '*.nx'` vacío; existe `DataModel/Archives/XX/<hash>`; `Downloads/` con los nombres de Nexus.
6. **Store borrado a mano:** con la app cerrada, borrar `DataModel/Archives/`; abrir, Aplicar → se reextrae todo desde Descargas. Repetir con la app **abierta** (borrar y aplicar): no tiene que fallar el sync.
7. **Reinstalar la colección después de borrar el store:** no tiene que volver a bajar nada de Nexus.
8. **Storage Manager:** Deep Clean → `Downloads/` intacta y el backup nuevo presente en `tModManager/Backups`. "Borrar descargas" pide confirmación. "Borrar prefix de Proton" solo con el juego cerrado (opcional, pierde saves no sincronizados).
9. **Logs:** `~/.local/state/tModManager/Logs/nexusmods.app.main.current.log` (antes de la rama de seguridad del 2026-09-25 estaban en `NexusMods.App/Logs`). Pegar cualquier excepción en la sesión.

Notas: login desde un build de `bin/` necesita `/etc/dotnet/install_location` apuntando a `~/.dotnet` (ya está en esta PC). Si algo crashea solo en Debug con `Assertion failed`, es un `Debug.Assert`: anotar cuál.

### Pendiente después de probar

- [ ] **Super Clean con varios loadouts:** hoy se niega (guarda del 2026-09-25). Arreglo real: conservar todos los snapshots de una corrida (cada pasada con sync crea uno y `PruneOldBackups` se lleva el primero, el único con archivos no gestionados). El orden de snapshots es por nombre (hora local): un cambio de horario o una carpeta ajena en `Backups/` puede elegir mal
- [ ] **Desacoplar Cyberpunk del core** (ver sección Multi-juego). Rama nueva desde `main`, verificada con la suite local.

## ✅ Completado

- [x] Descarga automatizada de colecciones (sin premium, sin browser)
- [x] Captura de enlaces NXM (protocolo independiente)
- [x] Vista unificada de descargas
- [x] Diagnósticos de mods esenciales (Redscript, RED4ext, CET, ArchiveXL, TweakXL, Codeware)
- [x] Deep Clean + Storage Manager
- [x] Remoción total de telemetría
- [x] Rebrand a "Cyberpunk 2077 Mod Manager"
- [x] Limpieza de código muerto (directorios vacíos, NuGet huérfanos, tiendas removidas, UI de feedback, ComingSoon, settings muertos, premium gates, páginas de debug bajo `#if DEBUG`)
- [x] Limpieza de tests: eliminación de databases de StardewValley, corrección de migración \_0004 para tolerar juegos no registrados, fix de limpieza de carpetas vacías en el Synchronizer, actualización de snapshots Verify, limpieza de referencias a juegos removidos en test data

## 🔄 Consolidación de Vistas de Descarga

Actualmente hay dos sistemas paralelos de descarga con componentes duplicados:

- [x] **Unificar componentes de descarga:** Extraídos `SizeProgressComponent` y `SpeedComponent` a `SharedProgressComponents.cs`, usados por ambas vistas
- [x] **Progreso de colecciones:** Las descargas de colección ahora muestran barra de progreso real y velocidad, conectándose a `IDownloadsService.ActiveDownloads` por `FileMetadataId`
- [x] **Agregar interface a CollectionDataProvider:** Extraída `ICollectionDataProvider` con registro DI apropiado
- [x] **Remover página de descargas vacía:** La navegación a la página standalone de descargas fue eliminada (estaba vacía). El botón de velocidad en el spine es ahora solo informativo. Descargas activas se ordenan arriba en la vista de colección
- [x] **Controles de descarga en colección:** Botones de pausa/resume/cancel en la columna de acciones, conectados a `IDownloadsService`
- [x] **Auto-reordenamiento:** La lista se re-ordena automáticamente cuando una descarga termina para que la siguiente suba al tope
- [x] **Botón "Ver página del mod":** Abre la página de Nexus Mods del mod directamente desde la vista de colección
- [x] **IsLoading infrastructure:** Agregado `IsLoading` a `APageViewModel` con control `LoadingSection` reutilizable
- [ ] **Refactorizar CollectionDownloadViewModel:** ~850 líneas. Extraer lógica de orquestación a un servicio separado
- [ ] **Unificar settings de paralelismo:** `DownloadSettings.MaxParallelDownloads` solo controla descargas de colección, no las regulares

## 🎨 Mejoras de UI/UX

- [ ] **Tema "cueva": negro + violeta/índigo.** Reemplazar la paleta naranja/gris de Nexus por fondo negro y acentos violeta/índigo. Colores en `src/NexusMods.Themes.NexusFluentDark/Resources/` (src/NexusMods.Themes.NexusFluentDark/Resources/Palette/Colors/BrandColors.axaml src/NexusMods.Themes.NexusFluentDark/Resources/Palette/Colors/ElementColors.axaml ); los controles referencian brushes con nombre, así que es cambiar la paleta, no los controles. Pedido 2026-09-22
- [ ] **Ícono nuevo** para reemplazar `src/NexusMods.App/icon.svg` e `icon.ico` (heredados de Nexus). Base: el de tWriter, `~/Repos/tWriter/src/assets/icon.png` (también `src-tauri/icons/*.png` en varios tamaños). Misma línea visual que el tema cueva. Actualizar también `io.github.t4toh.tmodmanager.metainfo.xml` si cambia el nombre del ícono. Pedido 2026-09-22, repedido 2026-10-04: con el multi-juego tiene que ser genérico, nada de Cyberpunk

- [ ] **Ícono del juego en la barra lateral → página del juego** (pedido 2026-10-04): tocar el ícono de Cyberpunk tendría que llevar a donde hoy lleva el logo de Nexus, donde se maneja el juego (`MyGamesPageFactory`, `SpineViewModel.NavigateToHome`). Hoy el ícono del juego abre el workspace del loadout (`LoadoutPageFactory`). Decidir qué queda en el logo de arriba y cómo se llega al loadout. Va junto con el ícono genérico
- [ ] **Botón "Limpiar biblioteca"** en Storage Manager, junto a "Borrar descargas" (pedido 2026-10-04): quitar todos los items de la biblioteca, no solo los archivos de `Downloads/`. Reusar `LibraryItemRemover`; definir qué pasa con los mods instalados que vienen de esos items. Con confirmación

- [ ] **Botón "Borrar prefix de Proton"** en Storage Manager, junto al Deep Clean: borra `steamapps/compatdata/1091500/` (Steam lo recrea al lanzar). Con confirmación: se pierden saves no sincronizados con la nube y toda la config del prefix. Pedido 2026-09-22

- [x] **Loading indicators:** Agregado `IsLoading` al `APageViewModel` base con control `LoadingSection` reutilizable
- [ ] **Manejo de errores visible:** Muchos ViewModels tienen `// TODO: handle errors`. Implementar notificación al usuario vía `IWindowNotificationService` en todos los comandos async
- [ ] **Empty states consistentes:** El control `EmptyState` existe pero no todas las páginas lo usan. Auditar y completar: MyGames sin juego, Library vacía, Loadout sin mods
- [ ] **Accesibilidad básica:** Faltan `TabIndex`, estilos `:focus`/`:keyboard`, tooltips en botones icon-only
- [ ] **Strings hardcodeados:** Centralizar textos en español en `Language.resx` (actualmente mezclados inline en ViewModels y AXAML)
- [ ] **DesignViewModels rotos:** `ApplyDiffDesignViewModel` tira `NotImplementedException`; varios otros son stubs mínimos

## 🔮 Features Futuras

- [ ] **Endorse de mods desde la app:** Botón de endorse en la UI por cada mod instalado + endorse masivo para colecciones. La API ya tiene el endpoint (`POST /v1/games/{domain}/mods/{id}/endorse.json`). Los modders se lo merecen y la app oficial nunca lo implementó
- [ ] Soporte multi-browser para cookies (Chrome/Chromium). Ver [implementación](README.md#agregar-soporte-para-otro-browser)
- [ ] Optimización de rescan MD5 para carpetas grandes
- [ ] Tema claro / alto contraste (solo existe `NexusFluentDark`)

## 🧹 Limpieza sistemática (PRs #26, #27, #28 mergeados)

Objetivo: codebase confiable antes de tocar features. Un PR por bloque, build + tests entre cada uno.

- [x] **Borrar proyectos vacíos/huérfanos del sln:** `NexusMods.Cli` (0 .cs), `NexusMods.UI` (0 .cs), `App.Generators.Diagnostics.Sample`, `src/Examples` (ejemplos upstream, nadie los referencia)
- [x] **Warnings a cero:** `CS0105` usings duplicados en `Sdk/Loadouts/Models/Loadout.cs`, `CS0168` en `Sdk/Games/IGameData.cs`, `NU1510` `System.Linq` en `Abstractions.Loadouts.Synchronizers.csproj`, `CS0612 Tracking` en `CollectionCreator.cs`, `CS0618` `GameInstallMetadata.Name` / `ManuallyAddedGame` (obsolete upstream; quitar `[Obsolete]` o migrar)
- [x] **`CS8785` resuelto:** era el analyzer transitivo `Weave` (dependencia de `MnemonicDB.SourceGenerator`), no Fody. Se remueve en `Directory.Build.targets`
- [x] **`ExperimentalSettings`:** quitado `StardewValley` de `SupportedGames`. `EnableCollectionSharing` se mantiene (gatea la UI de compartir colecciones)
- [x] **13 `// TODO: handle errors`:** `GraphQlResult.AssertHasData()` ya no hace `Debug.Assert` (crasheaba builds Debug ante cualquier error de API) y la excepción incluye los errores GraphQL. Los dos sitios de UI usan `TryGetData` y no rompen la vista
- [x] **Actualizar CLAUDE.md:** conteo de proyectos, stubs de telemetría, build en macOS
- [ ] **Bugs runtime reales:** pendiente reproducir en Linux con juego instalado (crashes esporádicos reportados). Hipótesis a verificar: hay 108 `Debug.Assert`/`Debug.Fail` en `src/`; en build Debug (`dotnet run`, `dev.sh`) cualquier assert fallido mata el proceso. El AppImage es Release y no los ejecuta. Si los crashes son corriendo desde `dev.sh`, correr con `-c Release` para descartar
- [x] **Paquetes al día** (2026-09-24, PRs #38, #39, #40): vulnerabilidades NuGet a 0, bumps dentro del major, Microsoft.Extensions 10, Humanizer 3, StrawberryShake 16, tests a xunit v3 / TUnit 1.x / Verify 32, Paths 0.22.5. Retenidos a propósito: TreeDataGrid 11.1.1 (11.2+ es comercial, Avalonia Accelerate), FluentAssertions 7.x (8 es comercial), Verify 32.x (33+ trae SponsorCheck que rompe el build), Fomod 1.2.1 (solo Windows), MnemonicDB 0.28.2 (ver abajo)
- [ ] **Avalonia 12** (+ ReactiveUI 24, SkiaSharp 4, Splat 21): migrar ~259 `[Reactive]` de ReactiveUI.Fody (muerto) a ReactiveUI.SourceGenerators; TreeDataGrid 12 es comercial, así que vendorizar el fuente MIT de 11.1 (`AvaloniaUI/Avalonia.Controls.TreeDataGrid`, archivado) y portarlo. Sin apuro mientras Avalonia 11 reciba parches (11.3.22 el 2026-09-11)

## 📦 Dependencias heredadas de Nexus

Decidido 2026-09-24. Todas son GPL-3.0 como tModManager: se pueden vendorizar (copiar el fuente a `src/`) sin problema legal. Criterio: **congeladas en la última versión que funciona; se vendorizan cuando haya un motivo concreto** (bug, vulnerabilidad en una dependencia nativa, versión de .NET que las rompa), no antes.

- **MnemonicDB** (la base: loadouts, mods, colecciones): repo archivado 2025-11. Quedamos en **0.28.2**, la última que usó la app oficial en producción. **No subir a 0.50+**: es una reescritura de API publicada dos semanas antes de archivar, ninguna app real la usó. Si hace falta tocarla, vendorizar el tag `v0.28.2`. Depende de RocksDB 9.10 y DuckDB (vigilar sus advisories). Largo plazo opcional: reemplazar por SQLite (~286 archivos la usan)
- **NexusMods.Paths** (`AbsolutePath`, `GamePath`, filesystem en memoria para tests): sin commits desde 2025-10. Congelada en **0.22.5**. Ojo: 0.22 trae su propio `ChunkedStream`/`IChunkedStreamSource` (este último en el namespace global); usamos el nuestro de `NexusMods.Sdk.IO`, calificado
- **NexusMods.Hashing.xxHash3**: reemplazable por `XxHash3` de `System.IO.Hashing` (paquete oficial de Microsoft). Hacerlo junto con la eliminación de `.nx`, porque los hashes también se guardan en la base
- [x] **Eliminar `.nx` file store** (2026-09-24, PRs #42, #43, #45; falta la prueba con la app, ver arriba): reemplazado por `LooseFileStore` (content-addressed, `Archives/<2-hex>/<hash>`) + GC por barrido (`LiveHashes`). Descargas de primera clase en `tModManager/Downloads` con `LibraryFile.DownloadPath` y reextracción vía `IDownloadReExtractor`. Asistente de limpieza guiada para datos viejos (`.nx`, DB vieja), Deep Clean reforzado, borrado del prefix de Proton. Salen `NxFileStore`, los tres proyectos `GarbageCollection.*`, `NexusMods.Archives.Nx` y `NexusMods.Paths.Extensions.Nx`
  - [ ] **Archivos locales fuera de Descargas:** lo que se agrega con `AddLocalFile` desde otra carpeta (`ManualDownloadRequiredOverlay.cs`, `LibraryViewModel.cs` "agregar desde archivo") no se copia a `tModManager/Downloads`, así que queda sin `DownloadPath`: no se puede reextraer si se borra del store ni es portable. Copiarlo (o moverlo) a Descargas antes de agregarlo
  - [ ] **Backups viejos de NexusMods.App:** `LegacyDataDetector.LegacyBackupsFolder` no se usa; `~/.local/share/NexusMods.App/CyberpunkBackups` nunca se cuenta ni se ofrece borrar. El asistente de limpieza podría mostrarlo y ofrecer borrarlo
- Activas, no requieren acción: `FomodInstaller` (Nexus, commits 2026-09), `GameFinder` y `TransparentValueObjects` (erri120)

## 🎮 Multi-juego (después de limpieza)

Intento anterior falló por acoplamiento a Cyberpunk filtrado fuera de `Games.RedEngine` (~35 archivos). Orden:

- [x] **Renombrar app a tModManager:** app ID `io.github.t4toh.tmodmanager`, data dir `~/.local/share/tModManager/` con migración automática desde `NexusMods.App.Cyberpunk/`, `.desktop` viejo se borra al registrar el handler nxm. Pendiente: renombrar el repo GitHub `cp2077-mm` → `tModManager` (manual, GitHub redirige)
- [x] ~~**CI propio:**~~ sacado el 2026-09-25 (fallaba y se prefiere probar local; la suite equivalente es `./dev.sh` opción 4). Era: GitHub Actions, `.github/workflows/ci.yaml` (ubuntu, `dotnet build -warnaserror`, xUnit vía `dotnet test` con filtro, TUnit vía `dotnet run`). Primera corrida verde: 1175 tests xUnit + 94 TUnit en ~4.5 min. Único arreglo necesario: ordenar hijos antes de `Verify` en `PathBasedInstallerTests` (orden de enumeración difiere entre ext4 y APFS)
- [ ] **Desacoplar Cyberpunk del core** (relevo del 2026-10-03, detalle abajo). Dos reglas: **nada de CP fuera de `Games.RedEngine`** y **nada atado a Nexus**: Nexus Mods es una fuente de mods más (la más popular), no la única; KOTOR vive sobre todo en Deadly Stream, otros mods en GitHub. No hace falta inventar ya una abstracción de "fuentes" (se diseña cuando haya una segunda fuente real), pero no sumar acople nuevo y sacar el que se toque.
- [ ] **Juegos a agregar, en orden** (pedido 2026-09-24; Witcher 3 arranca después de la fase 1 del desacople):
  1. **The Witcher 3 Remastered (5.x)** (salió 2026-09-29; expansión *Songs of the Past* en 2027). Investigado 2026-10-03, detalle en "Witcher 3: lo investigado" abajo. El merge de scripts/bundles va último: la edición es muy nueva y las herramientas de la comunidad cambian día a día
  2. **Skyrim, la versión más nueva en Steam** (puede esperar, decidido 2026-10-03: primero los juegos que se van a jugar. Referencia: [Corkscrew](https://corkscrewmodmanager.com/), GPL-3, Rust/Tauri, ya cubre Skyrim SE/AE + Fallout 4 en Linux/macOS con colecciones, Wabbajack, LOOT y FOMOD; beta v0.9.x) (Special/Anniversary Edition): SKSE, `plugins.txt`/load order, FOMOD (ya existe), Proton. Fallout 4 comparte motor y queda casi gratis después
  3. **KOTOR 1 y 2**: juegos viejos que hoy se modean a mano sí o sí (overrides en `Override/`, TSLPatcher/HoloPatcher con instrucciones por mod, orden de instalación estricto). El valor está en automatizar eso. Investigado 2026-10-03:
     - Steam 32370 / 208580. TSL nativo tiene dos `override` y los parches de exe de la comunidad son solo Windows: TSL por Proton. En Linux todo a minúsculas; Workshop de TSL vacío
     - 25-45% de los mods son patchers que editan `.2da`/`dialog.tlk`/`.mod` según lo instalado antes, sin desinstalación: necesita las piezas 3 y 8 de abajo completas
     - HoloPatcher (PyKotor, LGPL) corre sin interfaz en Linux y es case-aware: invocarlo como proceso. KOTORModSync es BSL (no reutilizable) y su repo da 404. Referencia GPL: `ChristopherVR/kotor-mod-manager` (convierte los mod builds de `kotor.neocities.org` a JSON)
     - Fuente principal Deadly Stream: sin API, login + CSRF, 55 descargas/día; los términos no dicen nada de bots (preguntar)
     - Esfuerzo XL (MVP M sin reordenar)
- [ ] **Requisitos de cada juego como datos, no como wiki** (pedido 2026-10-03): lo que hoy hay que ir a leer a la wiki de cada juego (paquetes de protontricks como `vcrun2022`/`d3dcompiler_47`, DLL overrides, opciones de lanzamiento de Steam, archivos de config a tocar, pasos tras recrear el prefix) declarado por juego, y que la app lo muestre como checklist con estado real y, donde se pueda, un botón que lo haga (`protontricks <appid> -q ...`). Hoy está hardcodeado en `WinePrefixRequirementsEmitter` de CP2077; generalizarlo es parte del desacople (que cada `IGame` declare sus requisitos y el health check sea genérico)
- **No recuperar `Games.CreationEngine` de upstream**: se sacó a propósito porque no gustaba cómo estaba hecho. Escribir cada juego desde cero sobre el core desacoplado; el código viejo (history de NexusMods.App) sirve como mucho de referencia
- [ ] **Referencia Vortex:** `Nexus-Mods/vortex-games` (GPL-3) tiene una carpeta `game-*` por juego (100+) con las reglas de layout/instalación de cada uno; la de Cyberpunk es `E1337Kat/cyberpunk2077_ext_redux` (~25 tipos de layout vs nuestros 4 instaladores). No es código portable (TypeScript/Electron/Windows), son reglas a leer. Para CP2077 sirven: layouts "arreglables" (`.archive` suelto → `archive/pc/mod/`, Redscript sin subcarpeta → `r6/scripts/<mod>/`, DLL suelta → `red4ext/plugins/<mod>/`, REDmod sin `mods/`), mods envueltos en carpeta extra, archivos protegidos (`inputContexts.xml`, `inputUserMappings.xml`, `options.json`) con confirmación, core mods por versión (RED4ext `winmm.dll` vs `d3d11.dll`), CET exige `init.lua`

### Relevo del desacople (2026-10-03)

El core heredado de upstream ya es multi-juego (`IGame`, `SteamLocator`, API de Nexus, nxm, mapeo dominio→juego, Protontricks con appid como parámetro). Lo que ata a CP son agregados del fork cableados por afuera de `IGame` (commits `97480bbf7`, `89368ded9`, `2e7827cad`, `65277f97f`/`0470c083d`, `2520ad2bf`, `723cb0a11`). `App.UI` referencia `Games.RedEngine` desde 4 archivos (`GameWidgetViewModel`, `EssentialModsPage`, `EssentialModsViewModel`, `MyGamesViewModel`).

**Fase 1: arreglos antes de que un segundo juego toque el disco** (hecha el 2026-10-03, rama `feat/decouple-phase1`; cada arreglo con un test de dos juegos donde se pudo):

- [x] **`SortOrderManager` uno por juego** (era singleton y `RegisterSortOrderVarieties` pisaba las variedades del otro juego)
- [x] **`Backups/<GameId>/<timestamp>`**: ruta en un solo lugar (`Sdk/Games/GameBackups.cs`), "conservar el más nuevo" por juego, los snapshots viejos se mueven a `Backups/RedEngine.Cyberpunk2077/` al arrancar (`DataDirectoryMigration.MoveLegacyBackups`)
- [x] **Prefix de Proton y `steam://validate`** tomados de la instalación (`LinuxCompatabilityDataProvider`, `StoreIdentifier`), con la guarda `steamapps/compatdata/<appid>` numérico; el Storage Manager pregunta de qué juego si hay varios de Steam
- [x] **Super Clean**: el límite de un loadout se cuenta por juego. Sin test del caso permitido (correría un Deep Clean real sobre `~/.local/share`: los tests no redirigen XDG)
- [x] **`SupportedGames` y "Enable unsupported games"** borrados: registrar un juego es lo que lo hace soportado
- [x] **Agregar a mano** con selector de juego; valida con `GetPrimaryFile` del juego elegido (si falla, deshace el alta)
- [x] **Identidad del juego = `GameId`**: atributos nuevos `.../Game` en `GameInstallMetadata` y `ManuallyAddedGame`; migración `_0011` los completa desde el id de Nexus (mapeo histórico fijo 3333 → CP, no construye juegos). El id de Nexus viejo queda como `LegacyNexusModsGameId`, que solo lee la migración y `GameRegistry` (para migraciones anteriores a `_0011`). `StubbedGame` ya no tiene id de Nexus y se puede gestionar al lado de CP (`GameWithoutNexusModsIdTests`)
- [x] **Botones de Essentials** y **panel de Wine prefix** solo para el juego que tiene lista/requisitos (el gate sigue siendo "es CP", en un solo lugar cada uno, hasta la fase 2)
- [x] **Borrados globales** del Storage Manager: siguen siendo de todos los juegos, ahora lo dicen los textos. Acotarlos va con "Storage Manager por juego"
- [x] **Fixture de 2 juegos:** no hizo falta una base común; cada test registra el segundo juego que necesita. Borrados los 30 Verify huérfanos de TestFramework (Skyrim/FO4, colecciones viejas, `RedModInstallerTests.*` duplicado): ningún test los generaba

**Fase 2: con Witcher 3 en marcha, abstraer recién cuando haya dos casos reales:**

- [ ] **Herramienta de limpieza** por juego (p. ej. `IDeepCleanTool : ITool`): reemplaza `OfType<CyberpunkDeepCleanTool>()` (`MyGamesViewModel.cs:285`), `IsDeepCleanAvailable` por `GameId` (`GameWidgetViewModel.cs:125`) y el nombre mágico en `StorageAnalyzer`
- [ ] **Essential mods** por juego: mover el record `EssentialMod` fuera de RedEngine, lista opcional (vacía por defecto); que no asuma Nexus (ya tiene fallback de GitHub)
- [ ] **Requisitos del prefix** declarados por juego + un emitter genérico (ver "Requisitos de cada juego como datos")
- [ ] **Sacar `App.UI → Games.RedEngine`** cuando lo anterior esté hecho
- [ ] **Launch sin `IRunGameTool`** explota (`LaunchButtonViewModel.cs:80`, `.First()`): run tool por defecto en `AddGame<T>()`
- [x] **Lista de archivos originales:** hecho (rama `feat/vanilla-baseline`, PR pendiente): `GameBaselineFile` por instalación, de Nexus si conoce la versión, del disco si no (con la propiedad de los mods leída del último loadout aplicado); botón "Actualicé el juego" (`ISynchronizerService.UpdateBaseline`, serializado con los syncs). Texto original: apply en Steam se niega a borrar si la base de hashes no conoce la versión (`ALoadoutSynchronizer.cs:484`). La base viene de `Nexus-Mods/game-hashes` (congelada) y el builder revienta con juegos no registrados (`BuildHashesDb.cs:136`). **Verificar si trae Witcher 3**; si no, usar el estado inicial del disco (`GameInstallMetadata.InitialDiskStateTransaction`) o una base propia. Sin esto no se pueden sacar mods en Witcher
- [ ] **Downloads por juego:** una sola carpeta; el rescan de colecciones fuerza el vínculo cuando coincide solo el nombre (`CollectionDownloader.cs:825-897`). Subcarpeta por juego o filtrar candidatos; nunca vincular solo por nombre
- [ ] **Storage Manager por juego:** `StorageStats.CyberpunkBackupsSize` → tamaños por juego
- [ ] **GOG/Heroic** (sacado en `723cb0a11`): Witcher 3 y KOTOR se juegan mucho por GOG. Decidir si vuelve o si alcanza con agregar a mano
- [ ] **Menores:** `LegacyDataDetector.LegacyBackupsFolder` (código muerto), referencias duplicadas en `App.csproj` (líneas 23/25 y 24/26), `FileType.Cyberpunk2077AppearancePreset` en `Sdk/FileExtractor/Signatures.cs` (mover a RedEngine), `FileHashesService.cs:434` busca versiones por nombre sin filtrar por juego, textos (Welcome, `.desktop`, metainfo, pupnet), PNG de diseño `cyberpunk_game.png`

Ya genérico, no tocar: `SteamLocator`, Protontricks, API de Nexus y cookies, nxm, mapeo dominio→juego, migración `_0010`, semáforo del `SynchronizerService` (serializa entre juegos: más lento, correcto). Library ya separa `LocalFile` de `NexusModsLibraryItem`: una fuente nueva es otro tipo de item + su descargador.

### Piezas genéricas para el segundo juego (decidido 2026-10-03)

No se escribe código de Witcher 3 ni de KOTOR hasta que estas piezas existan. Cada una va al core, se prueba primero con CP2077 (que ya tiene un caso real para casi todas) y después la usa W3. Nexus sigue siendo la fuente por defecto, pero ninguna pieza depende de él. Lo que nos diferencia es Linux: herramientas de Windows corriendo en el prefix correcto, Proton y mayúsculas, que es justo lo que Vortex no hace.

| # | Pieza | Prueba con CP2077 | Uso en W3 | Después |
|---|---|---|---|---|
| 1 | Lista vanilla sin la base de Nexus (hecha) (= "Lista de archivos originales" de la fase 2; la base local **no trae W3**) | después de cada parche la app no aplica | poder sacar mods | cualquier juego |
| 2 | Ubicaciones dentro del prefix, con whitelist de archivos gestionados (hoy el reset borra todo lo no vanilla de cualquier ubicación) + test con symlink | saves, `AppData/Local/.../UserSettings.json`; vuelve AppData | `Documents/The Witcher 3` | saves/config de cualquier juego |
| 3 | Mods locales de primera clase (= "Archivos locales fuera de Descargas") + metadata opcional de fuente/URL/versión | archivos agregados a mano | mods de mod.io/GitHub/foros | KOTOR |
| 4 | Primer uso real de `IIntrinsicFile` (archivo base + bloques por mod; `Ingest` de lo que cambia el juego) | `inputUserMappings.xml`, `options.json` | `mods.settings`, `dx12user.settings`/`input.settings`, XML de menús | `plugins.txt` |
| 5 | Load order que se escribe a archivo (variedad de sort order + writer) | `modlist` de REDmod | `Priority` de `mods.settings` | Skyrim, orden de patchers KOTOR |
| 6 | Requisitos del prefix como datos (= item de la fase 2) | `WinePrefixRequirementsEmitter` | `dinput8=n,b` para ASI, aviso DLSS bajo Proton | cualquier juego |
| 7 | Runner de herramientas Windows en el prefix (generaliza `GameToolRunner`) | deploy de REDmod | Script Merger, `wcc_lite` | HoloPatcher, xEdit |
| 8 | Archivos derivados: grupo generado con fuentes + hashes, se marca viejo y se regenera | **salida del deploy de REDmod** (hoy entra como "External Changes" y nada la invalida) | `mod0000_MergedFiles` | patchers de KOTOR, bashed patch |
| 9 | Merge 3-way de texto propio (vanilla de base, N-way, archivos con marcadores) | sin equivalente (redscript usa anotaciones) | `.ws` sin Wine | juegos con scripts de texto |

Arquetipos para elegir juegos futuros (cada candidato lleva una ficha: app ID, layout y prefix, arquetipo, herramientas externas, fuentes, qué hacen Vortex/MO2/el manager de la comunidad, piezas que faltan):

- **Archivos sueltos superpuestos** (CP2077, muchos Unity/Unreal): instaladores + 1, 2
- **Carpetas de mod + archivo de orden** (W3, Stardew/SMAPI): + 4, 5
- **Plugins con load order** (Skyrim, Fallout 4): + 5, 7 (xEdit, LOOT), 8 (bashed patch)
- **Cadena de patchers** (KOTOR, Infinity Engine/WeiDU): + 3, 7, 8 re-ejecutándose en orden

### Witcher 3: lo investigado (2026-10-03)

**Juego**
- 5.00 Remastered: solo DX12 (`bin/x64_dx12/witcher3.exe`), scripts/XML/csv/w3strings en UTF-8 (antes UTF-16LE), formato de `.bundle` nuevo (registros 0x140 → 0x130), filelists de menús eliminados, mod.io integrado (dónde guarda en PC: sin documentar)
- REDkit 5.0: overrides por scope (puede volver innecesario el Script Merger), `precompiled.rsblob`, XML con `onConflict`
- Ediciones: re 5.x, ng 4.04 y og 1.32 (betas de Steam). Detección: `launcher-configuration.json` con `remasteredEdition`, exe DX11 = legacy, `bin/config/base/freecamera.ini` = 5.x. Sonda: `content/content0/scripts/game/r4Game.ws`
- Steam 292030 (+499450 GOTY), DLC 378649/378648. ProtonDB Platinum. 5.00 detecta Wine y apaga DLSS/RT/FG; arreglado en Proton Experimental 2026-10-02: diagnóstico

**Dónde va cada cosa**
- `mods/mod*/content/` (nombre con `mod` adelante, 63 caracteres como máximo en 5.x), `dlc/<nombre>/content/` (no va en `mods.settings`), menús en `bin/config/r4game/user_config_matrix/pc/` (en 4.x registrar `name.xml;` en `dx11filelist.txt`/`dx12filelist.txt`, UTF-16), `bin/config/base/*.ini`
- Prefix `compatdata/292030/.../Documents/The Witcher 3/`: `mods.settings`, `input.settings`, `dx12user.settings`, `user.settings` (solo legacy), `gamesaves/`. No existen hasta la primera corrida
- **`[ContentManager/Mods] EnabledLocal=false`** en `dx12user.settings` apaga todo `mods/`: diagnóstico obligatorio; no pisar esas claves

**Load order:** sin `mods.settings`, orden ordinal sin mayúsculas por carpeta, gana el primero. `mods.settings`: `[carpeta] Enabled=0|1 Priority=1..9999`, menor gana, únicas; el juego agrega solo las carpetas desconocidas. Deshabilitar = `Enabled=0` (no renombrar a `~`). `mod0000_MergedFiles` siempre primero

**Instaladores** (Vortex + TW3MM, sin mayúsculas, en orden): rechazar archivos con `WitcherScriptMerger.exe`; XML de menú (saltear copias "backup"); mixto `mod*`+`dlc*`; `…/mods/modX` (quitar lo anterior); `content/` suelto → `mods/mod<Archivo>/content`; todo bajo `dlc*`. No desplegar readmes, `*.part.txt`, `__MACOSX`

**Settings:** fragmentos `input.settings.part.txt`, `user.settings.part.txt`, `dx12user.settings.part.txt` (Vortex/Settings Updater) + regex sobre readmes `.txt` (`[Context]` + `IK_*=(Action=…)`, TW3MM). Merge por sección/clave sobre una base, preservando lo que el usuario cambió en el juego; detectar choques de acción/tecla. En 5.x todo va a `dx12user.settings`

**Scripts y bundles:** conflicto = mismo `content/scripts/<ruta>` en dos o más mods habilitados. Solo hace falta merge si dos o más traen el archivo completo (los que usan `@(wrapMethod|replaceMethod|addMethod|addField)` no). Avisar si la copia del mod no tiene muchas líneas del vanilla actual (umbral SM-FAE: 50, o 10 y 10%). Bundles: leer en C# se puede; escribir `.bundle`/`metadata.store` solo con `wcc_lite` bajo Proton. Primera versión: avisar y correr Script Merger Remastered (Nexus 13076) en el prefix

**Referencias:** Vortex `extensions/games/game-witcher3` (GPL-3: `installers.ts`, `edition.ts`, `menumod.ts`, `contentManager.ts`, `modSettingsPriority.ts`, `scriptStyle.ts`); `Systemcluster/The-Witcher-3-Mod-manager` (BSD-2, soporta Proton); Script Merger IDCs/SM-FAE (GPL-2, confirmar si es "o posterior"); `TheValiantOne/WitcherScriptMerger` (.NET 10, sin interfaz, DiffPlex); W3MM (MIT, merge N-way en `script_merge.rs`; su `mods.settings` está mal)

**Sin verificar:** dónde guarda mod.io y cómo ordena contra los locales, orden de las carpetas DLC, si `--launcher-skip` sigue andando en 5.x, si el compilador de REDkit corre sin interfaz bajo Wine, impacto de *Songs of the Past*

## 🧬 Herencia de upstream a nivel repo

Hecho el 2026-09-22 (rama `feat/rename-tmodmanager`): borrados `.github/` completo (dependabot, issue templates, 17 workflows, scripts), submódulos `extern/SMAPI` y `docs/Nexus`, `docs/` + `mkdocs.yml`, `scripts/`, `codecov.yaml`, `qodana.yaml`, `CHANGELOG.md`, `CONTRIBUTING.md`, `NexusMods.App.sln.DotSettings`, `Nexus-Icon.png`, `.idea/` (untrackeado). `NuGet.Build.props` reducido a `GenerateDocumentationFile`. README con atribución a NexusMods.App (GPL-3.0). PR #25 de dependabot cerrado.

- [x] **Renombrar repo GitHub** `cp2077-mm` → `tModManager` (2026-09-22, GitHub redirige la URL vieja). URLs actualizadas en `metainfo.xml` y `app.pupnet.conf`
- [x] **Limpieza GitHub** (2026-09-22): borrados 3 deployments fallidos + environment `test` (los disparó un workflow de release de upstream el 2026-02-09 sobre la rama `remove-stuff`, antes de borrar `.github/`); borrados los 51 tags heredados de upstream (`v0.0.1`..`v0.21.1`, `0.6.1-temp`), quedan solo los 6 del fork con release (`v0.22.0`..`v0.23.4`); wiki y projects deshabilitados. El workflow dinámico "Dependabot Updates" desaparece solo. `dev.sh` opción 10 pasa `--app-version` desde el último tag git (antes el AppImage salía siempre como 1.0.0)
- [ ] **Issue templates propios** (bug + feature) si hace falta

## 🐛 Errores conocidos y deuda

Estado al 2026-09-22:

- **Issues abiertos en GitHub:** 0
- **Build:** 0 errores, 0 warnings de compilador (solo `NU19xx` de auditoría NuGet, ver arriba)
- **Comentarios `TODO`/`FIXME` en `src/`:** 99

Encontrado el 2026-09-22 con el juego real (instalación anterior modeada, restaurada por Steam con solo 20 MB de descarga):

- [x] **Deep Clean no cubre todo.** Cubierto (2026-09-24, `.nx` removal PR 3): `CyberpunkDeepCleanTool` ahora mueve la raíz del juego (todo archivo no vanilla), `r6/audioware/`, `r6/input/`, `r6/config/cybercmd/`, `r6/config/redsUserHints/`, `r6/publishing/` (diffado archivo por archivo, no wholesale: la base ya tiene vanilla ahí), `r6/logs/`, INIs de `engine/config/platform/pc/`, `engine/config/base/scripts.ini`, `r6/cache/final.redscripts*`, `r6/cache/input*.xml`, `tools/redmod/tweaks/**/devices.tweak`, `bin/x64/CyberPunk.bat`, todo contra el set vanilla de `IFileHashesService`.
- [ ] **Instaladores dejan readmes en la raíz del juego.** Deep Clean ahora limpia esos readmes/imágenes después del hecho (item de arriba), pero los instaladores los siguen deployando ahí. Vortex los manda a una carpeta aparte (`SpecialExtraFiles`); nosotros deberíamos ignorarlos o no deployarlos. Confirmado con la colección real: Apply dejó 4 readmes en la raíz (`FlatlinedExit_readme.txt`, `ItemRecordsFixes_readme.txt`, `Slaughtomatic_*_readme.txt`)
- [ ] **OAuth client_id propio.** `Auth/OAuth.cs:21` usa `"nma"`, el client de la app oficial; Nexus podría revocarlo. Registrar uno para tModManager si Nexus lo permite (probablemente no den clients a forks). Anotado 2026-09-24
- [ ] **Tests no deben tocar estado real del usuario.** Ya pasó dos veces (`.desktop` en #32, `Temp/` en #33). Revisar el resto de `AddDefaultServicesForTesting` + `AddOSInterop` real: `xdg-settings set` sigue corriendo en tests

Incidente 2026-09-25, primera prueba real del asistente de limpieza:

- [x] **"Borrar prefix de Proton" borró parte de `~/.local/share`.** `DeleteDirectory(recursive: true)` de NexusMods.Paths sigue symlinks y el prefix trae `dosdevices/z: -> /`. Se cortó a los 13 s por un archivo de solo lectura de flatpak. Perdido: KWallet (`kwalletd`), baloo y todo lo de `.local/share` creado antes que `flatpak`; `klipper` y `kactivitymanagerd` rescatados de `/proc/*/fd`; repo flatpak del usuario dañado (`flatpak repair --user`). `@home` no tenía snapshots. Arreglado: `DeleteDirectoryNoFollow` en todos los borrados recursivos, Deep Clean ignora archivos detrás de symlinks, tamaño de backups sin seguir links, tests con symlink hacia afuera. Prevención: `sudo snapper -c home create-config /home`
- [x] **Auditoría de todo lo que borra/mueve/sobrescribe** (misma rama, 3 agentes + verificación propia). Arreglado, cada uno con test que falla sin el fix:
  - `SevenZipExtractor.FixPaths` usaba los nombres crudos del archivo: una entrada `/ruta/.` o `../../x/.` borraba esa carpeta en cualquier lado del disco
  - `..` en rutas de mods (FOMOD, `collection.json`) escribía fuera del juego: `GameLocations.ToAbsolutePath` lo rechaza y los dos `RunActions` validan todo antes de tocar el disco (también que no haya carpetas-symlink en el medio)
  - El escaneo del juego ya no entra en carpetas enlazadas (antes: limpiar/desgestionar/cambiar de loadout borraba lo que había detrás del link); `ExtractFiles` reemplaza un symlink en el destino en vez de escribir a través
  - Sin lista vanilla (parche de Steam que la base de hashes no conoce, o juego agregado a mano) no hay reset, "Limpiar carpeta" ni apply en Steam: antes borraba el juego entero. **Consecuencia: después de un parche la app no aplica hasta tener hashes de la versión nueva**
  - Nombres con `\` en descargas y en entradas de archivos comprimidos (salían de la carpeta), zip de la base de hashes, mudanza de descargas viejas
  - GC y "Borrar archivos" del store: solo el primer nivel, sin links. `uninstall-app` ya no borra la Storage Location elegida, una DB que no sea RocksDB, `Backups/` ni `Downloads/`. "Borrar descargas" solo borra lo registrado en la biblioteca. Reset de la DB vieja exige marcadores de RocksDB antes de borrar nada
  - Deep Clean: nada debajo de carpetas-symlink; si un movimiento al backup falla (juego en otro disco) corta antes de tocar la DB o podar backups
  - Configs, logs, temp y base de hashes salen de `NexusMods.App/` (compartido con la app oficial) a `tModManager/`, con copia única al arrancar; el limpiador de PID viejo solo mata si el proceso sigue siendo tModManager
  - Se sacó la ubicación AppData (apuntaba a `~/.local/share` nativo, no al prefix)
- [ ] **Deep Clean `Execute` sin test de integración:** `BackupsRoot` usa el `XDG_DATA_HOME` real, así que un test escribiría (y `PruneOldBackups` podría borrar) los backups reales del usuario. Primero hacer inyectable la carpeta de backups
- [ ] **Fixture aislado de sync: archivos `archive/pc/...` de `AddModAsync` quedan en `WarnOfUnableToExtract`** (no se consideran en el store) mientras `bin/...` se despliega. Investigar si es algo del fixture o un bug real con `.archive`
- [ ] **Validar la Storage Location elegida** (rechazar `/`, `$HOME`, raíces XDG, bibliotecas de Steam). Con el GC ya acotado no es destructivo, pero mezcla el store con datos del usuario
- [ ] **Rechazar `..` al instalar el mod** (`FomodXmlInstaller.cs` destinos, `FallbackCollectionDownloadInstaller`), no solo al aplicar: hoy un solo mod con `..` hace fallar el apply de todo el loadout (no borra nada, pero bloquea)
- [ ] **Diagnóstico para carpetas-symlink dentro del juego:** el escaneo ya no entra ahí y escribir debajo bloquea el apply; mostrar qué carpeta es en vez de solo el error
- [ ] **El asistente mueve `NexusMods.App/Downloads` de la app oficial** (paso 2): si la oficial se sigue usando le rompe la biblioteca. Copiar, o mover solo lo que la base de tModManager referencia
- [ ] **Limpiador de PID viejo compara `ProcessName`:** bajo `dotnet NexusMods.App.dll` el nombre es `dotnet`. Comparar `/proc/<pid>/exe` con el ejecutable propio (ojo: el AppImage monta en un `/tmp/.mount_*` distinto cada vez)
- [ ] **Cookie de Nexus en el argv de curl** (`NexusApiClient.cs`): visible en `ps` para otros usuarios locales. Pasarla por `-H @archivo` o stdin
- [ ] **7zz 21.03 (2021) empaquetado.** Rechaza links peligrosos (probado), pero es viejo: hay CVEs posteriores (p. ej. zstd, links en ZIP). Actualizar a 25.x

Encontrado el 2026-09-24 en la eliminación de `.nx` (revisiones de implementación):

- [ ] **Doble apertura con un reset pendiente.** Dos lanzamientos dentro de la ventana de migración pueden actuar ambos como main; el segundo puede borrar la base recién creada por el primero (se pierde solo esa sesión). Arreglo: lock exclusivo sobre el marker de reset, o reclamar el slot de instancia única antes de resolver `MigrationService`
- [ ] **`CleanupUnresponsiveProcesses` (heredado de upstream) mata con SIGKILL** el PID anotado en el archivo de sync si el heartbeat tarda más de 6s. Desde el 2026-09-25 solo si el proceso sigue llamándose tModManager (un PID reusado ya no muere); un main propio ocupado todavía puede morir
- [ ] **Storage Manager: botones sin `CanExecute` atado a `IsBusy`** (solo guard dentro del cuerpo); el botón de cerrar ventana sigue activo mientras corre un paso del asistente
- [x] **Deep Clean: `Directory.Move` falla entre filesystems** (librería de Steam en otro disco o subvolumen btrfs): arreglado el 2026-10-03 con `NoFollowMove`. Sigue abierto: el `final.redscripts.bk` de redscript viejo se mueve pero un `final.redscripts` modeado se queda (Steam verify lo arregla)
- [ ] **Collections: `PackageReExtractionTests` reimplementa la secuencia restore-then-parse** en vez de correr `InstallCollectionJob` (hace falta un fixture de `CollectionRevisionMetadata` sin red)
- [x] **Deep Clean borra "My Mods" y la biblioteca no puede instalar** (visto 2026-10-04, arreglado el mismo día): Deep Clean vacía las colecciones editables en vez de borrarlas (`CyberpunkDeepCleanTool.RemoveModGroups`); la biblioteca, si no queda ninguna (loadouts ya limpiados, o "My Mods" borrada a mano con otra colección presente), instala sin destino y `InstallLoadoutItemJob` crea una sola "My Mods" bajo lock aunque se instale en paralelo
- [x] **Archivos extraídos con permisos `000`** (visto 2026-10-04, arreglado el mismo día): la versión nueva de AdaptiveSliders guarda sus `.reds` con modo unix 0 y 7zz los restauraba tal cual, así que `AddLibraryFileJob.HashAsync` fallaba con `UnauthorizedAccessException`. `FileExtractor.ExtractAllAsync` ahora da `u+rw` (`u+rwx` en carpetas) a todo lo extraído, sin seguir symlinks
- [x] **Descarga de la página del mod en vez del archivo** (visto 2026-10-04, no era un bug de descarga): el warning de `HttpDownloadJob` logueaba `DownloadPageUri` (la página del mod, que solo se guarda) en vez de la URL que realmente baja; los ~370 KB eran el archivo del CDN, que se trabó y se reanudó bien. Ahora loguea la URL del archivo (sin la query firmada) y la página aparte
- [x] **"Borrar descargas" deja lo que la biblioteca no registró** (visto 2026-10-04, arreglado el mismo día): tras un reset de la base, de 501 archivos borró 284 y dejó 217 sin registrar. Ahora, si la carpeta es la de tModManager (`DownloadsSettings.DefaultFolder`, y no es un symlink), se borra todo lo de primer nivel; si es una carpeta elegida por el usuario, solo lo registrado. Los `.tmp-` a medio escribir se respetan en los dos casos
- [x] **Log `Remaining Limit: 0` engañoso** (visto 2026-10-04, arreglado el mismo día): un header `x-rl-*` ausente se parseaba como 0. `NexusApiClient.ParseHeaders` ahora loguea el endpoint y el límite solo si vino el header ("no rate limit headers" si no); el próximo log dice qué endpoint es cuál. Confirmado con la prueba del 2026-10-04: las llamadas sin header son todas `users.nexusmods.com/oauth/userinfo` (19 en 5 min); `api.nexusmods.com/v1` sí lo manda (~19900)
- [x] **Descargas que fallan al actualizar mods** (visto 2026-10-04, arreglado el mismo día: `GenerateDownloadUrlThrottle`): con "Actualizar" sobre muchos mods, Cloudflare responde a `GenerateDownloadUrl` con su página "Just a moment..." y la descarga falla (solo queda un `DEBUG` "returned non-JSON ... skipping"; el usuario no ve por qué). El semáforo de `CallCurlGenerateDownloadUrlAsync` serializa las llamadas pero no las espacia: las que salen ~100 ms después de la anterior reciben el desafío, las separadas por ≥0,7 s pasan casi siempre (log del 2026-10-04 11:22: 39 de 65 rechazadas). Arreglo: dentro del semáforo, mínimo ~1 s entre llamadas y reintento con espera creciente (2/4/8 s) si vuelve HTML; loguear como `WARN` cuando se agotan los reintentos. Test con un `curl` falso que devuelva HTML las primeras veces
- [ ] **Instalar desde la biblioteca un mod sin descarga ni store falla en silencio** (visto 2026-10-04 probando el PR #53): tras "Borrar descargas", instalar *Buzzsaw VFX Fix* tira `InvalidOperationException` ("Faltan 1 archivo(s)… la descarga no está o cambió de contenido. Volvé a bajar el mod.") desde `InstallLoadoutItemJob` y llega como "unhandled exception in R3" (6 clics, parece que el botón no hace nada). Arreglo: `LibraryViewModel.InstallLibraryItem` atrapa la falla del job y muestra un toast con el mensaje. Además la biblioteca sigue ofreciendo como instalables ítems cuya descarga ya no existe: marcarlos o filtrarlos (va junto con "Limpiar biblioteca")

### Otros TODO relevantes en código

- `NexusMods.Library/DownloadsService.cs:46` — restaurar descargas completadas desde storage al arrancar
- `NexusMods.Networking.NexusWebApi/NexusModsLibrary.Collections.cs:239-261` — metadata de colección hardcodeada (`AdultContent`, `Summary`, `Author`)
- `NexusMods.Networking.NexusWebApi/LoginManager.cs:303` — diálogo de "necesitás login" para operaciones
- `NexusMods.Backend/FileExtractor/Extractors/SevenZipExtractor.cs:251` — sin reporte de progreso
- `NexusMods.Games.RedEngine/RedModDeployTool.cs:89` — usa sort order del loadout en vez del "Active"
- `NexusMods.Games.RedEngine/Cyberpunk2077/SortOrder/RedMod/RedModSortOrderVariety.cs:87-267` — criterio de ganador por `ModGroupId` más reciente, mejorar
- `NexusMods.Abstractions.Games/SortOrder/ASortOrderVariety.cs:147,190` — sin retry ante data race en transacción
- `NexusMods.Games.RedEngine/Cyberpunk2077/Emitters/PatternBasedDependencyEmitter.cs:75` — usar index scan ordenado
- `NexusMods.App.UI/Settings/ExperimentalSettings.cs:19` — remover para GA
- `NexusMods.Backend/FileExtractor/FileExtractor.cs` (`ExtractAllAsync`) — traga la cancelación de cada intento de extractor y relanza `FileExtractionException` en vez de `OperationCanceledException`; el re-extractor lo esquiva con `ThrowIfCancellationRequested` explícito
