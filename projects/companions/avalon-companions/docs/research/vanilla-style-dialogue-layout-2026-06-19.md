# Research: Vanilla-Style Dialogue Layout

Date: 2026-06-19
Scope: correct the 0.1.22 Unity UI dialogue host after user screenshot validation showed it still looked like a centered custom panel instead of FoA's dialogue composition.
Question: Can 0.1.23 move the existing custom dialogue host toward the vanilla dialogue layout without adding native story graph dialogue or advanced companion behavior?

## Evidence read

- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/docs/research/custom-dialogue-interface-2026-06-19.md`
- `mods/avalon-companions/docs/research/custom-dialogue-ui-system-host-2026-06-19.md`
- `mods/avalon-companions/docs/decisions/0005-unity-ui-dialogue-host.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- User screenshots from 2026-06-19: target vanilla dialogue layout with right-side choices plus bottom dialogue text, and current Avalon result with a centered panel.

## Findings

- 0.1.22 solved the renderer class problem by moving the companion command surface from IMGUI to Unity UI, but it did not solve the composition problem. It still created a centered panel with header, context, button rows, and footer.
- The target screenshot uses two separate visual zones: stacked choices on the right side of the screen and a bottom dialogue text band. There is no centered command panel.
- True native `VDialogue` is still blocked by the same evidence gap: native dialogue actions depend on valid game-authored or registered `StoryBookmark` and `StoryGraphRuntime` data.
- The existing runtime-only `Companion` action and Unity UI host remain the approved route because they avoid templates, story graph registration, fake bookmarks, and save-owned interaction edits.
- The fix can be layout-only. Existing command methods, command gates, input scope, event-system handling, lifecycle, recovery, continuity, native ally behavior, and debug panel behavior do not need to change.

## Approved route for 0.1.23

- Keep `Companions.EnableNativeCommandMenu=true` and `Companions.EnableCustomDialogueInterface=true` routing through one runtime-only native `Companion` prompt.
- Keep `Avalon Companions Debug` on `KeypadPeriod` as the separate IMGUI debug panel.
- Replace the centered Unity UI dialogue panel composition with:
  - a full-screen transparent raycast blocker,
  - right-anchored vertical command choices,
  - a bottom-centered translucent dialogue text band.
- Keep all choices routed through the existing approved command functions.
- Add a BepInEx marker line that identifies the layout as `layout=vanilla-style`.

## Not approved

- Calling the result true native `VDialogue`.
- Creating or faking `StoryBookmark` values.
- Registering runtime story graphs.
- Adding `DialogueAttachment`, `PetTalkAttachment`, `StoryInteractAction`, or template edits.
- Adding custom AI/pathing, target scans, attack commands, healing, resurrection, persistence, active squads, taming, training, or loyalty systems.

## Validation needed

- Debug build.
- Release build.
- Release deploy build and live DLL version/hash check.
- Fresh BepInEx load validation showing `Avalon Companions 0.1.23 loaded`.
- In-game smoke on a throwaway save: summon one managed companion, activate `Companion`, confirm the centered panel is gone, confirm choices appear at the right side and dialogue text appears at the bottom, click at least Follow/Stay/Goodbye, confirm close/Esc restores input, and confirm `KeypadPeriod` opens only `Avalon Companions Debug`.
