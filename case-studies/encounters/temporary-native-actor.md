---
document_type: case
scope: reviewed temporary runtime NPC creation/removal
evidence:
  runtime: TGE_NATIVE_LIFECYCLE_PASS
last_verified: 2026-09-20
---

# Temporary Native Actor: Prove Cleanup, Not Only Spawn

The TGE encounter proof demonstrated why “spawn returned an object” is too weak.

The accepted lifecycle waited for:

- native loading readiness;
- initialized Location/NPC;
- live actor;
- active visual.

On removal it waited for:

- exact owned Location discard;
- model discard state;
- captured view/visual destruction.

## Lesson

A runtime actor feature needs proof at **both ends** of the lifecycle.
