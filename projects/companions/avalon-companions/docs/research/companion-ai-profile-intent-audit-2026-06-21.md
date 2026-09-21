# Research: Companion AI Profile and Intent Audit

Date: 2026-06-21
Scope: implement the first code-bearing design layer approved by the 0.1.34 AI boundary evidence review.
Question: Can Avalon classify active managed companion state without executing custom AI behavior?

## Evidence read

- `mods/avalon-companions/docs/research/ai-boundary-evidence-review-2026-06-21.md`
- `mods/avalon-companions/docs/research/custom-ai-boundary-map-2026-06-20.md`
- `mods/avalon-companions/docs/research/framework-contract-extraction-2026-06-18.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Plugin.cs`

## Decision

Avalon Companions 0.1.35 may add runtime-only framework contracts:

- `CompanionAiProfile`
- `CompanionAiIntent`

It may also add a default-off profile audit:

- `Diagnostics.WriteCompanionAiProfileAudit=false`
- `Diagnostics.CompanionAiProfileAuditSeconds=2`
- output file: `companion-ai-profile.csv`

The audit may classify active managed companion state as:

- `NoActiveCompanion`
- `IdleNativePatrol`
- `StayPosition`
- `FollowCatchUpCandidate`
- `DefendWaitingForThreat`
- `NativeCombatObserved`
- `MovementBlocked`
- `NoTargets`
- `EvidenceInsufficient`

## Implementation boundary

Allowed:

- Read existing managed companion state.
- Reuse the existing safe native-state reads from the AI boundary diagnostic.
- Classify observed native movement, target-count, attacker-count, and mode evidence into `CompanionAiIntent`.
- Append audit evidence only when explicitly enabled.
- Keep command execution in the existing native-safe companion command lane.

Not allowed:

- Custom AI execution.
- Direct target override.
- Native target recalculation.
- `TargetOverrideElement.GetTarget`.
- `NpcMovement.ChangeMainState(...)`.
- Attack buttons or target selector UI.
- Forced hostility or arbitrary nearby target scans.
- Taming, training, loyalty, healing, resurrection, squads, persistence, restoration, respawn, or Core-executed behavior.

## Validation needed

- Debug build.
- Release build and deploy if preparing the live game install.
- Confirm the new config entries appear under Diagnostics.
- In-game smoke on a throwaway save: enable `Diagnostics.WriteCompanionAiProfileAudit=true`, summon or swap to an active managed companion, and verify `companion-ai-profile.csv` rows classify active state while keeping `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
- Continue collecting evidence across peaceful, combat, stealth, interior, transition, rest, quit/reload, and return cases before any behavior system is considered.
