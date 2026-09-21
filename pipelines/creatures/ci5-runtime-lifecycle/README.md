# Creature CI5 — Runtime Actor Lifecycle

## Objective

Construct one controlled actor and hand ownership to the native runtime systems while keeping the proof disposable.

## Controlled sequence

    resolve exact LocationTemplate
    → SpawnLocation
    → immediately apply session/save policy
    → validate spawned Location/NpcElement
    → preserve native controller/AI/combat ownership
    → retain exact spawned Location identity
    → explicit cleanup

## Procedure

1. Use a disposable save/session.
2. Resolve the exact pack-owned template identities.
3. Spawn one controlled actor at a safe, known location.
4. Mark session-only proof actors not saved immediately when persistence is not the current gate.
5. Validate expected NpcElement/controller/presentation components before use.
6. Fail closed if required components are missing.
7. Retain the exact Location handle for cleanup.
8. Do not use a story/unique template as a disposable proof actor.
9. Do not promote this single controlled actor into ambient population.

## Check

Confirm that one actor is constructed through the intended native Location/Npc ownership with correct save policy and deterministic cleanup identity.

## Before continuing

Movement quality, combat, death/corpse, population, persistence, or companion behaviour.

## Next

Proceed to [combat, death, and corpse handoff](../combat-death-corpse/README.md).
