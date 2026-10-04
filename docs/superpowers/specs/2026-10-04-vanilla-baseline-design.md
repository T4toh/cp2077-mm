# Lista de archivos originales sin la base de Nexus (pieza 1)

Fecha: 2026-10-04 · Estado: implementado en la rama feat/vanilla-baseline (PR pendiente). Ajustado al escribir el plan (ver "Ajustes del plan")

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
  versión"), y solo corre cuando la lista salió de Nexus.
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

**Marca en la instalación:** `GameInstallMetadata.BaselineFromDisk` (`BooleanAttribute`, opcional).
Presente = la lista ya se armó; `true` = salió del disco (versión desconocida), `false` = de Nexus
(más las entradas conservadas de la lista anterior, ver abajo).

**"Nexus conoce la versión"** significa una sola cosa en todo el diseño: la tienda es Steam y la base
tiene archivos para **todos** los IDs de manifest del loadout (`UnknownLocatorIds` vacío). Es la misma
condición que hoy usa el SQL (`Store = 'Steam'`). Un juego agregado a mano nunca cuenta como conocido.

**La lista es siempre `GameBaselineFile`.** Si Nexus conoce la versión, la lista se llena con los
archivos de Nexus para esos IDs (la lista completa, igual que el layer 0 de hoy); si no, con la regla de
la foto. Al rearmar desde Nexus, las entradas de la lista anterior que Nexus no lista (de una lista del
disco, o adoptadas por el botón) se conservan si el disco tiene esa ruta (con cualquier contenido: una
edición desde el último sync pasa a "External Changes" contra el original conservado) o la ruta es del
loadout: nada que era original pasa a ser un sobrante. Nadie más lee la base de Nexus para saber qué es original.

La leen:
- el layer 0 en SQL;
- el freno de `ALoadoutSynchronizer` (`EnsureVanillaDataKnown`), que pasa a exigir "la lista existe"
  y aplica a **cualquier** tienda;
- `ResetToOriginalGameState` (pierde el parámetro de IDs: usa la lista de la instalación);
- Deep Clean (reemplaza `ResolveVanilla`, que se borra);
- `MyGamesViewModel.HasVanillaData`.

`ReprocessOverrides` sigue con la base de Nexus, pero no corre mientras la lista salió del disco (o no
hay lista): con IDs conocidos a medias sacaría de las overrides archivos que la lista no tiene, y el
próximo sync los borraría. Su trabajo lo hace el rearmado cuando la base aprende la versión.

**SQL:** el layer 0 de `WinningFiles` pasa a ser solo `GameBaselineFile` unido a `Loadout` por la
instalación; `file_hashes.loadout_files` sale de `Synchronizer.sql` (y sus macros, si no las usa nadie
más). La lista guarda `GamePath` completo (ubicación + ruta).

### 2. Cuándo se toma la foto

Disparadores, todos con la misma regla:

1. **Al gestionar el juego y en instalaciones existentes:** en `Synchronize`, después de
   `UpdateLocatorIds` y antes de armar el árbol, si la instalación no tiene la marca. La primera
   sincronización de `CreateLoadout` pasa por acá, así que un juego nuevo nunca se sincroniza sin
   lista. Cubre el Cyberpunk ya gestionado, sin migración.
2. (Fusionado con el 1.)
3. **Cambio de IDs de manifest:** en `Synchronize`, si `UpdateLocatorIds` cambió los IDs, se rearma la
   lista (de Nexus si conoce la versión nueva, con la regla si no).
4. **Botón "Actualicé el juego"** en el menú del juego en Mis juegos (junto a Deep Clean): reindexa y
   vuelve a tomar la foto. Para juegos agregados a mano, GOG, o si algo salió mal. Como es una acción
   explícita, al rearmar desde el disco adopta los "External Changes" que siguen iguales en disco
   (archivos, no borrados, fuera de rutas de mods y de archivos intrínsecos) y los saca de las overrides.

