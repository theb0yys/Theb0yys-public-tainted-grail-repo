# Research: AI Profile Check Smoke Aid

Date: 2026-06-21
Scope: add a manual debug-panel smoke aid for the default-off `CompanionAiProfile` / `CompanionAiIntent` audit path.
Question: Can the remaining profile coverage be easier to collect without adding behavior?

## Evidence read

- `mods/avalon-companions/docs/research/ai-profile-audit-smoke-review-2026-06-21.md`
- `mods/avalon-companions/docs/research/companion-ai-profile-intent-audit-2026-06-21.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Plugin.cs`

## Decision

Version 0.1.37 may add a visible debug-panel `AI Profile Check` button.

The button may:

- Stay visible in the IMGUI debug panel beside the existing diagnostic controls.
- Report a blocked status and write a blocked `ai-profile-check` command-log row when `Diagnostics.WriteCompanionAiProfileAudit=false`.
- When `Diagnostics.WriteCompanionAiProfileAudit=true`, call the existing profile-audit CSV writer once with reason `panel-ai-profile-check`.
- Write one `NoActiveCompanion` row when no active managed companion exists, because that is a valid classifier outcome.
- Write one row per active managed companion when active managed companions exist.
- Add an `ai-profile-check` command-log row that states the snapshot is read-only.

## Implementation boundary

Allowed:

- Reuse the existing `CollectAiBoundaryDiagnosticLocations` read path.
- Reuse the existing `WriteCompanionAiProfileSnapshot` CSV writer.
- Reuse the existing command-log audit path for blocked/pass status.
- Show concise status text in the existing debug-panel status line.

Not allowed:

- Dispatch companion commands.
- Queue native follow or defend refresh.
- Change movement or target state.
- Call native target recalculation.
- Call `TargetOverrideElement.GetTarget`.
- Store progression state.
- Persist actors.
- Execute behavior through Avalon Core.
- Add custom AI, taming, training, loyalty, attack commands, target selectors, or actor restoration.

## Validation needed

- Debug build.
- Release build.
- In-game smoke with `Diagnostics.WriteCompanionAiProfileAudit=false`: click `AI Profile Check`, confirm blocked status and a blocked `ai-profile-check` row.
- In-game smoke with `Diagnostics.WriteCompanionAiProfileAudit=true` and no active managed companion: click `AI Profile Check`, confirm a `panel-ai-profile-check` `NoActiveCompanion` row.
- In-game smoke with an active managed companion: click `AI Profile Check` in Follow, Stay, Defend, catch-up, and native-combat-adjacent cases, then review `companion-ai-profile.csv` for the missing intent coverage.

## Decision result

This is still an evidence collection aid only. It does not approve custom AI execution or progression systems.
