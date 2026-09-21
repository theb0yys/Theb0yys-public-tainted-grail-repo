# Research: Lifecycle Validation Gate

Date: 2026-06-21
Scope: define the 0.1.42 throwaway-save lifecycle validation gate for existing managed one-session companions.

## Evidence read

- `mods/avalon-companions/docs/research/transition-save-lifecycle-gate-2026-06-15.md`
- `mods/avalon-companions/docs/research/lifecycle-validation-checkpoint-2026-06-15.md`
- `mods/avalon-companions/docs/research/session-continuity-persistence-gate-2026-06-16.md`
- `mods/avalon-companions/docs/research/session-continuity-evidence-review-2026-06-16.md`
- `mods/avalon-companions/docs/research/native-dialogue-decision-gate-2026-06-21.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/src/Plugin.cs`

## Current state

- The lifecycle guard already marks managed one-session companions not saved.
- The guard can remove untracked one-session roster allies and excess active roster actors when `Companions.EnableLifecycleSafetyGuard=true`.
- The panel `Lifecycle Check` button already forces a lifecycle pass and writes lifecycle plus command evidence.
- Prior evidence reviewed scene-change, long-hero-move, duplicate cleanup, recovery, and shutdown rows.
- Prior evidence did not complete rest, save, quit, reload, return, no-orphan-prompt, and no-auto-respawn validation with a managed creature active.

## 0.1.42 gate

Avalon Companions 0.1.42 is a validation gate only. It may document the required smoke route, bump version metadata, and record build output. It must not change companion lifecycle behavior.

The throwaway-save validation route is:

1. Start with `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, `Companions.EnableLifecycleSafetyGuard=true`, `Diagnostics.WriteCompanionLifecycleDump=true`, and `Diagnostics.WriteCompanionCommandLog=true`.
2. Summon one reviewed managed one-session creature candidate.
3. Press `Lifecycle Check` and confirm a `panel-lifecycle-check` lifecycle row plus `lifecycle-check` command row.
4. Transition or fast travel with the companion active, then press `Lifecycle Check` again.
5. Rest with the companion active, then press `Lifecycle Check`.
6. Save, quit, reload, and return to the throwaway save.
7. Press `Lifecycle Check` after reload if the panel and runtime are available.
8. Inspect `companion-lifecycle.csv` and `companion-command-log.csv`.

## Pass criteria

- Managed companion rows keep `markedNotSaved=true`.
- Active roster count is zero or one after each validation point.
- Duplicate cleanup, if triggered, leaves no more than one active managed roster companion.
- Untracked one-session roster ally cleanup does not repeat indefinitely.
- No unmanaged native `Companion` prompt is left behind after transition, rest, or reload.
- Quit/reload does not auto-restore, auto-respawn, persist, or re-adopt the one-session actor.
- Command rows keep `touchesPersistence=false` and `touchesTargeting=false`.
- If the companion is gone after reload because it was not saved, that is a passing result for this gate.

## Boundary

This gate does not approve:

- actor persistence,
- reload restoration,
- automatic respawn,
- same-session actor re-adoption after reload,
- changing `MarkedNotSaved` ownership,
- saved native command actions,
- active squads,
- healing, resurrection, or recovery behavior changes,
- custom AI, target selection, attack UI, taming, training, loyalty, or Core-executed behavior.

## Validation needed

- Debug build.
- Local Release build.
- `git diff --check -- mods/avalon-companions`.
- In-game throwaway-save lifecycle smoke using the route above.
- CSV review after the smoke run.
