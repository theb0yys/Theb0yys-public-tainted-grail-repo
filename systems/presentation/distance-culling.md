---
document_type: system
scope: native FoA distance-culling / LOD ownership
runtime: mono
evidence:
  static: DECOMPILED
  runtime: TARGETED_PROBE_CORROBORATED
last_verified: 2026-09-20
---

# Native Distance Culling

FoA has its own distance-culling ownership rather than relying only on `Camera.farClipPlane` or global Unity LOD settings.

## Native owner chain

```text
DistanceCullingSetting
  → BiasValue / SetDebugValue(float)
  → refresh + DistanceCullersService.BiasChanged()
  → QualitySettings.lodBias

DistanceCullingCameraController
  → GenericTarget = DistanceCullingSetting
  → writes bias into HDRP camera frame settings

LodBiasWatcher
  → re-syncs through DistanceCullingSetting when camera/global bias diverges

DistanceCuller
  → recomputes native squared cull distances from range table / Bias
  → DistanceCullerGroup renderer/effect enabled state
```

A targeted live probe confirmed `Camera`, `HDAdditionalCameraData`, `HLODCameraRecognizer`, `LodBiasWatcher` and `DistanceCullingCameraController` on the active main-camera path, with the controller target resolving to `DistanceCullingSetting`.

## HLOD is separate

The game also has HLOD controllers/load management and recognized-camera ownership. Those surfaces were mapped/probed but were not promoted into a general HLOD mutation mechanic.
