# Tainted Grail: The Fall of Avalon — Community Modding

Want to make something for **Tainted Grail: The Fall of Avalon**?

Start with what you want to do. You do **not** need to understand this whole repository first.

## What do you want to do?

### 🌱 I've never made a mod before

Start here:

**[Make your first Tainted Grail mod →](00-never-made-a-mod-start-here/README.md)**

This path starts from the beginning: setting up your workspace, identifying your game runtime, making your first test, and knowing whether it actually worked.

---

### ⚙️ I want to change how the game behaves

Examples include player behaviour, stats, movement, combat, UI, interactions, audio, and other systems.

First identify whether your installed game uses **IL2CPP or Mono** — the two supported Unity runtime forms this repository treats as separate modding setups:

**[Find your game and identify the runtime →](00-never-made-a-mod-start-here/02_FIND_GAME_AND_RUNTIME.md)**

Then build the matching first plug-in:

- **[IL2CPP plug-in →](00-never-made-a-mod-start-here/03_FIRST_IL2CPP_PLUGIN.md)**
- **[Mono plug-in →](00-never-made-a-mod-start-here/04_FIRST_MONO_PLUGIN.md)**

After your first plug-in loads successfully:

- **IL2CPP:** **[Make your first change to the running game →](01-basic/03A_FIRST_IL2CPP_GAME_CHANGE.md)**
- **Both lanes:** **[Learn the everyday modding loop →](01-basic/README.md)**

---

### 🗡️ I want to make items, weapons, armour, or creatures

Use the content-authoring path:

**[Make your first piece of content →](00-never-made-a-mod-start-here/05_FIRST_CONTENT_AUTHORING.md)**

Then explore the individual authoring pipelines:

- [Items](docs/pipelines/ITEMS.md)
- [Weapons](docs/pipelines/WEAPONS.md)
- [Armour](docs/pipelines/ARMOUR.md)
- [Creatures / NPCs](docs/pipelines/CREATURES_KANDRA.md)

---

### 🔧 Something isn't working

You do not need to start over.

**[Follow the debugging guide →](docs/DEBUGGING.md)**

It helps narrow a problem down from the loader, runtime, plug-in, patch, or content pipeline instead of changing several things at once.

---

### 🧠 I already know the basics

Go deeper according to what you are trying to understand:

**[Understand how mods work →](02-foundational/README.md)**  
Understand runtimes, dependencies, identity, compatibility, and why mods work.

**[Build robust game changes →](03-advanced/README.md)**  
Work with more complicated patches, failures, compatibility, packaging, and diagnostics.

**[Build reusable mod systems →](04-framework/README.md)**  
Learn how reusable mod systems and stable contracts are designed.

**[Ship and maintain mods →](05-infrastructure/README.md)**  
Learn about repositories, CI, validation, contribution flows, versioning, and releases.

---

## Your first-mod milestones

You always know what the next win is:

**Start → Loader working → First plug-in → First game change → First complete mod**

Use **[Make Your First Tainted Grail Mod](00-never-made-a-mod-start-here/README.md)** to begin. Each milestone page tells you what counts as success before you move on.

## Not sure where to start?

Use this route:

**Set up → prove the loader → load your own plug-in → change one thing → finish one small mod.**

You are not expected to know all of the terminology before you begin. The guides introduce concepts as they become useful.

> **Unofficial community project.** Tainted Grail: The Fall of Avalon and related names, assets, and marks belong to their respective rights holders. This repository is not affiliated with or endorsed by the game's developers or publishers.

## What is here

- `docs/` — practical setup, runtime selection, architecture, debugging, and reference notes.
- `docs/pipelines/` — public-safe FoA authoring pipelines derived from Merlin Workshop's actual toolkit structure.
- `templates/mono-basic/` — minimal BepInEx 5 / Unity Mono plug-in starter.
- `templates/il2cpp-basic/` — minimal BepInEx 6 / Unity IL2CPP plug-in starter.
- `examples/il2cpp-first-game-change/` — a small reversible BepInEx 6 / IL2CPP runtime-state change that bridges plug-in loading to changing the running game.
- `examples/mono-harmony-self-test/` — a Harmony example that patches only its own test method; it does not modify game behavior.
- `examples/proven-paths/` — clean-room mechanism templates based on paths that have been tested in the maintainer's working environment, without copying finished mod designs.
- `examples/mod-cookbook/` — real FoA-target teaching mods for stats, damage, magic, HUD, interaction, audio, movement, and content authoring, with each example showing how far it has actually been checked.
- `tools/verify-public-surface.ps1` — CI-purpose repository guard.
- `.github/workflows/public-surface.yml` — runs the public-surface guard on pushes and pull requests.

## Learn vs reference

**Learning pages** are the guided paths linked above. Follow those in order when you are building your first mods.

**Reference pages** are for lookup. You do not need to read `docs/` front-to-back.

**[Open the reference library →](docs/REFERENCE_MAP.md)**

Common lookups:

- [Runtime Guide](docs/RUNTIME_GUIDE.md) — Mono vs IL2CPP and local IL2CPP reference layers.
- [FoA Authoring Pipelines](docs/pipelines/README.md) — detailed Merlin Workshop content contracts.
- [Debugging](docs/DEBUGGING.md) — troubleshoot a specific failure.
- [Testing and Evidence Status](docs/EVIDENCE.md) — exact meanings of deeper testing-status labels.

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
