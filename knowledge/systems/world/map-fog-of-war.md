---
document_type: system
scope: map-screen fog-of-war and marker visibility
runtime: mono
game_build: Steam mono 24270691
evidence:
  static: DECOMPILED
last_verified: 2026-09-20
source_artifact_sha256: 749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982
---

# Map Fog of War

Use this page when you want to change **what the map reveals or hides**.

Map fog is not world atmospheric fog, and changing it does not automatically change discovery or fast travel.

## Native map-fog owners

- `MapUI.FogOfWarEnabled` — whether map fog display is enabled.
- `VMapSceneUI.ApplyFogOfWar()` — applies or clears the map mask.
- `FogOfWar.CreateMaskTexture()` — builds the visibility mask from visited pixels/current position.
- `FogOfWar.IsPositionRevealed(...)` — participates in marker visibility.
- `MapMemory` — persists `visitedPixels`.

## Keep these systems separate

~~~text
world HDRP fog
≠ map fog display
≠ map marker visibility
≠ persisted MapMemory
≠ fast-travel/discovery state
~~~

If your goal is only to remove the visible map mask, do not also write discovery memory unless that is intentional.

## Display-only changes

A display-layer change can:

- disable fog rendering;
- replace/skip mask creation;
- change how the map image presents explored state.

That does not necessarily mean the player has "discovered" every location in native game state.

## Persistent discovery changes

Writing `MapMemory.visitedPixels` or other discovery state is a different feature with save implications.

Treat it separately from a visual fog toggle.

## Marker visibility

Because `FogOfWar.IsPositionRevealed(...)` can participate in marker visibility, a fog change may affect which markers are shown even when travel permissions are unchanged.

Verify marker behavior separately.

## How to verify

Check:

1. fog display on/off;
2. mask generation;
3. revealed/unrevealed positions;
4. marker visibility;
5. `MapMemory` unchanged for a display-only mod;
6. map reopen;
7. scene/map transition;
8. save/load only if persistent discovery is changed;
9. fast-travel state separately.

## Evidence

This page is based on decompilation of the inspected Mono build `24270691`.
