---
document_type: mechanic-index
scope: custom weapon definition, native item registration and presentation
runtime: mono
evidence:
  static: SUPPORTED
  runtime: PARTIAL
  persistence: PARTIAL
  compatibility: PARTIAL
last_verified: 2026-09-20
---

# Weapons

Use this section when you are implementing a weapon feature and need to understand which part of the weapon you are actually changing.

A FoA weapon spans definition, runtime Item state, equip lifecycle, combat behavior, presentation, teardown, and persistence. Treat those as separate responsibilities so a visual or inventory success is not mistaken for a complete weapon integration.

## Mechanics

- [Register and resolve a custom weapon](register-and-resolve-custom-weapon.md)
- [Equipped presentation through Drake](equipped-presentation.md)

## Native system references

- [Weapons](../../systems/gameplay/weapons.md)
- [Native weapon integration](../../systems/gameplay/native-weapons/README.md)
- [Drake](../../systems/presentation/drake/README.md)

## Proof boundary

Private runtime evidence demonstrates a custom weapon being redirected through native equip selection into a framework-owned Drake prototype and serving registered mesh/material keys. Full repeated equip/unequip, multiple-instance, hide/show and scene-transition coverage remained separate validation lanes in that receipt.
