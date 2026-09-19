# Tainted Grail: The Fall of Avalon — Community Modding

Want to make something for **Tainted Grail: The Fall of Avalon**? Start with the thing you want to accomplish. You do **not** need to understand the whole repository first.

> **Unofficial community project.** Tainted Grail: The Fall of Avalon and related names, assets, and marks belong to their respective rights holders. This repository is not affiliated with or endorsed by the game's developers or publishers.

## What do you want to do?

### 🌱 Make my first mod
Start with **[Make your first Tainted Grail mod](learn/first-mod/README.md)**.

### ⚙️ Change how the game behaves
Use the **[runtime modding guide](learn/runtime-modding/README.md)** and **[everyday modding](learn/everyday-modding/README.md)**. For a procedure use **[How-to guides](how-to/README.md)**; for the native owner use **[Game systems](systems/README.md)**.

### 🗡️ Add new content
Begin with **[First content authoring](learn/first-mod/first-content-authoring.md)**, then use [Items](how-to/items/README.md), [Weapons](how-to/weapons/README.md), [Armour](how-to/armour/README.md), or [Creatures](how-to/creatures/README.md).

### 🧠 Understand how Tainted Grail actually works
Open **[Game systems](systems/README.md)** for native ownership, lifecycle, services, gameplay architecture, world systems, UI/audio, and Questline/Awaken presentation technology.

### 🔎 Look up an exact fact
Open **[Reference](reference/README.md)** for assemblies, identities, hooks, assets, content domains and compatibility.

### 🧪 See what real mods proved
Open **[Case studies](case-studies/README.md)** for reusable lessons extracted from working mods.

### 💻 Start from working code
Open **[Examples](examples/README.md)** for small source-only Mono and IL2CPP projects.

### 🔧 Something is broken
Use **[Debugging](learn/debugging/README.md)**.

### 🔬 Investigate an unknown system
Use **[Reverse engineering and discovery](how-to/reverse-engineering/README.md)**.

## Choose the correct modding lane

- **Mono runtime mods** — BepInEx/Harmony and managed game assemblies.
- **IL2CPP runtime mods** — BepInEx 6/IL2CPP interop and the installed IL2CPP runtime.
- **Merlin's Workshop content** — Questline's official Unity/Addressables modding surface.
- **Hybrid work** — projects deliberately combining content authoring with runtime integration.

See [Runtime guide](learn/runtime-modding/runtime-guide.md) and [Official sources](sources/official/README.md).

## Repository map

| Area | Use it for |
| --- | --- |
| [learn/](learn/README.md) | Ordered learning paths |
| [how-to/](how-to/README.md) | Task-oriented procedures |
| [systems/](systems/README.md) | Native game architecture and ownership |
| [reference/](reference/README.md) | Exact lookup material |
| [case-studies/](case-studies/README.md) | Lessons from real working mods |
| [examples/](examples/README.md) | Small runnable/source examples |
| [sources/](sources/README.md) | Provenance and evidence standard |
| [templates/](templates/README.md) | Minimal project starters |
| [tooling/](tooling/README.md) | Repository/validation tooling |

## One canonical home

Reading order is navigation, not ownership.

- **How do I learn this?** → `learn/`
- **How do I do this?** → `how-to/`
- **How does the game own this?** → `systems/`
- **What exact value/type/hook do I need?** → `reference/`
- **What did a working mod teach us?** → `case-studies/`
- **Where is the runnable code?** → `examples/`

Other pages should link to the canonical owner instead of keeping parallel current copies.

## Evidence language

See **[Evidence standard](sources/evidence-standard.md)**. Keep Official, static/source, build, loader, runtime, persistence, compatibility and release proof distinct. One lane does not silently substitute for another.

## Public-repository boundary

Do not commit proprietary game binaries, extracted commercial assets, localization dumps, generated interop assemblies, saves, private diagnostics, credentials or bulk decompiled game source.

The [public-surface guard](tooling/README.md) rejects common binary/archive/asset formats, oversized files, obvious secrets and private machine paths.

## Foundations and upstream projects

- Questline Merlin's Workshop: https://github.com/AR-Questline/merlin-workshop
- BepInEx: https://github.com/BepInEx/BepInEx
- HarmonyX: https://github.com/BepInEx/HarmonyX

See [CONTRIBUTING.md](CONTRIBUTING.md) before contributing.
