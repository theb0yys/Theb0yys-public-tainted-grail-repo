# TG.Main Spider Ownership and Construction Decision

Date: 2026-08-09
Status: under evaluation; offline source decision complete; implementation not authorized
Scope: current Avalon Broodmother/Spider companion actor, native execution-owner lifecycle, and the `BlazeOwned` construction boundary
Runtime activity: none; no FoA launch, save access, deployment, installed-file write, or Unity asset mutation

## Verdict

**Select wholly Avalon-created actor construction for every future `BlazeOwned` Broodmother and Spider role. Reject a native-shell/Blaze-motion split for the current `Location`/`NpcElement` actor.**

The current `TG.Main` source exposes reversible controls for individual pieces of the native actor, but it does not expose one complete, reversible ownership-transfer boundary. A takeover would have to suppress and later restore independently active owners across summon/ally logic, AI state, combat behavior, movement, pathfinding, root motion, animation, damage reaction, health, and death. Several of those surfaces have no pause/resume contract; others are destructive, re-enter through retained event listeners, reset rather than restore their prior state, or fire completion callbacks while being disabled.

This decision resolves the construction branch only. It does **not** authorize implementation. The current Avalon Blaze composition creates an isolated, bare `GameObject` and proves only `avalon.blaze.stay-idle`; it does not yet construct a Spider visual, custom animation controller, navigation/combat body, health/damage/death system, faction identity, or scene lifecycle. Those owners require a separate source-confirmed component contract before runtime work.

## Authority and evidence boundary

This packet answers the exact next research task from `mods/avalon-broodmother-companion/docs/blazeowned-spider-lifecycle-gate-2026-08-09.md`. It applies the repository research and evidence rules and remains research material until independently reviewed and promoted by the required human owner.

Direct repository evidence:

- `mods/avalon-broodmother-companion/src/Plugin.cs:881-901` creates the current one-session native ally by retaining the spawned native actor, overriding faction, and adding `NpcHeroPetAlly`.
- `mods/avalon-broodmother-companion/src/Plugin.cs:2906-2957` dismisses or rejects that actor through native `Location.Discard()`.
- `mods/avalon-broodmother-companion/docs/blazeowned-spider-lifecycle-gate-2026-08-09.md` defines the required exclusive-ownership, restoration, damage/death, and repeat-safe cleanup invariants.
- `mods/avalon-ai-runtime/docs/api-2.0-assembly-and-contracts.md:398-415` assigns navigation, movement, targeting, local state, animation coordination, hit reaction, knockout, and death to `BlazeOwned` and rejects silent native fallback.
- `mods/avalon-ai-runtime/src/AvalonAI.Execution.Blaze/AvalonBlazeRuntimeComposition.cs:93-145`, `:220-353`, and `:361-527` implement the accepted isolated Avalon-created stay-idle lifecycle, not adoption of an existing FoA NPC.

The method-body findings below come from offline decompilation of the exact locally installed managed assembly:

| Item | Identity |
|---|---|
| Assembly | `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\Fall of Avalon_Data\Managed\TG.Main.dll` |
| Length | `9,058,304` bytes |
| Last write time | `2026-07-22 20:19:43` local filesystem time |
| File/product version | `0.0.0.0` / `0.0.0.0` |
| SHA-256 | `749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982` |
| Decompiler | ILSpy command-line decompiler `10.1.0.8386` |
| Inspection mode | named type decompilation to stdout only; no decompiled source files retained |

Reproduction command shape used for the method-body pass:

```powershell
$assembly = 'A:\SteamLibrary\steamapps\common\Tainted Grail FoA\Fall of Avalon_Data\Managed\TG.Main.dll'
$types = @(
  'Awaken.TG.Main.Locations.Setup.LocationTemplate',
  'Awaken.TG.Main.Locations.Location',
  'Awaken.TG.Main.Fights.NPCs.NpcElement',
  'Awaken.TG.Main.AI.NpcAI',
  'Awaken.TG.Main.AI.States.NpcBehaviour',
  'Awaken.TG.Main.AI.States.StateAIWorking',
  'Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroPetAlly',
  'Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon',
  'Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly',
  'Awaken.TG.Main.Fights.NPCs.Providers.NpcCanMoveHandler',
  'Awaken.TG.Main.AI.Movement.NpcMovement',
  'Awaken.TG.Main.AI.Movement.Controllers.NpcController',
  'Awaken.TG.Main.AI.Movement.RootMotions.RootMotion',
  'Awaken.TG.Main.Utility.Animations.ARAnimator.ARNpcAnimancer',
  'Awaken.TG.Main.AI.Combat.Attachments.EnemyBaseClass',
  'Awaken.TG.Main.Character.HealthElement',
  'Awaken.TG.Main.Locations.Attachments.Elements.DeathElement',
  'Awaken.TG.Main.Fights.NPCs.NpcDummy',
  'Awaken.TG.Main.Locations.Attachments.Elements.Corpse'
)
Get-FileHash -LiteralPath $assembly -Algorithm SHA256
foreach ($type in $types) {
  & 'C:\Users\kane0\.dotnet\tools\ilspycmd.exe' -t $type $assembly
}
```

