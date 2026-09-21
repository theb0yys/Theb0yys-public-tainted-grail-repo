# Human Proof Command Panel

Date: 2026-06-15

## Scope

Add a proof-only command panel for Avalon Human Companions because the 0.1.3 native interaction actions did not work in live testing.

This is not a full companion UI, not a release-ready human follower panel, and not a native dialogue/interaction integration. It is a plugin-owned overlay that calls the already bounded one-session proof commands.

## Evidence read

- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/research/human-one-session-native-ally-proof-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-command-surface-2026-06-15.md`
- `docs/in-game-ui-quality-standard.md`
- `docs/code-review-standard.md`
- `docs/hotkey-registry.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/foa-mod-manager/src/Plugin.cs`

## Decision

Avalon Human Companions may add a disabled-by-default, styled, plugin-owned IMGUI overlay opened by a configurable hotkey. The default hotkey is `End`, which is not currently reserved in `docs/hotkey-registry.md`.

The panel may:

- open and close with `End`,
- close with `Esc` or a Close button,
- show the reviewed target and active proof actor status,
- spawn the one-session ally proof through the existing guarded spawn path,
- run Follow, Come Close, Recall, Defend, and Dismiss through the existing proof command backend,
- unlock the cursor while open and restore cursor state when closed,
- reset input axes while open to reduce player movement behind the panel.

The panel must:

- stay disabled by default,
- require `ResearchModeOnly=false` for live proof commands,
- act only on the plugin-owned active one-session proof actor,
- mark the actor not saved when commands run,
- avoid vanilla serialized interaction lists, story graphs, dialogue, quest state, crime state, global faction data, and save data,
- remain documented as proof UI until in-game screenshot or video validation is recorded.

## Not approved

- Stay, Wait, or Hold Position.
- Converting existing NPCs.
- Adding commands to unrelated or existing NPCs.
- Custom target selection or forced attack targeting.
- Full release-ready companion panel claims before visual validation.
- Persistence, roster save data, area-transition reconstruction, or quit/relaunch reconstruction.

## Validation needed

- Build and deploy 0.1.4.
- Confirm `End` opens and closes the panel.
- Confirm `Esc` and Close close the panel and cursor state restores.
- Confirm the panel can spawn the proof actor when the existing proof gates are enabled.
- Confirm Follow, Come Close, Recall, Defend, and Dismiss work from the panel on the active proof actor.
- Confirm no Stay/Wait/Hold control is shown.
- Capture an in-game screenshot before calling the panel release-ready.
