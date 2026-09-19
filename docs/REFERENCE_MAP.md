# Reference Library

This is the repository's **technical handbook and lookup layer**, not a second beginner curriculum.

If you are learning from the beginning, use the **[repository front page](../README.md)**. Open handbook pages when a tutorial sends you there or when you are investigating a specific system.

## The page format

Every technical handbook page answers the same questions:

~~~text
What this system is
Who owns it in FoA
Important identities, types, and methods
Where it exists in the lifecycle
How we interact with it
Why this route
What goes wrong
How to verify
Current proof boundary
~~~

That format is deliberate. The repository should teach not only **how**, but **why the working route has its current shape** and **what failures established the rule**.

## Golden rules

### [Golden Rules of Tainted Grail Modding](reference/GOLDEN_RULES.md)

The cross-cutting rules extracted from successful paths and failed attempts: full-path equivalence, native ownership, exact identity, lifecycle timing, asset-vs-registration separation, cleanup, evidence boundaries, version scope, and fail-closed behavior.

### [What Works, What Does Not, and Why](reference/WORKS_FAILS_WHY.md)

Successful, partial and rejected approaches connected to the failure or ownership lesson that explains the final rule.

### [Working, Partial, and Rejected Patterns](reference/WORKING_REJECTED_PATTERNS.md)

A status ledger for techniques that are currently bounded-working, source/static only, blocked, or explicitly rejected—and why.

## Start with the rules

### [Golden Rules of FoA Modding](reference/GOLDEN_RULES.md)

The cross-cutting rules established by successful paths and failed attempts: exact identity, native ownership, lifecycle timing, lane separation, cleanup, persistence boundaries, version scope, and failure discipline.

### [FoA Systems and Knowledge Map](reference/GAME_SYSTEMS_MAP.md)

A map of the research corpus: combat, items, weapons, armour, creatures, recipes, merchants, UI, world, quests, audio, VFX, save systems, and the boundary between game facts, native systems, modding mechanics, and runtime evidence.

## Foundation

### [Game and Runtime Architecture](reference/GAME_RUNTIME_ARCHITECTURE.md)

How BepInEx, Unity, FoA services/templates, `World`, and live runtime objects fit together.

### [Identity: GUIDs, Names, Addresses, and Stable IDs](reference/IDENTITY_GUIDS_NAMES.md)

Native GUIDs, custom GUIDs, template names, display names, plug-in GUIDs, Unity GUIDs and Addressables addresses—and why they are not interchangeable.

### [Templates and Registries](reference/TEMPLATES_REGISTRIES.md)

`TemplatesLoader`, `TemplatesProvider`, readiness, lookup, cloning, registration, collision risk, and the difference between an object existing and a definition being registered.

### [Lifecycle and Hooks](reference/LIFECYCLE_HOOKS.md)

Harmony Prefix/Postfix concepts, lifecycle timing, template readiness, merchant timing, save observation, and why hook position matters.

### [Hook and Intervention Process](reference/HOOKS_AND_INTERVENTION_PROCESS.md)

The full process for selecting an intervention: native owner → lifecycle map → observation → smallest hook/API → downstream proof → cleanup → compatibility.

### [FoA MVC: Models, Elements, Views, Events, and Services](reference/MVC_MODELS_ELEMENTS_EVENTS.md)

The native logical lifecycle behind `World.Add`, Model/Element ownership, View teardown, events, listener cleanup, and services.

### [Scene, Service, and Template Lifecycle](reference/SCENES_SERVICES_TEMPLATES.md)

Startup ordering, SceneService, Addressables scene discovery, template loading labels, `TemplatesLoader`, and `TemplatesProvider` readiness.

### [Native Object Ownership](reference/NATIVE_OBJECT_OWNERSHIP.md)

Why `World.Add`, `HeroItems.Add`, `Stock.AddItem`, `Location` ownership and other native owners matter.

### [Assets, Addressables, and Presentation](reference/ASSETS.md)

AssetBundles, Addressables, `ARAssetReference`, prefabs, icons, models/materials, Merlin Workshop's actual boundary, and why an asset load is not gameplay registration.

### [Localisation and Questline Babel](reference/LOCALIZATION_BABEL.md)

`LocString`, fallback text, semantic IDs, positional Babel IDs, `LightLocString`, multilingual boundaries, and why raw text does not register a translation.

