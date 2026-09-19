# Creatures and Actors

Document type: **domain system hub**.

Creature work spans source assets, animation, templates, runtime actor ownership, AI/combat/death, spawning/population, cleanup and persistence. Those are separate contracts.

## Domain map

```text
source / visual
→ native baseline
→ animation mapping
→ NpcTemplate / LocationTemplate
→ runtime Location / NpcElement
→ AI / combat / movement / presentation
→ death / NpcDummy / Corpse
→ cleanup
→ separate placement/population and persistence
```

## Canonical chapters

### Understand the runtime actor

- [Location and session actor lifecycle](../actors/location-and-session-lifecycle.md)

### Build a custom creature

- [Custom creature injection](../../mechanics/creatures/custom-creature-injection.md)

### Spawn or own population

- [Spawning and population](../../mechanics/actors/spawning-and-population.md)

### Learn in sequence

- [Creatures content-authoring journey](../../learn/content-authoring/creatures/README.md)

## Core boundaries

- visual success is not actor success;
- template success is not runtime actor success;
- attack success is not locomotion/death proof;
- one-session `SpawnLocation` is not persistent population authority;
- cleanup must preserve native death/corpse handoff;
- each creature keeps its own readiness/evidence state.

## Proof boundary

The process shape is reusable. Individual creature readiness is per creature and per stage; persistent population and persistence remain separate proof problems.
