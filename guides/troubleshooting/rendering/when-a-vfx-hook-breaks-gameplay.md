# When a VFX Hook Breaks Gameplay

Use this when a presentation mod causes unexpected hit, damage, collision or resource-interaction regressions.

Working lesson: [Wrong Lifecycle Seam: Target Resolution Regression](../../../research/case-studies/vfx/target-resolution-regression.md).

## Symptom

A VFX/blood/effect mod appears presentation-only, but users report gameplay regressions such as incorrect damage or stone/resource hits.

## Root problem

The mod hooked an **earlier gameplay owner** merely because it exposed useful data.

In the Tainted Blood case, `Damage.DetermineTargetHit` was used for extra corpse-hit blood. Research then established it as target resolution **before damage**, making it the wrong owner for a presentation sidecar.

## Corrected model

Use accepted lifecycle outcomes:

```text
living character effect
→ HealthElement.OnDamage

terminal character effect
→ HealthElement.OnDeathEvents
```

Leave target resolution native.

## Diagnostic sequence

1. list every Harmony target used by the presentation mod;
2. classify whether each target owns gameplay decision or post-decision presentation;
3. temporarily remove the earliest/highest-authority hook;
4. reproduce the regression;
5. move presentation onto the latest native lifecycle event that still contains the data you need.

## Rule

> Access convenience is not ownership.

A method is not safe for VFX merely because it exposes target/hit information.

## Verify the correction

Confirm:

- native target selection works;
- damage behaviour returns to baseline;
- resource/stone hits work;
- living-character custom VFX still triggers;
- terminal custom VFX triggers once;
- unsupported corpse-hit behaviour remains unsupported rather than reintroducing the unsafe hook.

## Evidence boundary

The user-reported regression persisted past asset rollback and was corrected by removing the target-resolution patch from the stable route. Repeated corpse-hit support remains outside the proven stable path.
