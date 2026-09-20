---
document_type: mechanic
scope: bounded temporary runtime actor spawn
runtime: mono
evidence:
  static: DECOMPILED
  runtime: PROVEN_FOR_REVIEWED_TGE_WYRDSPIRIT_ROUTE
  persistence: NOT_SAVED_BY_DESIGN
last_verified: 2026-09-20
---

# Bounded Temporary Runtime Spawn

A safe runtime-spawn mechanic needs more than a template GUID.

## Path

```text
exact reviewed template identity
→ confirm game/loading readiness
→ verify requested placement through native placement helper
→ create/spawn native Location
→ mark owned actor not saved immediately
→ wait for Location/Npc initialization
→ verify expected identity/state
→ hand off to the next owner (combat/patrol/ally/etc.)
→ track exact owned handle/reference
→ on terminal cleanup: discard exact owned Location
→ observe model/view destruction
```

## Strong preconditions

Useful gates include:

- exact build/profile where patch-sensitive;
- exact template GUID **and** expected name/type;
- non-unique/appropriate template policy;
- placement bounds and finite coordinates;
- scene/world identity;
- one-attempt or bounded retry policy;
- no automatic retry after unknown outcome.

## TGE live evidence

A bounded TGE encounter run on the pinned Mono build observed a Wyrdspirit initialize/live/visually activate and later confirmed exact model/view destruction. A two-Wyrdspirit composition then repeated the owned lifecycle.

That proof does not make every LocationTemplate safe to spawn.
