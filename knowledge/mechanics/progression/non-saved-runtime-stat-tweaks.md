---
document_type: mechanic
scope: project progression effects through runtime StatTweak-style hero stats
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: PROJECT_SPECIFIC_PARTIAL
  persistence: NONE_BY_TWEAK
last_verified: 2026-09-20
---

# Non-Saved Runtime Progression Effects

Immersive Progression demonstrates a useful separation:

```text
project-owned committed branch rank
→ bounded runtime effect mapping
→ non-saved HeroStats / character-stat tweak
→ native gameplay consumes resulting stat
```

Examples researched/mapped include attack-speed, block, movement, stamina-cost/recovery, bow draw, stealth/noise/visibility, critical/weak-spot and armour-handling stats.

## Why non-saved matters

The runtime tweak should not masquerade as native permanent progression state.

The project can reconstruct or reapply its effect from its own validated progression model while native gameplay continues consuming the ordinary stat.

## Boundary

Each stat target needs its own evidence and balance/runtime validation. A confirmed stat member is not permission to apply an arbitrary value.
