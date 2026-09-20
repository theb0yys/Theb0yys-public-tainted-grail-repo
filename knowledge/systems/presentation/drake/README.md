# Drake and MergedDrake

Use this page when a **rigid mesh, equipped weapon, runtime rigid prefab, or large static rigid scene group** is rendered through FoA's Drake system instead of an ordinary Unity Renderer path.

## What Drake does

Drake is Questline's rigid-mesh rendering system.

It takes conventional rigid-mesh authoring data and represents the runtime rendering side through Unity ECS / Entities Graphics while the original gameplay or scene object keeps owning the gameplay state.

A useful mental model is:

~~~text
gameplay / scene owner
→ Drake mesh/material identity
→ Drake manager registration
→ ECS render entity
→ visibility / LOD / transform updates
→ owner-led release
~~~

## What Drake owns

Drake owns rendering concerns such as:

- mesh/material resource identity;
- Addressables-backed resource loading;
- resource reference counts;
- ECS render entities;
- render archetype;
- LOD/load state;
- transform synchronization for movable objects;
- material state;
- mipmap-demand participation;
- renderer resource cleanup.

It does **not** own:

- item identity;
- inventory;
- weapon combat;
- NPC AI;
- save state.

For an equipped weapon, the `Item` / equip / hand chain remains the gameplay owner while Drake handles the visual representation.

## Main runtime types

Important types include:

- `DrakeMeshRenderer`
- `DrakeLodGroup`
- `DrakeRendererManager`
- `DrakeRendererLoadingManager`
- `DrakeRendererComponentsManager`
- `DrakeRendererEntitiesManager`
- `DrakeMergedRenderersRoot`
- `DrakeMergedRenderersLoading`

Primary managed assembly: `Awaken.ECS.dll`.

## Normal Drake lifecycle

~~~text
MeshRenderer / MeshFilter / optional LODGroup
→ Drake authoring bridge
→ mesh/material Addressable identities
→ manager registration
→ resource acquisition
→ ECS entity creation
→ Entities Graphics mesh/material IDs
→ LOD / visibility / transform updates
→ owner-led release
~~~

For movable objects, the gameplay/equip owner remains responsible for when the object exists; Drake owns the render representation underneath it.

## MergedDrake

MergedDrake is the bulk/static form used for large groups of static Drake content.

~~~text
eligible static scene content
→ scene build processing
→ merged records
→ StreamingAssets/DrakeMR/merged_drakes.arch
→ runtime reader
→ bulk ECS entity creation
→ Drake resource realization
~~~

Use MergedDrake when investigating large static populations.

Do **not** treat it as the normal starting point for an equipped weapon or other movable runtime object.

## Shared resources matter

Drake can share one loaded mesh/material resource across several renderers.

That means lifetime is not just "delete this one entity."

The system tracks first/shared/last ownership so final resource release happens only when the final owner releases it.

Do not manipulate internal reference counts or ECS resources directly unless you have mapped that ownership.

## When this page is useful

Use Drake knowledge for:

- rigid equipped weapon visuals;
- rigid runtime prefabs;
- static rigid scene content;
- critter visuals rendered through Drake;
- Drake-owned material overrides.

## Common mistakes

- replacing only the visible Unity MeshRenderer and ignoring the Drake entity;
- making the Drake entity the source of gameplay truth;
- manually deleting shared resources;
- assuming MergedDrake applies to movable objects;
- treating a successful visual spawn as proof of equip/combat/save integration.

## What to verify

Check:

1. which gameplay/scene owner creates the visual;
2. exact Drake renderer/resource identity;
3. registration;
4. mesh/material resource load;
5. ECS entity creation;
6. LOD/visibility;
7. transform sync if movable;
8. release when the gameplay owner is removed;
9. no shared-resource leak;
10. scene/equip lifecycle as relevant.

## Related pages

- [Native weapons](../../gameplay/native-weapons/README.md)
- [Critter VAT / ECS](../critter-vat/README.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
