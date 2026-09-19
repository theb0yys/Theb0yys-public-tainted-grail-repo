# Tainted Grail Community Modding Starter

A clean public-facing starting point for community mod development around **Tainted Grail: The Fall of Avalon**.

This repository is deliberately separated from private project code. It contains reusable setup information, small starter templates, safe examples, and contributor guidance. It does **not** contain game assets, extracted commercial content, private automation/process material, secrets, or copies of in-development projects.

> Unofficial community project. Tainted Grail: The Fall of Avalon and related names, assets, and marks belong to their respective rights holders. This repository is not affiliated with or endorsed by the game's developers or publishers.

## What is here

- `docs/` — practical setup, runtime selection, architecture, debugging, and reference notes.
- `templates/mono-basic/` — minimal BepInEx 5 / Unity Mono plug-in starter.
- `templates/il2cpp-basic/` — minimal BepInEx 6 / Unity IL2CPP plug-in starter.
- `examples/mono-harmony-self-test/` — a Harmony example that patches only its own test method; it does not modify game behavior.
- `tools/verify-public-surface.ps1` — CI-purpose repository guard.
- `.github/workflows/public-surface.yml` — runs the public-surface guard on pushes and pull requests.

## Start here

1. Read [docs/START_HERE.md](docs/START_HERE.md).
2. Identify whether your installed game is **Mono** or **IL2CPP** using [docs/RUNTIME_GUIDE.md](docs/RUNTIME_GUIDE.md).
3. Copy the matching template into your own project directory.
4. Point the project at your **local** BepInEx/game installation. Do not commit those binaries.
5. Build the plug-in and place only your built plug-in DLL in your local `BepInEx/plugins` folder for testing.
6. Use [docs/DEBUGGING.md](docs/DEBUGGING.md) when the plug-in does not load.

## Known runtime reference

A previously captured local validation on **2026-08-30** identified Steam public build `24246014` / game version `1.25.029` as **Unity IL2CPP**, with BepInEx 6 bleeding-edge build `785` (commit `6abdba4`) successfully reaching chainloader startup and loading tested plug-ins in that captured environment.

That is a historical compatibility receipt, **not a guarantee for later game or BepInEx builds**. Re-check your installed runtime before choosing a template.

The older Mono lane used BepInEx `5.4.23.5` with UnityDoorstop `4.5.0`. Mono and IL2CPP are separate lanes; do not mix their loader files or plug-in APIs.

See [docs/RUNTIME_GUIDE.md](docs/RUNTIME_GUIDE.md) for the exact distinction and source references.

## Public-repository boundary

Do not commit:

- game textures, models, audio, video, maps, scenes, asset bundles, localization dumps, or other extracted content;
- game DLLs, Unity runtime DLLs, BepInEx binaries, executables, archives, or generated interop assemblies;
- decompiled bulk game source;
- saves, user data, crash dumps containing personal paths, or private diagnostics;
- API keys, tokens, signing material, credentials, or machine-specific secrets;
- code copied from private projects merely to make it public.

Use local references when a template needs game/BepInEx assemblies. The repository guard intentionally rejects common asset, binary, and archive formats.

## Reference projects

These existing repositories contain broader work and upstream/runtime context. They are references, not dependencies of the starter templates:

- FOA-SDK: https://github.com/theb0yys/FOA-SDK
- BepInEx Tainted Grail loader work: https://github.com/theb0yys/BepInEx-Tainted-Grail
- BepInEx upstream: https://github.com/BepInEx/BepInEx
- HarmonyX upstream: https://github.com/BepInEx/HarmonyX

## Contributing

Keep contributions small, reviewable, and redistributable. See [CONTRIBUTING.md](CONTRIBUTING.md).

## Licence status

No licence is included in this repository yet. Public visibility does not by itself grant a general redistribution or relicensing right. A licence should be added only through an explicit maintainer decision.
