# Research: Panel Debug and Native Dialogue Roadmap

Date: 2026-06-19
Scope: reframe the current Avalon Companions panel as a debug surface and define the next safe gate toward native dialogue, custom AI, and companion progression systems.
Question: What can change now without pretending the IMGUI panel is the final command UI or shipping unresearched native dialogue/custom AI?
Game version and branch: not revalidated in game for this note; existing local lane remains FoA Mono with BepInEx v5.

## Evidence read

- `docs/engineering-process.md`
- `docs/mod-lifecycle.md`
- `docs/code-review-standard.md`
- `docs/in-game-ui-quality-standard.md`
- `docs/foa-modding-environment.md`
- `docs/research/README.md`
- `Research/Making Tainted Grail The Fall of Avalon Mods-deep-research-report.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/docs/research/native-quick-commands-2026-06-15.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Findings

- The current plugin-owned IMGUI surface is useful as a development/control panel, but it should not be treated as the final player-facing companion UI.
- The existing native `Companion` action is only a runtime-owned prompt that opens the panel. Prior research does not prove runtime-authored `StoryBookmark` or story graph registration.
- True native dialogue choices still require a separate target pass around `DialogueAction`, `PetTalkAction`, `StoryInteractAction`, `StoryBookmark`, `StoryGraphRuntime`, `VDialogue`, and any available runtime registration or existing reusable story assets.
- Custom AI logic is a later gate after native dialogue/command surface proof. It needs exact target evidence for actor thinking, follow/pathing, combat state, and target selection boundaries.
- Taming, training, and loyalty are feature systems on top of the command/AI framework. They should not be implemented until command surface, actor ownership, persistence boundaries, and state storage are separately approved.

## Implementation boundary for 0.1.19

Avalon Companions 0.1.19 may:

- Rename/reframe the existing IMGUI command surface as `Avalon Companions Debug`.
- Add a `Debug.EnableDebugPanel` config gate for the temporary panel.
- Rename the runtime native prompt bridge to `Companion Debug`.
- Keep existing one-session companion command behavior unchanged when the debug panel is enabled.
- Keep fallback native quick commands available only when `Companions.EnableNativeCommandMenu=false`.

Avalon Companions 0.1.19 must not:

- Add native dialogue/story graph content.
- Create fake story bookmarks.
- Add runtime `DialogueAttachment`, `PetTalkAttachment`, or template edits.
- Add custom AI, custom target selection, attack commands, squads, healing, resurrection, persistence, auto-respawn, taming, training, or loyalty.

## Next gate

The next implementation gate should be a native dialogue proof design. It should inspect the current game assemblies for one of these safe routes:

- a plugin-owned native action that starts an existing valid dialogue bookmark, if one can be reused safely;
- a runtime story/dialogue registration path, if one actually exists and can be proven without template/save edits;
- a native-looking command UI host that does not rely on fake story graph data;
- or an explicit decision that native dialogue is blocked and the next player-facing route must use a polished plugin-owned or shared manager UI instead.

## Validation needed

- Build validation after the debug-panel rename/config change.
- BepInEx load validation showing `Avalon Companions 0.1.19 loaded` and `EnableDebugPanel=True`.
- In-game smoke validation that the prompt label is `Companion Debug`, the panel title is `Avalon Companions Debug`, and commands still behave as before when the debug panel is enabled.
- Config validation that `Debug.EnableDebugPanel=false` suppresses the debug panel/prompt path.
