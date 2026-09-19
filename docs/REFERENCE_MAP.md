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

### [Working, Partial, and Rejected Patterns](reference/WORKING_REJECTED_PATTERNS.md)

A status ledger for techniques that are currently bounded-working, source/static only, blocked, or explicitly rejected—and why.

## Foundation

### [Game and Runtime Architecture](reference/GAME_RUNTIME_ARCHITECTURE.md)

How BepInEx, Unity, FoA services/templates, `World`, and live runtime objects fit together.

### [Identity: GUIDs, Names, Addresses, and Stable IDs](reference/IDENTITY_GUIDS_NAMES.md)

Native GUIDs, custom GUIDs, template names, display names, plug-in GUIDs, Unity GUIDs and Addressables addresses—and why they are not interchangeable.

### [Templates and Registries](reference/TEMPLATES_REGISTRIES.md)

`TemplatesLoader`, `TemplatesProvider`, readiness, lookup, cloning, registration, collision risk, and the difference between an object existing and a definition being registered.

### [Lifecycle and Hooks](reference/LIFECYCLE_HOOKS.md)

Harmony Prefix/Postfix choices, lifecycle timing, template readiness, merchant timing, save observation, and why hook position matters.

### [FoA MVC: Models, Elements, Views, Events, and Services](reference/MVC_MODELS_ELEMENTS_EVENTS.md)

The native logical lifecycle behind `World.Add`, Model/Element ownership, View teardown, events, listener cleanup, and services.

### [Scene, Service, and Template Lifecycle](reference/SCENES_SERVICES_TEMPLATES.md)

Startup ordering, SceneService, Addressables scene discovery, template loading labels, `TemplatesLoader`, and `TemplatesProvider` readiness.

### [Native Object Ownership](reference/NATIVE_OBJECT_OWNERSHIP.md)

Why `World.Add`, `HeroItems.Add`, `Stock.AddItem`, `Location` ownership and other native owners matter.

### [Assets, Addressables, and Presentation](reference/ASSETS.md)

AssetBundles, Addressables, `ARAssetReference`, prefabs, icons, models/materials, Merlin Workshop's actual boundary, and why an asset load is not gameplay registration.

### [Questline Rendering and Proprietary Runtime Systems](reference/PROPRIETARY_RENDERING_SYSTEMS.md)

Drake, Kandra, Leshy, Medusa, HLOD, mip streaming, scene baking, and why ordinary Unity renderer assumptions often fail.

### [Private APIs, Reflection, and Compatibility](reference/PRIVATE_APIS_COMPATIBILITY.md)

How to use private/reflected surfaces as explicit version-scoped interventions rather than pretending they are stable public APIs.

### [Saving and Persistence](reference/SAVING_PERSISTENCE.md)

Template GUID serialization/restoration, registration timing, session-only content, missing-mod risk, save/load proof, and current unknowns.

## Native systems and research routing

### [FoA Systems and Knowledge Map](reference/GAME_SYSTEMS_MAP.md)

Routes questions to the right game-knowledge/native-system owner: items, weapons, armour, creatures, recipes, merchants, spells, UI, spawning, saves, world/scenes, and more.

### [Mechanics Catalogue](reference/MECHANICS_CATALOGUE.md)

Human-readable status map of reusable mechanics extracted from the working repository, including their evidence state and forbidden generalisations.

### [Combat, Stats, Costs, Poise, and Stagger](reference/COMBAT_STATS_EFFECTS.md)

Native stat ownership, non-saved `StatTweak` patterns, item costs, parry/block, poise-vs-stagger, and scoped damage-event tuning.

### [Recipes, Merchants, and Economy Integration](reference/RECIPES_ECONOMY.md)

Separates recipe learning, runtime recipe append, custom registration, merchant insertion, loot and reward lanes.

### [Actors, Locations, Spawning, and Session Ownership](reference/ACTORS_LOCATIONS_SPAWNING.md)

`LocationTemplate`, `Location`, `NpcElement`, session-only actors, death/corpse handoff, cleanup and persistence boundaries.

### [UI, Input, Focus, and Command Routing](reference/UI_INPUT_INTEGRATION.md)

The full UI path: owner, lifecycle, input/control, cursor/focus, dispatch, handler, command, close and restoration.

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

### [Testing and Evidence Status](EVIDENCE.md)

Exact meanings of deeper testing/evidence states.

## Catalogues

### [Hook Catalogue](reference/HOOK_CATALOGUE.md)

Selected exact types/methods, patch kind, lifecycle meaning, proven use, risks and proof boundary.

### [Identity Catalogue](reference/IDENTITY_CATALOGUE.md)

Curated native/custom GUIDs used by documented examples. This is intentionally not a bulk game-data dump.

## Proven content processes

### [Items: Proven Custom Item Integration](reference/ITEMS.md)

The first fully reasoned new-content process:

~~~text
native prototype
→ custom identity
→ clone validation
→ native registration
→ provider resolution
→ World-owned Item
→ controlled acquisition
→ runtime verification
~~~

This page also explains the failed batch/timing assumptions that produced the final process.

### [Content Domains: Do Not Generalise One Process Across Everything](reference/CONTENT_DOMAINS.md)

Why weapons, armour, creatures, spells, recipes, vendors and world content each require their own native graph and proven process.

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
