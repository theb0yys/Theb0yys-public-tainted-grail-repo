<!-- Canonical Wave 5 location. Migrated from docs/reference/GAME_SYSTEMS_MAP.md. -->
# FoA Systems and Knowledge Map

> **Reference page.** Use this to identify which native/game-knowledge domain owns a question before choosing a hook or implementation strategy.

## What this system is

The game is not one API surface.

Different questions belong to different owners:

~~~text
game knowledge
= what subject/identity/relation exists

native system
= how that system owns lifecycle/execution

modding mechanic
= how a mod may observe or intervene

runtime evidence
= what actually happened in a particular build/session
~~~

Do not collapse these layers.

## Who owns it in FoA

### Runtime/core ownership

- MVC / World / Model / Element / Events
- scenes and domains
- services
- templates and registries
- save/load
- Unity/Addressables
- renderer/presentation systems

### Major gameplay knowledge domains

The research corpus contains dedicated domains for:

- abilities, spells, magic;
- AI behavior;
- animation and character presentation;
- armour/equipment;
- audio/music;
- cameras;
- combat;
- containers, loot, rewards;
- crafting/alchemy/recipes;
- creatures/enemies;
- cutscenes/sequences;
- identities/relationships;
- dialogue;
- economy/merchants;
- effects/status;
- environment/world simulation;
- events/triggers/scripting;
- factions/reputation/crime;
- input/controls;
- interactions/usables;
- items/inventory;
- localization/text;
- map/discovery/fast travel;
- models/prefabs/assets;
- navigation/pathfinding;
- NPCs/characters;
- physics/collision;
- player;
- quests;
- save/persistence;
- skills/perks/progression;
- spawning/encounters;
- stats/attributes;
- UI/HUD;
- VFX/lighting/rendering;
- weapons/combat equipment;
- world/locations.

## Important identities, types, and methods

The exact type/method/GUID depends on the selected system.

Before implementation, determine which of these classes of identity you need:

- native template GUID;
- named actor identity;
- `LocationTemplate`;
- item/recipe GUID;
- scene name/address;
- state key;
- quest/decision key;
- Addressables address;
- managed type/method;
- project-owned synthetic identity.

## Where it exists in the lifecycle

Every domain should be mapped through the same questions:

~~~text
definition
→ identity
→ resolver/registry
→ runtime owner
→ events/hooks
→ presentation
→ mutation
→ cleanup
→ persistence
~~~

Not every domain has every stage, and the owner may change between stages.

## How we interact with it

### Items/inventory

Start with [Items](../docs/reference/ITEMS.md), [Templates and Registries](../docs/reference/TEMPLATES_REGISTRIES.md), and [Native Object Ownership](../docs/reference/NATIVE_OBJECT_OWNERSHIP.md).

### Weapons

Requires item identity plus equip/presentation/combat/animation ownership. Do not infer from Items alone.

### Armour

Requires item/equipment identity plus native clothes/Kandra/equip lifecycle.

### Creatures/NPCs

Requires actor/template identity plus Location/AI/combat/death/spawn/persistence ownership.

### Recipes/crafting

Separate existing-recipe learning, runtime append, custom registration, station visibility, crafting execution and persistence.

### Merchants/loot/rewards

These are acquisition/distribution surfaces. They do not create the underlying content definition.

### Spells/effects

Separate spell/item identity, skill/effect graph, cast lifecycle, VFX/audio and gameplay effect ownership.

### UI/input

Separate data visibility from full input/focus/cursor/dispatch ownership.

### World/scenes/spawning

Separate coordinates/assets from scene ownership, placement validity, native spawn lifecycle, population/density and persistence.

## Why this route

The working research repeatedly found failures caused by choosing a hook before choosing the **owner**.

A method can look relevant while belonging to the wrong stage:

- door animation instead of scene travel;
- renderer object instead of equipment lifecycle;
- asset bundle instead of content registration;
- item stock instead of item identity;
- name fragment instead of spell ownership.

The map keeps research oriented around the owner first.

## What goes wrong

- duplicate facts copied into multiple domain pages and drifting apart;
- game-knowledge identity treated as runtime permission;
- native system internals treated as game-content identity;
- runtime observation treated as general cross-version truth;
- mod implementation treated as proof of a native relationship;
- one domain's lifecycle generalized to another.

## How to verify

For a new feature, write down:

1. primary domain;
2. related domains;
3. exact native subject identity;
4. native system owner;
5. available hook/API;
6. version/build scope;
7. evidence maturity;
8. mutation/persistence boundary;
9. validation needed.

If you cannot identify the owner, stay in research.

## Current proof boundary

This page is a routing model derived from the structured game-knowledge/domain corpus. It is not a claim that every domain already has a complete public implementation process.
