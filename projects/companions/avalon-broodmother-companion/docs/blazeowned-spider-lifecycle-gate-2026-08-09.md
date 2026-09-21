# BlazeOwned Spider lifecycle gate - 2026-08-09

## Research metadata

- Tier: T4 mod-specific lifecycle and ownership gate
- Domain: `mods/avalon-broodmother-companion`, Avalon AI Runtime V2, Blaze execution, FoA actor ownership
- Owner: Avalon Broodmother Companion
- Status: **BLOCKED for implementation; source-confirmed research gate complete**
- Authority state: under evaluation; not promoted implementation authority
- Allowed use: discovery, design review, evidence collection, offline fixture design, and planning
- Forbidden use: runtime implementation, existing-actor takeover, provider expansion, deployment, FoA launch, save access, packaging, or release-readiness claims
- Review record: none; this artifact has not passed the research-promotion hierarchy
- Evidence date: 2026-08-09

## Request and scope

This gate evaluates the exact current Spider-family companion route against the requested `BlazeOwned` lifecycle: creation, native-AI suppression, exclusive binding, damage/death ownership, interruption, restoration, and idempotent disposal. It is offline only.

The requested commercial-stack packet already requires the host to prove native `NpcAI` unable to execute before `BlazeOwned` acquisition, forbids native combat or static-evaluator fallback while Blaze owns the actor, and identifies the suppression/restore mechanism as unknown (`mods/avalon-broodmother-companion/docs/commercial-stack-broodmother-ai-integration-packet-2026-08-09.md:289-297`). This document resolves what the current repository sources prove and names the remaining evidence required. It does not invent the missing transfer mechanism.

## Verdict

**BLOCKED.** The current Spider-family companion cannot be admitted to `ActorExecutionMode.BlazeOwned` on the available evidence.

Two individually evidenced lifecycles exist, but they do not compose:

1. The Brood Mother plugin creates a native FoA `Location` through `LocationTemplate.SpawnLocation`, requires its native `NpcElement`, installs the native summon faction and `NpcHeroPetAlly`, drives `NpcAI.EnterCombatWith`, routes damage through `HealthElement`, accepts death through `DeathElement` to `NpcDummy` plus `Corpse`, and disposes the location through `Location.Discard` (`mods/avalon-broodmother-companion/src/Plugin.cs:555-635`, `:836-909`, `:930-1015`, `:1377-1406`, `:2170-2224`, `:2259-2400`, `:2446-2586`, `:2906-2958`).
2. The accepted Blaze composition creates and destroys its own inactive ordinary Unity `GameObject`, attaches a new `BlazeAI`, acquires and binds an exact `BlazeOwned` lease before activation, and supports only `avalon.blaze.stay-idle` (`mods/avalon-ai-runtime/src/AvalonAI.Execution.Blaze/AvalonBlazeRuntimeComposition.cs:93-145`, `:220-331`, `:361-407`; `mods/avalon-ai-runtime/src/AvalonAI.Execution.Blaze/AvalonBlazeStayIdleExecutor.cs:82-184`). Its public construction path accepts no existing FoA actor, and the accepted architecture declares existing FoA NPCs ineligible until an actor-specific takeover gate proves native suspension and cleanup (`mods/avalon-ai-runtime/docs/api-2.0-assembly-and-contracts.md:398-415`; `mods/avalon-ai-runtime/docs/decisions/0040-api-v2-avalon-created-blaze-actor-lifecycle.md:15-32`).

No inspected source or accepted research provides a reversible, read-back-verifiable suppression and restoration path for the current actor's `NpcAI`, `NpcBehaviour`, `EnemyBaseClass`, `NpcMovement`, `NpcController`, Animancer/root motion, reactions, health, or death. The current companion source contains native combat-entry calls but no candidate `NpcAI`/`NpcBehaviour` pause, disable, working-state mutation, or restoration operation. This is a scoped source-scan result, not a claim that no such FoA API exists.

Therefore the requested ownership transfer, Spider action executor, combat procedures, damage/death bridge under Blaze, restoration path, and existing-actor disposal path remain unauthorized.

## Evidence classification

