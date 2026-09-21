# Human Dialogue GUI Maintenance Path - 2026-06-21

## Request

Live testing after the 0.1.19 deploy still showed the human companion dialogue surface with cursor/input problems. The user directed the implementation to use the existing working Avalon Companions path rather than another human-only input experiment.

## Research read

- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- `mods/avalon-companions/docs/research/dialogue-button-input-gate-2026-06-20.md`
- `mods/avalon-companions/docs/research/dialogue-direct-click-gate-2026-06-20.md`
- `mods/avalon-human-companions/src/Plugin.cs`
- `mods/avalon-human-companions/src/HumanCompanionDialogueUi.cs`
- `mods/foa-mod-manager/src/Plugin.cs`
- `mods/foa-mod-manager/src/Patches/GameInputPatch.cs`
- `mods/Tainted Interface/src/Plugin.cs`

## Findings

- The human dialogue already had the 0.1.25-style Rewired pass-through and the 0.1.27-style direct pointer hit-test fallback.
- The remaining mismatch was in the render/update lifecycle. Avalon Companions calls `EnsureCursorForPanel()` and `EnsureDialogueUiHost(...)` from its GUI pass while the dialogue is visible, before drawing the debug panel.
- Avalon Human Companions only refreshed cursor/input from `OnGUI()` when the IMGUI debug panel was visible. The dialogue surface relied on `Update()` and `LateUpdate()` only.
- The live load order shows several input-locking plugins plus FoA Mod Manager and Tainted Interface. A late GUI-pass cursor refresh matches the working companion controller and reduces the chance that another input owner rewrites cursor state after the human dialogue's update pass.
- FoA Mod Manager custom UI scope currently disables `BaseInputModule` instances while it owns mod UI input. That keeps the manual dialogue hit-test fallback important for mouse clicks, because Unity `Button.onClick` may not be enough under manager-owned input.

## Decision

Version 0.1.20 adds the missing Avalon Companions GUI-pass dialogue maintenance to Avalon Human Companions:

- When `_dialogueVisible` is true, `OnGUI()` now calls `EnsureCursorForPanel()` and `EnsureHumanCompanionDialogueUiHost()` before the debug-panel branch.
- The debug-panel branch is unchanged.
- The dialogue command list, proof gates, roster, spawn/swap behavior, scanner, lifecycle guard, input-lock patches, and persistence boundaries are unchanged.

## Boundary

This is a cursor/input lifecycle alignment only. It does not add recruitment, persistence, existing-NPC conversion, story graph dialogue, quest state, crime state, custom targeting, true Wait, saved hold positions, save data, or release-ready human companion behavior.
