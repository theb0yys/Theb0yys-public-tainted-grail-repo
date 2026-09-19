# Creature / NPC / Kandra Pipeline

Evidence state: **STATIC_CONFIRMED**  
Fresh editor/game execution for this document: **NOT_RUN**

Source snapshot: `theb0yys/merlin-workshop@073bdab3e09d6adad5003339fc49b021738d71e6`

## The important distinction

In Merlin, **Kandra is part of the rendering/rig preparation path**, not the name of a creature gameplay class.

The practical creature route has two distinct stages:

1. prepare a visual prefab so it satisfies the NPC/Kandra/Animator contract;
2. prepare an NPC spec by duplicating a known-good logic/template/fighting-style chain and replacing its visual.

That second step is especially useful for modding because it avoids reconstructing every NPC dependency from zero.

# Stage 1 — Prepare the visual prefab

Merlin provides:

`TG → Assets → Prefabs → Prepare NPC Prefab`

The selected visual prefab must contain an `Animator`.

The preparation tool then checks/builds the structure expected by the NPC runtime.

## Animator-side components

The tool ensures the Animator object has:

- `ARNpcAnimancer`;
- root-motion component;
- `AnimatorClipPlayer`.

## Kandra renderer requirement

The tool searches under the Animator for `KandraRenderer` components.

If no Kandra renderer exists, preparation fails.

From the final Kandra renderer it resolves:

- renderer rig;
- root-bone index;
- root-bone transform.

This is why a generic imported skinned mesh is not automatically a valid creature prefab.

## Ragdoll/root setup

The preparation route checks for a ragdoll-layer object.

If none is found, it applies a temporary/fix-required fallback to the root bone and reports that it needs review.

## AlivePrefab / walk-through collider

If no `AlivePrefab` child exists, the tool creates one and creates a cylinder-based walk-through collision object sized from renderer bounds.

That generated collider is a starting point to inspect, not an excuse to skip creature-specific collision tuning.

## VFX renderer

The tool examines Kandra renderers and renderer markers.

When needed it creates a dedicated `VFXRenderer` with:

- a copied Kandra renderer data set;
- the same rig;
- optional simplified Kandra mesh;
- `VFXBodyMarker`.

If no simplified VFX mesh is supplied, the tool reports that as work still to fix.

## Required tags

The tool ensures/tag-marks:

- `RootBone`;
- `Head`;
- `Torso`.

If head or torso transforms were not supplied, placeholders are created under the root bone and explicitly reported for later correction.

## Save

For prefab assets, Merlin saves the modified prefab back through Unity's prefab API and marks the result dirty.

# Stage 2 — Prepare the NPC spec

Merlin provides:

`TG → Assets → Prefabs → Prepare NPC Spec`

This tool works from a **known-good base NPC spec prefab** plus the prepared visual prefab.

Its source comments identify the important dependency set:

- logic prefab;
- visual prefab;
- NPC template.

The tool duplicates and rewires the related content rather than sharing every original object unchanged.

## What the spec preparation duplicates/wires

Static-confirmed sequence:

1. load the selected base NPC spec prefab;
2. duplicate the referenced logic prefab;
3. replace the NPC visual prefab reference with the selected mod visual;
4. preserve/use the existing addressable group when making the new visual reference;
5. duplicate the referenced `NpcTemplate`;
6. point the NPC attachment at the duplicated template;
7. duplicate the template's fighting style;
8. duplicate the fighting style's base animation data;
9. duplicate the fighting style's base behaviour data;
10. preserve addressable/template registration for the duplicated assets;
11. save the resulting NPC spec.

The tool also supports rename substitutions so copied files can receive a new creature/NPC identity instead of retaining the base NPC's filenames.

# Stage 3 — Configure NPC gameplay data

The duplicated `NpcTemplate` is the gameplay-facing data surface.

It includes, among other things:

- faction / crime owner;
- `NpcData` perception and alert configuration;
- fighting style;
- health/stamina;
- melee/ranged/magic damage;
- armor and armor multiplier;
- damage-received multipliers;
- force/poise/block/combat-slot behavior;
- NPC type;
- inventory items;
- loot and corpse-loot tables;
- tags;
- audio/surface settings;
- VFX references;
- animation/death references.

The `NpcAttachment` connects the NPC template to its visual prefab and other visual/story data.

# Stage 4 — Runtime dependencies to verify

The runtime initializer expects the fighting-style/visual chain to be coherent.

The NPC initialization path adds and initializes systems including:

- health/death/damage;
- movement;
- NPC stats;
- inventory/items;
- body/clothing features;
- AI;
- animation state machines;
- weapon handling where supported.

A missing fighting style is treated by the source as a game-breaking configuration error. That makes fighting-style duplication/wiring a required part of the practical creature pipeline, not optional cleanup.

# Recommended modder workflow

For a first custom creature:

1. start from a vanilla/toolkit NPC spec that is behaviorally close to the desired creature;
2. duplicate it through **Prepare NPC Spec** rather than manually copying random files;
3. prepare the new visual through **Prepare NPC Prefab**;
4. inspect every warning produced by the preparation tool;
5. correct real head/torso/root/ragdoll/collider placement;
6. keep the duplicated fighting style initially;
7. tune the duplicated `NpcTemplate` in small steps;
8. prove spawn/idle/movement first;
9. then prove combat/weapon/loot/death behavior;
10. only after that introduce custom fighting-style or animation changes.

This minimizes the number of unknowns in one test.

# What this document does not claim

This documentation pass did not launch Unity or FoA.

Therefore:

- visual-prep path: **STATIC_CONFIRMED**;
- NPC-spec duplication/wiring: **STATIC_CONFIRMED**;
- fresh editor execution: **NOT_RUN**;
- fresh FoA spawn/combat runtime execution: **NOT_RUN**.

If a local run is later recorded, add the exact Merlin commit, game build, test creature, actions exercised and observed result rather than replacing these states with a vague "works."

## Merlin source anchors

- `Assets/Code/Editor/Prefabs/PrepareNPCPrefab.cs`
- `Assets/Code/Editor/Prefabs/PrepareNPCSpec.cs`
- `Assets/Code/Main/Fights/NPCs/NpcAttachment.cs`
- `Assets/Code/Main/Fights/NPCs/NpcTemplate.cs`
- `Assets/Code/Main/AI/NpcData.cs`
- `Assets/Code/Main/Fights/NPCs/NpcInitializer.cs`
