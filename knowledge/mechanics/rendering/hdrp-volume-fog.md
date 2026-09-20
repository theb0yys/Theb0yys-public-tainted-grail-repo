---
document_type: mechanic
scope: runtime fog preset over active game-owned HDRP fog components
runtime: mono
evidence:
  static: DECOMPILED_AND_PROBED
  runtime: LIVE_VISUAL_PASS_FOR_BOUNDED_PRESETS
last_verified: 2026-09-20
---

# Control World Fog Through Existing HDRP Owners

The proven direction is to mutate only active game-owned HDRP fog components by reflection.

## Path

```text
active scene HDRP Volume profile
→ find active Fog parameters
+ active LocalVolumetricFog components
→ capture original active/parameter state
→ apply bounded preset values
→ restore originals on disable/unload
```

## Why this route exists

Legacy `RenderSettings` changes produced little/no visible effect while HDRP volume fog remained active.

A later HDRP/FoA volume route produced strong visible fog-removal / far-landmark improvement in private screenshot evidence.

## Rules

- do not create a parallel global HDRP volume unless separately proven;
- avoid broad per-tick `Resources.FindObjectsOfTypeAll<Component>()` scans;
- cache discovered fog targets and refresh on a slower lifecycle;
- restore captured originals;
- keep weather/time readback separate from weather mutation.

## Dynamic day/night

A preset may read native `GameRealTime.WeatherTime.IsNight` and select different fog strength/distance policy without changing game time/weather.

That policy is mod-owned; the day/night truth remains native.
