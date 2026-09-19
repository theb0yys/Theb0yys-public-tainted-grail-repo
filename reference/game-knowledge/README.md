<!-- Canonical Wave 5 location. Migrated from docs/reference/GAME_KNOWLEDGE_INDEX.md. -->
# FoA Game-Knowledge Index

> **Reference page.** This is the subject index for the game information represented in the maintainer's research corpus. Use it to find what kind of fact you are dealing with before jumping to implementation.

## What this system is

The research corpus separates game facts into 38 subject domains.

A domain answers **what exists, how it is identified, and how it relates to other game subjects**.

It does not automatically answer **how the native system executes it** or **where a mod should hook it**. Those belong in the systems/modding handbook pages.

## Who owns it in FoA

The narrowest relevant domain owns the subject fact. Cross-domain relationships should be linked rather than duplicated as separate "truths."

## Important identities, types, and methods

| Domain | Owns facts about | Common related domains |
|---|---|---|
| abilities-spells-magic | spells, abilities, costs, targeting, cooldowns, projectiles, schools | effects, skills, combat, VFX, audio |
| achievements-meta-progression | achievements, unlock conditions, statistics | content versioning, difficulty, player, quests |
| ai-behaviour | AI definitions, perception, aggression, schedules, patrol relations | spawns, NPCs, creatures, navigation |
| animation-character-presentation | clips, controllers, rigs, locomotion/combat animation, animation events | assets, combat, actors, audio, VFX |
| armour-equipment | armour, shields, jewellery, wearable slots, restrictions, resistances | items, stats, combat, assets |
| audio-music | SFX, ambience, music, voice, emitters, zones | dialogue, animation, VFX, world |
| books-notes-lore | books, notes, readable/codex records, source-bounded lore | localization, world, quests, interactions |
| cameras | gameplay/dialogue/cinematic cameras, FOV, camera effects/states | cutscenes, dialogue, UI, animation |
| combat | damage, block/parry/stagger/critical, mitigation, death/execution | weapons, armour, stats, effects, creatures |
| containers-loot-rewards | containers, loot pools, drops, rewards, randomization, respawn | items, quests, spawns, merchants |
| content-versioning | DLC/content-pack ownership, patch-added/removed records, version-specific applicability | every version-sensitive domain |
| crafting-alchemy-recipes | recipes, ingredients, outputs, stations, discovery, crafting requirements | items, interactions, merchants, skills |
| creatures-enemies | creatures, enemy archetypes, variants, bosses, roles, resistances, loot | AI, combat, spawning, assets, animation |
| cutscenes-sequences | cutscene/timeline identities, cameras, dialogue sequences, scripted encounters | cameras, dialogue, events, animation, audio |
| data-identities-relationships | ScriptableObject identities, registries, GUID relations, aliases, lookup keys | identifiers, schemas, every subject domain |
| dialogue-conversations | speakers, responses, checks, branches, conversation state, quest hooks | NPCs, quests, localization, audio |
| difficulty-game-rules | difficulty definitions, global multipliers, game-rule flags | combat, player, stats, achievements |
| economy-merchants | vendors, stock, prices, currencies, restocking, services | items, loot, factions, NPCs |
| effects-status | buffs, debuffs, diseases, poison, bleed, burn, stacking, immunity | spells, combat, stats, actors |
| environment-world-simulation | weather, time, hazards, water, vegetation, world-state changes | world, physics, VFX, navigation |
| events-triggers-scripting | events, trigger records, conditions/actions, scripted relationships | quests, cutscenes, interactions, save |
| factions-reputation-crime | factions, cultures, hostility, reputation, crime, witnesses, bounties | NPCs, quests, dialogue, merchants, world |
| input-controls | actions, bindings, contexts, controller maps | UI, player, cameras, interactions |
| interactions-usables | doors, switches, beds, pickups, readables, harvesting, activators | world, loot, lore, events, UI |
| items-inventory | item/template identities, stacking, durability, consumables, quest items, slots | weapons, armour, loot, merchants, crafting |
| localization-text | localization keys, languages, names, descriptions, subtitles, UI text | dialogue, lore, UI, all content domains |
| map-discovery-fast-travel | markers, discovery conditions, coordinates, fast-travel data | world, navigation, quests, UI |
| models-prefabs-assets | meshes, prefabs, materials, textures, shaders, LOD, asset identities | animation, VFX, items, actors |
| navigation-pathfinding | nav identities, links, obstacles, route relationships, access facts | world, spawns, AI, physics |
| npcs-characters | named/persistent characters, templates, inventories, schedules, roles, services | dialogue, quests, factions, items, world |
| physics-collision | colliders, rigidbodies, layers, triggers, ragdolls | environment, navigation, assets, combat |
| player | player resources, equipment state, death/respawn, player-specific facts | skills, stats, items, combat, saves |
| quests | quests, objectives, stages, rewards, failures, branches, dependencies | dialogue, NPCs, world, rewards, saves |
| save-persistence | persistent IDs/state, world changes, compatibility-relevant persistence | quests, player, items, world, versioning |
| skills-perks-progression | skills, perks, trees, currencies, unlocks, levels, respec | player, stats, spells, difficulty |
| spawning-encounters | spawn definitions, anchors, populations, encounter groups, patrols, respawn | creatures, NPCs, world, AI, loot |
| stats-attributes | health, stamina, mana, resistances, regeneration, caps, modifiers, scaling | player, combat, effects, armour, skills |
| ui-hud | screens, HUD, menus, journal/map, tooltips, prompts, widgets | input, localization, quests, map |
| vfx-lighting-rendering | VFX, lighting, weather visuals, post-processing, decals, terrain presentation | assets, environment, spells, combat |
| weapons-combat-equipment | weapon classes, ammo, movesets, attacks, blocking/parry refs | items, combat, animation, stats, assets |
| world-locations | regions, scenes, settlements, dungeons, interiors, roads, transitions | map, navigation, quests, actors, environment |

