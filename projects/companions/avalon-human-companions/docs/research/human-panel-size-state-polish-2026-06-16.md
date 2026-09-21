# Human Panel Size and Button State Polish

Date: 2026-06-16

## Scope

Fix the Avalon Human Companions proof panel after in-game screenshot validation showed the 0.1.9 panel was still too small and vertically clipped. The user also reported that button color/state looked wrong after using an action.

This is a proof-panel UI polish pass only. It does not approve recruitment, persistence, existing NPC conversion, save-backed roster data, dialogue, quest state, inventory, equipment, leveling, attack commands, custom targeting, true Wait, saved hold positions, or full release-ready human companion behavior.

## Evidence read

- `mods/avalon-human-companions/docs/research/human-panel-flow-input-lock-and-diagnostic-map-2026-06-16.md`
- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/validation-plan.md`
- `mods/avalon-human-companions/docs/test-notes.md`
- `mods/avalon-human-companions/docs/compatibility.md`
- User screenshot: `C:/Users/kane0/Pictures/Screenshots/Screenshot (3687).png`

## Decision

Version 0.1.10 may polish the proof panel visual sizing and button state behavior.

Approved changes:

- Increase the default proof panel size from `980x620` to `1320x860`.
- Increase minimum panel size to keep the two-column proof flow readable.
- Increase the left status column width so target/active actor GUIDs have more room.
- Increase header, close button, label, muted text, and button sizes slightly.
- Keep the panel clamped to the current screen.
- Keep the panel draggable from the header.
- Keep every panel command scoped to the active plugin-owned one-session proof actor.
- Clear IMGUI button hot/keyboard focus after panel actions so buttons do not stay in a strange active/focused color state.
- Pin button `normal`, `hover`, `active`, `focused`, and `on*` GUI states so Unity's default skin cannot leak unexpected colors after a click.
- Change the selected command color from the harsher brown state to a calmer teal selected state with gold text.

Not approved:

- New commands.
- Any change to actor scanner behavior.
- Any change to proof actor spawn, ally marker, faction, Hold, Dismiss, lifecycle, save/load, or persistence behavior.
- Any `Time.timeScale` pause.
- Calling the panel release-ready before the 0.1.10 build is tested in game.

## Validation needed

1. Build 0.1.10.
2. Deploy 0.1.10 to the live BepInEx plugin folder.
3. Confirm local and live DLL hashes match.
4. Launch the game and confirm `Avalon Human Companions 0.1.10 loaded`.
5. Open the proof panel and confirm the bottom section and footer are visible at the user's in-game resolution.
6. Use `Follow`, `Hold`, `Come Close`, `Recall`, `Defend`, `Dismiss`, `Scan Actors`, and `Lifecycle Check` where available and confirm buttons do not stick in strange colors after clicks.
