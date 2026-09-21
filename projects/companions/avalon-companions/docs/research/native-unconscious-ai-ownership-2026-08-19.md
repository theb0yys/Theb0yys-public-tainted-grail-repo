# Native Unconscious Acquisition Backbone — Static Evidence and AI Ownership

Date: 2026-08-19  
Branch: `companions`  
Owner: Avalon Companions 1.0 overhaul  
Evidence lane: `decompilation-static` plus repository source/decision cross-check  
Runtime validation: `NOT_RUN`

## Purpose

Trace the complete native `UnconsciousElement` lifecycle in the current FoA `TG.Main.dll`, determine whether it can serve as the physical backbone for companion capture/taming/recruitment, and identify the exact seams where Avalon AI can own acquisition decisions without allowing FoA's vanilla combat AI to become the decision owner.

This record also preserves the agreed Companion 1.0 acquisition direction so the design is not lost between branches or later implementation passes.

## Source identity

Static inspection used the supplied current `TG.Main.dll` bytes:

```text
file = TG.Main(3).dll
size = 9058304
SHA-256 = 749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982
MVID = 68528841-991C-481E-BD94-7F1776FC3579
```

That fingerprint matches the installed-binary contract already recorded in repository process state for the current FoA build.

Repository cross-checks:

- `mods/avalon-companions/docs/research/taming-eligibility-gate-2026-07-02.md`
- `mods/avalon-companions/docs/research/taming-encounter-evidence-probe-2026-07-09.md`
- `mods/avalon-companions/src/Framework/AvalonCompanionAiGameSystems.cs`
- `mods/avalon-companions/ai-package/src/AvalonCompanions.AI.Package/AvalonCompanionAdvancedAiPackage.cs`
- `mods/avalon-ai-runtime/docs/decisions/0023-api-v2-atomic-ownership-migration.md`

## Native unconscious lifecycle

### Native element and movement lock

`Awaken.TG.Main.Locations.Attachments.Elements.UnconsciousElement` is an element over `NpcElement` and implements `Awaken.TG.Main.Fights.NPCs.Providers.ICanMoveProvider`. Its `get_CanMove()` returns `false`.

`OnInitialize()` sets `IsUnconscious=true`, registers itself with `NpcCanMoveHandler.AddCanMoveProvider`, waits for the owning `Location` visual to be loaded, then enters the lose-conscious path. Movement denial is therefore already a native actor-state mechanism.

### Lose-conscious physical transition

`OnVisualLoaded()` calls `InitializeLoseConscious()`, which calls `LoseConscious()` and then `InitRegainConsciousListeners()`.

Static IL for `LoseConscious()` proves it:

- triggers `Events.LoseConscious` on the owning `NpcElement`;
- calls `NpcAI.ExitCombat(true, true, true)`;
- adds `UnconsciousInvisibility`;
- creates `RagdollMovement(Vector3.zero, 0f, +infinity, false)`;
- interrupts current `NpcMovement` with that ragdoll state;
- conditionally adds `KillUnconsciousAction`;
- closes the NPC's eyes;
- overrides `NpcStats.Sight` to `0`;
- overrides `NpcStats.Hearing` to `0`;
- updates `NpcCrimeReactions.SetSeeingHero(false, true)`;
- removes negative statuses.

The element therefore supplies the majority of the convincing knockout presentation and physical control needed by Companion 1.0: the actor remains alive, cannot move, enters ragdoll, loses perception, leaves combat, and exposes an unconscious state.

### Vanilla combat-AI takeover is a separate seam

`EnemyBaseClass.OnInitializeInternal()` registers:

```text
Events.LoseConscious -> EnemyBaseClass.EnterUnconscious
```

`EnemyBaseClass.EnterUnconscious()` then calls `UpdateCombatStatus(false)`, `InterruptBehaviourWith(UnconsciousBehaviour, true)`, and unequips weapons when appropriate.

`UnconsciousBehaviour` is therefore a vanilla combat-behaviour layer placed on top of the native `UnconsciousElement`; it is not required to obtain the basic movement/ragdoll/perception state.

This gives Avalon a clean separation:

```text
native physical unconscious state
    !=
vanilla combat-AI unconscious behaviour
```

### Native automatic wake scheduling is a separate seam

`InitRegainConsciousListeners()` evaluates hero-combat and NPC-chunk danger. It increments `_dangerCounter` for active danger sources and begins recovery only after the last source clears.

The generated state machine for `DelayRegainConscious()` proves the native delay is:

```text
AsyncUtil.DelayTime(10.0f, ...)
```

and, if the wait completes successfully, it calls `UnconsciousElement.RegainConscious()`.

Vanilla FoA therefore owns wake timing only because this listener/scheduler is installed.

### Native recovery executor

`UnconsciousElement.RegainConscious()` is a public, zero-argument method.

It:

- sets `IsUnconscious=false`;
- triggers `Events.RegainConscious`;
- exits the stored ragdoll movement;
- clears the stored ragdoll handle;
- resets `NpcAI.AlertStack`;
- opens the NPC's eyes;
- removes negative statuses;
- discards the `UnconsciousElement`.

