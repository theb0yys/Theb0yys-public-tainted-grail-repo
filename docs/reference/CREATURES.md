# Creatures: Proven Injection Gates, Native Actor Lifecycle, and Provider Ownership

> **Reference/process page.** This page reconstructs the evidence-backed custom-creature process from the maintainer's private working repository. Creature work has the clearest explicit gated process of the four content domains, but that does **not** mean every creature is live-complete. Each gate proves only its own lane.

## Document status

**Domain:** creatures / NPCs / hostile actors / companions / population  
**Canonical process shape:** **CI1 → CI2 → CI3 → CI4A → CI4 → CI5 → focused live gate**  
**Process status:** evidence-backed operating process  
**Runtime status:** creature-specific and lane-specific  
**Provider model:** creature assets/templates/resolvers/lifecycle remain provider-owned  
**Persistence:** one-session proof actors remain save-excluded unless a separate persistence gate proves otherwise  
**Population:** separate from controlled one-session actor proof  
**Companion behavior/UI:** separate consumer route with its own full-path requirements

The public process must not turn “a complete gate sequence exists” into “every creature has passed it.”

---

# 1. What a custom creature actually requires

A creature is not:

~~~text
prefab
→ spawn
~~~

The full problem is closer to:

~~~text
source package
→ source/provenance intake
→ native baseline decision
→ visual transport
→ animation-state mapping
→ NpcTemplate
→ LocationTemplate
→ provider identity/resolver
→ controlled actor construction
→ native actor initialization
→ movement/AI
→ combat
→ hit reaction
→ death
→ corpse handoff
→ cleanup
→ placement/population ownership
→ persistence/save policy
→ optional consumer behavior
~~~

Every arrow is an independent claim.

---

# 2. Provider versus consumer ownership

The creature-provider layer owns:

- imported creature assets;
- visual roots;
- animation mapping;
- `NpcTemplate`;
- `LocationTemplate`;
- stable creature identities;
- runtime resolvers;
- provider lifecycle contracts;
- provider cleanup/source of truth.

Gameplay consumer mods may own:

- companion commands;
- hunt/encounter behavior;
- route/world use;
- merchant distribution;
- quests;
- UI;
- audio;
- summon-like entry points;
- other player-facing features.

Consumers must **not**:

- copy provider templates and become a second source of truth;
- fork provider GUID identity;
- invent raw fallback templates;
- mutate provider identities;
- silently substitute a different creature when resolution fails.

The handoff should be through a reviewed provider/API or bounded reflection boundary.

---

# 3. One creature or cohesive family per consumer

Unrelated creatures must not be bundled into one arbitrary gameplay owner simply because they use the same provider.

A shared consumer is justified only when the scope is a real family with:

- shared identity model;
- shared lifecycle;
- shared UX;
- shared validation evidence;
- an explicit gate naming that family.

Spider/Broodmother is an example of a family-shaped route.

“Everything imported by Avalon Awakened” is not a creature family.

---

# 4. The complete CI gate sequence

~~~text
CI1  Source Intake
CI2  Suitability / Native Baseline
CI3  Visual Transport
CI4A Animation Mapping
CI4  Template + Actor Contract
CI5  Runtime Lifecycle
Live Focused Runtime Gate
~~~

No gate may inherit authority from a later one.

Examples:

~~~text
CI3 pass
!= template pass

CI4 pass
!= spawn pass

CI5 source/build pass
!= live combat pass

live spawn
!= save persistence
~~~

---

# 5. CI1 — Source intake

## Goal

Establish exactly what source content exists.

## Record

At minimum:

- package/project identity;
- source hash;
- source version/date;
- licence/redistribution state;
- Unity/source-tool version where relevant;
- source prefabs;
- meshes/skinned meshes;
- skeleton/root facts;
- materials/textures;
- animation clips;
- audio;
- colliders;
- obvious missing scripts/dependencies;
- intended creature/variant scope.

## Do not claim

CI1 does not prove:

- import suitability;
- native template compatibility;
- animation integration;
- actor construction;
- AI;
- combat;
- death;
- save;
- distribution.

## Acceptance

