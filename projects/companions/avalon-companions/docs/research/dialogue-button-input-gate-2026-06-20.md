# Research: Dialogue Button Input Gate

Date: 2026-06-20
Scope: fix the 0.1.24/0.1.23 vanilla-style Unity UI companion dialogue after in-game validation showed the layout was correct but no dialogue option was interactable.
Question: Can the Unity UI dialogue receive button input without reopening gameplay input or weakening the IMGUI debug-panel lock?

## Evidence read

- `mods/avalon-companions/docs/research/custom-dialogue-ui-system-host-2026-06-19.md`
- `mods/avalon-companions/docs/research/vanilla-style-dialogue-layout-2026-06-19.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Patches/PanelInputLockPatch.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- User screenshot/report on 2026-06-20 showing the correct right-side dialogue layout but non-interactable options.

## Findings

- The Unity UI host correctly keeps `BaseInputModule` instances enabled for dialogue; the code only disables those modules through the IMGUI debug-panel path.
- `PanelInputLockPatch` still neutralized Rewired axis and button reads while any companion UI was visible.
- FoA may route EventSystem pointer/submit input through Rewired. In that case, Unity UI buttons render but do not receive clicks because `GetButton*` reads return `false`.
- The IMGUI debug panel still needs strict Rewired neutralization because IMGUI receives mouse events directly and does not need Unity UI input modules.
- `PlayerInput.ProcessLateUpdate` can continue returning `false` while either UI is visible. That preserves movement/look blocking without preventing Unity UI from seeing EventSystem input.

## Approved route for 0.1.25

- Expose a dialogue-visible state from `PetCompanionController`.
- Keep `GameUI.UpdateMousePosition` and `PlayerInput.ProcessLateUpdate` blocked while either companion UI is visible.
- Keep Rewired axis/button neutralization for the IMGUI debug panel.
- Allow Rewired axis/button reads to pass while the Unity UI dialogue is visible so FoA's EventSystem can drive button hover/click/submit.
- Do not change command routing or companion behavior.

## Not approved

- Disabling the input lock entirely.
- Re-enabling gameplay movement/look while the dialogue is open.
- Adding new commands, AI/pathing, target scans, attack actions, healing, persistence, taming, training, or loyalty systems.
- Converting the custom Unity UI host into true native story dialogue.

## Validation needed

- Debug build.
- Release build.
- Release deploy build and live DLL version/hash check.
- Fresh BepInEx load validation showing `Avalon Companions 0.1.25 loaded`.
- In-game smoke: open `Companion`, confirm options hover/click, click at least Follow or Stay, confirm the dialogue closes after a command, confirm Goodbye closes, confirm Esc restores input, and confirm `KeypadPeriod` still opens only the IMGUI debug panel.
