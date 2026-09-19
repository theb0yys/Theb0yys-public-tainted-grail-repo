<!-- Canonical Wave 5 location. Migrated from docs/reference/CONTENT_DOMAINS.md. -->
# Content Domains and Native Owners

FoA content shares identity, templates, assets and lifecycle concepts, but each content type has its own runtime owners.

| Content | Native owners | Reference |
| --- | --- | --- |
| Items | `ItemTemplate`, `Item`, inventory and stock | [Items](../docs/reference/ITEMS.md) |
| Weapons | Item, equip, hand, combat and rigid presentation | [Weapons](../docs/reference/WEAPONS.md) |
| Armour | Item, native clothes, stitching and Kandra | [Armour](../docs/reference/ARMOUR.md) |
| Creatures and NPCs | `NpcTemplate`, `LocationTemplate`, actor, AI and presentation | [Creatures and NPCs](../docs/reference/CREATURES_NPCS.md) |
| Spells | Item, skill/effect graph and cast lifecycle | [Spells and Effects](../docs/reference/SPELLS_EFFECTS.md) |
| Recipes | Recipe definitions, station collections and learned recipes | [Recipes and Crafting](../docs/reference/RECIPES_CRAFTING.md) |
| Merchants | Shop, stock, pricing and shop UI | [Merchants and Distribution](../docs/reference/DISTRIBUTION_MERCHANTS_LOOT.md) |
| World content | Scenes, locations, spawners and travel | [World, Scenes and Travel](../docs/reference/WORLD_SCENES_TRAVEL.md) |

## Keep responsibilities separate

Item registration does not supply weapon presentation. A loaded model does not supply an NPC's gameplay components. A recipe displayed in a menu does not establish persistent recipe identity.

Use each domain's native owner for its operation. Keep asset loading, gameplay behaviour, cleanup and persistence distinct.

Supporting references: [Identity](../docs/reference/IDENTITY_GUIDS_NAMES.md), [Templates and Registries](../docs/reference/TEMPLATES_REGISTRIES.md), [Native Object Ownership](../docs/reference/NATIVE_OBJECT_OWNERSHIP.md), and [Resource Lifetime](../docs/reference/RESOURCE_LIFETIME.md).
