# Research: AI Boundary Evidence Review and First Safe Behavior Design

Date: 2026-06-21
Scope: review the first live `companion-ai-boundary.csv` evidence and define the first safe behavior design boundary for Avalon Companions 0.1.34.
Question: Does the AI boundary diagnostic evidence clear custom AI behavior, or only a non-mutating design layer?

## Evidence read

- `mods/avalon-companions/docs/research/custom-ai-boundary-map-2026-06-20.md`
- `mods/avalon-companions/docs/research/post-0.1.29-development-sequence-2026-06-20.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/README.md`
- Live BepInEx config: `kane.tgfoa.avalon-companions.cfg`
- Live AI boundary diagnostic CSV: `companion-ai-boundary.csv`
- Live command audit CSV: `companion-command-log.csv`
- Live BepInEx log: `LogOutput.log`

## Live diagnostic evidence

Source file: `<local-path>`

- Rows reviewed: 276.
- Time range: 2026-06-21 00:15:51.346 through 2026-06-21 00:25:43.923.
- Last write observed: 2026-06-21 00:25:43.
- Reasons: `periodic=273`, `panel-ai-boundary-check=3`.
- Scene coverage: `CampaignMap_HOS=276`.
- Mode coverage: `Stay=188`, `Defend=88`.
- Follow range coverage: `Normal=276`.
- Companion coverage:
  - `Wolf Candidate=167`
  - `Sharg Candidate=103`
  - `Bullrat Candidate=1`
  - `Corpse Eater Candidate=1`
  - `Cow Candidate=1`
  - `Grindylow Candidate=1`
  - `Pig Candidate=1`
  - `Redcap Candidate=1`
- Movement-state coverage:
  - `Patrol=219`
  - `NoMoveAndRotateTowardsTarget=26`
  - `KeepPosition=16`
  - `NoMove=13`
  - `SnapToPositionAndRotate=2`
- Combat and target-state coverage:
  - `npcInCombat=false=233`, `npcInCombat=true=43`
  - `heroLiveAttackers=0=212`, `heroLiveAttackers=1=64`
  - `possibleTargetCount=0=233`, `possibleTargetCount=1=43`
  - `possibleAttackerCount=0=231`, `possibleAttackerCount=1=45`
  - `hasTargetOverride=false=276`
  - `hasHeroSummonTargetOverride=false=276`
- Native movement blocker coverage:
  - `canMove=true=267`, `canMove=false=9`
  - `canOverrideDestination=false=248`, `canOverrideDestination=true=28`
- Safety flags:
  - `touchesCommands=false` for all reviewed rows.
  - `touchesMovement=false` for all reviewed rows.
  - `touchesTargeting=false` for all reviewed rows.
  - `touchesPersistence=false` for all reviewed rows.
  - `coreExecuted=false` for all reviewed rows.
  - Computed `UNSAFE_FLAGS=0`.

Command audit crosscheck:

- `companion-command-log.csv` had 288 rows at review time.
- `ai-boundary-check` rows: 13.
- `summon/swap` rows: 36.
- `dismiss` rows: 27.
- The latest combat-adjacent command rows showed Defend mode using the existing native ally path with `defend-armed`, `defend-prompt`, `defend-clear`, and one `auto-catch-up` row. These rows kept `touchesTargeting=false` and `touchesPersistence=false`.

## What this evidence proves

- The AI boundary diagnostic is writing live runtime evidence while preserving the intended read-only lane.
- The diagnostic can observe active managed companions across multiple roster entries.
- The diagnostic can observe native combat-adjacent state through the current `NpcHeroPetAlly` path: live hero attackers, possible target/attacker relation counts, `npcInCombat`, and movement-state changes.
- The diagnostic can observe native movement blocker state without mutating movement.
- The current native-safe Defend path continues to produce audit rows without direct Avalon target selection or persistence.

## What this evidence does not prove

- It does not prove direct `TargetOverrideElement` or `HeroSummonTargetOverride` usage is safe.
- It does not prove direct `NpcMovement.ChangeMainState(...)` usage is safe.
- It does not prove arbitrary target scans, attack buttons, or player-selected targets are safe.
- It does not prove actor persistence, reload restoration, saved ownership, or re-adoption is safe.
- It does not prove taming, training, loyalty, healing, resurrection, or active squads are safe.
- It does not prove behavior in every required lifecycle state. The reviewed CSV contains only `CampaignMap_HOS`; transition, rest, quit/reload, return, interior, and stealth evidence still need explicit review before persistence or advanced AI claims.

## First safe behavior design

0.1.34 may define a non-mutating companion behavior planning layer. It may not execute behavior.

Allowed design:

- `CompanionAiProfile`: a runtime-only description of the active managed companion's observed native state.
- `CompanionAiIntent`: a runtime-only classification of what Avalon would consider doing if a later gate approves behavior.
- `CompanionAiIntent` values may include:
  - `NoActiveCompanion`
  - `IdleNativePatrol`
  - `StayPosition`
  - `FollowCatchUpCandidate`
  - `DefendWaitingForThreat`
  - `NativeCombatObserved`
  - `MovementBlocked`
  - `NoTargets`
  - `EvidenceInsufficient`
- Audit output may record profile and intent decisions as evidence only.

The design layer must:

- read existing native state,
- classify observed state,
- write audit evidence only when explicitly enabled,
- keep command execution in the existing native-safe lane,
- avoid progression state,
- avoid persistence,
- avoid Avalon Core execution.

The design layer must not:

- call `TargetOverrideElement.GetTarget`,
- add a `TargetOverrideElement` or `HeroSummonTargetOverride`,
- call native target recalculation,
- call `NpcMovement.ChangeMainState(...)`,
- force hostility,
- scan arbitrary nearby enemies as selected targets,
- add attack buttons or target selectors,
- move actors outside existing recall/catch-up commands,
- store taming/training/loyalty/progression state,
- restore or persist actors,
- execute behavior through Avalon Core.

## Decision

Proceed with 0.1.34 as an evidence-review and behavior-design slice only.

The next code-bearing slice may add a runtime-only profile/intent classifier and optional audit rows, but it must remain non-mutating. Custom AI execution, movement overrides, target overrides, attack commands, taming, training, loyalty, persistence, and Core-executed behavior remain blocked.

## Validation needed

- Build if version metadata changes.
- If a later classifier is implemented, validate that it only writes profile/intent audit rows and does not change companion behavior.
- Continue runtime evidence collection in:
  - transition,
  - rest,
  - quit/reload,
  - return,
  - interior,
  - stealth or non-combat hostile proximity.
- Confirm future rows still keep `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false` for diagnostic/profile-only paths.
