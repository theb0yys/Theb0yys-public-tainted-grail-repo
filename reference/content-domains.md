# Content Domains and Native Owners

FoA content shares identity, templates, assets and lifecycle concepts, but each content type has its own runtime owners.

| Content | Native owners | Reference |
| --- | --- | --- |
| Items | `ItemTemplate`, `Item`, inventory and stock | [Items](../systems/gameplay/items.md) |
| Weapons | Item, equip, hand, combat and rigid presentation | [Weapons](../systems/gameplay/weapons.md) |
| Armour | Item, native clothes, stitching and Kandra | [Armour](../systems/gameplay/armour.md) |
| Creatures and NPCs | `NpcTemplate`, `LocationTemplate`, actor, AI and presentation | [Creatures and NPCs](../systems/gameplay/creatures-npcs.md) |
| Spells | Item, skill/effect graph and cast lifecycle | [Spells and Effects](../systems/gameplay/spells-effects.md) |
| Recipes | Recipe definitions, station collections and learned recipes | [Recipes and Crafting](../systems/gameplay/recipes-crafting.md) |
| Merchants | Shop, stock, pricing and shop UI | [Merchants and Distribution](../systems/gameplay/distribution-merchants-loot.md) |
| World content | Scenes, locations, spawners and travel | [World, Scenes and Travel](../systems/world/world-scenes-travel.md) |

## Keep responsibilities separate

Item registration does not supply weapon presentation. A loaded model does not supply an NPC's gameplay components. A recipe displayed in a menu does not establish persistent recipe identity.

Use each domain's native owner for its operation. Keep asset loading, gameplay behaviour, cleanup and persistence distinct.

Supporting references: [Identity](identities/identity-guids-names.md), [Templates and Registries](../systems/core/templates-registries.md), [Native Object Ownership](../systems/core/native-object-ownership.md), and [Resource Lifetime](../systems/core/resource-lifetime.md).
