# Critter Gameplay vs VAT Visual Entity

Use this page when working with critter animation/rendering and you need to know which object owns gameplay.

Canonical overview: [Critter VAT / ECS Integration](README.md).

## Split ownership

The useful model is:

```text
gameplay/controller proxy
→ movement / damage / death / loot
        │
        └─ visual-state handoff
              ↓
        VAT animation state
              ↓
        Drake/ECS visual entity
              ↓
        shader deformation
```

The VAT/ECS entity is presentation output.

## Do not put gameplay state on the visual entity

Avoid treating the visual entity as authoritative for:

- health;
- loot;
- spawn ownership;
- damage;
- audio;
- persistent actor identity.

Those belong to the gameplay/controller side.

## Animation changes

A critter animation intervention should establish:

1. which gameplay/controller transition selects the animation;
2. which VAT animation state is published;
3. which visual entity consumes it;
4. what happens on transition/death/despawn;
5. whether the entity is recreated while the gameplay owner survives.

## Why this differs from Kandra

Critter VAT is not the normal humanoid skinned-character path.

Do not port Kandra armour/rig assumptions into VAT critter work merely because both end in GPU-driven rendering.
