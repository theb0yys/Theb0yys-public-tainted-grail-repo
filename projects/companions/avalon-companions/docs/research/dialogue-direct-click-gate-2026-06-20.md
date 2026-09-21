# Research: Dialogue Direct Click Gate

Date: 2026-06-20
Scope: fix the 0.1.25 Unity UI companion dialogue after fresh in-game validation showed the vanilla-style options still did not execute commands.
Question: Can Avalon Companions make the visible dialogue choices clickable without weakening the debug-panel input lock or adding new companion behavior?

## Evidence read

- `mods/avalon-companions/docs/research/custom-dialogue-ui-system-host-2026-06-19.md`
- `mods/avalon-companions/docs/research/vanilla-style-dialogue-layout-2026-06-19.md`
- `mods/avalon-companions/docs/research/dialogue-button-input-gate-2026-06-20.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/src/Patches/PanelInputLockPatch.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- BepInEx log showing 0.1.25 loaded, the vanilla-style dialogue host opened, and no command rows after user click attempts.
- User screenshot/report on 2026-06-20 showing the correct dialogue layout still not accepting inputs.

## Findings

- 0.1.25 successfully split Rewired neutralization, but the custom Unity UI `Button.onClick` route still did not receive input in game.
- The host is plugin-owned and already knows every visible choice rectangle, command delegate, and enabled/disabled state.
- A manual hit-test against the visible `RectTransform` choice rows can use `UnityEngine.Input.GetMouseButtonDown(0)` and `RectTransformUtility.RectangleContainsScreenPoint(...)` without depending on FoA's EventSystem path.
- This is narrower than disabling the input lock. It only dispatches a click when the pointer is inside one of Avalon's visible enabled dialogue choices.

## Approved route for 0.1.27, carried by current 0.1.28

- Keep the Unity UI dialogue layout and existing `Button.onClick` handlers.
- Add a dialogue-local direct pointer fallback that checks left-click against enabled choice row rectangles.
- Route direct clicks through the same existing command delegates used by `Button.onClick`.
- Log a compact direct-click marker only when a direct choice dispatch occurs.
- Keep movement/look blocked and keep the IMGUI debug panel input lock unchanged.

## Not approved

- Adding new commands or changing command behavior.
- Re-enabling gameplay movement/look while the dialogue is open.
- Disabling the debug-panel input lock.
- Custom AI/pathing, target scans, attack commands, healing, persistence, taming, training, or loyalty systems.
- Calling this true native `VDialogue`.

## Validation completed

- Debug build passed with 0 warnings and 0 errors.
- Release build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- The current deployed build carrying this fallback reports file version `0.1.28.0`; local and live SHA-256 both equal `36FF6A4F9BF1D9406A87A48ED7D3FE9B988F88B7D27283CD83E0B54B5A267577`.
- Fresh BepInEx load validation passed on 2026-06-20 with `Loading [Avalon Companions 0.1.28]` and `Avalon Companions 0.1.28 loaded`.
- In-game command-click smoke passed by user report and command CSV evidence: `mode-follow` at `2026-06-20 02:45:26.646` and `mode-stay` at `2026-06-20 02:45:36.697` for `Wolf Candidate`.
- The fallback-specific `Avalon Companions dialogue direct choice clicked` marker was not observed, so the validated clicks likely used the normal Unity `Button.onClick` path; the manual fallback remains in place for the original EventSystem failure mode.

## Validation still needed

- Separate UI smoke for Goodbye, Esc input restoration, and `KeypadPeriod` debug-only behavior.
