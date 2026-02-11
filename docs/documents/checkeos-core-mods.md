# Cyberpunk 2077 – Estructuras y checks de Core Mods (para implementar validaciones)

Guía práctica orientada a desarrollar validaciones automáticas en un mod manager.

---

# Cyber Engine Tweaks (CET)

## Estructura típica correcta

```
Cyberpunk 2077/
└─ bin/
   └─ x64/
      ├─ version.dll
      ├─ global.ini
      └─ plugins/
         ├─ cyber_engine_tweaks.asi
         └─ cyber_engine_tweaks/
            ├─ fonts/
            ├─ scripts/
            ├─ tweakdb/
```

## Archivos indicadores fiables

Detectar CET si existe:

```
bin/x64/plugins/cyber_engine_tweaks.asi
```

Este archivo es el check más robusto.

## Checks útiles

1. Existe `.asi`
2. Existe carpeta:

```
plugins/cyber_engine_tweaks
```

1. Log presente tras ejecutar:

```
cyber_engine_tweaks.log
```

---

# RED4ext

## Estructura correcta

```
Cyberpunk 2077/
└─ red4ext/
   ├─ RED4ext.dll
   ├─ logs/
   └─ plugins/
```

## Indicador fiable

```
red4ext/RED4ext.dll
```

## Checks útiles

- Carpeta `plugins` existe
- Carpeta `logs` existe
- Al menos un log después de ejecutar el juego

---

# ArchiveXL

ArchiveXL es un plugin de RED4ext, no standalone.

## Estructura

```
Cyberpunk 2077/
└─ red4ext/
   └─ plugins/
      └─ ArchiveXL.dll
```

Puede venir además con:

```
ArchiveXL/
```

## Indicador fiable

```
red4ext/plugins/ArchiveXL.dll
```

## Checks útiles

1. RED4ext instalado
2. DLL presente
3. No duplicados en plugins

---

# TweakXL

## Estructura

```
Cyberpunk 2077/
└─ red4ext/
   └─ plugins/
      └─ TweakXL.dll
```

## Indicador fiable

```
red4ext/plugins/TweakXL.dll
```

## Checks útiles

- RED4ext presente
- DLL presente

---

# Codeware

## Estructura

```
Cyberpunk 2077/
└─ red4ext/
   └─ plugins/
      └─ Codeware.dll
```

## Indicador fiable

```
red4ext/plugins/Codeware.dll
```

## Checks útiles

Igual que TweakXL.

---

# Redscript

Es el framework que más confusión genera.

## Estructura esperada

```
Cyberpunk 2077/
└─ engine/
   └─ tools/
      └─ redscript/
```

y

```
Cyberpunk 2077/
└─ r6/
   └─ scripts/
```

## Indicadores fiables

Uno de estos:

```
engine/tools/redscript
```

o

```
r6/scripts
```

o log:

```
r6/logs/redscript.log
```

## Check realmente fiable

El mejor check práctico:

- existe carpeta `r6/scripts`
- existe carpeta `r6/cache`

Eso indica que compiló correctamente.

---

# Archive Mods (no framework)

También conviene detectarlos.

## Estructura

```
archive/pc/mod/*.archive
```

## Indicador

```
*.archive
```

Estos generalmente no tienen dependencias salvo ArchiveXL en algunos casos.

---

# Detección automática del tipo de mod

Clasificación útil mirando contenido:

| Archivo encontrado | Tipo               |
| ------------------ | ------------------ |
| `.asi`             | CET mod            |
| `.reds`            | Redscript          |
| `.archive`         | Archive mod        |
| `.xl`              | Requiere ArchiveXL |
| `.yaml` tweak      | Requiere TweakXL   |

Este método funciona bien en la práctica.

---

# Checks de instalación incorrecta

## Carpeta redundante

Error común:

```
Cyberpunk 2077/Cyberpunk 2077/bin/x64
```

### Regla útil

Si dentro del mod existe una carpeta que contiene:

- `bin`
- `r6`
- `red4ext`

probablemente hay un nivel extra.

---

## Plugins en lugar incorrecto

Error:

```
Cyberpunk 2077/plugins/
```

Correcto:

```
bin/x64/plugins
```

---

# Dependencias reales (reglas prácticas)

Reglas simples que cubren la mayoría de casos:

```
ArchiveXL.dll -> requiere RED4ext
TweakXL.dll -> requiere RED4ext
Codeware.dll -> requiere RED4ext
CET mods -> requieren CET
.reds -> requiere Redscript
.xl -> requiere ArchiveXL
```

Esto cubre aproximadamente el 90% de los casos reales.

---

# Logs útiles para health checks

CET:

```
bin/x64/plugins/cyber_engine_tweaks/cyber_engine_tweaks.log
```

RED4ext:

```
red4ext/logs
```

Redscript:

```
r6/logs
```

---

# Orden recomendado para validaciones

Para evitar falsos positivos:

1. Detectar frameworks instalados
2. Detectar dependencias faltantes
3. Detectar estructura incorrecta
4. Detectar logs con errores

Este orden suele dar resultados más fiables.
