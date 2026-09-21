# Human Proof Roster Panel Scale

Date: 2026-06-21

## Scope

Adjust the debug panel size after screenshot evidence showed the registered proof roster working but cramped on a 3840-wide viewport.

## Evidence read

- User screenshot `Screenshot (3871).png`.
- `mods/avalon-human-companions/docs/research/human-proof-candidate-roster-2026-06-21.md`
- `mods/avalon-human-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

Version 0.1.17 keeps the compact Avalon Companions-style panel layout but changes panel sizing from a fixed `900x620` to a responsive size that keeps the same minimum and grows on large displays:

- width: max fixed width or 42% of the viewport,
- height: max fixed height or 38% of the viewport,
- still clamped to screen margins.

This is visual/layout polish only. It does not change proof gates, roster entries, candidate approval, spawn behavior, command behavior, recruitment, persistence, existing-NPC conversion, or save handling.

## Screenshot diagnosis

The screenshot also showed `Gate: research mode on`, so `Spawn / Swap` and live proof commands were correctly disabled by config. Enabling live proof behavior still requires the existing throwaway-save proof gates; this panel-size change does not bypass them.