The source is preserved, fingerprinted and understood well enough to ask the next question.

---

# 6. CI2 — Suitability and native baseline

## Goal

Choose the FoA native behavior/profile to preserve.

The baseline must be selected by native contract, not by visual resemblance.

## Record

Depending on creature type:

- native template family;
- abstract type;
- faction/hostility;
- stats;
- fighting style;
- manoeuvre/action profile;
- collider/controller expectations;
- `LocationTemplate` attachment order;
- death/corpse policy;
- body/rendering technology;
- scale policy;
- placement constraints;
- explicit blockers.

## Example principle

Spider/Broodmother used a Bear-derived hostile baseline because the relevant behavior/template contract was the reviewed fit.

The lesson is not “all spiders should copy Bear.”

The lesson is:

~~~text
choose a native baseline through evidence
and preserve the complete relevant contract
~~~

## Acceptance

A specific baseline is approved for the next proof slice.

---

# 7. Baseline selection failures

Do not choose the baseline only because:

- body size is similar;
- attack style looks similar;
- source asset has an Animator;
- filename says monster;
- another mod used the same animal once.

Wrong baseline selection can break:

- AI;
- controller expectations;
- colliders;
- combat state;
- death;
- corpse;
- faction;
- placement.

---

# 8. CI3 — Visual transport

## Goal

Prove the mod-owned visual assets can be built, loaded, inspected and released through the supported content route.

## Requirements

- use the reviewed FoA/Unity baseline;
- build only mod-owned/redistributable output;
- assign explicit roots/addresses;
- record catalogue/bundle hashes;
- validate dependencies;
- detect missing scripts;
- validate renderers/materials/shaders;
- load/release in at least the required clean cycles;
- require stable fingerprints where the gate defines them.

## Acceptance

The visual asset can survive the isolated load/release contract.

## Explicit boundary

CI3 proves **visual transport only**.

It does not prove:

- `NpcTemplate`;
- `LocationTemplate`;
- actor;
- spawn;
- combat;
- death;
- population;
- save.

---

# 9. Why two-cycle load/release matters

One successful load can hide:

- stale Editor state;
- unreleased handles;
- duplicate resource state;
- non-deterministic catalogue issues;
- cleanup failures.

Repeated clean load/release with the same expected fingerprint provides stronger asset-lifecycle evidence.

It still does not prove a game actor.

---

# 10. CI4A — Native animation mapping

FoA creature animation ownership uses native mapping, not arbitrary source `AnimatorController` playback.

The proven state IDs for the scoped custom-creature work include:

~~~text
Idle        = 1
Movement    = 2
ShortRange  = 16
GetHit      = 32
Death       = 44
~~~

## Goal

Map source clips to the native state semantics.

## Typical requirements

- neutral idle;
- movement mixer/children;
- short-range attack(s);
- hit reaction(s);
- death animation(s);
- exact mapping identity;
- build/load/release proof.

## Deferred clips

A source package can contain more clips than FoA states proven for the profile.

Examples such as jump must remain deferred until a native state/route is established.

Do not “use every clip because it exists.”

---

# 11. Movement mapping

The evidenced movement route uses a Cartesian/turning native mapping.

Example shape:

~~~text
Idle child      threshold 0
Walk child      threshold 1
Run child       threshold 8
~~~

Exact values are profile evidence, not universal values for every creature.

The important process rule is that movement animation is mapped into the native state owner instead of playing a disconnected source controller.

---

# 12. Attack timing boundary

An attack clip mapped to `ShortRange(16)` does not automatically prove damage timing.

If a claim includes:

- attack release;
- hit frame;
- recovery;
- damage application;

then those need their own timing/runtime evidence.

Animation playback is not damage proof.

---

# 13. CI4A acceptance

A mapping asset:

- builds;
- loads;
- validates expected state coverage;
- releases cleanly.

No actor/template/runtime claim is made.

---

# 14. CI4 — NpcTemplate and LocationTemplate contract

CI4 moves from presentation into native definition identity.

The key pair is:

~~~text
NpcTemplate
+
LocationTemplate
~~~

Both need explicit custom identity.

## NpcTemplate

Owns creature/NPC definition-level facts.