| ID | Classification | Evidence | Claim supported | Limitation |
|---|---|---|---|---|
| E1 | Accepted architecture | `mods/avalon-ai-runtime/docs/api-2.0-assembly-and-contracts.md:398-415` | `BlazeOwned` requires an accepted fully owned lifecycle; existing FoA NPCs remain native-assisted until takeover is proved | Does not itself supply a takeover implementation |
| E2 | Accepted decision | `mods/avalon-ai-runtime/docs/decisions/0040-api-v2-avalon-created-blaze-actor-lifecycle.md:15-32` | Current production composition creates a new actor, binds before activation, and excludes existing FoA actors | No movement, combat, damage, death, or host authority |
| E3 | Accepted decision | `mods/avalon-ai-runtime/docs/decisions/0041-api-v2-blaze-lifecycle-fault-containment.md:21-40` | Isolated lifecycle has bounded idempotence, fake-null handling, next-frame identity reuse, and explicit retained-context fault semantics | Still excludes existing actors and gameplay capabilities |
| E4 | Current source | `mods/avalon-ai-runtime/src/AvalonAI.Execution.Blaze/AvalonBlazeRuntimeComposition.cs:93-145`, `:220-331`, `:361-439` | Exact current construction, start, release, unbind, stop, close, destruction, and disposal order | Owns only the object it creates |
| E5 | Current source | `mods/avalon-ai-runtime/src/AvalonAI.Execution.Blaze/AvalonBlazeActorBinding.cs:7-32`; `AvalonBlazeStayIdleExecutor.cs:39-89`, `:234-270` | Binding requires the exact lease; executor binding is singular; cancellation clears the owned idle command | Capability is only `StayIdle` |
| E6 | Current source | `mods/avalon-ai-runtime/src/AvalonAI.Runtime.V2/AvalonAiRuntimeV2.cs:201-225`, `:711-760`, `:957-1049`; `AvalonActionGatewayV2.cs:371-421` | Runtime cancels exact active execution on timeout, loss of control, package disable, suspend, actor release, and stop paths | This is action/plan/context cancellation, not Spider motion recovery or FoA native-owner restoration |
| E7 | Current source | `mods/avalon-broodmother-companion/src/Plugin.cs:555-635`, `:836-909` | Current actor is a spawned native `Location`/`NpcElement` with native ally ownership | Not an Avalon-created Blaze actor |
| E8 | Current source | `mods/avalon-broodmother-companion/src/Plugin.cs:930-1015`, `:1332-1420` | Current actor deliberately retains native `HealthElement`, `DeathElement`, hitboxes, and native corpse handoff | No Blaze damage/death ownership path |
| E9 | Current source | `mods/avalon-broodmother-companion/src/Plugin.cs:2170-2224`, `:2259-2400`, `:2446-2586` | Current combat still drives native `NpcAI`, native pet combat, and a native `HealthElement.TakeDamage` bridge | Dual execution would result if Blaze were bound without suppression |
| E10 | Decompiled target research | `docs/research/game-systems/bandit-outlaw-foa-target-ownership-2026-08-06.md:68-89` | `NpcAI`/`NpcBehaviour`/`EnemyBaseClass`, movement/controller/root motion, `HealthElement`, and `DeathElement` are distinct native owners | A safe injection, suppression, or restoration point is explicitly unknown |
| E11 | Accepted native pattern | `mods/avalon-awakened/docs/research/ci5c-creature-combat-behaviour-system-2026-07-15.md:23-37`, `:61-65` | Existing custom-creature proof extends the native selector and preserves native death rather than seizing ownership | It is evidence for native-assisted extension, not `BlazeOwned` takeover |
| E12 | Accepted isolated evidence | `mods/avalon-ai-runtime/docs/research/blaze-owned-actor-lifecycle-composition-2026-07-19.md:25-41`, `:55-84`; `blaze-lifecycle-fault-containment-and-play-mode-readiness-2026-07-19.md:310-366` | The isolated Avalon-created lifecycle and its cleanup/fault contours were exercised | Evidence records `existing-foa-eligible=0` |

## Full lifecycle gate matrix

