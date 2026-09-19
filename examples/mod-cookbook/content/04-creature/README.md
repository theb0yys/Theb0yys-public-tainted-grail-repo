# Content Example — First Creature / NPC

**Authoring path:** Merlin Workshop  
**Evidence:** STATIC_CONFIRMED  
**This public recipe:** NOT_RUN

A creature is not a good first-ever content mod. Use this after items/equipment make sense.

## Goal

Make a new creature/NPC using a mod-owned visual while reusing a behaviorally close known-good NPC chain.

## Stage 1 — visual prefab

Run:

~~~text
TG -> Assets -> Prefabs -> Prepare NPC Prefab
~~~

The selected prefab needs an Animator.

The preparation route expects/creates the structures around:

- ARNpcAnimancer;
- root motion;
- AnimatorClipPlayer;
- KandraRenderer;
- rig/root bone;
- ragdoll/root review;
- AlivePrefab walk-through collision;
- VFX renderer path;
- RootBone, Head and Torso markers.

Treat generated placeholder head/torso/collider/ragdoll choices as things to correct, not final authoring.

## Stage 2 — NPC spec

Run:

~~~text
TG -> Assets -> Prefabs -> Prepare NPC Spec
~~~

Use a known-good base NPC whose behavior is close to your intended creature.

The preparation route duplicates/rewires the important chain:

~~~text
NPC spec
 -> logic prefab
 -> your prepared visual
 -> duplicated NpcTemplate
 -> duplicated fighting style
 -> duplicated base animation data
 -> duplicated base behavior data
~~~

## Stage 3 — start with boring gameplay values

NpcTemplate exposes, among other things:

- level;
- maxHealth;
- maxStamina;
- stamina regen/usage;
- melee/ranged/magic damage;
- armor;
- damage received multipliers;
- poise/force/block;
- inventory and loot;
- tags;
- audio/surface;
- VFX;
- fighting style.

For the first test, keep the duplicated fighting style and most base values.

## Test order

1. spawn/resolve;
2. idle;
3. movement/pathing;
4. collision;
5. animation;
6. combat;
7. damage/death;
8. loot;
9. only then custom fighting-style/animation behavior.

See docs/pipelines/CREATURES_KANDRA.md.