### [Story Graphs, Dialogue, Choices, and Runtime Execution](reference/STORY_DIALOGUE_CHOICES.md)

XNode authoring vs compiled Story runtime, graph GUIDs, bookmarks, Story Model ownership, choices, and persistence limits.

### [Serialization, Unity Archives, and .arch](reference/SERIALIZATION_ARCHIVES.md)

Separates Questline payload formats from the Unity Archive outer container and corrects the assumption that every `.arch` shares one proprietary schema.

### [Questline Rendering and Proprietary Runtime Systems](reference/PROPRIETARY_RENDERING_SYSTEMS.md)

Drake, Kandra, Leshy, Medusa, HLOD, mip streaming, scene baking, and why ordinary Unity renderer assumptions often fail.

### [Private APIs, Reflection, and Compatibility](reference/PRIVATE_APIS_COMPATIBILITY.md)

How to use private/reflected surfaces as explicit version-scoped interventions rather than pretending they are stable public APIs.

### [Saving and Persistence](reference/SAVING_PERSISTENCE.md)

Template GUID serialization/restoration, registration timing, session-only content, missing-mod risk, save/load proof, and current unknowns.

## Native systems and research routing

### [FoA Systems and Knowledge Map](reference/GAME_SYSTEMS_MAP.md)

Routes questions to the right game-knowledge/native-system owner: items, weapons, armour, creatures, recipes, merchants, spells, UI, spawning, saves, world/scenes, and more.

### [FoA Game-Knowledge Index](reference/GAME_KNOWLEDGE_INDEX.md)

All 38 research domains—items, actors, combat, quests, dialogue, world, UI, assets, saves, versioning and more—with ownership boundaries and related domains.

### [Mechanics Catalogue](reference/MECHANICS_CATALOGUE.md)

Human-readable status map of reusable mechanics extracted from the working repository, including their evidence state and forbidden generalisations.

### [Combat, Stats, Costs, Poise, and Stagger](reference/COMBAT_STATS_EFFECTS.md)

Native stat ownership, non-saved `StatTweak` patterns, item costs, parry/block, poise-vs-stagger, and scoped damage-event tuning.

### [Crime, Stealth, Bounty, Guards, and Consequences](reference/CRIME_STEALTH_BOUNTY.md)

Native crime ownership, bounty, witnesses, guard response, jail, stolen pricing, reversible stealth tuning, and why template taxonomy is not spawn authority.

### [Skills, Progression, XP, Talents, and Reversible Stat Growth](reference/PROGRESSION_SKILLS.md)

XP context vs save-affecting sink, native multipliers, non-saved progression effects, and talent-spend transaction boundaries.

### [Recipes, Merchants, and Economy Integration](reference/RECIPES_ECONOMY.md)

Separates recipe learning, runtime recipe append, custom registration, merchant insertion, loot and reward lanes.

### [Actors, Locations, Spawning, and Session Ownership](reference/ACTORS_LOCATIONS_SPAWNING.md)

`LocationTemplate`, `Location`, `NpcElement`, session-only actors, death/corpse handoff, cleanup and persistence boundaries.

### [UI, Input, Focus, and Command Routing](reference/UI_INPUT_INTEGRATION.md)

The full UI path: owner, lifecycle, input/control, cursor/focus, dispatch, handler, command, close and restoration.

### [Interactions, Prompts, Pickups, Containers, and Illegal Actions](reference/INTERACTIONS_USABLES.md)

The native interaction stack and the Hold-to-Steal failure sequence that proves why prompt text, callback, authorization, and final action ownership must be mapped separately.

### [Audio, FMOD, Event Identity, and Safe Replacement Boundaries](reference/AUDIO_FMOD_INTEGRATION.md)

Exact FMOD event identity, actor/item scoping, hash-pinned sidecars, parameter limitations, ownership conflicts, and fail-closed compatibility.

### [Asset and Resource Lifetime](reference/RESOURCE_LIFETIME.md)

Async load, owner adoption, cancellation, release, and why logical/GameObject/renderer lifetimes must be handled separately.

### [World, Scenes, Portals, and Travel](reference/WORLD_SCENES_TRAVEL.md)

Native travel chain, SceneService, scene identity/config, additive lifecycle and the still-NOT_RUN external custom-scene gate.

### [Reverse Engineering and Discovery](reference/REVERSE_ENGINEERING_DISCOVERY.md)

