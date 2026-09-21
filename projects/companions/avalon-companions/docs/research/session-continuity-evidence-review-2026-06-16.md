# Research: Session Continuity Evidence Review

Date: 2026-06-16
Scope: inspect live lifecycle and command CSV evidence for the session-continuity gate, then decide the smallest allowed next slice.

## Evidence read

- `mods/avalon-companions/docs/research/session-continuity-persistence-gate-2026-06-16.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `A:/SteamLibrary/steamapps/common/Tainted Grail FoA/BepInEx/config/kane.tgfoa.avalon-companions/companion-lifecycle.csv`
- `A:/SteamLibrary/steamapps/common/Tainted Grail FoA/BepInEx/config/kane.tgfoa.avalon-companions/companion-command-log.csv`
- `A:/SteamLibrary/steamapps/common/Tainted Grail FoA/BepInEx/LogOutput.log`

## CSV findings

- `companion-lifecycle.csv` contained 1504 rows from `2026-06-15 06:02:53.302` through `2026-06-16 07:44:16.614`.
- Lifecycle reason counts included 39 `summon`, 21 `dismiss`, 5 `long-hero-move`, 5 `scene-change`, 3 `panel-lifecycle-check`, 3 `recover-precheck`, 3 `recover`, and 1 `shutdown`.
- No lifecycle row with a managed `templateGuid` had `markedNotSaved=false`.
- Latest 0.1.13 recovery/lifecycle rows for `Corpse Eater Candidate` kept `markedNotSaved=true`, `hasNpcHeroPetAlly=true`, `isTrackedCreature=true`, `hasCommandAction=true`, `duplicatesDiscarded=0`, and `untrackedDiscarded=0`.
- Scene-change and long-hero-move rows existed. The 2026-06-16 scene-change rows kept a Qrko active with `markedNotSaved=true`, no duplicates, and no untracked discard.
- Older 2026-06-15 summon rows showed `activeRosterCount=2` with `duplicatesDiscarded=1`; the lifecycle guard discarded the excess active roster actor and kept one active companion.
- The latest shutdown row at `2026-06-16 07:44:16.614` reported `activeRosterCount=0` and `no active roster actors`.
- `companion-command-log.csv` contained 215 rows from `2026-06-15 06:48:06.549` through `2026-06-16 07:20:26.329`.
- Command counts included 34 `summon/swap`, 30 `auto-catch-up`, 26 `dismiss`, 16 `mode-stay`, 9 `mode-follow`, 5 `recall`, 4 `heel`, 3 `lifecycle-check`, 3 `mode-defend`, and 3 `recover`.
- No command row had `touchesPersistence=true`.
- No command row had `touchesTargeting=true`.
- Latest 0.1.13 command rows showed explicit `summon/swap`, `lifecycle-check`, `recover`, `recall`, and `dismiss` activity with `touchesPersistence=false`.
- `LogOutput.log` showed `Avalon Companions 0.1.13 loaded` with `ResearchModeOnly=False`, companion roster, lifecycle dump, command log, native command menu, native quick commands, actor-control diagnostics, and dry-run proof enabled.

## Decision

The evidence is enough to approve only a metadata/status continuity slice. It is not enough to approve actor persistence, reload restoration, auto-respawn, or same-session live actor re-adoption after reload.

Avalon Companions 0.1.14 may add:

1. Plugin-owned metadata for last selected companion GUID, display name, command mode, and follow range.
2. Status text that reports the remembered companion when no active managed companion exists.
3. A manual continuity path that tells the player to use the existing Summon/Swap command; the existing explicit summon path remains the only actor creation path.
4. Command-log evidence for metadata/status decisions with `touchesTargeting=false` and `touchesPersistence=false`.

## Boundary

This does not approve:

- actor persistence,
- automatic companion restoration after load,
- automatic respawn,
- save-owned `Location`, `NpcElement`, `PetVariantBase`, or native command action ownership,
- changing `MarkedNotSaved` behavior,
- restoring a dead or discarded actor,
- same-session actor re-adoption after reload,
- active squads,
- humanoid persistence,
- new combat commands, target selectors, or attack buttons.

The older duplicate-cleanup rows prove the guard can discard excess active roster actors; they do not prove it is safe to persist companions.

## Remaining evidence gap

Full transition/save validation is still incomplete. The next in-game validation pass still needs:

- rest with a managed creature active,
- save, quit, reload, and return with a managed creature active,
- confirmation that no unmanaged native `Companion` prompt is left behind,
- confirmation that no actor auto-respawns,
- confirmation that any remembered companion is metadata/status only until the player explicitly summons again.
