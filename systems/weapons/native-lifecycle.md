# Native Weapon Lifecycle

Document type: **native system**.

Use this page to understand who owns a weapon from durable item identity through equipped view, combat behaviour, presentation, and teardown.

## Ownership model

```text
ItemTemplate
→ Item
→ ItemEquipSpec
→ ItemEquip
→ CharacterHandBase / CharacterWeapon
→ renderer-specific presentation owner
→ detach / discard / release
```

The durable gameplay object is the native `Item`; there is no need to invent a second universal runtime Weapon model.

## Owners

- `ItemTemplate` — weapon identity, classification, attachments and references.
- `Item` — runtime MVC Model and saved item state.
- `ItemEquipSpec` — equippable attachment and representation selection.
- `ItemEquip` — equipped representation lifecycle.
- `CharacterHandBase` — native equipped View.
- `CharacterWeapon` — melee implementation and native combat/sweep behaviour.
- renderer-specific owner such as Drake — rigid presentation/resource lifetime where selected.
- inventory/equipment/save systems — durable ownership.

## Equip lifecycle

```text
Item equipped
→ ItemEquip selects representation
→ asset reference loads GameObject
→ validate owner/item still valid
→ instantiate under native socket
→ require CharacterHandBase
→ SetUnityRepresentation(...)
→ bind View to Item
→ attach to character
→ active combat/presentation

unequip
→ detach
→ discard hand/View
→ release asset reference
```

## Important boundaries

### Presentation is not combat ownership

A custom mesh does not define native melee sweep/hit behaviour.

### Visible is not fully integrated

A direct Unity renderer can appear while bypassing equip View ownership, FPP/TPP handling, renderer lifetime, or teardown.

### Registration is upstream

Weapon template registration uses the item/template system. See [Items](../items/README.md).

### Persistence is separate

A runtime weapon path does not establish save/load, missing-package, migration, or uninstall behaviour.

## Canonical mechanic

- [Custom weapon integration](../../mechanics/weapons/custom-weapon-integration.md)

## Related systems

- [Items and Inventory](../items/README.md)
- [Native object ownership](../core/native-object-ownership.md)
- [Saving and persistence](../persistence/README.md)

## Current proof boundary

The native `Item → ItemEquip → CharacterHandBase/CharacterWeapon` architecture is strongly understood. Generic custom-weapon importer behaviour remains bounded by the proof state documented in the mechanic page.
