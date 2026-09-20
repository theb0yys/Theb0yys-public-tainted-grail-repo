---
document_type: case
scope: Views of Avalon world-fog target correction
evidence:
  runtime: TARGETED_PROBE_PLUS_VISUAL_EVIDENCE
last_verified: 2026-09-20
---

# World Fog: Correcting the Owner

Views of Avalon went through several wrong or incomplete assumptions:

1. legacy `RenderSettings` fog changes;
2. direct HDRP reference experiments that caused startup crashes;
3. reflection-only targeted probing;
4. discovery of active game-owned HDRP Volume/Fog and local volumetric fog;
5. bounded mutation/restoration of those existing owners.

The later route produced visible far-view/fog changes in private screenshots.

## Lesson

When a visual setting “applies” but the screen does not change, prove which renderer/volume actually owns the visible result before broadening mutation.
