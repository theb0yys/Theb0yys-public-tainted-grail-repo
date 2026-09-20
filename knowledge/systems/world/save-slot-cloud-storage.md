---
document_type: system
scope: native save slots and CloudService read/write surfaces
runtime: mono
evidence:
  static: DECOMPILED
last_verified: 2026-09-20
---

# Save Slots and Cloud Storage

FoA separates save-slot coordination from provider storage.

## Save coordination

`LoadSave` owns save eligibility and slot writes.

Relevant behaviours include:

- `CanAutoSave()` checking native blockers/restrictions/map interactivity;
- `Save(SaveSlot,...)` serializing current save domains then writing asynchronously;
- `QuickSave()` using the native quick-save slot path;
- `AutoSaving` using recurring native actions and the normal save pipeline.

## SaveSlot metadata

The inspected `SaveSlot` model exposes metadata such as:

- `SaveFileName`;
- hero level;
- hero location;
- hero name;
- last-saved time.

Native auto-save and quick-save groups are capped/rotated rather than being unlimited new visible slots.

## CloudService storage API

The abstract provider surface includes read operations such as:

- `BeginLoadSlot(string slotId)`;
- `EnumerateFilesInSlot()`;
- `TryLoadSlotFile(...)`;
- `EndLoadSlot(...)`.

Steam/debug-no-cloud providers use zip-style save archives; game entries are stored as `<domain>.data`. GOG uses Galaxy-backed storage while exposing the same abstract provider interface.

## Boundary

Reading/copying native save-domain bytes through the provider is different from:

- registering a new native save domain;
- adding a visible save slot;
- restoring arbitrary mod state;
- writing directly into the game save folder.
