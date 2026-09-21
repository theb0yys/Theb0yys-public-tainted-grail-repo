# Companion 1.0 Implementation Architecture — Deep Research Intake Review

Document control:
- Date: 2026-08-19
- Review status: `REVIEWED_FOR_PLANNING_ONLY_WITH_OWNER_SCOPE_CORRECTION`
- Research state: `reviewed` for bounded creature/animal planning context; not `current`
- Owner: `mods/avalon-companions/`
- Source record: `companion-1-0-implementation-architecture-deep-research-source-2026-08-19.md`
- Cleaned derivative: `companion-1-0-implementation-architecture-deep-research-cleaned-2026-08-19.md`
- RH1 claim evaluation: `PASSED`
- RH2 domain review: `PASSED_WITH_OWNER_CORRECTION`
- RH3 independent review: `NOT_RUN`
- RH4 validation review: `NOT_RUN`
- RH5 human promotion: `NOT_PROVIDED`
- Implementation/runtime/save authority: none

## Owner correction

The returned report and the first review incorrectly treated Avalon Companions as a universal platform for animal and human companions and treated AI/dialogue framework internals as Companion 1.0 design responsibilities.

The repository owner corrected that premise with the ecosystem's existing **one system, one truth** rule:

```text
Avalon AI owns AI.
The Dialogue Framework owns dialogue.
Avalon Human Companions owns human companions.
Avalon Companions owns creature and animal companions.
```

The shared `companions` branch is a coordination lane only. It does not merge ownership.

Any report claim that conflicts with those boundaries is rejected for planning and implementation use.

## Corrected review question

Which findings from the report remain useful for Avalon Companions' creature and animal companion overhaul when AI is consumed through an Avalon AI package, dialogue is consumed through the Dialogue Framework service, and human companions remain owned by Avalon Human Companions?

## Findings retained

### 1. Native unconscious is the strongest first creature acquisition proof

The report correctly uses the existing static `UnconsciousElement` research as the leading physical knockout candidate.

Retained use:

- plan one wolf runtime proof first;
- preserve native physical unconsciousness where safe;
- keep acquisition decision state in Avalon Companions;
- keep AI reasoning in Avalon AI;
- block unique, boss, story-critical, undead and unsupported actor families by default.

Runtime compatibility remains `NOT_RUN`.

### 2. The subdual bow should clone one proven native bow/ammunition path

Retained use:

- map one exact native inventory/equip/draw/fire/projectile/impact/attribution path;
- create mod-owned bow/ammo identities;
- leave source templates unchanged;
- keep health and subdual separate;
- require exact projectile identity and target attribution.

No bow/projectile path is yet proven.

### 3. Hunter and notice-board progression is the correct first acquisition introduction

Retained first slice:

```text
wolf bounties
  -> capture-alive tutorial
  -> exact quest stage
  -> exact quest-marked wolf
  -> subdual bow/ammo
  -> unconscious
  -> creature interaction through Dialogue Framework
  -> creature-domain result in Avalon Companions
```

This keeps early acquisition logic narrow and testable.

### 4. Exact actor continuity remains central

The encountered creature should become the companion where possible.

Later reconstruction is persistence/reconciliation for the same Avalon Companions identity, not a second acquisition event.

### 5. Creature progression should unlock capability

The report's capability-first progression model remains useful for creatures and animals:

- physical competence;
- tactical capability declarations;
- speciality;
- relationship/training gates;
- semantic action repertoire;
- compatible vanilla animation bindings.

The creature/animal AI package exposes allowed capabilities to Avalon AI. Avalon Companions does not become the planner.

### 6. Bonfire is the correct deep creature/animal management context

The report correctly separates field interaction from deep management.

Avalon Companions may own its creature/animal progression screen and data while using ecosystem UI services and the Dialogue Framework for conversations/training interactions.

The exact bonfire entry and focus lifecycle remain unproven.

## Findings rejected or rerouted

### Rejected — Avalon Companions as universal companion platform

Disposition: `REJECTED`

Avalon Companions does not own human identity, recruitment, progression, persistence or behaviour.

### Rerouted — human capture/recruitment design

Disposition: `OUT_OF_SCOPE_FOR_AVALON_COMPANIONS`

All human companion design belongs to Avalon Human Companions. The report may be used there only after that owner separately reviews it.

### Rerouted — selection of AI technologies

Disposition: `OUT_OF_SCOPE_FOR_AVALON_COMPANIONS`

