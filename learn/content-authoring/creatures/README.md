# Content Authoring Journey — Creatures

Creature content is a staged ownership problem, not “import prefab and add AI”.

## Prerequisites

1. [Native object ownership](../../../systems/core/native-object-ownership.md)
2. [Location and session actor lifecycle](../../../systems/actors/location-and-session-lifecycle.md)
3. [Saving and persistence](../../../systems/persistence/README.md)

## Journey

### 1. Understand the runtime actor

Read [Location and session actor lifecycle](../../../systems/actors/location-and-session-lifecycle.md).

Be able to explain the relationship between:
- `LocationTemplate`;
- `Location`;
- `NpcElement`;
- native AI/combat/movement owners;
- `DeathElement`, `NpcDummy`, and `Corpse`;
- cleanup and save policy.

### 2. Follow the staged creature mechanic

Read [Custom Creature Injection](../../../mechanics/creatures/custom-creature-injection.md).

Each stage proves only its own contract.

### 3. Keep one-session actor proof separate from population

Read [Spawning and Population](../../../mechanics/actors/spawning-and-population.md).

A disposable not-saved `Location` proof is intentionally not durable ambient-spawn authority.

### 4. Preserve correction history

When a visual, collider, animation or death path fails, identify the earliest failed owner/stage rather than broadening the patch.

## Completion outcome

You should be able to explain:

```text
visual ≠ animation ≠ template ≠ runtime actor ≠ combat/death ≠ population ≠ persistence
```

and identify which owner/evidence lane is responsible for each.
