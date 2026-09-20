# Drake and MergedDrake

## What it is

**Drake** is Questline's rigid-mesh rendering system. It takes conventional Unity rigid-mesh authoring and represents the render side through Unity ECS / Entities Graphics while preserving the gameplay or scene object that owns the content.

**MergedDrake** is the scene-static bulk form. It is used when large numbers of static Drake renderers can be compiled into a compact dataset and created as ECS render entities without keeping thousands of ordinary renderer components alive.

## What it owns

Drake owns the rigid presentation layer:

- mesh and material resource identity;
- Addressables-backed mesh/material loading;
- resource reference counts;
- ECS render entities;
- render archetype selection;
- LOD/load state;
- scene lifetime;
- movable transform synchronisation;
- runtime material state;
- mipmap-demand participation.

It does **not** own item identity, inventory, combat, saves or NPC logic.

## Main public/runtime types

- `DrakeMeshRenderer`
- `DrakeLodGroup`
- `DrakeRendererManager`
- `DrakeRendererLoadingManager`
- `DrakeRendererComponentsManager`
- `DrakeRendererEntitiesManager`
- `DrakeMergedRenderersRoot`
- `DrakeMergedRenderersLoading`

Primary managed assembly: `Awaken.ECS.dll`.

## Ordinary Drake lifecycle

~~~text
MeshRenderer / MeshFilter / optional LODGroup
→ Drake authoring bridge
→ mesh/material Addressable identities
→ Drake manager registration
→ resource acquisition
→ ECS render entity creation
→ Entities Graphics mesh/material IDs
→ LOD / visibility / transform updates
→ owner-led release
~~~

Movable gameplay objects such as equipped rigid weapons keep their gameplay/equip owner while Drake owns the render entities underneath.

## MergedDrake lifecycle

~~~text
eligible scene-static Drake content
→ scene build processing
→ merged static records
→ StreamingAssets/DrakeMR/merged_drakes.arch
→ runtime reader
→ bulk ECS entity creation
→ normal Drake resource realisation
~~~

MergedDrake is for large static populations, not the normal starting point for a runtime weapon or other movable gameplay object.

## Resource ownership

Drake tracks first-owner / shared-owner / last-owner resource lifetime. Multiple renderers can share one loaded mesh/material resource; final release occurs when the last owner releases it.

That is why direct manipulation of internal resource counters or ECS state is unsafe: the visible entity is only one part of the ownership graph.

## Modding relevance

Use Drake knowledge when working with:

- rigid equipped weapon presentation;
- runtime rigid prefabs;
- static rigid scene content;
- critter visuals that render through Drake;
- material overrides on Drake-owned content.

The public cookbook's general rule applies: preserve the gameplay owner and integrate with the real presentation owner instead of replacing the entire object just to change its mesh.

## Related systems

- [Shared mipmap streaming](../mipmap-streaming/README.md)
- [Scenes Baking](../../world/scenes-baking/README.md)
- [Critter VAT / ECS](../critter-vat/README.md)
- [Native weapons](../../gameplay/native-weapons/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
