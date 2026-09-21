# Design Notes

## Scope

- Requested behavior: split wolf companion work into its own diagnostic/scaffold-only mod before any advanced behavior.
- Files expected to change: `mods/avalon-wolf-companion/**`.
- Risk level: Low for scaffold; high for future companion behavior.

## Target

- Assembly: none beyond BepInEx and Unity input references.
- Class: `AvalonWolfCompanion.Plugin`.
- Runtime object: no game runtime object is modified.
- Evidence: `Spec_AnimalWolf` has FOA-Diagnostic Tool route-context spawner evidence and a review row at `AVALON-PET-WOLF-001`, but is only a predator companion review candidate. It is not approved for companion behavior.

## Approach

- Patch type or plugin behavior: no Harmony patches.
- Config entries:
  - `General.Enabled`
  - `Research.ResearchModeOnly`
  - `Target.TemplateName`
  - `Target.TemplateGuid`
  - `Diagnostics.LogTargetOnLoad`
  - `Prototype.EnableDisabledSummonTest`
  - `Prototype.DisabledSummonTestHotkey`
- Save impact: none expected.
- Compatibility considerations: no default hotkey, no Harmony patch, no `TG.Main` reference.
- Failure behavior: load logging reports the review row and blocked gates. If the disabled summon-test gate is enabled and hotkey configured, pressing it logs a warning that behavior is blocked.

## Future Gate

Do not add defend/follow/combat behavior until actor safety, faction behavior, command behavior, placement, transition handling, rest behavior, and save/load behavior are researched and validated.