`OnDiscard()` removes the element from `NpcCanMoveHandler`, restoring movement-provider state.

This is the native recovery primitive Avalon can invoke after its own acquisition reasoning decides the actor should wake.

## Avalon AI ownership design

### Core rule

For an actor currently under an Avalon acquisition lease:

```text
Avalon decides.
FoA executes physical state.
Vanilla FoA combat AI does not own acquisition decisions.
```

Do not replace working native ragdoll, movement-provider, perception, eye, status, or recovery machinery merely to avoid using native code. The goal is to eliminate vanilla **decision ownership**, not native execution primitives.

### Scoped hook 1 — block vanilla `UnconsciousBehaviour`

Harmony-prefix `EnemyBaseClass.EnterUnconscious`.

For actors that carry an active Avalon acquisition lease, skip the original. `UnconsciousElement` still performs physical knockout and the `LoseConscious` event still exists, but vanilla `UnconsciousBehaviour` does not become the decision behaviour. Non-Avalon actors execute the original unchanged.

### Scoped hook 2 — block vanilla wake scheduling

Harmony-prefix `UnconsciousElement.InitRegainConsciousListeners`.

For an element whose parent actor carries an active Avalon acquisition lease, skip the original. This prevents hero-combat/chunk-danger scheduling and the fixed native ten-second wake decision. Avalon AI owns the duration and exit condition. Non-Avalon actors use the original schedule unchanged.

### Scoped hook 3 — remove the vanilla kill prompt for capture targets

`UnconsciousElement.get_AddKillUnconsciousAction()` returns `true` in the inspected binary and `LoseConscious()` consequently adds `KillUnconsciousAction`.

For an Avalon acquisition target, the player-facing action should be the new acquisition interaction (`Approach`, offer/care/dialogue), not the vanilla unconscious kill action. Implementation should suppress that property result or remove only the generated action for the exact acquisition-owned actor.

### `NpcAI.ExitCombat` boundary

`UnconsciousElement.LoseConscious()` performs a one-shot `NpcAI.ExitCombat(true, true, true)`.

This is accepted as a native state-reset/executor call, not AI decision ownership. Reimplementing the whole native `LoseConscious()` method solely to eliminate that call would duplicate event dispatch, invisibility state, ragdoll construction, movement interruption, perception suppression, eye state, crime-perception state, and status cleanup.

Companion 1.0 therefore uses this rule:

```text
No vanilla planning/behaviour/decision ownership while Avalon owns the acquisition lane.
Narrow native methods may still be used as physical actuators and state-reset primitives.
```

## Acquisition ownership lifecycle

```text
vanilla actor
  -> taming/subdual projectile hit
  -> Avalon validates eligibility
  -> subdual buildup reaches threshold
  -> acquire exclusive Avalon acquisition lease
  -> attach native UnconsciousElement
  -> suppress vanilla UnconsciousBehaviour handoff
  -> suppress vanilla automatic wake scheduling
  -> Avalon AI evaluates approach/offers/dialogue/recruitment
  -> success:
       promote exact actor into persistent companion identity/ownership
       configure companion runtime ownership
       call native RegainConscious()
       continue under Avalon companion AI
  -> failure/timeout/release:
       call native RegainConscious()
       release Avalon acquisition lease
       return actor to vanilla ownership
```

There must be no ownership gap between successful acquisition and wake. A successful target must not regain consciousness under vanilla AI and only later be converted.

## Existing Avalon AI implications

Current Avalon Companion AI already carries `bool Unconscious` in advanced observations.

The current advanced package intentionally treats an unconscious managed actor as non-actionable and an unconscious wild tame candidate as unsafe. That was correct for the old read-only taming gate, but it is insufficient for Companion 1.0.

The acquisition lane needs provider-neutral semantic state distinct from ordinary companion combat proposals. Minimum conceptual states are:

```text
None
SubdualBuilding
UnconsciousPendingInteraction
InteractionAvailable
NegotiatingOrBonding
Accepted
Rejected
RecoveryPending
```

Exact contract names remain an implementation decision.

Future AI inputs should include, where evidence permits:

- exact runtime actor identity;
- template/archetype;
- human / animal / Wyrd-creature acquisition provider;
- subdual amount and resistance;
- alive/unconscious state;
- elapsed unconscious time;
- player distance and nearby danger;
- hostility/faction state;
- prior acquisition attempts;
- offered item/category;
- hunger/fear/temperament for animals;
- Wyrd affinity/resistance for Wyrd creatures;
- fear/respect/greed/hunger/morale/personality for humans;
- relationship and memory context;
- story/unique/boss protection state.

## Companion 1.0 acquisition design preserved by this research

### Mod-owned taming weapon path

Use a cloned FoA bow and arrow pair as mod-owned acquisition items. Do not globally rewrite a vanilla bow or ammunition template.

Preserve the native:

```text
equip -> draw -> fire -> projectile -> impact
```

path and begin Avalon acquisition logic only after the exact mod-owned projectile impact is identified.

