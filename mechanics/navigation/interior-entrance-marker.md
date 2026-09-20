---
document_type: mechanic
scope: remember entry coordinates and display a passive exit-direction marker in interiors
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: PRIVATE_IMPLEMENTATION_PENDING_RELEASE_VISUAL_PROOF
  save: NONE
last_verified: 2026-09-20
---

# Passive Interior Entrance Marker

A small “where did I enter?” helper can avoid map/teleport mutation completely.

## Path

```text
SceneService reports non-open-world scene
→ wait short settle delay
→ read Hero.Current.Coords
→ remember coordinate in session
→ each frame project remembered point through camera
→ draw passive EXIT marker/distance
→ clear/replace on scene change
```

Optional combat hiding can read `HeroCombat.IsHeroInFight`.

## Boundary

This does not:

- map interior geometry;
- create native map/compass markers;
- reveal loot/enemies;
- teleport;
- change scene transitions;
- write saves.

The remembered position is session-only until persistence is separately researched.

## Camera behaviour

If the remembered entrance is behind the camera, hide the marker rather than mirroring it into view. Screen-edge clamping is appropriate only for points in front of the camera.
