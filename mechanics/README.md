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
- [Native mount velocity](movement/native-mount-velocity.md)
- [One-session companions](creatures/one-session-companion.md)
- [Modal/custom UI command surfaces](ui/modal-command-surface.md)
- [Recipes: runtime construction vs persistent learning](recipes/runtime-vs-persistent.md)
- [Save-state boundary](save-state/README.md)

## Read mechanic status correctly

- **Static/source inspected** means the route exists in code or decompiled evidence.
- **Runtime proven** means the relevant behaviour was directly observed.
- **Persistence proven** requires a save/load or equivalent durable-state test.
- **Compatibility tested** is scoped to the stated environment.
- **Blocked / under evaluation** is useful knowledge: it means the repository knows what is still missing and should not publish a fake recipe.

See [Evidence reference](../reference/evidence/README.md) and the [Mechanics catalogue](../reference/mechanics/README.md).
