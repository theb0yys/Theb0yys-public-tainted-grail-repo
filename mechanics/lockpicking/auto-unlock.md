---
document_type: mechanic
scope: skip the lockpicking minigame while preserving key-only and crime boundaries
runtime: mono
evidence:
  static: DECOMPILED
  runtime: NOT_RUN_IN_CITED_PLAN
  risk: MEDIUM_HIGH
last_verified: 2026-09-20
---

# Auto-Unlock: Preserve Native Gates

`LockAction.OnStart(Hero, IInteractableWithHero)` owns the initial lock interaction.

A safe auto-unlock design must preserve native gates.

## Let vanilla run when

- an existing lockpicking interaction is already active;
- the lock is key-only;
- the hero has the required native key/tool route;
- the hero cannot lockpick this target.

## Auto path

For a normal lockpickable lock:

- invoke the native private unlock path;
- keep native lock/location state transition;
- preserve native lockpicking-crime reporting where applicable.

## Risk

This skips a progression-sensitive minigame and uses private members.

Static target evidence is not enough for release-ready claims; disposable-save progression/door/container testing is required.
