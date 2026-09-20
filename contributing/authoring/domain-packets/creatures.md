# Domain Packet — Creatures and Actors

## Subject

Creature source assets, native baseline, visual transport, animation mapping, `NpcTemplate`/`LocationTemplate`, runtime actor construction, AI/combat/death/corpse, placement/population, cleanup and persistence.

## Reader questions

1. Why is a creature not just a prefab with AI?
2. Which native objects own a runtime actor?
3. What does each stage of the custom-creature process actually prove?
4. How is a controlled session-only actor different from persistent population?
5. How must cleanup respect native death/corpse handoff?
6. Why must per-creature readiness remain independent?

## Archetypes required

- **System hub:** actor/creature ownership map.
- **System:** one-session `Location`/actor/death lifecycle.
- **Mechanic:** staged custom-creature injection.
- **Mechanic/system:** spawning/population, kept separate from one-session actor proof.
- **Case:** visual/animation/controller/death corrections.
- **Learning journey:** creature content path.
- **Reference/evidence:** per-creature readiness remains per subject.

## Canonical native owners

- `NpcTemplate`
- `LocationTemplate`
- `Location`
- `NpcElement`
- native movement/AI/combat owners
- `DeathElement`, `NpcDummy`, `Corpse`
- native spawner/population systems for durable population.

## Required prerequisites

- [Native object ownership](../../../systems/core/native-object-ownership.md)
- scene/service lifecycle
- asset/resource lifetime
- [Saving and persistence](../../../systems/persistence/README.md)

## Public source set

- `docs/reference/CREATURES_NPCS.md`
- `docs/reference/ACTORS_LOCATIONS_SPAWNING.md`
- `docs/reference/SPAWNING_ENCOUNTERS.md`
- `docs/reference/CREATURES_NATIVE_PROCESS.md` (legacy shim only)
- related AI/animation/assets/death pages.

## Private-evidence conclusions to preserve

- visual transport, animation mapping, template validity, actor construction, combat/death, population and persistence are independent claims;
- one-session companion/actor routes resolve exact templates, create a `Location`, establish not-saved policy, preserve native owners and explicitly discard on teardown;
- native death acceptance may require waiting for the `NpcDummy` + `Corpse` handoff rather than destroying the location early;
- project resolver APIs are provider contracts, not native FoA APIs;
- the staged creature process is reusable, while individual creature readiness varies;
- persistent population requires a different native owner and proof matrix than a disposable one-session actor.

## Claims to teach

- visual success is not actor success;
- template success is not runtime actor success;
- attack/combat success is not locomotion/death proof;
- one-session spawn is deliberately not population proof;
- cleanup can destroy valid native death state if performed too early;
- native baseline selection must be contract-based, not visual resemblance;
- each creature keeps its own evidence status.

## Failure/correction history to preserve

- headless graphics does not prove rendered/Kandra acceptance;
- generic renderer/shader assumptions can fail native skinning;
- collider geometry alone does not complete controller/grounding contracts;
- missing animation states/root-motion policy can produce working attacks but broken locomotion/death;
- cleanup that discards the living location too early can erase native dummy/corpse handoff;
- plausible safety guards can be wrong when they do not match the actual native consumer.

## Required diagrams

1. **Ownership:** template → location → NPC element → AI/combat/death/presentation owners → corpse/cleanup.
2. **Process:** source → baseline → visual → animation → template → runtime actor → focused live proof.
3. **Population split:** one-session `SpawnLocation` path versus persistent spawner/population ownership.
4. **Death lifecycle:** alive actor → death owner → dummy/corpse → cleanup.

## Worked-example candidate

Future cases should focus on failure/correction lessons rather than publishing private creature assets or source packages.

## Evidence lanes

- process shape: strong repeated evidence;
- individual creature readiness: per-creature/per-stage;
- one-session actor lifecycle: strong bounded evidence;
- persistent population: separate;
- save/persistence: separate.

## Canonical target map

- `systems/creatures/README.md`
- `systems/actors/location-and-session-lifecycle.md`
- `mechanics/creatures/custom-creature-injection.md`
- `mechanics/actors/spawning-and-population.md` — later move of spawning page after role review.
- `learn/content-authoring/creatures/README.md`

## Completion checks

- gate/stage names always explain what each stage proves and does not prove;
- one-session actor lifecycle is not presented as persistent spawn authority;
- native death/corpse handoff is preserved;
- per-creature evidence remains independent;
- public docs do not expose source assets or private implementation code.
