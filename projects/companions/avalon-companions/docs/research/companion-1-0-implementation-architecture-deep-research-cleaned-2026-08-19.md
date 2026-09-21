# Companion 1.0 Implementation Architecture — Cleaned Deep Research Derivative

Document control:
- Status: `under-evaluation`
- Knowledge class: cleaned derivative of a returned ChatGPT Deep Research report
- Source record: `companion-1-0-implementation-architecture-deep-research-source-2026-08-19.md`
- Source execution state: `RETURNED`
- Date: 2026-08-19
- Owner: Avalon Companions 1.0 research
- Repository branch: `companions`
- Maximum current use: planning-only after RH1/RH2 review
- Implementation: `NOT_RUN`
- Decompilation execution: `NOT_RUN`
- Internal-game/runtime validation: `NOT_RUN`
- Save/persistence validation: `NOT_RUN`
- Compatibility/performance/release validation: `NOT_RUN`
- Human promotion: `NOT_PROVIDED`

## Derivative notice

This document is a cleaned derivative of the Deep Research report pasted into the project conversation. It is not represented as the untouched original. Conversation-native citation tokens were removed. Load-bearing external claims were rechecked against currently accessible first-party or official sources where possible. Unsupported, stale, ambiguous and legal-interpretation claims remain visible rather than being silently strengthened.

## Executive disposition

The returned report is substantively useful and its central architecture is coherent:

```text
Avalon Companions owns durable companion identity and platform state.
Avalon AI owns live decisions and exclusive actor authority.
Dialogue presents authored interaction and emits bounded semantic results.
FoA retains proven low-level physical/state execution primitives.
```

The report correctly resists building taming, dialogue, persistence, progression, party logic and combat competence as disconnected features. Its strongest retained recommendation is to prove one narrow end-to-end vertical slice before broadening:

```text
one hunter capture quest
+ one mod-owned bow/ammunition clone
+ one exact quest-marked wolf
+ one exclusive Avalon acquisition lease
+ one native unconscious transition
+ one animal offer/bonding interaction
+ one durable CompanionId
+ the exact wolf waking under Avalon companion ownership
```

That recommendation remains planning context, not implementation authority.

## Major corrections and qualifications

### 1. Pixel Crushers is a leading candidate, not a proven best choice

The report calls Pixel Crushers Dialogue System the best current architecture. The product is a strong feature fit and a reasonable leading candidate, but the research did not perform a complete comparative evaluation against every viable dialogue architecture. Retain the recommendation as:

```text
Pixel Crushers Dialogue System is the leading authored-dialogue candidate,
conditional on FoA/BepInEx runtime proof and redistribution/licensing clearance.
```

Do not promote it to a mandatory dependency until the exact installed package and FoA runtime are validated.

### 2. Pixel Crushers version and Asset Store classification are verified

The Unity Asset Store currently records:

- product: `Dialogue System for Unity`;
- publisher: Pixel Crushers;
- latest version: `2.2.73.2`;
- latest release: `2026-07-29`;
- original Unity version: `2022.3.0`;
- licence: Standard Unity Asset Store EULA;
- licence type: Extension Asset.

Durable source:
- https://assetstore.unity.com/packages/tools/behavior-ai/dialogue-system-for-unity-11672

The report's version/date statement is retained.

### 3. Pixel Crushers capability claims are supported, but current docs lag the package

Current public Pixel Crushers API/manual pages inspected are generated against approximately Dialogue System 2.2.54–2.2.55, while the store package is 2.2.73.2. Those docs support the existence of:

- Lua variables and conditions;
- dialogue events and UnityEvent-style hooks;
- localisation APIs;
- save-system classes and custom Saver integration;
- dialogue UI override/customisation surfaces;
- conversation/bark/quest-related components.

