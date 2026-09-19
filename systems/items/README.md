# Items and Inventory — Native Ownership Map

Document type: **native system hub**.

This page explains where item definition, runtime item ownership, acquisition, distribution, and persistence meet. It does not repeat the full custom-item procedure.

## Mental model

```text
ItemTemplate identity
→ TemplatesLoader / TemplatesProvider
→ World-owned Item
→ acquisition/distribution owner
→ inventory/UI/gameplay consumer
→ persistence when durable
```

## Canonical owners

- `ItemTemplate` — reusable item definition.
- `TemplatesLoader` — template loading/insertion lifecycle.
- `TemplatesProvider` — normal lookup.
- `World` — live `Item` model ownership.
- `HeroItems` — hero inventory.
- distribution owners such as merchant stock, pickups, containers, crafting, rewards and loot systems.
- save/template lookup — separate durable-state concern.

## Important separations

### Definition is not registration

A cloned Unity object does not become normal FoA content until the template system can resolve its custom identity.

### Registration is not acquisition

A resolvable `ItemTemplate` is still not in the player's inventory, merchant stock, loot container, reward flow or crafting output.

### Acquisition routes have different owners

Direct grant, vendor stock, world pickup, container transfer, corpse loot, recipe output and quest rewards are not interchangeable.

### Runtime success is not persistence proof

A custom GUID can work in the current session while save/load or missing-mod behaviour remains unproven.

## Canonical mechanics

- [Custom item integration](../../mechanics/items/custom-item-integration.md)
- [Acquisition](../../mechanics/items/acquisition.md)
- [Distribution](../../mechanics/items/distribution.md)

## Cross-cutting systems

- [Templates and registries](../core/templates-and-registries.md)
- [Native object ownership](../core/native-object-ownership.md)
- [Saving and persistence](../persistence/README.md)

## Learning route

- [Items content-authoring journey](../../learn/content-authoring/items/README.md)

## Evidence boundary

The current public baseline supports a bounded native-prototype clone/registration route, native item creation, and specific acquisition/distribution examples. It does not establish universal loot-table authoring, arbitrary from-scratch item definitions, or universal save/uninstall safety.
