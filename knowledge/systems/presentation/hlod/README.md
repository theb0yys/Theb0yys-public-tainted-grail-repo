# HLOD

Use this page when you are changing **distant world proxies, large environmental groups, or objects that switch between detailed and simplified representations with distance**.

## What HLOD does

HLOD is Questline's hierarchical level-of-detail and distant-world streaming system.

It groups many source renderers into spatial trees/clusters and uses cheaper proxy content at distance.

## What HLOD owns

HLOD owns:

- spatial HLOD trees/clusters;
- high/low representation state;
- proxy resource loading;
- camera/distance-driven transitions;
- load/release;
- distant-object culling;
- HLOD runtime state machines.

Primary runtime assembly: `HLOD.dll`.

## Main types

Current managed research maps types including:

- `HLODManager`
- `AddressableHLODController`
- `HLODTreeNode`
- `HLODLoadManager`
- `HLODCameraRecognizer`

## Content layout

FoA uses both:

- `StreamingAssets/HLODs/hlods.arch`
- HLOD-related Addressables resources/bundles.

The control/hierarchy data and the actual proxy visual resources can therefore live in different packaging layers.

## Runtime flow

~~~text
source scene renderers
→ HLOD build clustering
→ simplified proxy resources
→ control data + Addressables assets
→ HLODCameraRecognizer
→ HLODManager/controller
→ tree high ↔ low state
→ HLODLoadManager
→ distant representation
~~~

## Why HLOD exists

At distance, keeping full-detail source objects/renderers active is expensive.

HLOD substitutes cheaper proxy representations and reduces the amount of detailed active content.

## When this page is useful

Use HLOD knowledge for:

- distant world visibility;
- large static environment groups;
- proxy meshes;
- distance transitions;
- world streaming/culling issues.

## Do not make the proxy the gameplay object

An HLOD proxy is presentation.

Do not attach gameplay truth, interaction state, inventory, quest state, or other durable logic to the distant proxy representation.

The detailed/source gameplay owner remains separate.

## Common mistakes

- editing a proxy and expecting source gameplay state to change;
- treating the archive as the entire HLOD system;
- assuming the high/low object transition is a simple Renderer toggle;
- changing distant visuals without checking Addressables/resource ownership;
- confusing HLOD with Medusa/Drake without identifying the runtime owner.

## What to verify

Check:

1. HLOD tree/controller identity;
2. camera recognition;
3. high/low state transition;
4. proxy resource load;
5. source/detail resource state;
6. culling/visibility;
7. release when state changes;
8. scene cleanup/unload.

## Related pages

- [Medusa](../medusa/README.md)
- [Drake](../drake/README.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
