---
document_type: mechanic
scope: suppress overlapping native music while plugin-owned music is active
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: PATCH_REGISTRATION_AND_LOAD_PARTIAL
  audible_overlap: NOT_FULLY_VALIDATED
last_verified: 2026-09-20
---

# Scoped Native Music Suppression

When a plugin intentionally owns the current music lane, native exploration/alert/combat music can otherwise play underneath it.

Tainted Music researched a narrow suppression route around the three native music start paths and emitters.

## Boundary

While plugin-owned music is active:

- suppress native exploration/alert/combat starts;
- stop only the three native music emitters as a backstop;
- bypass suppression for configured unique/dramatic/boss scene or event exceptions.

Do **not** suppress:

- ambience;
- snapshots;
- dialogue;
- UI;
- SFX.

## Compatibility risk

The targeted native play methods are private/internal and therefore patch-sensitive.

If they cannot be found, fail by allowing native music rather than muting broad AudioCore behaviour.

## Proof boundary

Private build/load/patch-registration evidence exists, but the cited validation still had audible native-overlap and unique-authored exception testing pending.