The assembly type inventory contains no class, interface, struct, enum, or delegate whose managed type name contains `Spider`. No separately named managed Spider AI owner was therefore found in this inventory. The current plugin supplies the selected Spider through exact native template, attachment, visual, and animation identities already recorded in its owning research, but this type-name result does not prove that serialized behavior assets contain no Spider-specific configuration.

## Current native construction and lifecycle path

```text
LocationTemplate.SpawnLocation(...)
  -> LocationCreator.CreateRuntimeLocation(...)
  -> World.Add(Location)
  -> Location.OnInitialize / attachment initialization / visual loading
  -> NpcElement.InitFromAttachment / OnInitialize / OnFullyInitialized
     -> NpcRegistry registration and native event listeners
     -> visual load -> NpcController.Init
     -> NpcAI.Init -> AI.States.NpcBehaviour.Enter
     -> EnemyBaseClass initialization and time-dependent update
     -> NpcMovement initialization and time-dependent update
  -> plugin adds NpcHeroPetAlly
     -> NpcHeroSummon.Init
     -> NpcAlly.Init / AfterVisualLoaded
     -> faction, targeting, patrol, UnityUpdate, combat and teleport logic
  -> live damage
     -> HealthElement.TakeDamage
     -> NpcAI and EnemyBaseClass damage listeners
     -> lethal HealthElement.OnDeathEvents
     -> NpcElement.DieFromDamage
     -> DeathElement.OnDeath
     -> NpcDummy creation
     -> optional Corpse creation
     -> living NpcElement.Discard
  -> explicit dismissal/failure
     -> Location.Discard
     -> model and view teardown
```

Source locators: `TG.Main.dll` SHA-256 above, `Awaken.TG.Main.Locations.Setup.LocationTemplate::SpawnLocation`, `LocationTemplate::AddLocationToWorld`, `Awaken.TG.Main.Locations.Location::OnInitialize`, `Location::VisualLoaded`, `Awaken.TG.Main.Fights.NPCs.NpcElement::InitFromAttachment`, `NpcElement::OnInitialize`, `NpcElement::OnFullyInitialized`, `NpcElement::AfterVisualLoaded`, `Awaken.TG.Main.AI.NpcAI::Init`, `Awaken.TG.Main.AI.States.NpcBehaviour::.ctor`, `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon::Init`, `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly::AfterVisualLoaded`, `Awaken.TG.Main.Character.HealthElement::TakeDamage`, `HealthElement::OnDeathEvents`, and `NpcElement::DieFromDamage`.

## Native owner start/stop inventory

Legend:

- **Reversible** means the source exposes a paired operation with a usable readback or stable behavioral contract.
- **Partial** means the pair controls only one subsystem or resumes by reset rather than exact restoration.
- **Destructive** means listeners, models, controllers, visual references, or behavior assets are discarded/destroyed with no source-confirmed reconstruction of the same live actor.
- **Unproved** means a matching restoration body or postcondition was not present in the inspected source.

