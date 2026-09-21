# Creature Importation Pipeline

This pipeline reconstructs the evidence-backed custom-creature process as explicit gates. A creature is not a prefab with AI attached; source content, native baseline, visuals, animation, templates, runtime actor lifecycle, combat/death, population, cleanup, and persistence are separate claims.

## Canonical gate sequence

CI1 → CI2 → CI3 → CI4A → CI4 → CI5 → focused live validation

## Stage map

1. [CI1 — Source intake](ci1-source-intake/README.md)
2. [CI2 — Native baseline](ci2-native-baseline/README.md)
3. [CI3 — Visual transport](ci3-visual-transport/README.md)
4. [CI4A — Animation mapping](ci4a-animation-mapping/README.md)
5. [CI4 — NpcTemplate and LocationTemplate contract](ci4-template-contract/README.md)
6. [CI5 — Runtime actor lifecycle](ci5-runtime-lifecycle/README.md)
7. [Combat, death, and corpse handoff](combat-death-corpse/README.md)
8. [Placement and population](population-placement/README.md)
9. [Persistence and cleanup](persistence-cleanup/README.md)
10. [Validation matrix](validation/README.md)
11. [Known failure modes](failures/README.md)

## Canonical technical background

- [Creatures and NPCs](../../knowledge/systems/gameplay/creatures-npcs.md)
- [Actors, locations, and spawning](../../knowledge/systems/gameplay/actors-locations-spawning.md)
- [Kandra presentation](../../knowledge/systems/presentation/kandra/README.md)
- [Temporary human ally runnable example](../../examples/mono/gameplay/temporary-human-ally/README.md)

## Core rule

Every gate proves only its own boundary. A visual root does not authorise actor construction. A valid NpcTemplate does not prove combat. A spawned actor does not prove population or persistence.
