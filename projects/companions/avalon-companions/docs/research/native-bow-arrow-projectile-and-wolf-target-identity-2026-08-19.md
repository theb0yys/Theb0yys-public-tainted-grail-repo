# Native Bow/Arrow/Projectile and Wolf Target Identity Trace — 2026-08-19

Document control:
- Date: 2026-08-19
- Owner: `mods/avalon-companions/`
- Branch: `companions`
- Scope: creature/animal companion acquisition research only
- Evidence lanes: `decompilation-static` and existing repository evidence
- Implementation: `NOT_RUN`
- Runtime validation: `NOT_RUN`
- Save/persistence validation: `NOT_RUN`
- Avalon AI source modified: no
- Dialogue Framework source modified: no

## Requested outcome

Trace:

1. one exact native FoA bow/ammunition/projectile execution path suitable for a later creature-acquisition consumer that borrows the game's item, weapon, inventory and projectile services; and
2. one exact wolf template and live-actor identity path suitable for the hunter capture tutorial.

This record does not implement the bow, arrow, subdual channel, quest, unconscious transition, AI package, dialogue interaction, persistence, or companion acquisition.

## Ownership boundary

The ecosystem's existing one-system-one-truth boundary remains controlling:

```text
Avalon AI owns AI.
The Dialogue Framework owns dialogue.
Avalon Human Companions owns human companions.
Avalon Companions owns creature and animal companions.
```

Ownership attaches to domain truth, not to whichever caller happens to use a service. `Avalon Cheat Panel` does not own item discovery; it borrows FoA's `TemplatesProvider`, item and inventory services. Avalon Companions likewise may consume item/template lookup, weapon/ammunition, projectile, quest, UI, unconscious, AI and dialogue services without taking ownership of any of those truths.

This research identifies only the native physical/data seams Avalon Companions must consume for creature/animal acquisition. Avalon Companions owns only creature and animal companion truth: companion identity, lifecycle, roster, bond/relationship, progression, companion-domain acquisition state and persistence. It does not define or alter AI, dialogue, item, weapon, quest, inventory, UI or human-companion ownership.

## Evidence identities

### Current managed assembly

```text
file:   TG.Main(3).dll
size:   9,058,304 bytes
sha256: 749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982
runtime family: PC Mono / managed TG.Main
```

The assembly was inspected read-only through CLR metadata and IL. No game process was launched and no binary was modified.

### Repository evidence

The exact wolf row and current companion runtime-identity seam were cross-checked against the `companions` branch.

## Result matrix

| Claim | State | Evidence ceiling |
|---|---|---|
| Native bow/quiver/projectile execution chain identified | `PASSED` | decompilation-static for the fingerprinted assembly |
| Shooter and source-weapon attribution identified | `PASSED` | decompilation-static |
| Collider -> `IAlive` -> `NpcElement` hit path identified | `PASSED` | decompilation-static |
| Projectile lifetime/cleanup path identified | `PASSED` | decompilation-static |
| Same-build loaded `ItemTemplate` discovery path identified | `PASSED` | current Avalon Cheat Panel source borrowing FoA `TemplatesProvider` |
| One concrete current player-facing bow template GUID selected | `NOT_RUN` | existing runtime item-browser/service path can supply it; selection has not been executed |
| One concrete current arrow/quiver template GUID selected | `NOT_RUN` | existing runtime item-browser/service path can supply it; selection has not been executed |
| Wolf template identity selected | `PASSED` | repository/static diagnostic evidence |
| Wolf is regular/non-abstract and non-unique | `PASSED` | existing diagnostic review evidence |
| Same-session live wolf actor identity seam selected | `PASSED` | current repository source: `Location.ID` plus exact `NpcElement` reference |
| Exact tutorial wolf selected and bound in-game | `NOT_RUN` | requires quest/runtime work |
| `Location.ID` survives save/load as a durable identity | `NOT_RUN` | must not be assumed |
| Bow clone, hit attribution, subdual, unconsciousness and capture validated | `NOT_RUN` | requires separately authorised runtime proof |

## Exact native bow/ammunition/projectile path

### 1. Bow state-machine setup

`Awaken.TG.Main.Animations.FSM.Heroes.Machines.BowFSM.OnInitialize` (MethodDef RID 35924) builds the native bow state machine, including:

- idle/movement;
- equip and unequip;
- pull;
- hold;
- release;
- cancel draw;
- empty/no-ammunition state.

It also preloads the default projectile route through `ItemProjectile`.

### 2. Fire request

`BowFSM.FireProjectile` (RID 35936):

1. reads the equipped main-hand weapon's visual fire point;
2. falls back to `BowFSM.FirePoint` when required;
3. computes base projectile velocity as `60 * FireStrength`;
4. calls `CalculateArrowVelocity`;
5. starts `InstantiateProjectile` with the fire point, velocity, and offset data.

