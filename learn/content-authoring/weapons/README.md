# Content Authoring Journey — Weapons

Weapons build on the item system but add equip, combat and presentation ownership.

## Prerequisites

1. [Items journey](../items/README.md)
2. [Items and Inventory](../../../systems/items/README.md)
3. [Native weapon lifecycle](../../../systems/weapons/native-lifecycle.md)

## Journey

### 1. Separate the owners

Be able to distinguish:
- template/item identity;
- acquisition;
- equip lifecycle;
- combat owner;
- renderer/presentation owner;
- persistence.

### 2. Follow the bounded integration mechanic

Read [Custom Weapon Integration](../../../mechanics/weapons/custom-weapon-integration.md).

### 3. Diagnose by failed lane

A weapon that attacks correctly but is invisible has already proved different facts from a weapon whose template never resolves.

### 4. Keep persistence separate

Use [Saving and Persistence](../../../systems/persistence/README.md) only when durable behaviour is part of the claim.

## Completion outcome

You should be able to trace:

```text
ItemTemplate
→ Item
→ ItemEquip
→ CharacterHandBase / CharacterWeapon
→ renderer owner
→ cleanup
```

and explain which proof lane each stage requires.
