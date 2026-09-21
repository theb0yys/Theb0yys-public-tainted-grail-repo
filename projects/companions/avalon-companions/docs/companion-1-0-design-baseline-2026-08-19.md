# Avalon Companions 1.0 Design Baseline — Creature and Animal Companions

Status: owner-corrected design baseline for research and planning  
Date corrected: 2026-08-19  
Branch: `companions`  
Owner: `mods/avalon-companions/`  
Implementation: `NOT_RUN`  
Runtime validation: `NOT_RUN`

## Scope correction

This document applies the ecosystem's existing **one system, one truth** rule. It does not create a new shared companion platform or transfer authority between systems.

The controlling ownership boundaries are:

```text
Avalon AI owns AI.
The Dialogue Framework owns dialogue.
Avalon Human Companions owns human companions.
Avalon Companions owns creature and animal companions.
```

The shared `companions` branch is a coordination lane only. It does not make either companion mod the owner of the other mod's state, behaviour, dialogue, progression, persistence, UI, or implementation.

Previous wording in this file that described Avalon Companions as a universal human/animal companion platform is withdrawn.

## Avalon Companions product direction

Avalon Companions 1.0 is the owner of the creature and animal companion domain.

It owns only the creature/animal companion facts and mechanics that belong to this mod, including:

- creature and animal companion identity;
- the creature/animal roster;
- active creature/animal companion state;
- acquisition eligibility and acquisition state;
- taming and creature-binding outcomes;
- exact runtime actor binding for owned creature/animal companions;
- creature/animal lifecycle and reconciliation;
- creature/animal relationship, bond, temperament and training state;
- creature/animal progression, competence and speciality state;
- creature/animal commands and semantic capabilities;
- creature/animal camp, home, waiting, injured, unavailable and death states;
- creature/animal persistence and schema migration;
- the player-facing creature/animal companion management experience.

It does not own human companions, human recruitment, human progression, human persistence, human dialogue, or human combat development. Those belong to Avalon Human Companions.

It does not own AI. Creature/animal AI is supplied through an Avalon AI package and the AI framework's existing ownership, observation, planning and execution contracts.

It does not own dialogue. Creature/animal dialogue and interaction presentation use the Dialogue Framework's existing service. Avalon Companions supplies companion-domain context, available semantic actions and validated result handlers.

## AI integration boundary

Avalon Companions may expose an animal/creature AI package containing owner-specific observations, capabilities, goals, semantic action vocabulary and execution adapters.

The package is consumed and governed by Avalon AI. Avalon Companions must not grow another independent planner, blackboard, dialogue-intent engine, global scheduler or competing actor-control system.

Conceptual boundary:

```text
Avalon Companions
  -> publishes creature/animal identity, state and capabilities
  -> supplies an Avalon AI package

Avalon AI
  -> owns observation, memory, goal selection, planning, coordination and action admission
  -> returns approved semantic actions

FoA/native execution
  -> performs only proven bounded physical operations
```

Native FoA movement, navigation, unconsciousness, ragdoll, animation, hit, damage, perception-reset and interaction primitives may be used when proven. They do not become the decision owner.

## Dialogue integration boundary

Avalon Companions uses the Dialogue Framework service rather than implementing a dialogue engine or dialogue UI.

Avalon Companions supplies bounded creature/animal context such as:

- companion identity;
- species/provider;
- relationship and bond;
- temperament;
- relevant memories owned by this companion system;
- current condition;
- acquisition or training state;
- available semantic actions.

The Dialogue Framework owns conversation lifecycle, presentation, authored/generated dialogue composition, choices, conditions, voice, localisation, focus and input handling.

Dialogue results return through a semantic boundary. Avalon Companions validates and applies only creature/animal domain effects that it owns.

## Acquisition direction

Creature and animal companions should be acquired from actors that exist in the world rather than selected from a spawn menu.

The target lifecycle is:

```text
world creature/animal
  -> acquisition eligibility
  -> subdual opportunity
  -> native unconscious physical state
  -> provider-specific interaction
  -> accepted or rejected outcome
  -> durable creature/animal companion identity on success
  -> bind that exact actor where possible
```

