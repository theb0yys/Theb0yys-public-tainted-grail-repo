---
document_type: mechanic-index
scope: reusable FoA modding capabilities
game_build: mixed
runtime: mixed
evidence:
  static: MIXED
  runtime: MIXED
  persistence: MIXED
  compatibility: MIXED
last_verified: 2026-09-20
---

# Mechanics

Mechanics answer **what can a mod do, through which owner, and with what proof boundary?**

A mechanic is not reusable merely because a method name or Harmony patch exists. A useful mechanic documents the complete relevant path:

```text
prerequisites
→ native owner ready
→ exact identity or target resolves
→ narrow intervention
→ downstream native consumer
→ terminal success
→ cleanup/restoration
→ claim-fit proof
```

Persistence, compatibility and release remain separate proof obligations.

## Core mechanics

- [Intervention selection](intervention-selection/README.md)
- [Items: template resolution and hero grant](items/template-resolution-and-grant.md)
- [Weapons](weapons/README.md)
- [Armour: native clothes/Kandra path](armour/native-clothes-kandra.md)
- [Merchant restock on shop open](merchants/restock-on-open.md)
- [Lockpick durability guard](harmony/lockpick-durability-guard.md)
- [Spell cast VFX overlay](spells/cast-vfx-overlay.md)
- [Completed Addressables handle bridge](assets/completed-addressable-handle.md)
- [Native mount velocity](movement/native-mount-velocity.md)
- [One-session companions](creatures/one-session-companion.md)
- [Modal/custom UI command surfaces](ui/modal-command-surface.md)
- [Recipes: runtime construction vs persistent learning](recipes/runtime-vs-persistent.md)
- [Save-state boundary](save-state/README.md)
- [Native save-completion observation](save-state/native-save-completion-observation.md)

## Read mechanic status correctly

A page may be useful while still being static-only, runtime-partial or blocked. Evidence state is part of the mechanic, not a footnote.

See [Evidence reference](../reference/evidence/README.md) and the [Mechanics catalogue](../reference/mechanics/README.md).