How to discover new GUIDs, types, methods and ownership relationships without turning guesses or decompilation into unsupported runtime claims.

## Process and diagnostics

### [Debugging and Diagnostics](reference/DIAGNOSTICS.md)

How to find the earliest failed stage instead of changing unrelated systems.

### [Failures and Constraints](reference/FAILURES_CONSTRAINTS.md)

Reusable lessons extracted from failed attempts and corrected hypotheses.

### [Research Method](reference/RESEARCH_METHOD.md)

How the repository turns source, hypothesis, diagnostics, successes and failures into a reusable process.

### [Validation and Compatibility](reference/VALIDATION_AND_COMPATIBILITY.md)

How to separate source/build/load/feature/cleanup/persistence/release claims and how to scope compatibility evidence.

### [Testing and Evidence Status](EVIDENCE.md)

Exact meanings of deeper testing/evidence states.

## Catalogues

### [Hook Catalogue](reference/HOOK_CATALOGUE.md)

Selected exact types/methods, patch kind, lifecycle meaning, proven use, risks and proof boundary.

### [Source-Located Hook Atlas](reference/HOOK_ATLAS_CANDIDATES.md)

A broader research atlas of exact Harmony targets found across the working repository. Entries are leads, not automatic recommendations.

### [Hook Research Inventory](reference/HOOK_RESEARCH_INVENTORY.md)

Broader source-level inventory of researched Harmony targets across items, theft, crafting, combat, dialogue, travel, map, UI, economy, saves and rendering.

### [Identity Catalogue](reference/IDENTITY_CATALOGUE.md)

Curated native/custom GUIDs used by documented examples. This is intentionally not a bulk game-data dump.

## Proven content processes

### [Items: Proven Custom Item Integration](reference/ITEMS.md)

The complete evidence-bounded custom-item process: native prototype, separate custom identity, clone validation, registry readiness, native registration, provider round-trip, World-owned Item construction, controlled acquisition, merchant/UI lifecycle timing, failure handling, and the still-separate persistence gate.

### [Weapons: Native Item, Equip, Combat, Presentation, and Importer Process](reference/WEAPONS.md)

The native `ItemTemplate → Item → ItemEquipSpec → ItemEquip → CharacterHandBase/CharacterWeapon` ownership chain, implemented registrar behavior, Drake presentation path, provider lifetime, acquisition, combat-preservation, cleanup, persistence/migration requirements, and the exact boundary where the generic importer remains partial.

### [Armour: Source Geometry, Deformation, Kandra, Native Clothes, and Equip Process](reference/ARMOUR.md)

The staged armour path from source geometry and deformation proof through Kandra package generation/registration and the native `BaseClothes → ClothStitcher → KandraRig/KandraRenderer` equip lifecycle, with target-armour and persistence boundaries kept explicit.

### [Creatures: Proven Injection Gates, Native Actor Lifecycle, and Provider Ownership](reference/CREATURES.md)

The evidence-backed `CI1 → CI2 → CI3 → CI4A → CI4 → CI5 → focused live gate` process, including provider/consumer ownership, controlled actor lifecycle, native combat/death/corpse ownership, cleanup, population separation, one-session companion routing, and persistence limits.

### [Content Domains: Do Not Generalise One Process Across Everything](reference/CONTENT_DOMAINS.md)

Why items, weapons, armour, creatures, spells, recipes, vendors and world content remain separate owner graphs and proof lanes.

## Runtime and mod structure

### [Runtime Guide](RUNTIME_GUIDE.md)

Mono vs IL2CPP, loader/tooling differences, and local IL2CPP reference layers.

### [Mod Architecture](MOD_ARCHITECTURE.md)

How to organize a working plug-in without turning a small mod into unnecessary framework code.

### [Debugging](DEBUGGING.md)

Short symptom-oriented troubleshooting for loader, plug-in and Harmony failures.

## Merlin Workshop

Merlin Workshop remains important, but its role is bounded:

- official Questline modding/replacement toolkit;
- useful for replacing supported existing Addressables/assets;
- first-party source of information about native types, GUIDs, addresses and relationships;
- **not a general process for registering genuinely new FoA content**.

Older `docs/pipelines/` links are retained only as compatibility redirects. They no longer describe those Merlin structures as a new-content pipeline.

## Old start-page compatibility

[Start Here](START_HERE.md) exists only for old bookmarks and routes back to the current front door/reference pages.
