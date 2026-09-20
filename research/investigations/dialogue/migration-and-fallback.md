# Dialogue Migration and Fallback

A safe migration should be **per exact conversation identity**, not a global Story replacement.

Useful stages:

```text
M0 native baseline
M1 read-only shadow evaluation
M2 new standalone external conversations
M3 exact native-to-external proxy binding
M4 external primary for individually validated binding
M5 wider per-binding cutover
```

## Fallback rule

Native fallback is safest **before** external dialogue has made irreversible progress.

After a canonical consequence is committed—or its outcome is indeterminate—starting the original native Story may duplicate rewards/flags/follow-up state.

Missing, stale, ambiguous or unvalidated bindings should route native.

## Persistence rule

Do not duplicate canonical FoA quest/objective/flag/inventory/faction truth in the dialogue engine's save payload.

Persist only dialogue-owned attempt/idempotency/migration/session-local state that has an explicit restoration contract.
