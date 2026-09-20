---
document_type: troubleshooting
scope: scaled magic projectile misses crosshair / flies low or offset
last_verified: 2026-09-20
---

# Projectile Speed Scaling Breaks Aim

Do not multiply the entire runtime projectile velocity blindly.

FoA can include a hand/fire-point offset correction through `ProjectileOffsetData.InitialVelocity`.

## Failure model

The broken route effectively did:

```text
SetVelocityAndForward(totalVelocity * multiplier)
```

That can scale the offset-compensation vector and mark the offset interpolation complete.

## Corrected model

Recover the aim component:

```text
aimVelocity = projectile.Velocity - projectile.PositionOffset.InitialVelocity
scaled = aimVelocity * multiplier
final = scaled + original InitialVelocity
```

Then update runtime velocity without destroying the existing offset interpolation.

## Lesson

When a vector contains both gameplay intent and corrective transport data, scale only the component you actually own.