## Where it exists in the lifecycle

A game-knowledge fact becomes a modding target only after the system layer is mapped:

~~~text
game subject
→ exact identity
→ relationships/dependencies
→ native system owner
→ lifecycle/resolver
→ safe modding surface
→ validation
~~~

For example:

~~~text
ItemTemplate GUID
(game knowledge)
→ TemplatesProvider/TemplatesLoader
(native system)
→ exact lookup/registration boundary
(modding)
→ runtime observation
(evidence)
~~~

## How we interact with it

Use the index to answer the first question: **what am I actually changing?**

Then route to the relevant handbook layer:

- identity questions → [Identity](../../docs/reference/IDENTITY_GUIDS_NAMES.md)
- native owner/lifecycle → [FoA Systems Map](../../docs/reference/GAME_SYSTEMS_MAP.md)
- hooks → [Hook Catalogue](../../docs/reference/HOOK_CATALOGUE.md) and [Hook Research Inventory](../../docs/reference/HOOK_RESEARCH_INVENTORY.md)
- reusable modding mechanisms → [Mechanics Catalogue](../../docs/reference/MECHANICS_CATALOGUE.md)
- known successful/failed routes → [What Works, What Does Not, and Why](../../docs/reference/WORKS_FAILS_WHY.md)
- cross-cutting rules → [Golden Rules](../../docs/reference/GOLDEN_RULES.md)

## Why this route

A single feature commonly crosses several domains.

A custom weapon can involve:

~~~text
items-inventory
→ weapons-combat-equipment
→ models-prefabs-assets
→ animation-character-presentation
→ combat
→ ui-hud
→ save-persistence
~~~

If you research only the first domain, the result will be incomplete even if the item appears in inventory.

## What goes wrong

- a display-name match is treated as identity;
- a domain fact is mistaken for runtime permission;
- the same current fact is copied into several domains and drifts;
- a known native identity is treated as proof of safe mutation;
- a domain is assumed complete merely because a folder/index exists.

## How to verify

A domain fact used by a public process should state:

- exact subject identity;
- source/evidence;
- version/build scope;
- related owner/system;
- relationship to adjacent domains;
- what the fact permits;
- what it does not prove.

## Current proof boundary

This index represents the **scope of the research corpus**, not a claim that all 38 domains have equal maturity or a completed public modding process.

Domain-specific handbook pages should be promoted only as their research is distilled and evidence boundaries are clear.