Durable sources include:
- https://www.pixelcrushers.com/dialogue_system/manual2x/html/class_pixel_crushers_1_1_dialogue_system_1_1_localization.html
- https://www.pixelcrushers.com/dialogue_system/manual2x/html/class_pixel_crushers_1_1_saved_game_data.html
- https://www.pixelcrushers.com/dialogue_system/manual2x/html/class_pixel_crushers_1_1_dialogue_system_1_1_lua_on_dialogue_event.html
- https://www.pixelcrushers.com/dialogue_system/manual2x/html/increment_on_destroy.html

The exact purchased 2.2.73.2 API must still be inspected before integration because public docs lag the current package.

### 4. Asset Store redistribution is a compliance blocker, not a resolved legal conclusion

Unity's current official EULA/FAQ establishes that:

- Extension Assets are licensed per seat;
- Asset Store material may generally be distributed only when embedded or incorporated into a Licensed Product rather than separately extractable;
- SDK assets may not be included at runtime without publisher permission.

Durable sources:
- https://unity.com/legal/as-terms
- https://assetstore.unity.com/browse/eula-faq
- https://support.unity.com/hc/en-us/articles/208601846-A-package-I-want-to-purchase-on-the-Asset-Store-says-Editor-Extension-one-license-per-seat-under-the-requirements-section-What-does-this-mean-

The report's conservative release posture is retained, but its wording must not be read as a final legal determination that Dialogue System, PlayMaker, Blaze, Rabbit or UltEvents definitely cannot be shipped in a BepInEx package.

Required project disposition:

```text
third-party runtime redistribution = BLOCKED_PENDING_EXACT_LICENCE_REVIEW
publisher clarification = required where EULA classification or discrete-DLL distribution remains ambiguous
legal advice = not supplied by this research
```

### 5. Convai service constraints and optional-only recommendation are supported

Current first-party Convai sources verify:

- monthly interaction quotas and monthly-active-user limits;
- paid plan tiers;
- Long-Term Memory on eligible tiers;
- Action API availability on Professional plan and above;
- NPC-to-NPC conversation support;
- attribution obligations unless waived;
- Creator-tier content-use terms;
- Enterprise-specific data terms.

Durable sources:
- https://convai.com/pricing
- https://convai.com/tos
- https://docs.convai.com/api-docs/api-reference/core-api-reference/action-api
- https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/utilities/long-term-memory
- https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/adding-npc-to-npc-conversation

Retain:

```text
Convai = optional opt-in flavour/banter/NPC-to-NPC integration
Convai != authoritative companion memory
Convai != mandatory path for recruitment, quests, progression or save continuity
```

### 6. Convai Unity documentation is genuinely split between legacy and new architecture

The report correctly identifies a documentation transition:

- the older changelog labels `3.2.4` as current, released 2025-03-05;
- current beta/migration documentation describes a rebuilt plugin and migration from `3.3.4` to `4.0.0`;
- the beta plugin is described as redesigned from the ground up;
- compatibility requirements differ between legacy and beta documentation.

Durable sources:
- https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/changelogs
- https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin-beta-overview/migration-guide
- https://docs.convai.com/api-docs/beta-plugins/unity-plugin-beta-overview
- https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin-beta-overview/compatibility-and-requirements

Any Convai intake must therefore pin an exact package generation and must not mix legacy 3.x and 4.x/beta documentation.

### 7. GOAP findings are supported

CrashKonijn GOAP currently documents package `3.1.2`, installation through UPM/OpenUPM/Asset Store and Apache-2.0 licensing in the public repository. Its getting-started documentation explicitly states that it resolves actions for a supplied goal or goal set and that selecting the best goal is game-specific.

Durable sources:
- https://github.com/crashkonijn/GOAP
- https://goap.crashkonijn.com/readme/tutorial/gettingstarted

Retain:

```text
Avalon chooses and governs goals.
GOAP may plan bounded provider-neutral actions after Avalon admits the goal/capability set.
```

### 8. Rabbit Blackboard claims are supported but save ownership remains a design choice

Current Rabbit Blackboard documentation verifies:

