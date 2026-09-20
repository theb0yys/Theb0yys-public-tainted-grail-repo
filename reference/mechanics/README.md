# Mechanics Catalogue

This is a curated public view of reusable routes extracted from the private mechanics inventory. Status is claim-specific.

| Mechanic | Current public status | Important boundary |
| --- | --- | --- |
| Resolve `ItemTemplate` through `TemplatesProvider` after `AllLoaded` | Static/source inspected | Exact identity and runtime/build scope still matter |
| Construct `Item` and add to `HeroItems` | Static/source inspected; used by consumers | Does not prove save safety |
| Register custom item/weapon template | Multi-consumer source evidence; bounded runtime evidence | Persistence/missing-package remain separate |
| Patch lockpick durability consumption | Static/source inspected | Patch-sensitive across builds |
| Merchant restock on shop open | Static/source inspected | Owner evidence exists; broad runtime claim not implied |
| Runtime alchemy recipe append | Static/source inspected | Explicitly not persistent learning |
| Learn existing recipe through `HeroRecipes.LearnRecipe` | Static/source inspected | Save/reload proof remains separate |
| One-session companion spawn/ally lifecycle | Source + representative runtime evidence | Explicitly non-persistent |
| Native mount running/turning velocity postfix | Runtime proven on tested stack | Not custom-mount ownership |
| Observe native save completion | Source/decompiled candidate | Not a custom serialization API |
| Arbitrary native mod save-domain registration | **Blocked by current-binary static verdict** | No supported mutable registrar found |
| Mod sidecar persistence | Under evaluation | Static candidates exist; production runtime/save matrix not complete |

Canonical mechanics live under [mechanics/](../../mechanics/README.md).
