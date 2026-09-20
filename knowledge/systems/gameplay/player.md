# Player / Hero System

Canonical location for established knowledge about native player/hero ownership and execution.

Document the verified native owner, participating models/types/services, lifecycle, data flow, dependencies, presentation boundary, mutation boundary, cleanup and persistence implications.

## Questions this page should answer

- What represents the player/hero at each lifecycle stage?
- Which object owns gameplay state versus presentation?
- When do player references become available or stale?
- Which services or systems consume player state?
- What survives scene transitions, death/reload and save/load?
- Which mutations belong to the player owner and which belong to another system?

## Current proof boundary

Scaffold only. Populate claims from reviewed evidence; unresolved ownership or lifecycle questions belong in [Research](../../../research/README.md).
