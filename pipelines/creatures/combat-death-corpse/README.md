# Creature Stage — Combat, Death, and Corpse Handoff

## Objective

Prove the controlled actor participates correctly in native movement/combat/death ownership and does not destroy the native corpse transition through bad cleanup.

## Validate independently

- grounded movement/controller behaviour;
- target recognition;
- combat entry;
- each required attack;
- damage dealt/received;
- hit reaction;
- lethal transition;
- Death animation/state;
- NpcDummy / Corpse handoff;
- body-retention policy;
- cleanup after the native death lifecycle completes.

## Known lessons

A collider alone did not satisfy the controller grounding contract; CharacterGroundedData was a separate requirement.

An actor previously attacked successfully while locomotion dragged and death was broken because Movement/GetHit/Death animation states and root-motion/controller assumptions were incomplete.

Discarding the entire Location as soon as the living NpcElement disappeared deleted the native same-Location NpcDummy/Corpse handoff. Cleanup must respect death ownership.

## Check

Confirm that the claimed combat/death rows pass without bypassing the native actor lifecycle.

## Before continuing

World population, respawn, persistent placement, save/load, or companion UI/commands.

## Next

Proceed to [placement and population](../population-placement/README.md) only when the controlled actor gate is stable.