| Native owner | Start / re-entry surfaces | Stop / suppression candidates | Reversibility and decisive finding |
|---|---|---|---|
| `Location` | `SpawnLocation` -> `World.Add`; `OnInitialize`; `VisualLoaded` callbacks | `Kill`; `Discard`; `OnDiscard` | **Destructive.** `OnDiscard` records/discards state, destroys the non-static view parent, and tears down model ownership. There is no whole-location pause/resume surface. |
| `NpcElement` | `InitFromAttachment`; `OnInitialize`; `OnFullyInitialized`; `AfterVisualLoaded`; `NpcController.Init` | `DieFromDamage`; `Discard`; `OnDiscard` | **Destructive.** Death and discard unregister the NPC, clear targets/caches, release the visual reference, and transition to dummy/death ownership. No live-owner suspension or reconstruction contract exists. |
| `NpcAI` | `Init` constructs `NpcBehaviour`, calls `Init` and `Enter`, registers time-dependent update, and subscribes to damage | `Working=false`; `SetActivePerceptionUpdate(false)`; `ExitCombat(force:true)`; `Behaviour.Exit`; `OnDiscard` | **Partial.** `Working=false` gates `CanEnterCombat`, but native state entry can set it true again. Perception disable does not stop combat/targeting/update. Forced combat exit does not stop later re-entry. `Behaviour.Exit/Enter` is idempotent but re-enters the initial state rather than restoring the exact prior state. `OnDiscard` is destructive. |
| `NpcBehaviour` and `StateAIWorking` | `Enter`; state transitions; `StateAIWorking.OnEnter` registers working AI, perception, events, and sets `NpcAI.Working=true` | `Exit`; `StateAIWorking.OnExit` unregisters its own listeners and sets `Working=false` | **Partial reset.** Base `State.Enter/Exit` is paired and listener cleanup is owner-scoped, but `StateMachine.OnExit` clears the current state. A later enter selects the initial/current-band state; it does not restore the previous state. `StateAIPaused` is excluded for hero summons. |
| `NpcHeroPetAlly` / `NpcHeroSummon` | element addition and `Init`; visual-load callbacks; ally combat/portal/fast-travel/health listeners; `AnimatorBridge` provider | `Destroy`; `Discard`; `OnDiscard` | **No reversible pause.** Hero summons set `AlwaysUpdate=true`, set `RichAI.canBePaused=false`, and remain active through inherited ally logic. `OnDiscard` unregisters providers/listeners but element recreation is not a proved restoration path. `NpcElement.IsHeroSummon` and `NpcAI`'s captured private hero-summon flag are not reset by a public inverse. |
| `NpcAlly` | `Init`; `AfterVisualLoaded`; `UnityUpdateProvider.RegisterGeneric`; `FindTarget`; `EnterCombat`; targeting and culling listeners | no pause; `OnDiscard` removes movement provider, unregisters Unity update, and resets faction | **Destructive/partial.** Unity update independently finds targets, enters combat, changes patrol movement, and may teleport. Unregistering only update would leave event entry points; `OnDiscard` removes the element's whole lifecycle and is not a same-instance pause/resume pair. |
| `NpcCanMoveHandler` | `AddCanMoveProvider` | `RemoveCanMoveProvider`; a retained provider can return false | **Reversible but movement-only.** A false provider can stop `RichAI.canMove` and can later be removed, but it does not suppress teleport, targeting, combat, behaviors, animation requests, reactions, health, or death. |
| `NpcMovement` | `InitializerInitialize`; `ChangeMainState`; `ResetMainState`; update registration | `InterruptState(new NoMove())`; `StopInterrupting`; `OnDiscard` | **Reversible for ordinary movement interruption only.** `NoMove` and `StopInterrupting` provide a paired movement override. They do not stop ally teleport, AI/behavior dispatch, root-motion/animation ownership, damage reactions, or death. `OnDiscard` exits state and destroys the controller. |
| `NpcController` / `RichAI` | `Init`; `OnEnable`; `ToggleGlobalRichAIActivity(true)`; `RefreshRichAIActivity` | component disable; `OnDisable`; `ToggleGlobalRichAIActivity(false)` | **Reversible for controller/path movement only.** Disable toggles root motion and global `RichAI` activity. It does not unregister model updates, stop `NpcAlly`, stop `EnemyBaseClass`, remove health listeners, or transfer animation ownership. `OnDestroy` destroys `RichAI`/RVO and is terminal. Hero summon initialization explicitly sets `RichAI.canBePaused=false`. |
| `RootMotion` | `NpcController.Init`; `OnUpdate`; animator-move callback | component disable through `NpcController.OnDisable` | **Partial.** The enabled flag controls root-motion/velocity updates, not the action graph, targeting, reactions, animation event producers, or health/death. |
| `ARNpcAnimancer` | native initialization, playable/rig construction, behavior/event subscriptions, state playback | component `OnDisable`; `OnNpcDeath`; `BeforeNpcDiscarded`; `OnDestroy` | **Not a proved transparent pause.** `OnDisable` invokes end callbacks for Animancer states before pausing the graph. No class-specific restoration body proves preservation of current state, callbacks, action windows, or exact resumption. Death/discard paths release assets, destroy states, unregister, or null the NPC and are terminal. |
| `EnemyBaseClass` | `OnInitialize`; `OnVisualLoaded`; registered time-dependent `OnUpdate`; combat selection; damage listener; behavior interruption/start methods | `StopCurrentBehaviour(false)`; `OnDiscard` | **No complete reversible stop.** Stopping the current behavior leaves the owner's update and damage/event entry points alive; later selection or reaction can resume action. `OnDiscard` unregisters and releases behavior assets but provides no restoration API. |
| `HealthElement` | `Init`; hitbox initialization; `TakeDamage`; dealing/taking/after-damage events | no pause; `OnDiscard` | **Retainable data owner, not isolated execution.** Health can remain callable independently of `NpcAI`, but damage events enter `NpcAI`, `EnemyBaseClass`, reaction, VFX, and lethal native paths. No public pause can retain health while atomically detaching and later restoring every native reaction owner. |
| `DeathElement` | initialization from custom death controller; `OnDeath` dispatches all death behaviors and animation FSM | no pause/resume; terminal death/discard | **Terminal native execution owner.** It is an action/animation owner during death and cannot coexist with Blaze-exclusive animation/death ownership without duplicate authority. |
| `NpcDummy` | constructed during `NpcElement.DieFromDamage`; receives the transferred visual reference | `Discard` releases visual | **Terminal shell.** Restore removes/destroys movement controllers including `NpcController`, `CharacterController`, `RichAI`, and RVO; it is not a live takeover shell. |
| `Corpse` | conditionally added during native death | model discard | **Terminal post-death model.** It does not provide a reversible live-actor boundary. |

