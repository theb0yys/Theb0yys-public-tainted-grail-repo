---
document_type: troubleshooting
scope: legacy Unity fog controls have no visible effect
last_verified: 2026-09-20
---

# RenderSettings Fog Changes but the World Still Looks Foggy

Check HDRP/FoA ownership before adding another legacy fallback.

A private live probe found:

`RenderSettings.fog=false`

while heavy visible fog remained.

At the same time, active HDRP volume profiles contained active `Fog` components and local volumetric fog objects.

## Diagnose

1. Record legacy `RenderSettings` state.
2. Inspect active `Volume` profiles.
3. Identify active HDRP `Fog` components.
4. Inventory `LocalVolumetricFog`.
5. Identify FoA `FogController` / `FogQuality` context.
6. Change only the owner needed for the intended effect.
7. Restore the captured values when the feature disables.

Do not infer “the game ignores my config” when the config is writing the wrong owner.
