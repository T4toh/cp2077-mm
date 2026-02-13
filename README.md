# NexusMods.App - Cyberpunk 2077 (Linux/Steam Fork)

Este es un fork especializado de NexusMods.App, enfocado exclusivamente en la gestión de mods para **Cyberpunk 2077** en **Linux** a través de **Steam (Proton)**.

## 🎯 Objetivo del Proyecto

Proporcionar un gestor de mods moderno, nativo para Linux, que resuelva las complejidades de usar herramientas de Windows bajo Wine/Proton. Este fork prioriza la compatibilidad con el sistema de archivos de Linux y la integración con Steam Deck y escritorios Linux.

## ✨ Diferencias y Mejoras (vs Upstream)

- **Foco Único:** Eliminado el soporte para GOG, EGS, Windows, macOS y otros juegos para reducir la complejidad y el tamaño del binario.
- **Detección Manual de Juego:** Permite especificar manualmente la ruta de instalación de Cyberpunk 2077 y el prefijo de WINE (Proton), facilitando el soporte para instalaciones en discos secundarios o Steam Deck.
- **Gestión de Colecciones Global:** Ahora puedes navegar y descargar colecciones incluso si no tienes un juego gestionado o instalado.
- **Escaneo de Descargas (Rescan):** Implementado un sistema de detección por MD5 que identifica archivos ya descargados en tu carpeta de `Downloads`, evitando descargas redundantes.
- **Transparencia en Colecciones:** Nueva pestaña "Mod List" que permite ver el detalle de cada mod en una colección, sus hashes, enlaces originales y copiarlos al portapapeles.
- **Corrección de Archivos de Respaldo:** Los backups ahora mantienen sus extensiones originales (`.zip`, `.rar`, etc.) para asegurar que sean reconocibles por otras herramientas de compresión en Linux.
- **Rutas Linux Nativas:** Uso correcto de `XDG_DATA_HOME` para almacenar configuraciones y archivos descargados.

## 🛠 Arquitectura

- **Lenguaje:** C# / .NET 10.
- **UI:** Avalonia UI (MVVM con R3/ReactiveUI).
- **Base de Datos:** MnemonicDB (Inmutable, EAV).
- **Sincronización:** Sistema de enlaces simbólicos/hardlinks para no duplicar espacio en disco.

## 🚀 Próximos Pasos (Checklist)

- [ ] **Descarga Automatizada de Colecciones:** Integrar el flujo de descarga completa para que procese todos los mods de una colección de forma secuencial.
- [ ] **Captura de Enlaces NXM:** Implementar el manejador de protocolos para capturar clics de "Vortex/Mod Manager Download" directamente desde el navegador.
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

---

_Nota: Este proyecto no está afiliado oficialmente con Nexus Mods._
