# Scenes Baking

Use this page when you are dealing with scene content that is transformed during build/baking into FoA-specific runtime data.

It helps explain why the authored Unity scene object may not be the same representation that exists in the shipped game.

## What it is

**Scenes Baking** is Questline's editor/build-time scene compilation pipeline.

It is not one renderer. It coordinates scene preparation and hands scene content to specialised systems such as Leshy, HLOD, Medusa and Drake.

## Main build owners

Public source exposes:

- `BuildTools.ProcessScenes(...)`
- `BuildSceneBaking`
- `ScenesStaticSubdivision`
- `ScenesProcessing`
- `SceneProcessor`

## What it does

A production scene build can:

- open authored scenes;
- cache stable scene-object identities;
- run custom preprocessing;
- split static and dynamic content;
- create generated static scenes;
- flatten non-semantic hierarchies;
- run ordered system processors;
- merge subdivided scenes;
- rewrite Addressables ownership;
- save generated scene outputs;
- continue into later Addressables/archive/player-build steps.

## Processing order

Current managed constants include:

~~~text
Leshy       1
HLOD        5
Medusa     10
Drake      20
DistanceCulling 100
NpcDummy   200
SceneUnfold 999
~~~

The exact processors differ, but the order shows how specialised systems are sequenced relative to one another.

## Static subdivision

`ScenesStaticSubdivision` is responsible for much of the static/dynamic split. It can create a `StaticObjects` root, generated static scenes and the scene metadata needed for the final runtime layout.

## Wider build orchestration

Scene processing is one stage inside a larger content build that also handles localisation, Kandra preparation, story/skill baking, archive generation and Addressables.

## Modding relevance

This page explains why a raw Unity scene or renderer hierarchy may differ dramatically from the shipping runtime representation.

If a system is produced during Scenes Baking, runtime code should target the **shipping owner**, not assume the original authoring GameObjects survived unchanged.

## Related systems

- [Drake](../../presentation/drake/README.md)
- [Medusa](../../presentation/medusa/README.md)
- [Leshy](../../presentation/leshy/README.md)
- [HLOD](../../presentation/hlod/README.md)
- [Runtime orchestration](../../core/runtime-orchestration/README.md)
