---
document_type: mechanic
scope: custom passive vitals HUD reading native hero limited stats
runtime: mono
evidence:
  source: SOURCE_INSPECTED
  build: MULTIPLE_PRIVATE_BUILDS_PASSED
  visual_runtime: PARTIAL_AND_VERSION_SPECIFIC
  save: NONE
last_verified: 2026-09-20
---

# Read-Only Custom Vitals HUD

A passive custom vitals surface can read native truth without becoming a second stats model.

Read:

- `Hero.Current.Health`;
- `Hero.Current.Stamina`;
- `Hero.Current.Mana`.

Render those values in plugin-owned UI.

## Passive HUD rule

A passive vitals HUD should not:

- request modal custom-UI scope;
- unlock cursor;
- freeze gameplay input;
- freeze world time;
- write stat values;
- own save state.

Shared visual styles may be consumed without taking shared input ownership.

## Visual proof boundary

The private HUD project went through many screenshot-driven placement corrections and embedded-theme iterations.

Build/deploy/resource checks do not equal in-game alignment/readability proof for every resolution/theme/layout.

Treat each validated visual arrangement separately.
