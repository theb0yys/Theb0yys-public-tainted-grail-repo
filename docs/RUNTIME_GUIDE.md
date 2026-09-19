# Runtime Guide

Mono and IL2CPP are separate runtime lanes. Loader files, plug-in base classes, target frameworks, and generated assemblies differ.

## Current validated reference snapshot

A local validation captured on **2026-08-30** recorded:

- Steam App ID: `1466060`
- Steam public build: `24246014`
- game version: `1.25.029`
- runtime: **Unity IL2CPP**
- `GameAssembly.dll`: present
- `global-metadata.dat`: present
- Mono `Assembly-CSharp.dll` marker: absent
- BepInEx 6 IL2CPP build: bleeding-edge `785`, commit `6abdba4`
- validation scope: that captured machine/build only

That receipt established startup/plugin-load evidence for the captured build. It does not automatically apply to later Steam or BepInEx builds.

## IL2CPP indicators

Common indicators include:

```text
<GameRoot>/GameAssembly.dll
<GameRoot>/Fall of Avalon_Data/il2cpp_data/Metadata/global-metadata.dat
```

With BepInEx 6 IL2CPP, the loader target is the IL2CPP BepInEx assembly and generated interop assemblies are produced/managed by the IL2CPP toolchain.

Use `templates/il2cpp-basic`.

## Mono indicators

A Unity Mono install normally exposes managed game assemblies under the game's `*_Data/Managed` directory, including an `Assembly-CSharp.dll`-style game assembly.

The older Tainted Grail Mono loader lane used:

- BepInEx `5.4.23.5`
- UnityDoorstop `4.5.0`
- BepInEx v5 plug-in API
- `BaseUnityPlugin`

Use `templates/mono-basic`.

## Never mix lanes

Do not activate Mono and IL2CPP loader payloads together.

Symptoms of a mixed or wrong lane can include:

- no BepInEx log at all;
- native loader entry without managed chainloader startup;
- missing managed assemblies;
- plug-ins compiled against the wrong BepInEx API;
- generated interop/type failures.

When in doubt, stop and identify the runtime before replacing loader files.

## Source references

- https://github.com/theb0yys/BepInEx-Tainted-Grail
- https://github.com/BepInEx/BepInEx
- https://docs.bepinex.dev/
