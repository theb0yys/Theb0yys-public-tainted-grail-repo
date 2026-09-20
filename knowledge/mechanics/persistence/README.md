---
document_type: mechanic
scope: mod-owned durable state
runtime: mono
evidence:
  static: CURRENT_BINARY_PARTIAL
  runtime: NOT_RUN_FOR_GENERIC_SIDECAR
  persistence: BLOCKED_FOR_GENERIC_IMPLEMENTATION
last_verified: 2026-09-20
---

# Mod-Owned Save State

Use this section when your mod needs to keep its own state across game restarts or deliberately avoid persistence.

FoA does not currently expose a supported public API for registering an arbitrary new native save domain, so persistence work must start from the kind of state you own and the save/load lifecycle it can safely attach to.

## Fixed native-domain result

For inspected `TG.Main.dll` SHA-256:

`749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982`

the native domain set is fixed and no supported mutable arbitrary-domain registrar was found.

Unknown `<name>.data` entries may passively round-trip, but native restoration does not deserialize arbitrary names as new mod state.

## Sidecar direction: stronger static candidates

Later current-binary lifecycle research narrowed the candidate sidecar path:

```text
LoadSave.Save(SaveSlot,bool) Prefix
→ capture ID + SaveFileName + HeroId + immutable payload
→ native save proceeds
→ SaveInProgressHandle.MarkSucceeded Postfix
→ correlate exact pending generation
→ atomically commit sidecar

LoadSave.LoadSaveSlotToCache(SaveSlot)
→ stage sidecar inertly
→ native Gameplay + scene restoration
→ SceneLifetimeEvents.AfterSceneStoriesExecuted
→ verify generation/hero/scene/prerequisites
→ apply namespace once
```

Important corrections:

- `CloudService.EndSave(string)` is useful observation evidence but is **not** the strongest static success milestone; the native coordinator evaluates its result before `MarkSucceeded()`.
- visible save rename does not change `SaveFileName` in the inspected binary.
- `SaveSlot.ID` and storage `SaveFileName` can differ, so one identifier is not sufficient for transaction correlation.

## Still blocked before production

Runtime/provider/save-kind/crash/copy/delete/rotation/migration validation remains required.
