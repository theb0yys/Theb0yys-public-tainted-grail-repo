# Research: Profile-Driven Native Assist

Date: 2026-06-21
Scope: add the first profile-driven behavior bridge without adding custom targeting, pathing, persistence, or progression.
Question: What actual logic can use `CompanionAiIntent` while staying inside the already-approved native-safe lane?

## Evidence read

- `mods/avalon-companions/docs/research/custom-ai-boundary-map-2026-06-20.md`
- `mods/avalon-companions/docs/research/ai-boundary-evidence-review-2026-06-21.md`
- `mods/avalon-companions/docs/research/companion-ai-profile-intent-audit-2026-06-21.md`
- `mods/avalon-companions/docs/research/responsive-native-behavior-polish-2026-06-18.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Plugin.cs`

## Decision

Version 0.1.38 may add a gated profile-driven native assist executor.

Config:

- `Companions.EnableProfileDrivenNativeAssist=false`
- `Companions.ProfileDrivenNativeAssistSeconds=1`

Allowed behavior:

- Evaluate the existing `CompanionAiProfile` / `CompanionAiIntent` classifier in memory.
- For `FollowCatchUpCandidate`, call the existing recall/catch-up placement path.
- For `NativeCombatObserved`, call the existing `NpcHeroPetAlly.EnterCombat()` path only when the hero already has live attackers and either Defend mode or the existing native defend assist gate allows it.
- Log behavior rows to `companion-command-log.csv` as `ai-profile-native-catch-up` and `ai-profile-native-defend`.

Still blocked:

- Direct target selection.
- Target override elements.
- Native target recalculation.
- Direct movement-state overrides.
- Attack buttons or target selectors.
- Taming, training, loyalty, healing, resurrection, squads, persistence, reload restoration, re-adoption, or Core-executed behavior.

## Validation needed

- Debug build.
- Release build.
- In-game smoke with `Companions.EnableProfileDrivenNativeAssist=true`.
- Confirm a follow-distance breach produces `ai-profile-native-catch-up`.
- Confirm live hero attackers produce `ai-profile-native-defend` only through the existing native ally path.
- Confirm command rows keep `touchesTargeting=false` and `touchesPersistence=false`.
- Confirm no new UI attack command, target selector, persistence state, or Core callback appears.
