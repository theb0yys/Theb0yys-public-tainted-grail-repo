# FoA Proprietary Systems

This library explains the major Questline / Awaken Realms systems that sit underneath Tainted Grail's gameplay and presentation.

Use these pages to answer:

- **What is this system?**
- **What does it own?**
- **Where does it sit in the game?**
- **Which assemblies/types belong to it?**
- **Which other systems does it interact with?**
- **What does that mean for a mod?**

The cookbook is separate: [Modding Cookbook](../../examples/mod-cookbook/README.md).

## Rendering and streaming

| System | Purpose |
| --- | --- |
| [Drake / MergedDrake](drake/README.md) | rigid mesh rendering through ECS / Entities Graphics |
| [Kandra](kandra/README.md) | skinned/deforming character and clothing rendering |
| [Medusa](medusa/README.md) | high-volume static environment rendering |
| [Leshy](leshy/README.md) | baked/streamed vegetation cells, BRG rendering and colliders |
| [HLOD](hlod/README.md) | hierarchical distant-world proxy streaming |
| [Shared mipmap streaming](mipmap-streaming/README.md) | cross-renderer material/texture mip demand |
| [Critter VAT / ECS](critter-vat/README.md) | high-volume critter animation + Drake integration |

## Build and runtime foundations

| System | Purpose |
| --- | --- |
| [Scenes Baking](scenes-baking/README.md) | build-time scene compilation and renderer/content processing |
| [Runtime orchestration](runtime-orchestration/README.md) | startup, scene services and template loading |
| [MVC / GameObject / ECS lifetime](runtime-lifecycle/README.md) | logical, Unity, scene and renderer-resource ownership |
| [Serialization and .arch packaging](serialization-archives/README.md) | Unity Archive packaging of system-specific payloads |
| [Archive / file / buffer primitives](archive-io/README.md) | shared low-level IO and packed-data utilities |
| [Managed runtime map](managed-runtime/README.md) | which managed assemblies own each system |

## Content systems

| System | Purpose |
| --- | --- |
| [Story Graphs](story-graphs/README.md) | compiled story/dialogue/quest execution |
| [Babel localisation](babel/README.md) | compiled localisation corpus and runtime lookup |
| [Native weapon integration](native-weapons/README.md) | item → equip → hand → combat → presentation ownership |

## Movement

| System | Purpose |
| --- | --- |
| [Native glider movement](glider-movement/README.md) | FoA's built-in glider movement/controller integration |

## Cross-system mental model

~~~text
authored content
    ↓
Scenes Baking / content build
    ↓
runtime services + templates + archives
    ↓
gameplay owners
    ↓
presentation owners
    ├─ Drake
    ├─ Kandra
    ├─ Medusa
    ├─ Leshy
    ├─ HLOD
    └─ Critter VAT/Drake
    ↓
shared services
    ├─ mipmap streaming
    ├─ archive/file IO
    ├─ scene/domain lifetime
    └─ Babel / Story / template services
~~~

The central rule is **one owner per responsibility**. A visible mesh, loaded asset or successful reflection lookup is not automatically the gameplay, persistence or lifecycle owner.
