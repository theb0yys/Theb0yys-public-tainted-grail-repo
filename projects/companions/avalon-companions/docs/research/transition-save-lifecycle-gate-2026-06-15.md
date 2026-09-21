# Research: Transition and Save Lifecycle Gate

Date: 2026-06-15
Scope: prevent managed companions from duplicating, orphaning, or becoming persistent during fast travel, area transition, rest, quit, and reload validation.

## Evidence read

- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `mods/avalon-companions/docs/research/offensive-creature-roster-expansion-2026-06-15.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/ActorControlDiagnostics.cs`

## Current native lifecycle evidence

- Prior local `TG.Main.dll` inspection found that `NpcHeroSummon` handles portal, fast-travel, and long-teleport positioning for native hero summons.
- Prior local `TG.Main.dll` inspection found that `NpcHeroPetAlly` derives from the native summon/ally path and can be applied per spawned `NpcElement`.
- Prior research did not approve save-owned companion records, reload respawn, recruitment state, or persistent actor ownership for Avalon companions.
- The expanded roster is only approved as one active, explicit-command, one-session companion at a time.

## Decision

Avalon Companions 0.1.2 may add a conservative lifecycle guard and diagnostic dump:

1. Run only after the companion runtime gate is open.
2. Mark managed roster actors not saved during summon, recall, periodic checks, scene changes, long hero moves, and shutdown.
3. Scan live `NpcHeroPetAlly` elements and resolve their parent `Location` so reload/transition leftovers can be detected even when Avalon's in-memory tracker is empty.
4. Discard untracked one-session creature roster allies when `Companions.EnableLifecycleSafetyGuard=true`.
5. Discard excess active roster actors when `Companions.EnableLifecycleSafetyGuard=true`; this build still supports one active roster companion, not squads.
6. Append lifecycle evidence to `companion-lifecycle.csv` when `Diagnostics.WriteCompanionLifecycleDump=true`.

## Boundary

This does not approve:

- persistent companion saves,
- automatic respawn on load,
- companion restoration after quit/reload,
- multi-companion active squads,
- custom travel events,
- quest, story, or vanilla serialized interaction edits,
- native save ownership for `AvalonCompanionCommandAction`.

If quit/reload removes the companion because it was correctly marked not saved, that is the expected 0.1.2 result. A future persistence milestone needs separate research.

## Validation required

- Summon a reviewed one-session candidate, fast travel, and confirm there is no duplicate, orphan prompt, or saved extra copy.
- Summon a reviewed one-session candidate, cross an area/scene transition, and inspect `companion-lifecycle.csv` for scene-change or long-hero-move rows.
- Summon a reviewed one-session candidate, rest, and confirm the actor is not duplicated or persisted incorrectly.
- Summon a reviewed one-session candidate, save, quit, reload, and confirm the actor does not reappear as a persistent unmanaged ally.
- Inspect `BepInEx/config/kane.tgfoa.avalon-companions/companion-lifecycle.csv` for `markedNotSaved=true`, sane active counts, and no repeated untracked roster ally rows.
