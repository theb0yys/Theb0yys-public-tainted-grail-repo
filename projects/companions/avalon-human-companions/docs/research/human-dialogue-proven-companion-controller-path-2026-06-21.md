# Human Dialogue Proven Companion Controller Path - 2026-06-21

## Request

After the 0.1.18 dialogue input/cursor pass, the user clarified that the human implementation should reference the existing Avalon Companions system and use that proven path rather than a human-only scope experiment.

## Research read

- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- `mods/avalon-companions/src/Patches/PanelInputLockPatch.cs`
- `mods/avalon-companions/src/Patches/FoAModManagerBridge.cs`
- `mods/avalon-human-companions/src/Plugin.cs`
- `mods/avalon-human-companions/src/HumanCompanionDialogueUi.cs`
- `mods/avalon-human-companions/src/Patches/HumanPanelInputLockPatch.cs`
- `mods/avalon-human-companions/src/FoAModManagerBridge.cs`

## Findings

The working Avalon Companions controller uses this UI/input shape:

- `IsCompanionUiVisible` is the union of panel and dialogue visibility.
- `SetPanelVisible` and `SetDialogueVisible` both call `SetControllerCursorScopeActive(IsCompanionUiVisible)` and then `UpdateCursor()`.
- `Update()` and `LateUpdate()` keep the cursor alive through the same `EnsureCursorForPanel()` helper while any companion UI is visible.
- `EnsureCursorForPanel()` owns both Unity cursor unlock/show and shared interactive cursor refresh.
- `SetControllerCursorScopeActive(true)` tries the current Tainted Interface custom UI scope first, then falls back to FoA Mod Manager `SetCustomUiScope`.
- `SetControllerCursorScopeActive(false)` releases whichever scope was acquired.
- Panel input modules are restored only once no companion UI surface remains.
- Rewired axis/button reads pass through while dialogue is visible, but the IMGUI/debug panel remains strict.

The 0.1.18 human pass matched the Rewired dialogue pass-through but inverted the scope order to Mod Manager first. That did not match the proven Avalon Companions controller lifecycle.

## Decision

Version 0.1.19 adopts the Avalon Companions controller path for human proof UI:

- Restore shared interface first, FoA Mod Manager fallback scope order.
- Restore the FoA Mod Manager bridge to the same fire-and-forget reflection shape used by Avalon Companions.
- Add a shared human `UpdateCursor()` helper and call it from panel/dialogue visibility changes and update loops.
- Move shared interactive cursor refresh into `EnsureCursorForPanel()`.
- Restore panel input module state only when neither the human debug panel nor the human dialogue surface is visible.
- Keep the 0.1.18 Rewired dialogue pass-through because it is also part of the proven Avalon Companions path.

## Boundary

This is an input/cursor controller alignment only. It does not add recruitment, persistence, existing-NPC conversion, story graph dialogue, quest state, crime state, custom targeting, true Wait, saved hold positions, save data, or release-ready human companion behavior.
