# Research: Custom Dialogue Command Interface

Date: 2026-06-19
Scope: answer the next UI gate after 0.1.19 by replacing the debug-panel native bridge with a player-facing companion command-choice surface.
Question: Can Avalon Companions implement a full native or custom dialogue interface now without fake story graph data or new companion AI behavior?

## Evidence read

- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/docs/research/native-quick-commands-2026-06-15.md`
- `mods/avalon-companions/docs/research/panel-debug-native-dialogue-roadmap-2026-06-19.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/PanelInputLockPatch.cs`

## Findings

- The native interact surface still starts one available action. It is not a multi-choice command menu by itself.
- True native dialogue choices still require a valid game-authored or registered story graph and `StoryBookmark`. The prior research does not prove a safe runtime story graph registration route for this BepInEx plugin.
- The existing runtime-only `AvalonCompanionCommandAction` is the safe native entry point because it is attached only to managed one-session companion actors, is not saved, and does not edit templates, story bookmarks, or vanilla serialized interaction lists.
- The existing command methods already implement Follow, Stay, Defend, Come Close, Recall, Recover, range changes, and Dismiss through the reviewed native-safe companion path. A new UI can call those methods without adding custom AI, target scans, attack commands, persistence, or healing.
- `docs/in-game-ui-quality-standard.md` allows a plugin-owned overlay or modal when native UI integration is not yet proven, as long as the route is documented, styled, scoped, input-safe, and validated.

## Implementation boundary for 0.1.20

Avalon Companions 0.1.20 may:

- Restore the runtime native prompt label to `Companion`.
- Add `Companions.EnableCustomDialogueInterface`, default `true`.
- Make the `Companion` prompt open a plugin-owned dialogue-style command surface instead of the debug panel.
- Show all approved commands as visible choices instead of cycling one native quick command at a time.
- Keep `Debug.EnableDebugPanel` and `KeypadPeriod` for the separate `Avalon Companions Debug` panel.
- Keep fallback native quick commands available only when `Companions.EnableNativeCommandMenu=false`.
- Share cursor/input capture across the debug panel and the custom dialogue surface.

Avalon Companions 0.1.20 must not:

- Create or fake story bookmarks.
- Add runtime story graph, `DialogueAttachment`, `PetTalkAttachment`, `StoryInteractAction`, or template edits.
- Change the reviewed one-session actor ownership, lifecycle, persistence, native ally defend, or command behavior.
- Add custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, active squads, reload restoration, or persistence.

## Approved route

Use the existing native `AbstractLocationAction` as a runtime-only entry point, then show a plugin-owned modal styled as a companion dialogue. The modal is a custom UI surface, not a native `VDialogue` story. It should expose the complete approved command set at once and close after a choice so the player does not need to repeatedly interact with the same prompt to cycle commands.

## Validation needed

- Debug build.
- Release deploy build and live DLL version/hash check.
- BepInEx load log showing `Avalon Companions 0.1.20 loaded` and `EnableCustomDialogueInterface=True`.
- In-game smoke on a throwaway save: summon one managed one-session companion, confirm the world prompt is `Companion`, activating it opens the custom dialogue surface, all choices are visible, each choice routes through existing command behavior, and `Esc`/close releases cursor and world input.
- Confirm `Debug.EnableDebugPanel=false` disables only the debug panel and does not block the `Companion` dialogue prompt when `Companions.EnableCustomDialogueInterface=true`.
- Confirm `Companions.EnableNativeCommandMenu=false` still removes the dialogue prompt path and allows fallback quick commands when `Companions.EnableNativeQuickCommands=true`.
