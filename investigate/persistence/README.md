---
document_type: investigation
scope: FoA save-domain and sidecar persistence research
runtime: mono
evidence:
  static: CURRENT_BINARY_VERDICT
  runtime: PARTIAL_CANDIDATES
  persistence: NOT_PRODUCTION_READY
last_verified: 2026-09-20
---

# Persistence Investigation

The inspected native save architecture does not expose a supported mutable arbitrary-domain registrar.

That negative result changes the design space.

## Native boundary

Native FoA owns:

- save guards and slot selection;
- native domain serialization;
- provider writes;
- load/cache operations;
- Gameplay and scene restoration;
- save-slot UI/archive format.

## Candidate sidecar model

A sidecar design should separate:

```text
save request
→ capture immutable mod payload
→ native save proceeds
→ correlate successful native completion
→ atomically commit sidecar

load selection
→ stage sidecar
→ native gameplay/scene restore
→ consumer dependencies ready
→ validate binding/schema/integrity
→ apply once
```

## Still unresolved before a production mechanic

- stable slot/provider identity across rename/copy/delete/rotation;
- success/failure semantics for each provider;
- overlapping-save generation correlation;
- crash-consistent Windows atomic replacement;
- exact post-restore apply point;
- schema migration/downgrade;
- missing/corrupt/removed-mod behaviour;
- manual/quick/auto-save matrix.

Until those close, sidecar persistence remains a research/design surface.
