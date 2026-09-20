---
document_type: mechanic
scope: bounded native distance-culling bias control
runtime: mono
evidence:
  static: DECOMPILED
  runtime: PROVEN_CONTROLLER_TARGET_PATH
  safety: NON_EXPERIMENTAL_BOOSTS_CLAMPED
last_verified: 2026-09-20
---

# Native Distance-Culling Bias

The strongest researched control seam is the existing `DistanceCullingCameraController.GenericTarget → DistanceCullingSetting.SetDebugValue(float)` path.

## Use the owner, not fallbacks

Earlier attempts tried:

- generic `World.Any<DistanceCullingSetting>()`;
- direct HDRP camera frame-setting fallback;
- camera `layerCullDistances`;
- global LOD/terrain writes.

Targeted probing eventually identified the actual live controller/setting relationship. The public mechanic should use that proven owner path.

## Safety finding

User clipping reports and decompiled native `DistanceCuller` math made above-vanilla distance boosts unsafe to generalize.

Private project policy therefore:

- hid camera layer-cull mutation behind an explicit experimental gate;
- disabled risky broad probes by default;
- clamped non-experimental native distance-culling targets above the vanilla value.

## Lesson

A setting can successfully change a camera LOD bias while still not safely extending every proprietary renderer/group cull distance. Validate the downstream culler, not only the setting write.