Rabbit, GOAP, PlayMaker, Blaze and other AI internals are owned by Avalon AI. Avalon Companions consumes the AI framework through an AI package and existing service contracts.

Avalon Companions must not create substitute abstractions for those internal framework decisions unless the AI owner explicitly requests them.

### Rerouted — selection of dialogue technologies

Disposition: `OUT_OF_SCOPE_FOR_AVALON_COMPANIONS`

Pixel Crushers, Convai and other dialogue technology choices are owned by the Dialogue Framework. Avalon Companions consumes the companion dialogue service.

The companion mod only supplies owned context, content and semantic result handlers.

### Rerouted — framework licensing and deployment

Disposition: `OWNER_FRAMEWORK_CONCERN`

Licensing and packaging of AI/dialogue dependencies are handled by the framework that owns and distributes those dependencies. Avalon Companions only needs to know the supported service/package contract it consumes.

This report does not authorize or block those frameworks.

### Rejected — universal cross-owner `CompanionProfile`

Disposition: `REJECTED`

Avalon Companions owns its creature/animal companion state. Avalon Human Companions owns its human companion state. This review does not define a new cross-owner profile or persistence database.

## Corrected responsibility map

```text
Avalon Companions
  creature/animal acquisition
  creature/animal identity and roster
  creature/animal relationship and progression
  creature/animal persistence/reconciliation
  creature/animal capability declarations
  creature/animal management

Avalon AI
  AI ownership
  memory/observation
  goal selection and planning
  coordination
  semantic action admission
  cancellation and execution routing

Dialogue Framework
  dialogue lifecycle and UI
  authored/generated dialogue composition
  choices, conditions, voice and localisation
  focus, cursor and input ownership
  semantic dialogue result service

Avalon Human Companions
  all human companion truth and mechanics

FoA/native systems
  proven bounded physical execution only
```

## Corrected dependency order

1. Inspect the existing Avalon Companions AI package contract; do not redesign Avalon AI.
2. Inspect the existing Dialogue Framework companion service; do not select or integrate dialogue technologies locally.
3. Map one exact FoA bow/ammo/projectile path.
4. Complete one wolf unconscious runtime proof.
5. Map the hunter/notice-board/quest-stage path.
6. Build one quest-scoped wolf acquisition using the existing AI package and dialogue service.
7. Create and bind one durable Avalon Companions creature identity.
8. Prove rejection, recovery, lifecycle and same-session continuity.
9. Add persistence/reconciliation.
10. Add creature/animal bonfire progression and wolf competence.
11. Broaden only within Avalon Companions' owner scope.

## Claim disposition summary

| Claim | Corrected disposition |
|---|---|
| Avalon Companions owns a universal human/animal platform | `REJECTED` |
| Avalon Companions owns creature and animal companions | `ACCEPTED OWNER SCOPE` |
| Avalon Human Companions owns human companions | `ACCEPTED OWNER SCOPE` |
| Avalon Companions should implement AI logic | `REJECTED`; use AI package/framework |
| Avalon Companions should implement/select dialogue stack | `REJECTED`; use Dialogue Framework service |
| Native unconscious is the first physical acquisition candidate | `KEEP_FOR_RUNTIME_PROOF` |
| Mod-owned cloned bow/ammo path | `KEEP_FOR_STATIC_AND_RUNTIME_PROOF` |
| Hunter/notice-board wolf tutorial | `KEEP_FOR_PLANNING` |
| Exact captured wolf becomes the companion | `KEEP_FOR_PLANNING` |
| Creature competence unlocks AI capabilities and animation bindings | `KEEP_FOR_PLANNING` |
| Human capture/recruitment architecture in this mod | `OUT_OF_SCOPE` |

## Review state

```text
source preserved = PASSED
cleaned derivative preserved = PASSED
owner scope correction = PASSED
RH1 = PASSED
RH2 = PASSED_WITH_OWNER_CORRECTION
RH3 = NOT_RUN
RH4 = NOT_RUN
RH5 = NOT_PROVIDED
implementation = NOT_RUN
runtime proof = NOT_RUN
```

## Required next task

Inspect the current Avalon Companions AI package boundary and the current Dialogue Framework companion service entrypoints needed by the first wolf acquisition slice. Produce a call/data map limited to what Avalon Companions publishes or consumes. Do not modify either framework and do not define human companion state or a universal companion core.
