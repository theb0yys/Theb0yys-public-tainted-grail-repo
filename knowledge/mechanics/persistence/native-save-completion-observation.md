---
document_type: mechanic
scope: observe concrete CloudService EndSave slot IDs
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: IMPLEMENTED_OBSERVATION
  durability_semantics: NOT_PROVEN_GENERICALLY
last_verified: 2026-09-20
---

# Native Save Completion Observation

Smart Save Backups demonstrates a useful **observation seam** across known concrete save providers:

- `SteamCloudService.EndSave(string)`
- `SteamNoCloudService.EndSave(string)`
- `DebugCloudService.EndSave(string)`
- `GogCloudService.EndSave(string)`

A Harmony Postfix receives the `slotId` and can enqueue follow-up plugin work.

## What this proves

It proves that the concrete `EndSave(string)` targets can be located and observed by ordinary Harmony discovery in the inspected/runtime project context.

## What this does not prove

It does **not** automatically establish:

- that every `EndSave` call means a durable successful write;
- complete provider coverage for all future builds;
- safe custom serialization;
- generation correlation for overlapping saves;
- a supported mod-owned native save domain.

Use this as a lifecycle observation point, not as a persistence API.