| Lifecycle segment | Source-confirmed current state | Result for current Spider actor | Exact blocker |
|---|---|---|---|
| Creation | The plugin spawns a FoA `LocationTemplate`, marks the `Location` not saved, and accepts it only after exact `LocationTemplate`, display-name, `NpcElement`, and `NpcTemplate` checks (`Plugin.cs:582-635`, `:836-879`). The Blaze composition instead allocates a new inactive `GameObject` and adds `AvalonBlazeStayIdleBehaviour` plus `BlazeAI` (`AvalonBlazeRuntimeComposition.cs:361-399`). | **Incompatible / blocked** | Current Blaze construction cannot adopt the spawned `Location` or its `NpcElement`; no accepted alternative creates the complete custom Spider as a wholly Avalon-owned actor. |
| Native-AI suppression | Decompiled ownership research says `NpcAI` owns broad AI/target entry, `NpcBehaviour` owns active/not-working/paused state, and `EnemyBaseClass` owns behavior selection/interruption (`bandit-outlaw-foa-target-ownership-2026-08-06.md:82-87`). Current plugin source calls native combat entry and contains no scoped suppression/restore operation. | **Unknown / blocked** | Exact callable APIs, ordering, readback, side effects, and restoration for every native execution owner are not proved. Suppressing only `NpcAI` would not prove movement, behavior, animation, or reaction exclusivity. |
| Exclusive binding | Runtime acquires the exact `BlazeOwned` lease, constructs an exact binding, binds a single executor, and activates afterward (`AvalonBlazeRuntimeComposition.cs:232-257`; `AvalonBlazeActorBinding.cs:7-32`; `AvalonBlazeStayIdleExecutor.cs:39-89`). | **Proved only for the isolated Avalon-created actor; blocked for the current actor** | Binding construction is internal and current public composition has no existing-actor input. Native owners are still active, so the prerequisite exclusive state cannot be asserted. |
| Damage ownership | Native `HealthElement` owns damage application and death routing (`bandit-outlaw-foa-target-ownership-2026-08-06.md:84-87`). The current bridge constructs native `Damage` with the `NpcElement` dealer and calls target `HealthElement.TakeDamage` (`Plugin.cs:2504-2558`). | **Native route confirmed; Blaze route blocked** | No accepted Blaze Spider capability, guarded host command, animation-event window, projectile/spit owner, hit reaction, or single-owner damage contract exists. |
| Death ownership | Current actor requires `DeathElement`, keeps the corpse, accepts only exact native `NpcDummy` plus `Corpse`, and then releases live-actor state (`Plugin.cs:936-1015`, `:1377-1420`). Native research confirms `HealthElement -> NpcElement.DieFromDamage -> DeathElement.OnDeath` and live-NPC discard (`bandit-outlaw-foa-target-ownership-2026-08-06.md:86-89`). | **Native route confirmed; Blaze route blocked** | Current Blaze lifecycle explicitly has no death authority. It is unproved whether a Blaze-motion/native-health-death split is legal, how death revokes the lease, or which owner produces reactions and corpse transition. |
| Interruption | Runtime V2 cancels active direct/procedure execution and plans on timeout, poll fault, package disable, suspension, actor release, and stop (`AvalonAiRuntimeV2.cs:711-760`, `:957-1049`; `AvalonActionGatewayV2.cs:371-421`). The current Blaze executor can interrupt only `StayIdle` (`AvalonBlazeStayIdleExecutor.cs:234-270`). | **Runtime cancellation primitive confirmed; Spider interruption blocked** | Bite, leap, spit, flank, retreat, recovery, airborne safe-landing, animation cleanup, VFX cleanup, and native behavior interruption do not exist as accepted capabilities. |
| Restoration | Current accepted Blaze actor is destroyed rather than restored. The companion restores plugin-added hitbox layers/registrations before location discard (`Plugin.cs:1332-1375`, `:2906-2958`). | **Blocked** | No capture record exists for native AI/behavior/movement/animation state; no reversible native suppression is known; no lease-loss restoration order or partial-acquisition rollback is proved. |
| Idempotent disposal | `Dispose` delegates to `Stop`; a repeated clean stop returns `AlreadyStopped`; stop deactivates, releases Runtime state, unbinds, stops, closes, destroys, and releases identity (`AvalonBlazeRuntimeComposition.cs:269-353`). AIR-27 proves repeated stop/dispose and next-frame reuse for this isolated actor while explicitly allowing retained Runtime contexts on persistent dependency faults (`0041-api-v2-blaze-lifecycle-fault-containment.md:23-30`). Current companion dismisses through native `Location.Discard` and clears plugin state (`Plugin.cs:2906-2990`). | **Each existing lifecycle is bounded separately; composed disposal blocked** | No transfer record identifies which side owns which components, so cleanup cannot safely distinguish restore, remove, discard, death handoff, or externally destroyed actor cases. Isolated Blaze idempotence cannot be generalized to a native `Location`. |

