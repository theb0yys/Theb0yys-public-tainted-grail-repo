# Taming Encounter Evidence Probe - 2026-07-09

## Scope

Before adding any tame command, Avalon needs live actor evidence for reviewed wild wolf and bear actors. The blocker is not roster identity anymore; it is how live actors expose template identity, hostility, faction/ownership hints, native AI state, target state, movement state, and save behavior in the world.

## Research Read

- `taming-eligibility-gate-2026-07-02.md`: approves only read-only `CompanionTamingProfile` / `CompanionTamingEligibility` classification over reviewed roster identity.
- `wolf-bear-diagnostic-review-2026-06-14.csv`: identifies `Spec_AnimalWolf` and `Spec_AnimalBear` as reviewed regular non-abstract non-unique candidates with FOA Diagnostic Tool spawner evidence.
- `wolf-bear-native-defend-assist-2026-06-15.md`: approves native defend assist only for plugin-owned one-session candidates that already passed ally setup.
- Existing `ActorControlDiagnostics` live-world scanner patterns: read live locations/components and write evidence rows without dispatching commands or mutating actors.

## Decision

0.2.12 may add a manual read-only `Encounter Probe` button in the debug panel. The probe writes `companion-taming-encounter.csv` under this plugin's BepInEx config folder and writes a `taming-encounter-probe` command-log row.

The probe may record:

- reviewed template identity and review ID,
- display/debug/location identity,
- distance from hero and active scene,
- current domain and `MarkedNotSaved` state,
- whether the actor is already an Avalon managed/tracked creature,
- component presence for pet, NPC, native ally, and command action markers,
- native NPC combat/alert/idle/flee/visibility/perception state,
- current target identity/type/distance when readable without target recalculation,
- possible target/attacker counts and whether the hero appears in those read-only collections,
- movement-state and movement-blocker fields,
- target override presence without calling target recalculation,
- reflection-only faction and ownership hints.

## Explicit Non-Goals

The probe does not approve or perform capture, taming, wild actor adoption, actor restoration, actor persistence, auto-respawn, same-session re-adoption, faction changes, ownership changes, native ally attachment, target recalculation, movement changes, command behavior changes, spawn/dismiss behavior, save ownership, custom AI, training, or Core-executed behavior.

## Validation Gate

- Build Debug and Release.
- Open a throwaway save near wild wolf/bear encounters.
- Open `KeypadPeriod`, press `Encounter Probe`, and review `companion-taming-encounter.csv`.
- Confirm mutation flags remain `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
- Use the evidence to decide whether a later tame command can safely identify and gate a wild actor.