Decompiled source locators for this table: `TG.Main.dll` SHA-256 above, `Awaken.TG.Main.AI.NpcAI::{Init,Update,CanEnterCombat,EnterCombatWith,ExitCombat,SetActivePerceptionUpdate,OnDiscard}`, `Awaken.TG.Main.AI.States.NpcBehaviour::.ctor`, `Awaken.TG.Main.AI.States.StateAIWorking::{OnEnter,OnExit}`, `Awaken.TG.Main.Utility.StateMachines.State::{Enter,Exit}`, `Awaken.TG.Main.Utility.StateMachines.StateMachine::OnExit`, `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon::{Init,AfterVisualLoaded,EnterCombat,Destroy,OnDiscard}`, `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly::{Init,AfterVisualLoaded,UnityUpdate,StayCloseToAlly,FindTarget,OnDiscard}`, `Awaken.TG.Main.Fights.NPCs.Providers.NpcCanMoveHandler`, `Awaken.TG.Main.AI.Movement.NpcMovement::{InitializerInitialize,InterruptState,StopInterrupting,OnDiscard}`, `Awaken.TG.Main.AI.Movement.Controllers.NpcController::{Init,OnEnable,OnDisable,ToggleGlobalRichAIActivity,Update,OnAnimatorMoved,OnDestroy}`, `Awaken.TG.Main.AI.Movement.RootMotions.RootMotion`, `Awaken.TG.Main.Utility.Animations.ARAnimator.ARNpcAnimancer::{OnDisable,OnNpcDeath,BeforeNpcDiscarded,OnDestroy}`, `Awaken.TG.Main.AI.Combat.Attachments.EnemyBaseClass::{OnInitialize,OnVisualLoaded,OnUpdate,CombatUpdate,StopCurrentBehaviour,OnDamageTaken,OnDiscard}`, `Awaken.TG.Main.Character.HealthElement::{Init,TakeDamage,OnDeathEvents,OnDiscard}`, `Awaken.TG.Main.Locations.Attachments.Elements.DeathElement::OnDeath`, `Awaken.TG.Main.Fights.NPCs.NpcElement::{DieFromDamage,DeathNonCriticalFunctions,DeathCriticalFunctions,OnDiscard}`, `Awaken.TG.Main.Fights.NPCs.NpcDummy`, and `Awaken.TG.Main.Locations.Attachments.Elements.Corpse`.

## Why the available controls do not compose into exclusive ownership

A native-shell split would need all of these statements to be true at the same instant:

1. native target acquisition and combat entry cannot run;
2. native behavior selection, damage reactions, stagger/ragdoll, and attack release cannot run;
3. native movement, teleport, pathfinding, root motion, and animation state writes cannot run;
4. native health remains callable without re-entering any suppressed owner;
5. exactly one death system owns death animation, body transition, corpse/dummy, and cleanup;
6. the exact pre-takeover native state can be restored without duplicate listeners/providers or destroyed components.