### 3. Quiver ammunition selects the projectile

`BowFSM.<InstantiateProjectile>d__66.MoveNext` resolves the projectile as follows:

```text
if current quiver exposes ItemProjectile:
    ItemProjectile.GetProjectile(...)
else:
    ItemProjectile.GetDefaultArrow(...)
```

The returned `CombinedProjectile.logic` object is passed to `BowFSM.FireProjectileInternal`.

The selected ammunition is therefore not inferred from a display name. It is the actual equipped quiver `Item` whose template/component data supplies `ItemProjectile`.

### 4. `ItemProjectile` loads and combines logic and visual assets

Relevant methods:

- `ItemProjectile.GetProjectile` — RIDs 45578 and 45579;
- `ItemProjectile.GetLogicPrefab` — RID 45580;
- `ItemProjectile.GetDefaultArrow` — RIDs 45581 and 45582;
- `ItemProjectile.GetInHandProjectile` — RID 45571;
- async state machines:
  - `<GetProjectile>d__35.MoveNext` — RID 45619;
  - `<GetProjectile>d__36.MoveNext` — RID 45621;
  - `<GetLogicPrefab>d__37.MoveNext` — RID 45617;
  - `<CombinePrefabs>d__43.MoveNext` — RID 45597.

The current IL establishes this path:

```text
quiver ItemProjectile
  -> load logic asset reference
  -> load visual projectile
  -> create ProjectileData
  -> combine logic + visual
  -> require Projectile component on logic object
  -> attach release-on-destroy ownership
  -> return CombinedProjectile
```

`GetLogicPrefab` keeps the loaded rigidbody kinematic until the native launch/configuration path completes.

### 5. Authoritative launch, owner, weapon, ammunition and damage context

`BowFSM.FireProjectileInternal` (RID 35944):

1. reads the currently equipped `EquipmentSlotType.Quiver` item;
2. resolves the `Projectile` component from the combined logic object;
3. calls `SetVelocityAndForward`;
4. assigns the projectile owner to the firing `Hero`;
5. calls `SetBaseDamageParams` with:
   - the main-hand weapon item;
   - the equipped quiver item;
   - current bow draw strength;
6. when the projectile is an `Arrow`, calls `Arrow.SetItemTemplate(quiverItem.Template)`;
7. calls `FinalizeConfiguration`;
8. decrements the quiver item quantity;
9. resets fire strength.

This provides the complete per-shot provenance needed by a later subdual gate:

```text
shooter       = projectile Owner / firing Hero
source weapon = main-hand Item
ammunition    = equipped quiver Item + ItemTemplate
projectile    = instantiated Projectile / Arrow object
```

### 6. Quiver changes and in-hand presentation

`BowFSM.UpdateArrowProjectile` (RID 35947) watches the equipped quiver. When the quiver's `ItemProjectile` changes, it releases the old preload and preloads the new projectile route. Missing custom projectile data returns to the default arrow route.

`CharacterBow.<VerifyArrows>d__32.MoveNext` independently verifies the current hero's quiver and quantity, obtains either the quiver's `ItemProjectile` in-hand representation or the default arrow, and parents/configures that visual in the bow-hand pipeline.

This means a future subdual arrow should preserve both:

- the fired logic projectile route; and
- the in-hand/quiver visual route.

### 7. Cast result and contact dispatch

`DamageDealingProjectile.CheckCastResult` (RIDs 55554 and 55555):

- rejects prevented/no-collider/self-character-view contacts;
- forwards valid hits to virtual `OnRaycastHit`.

`DamageDealingProjectile.OnRaycastHit` (RID 55557):

- marks contact;
- moves the projectile to the hit point;
- invokes virtual `OnContact`;
- emits contact noise;
- stops projectile VFX.

`Arrow.OnRaycastHit` (RID 55378) retains the base contact path and sends the arrow's visual-graph hit event.

### 8. Collider -> `IAlive` -> damage/target identity

`Arrow.OnContact` (RID 55379):

1. reads the `HitResult.Collider`;
2. resolves an `Awaken.TG.Main.Character.IAlive` model from that collider;
3. rejects duplicate alive targets through its `_alivesHit` list;
4. builds projectile damage parameters containing draw strength, direction, force and damage type;
5. calls `DamageUtils.TryDoDamage` with:
   - the resolved `IAlive`;
   - hit collider;
   - raw projectile damage;
   - projectile owner;
   - source weapon;
   - this projectile;
6. routes unresolved living targets to environment-hit handling;
7. calls `OnContactEnding`.

The projectile target is therefore not identified merely by collider name or template name. The authoritative living-target seam is the resolved `IAlive` object. For an ordinary creature NPC, the concrete model can be an `NpcElement`.

