# Research: Profile-Driven Native Assist Hardening

Date: 2026-06-21
Scope: harden the gated 0.1.38 profile-driven native assist loop before in-game smoke.
Question: How do we prevent the profile assist path from immediately duplicating the existing follow/defend ticks?

## Evidence read

- `mods/avalon-companions/docs/research/profile-driven-native-assist-2026-06-21.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

Version 0.1.39 may de-duplicate the profile assist bridge against the older native-safe ticks.

Allowed:

- After profile assist performs `FollowCatchUpCandidate` recall/catch-up, defer the next regular creature follow tick by the existing follow tick interval.
- After profile assist prompts native defend through `NpcHeroPetAlly.EnterCombat()`, defer the next regular creature defend tick by the existing defend tick interval.
- Record this in command-log reasons as sibling tick deferral.

Not allowed:

- New targeting.
- New movement-state overrides.
- Target override elements.
- Attack UI.
- Persistence or progression state.
- Core-executed behavior.

## Validation needed

- Debug build.
- Release build and deploy.
- BepInEx load validation.
- In-game smoke with `Companions.EnableProfileDrivenNativeAssist=true`.
- Confirm `ai-profile-native-catch-up` and `ai-profile-native-defend` do not immediately pair with duplicate `auto-catch-up` or `defend-prompt` rows in the same moment.
