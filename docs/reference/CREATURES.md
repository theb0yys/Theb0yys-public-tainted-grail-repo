# Creatures: Proven Injection Gates and Runtime Ownership

> **Reference/process page.** Creature integration has the strongest explicit gate sequence in the private evidence repository. The process is evidence-backed, but every gate proves only its own boundary; a visual prefab is not an actor and a valid template is not a live creature.

## What this system is

The proven creature lane separates source content, native baseline selection, visual transport, animation mapping, template/actor definition, runtime lifecycle, and live validation.

~~~text
CI1 source intake
→ CI2 native baseline
→ CI3 visual transport
→ CI4A animation mapping
→ CI4 NpcTemplate + LocationTemplate contract
→ CI5 runtime lifecycle
→ separate live behavior / cleanup / save-policy proof
~~~

The Spider/Broodmother family is the clearest current example.

## Who owns it in FoA

The important owners are:

- `NpcTemplate` — reusable NPC/creature definition;
- `LocationTemplate` — location/actor construction definition;
- `Location` / native actor lifecycle — live runtime ownership;
- native movement, targeting, combat, damage and death systems — behavior ownership;
- animation-state mapping — native state-to-presentation handoff;
- provider/resolver layer — custom template identity and lookup;
- native population/spawner systems — a separate ownership lane from one-session mod-owned actors.

Custom creature assets/templates and provider cleanup belong to the creature-provider layer; consumer gameplay should use a reviewed provider/API boundary rather than copying template identity or inventing raw fallback templates.

## Important identities, types, and methods

A reusable creature process must record:

- exact `NpcTemplate` name and GUID;
- exact `LocationTemplate` name and GUID;
- reviewed native baseline;
- explicit resolver/provider route;
- source and generated asset identities;
- animation-state mapping;
- save policy;
- runtime actor identity checks;
- cleanup owner.

The proven animation lifecycle includes `Idle(1)`, `Movement(2)`, `ShortRange(16)`, `GetHit(32)`, and `Death(44)` for the scoped creature work.

## Where it exists in the lifecycle

~~~text
source package
→ reviewed native baseline
→ mod-owned visual roots
→ animation-state mapping
→ custom NpcTemplate / LocationTemplate pair
→ provider resolution
→ controlled runtime construction/spawn
→ native actor initialization
→ native AI/combat/damage/death ownership
→ cleanup/corpse policy
→ persistence boundary
~~~

Population spawning and one-session companion/summon ownership are different routes and must not be merged casually.

## How we interact with it

### CI1 — Source intake

Record source identity, hashes, Unity/source characteristics, licence/redistribution state, prefabs, meshes, skeletons, animations, materials and blockers.

**Boundary:** no actor, spawn, AI or save claim.

### CI2 — Choose the native baseline

Select the native template family by behavior and structural evidence, not visual resemblance. Record faction, stats, fighting style, location attachments, collider/controller expectations and death/corpse policy.

**Boundary:** baseline suitability does not prove custom runtime behavior.

### CI3 — Prove visual transport

Build only mod-owned visual roots through the reviewed asset route. Validate load/release cycles, dependencies, render state and missing-script conditions.

**Boundary:** a visible asset is not a creature.

### CI4A — Map animation states

Map source clips to exact native runtime states. Do not invent mappings for extra source clips without a native state/route.

**Boundary:** animation mapping does not prove actor construction or combat.

### CI4 — Prove the template/actor contract

Author custom `NpcTemplate` and `LocationTemplate` assets from the reviewed baseline, assign explicit GUIDs, preserve required native attachment order, and validate template/visual/behavior references.

**Boundary:** valid templates still do not prove registration, spawn, combat, death, loot, population or persistence.

### CI5 — Enter the runtime lifecycle

Only after the template contract is established, resolve the exact custom identity and enter the controlled runtime route. Validate the resolved template/actor, fail closed on missing components, and keep movement, targeting, combat, damage and death with their native owners unless a separate gate explicitly changes them.

For one-session companion-style actors, the established policy is disposable/not-saved ownership until a dedicated persistence gate proves otherwise.

### Live proof — prove terminal behavior

Runtime validation must cover the behavior actually claimed: visible actor, initialization, combat if claimed, hit/death if claimed, corpse/cleanup if claimed, duplicate prevention, transition/shutdown behavior, and save policy.

A successful spawn is not the terminal proof.

## Why this route

Creature failures occur at different layers. Keeping the gates separate prevents:

- a bundle load being called an actor proof;
- a template load being called a spawn proof;
- animation playback being called combat proof;
- a same-session actor being called persistent;
- one creature/family being generalized to unrelated creatures;
- consumer mods forking provider-owned identities and lifecycle.

## What goes wrong

- **Raw prefab fallback:** bypasses the reviewed template/provider contract.
- **Visual-first generalisation:** the model renders but native actor requirements are missing.
- **Wrong baseline:** visual similarity hides incompatible AI, collider, fighting-style or death assumptions.
- **Missing animation-state ownership:** clips exist but do not correspond to native lifecycle states.
- **Template = runtime assumption:** CI4 success is incorrectly promoted to spawn/combat proof.
- **Persistence leakage:** one-session actors become save-visible without a restore/removal contract.
- **Consumer identity forks:** separate mods copy or mutate provider identities and create multiple truths.
- **Unrelated creature bundling:** different creatures are treated as one family without shared lifecycle evidence.

## How to verify

A complete scoped creature proof should record:

1. source/provenance and hashes;
2. reviewed native baseline;
3. visual load/release proof;
4. exact animation-state coverage;
5. custom `NpcTemplate` and `LocationTemplate` identities;
6. provider resolution with no raw fallback;
7. controlled runtime construction/spawn;
8. native actor initialization;
9. AI/combat/damage/death behavior only where claimed;
10. corpse/cleanup ownership;
11. duplicate and transition behavior;
12. explicit persistence/save result;
13. exact unknowns and blocked adjacent lanes.

## Current proof boundary

**Strongest proven process shape:** the CI1 → CI5 creature-injection gate sequence and its strict proof boundaries.

**Scoped runtime evidence exists:** Spider/Broodmother and other named creature work provide template, provider, animation and selected live lifecycle evidence. Evidence remains creature/lane specific.

**Not a universal guarantee:** arbitrary creature packages, bosses, population injection, companion parity, loot/rewards, persistence, unrelated creature-family grouping, or every native AI/combat profile.

Use this process to prove each creature or cohesive creature family; do not treat the existence of the process as proof that every candidate has passed it.