- version `1.1.0` release notes;
- generated parent interfaces;
- `ToData` / `FromData`;
- `ToJson` / `FromJson`;
- `DontSave` support;
- documented limitations for GameObjects/interfaces and complex cases.

Durable sources:
- https://blackboard.crashkonijn.com/release_notes
- https://blackboard.crashkonijn.com/concepts/saving_loading
- https://assetstore.unity.com/packages/tools/utilities/rabbit-blackboard-pro-285782

The report correctly recommends project-owned durable Companion profiles rather than coupling save authority to Rabbit. Rabbit may remain a runtime/derived-state adapter.

### 9. PlayMaker release date correction

The report states PlayMaker 2 beta.79 was released 2026-08-08. The current Unity Asset Store page records:

- version: `2.0.0-beta.79`;
- release date: `2026-08-07`;
- licence type: Extension Asset;
- original Unity version: `2022.3.62`.

Durable source:
- https://assetstore.unity.com/packages/tools/visual-scripting/playmaker-2-391026

Corrected date: `2026-08-07`.

Retain the recommendation that PlayMaker remain an optional bounded-procedure adapter rather than a first-slice hard dependency.

### 10. Blaze AI Engine version is verified

The current official Asset Store fallback page records:

- version: `3.4.22`;
- release date: `2026-06-22`;
- original Unity version: `2022.3.53`;
- licence: Standard EULA;
- licence type: Extension Asset.

Durable source:
- https://assetstore-fallback.unity.com/packages/tools/behavior-ai/blaze-ai-engine-194525

The report correctly treats Blaze as an optional actuator candidate whose exact required API surface remains unverified.

### 11. UltEvents version in the report is stale

The report says UltEvents `3.0.6`, released 2025-08-19. The official Kybernetik change log currently records:

- `3.0.7` — 2025-10-19;
- `3.0.8` — 2025-10-27.

Durable sources:
- https://kybernetik.com.au/ultevents/docs/changes
- https://kybernetik.com.au/ultevents/docs/usage
- https://kybernetik.com.au/ultevents/docs/ult-vs-unity/

Correct current documentation version: `3.0.8`.

The architectural conclusion remains unchanged: UltEvents is optional glue and should not own AI, save truth or unrestricted gameplay mutation.

### 12. More Effective Coroutines remains unresolved for current package/licensing

The inspected Trinary documentation supports handles, tags, linking and cancellation patterns, but much of the public documentation is old. The report properly treats current package/version/licensing revalidation as mandatory.

Durable sources:
- https://trinary.tech/controlling-coroutines-by-handle/
- https://trinary.tech/cancelwith/

No current-package or redistribution claim is promoted from this derivative.

## Retained Companion 1.0 architecture

### Durable platform plane — Avalon Companions

Avalon Companions owns:

- `CompanionId` and schema version;
- archetype/provider identity;
- acquisition origin;
- roster and active-party membership;
- runtime binding/reconciliation state;
- relationship, trust, loyalty and bond;
- personality and temperament;
- significant memories and relationship summaries;
- level, competence, speciality and progression unlocks;
- camp/home assignment;
- durable player-approved behaviour preferences;
- life state and availability state;
- provider registration and capability contracts.

It must not persist transient native/runtime state such as:

- current target;
- navigation path;
- active animation;
- current planner object graph;
- nearby threat collections;
- temporary native handles;
- active combat procedure internals.

### Live decision plane — Avalon AI

Avalon AI owns:

- exclusive actor decision leases;
- observation/context construction;
- goal and policy admission;
- semantic action selection;
- cancellation and stale-generation fencing;
- party coordination;
- target reservation and capability-aware coordination;
- dialogue intent/context selection;
- acquisition interaction decisions after deterministic eligibility/subdual.

Native FoA operations may remain bounded physical/reset primitives. Using `NpcAI.ExitCombat`, native movement, native animation playback, ragdoll, `RegainConscious`, hit/damage APIs or perception reset does not by itself surrender strategic decision ownership.

### Dialogue plane

Recommended target split:

