# Medusa

Use this page when you are dealing with **large amounts of static environment geometry that no longer behave like ordinary scene MeshRenderers at runtime**.

## What Medusa does

Medusa is Questline's specialized renderer for fully static, long-distance environment geometry.

Artists can start with normal Unity objects such as:

- `LODGroup`;
- `MeshRenderer`;
- `MeshFilter`;
- meshes/materials;
- colliders.

Build-time processing converts the visual representation into compact Medusa runtime data.

Gameplay colliders can remain normal Unity scene objects.

## What Medusa owns

Medusa owns the static visual side:

- baked per-scene renderer data;
- compact transform/matrix data;
- mesh/material records;
- LOD/visibility data;
- persistent static GPU data;
- `BatchRendererGroup` rendering;
- high-volume frustum/LOD culling;
- mipmap-demand participation.

It is not the gameplay owner for the world object.

## Main types

- `MedusaRendererManager`
- `MedusaBrgRenderer`
- `MedusaRendererManagerBaker`
- `MedusaPersistence`
- `MedusaRendererPrefab`

Primary managed assembly: `Awaken.ECS.dll`.

## Build/runtime flow

~~~text
Unity static authoring
→ Medusa scene processing
→ per-scene Medusa payloads
→ medusa.arch
→ MedusaPersistence mount
→ compact CPU-side renderer/LOD state
→ static GPU data
→ BatchRendererGroup
→ Unity / HDRP
~~~

The scene data includes transforms/matrices, renderer records, transform indices, and UV-distribution data used by rendering/mipmap behavior.

## Why Medusa exists

A distant static environment can contain huge numbers of objects that do not need full active GameObject/Renderer overhead.

Medusa keeps those visuals in a compact representation optimized for:

- large static populations;
- long-distance rendering;
- per-view culling;
- LOD selection.

## When this page is useful

Use Medusa knowledge when:

- a cliff/rock/static environment group ignores ordinary MeshRenderer changes;
- distant visuals differ from nearby Unity objects;
- you are researching scene baking or Medusa payloads;
- you need to understand why colliders remain while the visual representation is specialized.

## Common mistakes

- treating a Medusa-owned visual as a movable runtime prefab;
- editing only the original authoring MeshRenderer and expecting the baked runtime representation to change;
- assuming the visual owner also owns gameplay/collision;
- confusing Medusa with HLOD or Drake without checking the object type.

## What to verify

Check:

1. whether the content is actually Medusa-owned;
2. scene/payload identity;
3. Medusa data mount/load;
4. renderer/LOD records;
5. visibility/culling;
6. material/mesh resources;
7. collider/gameplay object remains separate where applicable;
8. scene cleanup/unload.

## Related pages

- [HLOD](../hlod/README.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
