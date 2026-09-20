---
document_type: system
scope: visible world fog ownership in FoA HDRP runtime
runtime: mono
evidence:
  static: DECOMPILED_AND_TARGETED_PROBE
  runtime: LIVE_VISUAL_CORROBORATION
last_verified: 2026-09-20
---

# HDRP / FoA World Fog Ownership

Visible world fog in FoA is not controlled by legacy `RenderSettings.fog` alone.

## Evidence

A targeted live probe observed:

- `RenderSettings.fog=false`;
- legacy fog density at zero;
- visible atmospheric fog still present;
- active/global HDRP `Volume` profiles containing active `UnityEngine.Rendering.HighDefinition.Fog`;
- FoA `FogController` instances;
- HDRP `VolumeManager` fog-stack state;
- multiple `LocalVolumetricFog` components.

Decompilation also showed `FogController` reading the attached HDRP Fog component and a native `FogQuality` setting that controls quality, not “remove fog” distance/strength semantics.

## Ownership model

```text
game-owned HDRP Volume profile Fog
+ LocalVolumetricFog
+ FoA FogController / FogQuality
→ visible world atmospheric fog
```

A mod trying to change visible world fog should reason about these owners rather than assuming Unity legacy fog state is authoritative.

## Boundary

Creating new HDRP volumes or adding direct HDRP compile-time dependencies had caused startup instability in earlier experiments. The safer researched route used reflection over **already active game-owned** components, captured original values and restored them on disable/unload.
