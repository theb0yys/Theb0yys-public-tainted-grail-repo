# HDRP / FoA Fog Control

The working fog-removal path modifies FoA's **existing active fog owners**. It does not create a competing global fog system.

## Working implementation lineage

Views of Avalon 0.2.8 was feature-tested on the Mono/BepInEx 5 stack. User screenshots showed a major fog-removal / visibility change.

## Native/runtime owners

The successful path targets:

- active `UnityEngine.Rendering.Volume` profiles;
- the HDRP/FoA fog components inside those profiles;
- active local volumetric fog components where applicable.

The earlier legacy `RenderSettings.fog` route was insufficient because visible atmosphere remained even when legacy fog was already off.

## Working sequence

~~~text
discover active FoA/HDRP volume owners
→ cache the relevant fog targets
→ capture the values the mod will own
→ apply the selected fog profile
→ reapply only when required
→ restore captured values when disabled/unloaded
~~~

## Why reflection is useful here

The working implementation avoided a hard HDRP assembly dependency and reached the active FoA/HDRP components by reflection. That reduced load-time coupling while still editing the real runtime owners.

## Performance rule

Do not scan every loaded component every frame.

Discover/cache the active targets, then update the small owned set.

## Keep separate

Fog tuning does not imply ownership of:

- time of day;
- weather state;
- precipitation;
- HLOD;
- vegetation streaming;
- camera layer culling.

Those are separate systems.
