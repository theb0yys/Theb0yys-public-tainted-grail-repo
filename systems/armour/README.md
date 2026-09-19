# Armour

Document type: **domain system hub**.

Armour crosses geometry, deformation, Kandra representation, logical item/equipment definition, native clothes ownership, cleanup and persistence.

## Domain map

```text
source geometry
→ deformation compatibility
→ Kandra payload / registration
→ armour item/equip definition
→ BaseClothes
→ ClothStitcher / KandraRig
→ active presentation
→ unequip cleanup
→ separate persistence
```

## Canonical chapters

### Understand the native system

- [Native Armour / Kandra Clothes Lifecycle](kandra-clothes-lifecycle.md)
- [Items and Inventory](../items/README.md)

### Perform the bounded mechanic

- [Custom Armour Integration](../../mechanics/armour/custom-armour-integration.md)

### Learn in sequence

- [Armour content-authoring journey](../../learn/content-authoring/armour/README.md)

## Critical separation

“Mesh imports”, “Kandra registers”, “armour equips”, and “armour survives save/load” are different claims.

## Proof boundary

The public domain model is stronger than the generic importer completion state. Production target equip/deformation/persistence remain partial unless explicitly proven.
