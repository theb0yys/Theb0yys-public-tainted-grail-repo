---
document_type: mechanic
scope: direct custom ItemTemplate clone/registration
runtime: mono
evidence:
  static: SOURCE_INSPECTED_PLUS_NATIVE_STATIC_CORROBORATION
  runtime: MULTI_CONSUMER_PARTIAL
  persistence: NOT_GENERALIZED
last_verified: 2026-09-20
known_limits:
  - private TemplatesLoader map insertion is patch-sensitive
  - shared registrar is the preferred long-term owner
---

# Register a Custom Item Template

Multiple private implementations independently converge on the same direct registration shape:

```text
TemplatesProvider.AllLoaded
→ resolve reviewed native source ItemTemplate
→ clone source template GameObject
→ assign project-owned GUID/name/presentation fields
→ preserve expected component/type shape
→ insert through TemplatesLoader.AddToMap(...)
→ resolve the custom GUID back through TemplatesProvider
```

## Why this is not the ideal shared architecture

`TemplatesLoader.AddToMap` is a private native surface. Consumer-local reflection duplicates collision, idempotency, lifecycle and persistence risk.

The long-term ecosystem direction is a shared registrar that owns those rules once.

## Boundary

A successful clone/map insertion does not by itself prove:

- save restoration;
- missing-mod behaviour;
- copied-save behaviour;
- equipment presentation;
- merchant/loot/recipe integration.

See [Template save identity](template-save-identity.md) and [Native item registrar ownership](../../../tooling/frameworks/native-item-registrar.md).
