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

Map fog is a separate system from world atmospheric fog.

## Native owners

- `MapUI.FogOfWarEnabled` — display enable state.
- `VMapSceneUI.ApplyFogOfWar()` — applies/clears the map image material mask.
- `FogOfWar.CreateMaskTexture()` — constructs the visible-map mask from visited pixels/current position.
- `FogOfWar.IsPositionRevealed(...)` — participates in marker visibility.
- `MapMemory` — persists `visitedPixels`.

## Separation

```text
world HDRP fog
≠ map fog mask
≠ map marker visibility
≠ persisted map memory
≠ fast-travel / quest discovery state
```

Changing only the display layer should not write `MapMemory`.
