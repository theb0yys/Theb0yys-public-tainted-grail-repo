# Content Domains and Native Owners

FoA content shares identity, templates, assets and lifecycle concepts, but each content type has its own runtime owners.

| Content | Native owners | Reference |
| --- | --- | --- |
| Items | `ItemTemplate`, `Item`, inventory and stock | [Items](ITEMS.md) |
| Weapons | Item, equip, hand, combat and rigid presentation | [Weapons](WEAPONS.md) |
| Armour | Item, native clothes, stitching and Kandra | [Armour](ARMOUR.md) |
| Creatures and NPCs | `NpcTemplate`, `LocationTemplate`, actor, AI and presentation | [Creatures and NPCs](CREATURES_NPCS.md) |
| Spells | Item, skill/effect graph and cast lifecycle | [Spells and Effects](SPELLS_EFFECTS.md) |
| Recipes | Recipe definitions, station collections and learned recipes | [Recipes and Crafting](RECIPES_CRAFTING.md) |
| Merchants | Shop, stock, pricing and shop UI | [Merchants and Distribution](DISTRIBUTION_MERCHANTS_LOOT.md) |
| World content | Scenes, locations, spawners and travel | [World, Scenes and Travel](WORLD_SCENES_TRAVEL.md) |

## Keep responsibilities separate

Item registration does not supply weapon presentation. A loaded model does not supply an NPC's gameplay components. A recipe displayed in a menu does not establish persistent recipe identity.

Use each domain's native owner for its operation. Keep asset loading, gameplay behaviour, cleanup and persistence distinct.

Supporting references: [Identity](IDENTITY_GUIDS_NAMES.md), [Templates and Registries](TEMPLATES_REGISTRIES.md), [Native Object Ownership](NATIVE_OBJECT_OWNERSHIP.md), and [Resource Lifetime](RESOURCE_LIFETIME.md).
