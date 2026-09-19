# Tainted Grail Community Modding Starter

A clean public-facing starting point for community mod development around **Tainted Grail: The Fall of Avalon**.

This repository is deliberately separated from private project code. It contains reusable setup information, small starter templates, safe examples, and contributor guidance. It does **not** contain game assets, extracted commercial content, private automation/process material, secrets, or copies of in-development projects.

> Unofficial community project. Tainted Grail: The Fall of Avalon and related names, assets, and marks belong to their respective rights holders. This repository is not affiliated with or endorsed by the game's developers or publishers.

## Learning path

New to modding? Work through these in order:

1. [00 — Never made a mod? Start here](00-never-made-a-mod-start-here/README.md)
2. [01 — Basic](01-basic/README.md)
3. [02 — Foundational](02-foundational/README.md)
4. [03 — Advanced](03-advanced/README.md)
5. [04 — Framework](04-framework/README.md)
6. [05 — Infrastructure](05-infrastructure/README.md)

You can branch into either runtime plug-in development or content authoring from level 00. The later levels explain the common engineering underneath both paths.

## What is here

- `docs/` — practical setup, runtime selection, architecture, debugging, and reference notes.
- `docs/pipelines/` — public-safe FoA authoring pipelines derived from Merlin Workshop's actual toolkit structure.
- `templates/mono-basic/` — minimal BepInEx 5 / Unity Mono plug-in starter.
- `templates/il2cpp-basic/` — minimal BepInEx 6 / Unity IL2CPP plug-in starter.
- `examples/mono-harmony-self-test/` — a Harmony example that patches only its own test method; it does not modify game behavior.
- `examples/proven-paths/` — clean-room mechanism templates derived from owner-side runtime-evidenced paths without copying finished mod designs.
- `tools/verify-public-surface.ps1` — CI-purpose repository guard.
- `.github/workflows/public-surface.yml` — runs the public-surface guard on pushes and pull requests.

## Start here

1. Read [docs/START_HERE.md](docs/START_HERE.md).
2. For Merlin Workshop content authoring, start with [docs/pipelines/README.md](docs/pipelines/README.md).
3. Identify whether your installed game is **Mono** or **IL2CPP** using [docs/RUNTIME_GUIDE.md](docs/RUNTIME_GUIDE.md).
4. Copy the matching BepInEx template into your own project directory when writing a runtime plug-in.
5. Point the project at your **local** BepInEx/game installation. Do not commit those binaries.
6. Build the plug-in and place only your built plug-in DLL in your local `BepInEx/plugins` folder for testing.
7. Use [docs/DEBUGGING.md](docs/DEBUGGING.md) when the plug-in does not load.

## Merlin Workshop pipeline reference

The public authoring documents are grounded in the Tainted Grail modding toolkit:

`theb0yys/merlin-workshop`

Pinned source snapshot used for the current documents:

`073bdab3e09d6adad5003339fc49b021738d71e6` — **Update with new game content** (2026-02-06).

The documents distinguish what is directly established by toolkit source from what still requires local Unity/game runtime validation.

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

- Merlin Workshop / Tainted Grail modding toolkit: https://github.com/theb0yys/merlin-workshop
- BepInEx Tainted Grail loader work: https://github.com/theb0yys/BepInEx-Tainted-Grail
- BepInEx upstream: https://github.com/BepInEx/BepInEx
- HarmonyX upstream: https://github.com/BepInEx/HarmonyX

## Contributing

Keep contributions small, reviewable, and redistributable. See [CONTRIBUTING.md](CONTRIBUTING.md).

## Licence status

No licence is included in this repository yet. Public visibility does not by itself grant a general redistribution or relicensing right. A licence should be added only through an explicit maintainer decision.
