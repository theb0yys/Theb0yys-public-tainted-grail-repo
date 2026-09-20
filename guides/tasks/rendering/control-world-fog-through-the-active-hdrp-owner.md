# Control World Fog Through the Active HDRP Owner

Use this guide to change visible world fog without creating a competing rendering system.

Canonical mechanic: [Control World Fog Through Existing HDRP Owners](../../../knowledge/mechanics/rendering/hdrp-volume-fog.md).  
Working lineage: [HDRP / FoA Fog Control](../../../research/case-studies/rendering/fog-control.md).

## What you will build

```text
discover active FoA/HDRP Volume owners
→ find active Fog/local volumetric fog targets
→ capture original state
→ apply bounded preset
→ refresh only when lifecycle requires it
→ restore originals on disable/unload
```

## Step 1 — do not start with legacy RenderSettings

The working research found that changing legacy `RenderSettings.fog` could leave the visible atmosphere essentially unchanged because HDRP volume fog still owned the result.

Use the active renderer/volume owner.

## Step 2 — discover active fog targets

Find the active game-owned:

- HDRP `Volume` profiles;
- Fog parameters inside those profiles;
- active local volumetric fog components where applicable.

The working implementation used reflection to avoid a hard HDRP assembly dependency.

## Step 3 — capture before mutation

Before changing a target, capture only the state your mod will own:

- active/enabled state;
- relevant distance/strength parameters;
- local volumetric values you intend to modify.

Do not assume the original is a hardcoded default.

## Step 4 — apply one bounded preset

Start with a simple preset such as "reduced world fog".

Change only the relevant discovered parameters.

Do not also mutate:

- game time;
- weather state;
- skybox;
- HLOD;
- vegetation;
- camera culling.

Those are separate owners.

## Step 5 — cache targets

Do not scan every loaded component every frame.

Use:

```text
scene/context available
→ discover/cache targets
→ apply preset
→ refresh discovery only on slower lifecycle/context changes
```

## Step 6 — optional day/night policy

A preset may read native:

```text
GameRealTime.WeatherTime.IsNight
```

and choose a different **mod-owned fog preset**.

Reading day/night does not give the fog mod authority to change game time/weather.

## Step 7 — restore on disable/unload

Restore the captured original values for every target your mod changed.

Cleanup is part of the feature.

## Verification checklist

1. active HDRP fog owners are discovered;
2. baseline values are captured;
3. preset changes the visible fog;
4. far visibility changes as expected;
5. no unrelated sky/weather/time state changes;
6. scene change refreshes targets correctly;
7. repeated operation does not scan/allocate excessively;
8. disable/unload restores captured values.

Views of Avalon 0.2.8 was feature-tested and user screenshots showed a major fog-removal/visibility change.

## Evidence boundary

**Proven:** bounded live visual fog control through active FoA/HDRP owners, including strong visible far-view change.

**Not claimed:** weather ownership, universal HDRP versions, skybox control, HLOD/vegetation/camera culling or arbitrary rendering replacement.
