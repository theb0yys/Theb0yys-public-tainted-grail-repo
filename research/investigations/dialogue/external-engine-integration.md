---
document_type: investigation
scope: coexistence between native Story and an external authored dialogue engine
evidence:
  architecture: PASSED
  implementation: BLOCKED
  runtime: NOT_RUN
last_verified: 2026-09-20
---

# External Dialogue Engine Integration

The private Dialogue Overhaul architecture establishes a useful responsibility split without claiming implementation readiness.

## Proposed owners

- **Avalon/social AI** — why/when dialogue intent exists.
- **Dialogue Overhaul coordinator** — foreground session lease, route selection, checks/attempts, semantic outcomes, migration/fallback.
- **External dialogue engine** — selected authored conversation graph, response presentation, local sequencing.
- **Native Story** — every unmigrated/delegated native conversation.
- **FoA native systems** — canonical quests, objectives, flags, inventory, factions, rewards, actors/world/save state.

One foreground conversation must have **one engine owner**.

## Pure preview conditions

External response-condition evaluation may happen repeatedly.

Therefore condition functions should be read-only preview queries.

Do not:

- roll chance;
- consume evidence/items;
- set flags;
- change reputation;
- commit rewards;

inside a condition getter.

## Semantic outcome boundary

The external engine should submit a trusted semantic `OutcomeId`, not arbitrary native method calls.

The coordinator then:

```text
re-read canonical FoA state
→ resolve versioned allowlisted outcome definition
→ prepare typed native operations
→ commit through exact native owners
→ verify receipt
→ only then mark dialogue/session success
```

## Current blockers

- exact installed external-engine package fingerprint;
- current native focus/input/interruption hook map;
- safe native Story suppression for exact bindings;
- active-session save/load restoration;
- cross-domain atomic consequence commit.

This remains research/architecture, not a drop-in integration recipe.
