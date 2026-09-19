# Medusa

## What it is

**Medusa** is Questline's specialised renderer for fully static, long-distance environment geometry.

Artists can author with ordinary Unity concepts such as `LODGroup`, `MeshRenderer`, `MeshFilter`, meshes, materials and colliders. A build-time conversion moves the **visual representation** into Medusa's compact runtime data.

Gameplay colliders can remain normal Unity scene objects.

## What it owns

Medusa owns the static visual side:

- baked per-scene renderer data;
- compact transform/matrix data;
- renderer/material/mesh records;
- LOD and visibility data;
- static-scene GPU data;
- `BatchRendererGroup` rendering;
- high-volume frustum/LOD culling;
- mipmap-demand participation.

## Main types

- `MedusaRendererManager`
- `MedusaBrgRenderer`
- `MedusaRendererManagerBaker`
- `MedusaPersistence`
- `MedusaRendererPrefab`

Primary managed assembly: `Awaken.ECS.dll`.

## Build/runtime shape

~~~text
Unity static authoring
LODGroup + MeshRenderer + MeshFilter
→ Medusa scene processing
→ per-scene Medusa payloads
→ medusa.arch
→ MedusaPersistence mount
→ compact CPU renderer / LOD state
→ persistent static GPU payload
→ BatchRendererGroup
→ Unity / HDRP
~~~

The per-scene data includes transform and matrix information, renderer data, transform indices and reciprocal UV-distribution information used by the rendering/mipmap path.

## Why it exists

Static distant environments contain many objects that do not need full GameObject/Renderer runtime cost.

Medusa keeps that geometry in a representation suited to:

- long-distance rendering;
- compact immutable scene data;
- efficient per-view culling;
- large static populations.

## Modding relevance

Use Medusa knowledge when analysing cliffs, rock fields or other large static environment groups whose visual representation does not behave like a normal runtime `MeshRenderer`.

Do not treat a Medusa-owned static visual as an ordinary movable runtime prefab.

## Related systems

- [Scenes Baking](../scenes-baking/README.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
- [HLOD](../hlod/README.md)
- [Runtime lifetime](../runtime-lifecycle/README.md)