## Source-confirmed ownership maps

### Current native Spider-family companion

```text
LocationTemplate.SpawnLocation
  -> Location (one-session, MarkedNotSaved)
  -> NpcElement
     -> NpcAI / NpcBehaviour (AI and target/combat entry)
     -> NpcHeroPetAlly (summon ally combat prompt)
     -> EnemyBaseClass (native behavior selection and reactions)
     -> NpcMovement / NpcController / ARNpcAnimancer / RootMotion
     -> HealthElement (hitboxes, damage, lethal handoff)
     -> DeathElement
        -> NpcDummy + Corpse
        -> living NpcElement discarded
  -> Location.Discard on dismissal/failure
```

This map is supported by current plugin source and the installed-assembly ownership research (`Plugin.cs:607-635`, `:881-909`, `:930-1015`, `:2170-2224`, `:2446-2586`, `:2906-2958`; `bandit-outlaw-foa-target-ownership-2026-08-06.md:80-89`).

### Accepted isolated Blaze-owned actor

```text
reserve ActorId
  -> create inactive Avalon GameObject
  -> add AvalonBlazeStayIdleBehaviour + BlazeAI
  -> verify Animator + NavMeshAgent + CapsuleCollider + AudioSource
  -> start Runtime V2
  -> acquire exact BlazeOwned lease
  -> bind exact actor and lease
  -> activate object
  -> execute only avalon.blaze.stay-idle
  -> deactivate
  -> release actor (cancel action/plan/blackboard context)
  -> release binding
  -> stop Runtime
  -> close behavior
  -> destroy owned GameObject
  -> release ActorId after Unity destruction semantics permit it
```

This map is implemented at `AvalonBlazeRuntimeComposition.cs:93-145`, `:220-353`, `:361-527`. The accepted evidence explicitly records `existing-foa-eligible=0` and does not authorize movement, combat, damage, death, visuals, host integration, or gameplay (`blaze-owned-actor-lifecycle-composition-2026-07-19.md:55-84`; `blaze-lifecycle-fault-containment-and-play-mode-readiness-2026-07-19.md:296-366`).

## Mandatory exclusive-ownership invariants

These are acceptance conditions derived from the commercial-stack packet and current ownership architecture. They are not implementation instructions and do not select unproved APIs.

1. One stable actor ID and current generation identify the exact selected one-session Spider actor.
2. Every native execution owner that can initiate movement, targeting, behaviors, animation state, reactions, or attacks is named from current decompiled source.
3. A reversible suppression operation and a readback predicate are proved for each retained native execution owner.
4. Suppression completes and reads back before `AcquireActor(..., BlazeOwned)` and before any Blaze component/executor can run.
5. One exact `BlazeOwned` lease binds one exact actor controller and one exact executor. A stale or mismatched generation cannot bind, dispatch, poll, cancel, or restore.
6. Native `NpcAI.EnterCombatWith`, `NpcHeroPetAlly.EnterCombat`, `EnemyBaseClass` action selection, and the current static evaluator/damage bridge cannot execute while the Blaze lease is live.
7. Damage and death each have one named owner. A native-health/death shell is acceptable only if decompilation proves it can remain active without restoring native execution ownership or duplicating hit reaction/death behavior.
8. Any failed acquisition reverses only the changes captured in the current transfer record and leaves no Blaze lease, binding, command, procedure, reservation, VFX handle, or newly created component.
9. Interruption closes damage windows before cancelling motion/animation, reaches a source-proved controllable state, and cannot reopen a native or Blaze command after lease loss.
10. Death, dismissal, replacement summon, plugin shutdown, actor invalidation, scene unload, package revoke, kill switch, and Runtime stop converge on one repeat-safe release state machine.
11. Restoration occurs only for a still-live native actor whose identity and generation match the captured transfer record. Native death handoff and native discard are terminal and must not be reversed.
12. Persistent cleanup dependency faults are reported as retained/faulted state; they cannot be relabeled clean disposal merely because the Unity actor or binding is gone.

## Required state machine before implementation

The next research must prove enough source behavior to choose exact transitions for this abstract state machine:

