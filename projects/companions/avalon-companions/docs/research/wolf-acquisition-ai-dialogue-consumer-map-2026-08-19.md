# Wolf Acquisition — Avalon AI Package and Dialogue Framework Consumer Map

Document control:
- Date: 2026-08-19
- Owner: `mods/avalon-companions/`
- Branch: `companions`
- Scope: first quest-scoped wolf acquisition slice
- Evidence class: repository source inspection and owner-boundary review
- Implementation: `NOT_RUN`
- Runtime validation: `NOT_RUN`

## Purpose

Map exactly what Avalon Companions currently publishes to Avalon AI, what Avalon AI currently returns, and what the first wolf acquisition slice will need to publish or consume without moving AI or dialogue responsibility into Avalon Companions.

This record does not redesign Avalon AI, define the Dialogue Framework API, implement a wolf acquisition feature, or create human companion state.

## Controlling ownership rule

```text
Avalon AI owns AI.
The Dialogue Framework owns dialogue.
Avalon Human Companions owns human companions.
Avalon Companions owns creature and animal companions.
```

The `companions` branch coordinates related work only. It does not combine owner authority.

## Sources inspected

### Avalon Companions AI owner and package

- `mods/avalon-companions/src/Framework/AvalonCompanionAiGameSystems.cs`
- `mods/avalon-companions/ai-owner/src/AvalonCompanions.AI.Owner.Contracts/CompanionAiOwnerContracts.cs`
- `mods/avalon-companions/ai-package/README.md`
- `mods/avalon-companions/ai-package/src/AvalonCompanions.AI.Package/AvalonCompanionAdvancedAiPackage.cs`
- `mods/avalon-companions/ai-package/src/AvalonCompanions.AI.Package.V2/AvalonCompanionCatchUpV2Contract.cs`
- `mods/avalon-companions/ai-package/src/AvalonCompanions.AI.Package.V2/AvalonCompanionCatchUpGoalPolicy.cs`
- `mods/avalon-companions/ai-package/src/AvalonCompanions.AI.Package.V2/AvalonCompanionsAiPackageV2.cs`

### Avalon AI consumption and ownership path

- `mods/avalon-ai-runtime/src/AvalonAI.FoAHost.Mono.V2/CompanionV2OwnershipLane.cs`
- `mods/avalon-ai-runtime/src/AvalonAI.Observation.FoA.Mono/FoACompanionObservationSource.cs`
- `mods/avalon-ai-runtime/src/AvalonAI.Blackboard.Rabbit.Companions/AvalonCompanionRabbitSchema.cs`

### Dialogue owner

- `mods/dialogue-overhaul/README.md`
- `mods/dialogue-overhaul/docs/research.md`
- current `mods/dialogue-overhaul/` repository tree on branch `companions`
- repository searches for `DialogueService`, `IDialogue`, and dialogue-overhaul runtime/service source

## Current Avalon AI boundary

### Existing owner contract

`IAvalonCompanionAiOwnerBoundary` currently exposes:

```text
TryAcquire(ownerId, out reason)
TryRelease(ownerId)
TryInspectActorId(ownerId, out actorRuntimeId)
TryCollect(ownerId, out AvalonCompanionOwnerObservation)
TryDispatch(ownerId, observationSequence, AvalonCompanionOwnerCommand)
```

The current V2 owner observation contains only:

```text
Sequence
ActorRuntimeId
HasActiveCompanion
FollowCatchUpCandidate
```

The current owner command contains only:

```text
CatchUpRecall
```

This is a clean one-system-one-truth boundary:

- Avalon Companions owns the live creature/animal facts and the actual native command adapter.
- Avalon AI acquires ownership, consumes a sequenced observation and chooses whether to request the declared command.
- The package does not call FoA or Unity directly.

### Existing V2 package

Current package identity:

```text
Package ID: kane.tgfoa.avalon-companions.ai-intent-profile
Package version: 0.3.0
Actor role: avalon-companions.managed-companion
Goal: avalon-companions.catch-up
Action: avalon-companions.catch-up-recall
Capability: native-companion-catch-up-recall
```

Current authoritative blackboard input:

```text
namespace: avalon.foa.companions
name: follow-catch-up-candidate
scope: Actor
value: bool
version: 1
```

Current planning facts:

```text
avalon-companions.catch-up-required
avalon-companions.catch-up-recall-dispatched
```

The V2 package therefore proves the intended package model, but only for one managed-companion catch-up action.

### Existing V2 host path

The current host composition performs this sequence:

```text
Avalon AI host
  -> acquire IAvalonCompanionAiOwnerBoundary
  -> inspect exact runtime actor ID
  -> create ActorId from FoA runtime identity
  -> create Rabbit blackboard with companion schema
  -> create FoA observation source
  -> register AvalonCompanionsAiPackageV2
  -> start Avalon Runtime V2
  -> acquire actor in NativeAssisted mode
  -> collect sequenced owner observation
  -> project blackboard facts
  -> GOAP selects declared action
  -> FoA companion executor calls the owner command boundary
```

