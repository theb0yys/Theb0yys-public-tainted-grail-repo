# ChatGPT Deep Research Brief — Companion 1.0 implementation architecture

## 1. Control block

- Date: 2026-08-19
- Requesting task: convert the owner-accepted Companion 1.0 design into an evidence-backed implementation architecture that can later be decomposed into safe vertical slices.
- Repository: `theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods`
- Inspected branch/ref/commit: owner-authorized shared branch `companions`, design baseline commit `b386b3cec9385e0e0ab65a4202a9ff19f75f33b8`; earlier Companion 1.0 design commits are listed in the citation ledger.
- Protected paths: no protected Age of Men/Tales paths are requested. The current repository owner explicitly authorized this Companion 1.0 overhaul scope and the shared `companions` branch.
- Current authority chain: current repository-owner request -> root `AGENTS.md` -> `documents/process/START-HERE.md` -> research standards -> Companion 1.0 owner decisions -> underlying evidence.
- Intended consumer: Companion 1.0 architecture/planning work on `companions`, followed by bounded internal/static/runtime proof and later implementation.
- Intended usage lane: planning and research-gap closure only until claim review, internal proof, validation, and owner promotion complete.
- Deep Research execution scope: information-gathering only.
- Returned-report follow-on gate: review, clean, store in GitHub, and open/update PR before any next action, per the current Deep Research report-intake standard.
- Maximum allowed use of this brief: E1 research context only.

## 2. Trigger and blocker

- Immediate research question: **What implementation architecture, current framework capabilities, comparable-system lessons, and explicit risk controls should govern Companion 1.0 so that quest-gated acquisition, dialogue/AI, bonfire progression, competence/speciality development, vanilla-animation milestones, persistent identity, and party semantics can be implemented without creating duplicated animal/human systems or handing decision authority back to vanilla FoA AI?**
- Why available information is insufficient: the owner-directed product design is now coherent, and several FoA seams have static/repository evidence, but current external framework capabilities and integration constraints have not been reconciled against the design, while several implementation-critical FoA paths remain unproven at static/runtime level.
- Consequence of a wrong answer: duplicated or incompatible companion systems; unusable dialogue integration; brittle Unity/BepInEx dependencies; save corruption or lost companion identity; quest/story conflicts; vanilla AI competing with Avalon AI; animation/action mismatches; excessive global hooks; poor progression UX; or an architecture that cannot scale from one wolf to human, Wyrd, specialist, and party companions.
- Required review level after the report returns: at least RH1 claim evaluation + RH2 domain review for planning use. Any high-risk technical claim used for implementation requires RH3 independent review + applicable RH4 validation + RH5 human promotion under `review-hierarchy.md`.

## 3. Scope and non-scope

### In scope information-gathering questions

1. Current capabilities, architecture, runtime/editor boundaries, licensing, redistribution limits, version compatibility, and integration patterns for the dialogue/interaction stack selected by the project, including verification of exact current product names and supported surfaces.
2. Current capabilities and architectural constraints of the selected AI/orchestration stack, with special attention to blackboard/memory, GOAP-style planning, bounded FSM/procedural choreography, perception/combat execution, and how to preserve one decision owner.
3. Proven design patterns from strong RPG/companion systems for:
   - non-lethal capture/subdual leading to recruitment/taming;
   - animals and humans sharing infrastructure without sharing identical semantics;
   - quest/tutorial-gated mechanic introduction;
   - notice-board/bounty progression;
   - camp/bonfire companion management and training;
   - relationship/memory-driven dialogue;
   - competence/speciality progression that changes tactics and action vocabulary;
   - companion-to-companion party relationships;
   - durable identity separated from transient actor/runtime state.
4. Best-practice patterns for using animation/action repertoires as progression milestones while keeping AI action choice separate from raw animation selection.
5. UI/UX patterns for a companion roster/progression screen that supports large trees without turning ordinary conversation into a skill menu.
6. Failure-mode and scalability lessons relevant to a BepInEx/Unity mod that may manage multiple AI-driven companions.
7. Licensing/deployment issues that could prevent shipping required third-party runtime components or generated content in a public mod package.
8. Recommended architecture alternatives and tradeoffs, explicitly distinguishing externally supported advice from FoA-specific claims that still need internal proof.