| State | Required invariant | Allowed successor |
|---|---|---|
| `NativeUncaptured` | Verified selected live native Spider actor; no Blaze lease or binding | `NativeCaptured` or terminal native disposal |
| `NativeCaptured` | Exact native-owner states and generation recorded; nothing suppressed yet | `NativeSuppressed` or rollback to `NativeUncaptured` |
| `NativeSuppressed` | Every required native owner reports unable to execute; health/death disposition explicitly selected | `BlazeAcquired` or rollback/restoration |
| `BlazeAcquired` | Exact lease acquired; actor still inactive for Blaze execution | `BlazeBound` or release plus restoration |
| `BlazeBound` | One executor/controller bound to exact lease | `BlazeActive` or cancel/unbind/release plus restoration |
| `BlazeActive` | Blaze is sole execution owner; damage/death owner is explicit | `Interrupting`, `DeathHandoff`, or `Releasing` |
| `Interrupting` | Damage window closed; active procedure and command cancellation acknowledged | `BlazeActive` only with same valid lease, otherwise `Releasing` |
| `DeathHandoff` | Lease/commands revoked; exactly one death owner completes the transition | `Disposed` |
| `Releasing` | Cancel -> unbind -> Runtime release complete or fault explicitly recorded | `Restoring` or `Disposed` |
| `Restoring` | Exact still-live actor and transfer generation match; captured native state restored with readback | `NativeUncaptured` or explicit terminal fault |
| `Disposed` | Repeat calls have no side effects; no actor reuse until underlying Unity/native lifecycle permits | `Disposed` |

No current source implements `NativeCaptured`, `NativeSuppressed`, `Restoring`, or a composed `DeathHandoff`. Those are the decisive missing segments.

## Offline acceptance criteria for a future lifecycle implementation gate

A future implementation gate may proceed only after the research blocker below is resolved and an independently reviewed decision promotes one ownership model. Its offline fixture packet must then prove at least:

1. exact native owner inventory and suppression/readback coverage, with no unclassified owner;
2. acquisition rejection when any native owner remains executable;
3. lease-before-bind and bind-before-Blaze-activation ordering;
4. rejection of actor ID, role ID, lease ID, or generation mismatches;
5. zero native combat-entry or behavior dispatch while `BlazeOwned` is active;
6. one damage owner and one death owner, with duplicate damage/death dispatch rejected;
7. cancellation at every acquisition and action stage, including leap wind-up, airborne/landing, spit wind-up/projectile/VFX, and recovery;
8. restoration readback after every pre-death cancellation/fault stage;
9. death and native discard treated as non-restorable terminal states;
10. repeat-safe dismissal, replacement, plugin shutdown, external actor destruction, package revoke, Runtime stop, and kill switch;
11. zero remaining leases, bindings, action handles, procedure runner leases, reservations, VFX handles, and owned components after clean disposal;
12. explicit retained-context/fault diagnostics for persistent blackboard, planner, executor cancellation, native restoration, or native discard faults;
13. targeted static scans proving no call to the current basic evaluator, `NpcAI.EnterCombatWith`, or `NpcHeroPetAlly.EnterCombat` can occur inside a live `BlazeOwned` interval;
14. no FoA launch, save access, installed-file write, deployment, or release claim in the offline gate.

Passing fake/offline fixtures would prove only the selected source contract. Actor visuals, movement, animation, combat feel, damage timing, death presentation, and scene behavior would remain separate live-validation work requiring separate permission.

## Deep Research handoff brief

### Objective

Determine, from the current installed `TG.Main.dll` version and repository source, whether the existing native `Location`/`NpcElement` Spider can legally retain a native health/death shell while Avalon/Blaze exclusively owns movement, targeting, behavior, and animation, or whether the companion must instead be constructed as a wholly Avalon-created actor.

### Blocking contradiction

- Accepted Blaze architecture gives `BlazeOwned` full navigation, movement, targeting, local state, animation coordination, hit reaction, knockout, and death ownership, and declares existing FoA NPCs ineligible pending a takeover gate (`api-2.0-assembly-and-contracts.md:398-409`).
- The current companion is an existing FoA actor and intentionally retains native AI, movement/controller, health, death, corpse, and discard ownership (`Plugin.cs:607-635`, `:881-1015`, `:2170-2224`, `:2446-2586`, `:2906-2958`).

### Missing facts and research questions