### Separate subdual from health

Do not implement taming as "reduce target health to one."

```text
Health <= 0       -> death
Subdual >= limit  -> unconscious acquisition opportunity
```

Normal impact damage, if retained at all, should be small relative to subdual.

### Exact actor continuity

The actor that the player subdues is the actor that becomes the companion.

Do not destroy the encountered actor and spawn a generic companion replacement merely for convenience. Preferred lifecycle:

```text
wild actor runtime identity
  -> acquired persistent CompanionId
  -> same live actor becomes current physical manifestation
```

Later map/save reconciliation may reconstruct a physical actor if FoA requires it, but that is restoration of an existing companion identity, not a second recruitment or summon-from-nothing event.

### Animals

Knockout opens physical/behavioural taming. Candidate offer categories include meat, fish, honey, fruit/vegetables/herbs, species-specific preferred food, and later-proven care/healing interactions.

Avalon AI interprets the offer according to species, temperament, fear, hunger, injury, previous attempts, nearby danger, and player behaviour. Animals should use behavioural feedback rather than human speech.

### Wyrd creatures

Wyrd creatures use a distinct acquisition provider. Ordinary animal food is not assumed to work. They may require later-researched Wyrd-infused items, Wyrd essence, ritual/corrupted objects, or creature-specific offerings. Exact lore items remain a later content decision.

### Humans

Humans are subdued/captured, not tamed.

Knockout creates a negotiation opportunity. Candidate offer/leverage categories include food, alcohol, money, contextual items, and later dialogue/relationship checks.

Potential acquisition states include hired, persuaded, rescued, captured, volunteered, quest-joined, released, rejected, and temporary ally. Money may establish employment without trust; an offer's meaning depends on personality and context.

### Dialogue and interaction ownership

```text
Combat obtains the opportunity.
Interaction determines whether the player engages with it.
Dialogue/AI determines the relationship and recruitment result.
```

For animals, dialogue is behavioural interaction and readable reaction rather than spoken lines. For humans, waking/negotiation routes through the Companion 1.0 dialogue system.

## Static support boundary

`UnconsciousElement` is tied to `NpcElement`, and its inspected native path expects access to `NpcAI`, `NpcMovement`, `NpcStats`, `CharacterStatuses`, parent `Location`, and `NpcCrimeReactions`.

Current companion code already reasons over `NpcElement` for reviewed animal/creature actors, but static type compatibility does **not** prove every animal/monster prefab exposes the complete unconscious dependency set at runtime.

Therefore:

```text
ordinary non-unique humanoid support = PARTIAL_STATIC
reviewed wolf/bear support = PARTIAL_STATIC
other ordinary creatures = PARTIAL_STATIC
unique/story NPCs = BLOCKED
bosses = BLOCKED
undead/special lifecycle actors = BLOCKED until separately reviewed
broad all-creature support = NOT_PROVEN
```

## Required first runtime proof

Use a throwaway save and test exactly:

1. one reviewed wild wolf;
2. one reviewed wild bear;
3. one reviewed non-unique human combatant.

For each target capture:

- exact template/runtime identity;
- required component presence;
- whether `UnconsciousElement` can be added safely;
- `IsUnconscious=true`;
- `CanMove=false`;
- ragdoll/physical presentation;
- absence of the Avalon-suppressed vanilla `UnconsciousBehaviour` takeover;
- absence of vanilla automatic wake scheduling;
- acquisition interaction availability;
- AI observation contains exact actor/unconscious/acquisition state;
- Avalon-controlled recovery calls public `RegainConscious()`;
- movement/perception/eyes recover;
- no stale acquisition lease;
- failure route cleanly returns vanilla ownership;
- success route preserves exact actor identity while promoting companion ownership.

Fail an archetype on missing dependencies, add-element exception, death instead of unconsciousness, broken ragdoll, movement while unconscious, vanilla timed wake under Avalon ownership, continuing vanilla decision behaviour, unsafe interaction attachment, unexpected identity replacement, incomplete recovery, leaked ownership, or inability to restore vanilla ownership after a failed attempt.

## Current status

```text
supplied TG.Main fingerprint = PASSED
native UnconsciousElement static lifecycle trace = PASSED
native ten-second recovery trace = PASSED
vanilla EnemyBaseClass unconscious-behaviour seam = PASSED
public native RegainConscious executor = PASSED
Avalon AI existing unconscious observation = PASSED
broad actor-class runtime support = PARTIAL
Avalon acquisition AI contract = NOT_IMPLEMENTED
taming/subdual weapon = NOT_IMPLEMENTED
runtime wolf proof = NOT_RUN
runtime bear proof = NOT_RUN
runtime non-unique human proof = NOT_RUN
save/persistence proof = NOT_RUN
```

## Next researched task

Implement only a diagnostics-first acquisition proof for the three target archetypes above: inspect required unconscious dependencies, acquire a temporary Avalon acquisition lease, enter native unconscious state with vanilla behaviour/wake ownership suppressed, expose a minimal interaction marker, and recover through `RegainConscious()` without recruitment or persistence.
