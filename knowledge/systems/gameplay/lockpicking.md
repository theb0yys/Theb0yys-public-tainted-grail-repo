---
document_type: system
scope: native lockpicking entry, tolerance and pick-break ownership
runtime: mono
evidence:
  static: DECOMPILED
last_verified: 2026-09-20
---

# Native Lockpicking Ownership

`LockAction.OnStart` determines whether a lock follows:

- an existing interaction;
- a key/tool path;
- key-only restriction;
- ordinary lockpicking minigame.

`LockAction.Unlock(...)` performs the native unlock transition, re-enables other actions, clears lockpicking state and changes the location lock state.

`LockpickingInteraction` owns the minigame sweet spot and pick durability.

When pick HP reaches zero, native logic removes one lockpick item.

Lockpicking crime has its own native crime entry and should not be dropped merely because a mod changes tolerance or skips the minigame.
