# HLOD

## What it is

**HLOD** is Questline's hierarchical level-of-detail and distant-world streaming system.

It groups many source renderers into spatial hierarchies and uses cheaper simplified proxy content at distance.

## What it owns

HLOD owns the distant proxy lifecycle:

- spatial HLOD trees/clusters;
- high/low representation state;
- proxy asset loading;
- distance/camera-driven transitions;
- resource loading/release;
- distant-object culling;
- HLOD runtime state machines.

Primary runtime assembly: `HLOD.dll`.

## Runtime family

Current managed research maps types including:

- `HLODManager`
- `AddressableHLODController`
- `HLODTreeNode`
- `HLODLoadManager`
- `HLODCameraRecognizer`

## Content layout

FoA ships both:

- `StreamingAssets/HLODs/hlods.arch`
- HLOD-related Addressables bundles/resources

So the system is not just one archive. Control/hierarchy state and actual proxy visual resources can be packaged through different layers.

## Runtime shape

~~~text
scene source renderers
→ HLOD build clustering/hierarchy
→ simplified proxy resources
→ HLOD control data + Addressables assets
→ HLODCameraRecognizer
→ HLODManager / controller
→ tree high ↔ low state
→ HLODLoadManager
→ distant representation
~~~

## Why it exists

At long distances, full source geometry, objects and per-object visibility work are expensive. HLOD substitutes cheaper representations and reduces the amount of active detailed content.

## Modding relevance

Use HLOD knowledge when changing:

- long-distance world visibility;
- large environmental groups;
- distant proxy meshes;
- world streaming/culling behaviour.

Do not treat an HLOD proxy as the authoritative gameplay object.

## Related systems

- [Scenes Baking](../scenes-baking/README.md)
- [Medusa](../medusa/README.md)
- [Drake](../drake/README.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
