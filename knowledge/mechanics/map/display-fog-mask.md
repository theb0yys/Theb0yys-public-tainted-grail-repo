---
document_type: mechanic
scope: remove map-screen fog display without rewriting map memory
runtime: mono
game_build: Steam mono 24270691
evidence:
  static: DECOMPILED
  build: PASSED_PRIVATE_MOD
  runtime: NOT_CAPTURED_IN_CITED_PASS
last_verified: 2026-09-20
---

# Remove the Map Fog Display

A display-only map-fog route can operate without mutating visited-map memory.

## Bounded surfaces

While enabled, a mod may:

- force `MapUI.FogOfWarEnabled=false`;
- make `FogOfWar.CreateMaskTexture()` return no mask for the map display;
- optionally make `FogOfWar.IsPositionRevealed(...)` return true for marker-display policy.

## Do not mutate

- `MapMemory.visitedPixels`;
- map serialization;
- fast-travel permission;
- quest/location discovery state;
- source marker data;
- world atmospheric fog.

## Evidence boundary

The private No Map Fog project built/deployed against the exact Steam Mono build but its cited validation plan still listed plugin-load and in-game visual evidence as outstanding.