## LocationTemplate

Owns the native location/actor construction shell.

The exact composition/order of native location attachments can matter.

---

# 15. CI4 authoring rule

Author the custom templates from the reviewed native baseline.

Preserve:

- required location component order;
- required native behavior mapping;
- fighting style;
- animation mapping;
- collider/controller contract;
- scale policy;
- visual root references;
- exact identity.

Change only the intended custom fields.

---

# 16. CI4 identity

Every custom pair requires stable:

- `NpcTemplate` GUID;
- `NpcTemplate` name;
- `LocationTemplate` GUID;
- `LocationTemplate` name;
- addresses/catalogue identities as required.

Do not reuse a native identity for a genuinely new creature.

---

# 17. Historical identity versus current identity

Creature evidence has already demonstrated why identity must be versioned.

Spider/Broodmother had an isolated 2026-08-03 proof build with one four-GUID set.

Later provider/catalogue builds used a newer set.

The old proof remains valid **for that historical proof build**.

It must not be silently rewritten.

Likewise, a newer installed-catalogue finding must not be promoted to universal current public authority until its review/promotion lifecycle is complete.

The process rule is:

~~~text
identity is version/build scoped
historical proof stays historical
current consumers must match the promoted provider identity
~~~

---

# 18. CI4 template validation

A template proof should verify:

- exact GUIDs/names;
- expected labels/addresses;
- native baseline component order;
- animation mapping reference;
- fighting style;
- visual roots;
- scale policy;
- load/release;
- no missing references required by the profile.

## CI4 boundary

Even a perfect isolated template proof does not establish:

- runtime registration/provider;
- actor construction;
- spawn;
- placement;
- AI;
- combat;
- death;
- corpse;
- loot;
- population;
- persistence.

---

# 19. Provider identity and resolver contract

The provider should expose a strict resolver.

The resolver must:

1. identify the exact expected `LocationTemplate`;
2. resolve it through the supported catalogue/template route;
3. verify returned name/GUID;
4. later verify the initialized actor's expected `NpcTemplate`;
5. fail closed on mismatch.

Do not return “closest matching template.”

---

# 20. No raw fallback rule

Consumers must not bypass provider failure by:

- loading a prefab path directly;
- using an old GUID;
- falling back to a native creature with a similar name;
- constructing an unreviewed LocationTemplate locally.

If the provider cannot resolve the exact reviewed identity:

~~~text
fail
do not fork identity
~~~

---

# 21. Provider/consumer identity conflict lesson

A real conflict occurred where:

- provider/catalogue moved to newer Spider/Broodmother identities;
- a consumer still embedded older LocationTemplate GUIDs;
- consumer code required strict equality.

That means the consumer would fail closed once it reached resolution.

The correct response is:

- freeze dependent use;
- preserve historical evidence;
- open correction review;
- update every active consumer as one bounded impact wave;
- rebuild/revalidate;
- do not use “newest file wins.”

This is a textbook identity-governance lesson.

---

# 22. CI5 — runtime lifecycle

Only after exact template/animation/visual contracts are established should runtime actor construction begin.

The runtime must:

- resolve exact template;
- construct through the approved native actor/location route;
- validate exact identity after construction;
- mark proof actors save-excluded where the gate requires it;
- fail closed on missing components;
- preserve native behavior ownership unless a separate gate explicitly changes it.

---

# 23. Controlled actor construction

The one-session controlled route follows a pattern like:

~~~text
resolve exact LocationTemplate
→ request controlled spawn/construction
→ native Location created
→ mark Location not saved immediately
→ wait/validate initialization
→ resolve NpcElement/actor
→ verify exact LocationTemplate + NpcTemplate
→ verify required lifecycle components
→ arm observation
~~~

Construction is not the same as population injection.

---

# 24. Immediate save exclusion

For one-session proof actors, save exclusion is part of the construction contract.

The actor/location should be marked not saved immediately, not after the test has already run for several seconds.

Runtime evidence should read back that save exclusion state.

---

# 25. Why one-session proof is useful

A disposable actor route allows testing:

- identity;
- rendering;
- animation;
- AI;
- combat;
- death;
- corpse;
- cleanup;

