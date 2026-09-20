---
document_type: framework
scope: Tainted Weather / Tainted Skybox / Immersive Water owner split
runtime: mono
evidence:
  source: ACCEPTED_OWNER_CONTRACT
  runtime: BOUNDED_RAIN_DAY_GREENSHALLOWS_PASS
last_verified: 2026-09-20
---

# Weather Stack Ownership

The validated project stack separates **weather truth** from **presentation consumers**.

```text
Tainted Weather
  owns weather state / sky intent / water intent
        ↓
Tainted Skybox
  owns Unity skybox selection/mutation
        ↓
Immersive Water
  owns water-surface preset mutation
```

The consumers are optional integration owners, not hidden implementations inside Weather.

## Bounded live proof

A live integration record passed the stack for:

- weather state: `Rain`;
- time bucket: `Day`;
- water preset: `GreenShallows`.

Observed ownership markers kept:

- Weather sky mutation = false;
- Skybox Unity skybox mutation = true;
- Weather external water request = true;
- Immersive Water water mutation = true;
- terrain mutation = false.

## Not generalized

The cited pass does not establish:

- every weather family;
- every time bucket;
- every water preset;
- stale/session/scene negative controls;
- restore/unload behaviour;
- performance/soak;
- release readiness.