1. What exact methods and state predicates start, stop, pause, resume, enable, disable, initialize, and discard `NpcAI` and `NpcBehaviour`?
2. Does stopping `NpcAI` also stop `EnemyBaseClass`, temporary/current behaviors, `NpcMovement`, `RichAI`, `NpcController`, Animancer state requests, root motion, attack animation events, and reactions? If not, what independently owns each surface?
3. Which operations are reversible for a live non-unique summon, and which cause irreversible discard, event unsubscription, provider removal, or controller destruction?
4. Can `HealthElement` and `DeathElement` remain authoritative while native AI/behavior/movement/animation owners are suppressed? What calls `NpcElement.DieFromDamage`, creates the dummy/corpse, and discards the living NPC in that split?
5. Can native `EnemyBaseClass` hit reaction/stagger/death animation handling remain active without becoming a second action/animation owner?
6. What exact events expose death, discard, controller destruction, or scene removal early enough to revoke Blaze execution before native handoff?
7. What captured fields and initialization ordering are necessary to restore the native actor without duplicating subscriptions, behaviors, movement providers, hitboxes, faction state, or summon ally state?
8. Does adding `BlazeAI`, a second `NavMeshAgent`, a second collider, or a second audio source to the existing AlivePrefab/root conflict with `NpcController`, `RichAI`, root motion, hitbox ownership, or cleanup?
9. If adoption is unsafe, what source-confirmed construction path can create the full custom Spider rig, navigation, animation, hit reaction, health, damage, death, faction, scene, and cleanup as one wholly Avalon-owned actor?

### Required proof

- Decompiled method bodies and field/property contracts for `NpcAI`, `NpcBehaviour`, `EnemyBaseClass`, `NpcMovement`, `NpcController`, `ARNpcAnimancer`, `RootMotion`, `HealthElement`, `NpcElement`, `DeathElement`, `Location`, `NpcDummy`, and `Corpse`, with assembly identity/hash and narrow repository citations.
- A complete owner/event/lifecycle call graph from native spawn through AI start, combat, damage, lethal handoff, discard, and restoration candidates.
- An explicit supported/unsupported/unknown matrix for a native-shell/Blaze-motion split.
- A selected actor-construction model with evidence for every owner and cleanup transition, or an honest blocked conclusion.
- Exact implementation-unblock criteria and offline fixture requirements. No source implementation should be proposed unless the whole ownership path is proved.

### Constraints

- Offline only: do not launch FoA, read or write saves, deploy DLLs, modify installed files, or mutate Unity assets.
- Do not treat method-name presence as behavioral proof; inspect bodies and their callers/callees.
- Do not infer that pausing one owner suppresses other owners.
- Do not weaken Runtime V2 lease/gateway/cancellation rules or use native/static fallback during a `BlazeOwned` interval.
- Do not touch protected Tales from the Age of Men / Age of Men / overhaul paths.

### Exact unblock condition

Implementation becomes research-ready only when an independently reviewed decision, backed by the required decompilation packet, selects one complete model:

- **Native shell, Blaze execution:** every retained/suppressed native owner, damage/death boundary, restoration step, and terminal cleanup path is proved; or
- **Wholly Avalon-created Spider:** creation, scene ownership, navigation, animation, combat, damage, reactions, death, faction, and disposal are all proved without an existing FoA actor takeover.

Until then, `executor.blaze_owned` must remain false for all six Spider-family roles, no `AvalonBlazeSpiderExecutor` may bind the current companion, and the current static/native combat route cannot be represented as commercial-stack completion.

## Gate result

- Research gate: **complete**
- Current Spider `BlazeOwned` eligibility: **rejected pending evidence**
- Native-AI suppression: **unknown**
- Exclusive existing-actor binding: **not available**
- Blaze damage/death ownership: **not implemented or authorized**
- Spider procedural interruption/restoration: **not implemented or authorized**
- Isolated Avalon-created `StayIdle` disposal: **source-confirmed and accepted within its existing narrow gate**
- Composed native-Spider disposal: **blocked**
- FoA launch/save/deployment activity: **none**

## Next researched task

Decompile and inspect the current `TG.Main` native Spider actor ownership path to identify reversible start/stop surfaces for `NpcAI`, `NpcBehaviour`, `EnemyBaseClass`, `NpcMovement`, `NpcController`/Animancer/root motion, `HealthElement`, and `DeathElement`; then decide whether a native-shell/Blaze-motion split is legal or the companion must be rebuilt as a wholly Avalon-created actor—offline only.
