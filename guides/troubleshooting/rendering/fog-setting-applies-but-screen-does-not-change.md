# Fog Setting Applies but the Screen Does Not Change

Use this when logs say your fog value changed but visible world fog remains.

Working lesson: [World Fog: Correcting the Owner](../../../research/case-studies/rendering/hdrp-fog-owner-correction.md).  
Working guide: [Control World Fog Through the Active HDRP Owner](../../tasks/rendering/control-world-fog-through-the-active-hdrp-owner.md).

## Symptom

```text
fog setting changed
+ no exception
but
visible atmosphere remains
```

## Likely cause

You changed a setting that is not the active renderer owner.

The fog work progressed through:

1. legacy `RenderSettings` fog;
2. direct HDRP-reference experiments;
3. reflection-only targeted probing;
4. discovery of active game-owned HDRP Volume/Fog and local volumetric fog;
5. bounded mutation/restoration of those actual owners.

## Diagnostic sequence

1. confirm which rendering pipeline is active;
2. inspect active game-owned Volume profiles;
3. locate active Fog overrides;
4. locate local volumetric fog where relevant;
5. compare the changed object with the object actually contributing to the frame;
6. mutate only one bounded owner;
7. verify visible output.

## Avoid

- broad global fog systems;
- adding a new global Volume to compete with the game;
- scanning every component every frame;
- assuming a successful setter call proves rendered output.

## Corrective rule

> When a visual setting "applies" but the screen does not change, prove the renderer/volume that owns the visible result before broadening mutation.

## Evidence boundary

Later FoA/HDRP owner-targeted work produced visible far-view/fog changes. This troubleshooting page does not imply all HDRP versions or scene configurations share identical private/component layouts.