without prematurely creating a persistent world-content contract.

That reduces save/world risk.

---

# 26. Actor invariant checks

Before claiming a runtime creature ready, validate the components/invariants required by that profile.

Examples may include:

- expected NPC element;
- controller;
- renderer/Kandra state;
- VFX body marker;
- death element;
- ragdoll/death behavior;
- health;
- native AI availability;
- corpse policy.

Missing required lifecycle components should block the proof.

---

# 27. Native behavior ownership

The controlled hostile path preserves native ownership of:

- movement;
- target selection;
- combat entry;
- attack selection;
- damage;
- hit reaction;
- lethal transition;
- animated death;
- corpse handoff.

The proof observer should not secretly make the creature work by forcing all those systems.

Otherwise it proves the mod's replacement behavior, not the creature's native baseline integration.

---

# 28. Combat entry

A focused live gate should observe natural/approved native combat entry.

Record:

- distance/context;
- target;
- AI working state;
- whether combat is already active;
- whether any force flag was used;
- whether movement/target/faction overrides were applied.

If forced combat is part of a separate profile, state it explicitly.

---

# 29. Attack proof

For each claimed attack:

- native state/attack selected;
- animation plays;
- timing is valid;
- actual damage is observed if damage is claimed;
- no custom damage injection was used unless that is the named lane.

An attack animation without damage evidence is presentation-only.

---

# 30. Hit reaction

A complete hostile lifecycle should observe:

- incoming damage;
- native health reduction;
- `GetHit(32)` or equivalent approved hit reaction;
- continued actor validity afterward.

Do not claim hit-reaction integration from the existence of a clip.

---

# 31. Death lifecycle

The proven hostile/world target uses native death ownership.

The lifecycle includes:

~~~text
lethal native damage
→ death state
→ Death(44) animation
→ native death element/logic
→ NpcElement handoff
→ NpcDummy + Corpse state where profile requires it
→ retained same-session body if configured
→ lifecycle owner enters terminal state
~~~

This must be observed for a live death claim.

---

# 32. Corpse handoff

The exact corpse contract matters.

A creature that disappears on death is not equivalent to a creature profile that requires a retained corpse.

The live proof should record:

- `NpcDummy`/corpse presence as appropriate;
- retained-body policy;
- whether the same Location remains valid;
- cleanup owner after accepted death.

---

# 33. Death cleanup defect lesson

Source review found cases where active tracking entries were removed directly after accepted native death or identity change instead of routing through the established release owner.

That matters because the release owner also disposes lifecycle systems.

The rule is:

~~~text
terminal tracking removal
must go through the owner that performs teardown
~~~

Do not remove the dictionary entry and leave lifecycle objects alive.

---

# 34. CI5 focused live gate

A complete scoped live gate should prove the claims it names.

For a hostile creature this can include:

1. exact spawn request;
2. exact identity;
3. placement;
4. clearance;
5. save exclusion;
6. initialization;
7. visible name/rendering;
8. idle/movement;
9. combat entry;
10. every claimed attack;
11. damage;
12. hit reaction;
13. lethal damage;
14. death animation;
15. corpse handoff;
16. retained-body behavior;
17. cleanup/discard;
18. duplicate prevention;
19. terminal lifecycle disposal.

A source build cannot substitute for this gate.

---

# 35. Placement is a separate proof lane

Actor validity does not prove safe world placement.

Placement can require evidence for:

- scene/worldspace;
- coordinate;
- rotation;
- clearance;
- ground;
- nav/path;
- nearby blockers;
- large creature footprint;
- quest/story ownership;
- road/settlement context.

Do not call “spawn succeeded” placement proof.

---

# 36. Grounding and clearance

A native helper that returns a position is not necessarily complete grounding proof.

The process may require:

- ray/surface evidence;
- footprint clearance;
- path reachability;
- visual acceptance;
- large-body checks.

Failure to fall through the world once is not enough.

---

# 37. Controlled one-session actor versus population

These are different ownership models.

## Controlled proof actor

- explicit request;
- one/few instances;
- immediate save exclusion;
- mod-owned lifecycle lease;
- explicit cleanup;
- no population claim.