Later actor reconstruction after a transition or load is reconciliation of an existing creature/animal companion identity, not a new acquisition event.

## Hunter and bounty progression

Acquisition should be introduced through controlled hunter and notice-board progression.

Preferred first route:

```text
Hunter introduction
  -> ordinary wolf bounties
  -> demonstrate hunting competence
  -> capture-alive tutorial
  -> receive or borrow subdual bow and arrows
  -> exact quest-scoped wolf becomes acquisition-enabled
  -> native unconscious state
  -> meat/bonding interaction through the Dialogue Framework service
  -> wolf capability unlocked
```

During the tutorial, activation must be narrow: exact quest stage, exact projectile identity, exact eligible target or quest marker, and explicit failure/cleanup behaviour.

After completion, a durable Avalon Companions capability may broaden wolf or animal acquisition without leaving tutorial logic globally active.

## Subdual bow and arrows

The acquisition weapon should be a mod-owned clone of one proven native FoA bow/ammunition path.

The source path must first be traced and validated through:

```text
inventory
  -> equip
  -> aim/draw
  -> fire
  -> projectile
  -> impact
  -> shooter attribution
  -> target attribution
  -> cleanup
```

Health and subdual are separate semantic channels.

```text
health <= 0       -> death
subdual threshold -> acquisition unconsciousness
```

Subdual threshold calculation is deterministic companion-domain logic. Avalon AI does not decide whether a projectile physically reached the threshold.

## Native unconscious backbone

Existing static research identifies `UnconsciousElement` as the strongest candidate for the physical knockout state.

Avalon Companions owns creature/animal acquisition state. Avalon AI owns reasoning about the resulting situation. FoA supplies the proven physical unconscious transition and recovery primitives.

Runtime compatibility remains unproven and must be established separately for:

1. ordinary wolf;
2. ordinary bear;
3. one passive animal;
4. the first intended Wyrd/creature family.

Unique, boss, story-critical, undead and special-lifecycle actors remain blocked by default.

## Creature and animal acquisition providers

### Ordinary animals

Animal interaction is behavioural rather than fake human conversation.

Relevant state can include:

- fear;
- hunger;
- temperament;
- injury;
- confidence;
- previous encounters;
- violence used during subdual;
- food preference;
- nearby danger;
- existing bond or memory.

Provider-specific options may include approach, leave food, calm, treat wounds, back away, release or attempt bonding.

### Wyrd and supernatural creatures

Wyrd creatures remain owned by Avalon Companions as creature companions, but they require distinct provider semantics.

They may require specialised subdual ammunition, Wyrd offerings, creature-specific resources or other supernatural conditions. They must not be implemented as ordinary animals with only larger resistance numbers.

### Human boundary

Human capture, hiring, persuasion, recruitment and human progression are outside this owner's scope. Any analogous human flow belongs to Avalon Human Companions and uses its own domain state while consuming the same AI and dialogue framework services.

Avalon Companions may provide proven-path evidence to the human owner. It must not own or mutate human companion truth.

## Creature and animal identity and persistence

Successful creature/animal acquisition creates a durable identity owned by Avalon Companions.

Conceptual durable state includes:

- companion ID;
- provider/species/archetype;
- source template and origin context;
- name;
- temperament;
- relationship and bond;
- significant creature/animal memories;
- progression and competence;
- speciality or role;
- acquisition origin;
- roster and assignment state;
- player-approved behaviour preferences;
- life/injury/death state where designed.

Transient FoA state must be re-observed or reconstructed rather than blindly serialized:

- current target;
- current path;
- current animation;
- current native action;
- nearby threat lists;
- temporary AI/planner state;
- native runtime handles.

## Progression and competence

Creature and animal progression must change capability, not only numbers.

Progression may affect:

- health, resilience and recovery;
- movement/agility capabilities where proven;
- threat judgement;
- disengagement;
- target priority;
- flanking;
- guarding;
- pack or party coordination;
- action repertoire;
- command interpretation;
- autonomy.

Working competence tiers remain:

