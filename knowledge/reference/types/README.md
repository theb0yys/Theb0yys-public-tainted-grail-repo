# Types and Members Reference

Exact lookup for important FoA types and members. This is intentionally selective: include a member when knowing it saves meaningful FoA-specific source hunting or reverse engineering.

## Publicly useful types and members

| Type / member | Publicly demonstrated relevance | Evidence boundary |
| --- | --- | --- |
| `HeroItems.OnRestore` | Reliable IL2CPP availability point for the hero item owner; receives the valid instance as `this` | Public native IL2CPP mod |
| `HeroRecipes.LearnRecipe` | Issues an existing/native recipe through the hero recipe owner | Public native IL2CPP mod |
| `KnownItems` | Existing known-item state used for session-start recipe reconstruction/backfill | Public Mono + IL2CPP mod behaviour |
| `CraftingTemplate.recipes` | Recipe template references used to inspect station recipe content | Public IL2CPP implementation; entries were observed to contain stale/freed pointers in that native traversal |
| `TemplateReference[]` | Container type publicly observed behind `CraftingTemplate.recipes` | Same IL2CPP evidence boundary |
| `LootInteractAction.AddItemsToAttacker(heroItems)` | Public IL2CPP mod calls the same private extraction path used by a vein death callback to grant resource loot | Narrow mining/resource-node evidence; not a generic item-grant recommendation |
| `HealthElement.TakeDamage(Damage)` | Existing public working hook family for observing completed character damage | Already catalogued under hooks |
| `HealthElement.OnDeathEvents` | Existing public working hook family for terminal character death sidecars | Already catalogued under hooks |
| `ViewHosting.OnMainCanvas()` | Main-canvas host lookup in Questline's public Merlin source | Resolved through `Services.Get<ViewHosting>()` |
| `HeroStorage.Items` | Native hero-storage collection used by a public storage UI mod | Public documentation scopes the read to the Hero Storage interface being open |

## Public namespaces worth knowing

Questline's public Merlin Workshop source exposes game-facing namespaces including:

- `Awaken.TG.Main.Heroes`
- `Awaken.TG.Main.Character`
- `Awaken.TG.Main.Locations`
- `Awaken.TG.Main.Fights`
- `Awaken.TG.Main.Saving`
- `Awaken.TG.Main.AI`
- `Awaken.TG.MVC`
- `Awaken.TG.MVC.Domains`

Namespace presence is a navigation aid, not proof that every contained type is a supported mod API.

## Entry format

When a type/member is promoted into this reference, record where known:

- assembly and namespace;
- full type/member name and signature;
- static or instance ownership;
- relevant runtime/build scope;
- known callers or consumers;
- observable side effects;
- lifecycle constraints;
- evidence source and maturity.

Do not turn this area into a bulk decompilation archive. Follow the repository's public-source boundary.
