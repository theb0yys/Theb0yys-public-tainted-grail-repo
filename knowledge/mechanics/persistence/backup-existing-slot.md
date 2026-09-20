---
document_type: mechanic
scope: copy an existing native save slot into a plugin-owned backup archive
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  loader: PASSED_PRIVATE_PLUGIN
  archive_creation_runtime: NOT_RUN_IN_CITED_VALIDATION
last_verified: 2026-09-20
---

# Back Up an Existing Native Save Slot

Smart Save Backups researched a low-interference backup direction:

```text
trigger
→ optionally request ordinary existing native autosave if CanAutoSave
→ otherwise choose latest existing save if policy allows
→ read source slot through CloudService load API
→ copy domain entries to plugin-owned zip
→ keep original game slot/UI unchanged
```

## Storage boundary

Backups live only under the plugin-owned BepInEx config folder.

The project does not:

- create manual save slots;
- extend the save-slot UI;
- rename/delete game saves;
- write backup archives into the game save folder.

## Trigger candidates

Source-inspected trigger surfaces include:

- before `Story.OfferChoice`;
- before selected quest completion/turn-in-like methods;
- timed/area observations;
- concrete provider `EndSave(slotId)` for saves initiated by the mod.

## Failure behaviour

A backup failure must not block the original story/quest/save method.

If a fresh autosave is blocked, policy may copy the latest existing save instead—or skip.

## Evidence boundary

The cited private validation completed build/load/config-folder creation but **did not yet complete in-game backup archive creation**. Treat this as a strongly researched mechanic pending runtime artifact proof.
