# Human Dialogue Mod Manager Input Scope - 2026-06-21

Superseded note: version 0.1.19 changes the scope order again to match the proven Avalon Companions controller lifecycle through the current Tainted Interface bridge first, with FoA Mod Manager fallback. The Rewired dialogue pass-through from this note remains part of the accepted fix.

## Request

User confirmed the proof roster panel/spawn path now works, then reported that the native `Companion` dialogue input and cursor freeze when the companion-style dialogue surface opens. The requested fix was to use the FoA Mod Manager APIs.

## Research read

- `mods/foa-mod-manager/docs/mod-author-guide.md`
- `mods/foa-mod-manager/src/FoAModManagerApi.cs`
- `mods/foa-mod-manager/src/Patches/GameInputPatch.cs`
- `mods/avalon-companions/src/Patches/FoAModManagerBridge.cs`
- `mods/avalon-companions/src/Patches/PanelInputLockPatch.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-human-companions/src/FoAModManagerBridge.cs`
- `mods/avalon-human-companions/src/Patches/HumanPanelInputLockPatch.cs`
- `mods/avalon-human-companions/src/Plugin.cs`

## Findings

FoA Mod Manager exposes `FoAModManagerApi.SetCustomUiScope(ownerId, active, freezeWorld)` for plugin-owned Unity UI. That scope shows/unlocks the cursor, enables manager controller cursor handling, and freezes gameplay input while custom UI is active.

FoA Mod Manager also exposes `IsControllerCursorInputReadActive`. Mods that patch Rewired input must allow the manager's own cursor reads to pass through while it is collecting controller cursor input.

The working Avalon Companions input lock keeps the IMGUI/debug UI strict, but it explicitly lets Rewired axis and button reads pass while the companion dialogue surface is visible:

```csharp
if (!PetCompanionController.IsCompanionUiVisible ||
    PetCompanionController.IsDialogueVisible ||
    FoAModManagerBridge.IsControllerCursorInputReadActive())
{
    return true;
}
```

Avalon Human Companions had the Mod Manager bridge and custom UI scope call already, but `Plugin.SetControllerCursorScopeActive` tried Tainted Interface scope first. More importantly, `HumanPanelInputLockPatch` blocked Rewired axis/button reads whenever `_panelVisible || _dialogueVisible` was active. That matched the reported failure: the debug panel needed a hard input lock, but the dialogue-style surface needed Rewired input to keep UI navigation and cursor handling responsive.

## Decision

Version 0.1.18 should:

- Prefer FoA Mod Manager `SetCustomUiScope` when the human debug panel or dialogue surface opens.
- Fall back to Tainted Interface custom scope only when the FoA Mod Manager API is unavailable.
- Continue releasing both scope paths on close so stale ownership is not left behind after fallback changes or load-order changes.
- Expose human dialogue visibility to the input patch.
- Keep `GameUI.UpdateMousePosition` and `PlayerInput.ProcessLateUpdate` blocked while any human UI surface is active, preserving the existing world/player freeze.
- Let Rewired axis/button reads pass while the dialogue-style surface is visible, matching Avalon Companions.
- Continue allowing FoA Mod Manager controller cursor reads through at all times.

## Boundary

This is a cursor/input ownership correction only. It does not add recruitment, persistence, existing-NPC conversion, story graph dialogue, quest state, crime state, custom targeting, true Wait, saved hold positions, save data, or release-ready human companion behavior.
