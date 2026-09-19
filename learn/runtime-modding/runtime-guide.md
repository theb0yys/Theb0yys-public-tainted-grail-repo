# Runtime Guide

> **Reference page.** Use this when you need exact runtime, loader, or IL2CPP-reference details. If you are learning in order, return to the [first-mod learning path](../first-mod/README.md).

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

Use `templates/il2cpp/basic`.

## IL2CPP references after the smoke test

The first plug-in needs only enough of BepInEx to load.

As you make more capable IL2CPP mods, references normally grow in three layers:

1. **Loader/runtime host** — assemblies under `<GameRoot>/BepInEx/core/`, such as `BepInEx.Core.dll`, `BepInEx.Unity.Common.dll`, `BepInEx.Unity.IL2CPP.dll`, and `Il2CppInterop.Runtime.dll`.
2. **Unity runtime API** — Unity assemblies supplied by the installed IL2CPP modding environment, commonly under `<GameRoot>/BepInEx/unity-libs/`. These let a plug-in use Unity APIs such as `Application`, `QualitySettings`, scenes, GameObjects, and other engine-level types.
3. **Generated Tainted Grail interop** — managed projections generated for the installed IL2CPP game under `<GameRoot>/BepInEx/interop/`. When you need a Tainted Grail type, reference only the local generated assembly that actually contains the target, for example `TG.Main.dll` when the verified target lives there.

For Harmony patching, the project will also normally reference the local `0Harmony.dll` supplied by the installed BepInEx environment.

Do **not** copy generated interop, Unity, BepInEx, or game assemblies into this repository. Keep them as local compile-time references.

The progression is intentional:

~~~text
plug-in loads
-> use a Unity runtime API
-> inspect the local generated interop
-> verify one FoA type/method
-> add Harmony only for that verified target
~~~

Start with **[Make Your First IL2CPP Game Change](../everyday-modding/first-il2cpp-game-change.md)** before moving to a game-specific Harmony target.

Generated interop belongs to the game build that produced it. After a game update, re-check the generated assemblies and the exact target rather than assuming an older symbol or signature still applies.

## Mono indicators

A Unity Mono install normally exposes managed game assemblies under the game's `*_Data/Managed` directory, including an `Assembly-CSharp.dll`-style game assembly.

The supported Tainted Grail Mono loader lane uses:

- BepInEx `5.4.23.5`
- UnityDoorstop `4.5.0`
- BepInEx v5 plug-in API
- `BaseUnityPlugin`

Use `templates/mono/basic`.

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