### 9. Public projectile-contact event seam

`Arrow.OnContactEnding` (RID 55380) constructs:

```text
ProjectileContactParams(
    projectile,
    hitCollider,
    hitAlive)
```

and triggers `ICharacter.Events.OnProjectileContact` on the projectile owner.

`ProjectileContactParams.HitAlive` is typed as `IAlive`.

This is the narrowest identified post-contact observation seam because it preserves:

- exact shooter/owner;
- exact projectile instance;
- exact hit collider;
- exact resolved living target.

A future acquisition listener must still verify the ammunition identity and quest/capability gate. Listening to projectile contact alone must not make every arrow a subdual arrow.

### 10. Target handling and cleanup

`Arrow.OnTargetHit` (RID 55382) handles reflection, piercing/embedding, parenting and release scheduling.

Cleanup paths include:

- `Arrow.OnLifetimeEnd` (RID 55386);
- `Arrow.ReleaseSelf` (RIDs 55387 and 55388);
- `Projectile.BeforeGameObjectDestroy`;
- release of location-attached projectile references;
- destruction of the projectile game object;
- optional conversion to a non-saved pickable arrow through `ChangeToPickable`.

The future subdual implementation must not retain projectile object references after these cleanup paths. Any durable acquisition record must use companion-owned identifiers, not the projectile's Unity object identity.

## Existing borrowed source-pair selection route

The managed assembly establishes the execution contract and contains abstract/category terminology such as:

- `AbstractShortBow`;
- `AbstractArrow`;
- `AbstractAmmunition`;
- `ManualAmmunition`.

Those strings are not a concrete player-facing source item pair and do not establish usable template GUIDs.

The repository already contains a same-build runtime consumer in Avalon Cheat Panel. Its item browser reads the loaded corpus through:

```text
TemplatesProvider.GetAllOfType<ItemTemplate>()
```

and retains each safe row's:

- `TemplateGuid`;
- display name;
- category;
- base price;
- internal template name.

The cheat panel does not own that catalogue. It borrows FoA's native template and inventory services for diagnostic browsing and item grants. Avalon Companions must not duplicate that browser or promote the cheat panel into an item authority. It may consume the reviewed output of that existing service path: one concrete non-abstract bow identity and one compatible non-abstract arrow/quiver identity.

The source-pair selection state is therefore `NOT_RUN`, not blocked on a missing exporter. No concrete GUID is invented in this record.

## Exact wolf identity path

### Template identity

The reviewed tutorial species candidate is:

```text
Template name: Spec_AnimalWolf
Template GUID: 9086dee514edc644b9b55890d885db3f
```

Existing repository evidence records:

- 26 FoA diagnostic route-context spawner references;
- regular, non-abstract `LocationTemplate` classification;
- `RepetitiveNpcAttachment` actor evidence;
- `npcIsUnique=false`;
- one-session candidate status.

This establishes the template class that the hunter tutorial may target. It does not select one physical wolf instance.

### Live actor identity

The current Avalon Companions runtime boundary already treats a live companion actor as a `Location` and exports its non-empty `Location.ID` as the actor runtime identity.

`TryInspectAiRuntimeActorIdentity`:

1. resolves the active managed `Location`;
2. rejects discarded, unowned, ambiguous or blank-ID actors;
3. returns `Location.ID.Trim()`.

The current V2 host then namespaces that runtime identity as:

```text
ActorId = "foa.location:" + Location.ID
```

For the hunter tutorial, the correct identity layers are therefore:

| Layer | Identity | Purpose |
|---|---|---|
| Species/template eligibility | `LocationTemplate.GUID = 9086dee514edc644b9b55890d885db3f` | confirms the actor is the reviewed ordinary wolf family |
| Exact live tutorial target | `Location.ID` | distinguishes one physical wolf from every other wolf using the same template |
| Hit model | exact `NpcElement` / `IAlive` reference resolved from projectile contact | confirms the arrow actually hit the bound tutorial actor |
| Future durable identity | mod-owned `CompanionId` | created only after successful taming; not interchangeable with template or runtime IDs |

`LocationTemplate.GUID`, `Location.ID`, `NpcElement` reference, and future `CompanionId` must never be collapsed into one identifier.

### Tutorial-target binding contract

The later hunter capture tutorial should bind one exact live wolf before subdual is permitted:

```text
required quest stage is active
AND source bow identity matches the reviewed identity supplied by the item/weapon service
AND ammunition identity matches the reviewed identity supplied by the item/weapon service
AND projectile owner is Hero.Current
AND bound Location.ID matches the quest's current target
AND bound Location.Template.GUID == 9086dee514edc644b9b55890d885db3f
AND ProjectileContactParams.HitAlive is the bound target's NpcElement
AND target passes creature safety/exclusion checks
```

