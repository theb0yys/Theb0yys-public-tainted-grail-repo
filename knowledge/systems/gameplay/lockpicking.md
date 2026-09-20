---
document_type: system
scope: native lockpicking entry, tolerance and pick-break ownership
runtime: mono
evidence:
  static: DECOMPILED
last_verified: 2026-09-20
---

# Lockpicking

Use this page when you want to change **whether lockpicking starts, lockpick tolerance/durability, or the unlock result**.

FoA separates the entry decision, the minigame, the lockpick item's durability, crime, and the final unlock transition.

## Entry path

`LockAction.OnStart` decides whether the locked object uses:

- an existing interaction;
- a key/tool route;
- key-only restriction;
- the normal lockpicking minigame.

If your feature changes whether the minigame should start, this entry decision matters.

## Unlock

`LockAction.Unlock(...)` performs the native unlock transition.

It also:

- re-enables other actions;
- clears lockpicking state;
- changes the Location's lock state.

Do not replace this with a local "unlocked" flag if you want the native object lifecycle to remain coherent.

## Minigame and pick durability

`LockpickingInteraction` owns:

- the sweet spot/tolerance behavior;
- pick durability.

When pick HP reaches zero, native logic removes one lockpick item.

`LockpickingInteraction.ConsumePickHP(float)` is therefore a narrow durability-consumption seam.

## Crime is separate

Changing tolerance, pick durability, or even skipping part of the minigame does not automatically remove the crime consequences of attempting a lock.

Preserve the native crime route unless your feature explicitly changes legal behavior.

## Common mistakes

- changing durability by bypassing the whole unlock lifecycle;
- treating a key path and minigame path as the same operation;
- suppressing pick consumption but accidentally suppressing crime;
- changing UI/minigame state without completing native `Unlock`;
- assuming one lock type represents every locked object.

## How to verify

Check:

1. exact locked object/owner;
2. entry decision;
3. key/tool/minigame route;
4. tolerance/sweet-spot behavior if changed;
5. pick HP consumption;
6. item removal on break if still expected;
7. crime reporting if relevant;
8. `Unlock` transition;
9. object state/actions after unlock;
10. save/load if lock state persistence is part of the claim.

## Evidence

The entry, unlock, and durability ownership described here is based on inspected Mono code.
