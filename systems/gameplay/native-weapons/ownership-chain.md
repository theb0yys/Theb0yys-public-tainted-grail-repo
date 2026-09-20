# Weapon Ownership Chain

Use this page when a weapon change is failing because the wrong layer is being modified.

Canonical overview: [Native Weapon Integration](README.md).

## Chain

```text
ItemTemplate
→ Item
→ ItemEquipSpec
→ ItemEquip
→ CharacterHandBase
→ CharacterWeapon / combat
→ presentation owner
→ audio / VFX sidecars
```

Each layer owns a different responsibility.

## Decide the layer before coding

| Goal | Start with |
| --- | --- |
| change definition/stat | ItemTemplate / stat owner |
| change one runtime instance | Item / runtime attachment |
| make item equippable | ItemEquipSpec / ItemEquip |
| change hand behavior | CharacterHandBase / native hand owner |
| change damage/attack behavior | CharacterWeapon/combat owner |
| replace rigid mesh | Drake/presentation owner |
| change sound/effect | exact audio/VFX playback owner |

## Common category error

A mesh appearing in the hand proves presentation only.

It does not prove:

- correct Item identity;
- equip slot ownership;
- combat behavior;
- inventory serialization;
- first-person/third-person parity;
- save/load.

Likewise, a custom ItemTemplate resolving does not prove the presentation/equip chain.

## Integration rule

Prefer extending one layer while leaving downstream native owners intact.

Example:

```text
custom registered ItemTemplate
→ native Item creation
→ native equip
→ native hand/combat
→ custom presentation at the proven presentation owner
```

## Verification

Walk the full chain and record the first failed owner rather than patching later symptoms.

For rigid presentation see [Drake](../../presentation/drake/README.md). For persistence see [Saving and Persistence](../../world/saving-persistence.md).
