# Native Weapon Integration

Use this page as a compact map of **which native FoA layer owns which part of a weapon**.

## Ownership chain

~~~text
ItemTemplate
→ Item
→ ItemEquipSpec
→ ItemEquip
→ CharacterHandBase
→ CharacterWeapon / combat
→ presentation owner such as Drake
~~~

## Definition

`ItemTemplate` defines the weapon's native item identity and attachment graph.

## Runtime item state

`Item` is the runtime Model and carries instance state such as quantity, level, equipped slots, and Elements/attachments.

## Equip

`ItemEquipSpec` makes an item equippable and provides equipment/representation data.

`ItemEquip` owns the runtime equip/unequip representation lifecycle.

`CharacterHandBase` is the held-weapon View used by the native equip chain.

## Combat

`CharacterWeapon` and related combat systems own melee combat behavior.

Changing the visible mesh does not create or redefine a complete weapon.

## Presentation

Rigid weapon visuals can be rendered through Drake while the `Item` / equip / hand chain remains the gameplay owner.

First-person, third-person, and inventory-preview presentation can be separate consumers. Keep them on their native lifecycles.

## Decide what you are changing

Before patching, identify the layer:

- item definition/stat;
- runtime item instance;
- equip behavior;
- combat;
- visual presentation;
- audio/VFX.

Do not make the presentation object the source of truth for the weapon.

## Related pages

- [Weapons](../weapons.md)
- [Drake](../../presentation/drake/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
- [Runtime orchestration/templates](../../core/runtime-orchestration/README.md)