## Native population integration

- native spawner/population owner;
- spawn policy;
- density;
- region;
- persistence behavior;
- native cooldown/respawn semantics;
- duplication/coexistence.

Passing the controlled proof does not authorize population rows.

---

# 38. Native population lane

A later population integration may append or configure exact native candidate arrays/spawner inputs instead of calling `SpawnLocation` directly.

That changes ownership:

~~~text
native spawner owns when/how actor exists
~~~

This can imply save-owned or world-owned behavior different from a one-session actor.

Therefore population requires its own gate and persistence analysis.

---

# 39. No duplicate population source

If a creature moves from a custom route/patrol source into native population ownership, the old source must be stood down as required.

Otherwise:

- native spawner;
- route system;
- encounter system;

may all create the same creature population.

“One system, one truth” applies here too.

---

# 40. Creature family scaling

Spider/Broodmother demonstrates a family-style model:

- same source family;
- common creature tier;
- larger Broodmother tier;
- shared animation/profile concepts;
- separate stable template identities.

Scale is part of the reviewed creature contract.

Do not invent arbitrary scale multipliers at runtime without validating collider/controller/combat implications.

---

# 41. Variant ownership

When multiple visual variants exist:

- every variant needs stable address identity;
- all variants must satisfy the same required renderer/script contract;
- scale policy must be explicit;
- selection policy belongs to the provider/consumer named by the gate;
- one bad variant must not be hidden by other passing variants.

---

# 42. Creature audio boundary

Creature audio is a separate integration lane.

Possible owners include:

- native template audio;
- mod-owned sidecar playback;
- shared audio framework.

A custom creature visual/actor proof does not prove audio.

If native template audio mutation is out of scope, a companion may use plugin-owned audio resources without changing the provider template.

---

# 43. Creature VFX boundary

VFX may depend on:

- visual-root markers;
- native body markers;
- attack state;
- death;
- separate asset package.

Again:

~~~text
actor works
!= VFX integration works
~~~

---

# 44. One-session companion route

A companion is a **consumer** of the creature provider.

The proven process adds an additional full path:

~~~text
provider resolves exact custom creature
→ explicit player-owned summon/command trigger
→ one-session actor construction
→ MarkedNotSaved
→ exact identity validation
→ native ally/pet conversion for this instance
→ one-active-instance ownership
→ runtime Companion prompt
→ command UI
→ input/cursor ownership
→ command dispatch
→ follow/hold/defend/etc.
→ dismiss/death/disable/shutdown cleanup
~~~

This is not the same as hostile/world population integration.

---

# 45. Companion provider rule

The companion consumer:

- calls the provider resolver;
- uses exact returned template;
- does not copy provider templates;
- does not add raw fallback;
- does not mutate provider identity.

Provider owns content.

Companion owns behavior.

---

# 46. Companion save policy

The established first route is:

~~~text
one-session
in-memory tracking
MarkedNotSaved
disposable
~~~

unless a separate persistence gate explicitly proves a durable companion model.

Do not turn same-session survival into a save claim.

---

# 47. One active instance

The companion consumer should maintain one active instance per owned slot/creature contract as defined by the profile.

Required duplicate prevention includes:

- active actor identity check;
- replacement/swap behavior;
- stale-reference cleanup;
- no second hidden spawn;
- no save reload auto-duplication.

---

# 48. Ally conversion scope

Only the specifically spawned/verified actor instance should receive companion/ally semantics.

Do not globally mutate:

- species faction;
- all templates;
- all world instances.

The source wild creature and provider identity remain unchanged.

---

# 49. Companion prompt lifecycle

The proven route uses a runtime-only `Companion` interaction/action.

Requirements include:

- attached only to active verified actor;
- action itself not saved;
- interactability active only while appropriate;
- stale prompt removed on invalid actor;
- original interactability restored on cleanup;
- prompt removed on dismiss/death/disable/shutdown.

A visible prompt is only one segment of the player-facing path.

---

# 50. Command UI full-path rule

A copied menu or button layout does not prove a working companion UI.

The complete path includes:

