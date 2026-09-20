---
document_type: mechanic
scope: bounded third-person Cinemachine framing adjustments
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: TRUE_THIRD_PERSON_PROJECT_LINEAGE
last_verified: 2026-09-20
---

# Third-Person Framing Without Replacing the Camera

A bounded third-person framing route operates on the existing `Cinemachine3rdPersonFollow` rig instead of creating a second camera.

Useful fields include:

- `CameraDistance`;
- `CameraSide`;
- `ShoulderOffset`;
- `VerticalArmLength`.

## Safe composition pattern

Capture the native/original rig values, then apply **context deltas**:

```text
resolved shoulder offset = original + context delta
resolved vertical arm length = original + context delta
```

Clamp deltas and preserve native zoom ownership when the feature is not intentionally replacing zoom.

## Aim lesson

Bow framing defaults were later neutralized after user evidence that offset framing could affect perceived aim alignment.

That correction kept bow projectile/reticle/FSM logic out of scope: camera framing should not be “fixed” by patching projectile aim unless aim ownership is separately proven.
