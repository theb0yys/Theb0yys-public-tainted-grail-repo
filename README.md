# Tainted Grail: The Fall of Avalon — Community Modding

Want to make something for **Tainted Grail: The Fall of Avalon**? Start with what you want to accomplish. You do **not** need to understand the whole repository first.

> **Unofficial community project.** Tainted Grail: The Fall of Avalon and related names, assets, and marks belong to their respective rights holders. This repository is not affiliated with or endorsed by the game's developers or publishers.

## What do you want to do?

### 🌱 Make my first mod

Start with **[Make your first Tainted Grail mod](learn/first-mod/README.md)**.

### ⚙️ Perform a known modding capability

Open **[Mechanics](mechanics/README.md)** for bounded capabilities such as item resolution, custom weapon integration, mount velocity, one-session companions, modal UI and persistence boundaries.

### 🗡️ Add new content

Begin with **[First content authoring](learn/first-mod/first-content-authoring.md)** and use the relevant mechanic/system pages. Existing [how-to guides](how-to/README.md) remain available while they are progressively reconciled into canonical mechanics.

### 🧠 Understand how FoA actually works

Open **[Game systems](systems/README.md)**.

### 🔬 Investigate something the repository does not know yet

Open **[Investigate](investigate/README.md)**. Owner discovery and lifecycle tracing are first-class modding skills.

### 🔧 Diagnose a failure

Open **[Diagnose](diagnose/README.md)**. Troubleshooting starts from the earliest failed owner/stage.

### 🔎 Look up an exact fact

Open **[Reference](reference/README.md)**.

### 🧪 See what real mods proved

Open **[Case studies](case-studies/README.md)**.

### 💻 Start from working code

Open **[Examples](examples/README.md)**.

### 🧰 Use shared modding infrastructure

Open **[Tooling and Shared Infrastructure](tooling/README.md)** when your mod should integrate with FoA Mod Manager, Tainted Interface, Avalon Core, Tainted Framework, Avalon AI Runtime, Avalon Contracts, Tainted Grail Extender/FOA-SDK, or the Tainted Diagnostic Tool.

Start with the [recommended author stacks](tooling/ecosystem/author-stacks.md) or the [component reference](tooling/ecosystem/component-reference.md).

## Repository map

| Area | Use it for |
| --- | --- |
| [learn/](learn/README.md) | Guided learning paths |
| [systems/](systems/README.md) | Native FoA architecture and ownership |
| [mechanics/](mechanics/README.md) | Bounded reusable modding capabilities |
| [investigate/](investigate/README.md) | Discovering unknown owners/lifecycles |
| [diagnose/](diagnose/README.md) | Symptom-driven troubleshooting |
| [reference/](reference/README.md) | Exact lookup |
| [case-studies/](case-studies/README.md) | Lessons from real mod work |
| [examples/](examples/README.md) | Small public-safe runnable examples |
| [tooling/](tooling/README.md) | Shared mod-author infrastructure, APIs and integration recipes |
| [sources/](sources/README.md) | Official and upstream source links |
| [contributing/](contributing/README.md) | Public authoring standards |
| [how-to/](how-to/README.md) | Existing task guides pending reconciliation |
| [templates/](templates/README.md) | Minimal project starters |

## Core reasoning model

```text
exact subject / identity
→ native owner
→ lifecycle and data contract
→ smallest justified intervention
→ downstream native behaviour
→ cleanup / restoration
→ verify the intended behaviour
```

## One canonical explanation, multiple routes into it

Reading order is navigation, not ownership. Link to the canonical system/mechanic/reference page instead of maintaining parallel current copies.

## Public-repository boundary

Do not commit proprietary game binaries, extracted commercial assets, localization dumps, generated interop assemblies, saves, private diagnostics, credentials or bulk decompiled game source.

## Upstream projects

- Questline Merlin's Workshop: https://github.com/AR-Questline/merlin-workshop
- BepInEx: https://github.com/BepInEx/BepInEx
- HarmonyX: https://github.com/BepInEx/HarmonyX

See [CONTRIBUTING.md](CONTRIBUTING.md) and [documentation authoring standards](contributing/README.md).
