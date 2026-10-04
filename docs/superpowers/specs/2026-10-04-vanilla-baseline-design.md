# Lista de archivos originales sin la base de Nexus (pieza 1)

Fecha: 2026-10-04 · Estado: diseño aprobado, sin implementar

## Objetivo

Que la app sepa qué archivos del juego son originales sin depender de la base de hashes de Nexus
(`Nexus-Mods/game-hashes`, congelada: la última release es del 2025-09-30 y no trae Witcher 3). Es la
pieza 1 de "Piezas genéricas para el segundo juego" en `TODO.md` y bloquea a Witcher 3: sin ella no se
pueden sacar mods.

### Qué se decidió (2026-10-04)

- **Fuente combinada:** la base de Nexus manda cuando conoce la versión instalada; si no, una foto del
  disco tomada por la app. Sin red ni login a Steam (Steam no deja los manifests en disco:
  `steamapps/depotcache` está vacío).
- **Parches sin señal** (juego agregado a mano, GOG): no se adopta nada solo. Los cambios van a
  "External Changes" como hoy y un botón "Actualicé el juego" vuelve a tomar la foto.
- **Implementación:** modelo propio (`GameBaselineFile`), no reusar `AsOf(InitialDiskStateTransaction)`
  ni fabricar una base con el formato de Nexus.
- Ante la duda, no borrar: la regla siempre cae del lado de tomar algo como original.

### Criterios de éxito

1. Un juego agregado a mano nunca pierde archivos en la primera sincronización.
2. Con una versión de Steam que Nexus no conoce (Cyberpunk tras un parche nuevo, Witcher 3 siempre):
   instalar, aplicar, sacar mods y resetear funcionan; el reset restaura originales desde la foto.
3. Con una versión que Nexus conoce, el comportamiento no cambia (los mods preexistentes se siguen
   detectando como no originales).
4. Un parche de Steam desconocido no borra nada y adopta los archivos que cambió.
5. Nada del core asume que la fuente es Nexus.

### Fuera de alcance

- Ubicaciones dentro del prefix (`Documents/...`, AppData): pieza 2. La foto cubre lo que hoy indexa
  la app.
- El atajo de hashing al indexar (`GameLocationsService.GetExistingHash`): Witcher 3 hashea todo
  completo la primera vez.
