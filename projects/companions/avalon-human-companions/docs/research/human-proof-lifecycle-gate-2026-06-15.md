# Human Proof Lifecycle Gate

Date: 2026-06-15

## Scope

Define the next hard gate after proof actor live capture.

This gate proves that the one-session human proof actor stays runtime-only and does not duplicate, persist, orphan commands, or survive save/load in an unsafe state.

It does not approve recruitment, persistent human followers, roster state, dialogue, equipment, inventory, leveling, quest behavior, or conversion of existing NPCs.

## Evidence read

- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/validation-plan.md`
- `mods/avalon-human-companions/docs/research/human-one-session-native-ally-proof-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-command-surface-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-proof-command-panel-2026-06-15.md`
- `mods/avalon-companions/docs/research/transition-save-lifecycle-gate-2026-06-15.md`

## Current proof state

The proof actor live capture gate passed with a logging caveat:

- dev FOA-Diagnostic Tool dump `20260615-193847` was a playable-scene dump,
- `heroAvailable=true`,
- `NpcHeroPetAlly=1`,
- `NpcHeroSummon=1`,
- Avalon Human Companions scanner captured the reviewed proof target GUID,
- target row had `markedNotSaved=true`, `hasNpcElement=true`, `hasAlive=true`, and `hasHeroPetAlly=true`,
- every dry-run command row stayed `blocked=true` and `liveAction=false`.

That proves the scanner can see the managed proof actor. It does not prove lifecycle cleanup.

## Decision

Before adding more human companion behavior, run a lifecycle/no-persistence gate on the same proof actor.

The gate must use a throwaway save and should rely on:

- Avalon Human Companions panel/scanner CSVs,
- dev FOA-Diagnostic Tool F3 dumps from `mods/template-diagnostics`,
- BepInEx log lines where available,
- in-game observation for visible actor state.

Version 0.1.7 adds a small lifecycle diagnostics/guard pass to support this gate. It keeps the active proof actor marked not saved, clears the tracked actor after Dismiss/discard, discards the actor if the managed proof marker is lost, and writes `human-proof-lifecycle.csv` rows when diagnostics are enabled.

## Required test route

1. Load a throwaway playable save.
2. Spawn the one-session human proof actor.
3. Confirm the actor is visible and captured by `Scan Actors`.
4. Use `Dismiss` from the proof panel.
5. Confirm the actor disappears and a new `Scan Actors` pass no longer includes the proof actor target row.
6. Reload a fresh throwaway save and spawn the proof actor again.
7. Cross a scene or area transition, or use a fast-travel/long-move route if available.
8. Press F3 and run `Scan Actors`.
9. Confirm there is no duplicate proof actor and no untracked proof actor.
10. Save the throwaway save, quit to menu or desktop, reload it, press F3, and run `Scan Actors`.
11. Confirm the proof actor does not persist as an unmanaged ally after reload.

## Pass criteria

- Dismiss removes the active proof actor from the scanner output.
- A transition or long move does not create duplicate proof actors.
- `human-actor-candidates.csv` never shows more than one row for the reviewed proof target GUID in the managed proof route.
- Any visible proof actor row remains `markedNotSaved=true`, `hasHeroPetAlly=true`, `behaviorApproved=false`, and `panelApproved=false`.
- After save/quit/reload, the proof actor is absent unless explicitly spawned again.
- Dev FOA-Diagnostic Tool dumps after reload do not show unexpected `NpcHeroPetAlly` or `NpcHeroSummon` leftovers for the proof actor.
- No log, CSV, or UI claims persistence, recruitment, existing-NPC conversion, dialogue, quest ownership, save safety, or release-ready UI.
- `human-proof-lifecycle.csv` rows keep `behaviorApproved=false` and `persistenceApproved=false`.

## Fail conditions

- The proof actor remains visible after Dismiss.
- Scanner output still includes the proof target after Dismiss.
- Transition or long move creates duplicate proof target rows.
- Save/quit/reload restores the proof actor without an explicit new proof spawn.
- A proof actor loses `markedNotSaved=true`.
- A command row changes to `blocked=false` or `liveAction=true`.
- Any unrelated NPC gains proof commands, `NpcHeroPetAlly`, or a human companion panel state.

## Boundary

Passing this gate approves only the current one-session proof lifecycle boundary.

It still does not approve persistent human companions, recruitment, existing NPC conversion, save-backed roster data, dialogue, inventory, equipment, leveling, quest behavior, or a release-ready human companion panel.
