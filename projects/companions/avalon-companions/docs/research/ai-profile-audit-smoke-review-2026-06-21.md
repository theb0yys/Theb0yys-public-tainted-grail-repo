# Research: AI Profile Audit Smoke Review

Date: 2026-06-21
Scope: validate the default-off `CompanionAiProfile` / `CompanionAiIntent` audit path added in 0.1.35.
Question: Does the profile audit write live evidence for active managed companions while preserving the non-mutating boundary?

## Evidence read

- `mods/avalon-companions/docs/research/companion-ai-profile-intent-audit-2026-06-21.md`
- `mods/avalon-companions/docs/validation-plan.md`
- Live BepInEx config: `kane.tgfoa.avalon-companions.cfg`
- Live BepInEx log: `LogOutput.log`
- Live profile audit CSV: `companion-ai-profile.csv`
- Live command audit CSV: `companion-command-log.csv`

## Smoke setup

- Deployed DLL checked before launch:
  - File version: `0.1.35.0`
  - SHA-256: `A67329CC836E85776A95750C2B95C4AE68A506E61714DBD82B2A4461ED176EA0`
- Enabled live diagnostic config:
  - `Diagnostics.WriteCompanionAiProfileAudit=true`
  - `Diagnostics.CompanionAiProfileAuditSeconds=2`
  - `Diagnostics.WriteCompanionAiBoundaryDiagnostics=true`
  - `Diagnostics.CompanionAiBoundaryDiagnosticSeconds=2`
- BepInEx load validated after launch:
  - `Loading [Avalon Companions 0.1.35]`
  - `Avalon Companions 0.1.35 loaded`
  - load line included `WriteCompanionAiProfileAudit=True; CompanionAiProfileAuditSeconds=2`

## Profile audit evidence

Source file: `<local-path>`

- Rows reviewed: 27.
- Time range: 2026-06-21 02:01:53.091 through 2026-06-21 02:02:49.611.
- Scene coverage: `CampaignMap_HOS=27`.
- Active managed companion rows: 14.
- Active companion coverage: `Sharg Candidate=14`.
- Intent coverage:
  - `NoActiveCompanion=13`
  - `MovementBlocked=5`
  - `IdleNativePatrol=9`
- Mode coverage:
  - `Defend=16`
  - `Follow=11`
- Follow range coverage:
  - `Normal=16`
  - `Close=11`
- Active movement-state coverage:
  - `Patrol=13`
  - `SnapToPositionAndRotate=1`
- Active native movement blocker coverage:
  - `canMove=false=5`
  - `canMove=true=9`
- Combat and target-state coverage:
  - `heroLiveAttackers=0=27`
  - active `hasTargetOverride=false=14`
  - active `hasHeroSummonTargetOverride=false=14`
- Safety flags:
  - `touchesCommands=false` for all reviewed rows.
  - `touchesMovement=false` for all reviewed rows.
  - `touchesTargeting=false` for all reviewed rows.
  - `touchesPersistence=false` for all reviewed rows.
  - `coreExecuted=false` for all reviewed rows.
  - Computed `UNSAFE_FLAGS=0`.

Command audit crosscheck:

- `companion-command-log.csv` had 307 rows at review time.
- Latest smoke rows included:
  - `continuity-status` before active actor creation.
  - `summon/swap` at 2026-06-21 02:02:22.037 with `spawned one-session native pet ally candidate`.
  - `recover`, `summon-existing-recall`, `range-close`, `heel`, and one `ai-boundary-check`.
- Latest relevant command rows kept `touchesTargeting=false` and `touchesPersistence=false`.

## What this evidence proves

- The 0.1.35 build loads in BepInEx with the profile audit config active.
- `companion-ai-profile.csv` is created under the expected plugin-owned config folder.
- The audit records `NoActiveCompanion` rows before an active managed companion exists.
- After summon/swap, the audit records active managed companion rows for `Sharg Candidate`.
- The classifier observed native movement-blocked state during placement/initial movement and native idle patrol state after `canMove=true`.
- The audit rows preserve the intended evidence-only boundary: no command, movement, target, persistence, or Core execution flags were raised.

## What this evidence does not prove

- It does not cover `StayPosition`.
- It does not cover `FollowCatchUpCandidate`.
- It does not cover `DefendWaitingForThreat` after movement is unblocked.
- It does not cover `NativeCombatObserved`.
- It does not cover `NoTargets` as the selected outcome because `IdleNativePatrol` correctly wins for native patrol rows with no target evidence.
- It does not cover interiors, stealth, transition, rest, quit/reload, or return.
- It does not approve custom AI execution, target overrides, movement overrides, attack commands, taming, training, loyalty, persistence, or Core-executed behavior.

## Decision

0.1.36 may record the profile audit smoke as passed for BepInEx load, config activation, CSV creation, active managed companion rows, `MovementBlocked`, `IdleNativePatrol`, `NoActiveCompanion`, and zero unsafe flags.

The result is still evidence-only. Continue collecting profile and boundary rows across missing states before designing behavior.

## Validation needed next

- Repeat profile audit in Stay mode.
- Repeat profile audit with a follow catch-up distance breach.
- Repeat profile audit in Defend mode after native movement is unblocked and no attackers exist.
- Repeat profile audit while the hero has live attackers.
- Repeat profile audit across interior, stealth, transition, rest, quit/reload, and return cases.