~~~text
prompt
→ UI open
→ scope ownership
→ cursor
→ input lock/pass-through
→ EventSystem / pointer
→ button dispatch
→ command handler
→ command execution
→ UI resync
→ close
→ cursor/input restore
~~~

Every segment matters.

---

# 51. Cursor/input ownership

The working companion path uses an explicit ownership order.

Conceptually:

1. shared custom-UI scope by soft integration where available;
2. fallback shared UI scope when appropriate;
3. native cursor visibility;
4. capture current cursor state;
5. reassert while UI visible;
6. restore on close;
7. lock gameplay/player input;
8. allow UI/EventSystem/Rewired input required for the menu.

The exact optional integration may evolve, but the ownership requirement remains.

---

# 52. Event dispatch

The menu can route:

- Unity `Button.onClick`;
- bounded manual real-pointer hit test;

to the same command handler.

Duplicate suppression should be narrow, such as same-frame suppression.

Do not build a chain of speculative pointer-release/mouse-up/virtual-cursor patches when the actual issue is upstream ownership.

---

# 53. Approved companion command classes

A bounded companion menu may include commands such as:

- Follow;
- Hold Position;
- Defend;
- Keep Close;
- Keep Pace;
- Keep Distance;
- Come Close;
- Recall;
- Recover;
- Dismiss;
- Leave.

Each command should call the owned companion behavior path.

The UI should not invent a new AI framework.

---

# 54. UI open-state behavior

While the command UI owns player interaction, background companion behaviors that could fight the operator should be paused/controlled as specified.

Examples:

- aggressive follow catch-up;
- combat handoff;
- movement ticks.

UI ownership and gameplay ownership must not race.

---

# 55. Companion cleanup

Cleanup must handle:

- explicit dismiss;
- replacement/swap;
- actor invalidation;
- actor death;
- failed spawn/init;
- mod disabled;
- scene/gate closure where applicable;
- plugin shutdown.

Cleanup should remove:

- runtime prompt/action;
- input/UI scope;
- companion tracking;
- plugin-owned overlays/audio;
- temporary relation/behavior state;
- actor if the owning route requires disposal.

---

# 56. Persistence boundary for companions

A durable recruited companion is a different product from a one-session summonable companion.

Durable companion persistence would need proof for:

- actor identity;
- save owner;
- restore ordering;
- health/state;
- position/scene;
- relationship state;
- AI state;
- duplicate prevention;
- missing mod/provider;
- dismissal/removal;
- migration.

The one-session route intentionally avoids claiming this.

---

# 57. Hostile/world lifecycle completion rule

A creature is not live-complete unless the selected scope closes all required gates without conflict.

A full hostile/world lifecycle generally requires:

1. CI1 provenance;
2. CI2 native baseline;
3. CI3 visual transport;
4. CI4A required animation states;
5. CI4 exact template/actor contract;
6. CI5 runtime lifecycle owner;
7. placement/population owner;
8. persistence policy;
9. live spawn;
10. movement;
11. attacks;
12. damage;
13. hit reaction;
14. death;
15. corpse;
16. cleanup;
17. repeat/no duplicate;
18. relevant transition/save behavior.

Many creature records in the private evidence are partial at one or more of these rows.

---

# 58. Current lifecycle evidence must remain creature-specific

Examples from the reviewed matrix demonstrate different states:

- some creatures have strong source/static lifecycle closure but no current-build live closure;
- some have controlled actor proof but no world placement;
- some have visual transport but no native actor contract;
- some have source implementations that exceeded the documented process authority;
- Spider/Broodmother have identity/consumer correction history;
- Predatory Fish has a visual-world lifecycle but not a native hostile lifecycle.

Do not convert the strongest creature's evidence into a blanket claim for all creatures.

---

# 59. Failure history and resulting rules

## Failure: visual prefab treated as creature

**Rule:** CI3 stops at visual transport.

## Failure: source AnimatorController treated as native animation

**Rule:** map through native `ARStateToAnimationMapping`/NPC animation owner.

## Failure: template proof treated as actor proof

**Rule:** CI4 has explicit negative runtime boundaries.

## Failure: actor spawn treated as hostile lifecycle proof

