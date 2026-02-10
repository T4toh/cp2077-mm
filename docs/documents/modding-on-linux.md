# Cyberpunk 2077 Modding en Linux (Steam) — Resumen práctico

Fuente: [https://wiki.redmodding.org/cyberpunk-2077-modding/for-mod-users/users-modding-cyberpunk-2077/modding-on-linux](https://wiki.redmodding.org/cyberpunk-2077-modding/for-mod-users/users-modding-cyberpunk-2077/modding-on-linux)

## Requisitos básicos

Para que los mods funcionen en Cyberpunk 2077 usando Steam en Linux:

1. Instalar dependencias:
    - `d3dcompiler_47`
    - `vcrun2022`

2. Configurar Launch Options en Steam:

```
WINEDLLOVERRIDES="winmm,version=n,b" %command%
```

Este parámetro es **case-sensitive** y conviene copiarlo exactamente.

---

## Instalación de dependencias (Protontricks)

Herramienta recomendada: **Protontricks**

Pasos básicos:

1. Abrir Protontricks.
2. Seleccionar **Cyberpunk 2077**.
3. Elegir:
    - "Select the default wineprefix"

4. Instalar:
    - `d3dcompiler_47`
    - `vcrun2022`

Notas:

- Usar la versión más reciente de Protontricks.
- Si `vcrun2022` no aparece, actualizar Protontricks.

---

## Configurar Launch Options en Steam

1. Abrir Steam.
2. Ir a:
    - Biblioteca
    - Cyberpunk 2077
    - Propiedades
    - Launch Options

3. Pegar:

```
WINEDLLOVERRIDES="winmm,version=n,b" %command%
```

---

## Problemas comunes

### No funcionan los mods

Causa más frecuente:

- Error tipográfico en los Launch Options.
- Espacios extra o letras faltantes.

Solución:

- Copiar y pegar el comando exactamente.

---

## Troubleshooting rápido

- Asegurarse de:
    - Tener Protontricks actualizado.
    - Haber instalado correctamente `vcrun2022`.
    - No haber modificado el comando de lanzamiento.

---

## TL;DR

1. Instalar Protontricks.
2. Instalar:
    - d3dcompiler_47
    - vcrun2022

3. Poner en Steam:

```
WINEDLLOVERRIDES="winmm,version=n,b" %command%
```
