---
document_type: investigation
scope: FoA save-domain and sidecar persistence research
runtime: mono
evidence:
  static: CURRENT_BINARY_PARTIAL
  runtime: NOT_PRODUCTION_READY
last_verified: 2026-09-20
---

# Persistence Investigation

Use this investigation area when a feature needs durable mod-owned state and the correct save/load integration is still being established.

The current work focuses on what FoA's native save architecture actually supports, where arbitrary native save domains are blocked, and which sidecar/lifecycle options still require runtime proof.

## Current sidecar lifecycle candidates

### Save capture

`LoadSave.Save(SaveSlot,bool)` Prefix is the strongest static capture candidate because it owns the exact `SaveSlot` before asynchronous provider work begins.

Capture candidates:

- `SaveSlot.ID`
- `SaveSlot.SaveFileName`
- `SaveSlot.HeroId`
- transaction sequence
- immutable namespace payloads

### Native success

`SaveInProgressHandle.MarkSucceeded()` is the strongest current-binary static success milestone found.

Do not treat provider `EndSave(string)` alone as sufficient success proof.

### Load stage

`LoadSave.LoadSaveSlotToCache(SaveSlot)` is the strongest static stage point. Validate the sidecar here but **do not apply it yet**.

### Post-restore apply

`SceneLifetimeEvents.Events.AfterSceneStoriesExecuted` is the strongest static native delayed-apply pattern identified for save-slot-owned post-load state.

## Identity

Current static model:

```text
primary storage key = provider + SaveSlot.SaveFileName
runtime/model alias = SaveSlot.ID
secondary mismatch guard = HeroId
visible save name = display only
```

## Remaining proof

Overlapping saves, retries, provider differences, crash consistency, delete/key reuse, filesystem copy, quick/auto-save rotation, apply ordering and migrations remain runtime/save validation work.
