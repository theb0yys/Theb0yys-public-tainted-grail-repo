# Creature Importer Pipeline

Avalon Awakened owns the custom-creature import/provider path. The mature part of this work is the **gate sequence**: source assets, native behaviour, visuals, animation, templates, runtime lifecycle, and live behaviour are proved separately.

## Current status

The creature process is an evidence-backed operating guide. It has been used to structure custom creature and one-session companion work.

That does not mean every creature package is automatically supported. Each creature still needs its own source, native baseline, animation map, template identities, runtime lifecycle, and live validation.

## Pipeline

~~~text
CI1 source intake
→ CI2 suitability + native baseline
→ CI3 visual transport
→ CI4A animation mapping
→ CI4 template + actor contract
→ CI5 runtime lifecycle
→ CI5 live gate
→ optional companion/gameplay consumer
~~~

## CI1 — Source intake

Record the source before authoring:

- source package and hashes;
- licence/redistribution status;
- Unity version where relevant;
- prefabs, meshes, skinned meshes, skeleton/root;
- animation clips;
- materials/textures/audio;
- obvious blockers.

This proves what the source contains. It does not prove that FoA can use it.

## CI2 — Choose the native baseline

Choose the FoA baseline by behaviour and lifecycle evidence, not by visual resemblance.

Map the native creature family, faction, stats, controller/collider expectations, combat style, location/template structure, death/corpse behaviour, and known blockers.

The native baseline defines the behaviour contract the imported creature must fit.

## CI3 — Prove visual transport

Build only mod-owned/redistributable visual content through the intended Unity/Addressables route.

Validate at least:

- bundle/catalogue identity;
- hashes;
- explicit visual roots;
- missing-script state;
- shaders/materials;
- dependency closure;
- clean load and release across repeated cycles.

A clean visual load is **visual transport proof only**. It is not actor or template proof.

## CI4A — Map animations to FoA states

Map source clips to the exact FoA animation states required by the creature lifecycle.

The existing process treats state coverage as explicit data. Clips with no proven native state remain unmapped rather than being forced into a plausible-looking slot.

Validate the mapping asset independently before using it in a creature template.

## CI4 — Build the template and actor contract

Create mod-owned creature templates from the approved native baseline.

Record:

- custom NPC/template names and GUIDs;
- location/template identities;
- visual roots;
- animation mapping;
- behaviour/fighting-style references;
- scale policy;
- exact component/attachment order.

Validate the authored templates in isolated load/release tests.

At this point runtime spawn, combat, death, AI, loot, population, and save behaviour are still separate claims.

## CI5 — Wire the runtime lifecycle

Only after the template contract is established should runtime code resolve and use the custom creature.

The runtime should:

- verify exact identities before acting;
- fail closed on missing components;
- mark disposable proof actors as not saved when that is the selected policy;
- preserve native movement, target selection, attack selection, damage, death, and corpse handling where those paths are being reused;
- clean up failed or disabled actors predictably.

Source/build validation proves the implementation shape. It does not replace live behaviour evidence.

## CI5 live gate

Use a controlled test and prove the actual lifecycle:

- visible name and placement;
- native actor recognition;
- combat entry and attacks;
- damage and hit reaction;
- lethal damage and death animation;
- corpse/death handoff;
- duplicate prevention;
- cleanup;
- save-exclusion or persistence behaviour, whichever the design claims.

A live failure should reopen the smallest affected stage rather than trigger a broad rewrite.

## Provider versus gameplay consumer

Avalon Awakened owns the imported creature identity, templates, resolver, visual transport, and provider lifecycle.

Gameplay belongs in a separate consumer when appropriate:

~~~text
Avalon Awakened
  → creature identity + templates + resolver
  → standalone companion / encounter / hunt / quest consumer
~~~

Do not copy provider templates into each gameplay mod or invent raw fallback identities when the provider is missing.

## One-session companion route

For a session-only companion, add these requirements after the creature import itself:

- explicit player-owned summon/command route;
- one active instance;
- not-saved/disposable policy unless persistence is separately proved;
- native ally/faction integration;
- runtime-only companion interaction;
- clear UI/input/cursor ownership;
- dismiss/death/disable/shutdown cleanup;
- no raw-template fallback.

Companion UI success does not prove creature import success, and creature import success does not prove companion command/input behaviour.

## Per-creature record

Keep a compact record containing:

- source identity and redistribution status;
- selected native baseline;
- scale/display policy;
- animation-state map;
- template GUIDs/names;
- resolver/registration route;
- runtime owner;
- save and cleanup policy;
- build/load/release hashes;
- live checklist;
- explicit unproven claims.

This turns the next creature into a repeatable import instead of another one-off reverse-engineering project.
