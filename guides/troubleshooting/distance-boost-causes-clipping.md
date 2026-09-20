---
document_type: troubleshooting
scope: foliage/object clipping after draw-distance tweaks
last_verified: 2026-09-20
---

# Draw-Distance Boost Causes Clipping or Foliage Popping

Separate the controls you changed.

Private Views of Avalon history found regressions when global LOD/terrain and layer-cull paths were broadened before the native owner was fully mapped.

## Check

- Was `Camera.layerCullDistances` changed?
- Was global/HD camera LOD bias raised?
- Were terrain/tree/detail distances changed?
- Was the native `DistanceCullingSetting` path used?
- Did the proprietary `DistanceCuller` actually receive a safe corresponding bias?
- Are HLOD/vegetation systems making an independent visibility decision?

## Safe recovery

Restore the original camera/layer/global values, turn experimental paths off, and re-enable one owner at a time.

A visible farther camera does not prove every proprietary renderer will remain loaded.
