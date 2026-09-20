---
document_type: mechanic
scope: additive plugin-owned FMOD Core music selected from read-only game context
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: PARTIAL_LOG_LEVEL
  save: NONE
last_verified: 2026-09-20
---

# Plugin-Owned Contextual Music Lanes

Tainted Music uses plugin-owned FMOD Core playback instead of rewriting native scene/audio assets.

## Context inputs

The source-inspected route reads:

- playable `Hero.Current`;
- strict Wyrdness exposure;
- `SceneService.IsOpenWorld`;
- native game time / `WeatherTime.IsNight`;
- dialogue involvement;
- recent hero-involved damage for ducking;
- configurable scene/display-name keywords for settlement/scary-place buckets.

## Lane priority

```text
no playable hero → silence
Wyrdness → Wyrdness lane
scary-place keyword → scary lane
settlement keyword → settlement lane
not open world → interior lane
open world + daytime → day lane
otherwise → silence
```

Settlement/scary-place classification is deliberately **keyword policy**, because the inspected scene config did not expose a native settlement/asylum taxonomy.

## Playback ownership

The mod owns only its FMOD Core channels and fades/releases its own sounds.

Dialogue/combat ducking multiplies **plugin-owned target volume**; it does not mutate dialogue/combat audio.

## Evidence boundary

Historical private logs established some day/night/Wyrdness lane transitions. The expanded library, all lane transitions and audible overlap behaviour were not fully validated as one current matrix.
