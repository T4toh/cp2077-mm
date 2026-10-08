# tModManager

Gestor de mods para juegos de **Steam** en **Linux** (Proton). Por ahora soporta **Cyberpunk 2077**; el soporte multi-juego (Witcher 3 primero) está en camino, ver [TODO.md](TODO.md).

Nace como fork de [NexusMods.App](https://github.com/Nexus-Mods/NexusMods.App) (GPL-3.0), discontinuado por Nexus Mods; el código base y la licencia se heredan de ahí, el desarrollo sigue acá con foco en Linux.

## 📦 Instalación

1. Bajar `tModManager.x86_64.AppImage` de la [última release](https://github.com/T4toh/tModManager/releases/latest) (el `.sha256.txt` al lado sirve para verificarlo con `sha256sum -c`).
2. `chmod +x tModManager.x86_64.AppImage` y abrirlo. Necesita FUSE 2 (`fuse2` en Arch).
3. Al arrancar registra el handler `nxm://`, así los botones "Mod Manager Download" de Nexus Mods abren la app.

Requisitos opcionales:

- **Firefox** con sesión iniciada en nexusmods.com, para bajar colecciones enteras sin Premium (ver [Descarga directa](#-cómo-funciona-la-descarga-directa)).
- **protontricks** (nativo o flatpak), para instalar `d3dcompiler_47` y `vcrun2022` en el prefix desde la app.

## ✨ Qué tiene

### Descarga sin Premium

Usuarios free y supporter pueden bajar todos los mods de una colección con un clic. La app lee la sesión de Firefox y usa `curl` para pasar el TLS fingerprinting de Cloudflare. Antes de bajar, busca en Descargas lo que ya está (por MD5, o nombre y tamaño) para no bajarlo de nuevo.

### Descargas, store y reextracción

- Las descargas originales quedan en `~/.local/share/tModManager/Downloads`.
- Los archivos extraídos van a un store propio (`DataModel/Archives`, direccionado por hash). Si falta algo (borrado a mano, Deep Clean, limpieza), se reextrae de la descarga original al instalar o aplicar; solo se vuelve a bajar de Nexus si la descarga tampoco está.
- Detecta descargas vacías y el tipo real del archivo (7z/zip/rar) por sus primeros bytes cuando el servidor no manda extensión.

### Archivos originales del juego

La app guarda la lista de archivos vanilla de la instalación: de la base de hashes de Nexus si conoce la versión de Steam, del disco si no. Después de un parche, el botón **"Actualicé el juego"** la rehace. Reset, Deep Clean y Apply se apoyan en esa lista para no borrar nada original.

### Deep Clean y Storage Manager

Deep Clean mueve a `~/.local/share/tModManager/Backups/<juego>/<fecha>/` las carpetas de mods (`red4ext`, `r6/scripts`, `r6/tweaks`, `bin/x64/plugins`, `archive/pc/mod`, …), las DLL inyectadas (`d3d11.dll`, `winmm.dll`, `version.dll`, `powrprof.dll`) y los archivos sueltos que no están en la lista vanilla; conserva el backup nuevo y el anterior, saca los grupos de mods de la base (la biblioteca queda para reinstalar sin bajar) y vuelve a escanear el juego.

El Storage Manager borra por separado el store, las descargas, los backups y el **prefix de Proton** (`steamapps/compatdata/1091500`; avisa que después hacen falta `vcrun2022` y `d3dcompiler_47`). Ningún borrado sigue symlinks: un prefix trae `dosdevices/z: -> /`. Si quedan datos del formato `.nx` anterior, un asistente guía la limpieza.

### Wine/Proton

- Panel **"Wine prefix"** en Mis juegos: revisa `winetricks.log` y los DLL overrides (`WINEDLLOVERRIDES`), muestra el comando de protontricks con "Copiar" e "Instalar".
- Juego y prefix se pueden indicar a mano; lee configs de Lutris.

### Colecciones

- Instalación resistente: si falta el `collection.json` o un archivo del store, se reextrae; Apply funciona con instalaciones parciales; si el MD5 de un archivo no coincide (mod actualizado), prueba por ruta relativa.
- Avisa antes de instalar una colección si ya hay otra.
- Jugar y Aplicar quedan deshabilitados mientras se instala, y Jugar hasta aplicar.

### Diagnósticos de mods esenciales

Avisa si faltan Redscript, RED4ext, Cyber Engine Tweaks, ArchiveXL, TweakXL, Codeware o Equipment-EX, si un mod trae carpetas duplicadas (`Cyberpunk 2077/bin/...`) y si falta una dependencia detectable por patrones de archivo.

### Aislamiento y privacidad

- **App ID** `io.github.t4toh.tmodmanager`, datos en `~/.local/share/tModManager/` y handler `nxm://` propio: convive con la app oficial. Los datos de la versión anterior (`NexusMods.App.Cyberpunk/`) se migran solos.
- **Sin telemetría:** Matomo, Mixpanel y OpenTelemetry eliminados; la app no manda datos propios a ningún servidor.

## 🛠 Arquitectura

| Componente         | Tecnología                                                       |
| ------------------ | ---------------------------------------------------------------- |
| **Lenguaje**       | C# / .NET 10                                                     |
| **UI**             | Avalonia UI (MVVM con R3/ReactiveUI)                             |
| **Base de datos**  | MnemonicDB (inmutable, EAV)                                      |
| **Sincronización** | Diff de tres vías (disco anterior, disco actual, loadout); copia los archivos al juego |
| **Empaquetado**    | AppImage vía PupNet                                              |

| Proyecto(s)                           | Propósito                                                         |
| ------------------------------------- | ----------------------------------------------------------------- |
| `NexusMods.App`                       | Entry point; DI, Avalonia UI o CLI                                |
| `NexusMods.App.UI`                    | Vistas Avalonia + ViewModels                                      |
| `NexusMods.Backend`                   | Interop Linux, extracción, localizadores de juego (Steam, manual) |
| `NexusMods.DataModel`                 | Persistencia, sincronizador, store, Storage Manager               |
| `NexusMods.Library`                   | Biblioteca de mods y reextracción desde Descargas                 |
| `NexusMods.Collections`               | Descarga e instalación de colecciones                             |
| `NexusMods.Sdk`                       | Utilidades compartidas, borrado sin seguir symlinks, settings     |
| `NexusMods.Abstractions.*`            | Interfaces entre subsistemas                                      |
| `NexusMods.Games.RedEngine`           | Cyberpunk 2077                                                    |
| `NexusMods.Games.FileHashes`          | Base de hashes para detectar la versión del juego (Steam)         |
| `NexusMods.Networking.NexusWebApi`    | API de Nexus Mods + lectura de cookies de Firefox                 |

Más detalle en [CLAUDE.md](CLAUDE.md).

## 🏗 Cómo construir

Requiere el SDK de .NET 10.

```bash
dotnet build
dotnet run --project src/NexusMods.App/NexusMods.App.csproj
```

`./dev.sh` tiene un menú en castellano: compilar, correr, tests, AppImage. Lo más usado:

- **Opción 4:** suite completa sin tests de red, un proyecto por vez (no hay CI: la verificación es local). Nunca `dotnet test` sobre la solución entera: corre todo en paralelo y casi congela la PC.
- **Opción 10:** AppImage en `src/NexusMods.App/Deploy/OUT/` (requiere `dotnet tool install --global KuiperZone.PupNet` y FUSE). Toma la versión del último tag de git.
- **Opción 11:** abre el build Release dentro de una jaula de bubblewrap (`sandbox-run.sh`): todo el disco en solo lectura salvo los datos de la app, la carpeta del juego y su prefix, con un snapshot de `/home` antes si hay snapper. Para probar Deep Clean y borrados con datos reales.

## 🔐 Cómo funciona la descarga directa

Nexus Mods usa Cloudflare, que reconoce clientes que no son navegadores por su **TLS fingerprint**: al `HttpClient` de .NET le contesta con una URL vacía (`https://cf-files.nexusmods.com/cdn///`). `curl` tiene un fingerprint aceptado, así que la app lo usa como proceso aparte solo para pedir el link.

```
1. Del collection.json salen FileId + GameId de cada mod.
2. Por cada mod (de a uno, con al menos 1 s entre llamadas):
   a. Leer las cookies de nexusmods.com del perfil de Firefox.
   b. curl POST https://www.nexusmods.com/Core/Libs/Common/Managers/Downloads?GenerateDownloadUrl
      con User-Agent de Firefox; la cookie va por stdin (-H @-), así no aparece en `ps`.
   c. Respuesta: {"url": "https://files.nexus-cdn.com/...?md5=...&expires=..."}.
      Si vuelve la página de desafío de Cloudflare, reintenta a los 2, 4 y 8 s.
3. El archivo se baja del CDN con el descargador normal, hasta 5 en paralelo (Settings → Downloads).
```

Lectura de cookies (`FirefoxCookieReader.cs`): busca `profiles.ini` en `~/.mozilla/firefox` y `~/.config/mozilla/firefox`, toma el perfil por defecto, copia `cookies.sqlite` a un temporal (Firefox lo tiene bloqueado) y lo lee con `libsqlite3.so.0`.

Limitaciones:

- **Solo Firefox.** Chrome y derivados encriptan las cookies con una clave del keyring (Secret Service, "Chrome Safe Storage") y AES; habría que desencriptarlas.
- **Velocidad:** las cuentas free bajan a ~300-500 KB/s por archivo (límite del servidor).
- **Sesión:** hay que tener la sesión iniciada en el navegador; la app no hace el login del sitio.

---

_Este proyecto no está afiliado con Nexus Mods._
