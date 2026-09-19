# Tainted Grail: The Fall of Avalon — Community Modding

Want to make something for **Tainted Grail: The Fall of Avalon**?

Start with what you want to do. You do **not** need to understand the whole repository first.

## What do you want to do?

### 🌱 I've never made a mod before

**[Make your first Tainted Grail mod →](00-never-made-a-mod-start-here/README.md)**

This path covers workspace setup, runtime identification, your first plug-in/content test, and how to know whether it actually worked.

### ⚙️ I want to change how the game behaves

First identify whether your installed game uses **IL2CPP or Mono**:

**[Find your game and identify the runtime →](00-never-made-a-mod-start-here/02_FIND_GAME_AND_RUNTIME.md)**

Then build the matching first plug-in:

- **[IL2CPP plug-in →](00-never-made-a-mod-start-here/03_FIRST_IL2CPP_PLUGIN.md)**
- **[Mono plug-in →](00-never-made-a-mod-start-here/04_FIRST_MONO_PLUGIN.md)**

After it loads:
- **IL2CPP:** [Make your first runtime game change](01-basic/03A_FIRST_IL2CPP_GAME_CHANGE.md)
- **Both lanes:** [Learn the everyday modding loop](01-basic/README.md)

### 🗡️ I want to add genuinely new content

Start with the first proven new-content route:

**[Add your first new item →](00-never-made-a-mod-start-here/05_FIRST_CONTENT_AUTHORING.md)**

Then follow the canonical system/mechanic material for the domain you are changing. Items, weapons, armour, creatures, spells, recipes and world content are **separate integration problems**.

Useful starting points:
- [Systems](systems/README.md)
- [Mechanics](mechanics/README.md)
- [Reference](reference/README.md)
- [Items](systems/items/README.md) · [journey](learn/content-authoring/items/README.md)
- [Weapons](systems/weapons/README.md) · [journey](learn/content-authoring/weapons/README.md)
- [Armour](systems/armour/README.md) · [journey](learn/content-authoring/armour/README.md)
- [Creatures](systems/creatures/README.md) · [journey](learn/content-authoring/creatures/README.md)

### 🔧 Something isn't working

**[Diagnose the earliest failed stage →](diagnose/README.md)**

Do not change several unrelated systems at once.

### 🧠 I want to understand how FoA actually works

**[Open Systems →](systems/README.md)**

For guided progression:
- [Understand how mods work](02-foundational/README.md)
- [Build robust game changes](03-advanced/README.md)

These are learning journeys, not the canonical storage location for technical truth.

### 🧰 I want to know what a mod can safely do

**[Open Mechanics →](mechanics/README.md)**

### 🔬 I need to investigate something undocumented

**[Open Investigate →](investigate/README.md)**

### 📚 I need an exact ID, hook, method, version or proof status

**[Open Reference →](reference/README.md)**

### 🧱 I build frameworks, tooling, CI or releases

**[Open Tooling and Maintenance →](tooling/README.md)**

## Your first-mod milestones

**Start → Loader working → First plug-in → First game change → First complete mod**

Use **[Make Your First Tainted Grail Mod](00-never-made-a-mod-start-here/README.md)** to begin.

## The documentation model

The guide is organized by **reader need and canonical knowledge ownership**:

- `learn/` — guided journeys;
- `systems/` — how native FoA works;
- `mechanics/` — bounded modding capabilities;
- `investigate/` — how to discover unknown behaviour;
- `diagnose/` — symptom-driven failure isolation;
- `reference/` — exact lookup and evidence status;
- `examples/` — worked examples and case histories;
- `tooling/` — loaders, frameworks, SDK/tooling and maintenance;
- `contributing/authoring/` — mandatory documentation architecture and authoring process.

The core rule is:

> **One canonical explanation for each piece of truth; multiple routes into it.**

See [Mandatory Documentation Architecture](contributing/authoring/DOCUMENTATION_ARCHITECTURE.md).

## Existing learning routes

These remain valid during migration:

- [First mod](00-never-made-a-mod-start-here/README.md)
- [Everyday modding loop](01-basic/README.md)
- [Understand how mods work](02-foundational/README.md)
- [Build robust game changes](03-advanced/README.md)
- [Build reusable mod systems](04-framework/README.md)
- [Ship and maintain mods](05-infrastructure/README.md)

The last two are specialist routes, not mandatory higher levels of ordinary modding.

## Common lookups

- [Evidence and testing status](reference/evidence/README.md)
- [Validation and compatibility](reference/evidence/validation-and-compatibility.md)
- [Mechanics catalogue](reference/mechanics/README.md)
- [Hook catalogue](reference/hooks/README.md)
- [Identity catalogue](reference/identities/README.md)
- [Runtime Guide](docs/RUNTIME_GUIDE.md)
- [Intervention selection](mechanics/intervention-selection.md)
- [Research method](investigate/research-method.md)
- [Reverse engineering and discovery](investigate/reverse-engineering-discovery.md)
- [Golden Rules](learn/understand-foa/golden-rules.md)
- [Assemblies and system owners](reference/assemblies-system-owners.md)
- [Content-domain owner map](reference/content-domains.md)

Legacy `docs/reference/` paths are compatibility shims only after Wave 5; substantive technical ownership now lives in the canonical Systems, Mechanics, Reference, Learn, Investigate, Diagnose, and Examples surfaces.

## Public-repository boundary

Do not commit:

- game textures, models, audio, video, maps, scenes, asset bundles, localization dumps, or other extracted content;
- game DLLs, Unity runtime DLLs, BepInEx binaries, executables, archives, or generated interop assemblies;
- bulk decompiled game source;
- saves, user data, crash dumps containing personal paths, or private diagnostics;
- API keys, tokens, signing material, credentials, or machine-specific secrets;
- private implementation source that has not been explicitly approved for publication.

Use local references when templates need game/BepInEx assemblies.

## Examples and templates

- `templates/mono-basic/` — minimal BepInEx 5 / Unity Mono starter.
- `templates/il2cpp-basic/` — minimal BepInEx 6 / Unity IL2CPP starter.
- `examples/il2cpp-first-game-change/` — small reversible IL2CPP runtime-state example.
- `examples/mono-harmony-self-test/` — Harmony mechanics without modifying game behaviour.
- `examples/proven-paths/` — clean-room reusable mechanism demonstrations.
- `examples/mod-cookbook/` — game-target teaching examples grouped by topic.
- `examples/failures-and-corrections/` — case-study surface for preserved failure/correction histories.

Public examples carry their **own** evidence status; they do not inherit runtime proof from private predecessors.

## Reference projects

- Merlin Workshop / Tainted Grail modding toolkit: https://github.com/theb0yys/merlin-workshop
- BepInEx Tainted Grail loader work: https://github.com/theb0yys/BepInEx-Tainted-Grail
- BepInEx upstream: https://github.com/BepInEx/BepInEx
- HarmonyX upstream: https://github.com/BepInEx/HarmonyX

> **Unofficial community project.** Tainted Grail: The Fall of Avalon and related names, assets, and marks belong to their respective rights holders. This repository is not affiliated with or endorsed by the game's developers or publishers.

## Community and trust

- [Contributing](CONTRIBUTING.md)
- [Security](SECURITY.md)

## Licence status

No general licence is included yet. Public visibility does not by itself grant a general redistribution or relicensing right.
