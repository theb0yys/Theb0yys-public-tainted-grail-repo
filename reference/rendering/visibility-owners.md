# Visibility / Fog Owner Reference

| Visible problem | Primary owner family | Do not confuse with |
| --- | --- | --- |
| world atmospheric fog | HDRP Volume `Fog`, LocalVolumetricFog, FoA FogController | map fog-of-war |
| map-screen fog mask | MapUI / VMapSceneUI / FogOfWar | world HDRP fog |
| map visited memory | MapMemory.visitedPixels | display-only reveal |
| general proprietary object distance culling | DistanceCullingSetting / DistanceCuller | camera far clip alone |
| HLOD proxy streaming | HLODManager / AddressableHLODController | DistanceCullerGroup |
| vegetation visibility | Leshy / vegetation runtime ownership | global terrain distance alone |
| rigid ECS rendering | Drake | ordinary MeshRenderer assumptions |

Start from the owner that matches the visible symptom.