- Login a Steam para bajar manifests de depots.
- `ReprocessOverrides`: sigue usando solo la base de Nexus (es su mecanismo de "la base aprendió la
  versión").
- Arreglar `BuildHashesDb.cs:136` (revienta con juegos no registrados): deja de importar.

## Contexto (estado actual)

- **Layer 0 del sincronizador = base de Nexus, en SQL.** `Synchronizer.sql:67-76` (`WinningFiles`) toma
  los archivos del juego de `file_hashes.loadout_files(db)` (`FileHashesQueries.sql:26-48`), que une
  `Loadout.LocatorIds` → `SteamManifest` → `PathHashRelation` → `HashRelation`. Solo Steam; un juego
  agregado a mano no tiene IDs de manifest.
- **Primera sincronización.** El fork cambió `GetPreviouslyAppliedDiskState`
  (`ILoadoutSynchronizer.cs:62-84`, commit `09d577abf`) para usar `InitialDiskStateTransaction` como
  estado anterior. Con eso, todo archivo en disco que no esté en el layer 0 se mapea a
  `BackupFile | DeleteFromDisk` (`Rules/ActionMapping.cs:120-131`).
- **Freno actual.** `ALoadoutSynchronizer.cs:482-485` llama a `EnsureVanillaDataKnown` (`:568-575`)
  antes de borrar, pero solo para Steam. Riesgo sin verificar: un juego agregado a mano tiene layer 0
  vacío y ningún freno, así que la primera sincronización haría backup (tope 5 GB) y borraría `bin/`,
  `r6/`, `engine/`.
- **Otros consumidores de la base** (vía `IFileHashesService.GetGameFiles` / `UnknownLocatorIds`):
  `ResetToOriginalGameState` (`ALoadoutSynchronizer.cs:1461-1512`, usado al desactivar/borrar
  loadouts y al dejar de gestionar), `CyberpunkDeepCleanTool.ResolveVanilla` (`:124-142`),
  `MyGamesViewModel.HasVanillaData` (`:614-615`), `ReprocessOverrides` (`:681-724`).
- **Foto inicial existente.** `ReindexState` (`ALoadoutSynchronizer.cs:1333-1357`) marca
  `InitialDiskStateTransaction` la primera vez; las `DiskStateEntry` se mutan después, así que la foto
  solo se reconstruye con `AsOf`. Nunca se refresca e incluye lo que hubiera en disco (mods
  incluidos).
- **Versión.** `SteamLocator` toma los IDs de manifest de `appmanifest_<appid>.acf`;
  `UpdateLocatorIds` (`:629-679`) los refresca al inicio de cada `Synchronize` y busca la versión en
  la base.

## Diseño

### 1. Datos y fuente de verdad

**Modelo nuevo** en `NexusMods.Abstractions.Loadouts`:

```csharp
public partial class GameBaselineFile : IModelDefinition
{
    private const string Namespace = "NexusMods.Loadouts.GameBaselineFile";
    public static readonly ReferenceAttribute<GameInstallMetadata> Game = new(Namespace, nameof(Game)) { IsIndexed = true };
    public static readonly GamePathParentAttribute Path = new(Namespace, nameof(Path)) { IsIndexed = true };
    public static readonly HashAttribute Hash = new(Namespace, nameof(Hash));
    public static readonly SizeAttribute Size = new(Namespace, nameof(Size));
}
```

(Los nombres de atributos siguen el patrón de `DiskStateEntry`; se ajustan al implementar si el
generador pide otra forma.)

**"Nexus conoce la versión"** significa una sola cosa en todo el diseño: la base tiene archivos para
**todos** los IDs de manifest del loadout (`UnknownLocatorIds` vacío). Un juego agregado a mano (sin
IDs) nunca cuenta como conocido. No se mezclan fuentes: o todo Nexus o todo la foto.

**Una sola función de lectura** en el core: "archivos originales de este loadout". Devuelve los de la
base de Nexus si conoce la versión; si no, la foto de la instalación. Si no hay ninguna de las dos,
devuelve "desconocido".

La usan:
- el freno de `ALoadoutSynchronizer` (`EnsureVanillaDataKnown`), que pasa a exigir "Nexus conoce la
  versión **o** hay foto" y aplica a **cualquier** tienda;
- `ResetToOriginalGameState`;
- `CyberpunkDeepCleanTool.ResolveVanilla`;
- `MyGamesViewModel.HasVanillaData`.

**SQL:** el layer 0 de `WinningFiles` toma las filas de `file_hashes.loadout_files` solo para los
loadouts cuya versión conoce Nexus, y las de la foto para el resto (`UNION` de las dos ramas con
condiciones excluyentes). Hoy un loadout con algunos IDs conocidos y otros no recibe las filas
parciales de Nexus; con el cambio recibe la foto. La foto guarda `GamePath` completo (ubicación +
ruta), así que no se fuerza `Location = 'Game'` como en la rama de Nexus.

### 2. Cuándo se toma la foto

Disparadores, todos con la misma regla:

1. **Al gestionar el juego:** en `ReindexState`, en la misma transacción que marca
   `InitialDiskStateTransaction`, antes de armar el árbol de la primera sincronización.
2. **Instalación existente sin foto:** al inicio de `Synchronize`, si la instalación no tiene foto.
   (Cubre el Cyberpunk ya gestionado, sin migración.)
3. **Parche de Steam con versión desconocida:** en `Synchronize`, si `UpdateLocatorIds` ve IDs
   distintos a los anteriores y la base de Nexus no los conoce, se vuelve a tomar antes de armar el
   árbol.
4. **Botón "Actualicé el juego"** en el menú del juego en Mis juegos (junto a Deep Clean): reindexa y
   vuelve a tomar la foto. Para juegos agregados a mano, GOG, o si algo salió mal.

**Regla**, sobre el disco recién indexado y la foto anterior (vacía la primera vez):

| Situación en disco | Resultado en la foto |
|---|---|
| Ruta de un archivo de mod (layer 1) y hash en disco = hash del mod | se conserva la entrada anterior para esa ruta, si había |
| Ruta en "External Changes" (layer 2) | se conserva la entrada anterior, si había |
| Cualquier otro archivo | entra con su hash y tamaño actuales |
| Entrada anterior cuyo archivo ya no está y no es de un mod | sale |

Si Nexus conoce la versión en ese momento, solo se guardan los archivos cuya ruta y hash
coinciden con la lista de Nexus: la foto arranca limpia para el próximo parche desconocido.

**Aviso:** cuando la foto se toma sin que Nexus conozca la versión (disparadores 1-3), toast: "No
conozco esta versión: tomo como original todo lo que hay en la carpeta. Si ya tenías mods puestos a
mano, sacalos y tocá 'Actualicé el juego'".

**Límites conocidos (aceptados):**
- Un mod tirado a mano entre la última sincronización y un parche desconocido queda como original:
  no se borra, pero el reset tampoco lo saca.
- Un original borrado a mano sale de la foto; el reset ya no lo recupera.
- En una instalación existente sin foto y con versión desconocida, un original pisado por un mod no
  entra a la foto (no hay entrada anterior); el reset no lo restaura aunque su backup exista.

### 3. Pruebas

Cada una tiene que fallar sin su parte del cambio:

1. **Riesgo actual (primero):** juego agregado a mano, primera sincronización: hoy borra `bin/`
   (confirmar); con el cambio no borra nada.
2. Versión de Steam desconocida: instalar un mod, aplicar, sacarlo y resetear funcionan; el reset
   restaura originales desde la foto.
3. Regla, caso por caso: ruta de mod conserva la entrada anterior, "External Changes" conserva,
   original parcheado se adopta, archivo borrado sale.
4. Parche de Steam simulado (IDs nuevos desconocidos + un original modificado): no se borra nada y se
   adopta.
5. Versión conocida por Nexus: la foto es la intersección; un mod preexistente no cuenta como
   original (los tests actuales de `ExternalChangesTests` siguen pasando).
6. `ResolveVanilla` de Deep Clean usa la foto cuando Nexus no conoce la versión.
7. Foto tomada sobre una instalación existente (disparador 2).
8. Reset con foto y una carpeta symlink que apunta afuera: no la sigue.

El modelo nuevo actualiza el snapshot de esquema de `NexusMods.DataModel.SchemaVersions.Tests`; no
hace falta subir la versión de esquema (no hay migración).

## Orden de trabajo

Una rama (`feat/vanilla-baseline`), un PR, un commit por paso:

1. Test del riesgo con juego agregado a mano.
2. Modelo, función de lectura y `UNION` en `Synchronizer.sql`.
3. Regla de la foto + disparadores 1 y 2.
4. Disparador 3 (parche de Steam).
5. Botón "Actualicé el juego" + toast.
6. Consumidores a la función nueva: freno (todas las tiendas), reset, Deep Clean, UI.
7. `TODO.md` (pieza 1 y "Lista de archivos originales" de la fase 2) y `CLAUDE.md` (layer 0 ya no
   depende solo de Nexus).

Prueba real con `./dev.sh` opción 11: la instalación actual (versión conocida) no cambia de
comportamiento. El caso desconocido se prueba de verdad con Witcher 3 o simulando un parche.
