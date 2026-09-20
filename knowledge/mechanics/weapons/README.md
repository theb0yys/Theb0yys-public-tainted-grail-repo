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

A FoA weapon crosses several owners. Treat these as separate lanes:

```text
definition / identity
→ native ItemTemplate registration
→ acquisition
→ ItemEquip / CharacterHandBase lifecycle
→ combat owner
→ Drake presentation
→ hide/show / teardown
→ persistence and compatibility
```

A weapon can be valid in inventory and combat while its equipped presentation is wrong. Do not reopen registration or damage logic for a presentation-only failure.

## Mechanics

- [Register and resolve a custom weapon](register-and-resolve-custom-weapon.md)
- [Equipped presentation through Drake](equipped-presentation.md)

## Native system references

- [Weapons](../../systems/gameplay/weapons.md)
- [Native weapon integration](../../systems/gameplay/native-weapons/README.md)
- [Drake](../../systems/presentation/drake/README.md)

## Proof boundary

Private runtime evidence demonstrates a custom weapon being redirected through native equip selection into a framework-owned Drake prototype and serving registered mesh/material keys. Full repeated equip/unequip, multiple-instance, hide/show and scene-transition coverage remained separate validation lanes in that receipt.
