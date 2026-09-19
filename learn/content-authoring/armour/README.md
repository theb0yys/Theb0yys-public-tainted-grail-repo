# Content Authoring Journey — Armour

Armour requires more than item registration or a loadable skinned mesh.

## Prerequisites

1. [Items journey](../items/README.md)
2. [Items and Inventory](../../../systems/items/README.md)
3. [Native Armour / Kandra Clothes Lifecycle](../../../systems/armour/kandra-clothes-lifecycle.md)

## Journey

### 1. Separate the proof problems

Be able to distinguish:
- source geometry/provenance;
- deformation compatibility;
- Kandra payload/registration;
- logical item/equipment identity;
- native clothes equip/stitch;
- cleanup;
- persistence.

### 2. Follow the staged mechanic

Read [Custom Armour Integration](../../../mechanics/armour/custom-armour-integration.md).

### 3. Do not promote a proof fixture

A Kandra registration proof or test renderer does not automatically become a production custom armour item.

### 4. Keep durable behaviour separate

If the claim includes scene transition, save/load, missing-mod behaviour or migration, validate those lanes separately.

## Completion outcome

You should be able to explain why:

```text
mesh loads ≠ Kandra-valid
Kandra-valid ≠ equipped armour
equipped armour ≠ durable/persistent armour
```