The runtime retains exact actor identity and reports actor loss or actor change. It requires runtime stop and lease invalidation before ownership release.

This is the path the wolf acquisition package must follow. Avalon Companions must not add a parallel AI scheduler.

## Current advanced V1 package

The advanced V1 package already models a broader creature/animal observation:

```text
actor and template identity
scene and positions
distance to player
health and alive state
unconscious state
command/follow mode
trust, loyalty and bond
native state
target and threat state
visible attackers
movement blocked/stuck
managed, wild or unmanaged actor lane
tame candidate / hostile / blocked / passive classification
cooldown gates
ownership validity
kill switch
```

It proposes managed-animal actions such as:

```text
CatchUpRecall
NativeDefendPrompt
HoldAnchor
RegroupNearPlayer
RecoverStuck
RetreatOrStandDown
```

For wild animals it currently proposes only:

```text
TameCandidateReady
TameCandidateUnsafe
```

Important limitation:

```text
wild animal + unconscious = TameCandidateUnsafe
```

The current advanced package therefore does not represent the new unconscious acquisition lifecycle. It is useful source evidence, not the target implementation path.

## First wolf acquisition AI-package requirement

The first wolf slice requires an Avalon Companions package extension, not a local brain.

### Actor role separation

A wild quest acquisition target is not yet a managed companion. It must not be mislabeled as `avalon-companions.managed-companion`.

The package should use a separate owner-specific acquisition role, provisionally described as:

```text
Avalon Companions wolf acquisition target
```

The exact public role ID belongs in the Avalon Companions AI package and must be registered through the existing Avalon AI package/host process.

### Facts Avalon Companions owns and may publish

The wolf slice may publish only creature/animal domain or directly observed game facts that Avalon Companions owns or is authorised to expose:

```text
exact actor runtime identity
exact acquisition session/correlation identity
provider/species identity: wolf
quest acquisition eligibility
subdual threshold reached
native unconscious physical state established
interaction currently safe/available
player distance
nearby immediate danger
wolf alive/dead state
wolf injury/health band
wolf temperament/profile classification
current fear/confidence band owned by Avalon Companions
bonding offer category and validated item identity
prior acquisition attempt outcome
acquisition timeout/recovery gate
owner lease valid
kill switch / transition invalidation state
```

Avalon Companions must not publish invented world facts, human-companion state, raw dialogue implementation details, or a duplicate AI blackboard.

### Decisions Avalon AI may return through the package

The AI package may propose creature-acquisition semantic intents such as:

```text
WaitForSafeInteraction
AllowApproachInteraction
EvaluateBondingOffer
AcceptBondingOffer
RejectBondingOffer
RemainWary
RecoverAndFlee
RecoverAndResumeHostility
```

These names are planning vocabulary, not an implemented or final public API.

Avalon AI owns why and when a proposal is selected.

Avalon Companions remains responsible for validating and applying the owned domain transition:

```text
accepted offer
  -> create/bind creature companion identity
  -> promote from acquisition state to owned wolf companion state
  -> invoke proven native recovery under the new owner state

rejected offer
  -> invoke proven native recovery
  -> apply provider-owned rejection result
  -> release acquisition ownership after cleanup
```

The package must remain proposal-only:

```text
no direct Unity calls
no direct FoA calls
no inventory mutation
no quest mutation
no persistence writes
no faction mutation
no actor spawning
no dialogue UI ownership
```

### Deterministic work that remains outside AI

The following must not be delegated to Avalon AI:

```text
projectile identity and shooter attribution
hard target eligibility
quest-stage gate
unique/boss/story/undead exclusions
subdual amount and threshold
native unconscious component attachment
inventory quantity validation/consumption
creating or writing the creature companion profile
save commit
actor deduplication
ownership acquisition/release mechanics
```

AI reasons over the opportunity. It does not decide whether the physical and safety prerequisites are true.

## Dialogue Framework consumer map

### Current inspected repository state

On the inspected `companions` branch, `mods/dialogue-overhaul/` currently contains research/design documentation and no inspected runtime/service source, project, package or callable companion dialogue API.

Direct result:

```text
Dialogue Framework owner: PRESENT
Dialogue research/design foundation: PRESENT
Callable companion dialogue service in inspected repository ref: NOT_PRESENT
Avalon Companions local replacement authority: NONE
```

This finding is limited to the inspected repository ref. It does not claim that no local, uncommitted, external or later Dialogue Framework implementation exists.

Avalon Companions must not compensate by creating its own dialogue engine or service contract.

### Minimum consumer needs for the wolf slice

When the Dialogue Framework owner exposes the service, Avalon Companions needs to supply only bounded creature/animal interaction content and context.

Minimum request categories:

```text
interaction kind: animal acquisition / wolf bonding
acquisition session or correlation identity
exact target actor/acquisition identity
provider/species identity
player-facing wolf name or description
current physical condition
current provider-owned temperament/fear/bonding state
quest/tutorial context token
validated available semantic options
validated offer item identities and quantities
status or body-language presentation tokens
cancellation/transition token
```