### Out of scope operational tasks

Deep Research must not:

- run Tainted Grail, Unity, BepInEx, or any application;
- execute builds, tests, validators, scripts, decompilers, extractors, profilers, or diagnostics;
- create or mutate code, assets, repository files, branches, commits, PRs, game files, saves, or configuration;
- implement the taming bow, arrows, quests, UI, dialogue, AI, persistence, animation, or party systems;
- generate new internal-game or decompilation-static evidence;
- assert that a FoA hook is runtime-safe solely from public/external sources;
- promote recommendations into implementation authority.

### Version/build/DLC/branch/tool/save/world-state boundaries

- Target game branch: PC Mono / BepInEx v5 Mono as represented by the current repository.
- Current installed-build identity recorded in repository state: Steam build `23532579`; current supplied `TG.Main.dll` fingerprint is recorded in the Companion unconscious research record.
- Repository design branch: `companions`; note that `main` has advanced independently and branch divergence must be reconciled separately before integration.
- Avalon AI current release manifest: 0.8.2 Human executor release candidate; the main host payload currently excludes Runtime V2, Rabbit, GOAP, PlayMaker, observation/executor V2 and owner-contract V2 deployment.
- Save/runtime claims remain out of scope for Deep Research unless already-supplied evidence is being analyzed.

## 4. GitHub citation ledger