**Rule:** CI5 live must observe combat, damage, death/corpse and cleanup for those claims.

## Failure: same-session retained body treated as persistence

**Rule:** `MarkedNotSaved` remains explicit until save proof exists.

## Failure: historical template GUIDs reused as current

**Rule:** identity is build/version scoped; correction requires consumer-impact review.

## Failure: consumer embeds provider identity independently

**Rule:** provider owns identity; consumer resolves through provider.

## Failure: direct active-map removal bypasses cleanup owner

**Rule:** terminal lifecycle exits must go through release/disposal owner.

## Failure: unrelated creatures bundled into one companion owner

**Rule:** use one creature/cohesive family per consumer unless explicitly approved.

## Failure: UI appears but commands fail

**Rule:** map the full prompt→input→dispatch→handler→command→restore path before editing downstream behavior.

---

# 60. Complete hostile creature implementation sequence

1. Select exact creature and runtime role.
2. Preserve source package and hashes.
3. Record licence/redistribution state.
4. Inventory visual/rig/animation/audio/collider content.
5. Select native baseline by evidence.
6. Record faction/stats/fighting style/controller/death contract.
7. Build mod-owned visual roots.
8. Validate CI3 two-cycle load/release.
9. Author native animation mapping.
10. Validate required states.
11. Author custom `NpcTemplate`.
12. Author custom `LocationTemplate`.
13. Assign stable GUIDs/names/addresses.
14. Validate native location attachment order.
15. Validate mapping/fighting-style references.
16. Validate CI4 two-cycle load/release.
17. Publish strict provider resolver.
18. Ensure consumers use no raw fallback.
19. Build controlled runtime path.
20. Resolve exact LocationTemplate.
21. Construct controlled actor.
22. Mark not saved immediately for one-session proof.
23. Validate exact Location/NPC identity.
24. Validate actor components/invariants.
25. Observe rendering/name.
26. Observe native idle/movement.
27. Observe native combat entry.
28. Observe every claimed attack.
29. Observe actual damage.
30. Observe hit reaction.
31. Apply/observe lethal damage according to gate.
32. Observe `Death(44)`.
33. Observe native death/corpse handoff.
34. Verify retained-body policy.
35. Route terminal state through release owner.
36. Verify lifecycle system disposal.
37. Explicitly discard/cleanup.
38. Verify no duplicate within defined window.
39. Only then open placement/population gate.
40. Only then open persistence/save gate if durable content is intended.

---

# 61. Complete one-session companion sequence

1. Creature CI1-CI5 prerequisites satisfy the companion profile.
2. Dedicated consumer mod/family owner selected.
3. Provider resolver returns exact creature.
4. No raw fallback exists.
5. Explicit player trigger selected.
6. One-session spawn requested.
7. Location marked not saved immediately.
8. Exact template identities verified.
9. One active instance tracked.
10. Native ally/pet semantics applied only to that instance.
11. Runtime-only Companion prompt attached.
12. Original interactability state preserved.
13. Command UI opens from prompt.
14. UI/cursor/input owner acquired.
15. Gameplay input blocked as required.
16. UI input remains functional.
17. Button/manual pointer dispatch hits one handler.
18. Approved command executes.
19. Non-terminal command resyncs UI.
20. Close restores input/cursor.
21. Follow/defend/movement resumes according to state.
22. Dismiss removes prompt/state and actor as owned.
23. Death removes stale prompt and behavior.
24. Disable/shutdown runs cleanup.
25. No save persistence is claimed.

---

# 62. Minimum CI1 evidence

- source ID;
- package hash;
- variant list;
- licence state;
- source technical inventory;
- blockers;
- no downstream claims.

---

# 63. Minimum CI2 evidence

- exact native baseline;
- why chosen;
- component/attachment expectations;
- stats/faction/fighting style;
- collider/controller;
- death/corpse policy;
- scale policy;
- explicit unknowns.

---

# 64. Minimum CI3 evidence

- target Unity/build environment;
- explicit asset roots;
- bundle/catalogue hashes;
- dependency closure;
- renderer/script/shader checks;
- two clean load/release cycles;
- no actor/template claim.

