# Research: Runtime Bond Policy

Date: 2026-06-28
Scope: define the first behavior-bearing trust/loyalty policy after the runtime-only `CompanionTrustProfile` foundation.
Question: Which bond buffs/debuffs can run now without native stat effects, target overrides, movement overrides, persistence, or Core-executed behavior?

## Evidence read

- `mods/avalon-companions/docs/research/runtime-trust-loyalty-2026-06-28.md`
- `mods/avalon-companions/docs/research/custom-ai-boundary-map-2026-06-20.md`
- `mods/avalon-companions/src/Framework/CompanionTrustProfile.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Plugin.cs`

## Decision

Version 0.2.4 may add a `CompanionBondPolicy` that reads the runtime trust/loyalty profile and returns modifiers for Avalon-owned command timing only.

Allowed modifiers:

- catch-up tick interval multiplier,
- catch-up distance threshold multiplier,
- native defend prompt cooldown multiplier,
- automatic native defend assist allowed/blocked outside explicit Defend mode,
- trust gain and penalty multipliers.

Not allowed:

- player or companion stat effects,
- native status/buff records,
- direct target selection,
- target override elements,
- native movement state overrides,
- forced hostility,
- persistence, saved loyalty, reload restoration, actor re-adoption,
- taming, training, perk trees, or Avalon Core-executed behavior.

## Policy shape

- `Wary`: slower catch-up, longer catch-up distance threshold, longer defend cooldown, automatic defend assist blocked unless the player explicitly orders Defend, faster gains, stronger penalties.
- `Familiar`: neutral baseline.
- `Trusted`: faster catch-up, shorter catch-up threshold, shorter defend cooldown, normal automatic defend assist, slightly slower gains, softer penalties.
- `Loyal`: fastest catch-up, shortest catch-up threshold, shortest defend cooldown, normal automatic defend assist, slow gains, much softer penalties.

The effective bond level is the lower of trust level and loyalty level. This keeps the policy conservative when one score outruns the other.

## Validation needed

- `git diff --check -- mods/avalon-companions`
- Debug build.
- Release build.
- In-game smoke: compare a baseline companion and a high-bond companion, then confirm command-log reasons include bond policy data and no rows report targeting, persistence, or Core execution.
