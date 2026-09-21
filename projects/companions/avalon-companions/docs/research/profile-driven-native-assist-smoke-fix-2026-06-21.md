# Research: Profile-Driven Native Assist Smoke Fix

Date: 2026-06-21
Scope: fix the 0.1.39 in-game smoke failure where legacy native-safe ticks claimed the action before profile assist could log it.
Question: How do we make the enabled profile-driven bridge produce the expected audit rows without adding new behavior?

## Evidence read

- `mods/avalon-companions/docs/research/profile-driven-native-assist-2026-06-21.md`
- `mods/avalon-companions/docs/research/profile-driven-native-assist-hardening-2026-06-21.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- Live `companion-command-log.csv` rows after baseline 315 from the 2026-06-21 smoke.
- Live `companion-ai-profile.csv` rows around 2026-06-21 14:30:28 through 14:30:34.

## Finding

The 0.1.39 smoke loaded correctly with `Companions.EnableProfileDrivenNativeAssist=true`, but the command log did not produce `ai-profile-native-defend`.

Observed sequence:

- A regular `defend-prompt` row fired at 2026-06-21 14:30:30 while attackers appeared.
- The profile audit classified the same active managed companion as `NativeCombatObserved` at 2026-06-21 14:30:32.
- The profile assist path then found the existing defend prompt cooldown already claimed by the regular tick, so it did not write an `ai-profile-native-defend` row.

This means 0.1.39's "defer after profile action" hardening is insufficient when the older tick claims the first native action before the profile loop sees the evidence.

## Decision

Version 0.1.40 keeps the same native-safe behavior lane, but when `Companions.EnableProfileDrivenNativeAssist=true`, the profile loop owns the existing catch-up and native defend prompting paths. The older follow and defend ticks return early only while that gate is enabled.

Allowed:

- Reuse the existing profile classifier.
- Reuse the existing catch-up recall path for `FollowCatchUpCandidate`.
- Reuse the existing `NpcHeroPetAlly.EnterCombat()` path for `NativeCombatObserved`.
- Preserve the existing default-off gate.
- Continue writing `ai-profile-native-catch-up` and `ai-profile-native-defend` command rows.

Not allowed:

- New target selection.
- Target override elements.
- Native target recalculation.
- Direct movement-state overrides.
- Attack UI.
- Persistence or progression state.
- Core-executed behavior.
- Taming, training, or loyalty.

## Validation needed

- Debug build.
- Release build and deploy.
- BepInEx load validation for `Avalon Companions 0.1.40`.
- In-game smoke with `Companions.EnableProfileDrivenNativeAssist=true`.
- Confirm a follow-distance breach produces `ai-profile-native-catch-up`.
- Confirm live hero attackers produce `ai-profile-native-defend`.
- Confirm no immediate duplicate `auto-catch-up` or `defend-prompt` rows appear next to the profile action rows.