---

# 65. Minimum CI4A evidence

- mapping identity;
- `Idle(1)`;
- `Movement(2)`;
- `ShortRange(16)`;
- `GetHit(32)`;
- `Death(44)`;
- clip provenance;
- build/load/release;
- deferred unmapped clips listed;
- attack timing separately scoped.

---

# 66. Minimum CI4 evidence

- custom NpcTemplate GUID/name;
- custom LocationTemplate GUID/name;
- addresses;
- native baseline lineage;
- exact location component order;
- mapping/fighting-style references;
- visual root set;
- scale;
- load/release;
- negative runtime boundaries.

---

# 67. Minimum CI5 source/build evidence

- resolver contract;
- exact identity checks;
- construction path;
- immediate save exclusion;
- actor invariant checks;
- lifecycle system owner;
- cleanup owner;
- native behavior ownership;
- no unauthorized population/save behavior.

---

# 68. Minimum live hostile evidence

- exact environment/build;
- exact provider/template identities;
- spawn marker;
- placement/clearance;
- initialization;
- render/name;
- idle/movement;
- combat;
- attacks;
- damage;
- hit;
- death;
- corpse;
- cleanup;
- no duplicate;
- save-exclusion readback.

---

# 69. Minimum population evidence

If population is claimed:

- exact native spawner/population owner;
- exact candidate/template identity;
- region/scene;
- density/count policy;
- spawn lifecycle;
- save behavior;
- duplicate-source prevention;
- live spawner observation;
- cleanup/respawn semantics;
- quest/unique exclusions.

A controlled proof spawn cannot substitute.

---

# 70. Minimum persistence evidence

If persistent custom creatures are ever claimed:

- exact save owner;
- durable actor/template identity;
- cold save/load;
- scene restoration;
- health/state;
- death/corpse state;
- duplicate prevention;
- missing provider/package;
- uninstall;
- migration;
- rollback/recovery.

Until then, save-excluded one-session ownership is the safe documented baseline.

---

# 71. Public identity guidance

The public handbook may include exact identities only when their version/proof status is clear.

Do not take a historical proof GUID and label it current.

Do not take an under-evaluation installed identity and silently promote it to canonical.

For mutable creature catalogues, prefer:

~~~text
resolve current promoted identity through provider/catalogue
and verify exact returned identity
~~~

over copying old constants into consumers.

---

# 72. Current proof boundary

## Strongly established

- CI1→CI5 gate architecture;
- strict proof boundaries between gates;
- provider/consumer ownership separation;
- native-baseline selection principle;
- ModService-style visual transport and repeated load/release proof;
- native creature animation-state mapping concept;
- pack-owned `NpcTemplate`/`LocationTemplate` authoring;
- strict resolver/no-raw-fallback model;
- one-session save-excluded controlled actor pattern;
- native hostile behavior ownership;
- native death/corpse handoff targets;
- cleanup/release-owner requirement;
- one-session companion ownership and full UI/input path requirements;
- identity-version conflict handling as a real demonstrated need.

## Creature-specific runtime evidence exists

Different creature projects contain bounded runtime evidence for selected stages.

That evidence remains attached to the exact creature/build/gate.

## Not universally proven

- every creature package;
- every animation set;
- arbitrary native baseline selection;
- all custom creatures live-complete;
- universal world placement;
- native population integration for all creatures;
- persistent custom creatures;
- persistent companions;
- universal loot/rewards;
- arbitrary boss/unique reuse;
- universal save/load;
- all AI profiles;
- all audio/VFX;
- IL2CPP equivalence;
- current Spider identity promotion while its correction review remains incomplete.

---

# 73. The rule to carry forward

The creature process is:

~~~text
source truth
→ native baseline truth
→ asset truth
→ animation truth
→ template identity truth
→ provider truth
→ actor truth
→ behavior truth
→ terminal lifecycle truth
→ cleanup truth
→ placement/population truth
→ persistence truth
~~~

A mod may stop successfully at any bounded gate.

What it may **not** do is rename an earlier pass as a later one.

That discipline is what turns custom creature work from “a prefab appeared” into an evidence-backed integration process.
