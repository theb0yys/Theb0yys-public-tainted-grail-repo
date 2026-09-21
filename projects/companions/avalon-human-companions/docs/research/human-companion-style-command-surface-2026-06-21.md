# Human Companion-Style Command Surface

Date: 2026-06-21

## Scope

Replace the visible Avalon Human Companions proof command panel with the same vanilla-style Unity UI composition used by Avalon Companions: a full-screen transparent blocker, right-side command choices, and a bottom dialogue text band.

This is a command-surface/layout change only. It does not add recruitment, persistence, existing-NPC conversion, true Wait, saved hold positions, story graph dialogue, quest state, crime state, custom targeting, attack commands, or release-ready human companion behavior.

## Evidence read

- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/research/human-native-companion-prompt-2026-06-21.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/docs/research/native-quick-commands-2026-06-15.md`
- `mods/avalon-companions/docs/research/vanilla-style-dialogue-layout-2026-06-19.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`

## Decision

Version 0.1.14 incorrectly put the companion-style dialogue surface on the proof-panel hotkey route. Version 0.1.15 corrects the route split to match Avalon Companions:

- Native NPC `Companion` prompt opens the companion-style Unity UI dialogue choices.
- `HumanCommandPanel.TogglePanelHotkey` opens the IMGUI debug/control panel.
- The debug/control panel uses a compact Avalon Companions Debug-style layout.

Approved:

- Create a plugin-owned `Canvas` with `ScreenSpaceOverlay` render mode for the native prompt dialogue path.
- Add a transparent full-screen raycast blocker.
- Place command choices on the right side of the screen.
- Place proof status/dialogue text in a bottom-centered translucent band.
- Route choices to the existing proof backend: Follow, Hold, Defend, Come Close, Recall, Spawn Proof, Scan Actors, Lifecycle Check, Dismiss, and Goodbye.
- Keep the existing input/cursor scope while either surface is open.
- Keep the hotkey panel as a debug/control surface, not the native dialogue surface.
- Keep command availability bound to the existing proof-only gates.

Not approved:

- Calling this true native `VDialogue`.
- Creating or faking `StoryBookmark` values.
- Registering runtime story graphs.
- Adding dialogue attachments, pet-talk attachments, template edits, save-owned interaction lists, recruitment, persistence, existing-NPC conversion, custom targeting, attack UI, or true Wait.

## Validation needed

- Release build.
- Throwaway-save runtime check: open the human proof command surface from the native `Companion` prompt.
- Confirm native prompt choices appear on the right side and the text band appears at the bottom.
- Confirm `HumanCommandPanel.TogglePanelHotkey` opens the compact debug/control panel instead.
- Confirm Follow, Hold, Goodbye, and Dismiss route correctly and close the surface.
- Confirm close/Esc restores input.
- Confirm BepInEx logs the `layout=vanilla-style` marker.