The source provides no operation satisfying that conjunction. Combining the partial controls is still insufficient:

- `NpcAI.Working=false` is overwritten by `StateAIWorking.OnEnter` and does not stop ally update, behavior update, movement, animation, or damage reactions.
- `NpcBehaviour.Exit` resets the state machine rather than preserving an exact restoration snapshot.
- a false `NpcCanMoveProvider`, `NpcMovement.NoMove`, or disabled `NpcController` suppresses ordinary locomotion but not `NpcAlly` teleport, combat entry, behavior dispatch, health listeners, or death.
- disabling `ARNpcAnimancer` invokes state end callbacks, so it can change attack/procedure state instead of merely freezing it.
- `EnemyBaseClass.StopCurrentBehaviour(false)` leaves update and damage-reaction entry points registered.
- retaining `HealthElement` retains the event source used by native combat/reaction owners.
- retaining `DeathElement` retains a native animation/action owner; removing it loses the native dummy/corpse/death contract.
- discarding any of the principal model owners is teardown, not reversible suppression.

The result fails the prior gate's requirements for capture, complete suppression readback, exclusive binding, exact restoration, and one death owner. Partial controls are useful for native-assisted commands, but they are not a legal `BlazeOwned` transfer.

## Native death contradiction found in the current companion

The decompiled lifecycle also exposes a source contradiction in the existing native-death acceptance logic. This is directly relevant to ownership and release readiness; no runtime result is asserted.

1. The plugin adds `NpcHeroPetAlly` and does not remove it before death (`Plugin.cs:881-901`; repository searches show later lookups but no removal/discard of that element).
2. `NpcHeroPetAlly` inherits `NpcHeroSummon`, which inherits `NpcAlly` and implements the summon marker path used by `NpcElement.IsSummon` (`TG.Main.dll`, `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroPetAlly`, `NpcHeroSummon`, `INpcSummon`, and `NpcElement::get_IsSummon`).
3. `NpcElement.DeathNonCriticalFunctions` adds `Corpse` only when the NPC is **not** a summon. The current actor is a summon, so its native lethal path does not add the `Corpse` required by `Plugin.cs:1420-1424`.
4. Native `NpcElement.DieFromDamage` constructs `NpcDummy` without overriding its `hasDied` constructor default, so the resulting dummy has `HasDied=true` (`TG.Main.dll`, `NpcElement::DieFromDamage` and `NpcDummy` constructor).
5. `Plugin.cs:1420-1424` rejects the transition when `candidate.HasDied` is true.

Therefore the current `BROODMOTHER_COMPANION_NATIVE_DEATH_ACCEPTED` predicate is source-unreachable through the inspected standard summon death path for two independent reasons: required `Corpse` absence and rejection of the standard dead dummy. This is an offline source finding, not evidence about a live session. It requires correction or removal when the actor-construction migration is implemented; the current research task does not authorize that code change.

## Construction decision matrix

| Criterion | Native shell + Blaze motion | Wholly Avalon-created actor |
|---|---|---|
| One execution owner | Rejected: multiple native entry points remain | Architecturally achievable because Avalon creates every executable component |
| Reversible suppression | Rejected: only partial controls; destructive teardown for full removal | No takeover/restoration phase is needed |
| Exact native-state restoration | Rejected: state/animation/listener snapshot and reconstruction are not exposed | Not applicable; disposal owns the custom actor instead of restoring a native one |
| Health/damage split | Rejected: native health events re-enter native reaction owners | Must be implemented as an Avalon-owned contract |
| Death ownership | Rejected: native `DeathElement` is itself an action/animation owner | Must be implemented as a single Avalon-owned terminal transition |
| Blaze lease/binding | Would bind an actor whose native authority cannot be proved absent | Can follow the accepted create-inactive -> acquire -> bind -> activate order |
| Cleanup | Native discard is terminal and cannot restore a captured live actor | Can converge on repeat-safe unbind/release/destroy for an owned object |
| Existing proof | No complete takeover proof | Stay-idle lifecycle only; full Spider composition still missing |
| Decision | **Rejected** | **Selected, implementation blocked pending full component contract** |

## Selected ownership boundary

For `BlazeOwned`, the selected actor must be created by Avalon without `LocationTemplate.SpawnLocation`, `Location`, `NpcElement`, `NpcHeroPetAlly`, `NpcAI`, `NpcBehaviour`, `EnemyBaseClass`, `NpcMovement`, `NpcController`, `ARNpcAnimancer`, `HealthElement`, or `DeathElement` as hidden execution owners.

