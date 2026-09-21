# Research: Native Quick Commands

Date: 2026-06-15
Scope: add the second native-command layer after the runtime `Companion` prompt bridge.

## Evidence read

- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/docs/research/advanced-command-behavior-2026-06-15.md`
- `mods/avalon-companions/docs/research/heel-placement-safety-hotfix-2026-06-15.md`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

Avalon Companions 0.1.5 may add native quick-command action elements to managed one-session companions:

- `Follow`
- `Stay`
- `Defend`
- `Come Close`
- `Recall`
- `Dismiss`

These are separate plugin-owned `AbstractLocationAction` elements attached only to managed one-session companion `Location` instances that already have the native `NpcHeroPetAlly` marker. Each quick command reuses the same controller path as the existing panel button and writes the same command CSV rows.

0.1.6 correction: user smoke testing showed that keeping the existing `Companion` panel bridge beside the quick-command actions causes native interaction to start the panel bridge first. Because FoA's `DefaultAction(hero)` returns the first available action, quick-command mode must remove/disable the panel bridge and expose the quick-command actions only. The panel bridge remains available only when `Companions.EnableNativeQuickCommands=false`.

0.1.7 correction: user smoke testing showed that even with the panel bridge removed, separate quick-command actions still resolve through FoA's first/default available action and therefore only expose the first valid mode toggle. Quick-command mode must expose only one available quick action at a time and rotate the cursor after each command: `Follow`, `Stay`, `Defend`, `Come Close`, `Recall`, `Dismiss`, while skipping a mode command that is already active.

0.1.8 correction: user smoke testing showed the cursor can still stick on the final `Dismiss` prompt after the companion is removed. The quick-command cursor now resets before dismiss removes the actor and after successful summon/swap. This rotating native prompt should be treated as a temporary fallback, not the preferred command surface.

Next UI direction: use one runtime-only native `Companion` prompt to open a proper plugin-owned command menu. Current research does not approve directly converting vanilla `PetTalkAction`, `DialogueAction`, `StoryInteractAction`, or `SPetInteract` into Avalon companion commands, because those paths depend on valid `StoryBookmark`/story graph data or `PetVariantBase` actors. The safe proper-menu route is runtime-only and must not write into vanilla serialized interaction lists.

0.1.9 implementation: `Companions.EnableNativeCommandMenu=true` is now the default command surface. Managed one-session companions receive one runtime-only native `Companion` action that opens the Avalon command menu. The rotating quick-command actions are fallback-only when `Companions.EnableNativeCommandMenu=false` and `Companions.EnableNativeQuickCommands=true`.

## Boundary

This does not add true dialogue choices. It does not create or fake `StoryBookmark` values, story graphs, dialogue attachments, pet-talk attachments, template edits, or save-owned interaction lists.

This also does not approve:

- attack buttons,
- arbitrary target selection,
- forced hostility,
- healing,
- persistence or reload restoration,
- active squads,
- humanoid companions.

`Defend` remains limited to the native `NpcHeroPetAlly.EnterCombat()` path and still requires the hero to have live attackers before native combat is prompted.

## Validation needed

- Build and deploy validation.
- Spawn a managed one-session companion and confirm the native prompt exposes one `Companion` command-menu action when `Companions.EnableNativeCommandMenu=true`.
- Set `Companions.EnableNativeCommandMenu=false` and `Companions.EnableNativeQuickCommands=true` only to validate the rotating quick-command fallback.
- Confirm `Follow`, `Stay`, `Defend`, `Come Close`, `Recall`, and `Dismiss` run only on managed companions.
- Confirm repeated native interaction rotates through the quick-command cursor instead of staying on the first valid mode toggle.
- Confirm dismiss resets the native quick-command cursor before removing the actor and that the next summoned companion does not inherit a stale `Dismiss` prompt.
- Confirm no quick commands appear on wild, quest, unique, non-managed, or discarded actors.
- Confirm command CSV rows are written and keep `touchesTargeting=false` and `touchesPersistence=false` except for the already-known diagnostic defend classification.
- Confirm dismiss removes all native action elements.
- Confirm `Companions.EnableNativeCommandMenu=true` exposes only the `Companion` native prompt and opens the runtime-only command menu.
