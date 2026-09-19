# Diagnose — Works Now but Not after Load

Document type: **troubleshooting**.

## Symptom

A modded state or content path works in the current session but is missing, broken, duplicated, or unresolved after save/load or restart.

## First rule

Preserve the runtime result.

A feature can legitimately be:

- **runtime-passed**
- **persistence-unproven**

at the same time.

Do not rewrite the runtime mechanic until you know the persistence stage that failed.

## Persistence path

For durable state, trace:

```text
runtime state valid
→ durable identity known
→ capture/save request
→ successful durable write
→ process exit
→ load/restore starts
→ required definitions/services ready
→ saved identity/state resolves
→ apply/restore once
→ downstream behaviour valid
```

## 1. Confirm whether persistence is intended

Some features are deliberately session-only.

If the design uses an explicit not-saved/session-only policy, absence after load may be correct.

## 2. Identify what the save actually stores

For template-backed content, establish:

- the saved identity;
- whether the identity is stable;
- whether the custom definition exists before restore needs it.

For mod-owned side state, identify the actual persistence owner rather than assuming native save extensibility exists.

## 3. Separate save request from successful durable write

A callback at save-request time proves capture opportunity, not disk/provider success.

Require a success milestone appropriate to the persistence mechanism.

## 4. Check restore timing

A common failure shape is:

```text
saved identity exists
→ load begins
→ custom registry/service not ready yet
→ restore lookup/apply fails
```

Check whether registration/readiness occurs before the saved data is consumed.

## 5. Check apply idempotency

If state is restored twice, symptoms may include:

- duplicate items/effects;
- repeated handlers;
- double stat changes;
- duplicate actors.

A durable system needs a clear generation/one-time apply rule.

## 6. Check missing-mod / migration behaviour

Durable support should define what happens when:

- the mod is disabled;
- a custom identity disappears;
- schema changes;
- two versions disagree;
- content is removed.

Do not call persistence complete without documenting these cases.

## 7. Do not treat a candidate sidecar architecture as proven

Current research identifies a possible sidecar transaction model for arbitrary mod-owned state, but generic runtime/save validation remains separate.

Do not implement or recommend it as a proven recipe merely because static ownership points are known.

## Evidence to collect

```text
runtime identity/state
save slot/storage identity
capture marker
durable success marker
restart
load-stage marker
registration/service readiness
restore/apply marker
post-restore identity/state
duplicate count
missing-mod behaviour
```

## Related case

[Runtime success while persistence is still unproven](../../examples/failures-and-corrections/runtime-success-persistence-unproven.md)

## Canonical system

[Saving and Persistence](../../systems/persistence/README.md)
