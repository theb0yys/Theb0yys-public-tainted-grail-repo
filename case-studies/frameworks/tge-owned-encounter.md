---
document_type: case
scope: TGE owned NPC encounter lifecycle
runtime: mono
game_build: pinned TG.Main 749AABB...
evidence:
  runtime: NATIVE_LIFECYCLE_PASS
  human_visual: NOT_RUN
  save_compatibility: NOT_PROVEN
last_verified: 2026-09-20
---

# TGE Owned Encounter Lifecycle

The TGE encounter work demonstrates how a framework can execute a dangerous capability without giving up ownership discipline.

## Bounded proof

A successful live rerun:

- waited for native loading readiness;
- spawned one allowlisted Wyrdspirit near the player;
- observed initialized/living/active visual state;
- removed the exact owned actor and confirmed model/view destruction;
- then expanded a saved **composition definition** into two Wyrdspirits;
- observed and removed both with distinct owned handles.

## Safety model

- exact build fingerprint;
- allowlisted template identity;
- preview/fingerprint before execution;
- world/player displacement checks;
- placement verification;
- `MarkedNotSaved`;
- owned handles instead of global deletion;
- explicit async lifecycle;
- bounded cleanup attempts;
- no automatic spawn retry after unknown outcome.

## Lesson

Framework execution needs **stronger ownership**, not looser ownership, because more consumers may rely on it.