The selected path must eventually own, explicitly and exclusively:

- actor identity, generation, scene parent, activation, and disposal;
- the authored Broodmother/Spider visual and scale;
- navigation and locomotion, including leap displacement and landing;
- the custom animation graph, state completion, root motion policy, and interruption;
- perception, target facts, pack memory, GOAP planning, and PlayMaker procedure execution;
- hurt volumes, attack hit volumes, health, resistances, damage intake, reactions, and outgoing damage;
- spit projectile/VFX lifetime and collision;
- faction/hero-alliance identity and target filtering;
- hit reaction, interruption, death presentation, body cleanup, and no-save scene lifetime;
- Runtime V2 actor registration, `BlazeOwned` lease/binding, cancellation, diagnostics, and repeat-safe teardown.

This list is a required proof surface, not permission to invent implementations or choose APIs. Current `AvalonBlazeRuntimeComposition` proves the lifecycle skeleton only. It adds `AvalonBlazeStayIdleBehaviour`, `BlazeAI`, `Animator`, `NavMeshAgent`, `CapsuleCollider`, and `AudioSource` to a bare object; it does not prove any of the full Spider systems above.

## Implementation gate and offline acceptance criteria

No `BlazeOwned` Spider implementation may begin until a reviewed, source-confirmed wholly Avalon-created actor component contract names the exact source/API and cleanup owner for every item in the selected boundary.

The next gate must require offline fixtures that prove at least:

1. the actor is created inactive and has no native `Location`/`NpcElement` execution owner;
2. visual/rig/animation assets are resolved by an explicit reviewed route, with fail-closed behavior when missing;
3. no second navigation, animator, root-motion, collider-health, damage, reaction, or death owner can be attached;
4. Runtime V2 registration precedes lease acquisition, binding precedes activation, and execution rejects stale IDs/generations/leases;
5. each stable role ID receives its specialized planner/procedure capabilities without falling back to the current static evaluator or native combat prompts;
6. bite, leap, spit, flank, reposition, retreat, recovery, and interruption expose cancellable procedure phases and exactly one damage window;
7. leap owns launch, airborne displacement, collision policy, landing, recovery, and cancellation;
8. spit owns projectile creation, non-placeholder authored VFX, collision, damage, expiry, and cancellation;
9. health/damage/reaction/death have one owner and duplicate terminal transitions are rejected;
10. dismissal, replacement summon, plugin shutdown, actor invalidation, scene unload, package revoke, Runtime stop, and kill switch converge on repeat-safe cleanup;
11. clean disposal leaves no lease, binding, procedure, action, reservation, projectile, VFX, audio voice, blackboard context, component, or actor ID available before Unity destruction semantics permit reuse;
12. persistent cleanup faults remain diagnosed as retained/faulted, not reported as clean;
13. static scans reject `LocationTemplate.SpawnLocation`, `NpcHeroPetAlly`, `NpcAI.EnterCombatWith`, `NpcHeroPetAlly.EnterCombat`, the current basic evaluator, and native `DeathElement` inside the `BlazeOwned` construction/execution path;
14. tests remain offline and do not launch FoA, access saves, deploy files, write installed game paths, or make release claims.

Passing those fixtures would validate a source contract only. Visual quality, animation timing, collision, navigation, combat feel, VFX presentation, and scene integration would still require separately authorized live validation.

## Decision result

- Offline native-owner decompilation: **complete for the managed owners in the current path**
- Spider-named managed owner found: **none in the assembly type inventory**
- Reversible whole-native-actor stop/start surface: **not present**
- Native-shell/Blaze-motion split: **rejected**
- Wholly Avalon-created actor construction: **selected for future `BlazeOwned` roles**
- Full custom Spider component contract: **missing; implementation blocked**
- Existing native death acceptance predicate: **source contradiction found; correction not performed**
- Runtime implementation/deployment/live validation/release readiness: **not authorized or claimed**
- Protected Tales from the Age of Men / Age of Men / overhaul content: **not read or touched**

## Next researched task

Produce the source-confirmed wholly Avalon-created Spider actor composition gate covering exact visual/prefab ownership, custom rig and animation driver, Blaze navigation and leap motion, colliders and custom health/damage/reaction/death, faction and target identity, spit projectile/VFX ownership, scene lifetime, Runtime V2 binding, and idempotent disposal—offline only, with no native `Location`/`NpcElement` fallback.
