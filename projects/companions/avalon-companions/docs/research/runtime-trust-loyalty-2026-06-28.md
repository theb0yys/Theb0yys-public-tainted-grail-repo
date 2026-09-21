# Research: Runtime Trust and Loyalty Foundation

Date: 2026-06-28
Scope: define the first companion trust/loyalty feature slice after command policy, AI profile audit, profile-driven native assist, native dialogue, and lifecycle smoke passed in game.
Question: Can Avalon add a real companion progression foundation without taking save ownership, custom target selection, or movement control?

## Evidence read

- `mods/avalon-companions/docs/research/custom-ai-boundary-map-2026-06-20.md`
- `mods/avalon-companions/docs/research/ai-boundary-evidence-review-2026-06-21.md`
- `mods/avalon-companions/docs/research/companion-ai-profile-intent-audit-2026-06-21.md`
- `mods/avalon-companions/docs/research/profile-driven-native-assist-2026-06-21.md`
- `mods/avalon-companions/docs/research/lifecycle-validation-gate-2026-06-21.md`
- `mods/avalon-companions/src/Framework/CompanionCoreRuntimeProfile.cs`
- `mods/avalon-companions/src/Framework/CompanionCommandPolicy.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Plugin.cs`

## Decision

The next advanced feature slice may add a runtime-only `CompanionTrustProfile` attached to `CompanionCoreRuntimeProfile`.

Allowed:

- Track in-memory trust and loyalty scores for reviewed managed companions.
- Update scores from existing approved command/runtime events only: summon, recall, follow, stay, defend, range, recover, dismiss, lifecycle, and profile-driven/native-safe assist events.
- Show compact trust/loyalty status in the debug panel and companion dialogue subtitle.
- Write command-audit rows for trust/loyalty changes when command logging is enabled.
- Keep all trust/loyalty state runtime-only and discard it naturally with the plugin session.

Not allowed in this slice:

- Saved trust, saved loyalty, affinity persistence, reload restoration, actor re-adoption, or save ownership.
- Taming, training, feeding, perk trees, unlocks, morale penalties, recruitment, or companion inventory.
- Custom target selection, attack commands, target override elements, movement overrides, or forced hostility.
- Avalon Core-executed behavior.

## Implementation boundary

The trust runtime is a feature foundation, not a behavior override. It can describe companion bond state and produce audit evidence, but it cannot change native AI, movement, targeting, lifecycle ownership, persistence, or command availability yet.

Future gates may use this profile for taming/training or policy modifiers only after persistence/storage and behavior side effects are separately reviewed.

## Validation needed

- `git diff --check -- mods/avalon-companions`
- Debug build.
- Release build.
- In-game smoke: summon, open companion dialogue, run Follow/Stay/Recall/Come Close/Defend where available, Recover, Dismiss, then confirm panel/dialogue trust text appears and `companion-command-log.csv` contains `companion-trust` rows with `runtimeOnly=true`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