Minimum result categories Avalon Companions needs to consume:

```text
same interaction/session identity
same target identity
selected semantic option
selected validated offer item and quantity when applicable
close/cancel/timeout result
framework-owned dialogue completion status
```

The exact request/result type names, UI lifecycle, authored/generated composition, voice, localisation, cursor, focus and input contracts belong to the Dialogue Framework owner.

### Semantic result boundary

The Dialogue Framework should return a semantic choice or interaction result. It must not directly mutate Avalon Companions state.

Conceptual flow:

```text
Avalon Companions
  -> asks Dialogue Framework service to present a wolf acquisition interaction
  -> supplies owned context and allowed actions

Dialogue Framework
  -> owns presentation, dialogue, choices, input and close lifecycle
  -> returns selected semantic result

Avalon Companions
  -> validates target/session/item/current state
  -> records offer event
  -> publishes relevant event/state to Avalon AI package
  -> applies only the final creature-domain transition it owns
```

### Current blocker

```text
wolf unconscious diagnostic proof: not blocked by dialogue service
bow/projectile static proof: not blocked by dialogue service
quest-stage static proof: not blocked by dialogue service
full player-facing wolf acquisition vertical slice: BLOCKED_PENDING_DIALOGUE_FRAMEWORK_SERVICE
```

No temporary Avalon Companions dialogue implementation should be promoted as a substitute for the framework service.

## End-to-end responsibility map

```text
FoA projectile
  -> reports validated impact

Avalon Companions
  -> validates weapon, quest and target
  -> computes subdual
  -> acquires creature acquisition state/owner boundary
  -> enters proven native unconscious physical state
  -> publishes owned observation to Avalon AI package

Avalon AI
  -> reasons over acquisition state
  -> proposes safe interaction or recovery intent

Avalon Companions
  -> requests wolf interaction from Dialogue Framework service
  -> supplies allowed semantic choices and owned context

Dialogue Framework
  -> owns dialogue/interaction presentation and input
  -> returns semantic player result

Avalon Companions
  -> validates result and item use
  -> publishes offer/outcome context to Avalon AI package

Avalon AI
  -> proposes accept/reject/wary/recovery intent

Avalon Companions
  -> applies owned creature-domain transition
  -> creates/binds wolf identity on success
  -> invokes proven physical recovery
  -> releases or promotes ownership correctly
```

## No-leak matrix

| Responsibility | Owner | Avalon Companions action |
|---|---|---|
| AI memory, goals, planning, coordination | Avalon AI | publish package facts; consume proposals |
| Dialogue UI, flow, authored/generated composition, input | Dialogue Framework | call service; consume semantic result |
| Human recruitment and human companion state | Avalon Human Companions | no action |
| Creature/animal acquisition, identity, bond, progression, persistence | Avalon Companions | authoritative owner |
| Bow/projectile/unconscious physical execution | FoA/native path under bounded adapter | validate and invoke proven primitive |
| Quest stage and capability gate | owning quest/content integration with Avalon Companions domain result | read/validate exact stage; no broad mutation |

## Current state

```text
one-system-one-truth correction = PASSED
existing Avalon AI package boundary inspection = PASSED
existing Avalon AI V2 host consumption inspection = PASSED
existing Dialogue Framework repository inspection = PASSED
V2 wolf acquisition package = NOT_PRESENT
wolf acquisition owner observation = NOT_PRESENT
wolf acquisition semantic proposals = NOT_PRESENT
Dialogue Framework companion service in inspected ref = NOT_PRESENT
bow/projectile proof = NOT_RUN
wolf unconscious runtime proof = NOT_RUN
full wolf acquisition implementation = BLOCKED
```

## Exact next tasks

### Avalon Companions owner

1. Map one exact FoA bow/ammunition/projectile path.
2. Complete the wolf native unconscious runtime proof.
3. Define the creature-domain acquisition state required before and after the AI/dialogue calls.
4. Extend the Avalon Companions AI package and owner observation through the existing package boundary only after those facts are proven.

### Avalon AI owner

Consume the future package role, observations, capabilities and proposals through the existing owner/host/runtime path. Do not move creature companion truth into Avalon AI.

### Dialogue Framework owner

Expose the existing framework's companion/animal interaction service needed by the wolf slice. The service owns dialogue and input lifecycle; it returns semantic results and does not own creature persistence or taming truth.

## Stop conditions

Stop the full wolf acquisition implementation if:

- the Dialogue Framework service remains unavailable;
- the AI acquisition package/host role is absent;
- exact actor identity cannot be held across the interaction;
- projectile attribution is ambiguous;
- native unconscious cleanup is unproven;
- the target falls outside the permitted wolf tutorial scope;
- any implementation would require Avalon Companions to implement a second AI or dialogue system.

## Next researched task

Trace one exact native FoA bow/ammunition/projectile path and one exact wolf target identity path for the hunter capture tutorial. Keep dialogue and AI work at their existing owner boundaries; do not create fallback systems inside Avalon Companions.
