# Tune Poise Safely

**Evidence status: PARTIAL.** Static/decompiled evidence establishes the semantic distinction between poise accumulation and stamina-driven stagger; runtime proof for a generic public tuning recipe is not recorded here.

Working lineage: [Poise Is Not Stagger](../../../research/case-studies/combat/poise-not-stagger.md).

## Key correction

Do not assume a field named `PoiseThreshold` is simply a configuration scalar you can multiply.

Research showed that poise accumulation/poise break and stamina-driven stagger are separate systems.

## Safer process

1. Trace where incoming poise damage is processed.
2. Identify the owner that adds/scales poise damage.
3. Change the incoming poise contribution rather than treating the runtime limited stat like a simple threshold setting.
4. Leave stamina-driven stagger untouched unless that is your actual feature.
5. Test one enemy/profile first.

Conceptually:

```text
native hit
→ native poise contribution
→ bounded mod scale
→ native poise accumulation
→ native poise-break behaviour
```

## Verification

Check independently:

- ordinary hit behaviour;
- poise accumulation rate;
- poise-break event;
- stamina/stagger behaviour remains unchanged;
- disable/reload returns to baseline.

## Current proof boundary

The system distinction and chosen seam are static/source-backed. Runtime validation for the exact public implementation still has to be performed before calling a specific tuning profile proven.