```text
Pixel Crushers or equivalent authored dialogue candidate:
    conversation graph
    deterministic player choices
    conditions/checks
    localisation
    presentation
    bounded events through adapter

Avalon AI:
    dialogue intent
    context relevance
    personality/memory interpretation
    optional generated candidate intent

Avalon Companions:
    durable relationship/memory/profile state
    semantic action validation
    acquisition/recruitment/progression authority

FoA:
    world interaction entry
    cursor/input/focus coexistence
    proven physical consequences through bounded executors
```

No dialogue asset or cloud service may directly mutate arbitrary FoA state. All consequential results pass through a semantic gateway.

### Helper-framework abstraction

Retain project-owned interfaces so optional dependencies can be swapped or omitted:

```text
ICompanionBlackboard
IActionPlanner
IProcedureRunner
ITimingScheduler
IActorExecutor
IDialoguePresenter
IGenerativeDialogueService
```

Suggested adapters:

```text
Rabbit -> ICompanionBlackboard
CrashKonijn GOAP -> IActionPlanner
PlayMaker or C# -> IProcedureRunner
MEC or C# -> ITimingScheduler
FoA / optional Blaze -> IActorExecutor
Pixel Crushers / current fallback -> IDialoguePresenter
Convai / null -> IGenerativeDialogueService
```

## Acquisition and subdual disposition

### Hard gate before AI

Eligibility must fail closed before physical mutation:

- current quest/capability permits acquisition;
- exact weapon/projectile identity is Avalon-owned;
- shooter attribution is the player;
- target is alive and not already owned;
- target is not unique/story/boss/undead/special-lifecycle unless separately promoted;
- provider exists and has current actor-family proof;
- no conflicting ownership lease exists;
- required native unconscious dependencies are present;
- transition/save state permits the attempt.

AI must not decide whether an unsupported boss or quest NPC is physically modified.

### Deterministic subdual channel

Retain separate semantic channels:

```text
Health <= 0 -> lethal/death path
Subdual >= resistance -> acquisition/unconscious opportunity
```

Initial subdual calculation should be deterministic code, testable and source-attributed. AI may reason about fear, acceptance, negotiation, offerings or relationship consequences after the opportunity exists.

### Native unconscious as first proof lane

Existing repository static evidence makes native `UnconsciousElement` the strongest physical candidate, but runtime coverage remains `NOT_RUN`.

Required first compatibility order:

1. ordinary wild wolf;
2. ordinary bear;
3. reviewed non-unique hostile human;
4. one passive animal;
5. first reviewed Wyrd family;
6. negative controls for undead, boss, unique/story and unsupported actors.

No broad actor-family support is promoted until each required lane passes.

### Quest-gated onboarding

Retain the hunter/notice-board sequence:

```text
hunter introduction
-> ordinary wolf bounties
-> competence milestone
-> controlled capture-alive tutorial
-> loan/grant subdual bow and arrows
-> exact quest-scoped wolf
-> native unconscious + animal interaction
-> persistent wolf capability unlock
-> recipes / broader animal handling
```

This narrows the first implementation to:

```text
exact quest stage
+ exact weapon
+ exact projectile
+ exact target/provider
```

General free-world acquisition follows only after the controlled tutorial passes and grants a durable capability.

### Provider split

Shared contract, distinct semantics:

```text
Animal provider:
    taming, fear, hunger, temperament, care, food desirability, body-language presentation

Human provider:
    capture or voluntary recruitment, negotiation, employment, food/alcohol/money/aid, refusal/escape

Wyrd provider:
    family-specific subdual, Wyrd resources/offerings, supernatural interaction and progression
```

All providers converge on a durable event such as:

```text
CompanionAcquired(CompanionId, ProviderId, AcquisitionOrigin, RuntimeActor)
```

Acquisition origins remain durable because `Tamed`, `Hired`, `Persuaded`, `Rescued`, `Captured`, `Bonded`, `Volunteered` and `QuestJoined` can affect later dialogue and relationships.

## Persistence and reconciliation disposition