| ID | Repository path / GitHub URL | Ref or blob | Lane | Evidence class | What it establishes | What it does not establish |
|---|---|---|---|---|---|---|
| G1 | `mods/avalon-companions/docs/companion-1-0-design-baseline-2026-08-19.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/b386b3cec9385e0e0ab65a4202a9ff19f75f33b8/mods/avalon-companions/docs/companion-1-0-design-baseline-2026-08-19.md | `b386b3cec9385e0e0ab65a4202a9ff19f75f33b8` | process/context | owner design baseline | Consolidates current product direction, acquisition, AI, dialogue, progression, UI, animation, persistence and open gaps. | Does not prove implementation/runtime safety. |
| G2 | `mods/avalon-companions/docs/decisions/0008-companion-1-0-acquisition-dialogue-ai-ownership.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/d0fb3159fceb99831529bb0a425174d23f7191fc/mods/avalon-companions/docs/decisions/0008-companion-1-0-acquisition-dialogue-ai-ownership.md | `d0fb3159fceb99831529bb0a425174d23f7191fc` | process/context | owner decision | Avalon Companions platform ownership, Avalon AI decision ownership, acquisition direction and dialogue/UI direction. | Does not prove current external framework capability or live FoA hooks. |
| G3 | `mods/avalon-companions/docs/decisions/0009-companion-competence-progression-animation-milestones.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/7907081489ea0b4a30e9f950d5e4591be152470c/mods/avalon-companions/docs/decisions/0009-companion-competence-progression-animation-milestones.md | `7907081489ea0b4a30e9f950d5e4591be152470c` | process/context | owner decision | Competence/speciality progression and vanilla-animation milestone direction. | Does not identify compatible clips or runtime action bindings. |
| G4 | `mods/avalon-companions/docs/research/native-unconscious-ai-ownership-2026-08-19.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/c6ce029298f53359083fd65bb9cee34fb972b572/mods/avalon-companions/docs/research/native-unconscious-ai-ownership-2026-08-19.md | `c6ce029298f53359083fd65bb9cee34fb972b572` | decompilation-static | supplied-assembly static evidence | Records the native `UnconsciousElement` physical lifecycle and candidate Avalon ownership seams. | Does not prove representative actor/runtime compatibility or save safety. |
| G5 | `mods/avalon-companions/docs/research/taming-eligibility-gate-2026-07-02.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55/mods/avalon-companions/docs/research/taming-eligibility-gate-2026-07-02.md | `6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55` | internal/repository | prior research gate | Existing tameable/passive/unsafe/blocked roster classification and explicit pre-1.0 non-goals. | Does not authorize capture/taming. |
| G6 | `mods/avalon-companions/docs/research/taming-encounter-evidence-probe-2026-07-09.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55/mods/avalon-companions/docs/research/taming-encounter-evidence-probe-2026-07-09.md | `6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55` | internal-game request/repository | prior research design | Defines existing read-only encounter evidence intent for wolf/bear candidates. | Does not prove adoption, capture, faction conversion or persistence. |
| G7 | `mods/avalon-human-companions/README.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55/mods/avalon-human-companions/README.md | `6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55` | repository/context | implementation-state description | Human Companions 0.6.0 is one-session proof with native interaction/HUD/brain and capture/recruitment still blocked. | Does not prove persistent human recruitment or custom combat AI. |
| G8 | `mods/avalon-companions/ai-package/src/AvalonCompanions.AI.Package/AvalonCompanionAdvancedAiPackage.cs` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55/mods/avalon-companions/ai-package/src/AvalonCompanions.AI.Package/AvalonCompanionAdvancedAiPackage.cs | `6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55` | repository/source | current source | Existing advanced companion observation/proposal model and current handling of `Unconscious`/wild tame readiness. | Does not implement the new acquisition state machine or full AI stack. |
| G9 | `mods/avalon-ai-runtime/release/manifest.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/companions/mods/avalon-ai-runtime/release/manifest.md | branch `companions`; blob `b73fe26f1c5ada69ae9c0822379f267e7105a463` | repository/release state | current owner manifest | Current AI host is 0.8.2 and the V2/Rabbit/GOAP/PlayMaker composition is not in the main host payload; live Human executor gates remain pending. | Does not prove the future full stack cannot work. |
| G10 | `mods/immersive-progression/docs/research/native-campfire-skill-menu-entry-2026-06-20.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55/mods/immersive-progression/docs/research/native-campfire-skill-menu-entry-2026-06-20.md | `6ff65eee2ded9ea5c43c8c9aeb1b7d1df2ebfa55` | repository/internal-context | prior research | Documents the native-campfire progression-entry philosophy and specific `VFireplaceUI` research. | Does not prove a Companion screen implementation or final focus/input lifecycle. |
| G11 | `documents/process/animation-to-gameplay-process.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/companions/documents/process/animation-to-gameplay-process.md | branch `companions` | process authority | current process | Requires action-first animation design, separate binding, execution ownership, effect ownership and validation. | Does not identify any compatible companion animation. |
| G12 | `documents/process/research-standard/deep-research-brief-standard.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/companions/documents/process/research-standard/deep-research-brief-standard.md | branch `companions`; blob `e58d9c57c13a4446d34c69cce180bc406da11fc5` | process authority | current process | Defines Deep Research as information-gathering only and mandates lane separation and report intake. | Does not supply Companion 1.0 technical facts. |
| G13 | `documents/process/research-standard/review-hierarchy.md` — https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/blob/companions/documents/process/research-standard/review-hierarchy.md | branch `companions`; blob `ba85de177a2cd69cd29b9dfdcd24fda80c0fa753` | process authority | current process | Defines RH0-RH5 and high/critical technical review requirements. | Does not prove any game/framework claim. |

## 5. Known claim ledger before Deep Research

| Claim ID | Claim | Lane needed | Current support | Disposition | Missing evidence | Max permitted use now |
|---|---|---|---|---|---|---|
| C1 | Avalon Companions should become the shared companion platform while archetypes/providers retain distinct semantics. | owner/process + architecture | G1, G2 | accepted design direction | implementation decomposition and proof | planning |
| C2 | Avalon AI should be the sole companion decision owner; native FoA may remain bounded physical actuator. | owner/process + repository + later runtime | G1, G2, G8, G9 | accepted target, not production-proven | full V2/provider/executor integration, ownership runtime proof | planning |
| C3 | Native `UnconsciousElement` is a strong physical backbone for capture. | decompilation-static + later runtime | G4 | static support | representative runtime proof, lifecycle conflicts, negative controls | planning/static hypothesis |
| C4 | Wolves, bears, non-unique humans and wider creature families safely support the intended unconscious ownership override. | internal-game + decompilation-static | partial G4/G6 | unproven | per-archetype static/runtime matrix | none for mutation |
| C5 | Existing taming infrastructure already performs live capture/adoption. | repository/runtime | G5, G6 | false/currently blocked | new implementation required | none |
| C6 | Existing Human Companions already supports persistent recruitment/capture/custom AI. | repository/runtime | G7 | false/currently blocked | new implementation and proof required | none |
| C7 | A companion progression screen should be accessed through bonfire/campfire context. | owner design + internal/UI proof | G1, G10 | accepted design; UI not implemented | exact Companion entry/open/close/focus path | planning |
| C8 | Competence progression should unlock better AI actions and compatible vanilla animation/action sets. | owner design + animation static/runtime | G3, G11 | accepted design; content unproven | clip/action inventory, rig mapping, playback/effect proof | planning |
| C9 | The selected dialogue stack can replace the home-grown companion dialogue host and support AI-driven memory/context/skill checks. | external-research + repository integration + runtime | project direction only | unresolved | current official capabilities, runtime/editor boundaries, licensing, integration architecture, live proof | discovery/planning only |
| C10 | Hunter/notice-board bounty quests can safely gate and tutorialize acquisition. | owner design + decompilation-static + internal-game | G1 | accepted product direction; FoA path unproven | exact notice-board/quest/dialogue/state transition path | planning |
| C11 | Exact captured actors can become durable companion identities across save/load/scene transitions without serializing native AI state. | architecture + decompilation-static + internal-game/save | G1 | desired architecture | actor identity, ownership, save hooks, reconciliation matrix | planning |
| C12 | Multi-companion party/squad semantics can scale safely. | architecture + performance + internal-game | G1 | desired architecture | ownership, performance, transition, combat, UI, save evidence | planning |

## 6. External-research questions

### 6.1 Dialogue and interaction stack

- Question: What are the exact current capabilities and integration boundaries of the project's selected dialogue/runtime tools, and what division of responsibility best supports Companion 1.0?
- Research the exact current products referred to by the project as Pixel Crushers dialogue tooling / Next Gen Dialogue bundle, Convai NPC AI Engine, UltEvents, and More Effective Coroutines Pro. Verify official current naming rather than assuming project shorthand is exact.
- Required source types: official publisher documentation, official asset/store pages, official API manuals, maintained first-party examples, licensing/redistribution terms, release/version notes where material.
- Required citations: durable citations for runtime versus editor requirements; data/graph authoring; conditions/variables; custom UI; save/persistence; localization; voice/audio; event hooks; C# APIs; AI/NPC context interfaces; supported Unity versions; package/runtime dependencies; licensing and redistribution.
- Exclusions: do not infer FoA/BepInEx compatibility from ordinary Unity compatibility alone.
- Expected information output: a responsibility matrix showing what should own dialogue graph/content, UI presentation, AI intent/context, memory/state, events, async scheduling, voice, and persistence; identify integration risks and features that would require custom adapters.

### 6.2 AI/orchestration stack

- Question: What current public capabilities and constraints of the project's selected AI stack matter for a single-owner companion architecture?
- Research current official documentation/source for the selected blackboard/memory, GOAP, FSM/choreography, and combat/perception providers used or planned by Avalon AI. Verify exact products/versions rather than relying on shorthand.
- Required source types: official docs/source, maintained examples, release notes, licensing.
- Required citations: lifecycle/ownership, threading/main-thread assumptions, serialization, blackboard/memory semantics, planner inputs/outputs, interruption/cancellation, FSM integration, agent count/performance, Unity version support, API extension surfaces.
- Exclusions: public provider features must not be represented as already integrated into FoA.
- Expected information output: a provider-neutral architecture comparison against the repository target: persistent memory/blackboard -> Avalon governance -> semantic planning -> bounded procedure choreography -> FoA/native executor.

### 6.3 Quest-gated capture/taming progression

- Question: Which established RPG/companion design patterns best support introducing non-lethal capture through hunter bounties/notice-board contracts without making the mechanic global or tedious?
- Required source types: first-party game manuals/design talks/postmortems where available, reputable developer interviews, official wikis where primary material is unavailable, maintained mod/framework documentation for comparable systems.
- Required citations: onboarding/tutorial pacing, progression gates, repeatable versus authored bounties, failure/retry design, economy/resource sinks, capture versus kill incentives, species/class unlocks, abuse prevention.
- Exclusions: do not copy story/lore/content; extract mechanics and failure lessons only.
- Expected information output: 2-4 viable progression patterns and a tradeoff table against the owner design.

### 6.4 Companion progression, competence and speciality

- Question: What progression structures successfully make companions visibly more competent rather than merely numerically stronger?
- Required source types: strong RPG companion systems, tactical RPGs, party RPGs, developer talks, modding frameworks, AI/animation design literature.
- Required citations: skill-tree size/structure, general versus speciality progression, relationship gates, behavioural capability unlocks, training milestones, AI complexity scaling, player readability.
- Expected information output: recommended progression architecture patterns for animal, human and Wyrd providers, including what should be shared and what should remain archetype-specific.

### 6.5 Animation repertoire as progression

- Question: What robust Unity/game-AI patterns allow a character to unlock richer action/animation repertoires over time while keeping gameplay effects and animation playback decoupled?
- Required source types: Unity animation/state-machine documentation, AI action-system literature, official framework docs, technical talks on action selection/animation binding.
- Required citations: animator override/state-machine alternatives, root motion, blend trees, animation-event risks, combo sequencing, interruptibility, hit timing, weapon/rig compatibility, network/save irrelevance where applicable.
- Expected information output: action-first binding patterns compatible with the repository's animation-to-gameplay process, plus anti-patterns to avoid.

### 6.6 Companion UI and bonfire/camp management

- Question: What UI structures make a large companion roster, progression trees, relationship state, memories and role management readable without replacing ordinary dialogue with menus?
- Required source types: strong RPG/party UI examples, official UX breakdowns, accessibility guidance, Unity UI implementation guidance where relevant.
- Required citations: roster navigation, controller/keyboard focus, large-tree readability, comparison views, locked-node explanation, relationship feedback, contextual help, camp versus field separation.
- Expected information output: 2-3 screen architecture options and accessibility/input requirements.

### 6.7 Persistent identity and actor reconciliation

- Question: What engine-agnostic patterns are strongest for separating durable companion identity/progression from volatile scene/runtime actor state?
- Required source types: game architecture papers/talks, Unity save-system guidance, entity identity/persistence patterns, modding frameworks with actor reconciliation.
- Required citations: stable IDs, versioned schemas, scene transition reconciliation, missing-provider handling, stale runtime references, rollback/recovery, migration, duplicate prevention.
- Exclusions: do not claim any pattern is safe in FoA without later internal proof.
- Expected information output: a persistence architecture checklist and failure-mode matrix to compare against FoA once internal evidence is gathered.

### 6.8 Licensing and deployment

- Question: Which selected third-party components may be redistributed in a Nexus/BepInEx mod package, which require the end user to own/install dependencies separately, and which are editor-only development dependencies?
- Required source types: official EULAs, Asset Store license terms, vendor licensing pages, official redistribution guidance.
- Required citations: runtime redistribution, source redistribution, DLL packaging, generated data/content, seat/project restrictions, online-service requirements.
- Expected information output: a per-component deployment/licensing matrix with uncertainties called out.

## 7. Internal-game evidence required

Deep Research must identify these as later proof requirements and must not execute them.

### Acquisition and unconscious proof

- Throwaway-save runtime matrix for at least:
  - one ordinary wild wolf;
  - one ordinary bear;
  - one reviewed non-unique hostile human;
  - one passive animal;
  - one blocked/undead target negative control;
  - one unique/boss/story target negative control if safe to observe without mutation.
- Required observations:
  - native unconscious entry;
  - ragdoll and movement lock;
  - combat exit;
  - interaction availability;
  - vanilla unconscious behaviour suppression only while Avalon acquisition lease is active;
  - vanilla automatic wake suppression only for that lease;
  - explicit Avalon-triggered regain consciousness;
  - cleanup/ownership release;
  - no permanent faction/story/save mutation on rejection;
  - death/attack-during-unconscious negative controls.

### Bow/projectile vertical path

- Prove one exact vanilla bow + arrow source pair through inventory -> equip -> draw -> fire -> projectile -> impact -> target attribution -> cleanup.
- Prove the mod-owned clone preserves the native physical path and does not mutate the source template globally.
- Prove subdual accumulation is separate from lethal health reduction.

### Quest/notice-board/hunter path

- Prove exact notice-board or bounty entry creation/registration, quest start, stage transition, objective counting, controlled target binding, dialogue handoff, completion, reward/unlock, failure/cancel cleanup and save/reload state.
- Prove the tutorial capture hook is inactive outside the exact quest stage.

### Bonfire Companion UI

- Prove entry appears in intended campfire/bonfire context, controller/keyboard focus, cursor ownership, open/hide/restore lifecycle, close/cancel, scene transition, save/load, and fallback when UI dependency is missing.

### Dialogue runtime

- Prove the selected dialogue host can open from a companion interaction, receive a bounded Companion context snapshot, show deterministic authored choices/checks, dispatch semantic results, close cleanly, and restore input/world state.
- Prove AI-generated or AI-selected dialogue cannot directly mutate game state without the companion semantic-action boundary.

### Persistence and exact-actor continuity

- Prove durable CompanionId creation, exact-actor binding, area transition, quit/reload, missing actor reconciliation, duplicate prevention, death/injury state, provider missing/disabled handling, and schema migration/rollback.

### AI ownership

- Prove one active decision owner, acquire-before-enable, no concurrent vanilla/Avalon planner ownership, kill switch, transition invalidation, retryable cleanup, and safe fallback.
- Prove provider-specific action vocabularies for one animal and one human without duplicating platform ownership.

### Animation/competence

- Prove representative Novice and higher-tier action repertoires on one wolf and one human weapon role.
- Prove action selection -> binding -> playback -> effect owner -> cleanup for every promoted action family.
- Prove incompatible clip/rig/weapon combinations fail closed.

### Party scale

- Later matrix for at least 1, 2, and target maximum active companions covering frame-time/AI budget, pathing congestion, combat targeting, formation, transition, dialogue, save/load and cleanup.

### Environment identity required

Every later runtime packet must record game build, Mono branch, `TG.Main.dll` hash/MVID, BepInEx/Harmony state, relevant plugin DLL hashes, config, scene/save identity and source commit.

### Negative controls or matrix required

- acquisition-disabled normal gameplay;
- ordinary bow/ammo does not invoke subdual;
- wrong quest stage;
- wrong target class;
- dead/unconscious already-owned actor;
- unique/story/boss actor;
- missing AI runtime;
- missing dialogue/UI provider;
- transition during acquisition;
- save/load during pending acquisition where safe to test;
- provider disabled after companion was acquired.

### Receipts/artifacts required

- logs with stable lifecycle markers;
- per-case validation receipt;
- save/load/transition receipt where applicable;
- visual evidence for UI;
- hashes/source identity;
- exact failure/rollback markers.

Why external research cannot answer this lane: Deep Research is information-gathering only and cannot execute the internal-game proof.

## 8. Decompilation-static evidence required

Deep Research must identify these as downstream static work and must not execute decompilation/extraction.

### Native unconscious ownership

Later static work must complete and independently review the exact call graph around:

- `UnconsciousElement` creation/attachment;
- `LoseConscious` and `RegainConscious`;
- `EnemyBaseClass.EnterUnconscious` subscriptions and behaviour transition;
- automatic regain listener initialization;
- `KillUnconsciousAction` eligibility and interaction ownership;
- component/type assumptions for humanoids, animals, custom creature combat and bosses;
- save/serialization markers associated with unconscious elements.

### Bow/ammunition/projectile

Identify exact source templates, GUIDs/identities, item/template components, ammo compatibility, skill/effect data, projectile prefab/logic, collision/hit dispatch, damage entry, shooter/target attribution, inventory and crafting/merchant interfaces.

### Quest/notice-board/dialogue

Identify exact quest model, stage/state APIs, objective counters, notice-board/bounty registration, NPC dialogue/interaction route, reward/unlock state and persistence ownership.

### Bonfire progression UI

Identify exact `VFireplaceUI`/`FireplaceUI` entry lifecycle, button registration, focus/navigation, description, hide/restore, modal/input ownership, screen stack, and controller navigation paths relevant to a Companion screen.

### Dialogue integration

Identify exact FoA interaction entry and input/focus ownership that the external dialogue host must coexist with or replace; identify any native dialogue/story dependencies that must remain untouched.

### Companion identity/persistence

Identify exact actor/location/template/runtime identities, saved/not-saved markers, world/scene ownership, faction/ally components, lifecycle and spawn/reconciliation APIs, and any native persistence path that must explicitly not be serialized.

### Animation/action inventories

For representative wolf/bear/human weapon roles, inventory native controller/state/clip identities, rigs/skeletons, root-motion behaviour, transitions, events, attack-effect timing, interruption, locomotion and compatible variants. Keep clip identity distinct from semantic action authority.

### Stats/competence

Identify exact native health/damage/action-speed/movement/agility/stat surfaces that can be read or modified per actor, restoration semantics, caps, equipment interactions, and save implications.

### Party/faction/AI ownership

Identify per-instance ally/faction ownership, native combat planner/behaviour entry, targeting/perception, movement/navigation and party-scale collision/pathing surfaces needed to prove bounded Avalon ownership.

### Fingerprints required

Every static packet must record source assembly/file identity, SHA-256/MVID where applicable, repository source commit, exact symbol/metadata tokens or narrow decompiled locators, and version/build scope.

Why runtime evidence alone cannot answer this lane: static ownership, serialized fields, call paths and exact identities must be inspected separately; Deep Research does not execute decompilation or extraction tools.

## 9. Conflicts, contradictions, and stale scope

### Known conflicts

1. Existing Avalon Companions decision 0007 keeps the plugin-owned Unity dialogue host as the supported surface because safe native story-graph registration was not proven. Companion 1.0 intends to replace that home-grown host with a richer dialogue stack. The new external integration must therefore be proven rather than assumed.
2. The owner target is "all companion decision logic through Avalon AI Runtime," but the current 0.8.2 main host explicitly excludes Runtime V2/Rabbit/GOAP/PlayMaker from its main payload. The report must distinguish target architecture from current production state.
3. Current animal and human implementations are one-session/proof-heavy and deliberately block persistent recruitment/capture. They are evidence inputs, not proof that the 1.0 lifecycle already exists.
4. Static unconscious evidence is promising, but broad runtime compatibility across actor classes is unproven.
5. The bonfire progression route has prior research and partial/fallback history; it is not proof that a new Companion page will attach or behave correctly.
6. `companions` is currently a long-lived owner-authorized development branch and has diverged from newer `main`; any returned report citing repository state must record the exact ref/commit inspected and must not silently mix later main state with the brief baseline.

### Possible version splits

- Unity/game build changes affecting private fields, animation controllers, item templates, story/quest APIs or UI members.
- Third-party Asset Store/package updates changing API names, licensing, supported Unity versions or runtime dependencies.
- Avalon AI V1/V2 architecture transitions occurring after this brief.

### Sources likely to disagree

- vendor marketing pages versus technical API docs;
- older forum/community examples versus current plugin versions;
- Unity-version-specific examples;
- repo historical decisions versus current owner 1.0 direction;
- public framework capabilities versus BepInEx/FoA runtime constraints.

### How later review should resolve them

- prefer current first-party technical docs over marketing summaries;
- preserve historical sources only for historical context;
- record version-specific differences explicitly;
- verify repository state at exact commit/ref;
- never use public Unity compatibility as proof of FoA compatibility;
- route high-risk implementation claims through RH3/RH4/RH5 after required internal/static evidence.

## 10. Research-agent instructions

- Gather and synthesize information only.
- Do not execute runtime, decompilation, build, test, validation, mutation, deployment, or other operational tasks.
- Open every cited source before using it.
- Prefer current first-party/official technical sources; use reputable secondary/community sources only where first-party information is absent or to document practical failure modes.
- Preserve product/version qualifiers and licensing distinctions.
- Separate external, internal-game, and decompilation-static findings.
- Return unsupported FoA/internal claims as missing-proof requests, not guesses.
- Do not treat this brief, the returned report, or repository design decisions as evidence that a third-party framework actually supports a claimed runtime integration.
- Identify independence groups and citation reuse.
- Distinguish "recommended architecture pattern" from "verified available feature."
- When comparing games, extract system-design lessons rather than copying narrative content or assuming their engine constraints match FoA.
- Highlight any selected dependency whose licensing, online-service requirement, editor dependency, Unity-version support or runtime packaging model could materially block a public BepInEx mod.

## 11. Expected Deep Research report output

The returned report should contain:

1. **Executive answer bounded by available information** — whether the Companion 1.0 direction is architecturally coherent and what major external constraints should change or preserve it.
2. **Source table** — source title, publisher/owner, publication/version date, URL, source type, independence group, what it supports, limitations.
3. **Current dialogue-stack capability matrix** — exact current products, runtime/editor boundary, UI, conditions, variables, localization, save, voice, events, AI/context hooks, dependencies, licensing, redistribution.
4. **Current AI-stack capability matrix** — blackboard/memory, planning, FSM/choreography, perception/combat executor, cancellation, serialization, performance, Unity support, licensing, and how each maps to the repository's desired ownership model.
5. **Companion architecture pattern comparison** — acquisition, relationship/memory, progression, party, persistence and UI patterns from comparable systems, with tradeoffs.
6. **Quest-gated acquisition recommendation** — 2-4 mechanically credible progression structures for hunter/notice-board onboarding, including economy/failure/abuse considerations.
7. **Progression architecture recommendation** — general competence + speciality + personal/signature progression; animal/human/Wyrd differentiation; relationship/training gates; visible capability progression.
8. **Animation/action progression recommendation** — action-first patterns, vanilla-animation reuse options, combo/transition cautions and exact classes of internal evidence still required.
9. **Bonfire companion UI options** — 2-3 information architectures with input/accessibility considerations.
10. **Persistent identity checklist** — engine-agnostic schema/reconciliation principles and failure modes to test against FoA.
11. **Licensing/deployment matrix** — what may be shipped, what likely must remain user-owned/external, editor-only dependencies, online requirements and unresolved legal questions.
12. **Claim-by-claim findings** mapped back to C1-C12.
13. **Contradiction map** showing where public sources, older versions, or repo state conflict.
14. **Exact internal proof still missing** separated into `internal-game` and `decompilation-static` lanes.
15. **Recommended review path** for each consequential claim: RH1/RH2 planning-only, or RH3/RH4/RH5 before implementation/current authority.
16. **Implementation dependency graph proposal** — research-derived ordering only, not authorization to implement. It should identify which future slices depend on which external capability decisions and which internal/static proofs.

## 12. Exact unblock criteria

### External-research unblock criteria

The information-gathering questions are answered from inspected sources with durable citations, including:

- exact current identity and capability boundaries for each selected dialogue/AI/runtime dependency;
- licensing/redistribution/runtime/editor constraints;
- at least two strong comparison sources for each major design family where possible;
- clear distinction between vendor-supported feature and inferred integration;
- explicit architecture alternatives/tradeoffs.

### Internal-game unblock criteria

The report identifies the later proof matrix above without attempting to execute it. Implementation remains blocked until the applicable runtime packets actually pass for the intended feature slice.

### Decompilation-static unblock criteria

The report identifies the later static proof above without attempting decompilation/extraction. Implementation remains blocked until the applicable static packets are produced, reviewed and version-scoped.

### Review/promotion criteria

After the report returns:

1. perform mandatory Deep Research report intake;
2. preserve source identity and exact execution state;
3. inspect cited underlying sources where accessible;
4. create/retain the cleaned derivative under the correct research owner;
5. update its owner index/intake surface;
6. commit it on the authorized non-main branch and open/update the required PR;
7. complete RH1 claim evaluation;
8. complete RH2 domain review for planning use;
9. for implementation-consequential/high-risk claims, complete RH3 independent review;
10. execute the separately authorized internal/static/runtime validation required for the exact slice;
11. complete applicable RH4 validation review;
12. obtain RH5 human promotion for any claim that becomes current implementation authority.

### Remaining forbidden uses

Until those gates complete, the brief/report must not be used to claim:

- runtime safety;
- decompilation/static truth beyond already-inspected evidence;
- save/persistence safety;
- compatibility;
- performance;
- deployment/release readiness;
- permission to mutate game/repository state;
- automatic execution of the report's next task.

## 13. Next researched task

- **Current Companion 1.0 dialogue-stack integration and ownership map:** verify the exact current dialogue/UI/AI-related third-party products selected by the project, their runtime/editor/licensing boundaries, and the cleanest responsibility split between authored dialogue content, Avalon AI intent/context/memory, companion semantic actions, and FoA interaction/input ownership. This is an information-gathering topic only and is not automatically authorized until the returned Companion 1.0 report completes mandatory intake.