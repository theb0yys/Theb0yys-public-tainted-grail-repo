# Research: Native Dialogue Decision Gate

Date: 2026-06-21
Scope: close the 0.1.41 decision gate for true native `VDialogue` / `StoryBookmark` companion dialogue.
Question: Can Avalon Companions safely move from the plugin-owned Unity UI command surface to a real native `VDialogue` / `StoryBookmark` route now?

## Evidence read

- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/docs/research/native-quick-commands-2026-06-15.md`
- `mods/avalon-companions/docs/research/panel-debug-native-dialogue-roadmap-2026-06-19.md`
- `mods/avalon-companions/docs/research/custom-dialogue-interface-2026-06-19.md`
- `mods/avalon-companions/docs/research/custom-dialogue-ui-system-host-2026-06-19.md`
- `mods/avalon-companions/docs/research/vanilla-style-dialogue-layout-2026-06-19.md`
- `mods/avalon-companions/docs/decisions/0004-custom-dialogue-interface.md`
- `mods/avalon-companions/docs/decisions/0005-unity-ui-dialogue-host.md`
- `mods/avalon-companions/docs/decisions/0006-vanilla-style-dialogue-layout.md`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- Focused repo search for `VDialogue`, `StoryBookmark`, `StoryGraphRuntime`, `DialogueAction`, `PetTalkAction`, `StoryInteractAction`, and runtime story registration evidence.

## Findings

- The safe native entry point is already the runtime-only `AvalonCompanionCommandAction : AbstractLocationAction` labelled `Companion`.
- That action is `IsNotSaved`, uses `InteractRunType.DontRun`, and opens the plugin-owned companion command surface without writing to templates, story graphs, bookmarks, or vanilla serialized interaction lists.
- The current player-facing command surface is the plugin-owned Unity UI dialogue host with right-side choices and a bottom dialogue text band.
- Prior decompilation evidence shows native `DialogueAction`, `PetTalkAction`, and `StoryInteractAction` require valid `StoryBookmark` data that resolves through `StoryGraphRuntime`.
- The repo search found no later evidence that this BepInEx plugin can safely author, bake, register, or inject companion `StoryGraphRuntime` data at runtime.
- Reusing arbitrary existing game-authored bookmarks would couple companion commands to unrelated story content, quest state, localization, and authored choice graphs. That is not a safe companion framework contract.

## Decision

Avalon Companions 0.1.41 formally keeps the plugin-owned Unity UI as the supported companion dialogue surface.

Approved supported route:

1. Keep one runtime-only native `Companion` `AbstractLocationAction` on managed one-session companion actors.
2. Keep the action plugin-owned, not saved, and attached only to tracked managed companions that satisfy the existing native-safe gates.
3. Open the plugin-owned Unity UI dialogue host from that action.
4. Keep all visible choices routed through existing approved companion command methods.
5. Keep `Avalon Companions Debug` separate on `KeypadPeriod`.

Blocked native route:

1. Do not create fake `StoryBookmark` values.
2. Do not add runtime-authored story graphs.
3. Do not add `DialogueAttachment`, `PetTalkAttachment`, `StoryInteractAction`, or template edits.
4. Do not write to vanilla serialized interaction lists.
5. Do not call the plugin-owned Unity UI true native `VDialogue`.
6. Do not add recruitment, quest, affinity, taming, training, loyalty, persistence, target selection, attack UI, or Core-executed behavior through this gate.

## Future unlock condition

A real native dialogue route can be reconsidered only after a separate research pass proves one of these without template/save/story corruption:

- a game-authored companion-safe bookmark intentionally meant for this use,
- a supported runtime story graph registration path with validation and rollback,
- or a native UI API that exposes dialogue choice presentation without requiring `StoryBookmark` / `StoryGraphRuntime` content.

## Validation needed

- Debug build.
- Local Release build.
- `git diff --check -- mods/avalon-companions`.
- No in-game smoke is required for this design-only slice because no runtime behavior changes beyond version metadata are intended.