Una lista armada desde el disco que no tiene el archivo principal del juego (`GetPrimaryFile`) no se
guarda: queda la anterior (o ninguna). Al cambiar los IDs se borra la marca junto con los IDs viejos,
así un rearmado que falla deja "sin lista" en lugar de la lista vieja. Si la lista salió del disco y
la base de Nexus aprende esa versión después (mismos IDs), se rearma desde Nexus.

**Regla**, sobre el disco recién indexado y la foto anterior (vacía la primera vez):

| Situación en disco | Resultado en la foto |
|---|---|
| Ruta de un archivo de mod (layer 1) y hash en disco = hash del mod | se conserva la entrada anterior para esa ruta, si había |
| Ruta en "External Changes" (layer 2), borrada a propósito (`Deleted`) o archivo intrínseco (layer 3, lo genera la app) | se conserva la entrada anterior, si había |
| Cualquier otro archivo | entra con su hash y tamaño actuales |
| Entrada anterior cuyo archivo ya no está y no es de un mod | sale |

Si Nexus conoce la versión en ese momento, la lista es la de Nexus completa más lo conservado de la
lista anterior (ver sección 1): sirve de entrada anterior para el próximo parche desconocido.

**Aviso:** los disparadores 1 y 3 corren en segundo plano dentro del sincronizador, que no puede
mostrar toasts. Cuando la lista salió del disco, el widget del juego en Mis juegos dice "Versión
desconocida: originales tomados del disco" en lugar de "versión desconocida", y el tooltip del botón
"Actualicé el juego" explica qué hacer si había mods puestos a mano. El botón sí muestra un toast al
terminar, y uno de error si la lista del disco se rechaza (falta el archivo principal o lo pone un mod).

**Al dejar de gestionar** el juego se borran la lista y la marca, junto con el estado de disco.

**Límites conocidos (aceptados):**
- Un mod tirado a mano entre la última sincronización y un parche desconocido queda como original:
  no se borra, pero el reset tampoco lo saca.
- Lo mismo con los archivos que los mods crean o cambian mientras el juego corre (configs de CET,
  `db.sqlite3`, `final.redscripts.bk`) entre la última sincronización y un rearmado por cambio de IDs:
  quedan como originales. En Cyberpunk es más común que el mod tirado a mano. El botón "Actualicé el
  juego" hace lo mismo con los que ya estaban en "External Changes" fuera de rutas de mods.
- Un original borrado a mano sale de la foto; el reset ya no lo recupera.
- Lo que la lista conserva al rearmar desde Nexus (entradas que Nexus no lista) sigue como original
  mientras siga en disco, también tras parches conocidos: incluye archivos de una versión vieja
  que Steam no borró. El sync no los borra y el reset no los saca.
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
5. Versión conocida por Nexus (tienda Steam): la lista es la de Nexus; un archivo preexistente que
   Nexus no lista no cuenta como original.
6. Deep Clean y "¿hay lista?" leen la lista de la instalación (sin marca: no hay lista).
7. Foto tomada sobre una instalación existente (disparador 2).
8. Reset con foto y una carpeta symlink que apunta afuera: no la sigue.

El modelo nuevo actualiza el snapshot de esquema de `NexusMods.DataModel.SchemaVersions.Tests`; no
hace falta subir la versión de esquema (no hay migración).

## Ajustes del plan (2026-10-04)

Al leer el código para el plan cambiaron cuatro cosas de implementación, aprobadas junto con el plan:

1. La lista es siempre `GameBaselineFile`; si Nexus conoce la versión se llena con la lista de Nexus
   completa (antes: disco ∩ Nexus, y el SQL elegía entre dos ramas). El SQL deja de depender de Nexus.
2. "Nexus conoce" exige tienda Steam, como el SQL de hoy. En los tests la tienda es `Unknown`, así que
   los tests existentes pasan por la regla de la foto (hoy su layer 0 por SQL está siempre vacío).
3. Aviso en el widget en lugar de toast para los disparadores automáticos.
4. `ResetToOriginalGameState` pierde el parámetro de IDs.

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
