# Cyberpunk 2077 – Core Mods (cómo se instalan y cómo funcionan al instalarse)

Fuente: [https://wiki.redmodding.org/cyberpunk-2077-modding/for-mod-creators-theory/core-mods-explained](https://wiki.redmodding.org/cyberpunk-2077-modding/for-mod-creators-theory/core-mods-explained)

## Qué son los core mods

Los _core mods_ (o frameworks) son librerías base sobre las que dependen muchos otros mods.
Interactúan con el juego a bajo nivel, por lo que:

- Se rompen con actualizaciones del juego.
- Conviene desactivar el auto-update y actualizar cuando los frameworks estén listos.

---

## Qué core mods se suelen instalar

Los más comunes:

- RED4ext
- Redscript
- Cyber Engine Tweaks (CET)
- ArchiveXL
- TweakXL
- Codeware

Muchos mods requieren varios de estos al mismo tiempo.

---

## Orden lógico de instalación (importante)

De las dependencias que menciona la documentación:

1. **RED4ext** (base para muchos otros)
2. **Cyber Engine Tweaks** (requiere RED4ext)
3. **ArchiveXL / TweakXL / Codeware** (requieren RED4ext)
4. **Redscript** (independiente pero común)

Muchos mods grandes requieren todos.

---

## Cómo se instalan en la práctica

En general, todos estos frameworks se instalan igual:

1. Descargar el mod (Nexus o GitHub).
2. Extraer el contenido.
3. Copiar las carpetas al directorio del juego:

```
Cyberpunk 2077/
```

No se usan gestores especiales: se copian archivos respetando la estructura.

---

## Dónde se instalan internamente

Esto ayuda a verificar si quedó bien instalado:

### RED4ext

Carga plugins desde:

```
Cyberpunk 2077/red4ext/plugins
```

Ahí aparecen DLLs de otros frameworks o mods.

---

### Cyber Engine Tweaks

Scripts y mods suelen ir en:

```
Cyberpunk 2077/bin/x64/plugins/cyber_engine_tweaks/mods
```

---

### Redscript

Compila scripts desde:

```
Cyberpunk 2077/r6/scripts
```

Y genera archivos compilados en:

```
Cyberpunk 2077/r6/cache
```

---

### ArchiveXL

Carga archivos `.xl` y recursos al iniciar el juego.

Normalmente vienen incluidos dentro de mods y no requieren configuración adicional después de copiarlos.

---

### TweakXL

Lee archivos:

```
.yaml
.tweak
```

Y aplica cambios al iniciar el juego.

---

### Codeware

Se instala como librería base y no requiere configuración después de copiar archivos.

---

## Cómo verificar que están instalados correctamente

Los core mods generan logs.
Esto es la forma más confiable de verificar instalación:

### Logs importantes

Cyber Engine Tweaks:

```
Cyberpunk 2077/bin/x64/plugins/cyber_engine_tweaks/cyber_engine_tweaks.log
```

Redscript:

```
Cyberpunk 2077/r6/logs
```

RED4ext:

```
Cyberpunk 2077/red4ext/logs
```

ArchiveXL:

```
Cyberpunk 2077/red4ext/plugins/ArchiveXL
```

TweakXL:

```
Cyberpunk 2077/red4ext/plugins/TweakXL
```

Si el framework aparece cargado sin errores críticos, está funcionando.

---

## Errores comunes al instalar

Problemas típicos:

- Copiar el mod dentro de otra carpeta extra (doble carpeta).
- Falta de dependencias (ej: mod requiere RED4ext o CET).
- Versiones incompatibles tras update del juego.
- Tener mods duplicados.

---

## TL;DR

1. Instalar RED4ext primero.
2. Luego CET, ArchiveXL, TweakXL, Codeware y Redscript.
3. Copiar siempre en la carpeta raíz del juego.
4. Verificar en los logs si cargaron.
