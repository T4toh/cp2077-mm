# NexusMods.App - Cyberpunk 2077 (Linux/Steam Fork)

Este es un fork especializado de NexusMods.App, enfocado exclusivamente en la gestión de mods para **Cyberpunk 2077** en **Linux** a través de **Steam (Proton)**.

## 🎯 Objetivo del Proyecto

Proporcionar un gestor de mods moderno, nativo para Linux, que resuelva las complejidades de usar herramientas de Windows bajo Wine/Proton. Este fork prioriza la compatibilidad con el sistema de archivos de Linux y la integración con Steam Deck y escritorios Linux.

## ✨ Diferencias y Mejoras (vs Upstream)

- **Aislamiento del Sistema:** Este fork utiliza su propio ID de aplicación (`com.nexusmods.app.cyberpunk`) y directorios de datos independientes (`~/.local/share/NexusMods.App.Cyberpunk`). Esto permite que conviva con la versión oficial sin romper bases de datos ni conflictos de protocolos.
- **Descargas Compartidas:** A pesar del aislamiento, el fork comparte la carpeta de descargas con la versión oficial para ahorrar espacio en disco y evitar re-descargas.
- **Limpiador Profundo (Deep Clean) Nativo:** Integrada una herramienta de limpieza que desactiva y respalda automáticamente todos los mods instalados manualmente o por otros gestores. Mueve carpetas críticas (`red4ext`, `plugins`, `r6/scripts`, `r6/tweaks`, etc.) a un directorio de backup con marca de tiempo en `~/.local/share/NexusMods.App/CyberpunkBackups/<timestamp>/`. El pipeline de sincronización ignora correctamente esas carpetas de backup para evitar que el sincronizador las toque.
- **Sin Telemetría:** Eliminados completamente los tres sistemas de tracking del upstream (Matomo, Mixpanel y OpenTelemetry). La app no envía ningún dato a servidores externos.
- **Foco Único:** Eliminado el soporte para GOG, EGS, Windows, macOS y otros juegos para reducir la complejidad y el tamaño del binario.
- **Detección Manual de Juego:** Permite especificar manualmente la ruta de instalación de Cyberpunk 2077 y el prefijo de WINE (Proton), facilitando el soporte para instalaciones en discos secundarios o Steam Deck.
- **Gestión de Colecciones Global:** Ahora puedes navegar y descargar colecciones incluso si no tienes un juego gestionado o instalado.
- **Escaneo de Descargas (Rescan):** Implementado un sistema de detección por MD5 que identifica archivos ya descargados en tu carpeta de `Downloads`, evitando descargas redundantes.
- **Transparencia en Colecciones:** Nueva pestaña "Mod List" que permite ver el detalle de cada mod en una colección, sus hashes, enlaces originales y copiarlos al portapapeles.
- **Descarga de Colecciones sin Premium:** Usuarios free y supporter pueden descargar todos los mods de una colección con un solo click. El sistema usa las cookies de sesión del browser para autenticarse directamente contra los servidores de Nexus Mods, sin requerir la API premium. Ver [Cómo funciona la descarga directa](#-cómo-funciona-la-descarga-directa) más abajo.
- **Corrección de Archivos de Respaldo:** Los backups ahora mantienen sus extensiones originales (`.zip`, `.rar`, etc.) para asegurar que sean reconocibles por otras herramientas de compresión en Linux.
- **Rutas Linux Nativas:** Uso correcto de `XDG_DATA_HOME` para almacenar configuraciones y archivos descargados.
- **Soporte Lutris:** El parser de Wine detecta anulaciones de DLL configuradas en Lutris (`WINEDLLOVERRIDES`), mejorando la compatibilidad con instalaciones fuera de Steam.

## 🛠 Arquitectura

- **Lenguaje:** C# / .NET 10.
- **UI:** Avalonia UI (MVVM con R3/ReactiveUI).
- **Base de Datos:** MnemonicDB (Inmutable, EAV).
- **Sincronización:** Sistema de enlaces simbólicos/hardlinks para no duplicar espacio en disco.

## 🚀 Próximos Pasos (Checklist)

- [x] **Descarga Automatizada de Colecciones:** Un click descarga todos los mods de la colección directamente en la app (sin premium, sin abrir el browser).
- [x] **Captura de Enlaces NXM:** Implementado mediante aislamiento de ID de aplicación, permitiendo registro independiente del protocolo nxm.
- [x] **Vista Unificada de Descargas:** El panel de descargas muestra todos los jobs en una vista consolidada (sin separación por juego).
- [ ] **Soporte Multi-Browser para Cookies:** Actualmente se leen cookies solo desde Firefox. Ver [Cómo funciona la descarga directa](#-cómo-funciona-la-descarga-directa) para implementar soporte para Chrome/Chromium.
- [ ] **Chequeo de Frameworks Específicos:** Verificación automática de la presencia y versión de mods base críticos:
    - [ ] REDmod
    - [ ] Cyber Engine Tweaks (CET)
    - [ ] redscript
    - [ ] ArchiveXL / TweakXL
- [ ] **Optimización de Rescan:** Mejorar el rendimiento del escaneo MD5 para carpetas con cientos de archivos.
- [ ] **UI de Progreso Global:** Centralizar el estado de todas las descargas activas en una vista unificada.

## 🏗 Cómo Construir

```bash
dotnet build
dotnet run --project src/NexusMods.App/NexusMods.App.csproj
```

Para generar una AppImage (requiere `pupnet`):

```bash
./dev.sh
```

## 🔐 Cómo Funciona la Descarga Directa

Este sistema permite descargar mods de Nexus Mods sin la API premium, usando la sesión de tu browser.

### El problema: TLS Fingerprinting

Nexus Mods usa Cloudflare, que detecta clientes no-browser mediante **TLS fingerprinting** (análisis del handshake TLS). El `HttpClient` de .NET tiene un fingerprint diferente al de Firefox/Chrome, por lo que Cloudflare responde con una URL vacía (`https://cf-files.nexusmods.com/cdn///`) en vez del link de descarga real.

**Solución:** invocar `curl` como subprocess. Curl tiene un fingerprint aceptado por Cloudflare y devuelve la URL CDN real.

### Flujo completo

```
1. Colección JSON → lista de FileId + GameId por cada mod
2. Para cada mod (serializado, máx. 1 curl a la vez):
   a. Leer cookies de sesión del perfil Firefox (~/.mozilla/firefox o ~/.config/mozilla/firefox)
   b. curl -X POST https://www.nexusmods.com/Core/Libs/Common/Managers/Downloads?GenerateDownloadUrl
         -H "Cookie: <session cookies>"
         -H "User-Agent: Mozilla/5.0 (Firefox)"
         --data "fid=<fileId>&game_id=<gameId>"
   c. Respuesta JSON:
      - Free/Supporter: {"url": "https://files.nexus-cdn.com/<path>?md5=...&expires=..."}
      - Premium:        [{"name":"CDN Name","URI":"https://..."}]
3. Descargar el archivo desde la URL CDN con el HttpDownloadJob existente
4. Máx. 5 descargas en paralelo (configurable en Settings → Downloads)
```

### Lectura de cookies del browser

El archivo `FirefoxCookieReader.cs` hace:
1. Busca `profiles.ini` en `~/.mozilla/firefox` y `~/.config/mozilla/firefox`
2. Resuelve el perfil por defecto (primero la entrada `[Install...]Default=`, luego el primer `[Profile...]`)
3. Copia `cookies.sqlite` a un archivo temporal (para evitar el lock de SQLite cuando Firefox está abierto)
4. Lee todas las cookies con `host LIKE '%nexusmods.com%'` via P/Invoke a `libsqlite3.so.0`

### Agregar soporte para otro browser

Para soportar Chrome/Chromium (o cualquier browser basado en Chromium) hay que tener en cuenta dos diferencias:

**1. Ubicación de la base de datos**
```
Chrome:    ~/.config/google-chrome/Default/Cookies
Chromium:  ~/.config/chromium/Default/Cookies
Brave:     ~/.config/BraveSoftware/Brave-Browser/Default/Cookies
```

**2. Cookies encriptadas (DPAPI en Linux = Secret Service)**

A diferencia de Firefox, Chrome/Chromium **encripta los valores** de las cookies usando el sistema de claves del OS:
- En Linux, usa `libsecret` (freedesktop Secret Service) para guardar la clave de encriptación
- La clave se guarda en el keyring con el label `"Chrome Safe Storage"` (o similar)
- Las cookies se encriptan con AES-256-CBC usando esa clave

Para desencriptarlas necesitás:
```csharp
// Pseudocódigo
var key = await SecretServiceClient.GetSecret("Chrome Safe Storage");
var derivedKey = Pbkdf2(key, salt: "saltysalt", iterations: 1, keyLen: 16);
var decryptedValue = AesDecrypt(encryptedCookieValue[3..], key: derivedKey, iv: new byte[16]);
```

La estructura en SQLite es diferente:
```sql
-- Chrome usa la tabla "cookies" (no "moz_cookies")
SELECT name, encrypted_value FROM cookies
WHERE host_key LIKE '%nexusmods.com%'
```

**3. Estructura del archivo `IBrowserCookieReader` a implementar**

```csharp
// src/NexusMods.Networking.NexusWebApi/IBrowserCookieReader.cs
internal interface IBrowserCookieReader
{
    string BrowserName { get; }
    bool IsAvailable();
    string? TryGetNexusModsCookieHeader(ILogger logger);
}
```

Luego en `NexusApiClient.GenerateDirectDownloadUrlAsync`, iterar los readers disponibles en orden de prioridad:
```csharp
var readers = new IBrowserCookieReader[] { new FirefoxCookieReader(), new ChromeCookieReader() };
var cookies = readers.Select(r => r.TryGetNexusModsCookieHeader(_logger)).FirstOrDefault(c => c is not null);
```

### Limitaciones conocidas

- **Solo Firefox en este momento.** Chrome/Chromium requieren desencriptado adicional.
- **Velocidad:** Usuarios free están limitados a ~300-500 KB/s por descarga (límite del servidor, no del cliente). Con 5 descargas paralelas se puede alcanzar ~2.5 MB/s totales.
- **Sesión requerida:** El usuario debe haber iniciado sesión en Nexus Mods dentro del browser. La app no gestiona el login del website.
- **Semáforo en curl:** Las llamadas al endpoint `GenerateDownloadUrl` están serializadas (1 a la vez) para evitar que Cloudflare devuelva páginas de challenge HTML en lugar de JSON. Una vez obtenida la URL CDN, la descarga HTTP es paralela.

---

## 🛠️ Mejoras de Estabilidad y Deep Clean

### Download robustness
- **Detección de archivos vacíos:** Si una descarga produce un archivo de 0 bytes, se tira error antes de crear el library item. El mod queda como "no descargado" en lugar de crear una entrada rota.
- **Extensiones por magic bytes:** Cuando el servidor no envía extensión en el nombre del archivo, la app detecta el tipo real leyendo los primeros bytes (magic bytes) y agrega `.7z`, `.zip` o `.rar` según corresponda.

### Colecciones resilientes
- **Archive faltante:** Si el paquete de la colección (el ZIP con `collection.json`) fue borrado del disco, la instalación lo re-descarga automáticamente de Nexus Mods sin mostrar error al usuario.
- **Página sin crash:** Si la app se abre con el paquete de colección faltante en la DB, la página de colección carga igualmente (en lugar de crashear). El botón de instalación dispara la re-descarga automáticamente.
- **Apply parcial:** Si instalaste algunos mods de una colección (pero no todos), el botón Apply aparece correctamente para aplicar los que ya están descargados.

### Deep Clean mejorado
El Deep Clean (página Storage Manager) ahora:
1. Mueve los archivos mod del directorio del juego a un backup con timestamp
2. **Borra los backups viejos** de deep cleans anteriores (evita acumulación)
3. **Elimina todos los grupos de mods y colecciones** de la DB (no solo los desactiva) — los archives de la librería se conservan para reinstalar sin re-descargar
4. Re-escanea el directorio del juego

---

_Nota: Este proyecto no está afiliado oficialmente con Nexus Mods._