Recommended durable profile shape:

```text
CompanionProfile
    SchemaVersion
    CompanionId
    ProviderId
    ArchetypeId
    OriginTemplateId
    AcquisitionOrigin
    Name
    Personality
    Temperament
    RelationshipState
    ImportantMemories
    RelationshipSummary
    Level
    CompetenceTier
    Speciality
    ProgressionUnlocks
    TrainingMilestones
    LifeState
    RosterState
    PartyAssignment
    HomeAssignment
    BehaviourPreferences
    ReconciliationHints
```

Recommended load transaction:

```text
load profile
-> migrate schema
-> resolve provider
-> validate roster/life state
-> find one valid existing actor
-> reject/hold duplicates
-> acquire exclusive companion lease
-> bind or safely reconstruct if proven
-> restore durable capability/stat state only
-> rebuild fresh observations
-> enable Avalon AI
```

If actor binding/reconstruction is not proven, use `Unavailable/AwaitingReconciliation`; do not spawn an arbitrary replacement.

Initial pending-acquisition persistence should be conservative. Unsupported scene/save transitions should recover/release the actor cleanly instead of serialising ragdoll/unconscious internals.

## Party and relationship disposition

Retain:

- roster distinct from active party;
- shared party coordinator for formation anchors, threat snapshots and target reservations;
- each companion remains an individually reasoning Avalon agent;
- group commands become intents, not forced universal behaviours;
- shared sensing plus local reasoning;
- sparse, event-driven pair relationships rather than continuous full `N × N` simulation;
- inactive roster companions do not run full tactical AI.

No production maximum active-party size is selected from theory. Measure 1, 2 and stress 4 before setting a supported limit.

## Competence, speciality and animation disposition

Competence is a capability contract, not only stat scaling.

Retain working tiers:

```text
Novice
Trained
Veteran
Elite
Mastered
```

Capability progression may include:

- physical survivability/damage/recovery;
- better threat evaluation;
- positioning and disengagement;
- flanking and guarding;
- autonomy and coordination;
- speciality-specific semantic actions;
- training/dialogue/relationship-gated milestones;
- richer compatible vanilla animation bindings.

AI selects semantic actions; a binding resolver selects a proven compatible vanilla animation/state. Missing or incompatible high-tier bindings fall back to lower-tier semantic actions.

Do not promote raw clip names into AI policy. Do not globally accelerate animations as a progression mechanism.

First animation proof scope:

1. wolf novice versus advanced repertoire;
2. one human rig and one weapon family;
3. action -> binding -> playback -> movement/hit/effect owner -> interruption -> cleanup;
4. incompatible rig/weapon negative controls.

## Bonfire Companion hub disposition

Leading UI option:

```text
native-aligned Companions bonfire row
-> hide current fireplace
-> open standalone Companion hub
-> restore exact prior fireplace/focus/input state on close
```

Use the full hub for roster, party, progression, relationship, training and history. Use dialogue alongside it for milestones, not as the entire skill-tree browser.

Existing Immersive Progression campfire research is only a promising route because a previous native Skills row did not become visible in runtime validation. The Companion hub therefore requires fresh entry/focus/controller/open/close/fallback proof.

## Planning-only dependency order retained

```text
0. third-party licence and package-version audit
1. exact FoA binary/Unity compatibility fingerprint
2. native unconscious static revalidation and actor-family runtime proof
3. vanilla bow/ammunition/projectile static and runtime proof
4. notice-board/quest/unlock static and runtime proof
5. live exclusive Avalon AI ownership composition
6. deterministic provider-neutral acquisition/subdual core
7. one quest-scoped wolf vertical slice
8. durable CompanionId and reconciliation
9. production animal provider and hunter progression
10. authored dialogue adapter and semantic gateway
11. human recruitment provider
12. first Wyrd provider
13. bonfire companion hub and progression
14. competence/speciality and representative animation repertoires
15. measured multi-companion party scale
16. optional Convai integration
```

