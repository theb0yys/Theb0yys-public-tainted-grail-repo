# Research: Companion Taming Eligibility Gate

Date: 2026-07-02
Scope: add read-only taming profile definitions before any taming behavior.
Question: Can Avalon classify reviewed companion roster identities for future taming without capturing, adopting, restoring, or persisting actors?

## Evidence read

- `docs/ecosystem/vanilla-mechanics-gap-register.md`
- `docs/ecosystem/missing-game-aspects-roadmap.md`
- `mods/avalon-companions/docs/research/profile-progression-store-2026-07-02.md`
- `mods/avalon-companions/docs/research/progression-persistence-gate-2026-07-01.md`
- `mods/avalon-companions/README.md`
- `mods/avalon-companions/src/Framework/CompanionCoreRuntimeProfile.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

0.2.11 may add read-only taming definitions:

- `CompanionTamingProfile`
- `CompanionTamingEligibility`

Allowed eligibility classes:

- tameable candidate,
- already-companion,
- unsafe hostile,
- undead/blocked,
- passive-only,
- evidence insufficient.

The classifier may use only reviewed roster identity and existing runtime profile role/temperament classification. It must not inspect nearby wild actors as adoption candidates and must not attach commands.

## Initial roster classification policy

- Existing native pet roster entries are `already-companion`.
- Reviewed direct animal fantasy entries such as wolf and non-interaction bear are `tameable candidate`.
- Passive animal entries such as deer, pig, cow, and bear interaction are `passive-only`.
- Offensive monster entries are `unsafe hostile`.
- Undead entries are `undead/blocked`.
- Anything outside those evidence buckets is `evidence insufficient`.

## 0.2.11 implementation

Add a `Taming Gate` debug-panel button that writes one `taming-eligibility-gate` row to `companion-command-log.csv`.

The row records:

- selected reviewed roster identity,
- selected taming eligibility,
- selected runtime combat role,
- selected runtime temperament,
- roster classification counts,
- `noCapture=true`,
- `noWildActorAdoption=true`,
- `noActorPersistence=true`,
- `noActorRestore=true`,
- `noAutoRespawn=true`,
- `noCommandBehaviorChange=true`,
- `noSaveOwnership=true`,
- `noLocationHealthAiTargetState=true`,
- `touchesCommands=false`,
- `touchesMovement=false`,
- `touchesTargeting=false`,
- `touchesPersistence=false`,
- `coreExecuted=false`.

## Not approved

- Capture or tame command.
- Wild actor adoption.
- Changing live actor faction, AI, movement, target, health, inventory, or persistence.
- Saving actor identity, location, scene, health, target, native action state, or AI state.
- Auto-respawn, reload restoration, or actor re-adoption.
- Training, feeding, perk trees, unlocks, inventory, custom AI, target overrides, movement overrides, or Avalon Core-executed behavior.

## Validation needed

- `git diff --check -- mods/avalon-companions`
- Debug build.
- Release build.
- In-game smoke: open `KeypadPeriod`, click `Taming Gate`, confirm a `taming-eligibility-gate` row appears in `companion-command-log.csv`, and verify the row keeps every mutation flag false.
