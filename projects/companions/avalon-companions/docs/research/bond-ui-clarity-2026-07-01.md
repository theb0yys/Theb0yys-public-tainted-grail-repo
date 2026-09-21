# Research: Bond UI Clarity

Date: 2026-07-01
Scope: make the existing runtime-only trust/loyalty and bond policy readable in the supported companion dialogue and roster panel.
Question: Can Avalon clarify bond effects without adding new progression mechanics or persistence?

## Evidence read

- `mods/avalon-companions/docs/research/runtime-trust-loyalty-2026-06-28.md`
- `mods/avalon-companions/docs/research/runtime-bond-policy-2026-06-28.md`
- `mods/avalon-companions/docs/research/dialogue-icon-background-polish-2026-06-28.md`
- `mods/avalon-companions/README.md`
- `mods/avalon-companions/src/Framework/CompanionTrustProfile.cs`
- `mods/avalon-companions/src/Framework/CompanionBondPolicy.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`

## Decision

The next UI slice may clarify the already-existing runtime trust, loyalty, and bond policy state.

Allowed:

- Show effective bond as the lower of trust level and loyalty level.
- Show trust and loyalty scores compactly.
- Show a short, player-readable effect phrase based on the existing `CompanionBondPolicyDecision`.
- Keep the debug/roster panel's detailed timing multipliers for validation.

Not allowed:

- New trust events.
- New command behavior.
- New buffs, stats, native effects, target overrides, movement overrides, persistence, taming, training, saved progression, or Core-executed behavior.

## Validation needed

- `git diff --check -- mods/avalon-companions`
- Debug build.
- Release build.
- Live DLL deploy and hash check.
- In-game smoke: summon a managed companion, open `Companion`, confirm the dialogue subtitle shows bond/effect text without clipping, open `KeypadPeriod`, confirm the panel shows summary and detailed policy lines, and confirm commands still click.
