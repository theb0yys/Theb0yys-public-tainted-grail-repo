# Tainted Grail: The Fall of Avalon — Community Modding Platform

An unofficial public knowledge base and toolkit for **Tainted Grail: The Fall of Avalon** modding. It brings together step-by-step guides, technical game knowledge, shared modding tools, research, focused code examples, and starter projects.

> **Unofficial community project.** Tainted Grail: The Fall of Avalon and related names, assets, and marks belong to their respective rights holders. This repository is not affiliated with or endorsed by the game's developers or publishers.

## Start here

- **New to modding:** [Guides](guides/README.md) → [Getting started](guides/getting-started/README.md)
- **Need an exact game fact, hook, type, service, or known technique:** [Knowledge](knowledge/README.md)
- **Want to use shared tools or framework APIs:** [Tooling and shared infrastructure](platform/README.md)
- **Trying to figure out behavior that is still unclear:** [Research](research/README.md)
- **Need code you can read and adapt:** [Examples](examples/README.md)
- **Want a starter project:** [Templates](templates/README.md)

## What's in the repository?

| Section | What you'll find |
| --- | --- |
| [platform/](platform/README.md) | Shared tools, framework APIs, dependency guidance, and integration recipes |
| [guides/](guides/README.md) | Getting started, learning paths, task guides, troubleshooting, and shipping |
| [knowledge/](knowledge/README.md) | How the game works, reusable modding techniques, and exact reference information |
| [research/](research/README.md) | Investigation methods, unresolved questions, case studies, and source provenance |
| [examples/](examples/README.md) | Focused public-safe code examples for specific modding techniques |
| [templates/](templates/README.md) | Reusable starter projects |
| [contributing/](contributing/README.md) | Writing, evidence, and contribution rules |

## A useful way to reason about game changes

When changing FoA behavior, work through this chain:

```text
exact subject / identity
→ native owner
→ lifecycle and data contract
→ smallest justified intervention
→ downstream native behaviour
→ cleanup / restoration
→ verify the intended behaviour
```

The important idea is simple: find the part of the game that actually owns the behavior before deciding where to patch it.

## Where information lives

Keep one maintained explanation for each technical claim and link to it from other pages instead of maintaining competing copies.

See [Repository taxonomy](contributing/taxonomy.md) if you need the detailed contribution structure.

## Public-repository boundary

Do not commit proprietary game binaries, extracted commercial assets, localization dumps, generated interop assemblies, saves, private diagnostics, credentials, or bulk decompiled game source.

## Upstream projects

- Questline Merlin's Workshop: https://github.com/AR-Questline/merlin-workshop
- BepInEx: https://github.com/BepInEx/BepInEx
- HarmonyX: https://github.com/BepInEx/HarmonyX

See [CONTRIBUTING.md](CONTRIBUTING.md), [documentation guidance](contributing/README.md), and [evidence standards](contributing/evidence-standards.md).
