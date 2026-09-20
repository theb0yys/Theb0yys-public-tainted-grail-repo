---
document_type: mechanic
scope: custom weapon definition and native template registration
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: BOUNDED
  persistence: NOT_GENERALIZED
last_verified: 2026-09-20
known_limits:
  - shared registrar promotion remains evidence-bounded
  - source/build success does not prove save restoration
---

# Register and Resolve a Custom Weapon

A robust custom weapon path needs more than a bundle and a prefab.

## Ownership chain

```text
mod-owned immutable weapon identity
→ source native weapon template identity
→ templates ready
→ clone/derive template
→ collision and definition checks
→ native template map insertion
→ provider lookup verification
→ ordinary game item/equip path
```

Important identities include the custom template GUID/name, definition hash, runtime prototype address, mesh key and material-key namespace.

## Required behaviours for a reusable registrar

A shared registrar should own:

- template readiness;
- source-template resolution;
- source/custom profile validation;
- GUID and name ownership;
- definition hashing;
- idempotency;
- changed-definition rejection;
- collision rejection;
- insertion into the native template map;
- provider readback/lookup verification;
- receipts and failure reasons;
- explicit persistence/missing-package policy.

## What this does **not** prove

Registration alone does not prove:

- equipped rendering;
- combat semantics;
- save restoration;
- copied-save behaviour;
- missing-registrar behaviour;
- broad compatibility.

Those are downstream contracts.

See [Native item registrar ownership](../../../tooling/frameworks/native-item-registrar.md) for the framework boundary.
