---
document_type: mechanic
scope: compose exposure truth, route authority and patrol actor execution
runtime: mono
evidence:
  design: ACCEPTED_CROSS_PROJECT_CONTRACT
  runtime: PARTIAL_BY_EXACT_ROUTE_IMPLEMENTATION
last_verified: 2026-09-20
---

# Route Patrol: Keep Truth, Route and Actor Owners Separate

The Wyrdness route-patrol design is a useful cross-mod ownership model.

## Responsibility split

- **Wyrd Hunt** owns strict Wyrdness exposure truth.
- **Avalon Core** owns reviewed route identity/geometry/authorization.
- **Living Avalon** owns patrol actor selection, placement, spawn, not-saved marking, route binding, movement supervision and cleanup.

Dependency direction is read-only from the executor toward the exposure provider.

Wyrd Hunt does not spawn/move/discard patrol actors. Living Avalon does not recreate Wyrd exposure from native fields. Neither gets to redefine Core route truth.

## Activation shape

```text
read-only confirmed exposure
+ exact Core-authorized route
+ reviewed creature/spawn contract
→ executor may create one bounded temporary patrol actor
→ native game AI owns detection/combat
→ executor owns route binding and cleanup
```

## Fail closed

Missing/incompatible exposure provider means **no Wyrd patrol**, not a guessed exposure calculation.

Missing/stale route authorization means **no route actor**, not a locally copied route.

## Lesson

Cross-mod composition is safer when each project exports one narrow truth/contract rather than sharing mutable state.
