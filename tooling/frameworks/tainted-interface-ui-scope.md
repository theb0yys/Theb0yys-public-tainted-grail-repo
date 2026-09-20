---
document_type: framework
scope: Tainted Interface visual shell vs FoA Mod Manager custom UI scope
runtime: mono
evidence:
  source: SOURCE_INSPECTED
last_verified: 2026-09-20
---

# Tainted Interface UI Scope Boundary

Tainted Interface owns shared visual resources/shell behaviour. FoA Mod Manager currently owns reusable custom-UI/cursor scope services.

## Tainted Interface owns

- semantic UI/style/asset IDs;
- texture caching/teardown;
- UI shell/primitives;
- plugin-owned view composition.

## Shared scope owner handles

- custom UI active scope;
- controller cursor scope;
- previous cursor restoration;
- optional world freeze.

## Important rule

Do not duplicate cursor restoration in every consumer.

A consumer should:

```text
acquire shared UI scope
→ create its owned view
→ use existing EventSystem when required
→ process its semantic UI
→ destroy owned view/subscriptions
→ release shared scope
→ scope owner restores previous cursor/world state
```

A missing active EventSystem should fail closed for a UGUI screen rather than create a second global EventSystem by default.
