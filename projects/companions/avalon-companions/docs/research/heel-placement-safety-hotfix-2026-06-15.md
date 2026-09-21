# Research: Heel Placement Safety Hotfix

Date: 2026-06-15
Scope: fix user-reported 0.1.3 issue where the close command could kill or discard managed companions.

## Evidence read

- `mods/avalon-companions/docs/research/advanced-command-behavior-2026-06-15.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/living-avalon/src/DryRun/PopulationDirector.cs`

## Report

The user reported that the "heal" command killed companions instead of healing them. Avalon does not implement a healing command; the affected command is the 0.1.3 `Heel` command that sets Follow mode, selects Close range, and recalls active managed companions.

The likely unsafe path is close-range recall for one-session creature candidates. These actors do not use `PetElement.Recall(Vector3)`. Avalon moves them with `Location.MoveAndRotateTo(...)`, so a too-close target at the hero's Y coordinate can place a large creature or undead candidate into collision, bad terrain, or invalid ground.

## Decision

Avalon Companions 0.1.4 may add a safety hotfix:

1. Rename the visible button from `Heel` to `Come Close` to avoid confusion with healing.
2. Keep internal command logging as `heel` for continuity with existing CSV rows.
3. Do not shrink one-session creature candidate placement below the previously tuned Normal offsets when Close range is selected.
4. Verify recall placement through `BaseLocationSpawner.VerifyPosition(target, template, allowSnapToGround: true)` before moving a managed actor.

This remains inside the approved recall/placement command lane. It does not add healing, custom AI, custom target selection, attack commands, persistence, or active squads.

## Validation needed

- Debug and Release build validation.
- Live DLL hash/version verification.
- In-game smoke test: summon an animal, offensive creature, and undead candidate, press `Come Close`, and confirm the companion is not killed/discarded.
- Confirm `companion-command-log.csv` still records the command as `heel` with `touchesTargeting=false` and `touchesPersistence=false`.