1. Novice
2. Trained
3. Veteran
4. Elite
5. Mastered

These are design labels, not fixed final thresholds.

A novice wolf should have a narrow animal AI package capability set. A highly trained wolf should expose richer animal actions to Avalon AI, such as advanced repositioning, disengagement, protection, pack coordination and validated advanced attacks.

## Vanilla animation milestones

Avalon AI selects semantic actions. It does not select raw clip names.

```text
animal/creature capability unlocked
  -> Avalon AI selects semantic action
  -> animal/creature binding resolver chooses a proven compatible native motion
  -> FoA/native executor performs the bounded physical action
  -> effect and cleanup remain with their proven owners
```

The first animation/competence proof should cover one wolf. Later Wyrd or specialist creatures require family-specific rig and action evidence.

Human animation progression is outside this owner and belongs to Avalon Human Companions.

## Bonfire progression and management

Avalon Companions owns its creature/animal progression data and management workflow. It may expose a creature/animal companion entry from the bonfire/campfire context using the ecosystem's established UI services and proven FoA entry path.

The screen may show:

- creature/animal roster;
- selected companion;
- health/status;
- bond and temperament;
- level and competence;
- animal/creature speciality;
- training milestones;
- important history;
- assignment and availability;
- unlocked behaviour permissions.

Deep training or bonding conversations are launched through the Dialogue Framework service. The progression screen itself must not become a second dialogue system.

## Proven-path inputs

Before implementation, inspect existing owner-specific evidence:

- `mods/avalon-companions` for creature/animal roster, commands, lifecycle, taming diagnostics, progression scaffolding and AI package boundaries;
- `mods/avalon-broodmother-companion` for specialist creature lifecycle, death, damage and interaction evidence;
- `mods/avalon-ai-runtime` only to consume its existing package and owner contracts;
- the Dialogue Framework only to consume its existing companion dialogue service;
- `mods/immersive-progression` for bonfire entry evidence;
- FoA static/runtime evidence for bow, projectile, quest and actor identity paths.

Avalon Human Companions is a parallel owner, not an implementation layer inside Avalon Companions.

## Implementation sequence

1. Inspect the existing Avalon AI package contract used by Avalon Companions; do not redesign the AI framework.
2. Inspect the existing Dialogue Framework companion service; do not create a local dialogue engine.
3. Map one exact native bow/ammunition/projectile path.
4. Complete runtime unconscious proof for one ordinary wolf.
5. Map the exact hunter/notice-board/quest-stage path.
6. Build one quest-scoped wolf acquisition using existing framework services.
7. Bind that exact wolf to a durable Avalon Companions identity.
8. Prove same-session lifecycle, rejection and recovery.
9. Add persistence and transition reconciliation.
10. Add wolf progression and bonfire management.
11. Broaden within Avalon Companions: wolf -> bear -> passive animals -> first Wyrd family -> specialist creatures.

## Explicit non-goals

This baseline does not authorize:

- human companion implementation in Avalon Companions;
- a universal cross-owner companion profile or platform;
- AI framework redesign inside this mod;
- dialogue framework redesign inside this mod;
- a duplicate local planner, blackboard or dialogue engine;
- global unconscious, quest, faction, actor or interaction mutation;
- unique, boss, story-critical or undead acquisition;
- implementation, deployment or release without the required static and runtime evidence.

## Current status

```text
owner scope correction = PASSED
design baseline = CURRENT_FOR_PLANNING
AI package inspection = NOT_RUN
Dialogue Framework service inspection = NOT_RUN
bow/projectile static proof = NOT_RUN
wolf unconscious runtime proof = NOT_RUN
quest/hunter path proof = NOT_RUN
wolf acquisition implementation = NOT_RUN
persistence = NOT_RUN
bonfire progression = NOT_RUN
release = NOT_RUN
```

## Next researched task

Inspect the existing Avalon Companions AI package boundary and the existing Dialogue Framework companion service needed by the first wolf acquisition slice. Record only the animal/creature data, calls and semantic results Avalon Companions must provide or consume; do not alter either framework and do not define a cross-owner companion core.
