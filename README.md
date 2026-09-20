# Tainted Grail: The Fall of Avalon — Community Modding Platform

An unofficial, source-only modding platform for **Tainted Grail: The Fall of Avalon**. It combines author guidance, game-system knowledge, shared modding infrastructure, research, runnable examples, and reusable project starters.

> **Unofficial community project.** Tainted Grail: The Fall of Avalon and related names, assets, and marks belong to their respective rights holders. This repository is not affiliated with or endorsed by the game's developers or publishers.

## Start here

- **New to modding:** [Guides](guides/README.md) → [Getting started](guides/getting-started/README.md)
- **Need a known capability or exact game fact:** [Knowledge](knowledge/README.md)
- **Building against shared infrastructure:** [Platform](platform/README.md)
- **Investigating unknown behaviour or evidence:** [Research](research/README.md)
- **Need working source:** [Examples](examples/README.md)
- **Starting a project:** [Templates](templates/README.md)
- **Want setup/build/install helpers:** [Developer tools](platform/developer-tools/README.md)

## Repository structure

| Surface | Owns |
| --- | --- |
| [platform/](platform/README.md) | Shared infrastructure, component contracts, ecosystem rules and integration recipes |
| [guides/](guides/README.md) | Getting started, learning paths, task guides, troubleshooting and shipping |
| [knowledge/](knowledge/README.md) | Canonical systems, reusable mechanics and exact reference material |
| [research/](research/README.md) | Investigation methods, open investigations, case studies and source provenance |
| [examples/](examples/README.md) | Small public-safe runnable mechanism demonstrations |
| [templates/](templates/README.md) | Reusable project and mod-family starters |
| [contributing/](contributing/README.md) | Repository taxonomy, evidence standards and authoring rules |

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

## Canonical ownership

Put information where it belongs, then link to it. A subject can legitimately appear in several surfaces because each surface answers a different question.

See [Repository taxonomy](contributing/taxonomy.md).

## Public-repository boundary

Do not commit proprietary game binaries, extracted commercial assets, localization dumps, generated interop assemblies, saves, private diagnostics, credentials or bulk decompiled game source.

## Licensing

This repository uses a split licensing model:

- original software code and software artifacts: **Apache License 2.0** — see [LICENSE](LICENSE);
- original documentation and research material: **CC BY 4.0** — see [LICENSE-DOCS](LICENSE-DOCS).

Third-party, upstream, mirrored, proprietary, and trademarked material is not relicensed merely by appearing in or being referenced by this repository. A more specific file, directory, or upstream notice controls where present.

## Upstream projects

- Questline Merlin's Workshop: https://github.com/AR-Questline/merlin-workshop
- BepInEx: https://github.com/BepInEx/BepInEx
- HarmonyX: https://github.com/BepInEx/HarmonyX

See [CONTRIBUTING.md](CONTRIBUTING.md), [documentation authoring standards](contributing/README.md), and [evidence standards](contributing/evidence-standards.md).