This prevents a shot at another wolf of the same template from satisfying the exact tutorial capture unless the quest explicitly rebinds the target.

## Required negative controls

| Control | Required result |
|---|---|
| ordinary vanilla bow + ordinary arrow | no Avalon subdual state |
| approved bow + wrong ammunition | no Avalon subdual state |
| approved ammunition fired by a non-player actor | no player acquisition state |
| correct projectile outside the exact quest stage | no tutorial acquisition state |
| correct template but different `Location.ID` | tutorial target unchanged; no completion |
| environment hit with no `IAlive` | no target subdual state |
| hit on player/projectile owner | rejected by native/self and acquisition gates |
| unique, boss, story, undead or otherwise blocked actor | rejected before unconscious mutation |
| target discarded or `Location.ID` changed | pending target invalidated fail-closed |
| stale projectile callback after transition | ignored through acquisition-session generation/lease checks |

## Companion ownership and borrowed-service boundary

Avalon Companions owns only creature and animal companion truth:

- the creature/animal companion identity and durable `CompanionId`;
- the exact creature actor binding used by a companion acquisition session;
- companion-domain acquisition states such as candidate, interaction pending, accepted, rejected and acquired;
- species/provider interpretation of validated events and offers;
- bond, relationship, roster, lifecycle, progression and companion persistence/reconciliation.

Avalon Companions borrows or consumes services and facts owned elsewhere:

- FoA `TemplatesProvider`, item, inventory and equipment services for item truth;
- the applicable weapon/ammunition service for bow, arrow and projectile identities and physical subdual delivery;
- the quest/notice-board service for hunter quest stages, objectives and unlock truth;
- native FoA unconscious and actor-state operations for physical execution;
- Avalon AI packages and services for AI decisions;
- the Dialogue Framework service for dialogue and interaction presentation;
- the appropriate UI service for player-facing screens.

Using a service does not transfer ownership. Avalon Companions must not create a second item catalogue, weapon truth, quest truth, AI system, dialogue system, UI framework or human-companion model. It may validate that borrowed service results refer to the exact current creature acquisition session before applying a companion-owned state transition.

No external owner source was changed by this task.

## Exact downstream evidence required

### Content identity

Use the existing same-build FoA item-service consumer already exposed by Avalon Cheat Panel, or an equivalent read-only call to the same native service, to select:

- one non-abstract short-bow source template;
- one non-abstract arrow/quiver source template with the required `ItemProjectile` path;
- exact GUID, name, tags/components and equipment-slot behaviour for both.

Do not add an item browser, item catalogue or template exporter to Avalon Companions.

### Runtime source-pair proof

On a throwaway save, prove the selected unmodified native pair through:

```text
inventory
-> equip bow
-> equip quiver
-> draw
-> in-hand arrow visual
-> fire
-> combined projectile
-> Hero owner attribution
-> quiver/template attribution
-> hit collider
-> HitAlive/NpcElement
-> quantity decrement
-> cleanup/pickable behaviour
```

### Clone proof

Only after the native pair passes, the system that owns item/weapon content may provide the required subdual bow and ammunition identities. That owner must:

- create or clone the required item identities without mutating the vanilla originals;
- prove the originals remain unchanged;
- prove normal arrows never invoke the subdual route;
- prove the supplied projectile attribution remains exact.

Avalon Companions consumes those reviewed identities and events; it does not become the item or weapon owner.

### Wolf-target proof

- select one live `Spec_AnimalWolf` instance;
- bind its `Location.ID` to the exact tutorial session;
- prove projectile `HitAlive` resolves to that wolf's `NpcElement`;
- prove a second wolf with the same template is rejected;
- prove transition/despawn invalidates the binding.

### Unconscious and persistence proof

These remain separate later lanes:

- wolf support for the native unconscious physical path;
- scoped suppression of vanilla unconscious behaviour/wake ownership;
- controlled recovery;
- exact actor conversion;
- `CompanionId` save/load and transition reconciliation.

## Disposition

The native execution and hit-attribution architecture is sufficiently traced for planning. The wolf template and same-session live-actor identity model are sufficiently traced for the controlled tutorial design.

Implementation remains `NOT_RUN`. Concrete bow/ammunition selection is ready through the existing borrowed item-service path, while all runtime proof remains outstanding.

## Next bounded task

Use the existing Avalon Cheat Panel item browser as a consumer of FoA's native `TemplatesProvider` service to identify and record one exact non-abstract short bow and one exact compatible non-abstract arrow/quiver source pair, then prepare the source-pair runtime diagnostic matrix. Do not add an item catalogue or exporter to Avalon Companions, transfer item/weapon ownership to the cheat panel or companion system, implement subdual, modify Avalon AI, or create a fallback dialogue system.
