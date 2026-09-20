---
document_type: investigation
scope: sidecar persistence validation plan
runtime: mono
evidence:
  static: CURRENT_BINARY_PARTIAL
  runtime: NOT_PRODUCTION_READY
last_verified: 2026-09-20
---

# Persistence Sidecar Proof Plan

This page turns the current static candidates into an explicit validation plan.

Canonical investigation state: [Persistence Investigation](README.md).

## Current candidate lifecycle

```text
LoadSave.Save(SaveSlot,bool)
→ capture exact save identity/payload
→ native save proceeds
→ SaveInProgressHandle.MarkSucceeded()
→ commit sidecar generation

later load:

LoadSave.LoadSaveSlotToCache(...)
→ validate sidecar identity/schema only
→ native restore proceeds
→ AfterSceneStoriesExecuted
→ apply eligible sidecar state
```

This is a **candidate** architecture, not a promoted public persistence recipe.

## What must be proven

### Save transaction

- exact save request identity;
- overlapping/manual/auto/quick saves;
- success vs failure;
- retry behavior;
- crash between native success and sidecar commit;
- atomic replacement/rename strategy.

### Identity

- provider + `SaveFileName` stability;
- `SaveSlot.ID` alias behavior;
- `HeroId` mismatch handling;
- save copy/rename/delete/reuse behavior.

### Load/apply

- sidecar found for correct slot only;
- schema/version validation before apply;
- apply occurs after required native state exists;
- repeated apply is idempotent;
- missing/corrupt sidecar fails closed;
- stale sidecar does not contaminate another slot.

### Migration

- old schema accepted/migrated or rejected deliberately;
- downgrade policy;
- mod-disabled/missing behavior;
- orphan cleanup policy.

## Evidence ceiling

Static candidates define where to test. They do not prove transaction correctness.

Do not publish a generic sidecar persistence API until controlled disposable-save tests cover the required lifecycle and failure cases.
