# Native Weapon Integration

## What it is

A FoA weapon is not one object. It is a chain of native owners covering definition, runtime item state, equip, hand representation, combat and rendering.

## Ownership chain

~~~text
ItemTemplate
→ Item
→ ItemEquipSpec
→ ItemEquip
→ CharacterHandBase
→ CharacterWeapon / combat systems
→ presentation owner (for example Drake for rigid meshes)
~~~

## Definition and runtime state

`ItemTemplate` supplies the native definition and attachment graph.

`Item` is the runtime MVC Model and owns instance state such as quantity, level, equipped slots and attachments.

## Equip

`ItemEquipSpec` makes an item equippable and defines equipment type plus representation data.

`ItemEquip` is the runtime equip Element.

`CharacterHandBase` is a key presentation/equip owner for held weapons.

## Combat

Native combat remains owned by weapon/combat systems such as `CharacterWeapon` rather than by the rendered mesh.

That separation is important: replacing a visible model does not create a complete weapon.

## Presentation

Rigid weapon visuals can enter Drake while the Item/equip/hand chain remains the gameplay owner.

First-person, third-person and inventory-preview presentation are distinct consumers and should stay on their native lifecycle.

## Modding relevance

When changing an existing weapon, decide which layer you are actually changing:

- definition/stat;
- runtime item;
- equip;
- combat;
- presentation;
- audio/VFX.

Do not make a presentation object the source of truth for the weapon.

## Related systems

- [Drake](../drake/README.md)
- [Runtime lifetime](../runtime-lifecycle/README.md)
- [Runtime orchestration/templates](../runtime-orchestration/README.md)
