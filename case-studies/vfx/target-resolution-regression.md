---
document_type: case
scope: Tainted Blood target-resolution correction
evidence:
  project_history: USER_REPORTED_REGRESSION_PLUS_SOURCE_CORRECTION
last_verified: 2026-09-20
---

# Wrong Lifecycle Seam: Target Resolution Regression

Tainted Blood previously patched `Damage.DetermineTargetHit` to support extra corpse-hit blood.

A user-reported damage/stone-hit regression persisted even after asset rollback, which pointed away from the asset layer and back toward the hook.

Research then classified `DetermineTargetHit` correctly as target resolution before damage.

## Correction

- remove that patch/handler from the stable route;
- keep living custom impacts on post-damage `HealthElement.OnDamage`;
- move one terminal burst to `HealthElement.OnDeathEvents`;
- leave repeated corpse hits unsupported until a safe owner is proven.

## Lesson

A convenient early hook can be semantically wrong even when it exposes useful data. Correct ownership is more important than access convenience.