This order is retained as research-derived planning context. It is not permission to execute any stage.

## Claim disposition summary

| Claim | Disposition |
|---|---|
| Shared Avalon Companions platform | `KEEP — owner design direction` |
| Avalon AI as decision owner | `KEEP — dependency-gated; live V2 ownership not proven` |
| Native FoA physical primitives allowed | `KEEP — bounded actuator/reset use only` |
| Native unconscious as first acquisition backbone | `KEEP — static candidate; runtime matrix required` |
| Wolf/bear/human compatibility already proven | `REJECT` |
| All creatures can share one unconscious path | `REJECT` |
| Boss/unique/story actors eligible if mechanics work | `REJECT` |
| Mod-owned bow/ammo clone | `KEEP — source path unproven` |
| Separate subdual and health channels | `KEEP` |
| Exact encountered actor becomes companion | `KEEP` |
| Pixel Crushers mandatory/best proven system | `MODIFY — leading candidate only` |
| Pixel Crushers current version 2.2.73.2 | `VERIFIED` |
| Pixel Crushers redistribution permission resolved | `BLOCKED/UNRESOLVED` |
| Convai mandatory | `REJECT` |
| Convai optional flavour/banter | `KEEP — opt-in only` |
| Convai canonical memory owner | `REJECT` |
| Native StoryGraph/VDialogue immediate target | `BLOCKED` |
| GOAP chooses high-level goals | `REJECT` |
| GOAP bounded planning after Avalon goal selection | `KEEP` |
| PlayMaker as companion brain | `REJECT` |
| PlayMaker optional procedure adapter | `KEEP — beta and licence/runtime gated` |
| Rabbit as canonical save model | `REJECT` |
| Rabbit runtime blackboard adapter | `KEEP — optional/licence gated` |
| Blaze as mandatory AI | `REJECT` |
| Blaze optional actuator | `KEEP — API/runtime proof required` |
| UltEvents 3.0.6 current | `CORRECTED — official changelog shows 3.0.8` |
| Bonfire as deep-management context | `KEEP` |
| Existing campfire clone route already proven | `REJECT` |
| Competence unlocks behaviours and bindings | `KEEP` |
| Global animation speed as progression | `REJECT` |
| Four companions supported | `NOT_PROVEN — stress target only` |

## Required downstream proof

No implementation claim advances without the relevant separately authorised lane.

### Decompilation-static

- unconscious attachment/call graph, actor dependencies and serialization;
- bow/ammo/projectile source identities and attribution;
- quest/notice-board/stage/unlock/save paths;
- bonfire entry/focus/hide/restore path;
- FoA world interaction and native dialogue coexistence;
- stable actor/template/world identities and save markers;
- per-instance stat surfaces;
- wolf and one human weapon-family animation/action inventory;
- native AI ownership and per-instance faction/ally surfaces.

### Internal-game/runtime

- wolf, bear, non-unique hostile human, passive animal and Wyrd-family unconscious matrices;
- boss/unique/undead negative controls;
- vanilla bow/ammo and mod-owned clone path;
- ordinary ammo negative control and shooter attribution;
- hunter bounty/capture quest lifecycle and cancel/failure cleanup;
- same-session exact-actor conversion;
- save/load/transition reconciliation and duplicate prevention;
- dialogue runtime/input/focus/semantic gateway and missing-dependency fallback;
- bonfire mouse/keyboard/controller/fallback lifecycle;
- novice/advanced wolf and one human role animation/action proof;
- measured 1/2/4 companion scale.

## Current final state

```text
Deep Research source preserved: YES
Cleaned derivative: YES
External source recheck: PARTIAL_BUT_LOAD_BEARING_COMPLETE
RH1/RH2 review: separate review record required/completed alongside this derivative
RH3 independent review: NOT_RUN
RH4 validation review: NOT_RUN
RH5 human promotion: NOT_PROVIDED
Implementation authority: NO
Runtime proof: NO
Save/persistence proof: NO
Third-party redistribution clearance: NO
```
