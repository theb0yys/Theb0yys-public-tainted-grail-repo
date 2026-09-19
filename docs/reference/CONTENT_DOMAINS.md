# Content Domains: Do Not Generalise One Process Across Everything

> **Reference page.** Use this before applying the item process to weapons, armour, creatures, spells, recipes, vendors, loot or world content.

## What this system is

FoA content types share some concepts—identity, templates, assets, ownership, lifecycle—but they do **not** share one universal injection process.

A proven item registration route is not automatically a proven weapon, armour, creature, spell or recipe route.

## Who owns it in FoA

Each domain has different native owners.

Examples already identified in repository research include:

- items — `ItemTemplate`, `Item`, inventory/stock owners;
- weapons — item template plus equip/presentation/combat/Drake ownership;
- armour — item template plus native clothes/Kandra stitching/equip ownership;
- creatures/NPCs — `NpcTemplate`, `LocationTemplate`, actor/location/AI/combat/death owners;
- recipes — recipe definitions, station collections and `HeroRecipes`;
- vendors — `Shop`, stock implementations and UI lifecycle;
- spells — item/effect/skill graph/cast ownership;
- world content — scenes, locations, spawners, routes and persistence owners.

## Important identities, types, and methods

The handbook will add domain-specific catalogues only when their native graph is understood and the exact process has evidence.

Until then, use the shared foundation pages for:

- identity;
- lifecycle;
- assets;
- ownership;
- persistence;
- diagnostics.

## Where it exists in the lifecycle

Each domain must answer its own sequence:

~~~text
definition identity
→ registration/resolution
→ runtime owner
→ acquisition/spawn/equip/use
→ presentation/behavior
→ cleanup
→ persistence
~~~

Some domains have more stages and different owners.

## How we interact with it

When adding a new domain process:

1. map the native graph;
2. identify exact identities;
3. find the runtime owner;
4. research lifecycle timing;
5. establish the smallest controlled proof;
6. record failed alternatives;
7. verify each stage separately;
8. only then publish a reusable process.

## Why this route

Earlier public pipeline pages incorrectly inferred that because Merlin Workshop exposes native item/weapon/armour/NPC authoring structures, those structures themselves formed a general new-content process.

They do not.

Merlin remains valuable as an official replacement-oriented tool and first-party information source for native types, GUIDs, addresses and relationships.

## What goes wrong

- item registration is assumed to solve equipped weapon visuals;
- a model is loaded and called a creature;
- armour is treated as a mesh without native clothes/Kandra lifecycle;
- a recipe object appears in UI and is assumed persistent;
- a spell is classified by name fragments and treated as exact native ownership;
- a vendor mutation is treated as item registration;
- one content family is generalized from another without proving its owner graph.

## How to verify

A domain process is ready for the handbook when it can answer:

- exact native definition(s);
- exact identity;
- registration/resolution;
- runtime owner;
- presentation owner;
- behavior owner;
- lifecycle/hook point;
- cleanup;
- persistence;
- failure lessons;
- runtime validation;
- explicit unknowns.

## Current proof boundary

**Items** now have a public reasoned baseline in [Items: Proven Custom Item Integration](ITEMS.md).

The handbook now documents the requested evidence sequence:

1. [Items](ITEMS.md) — proven bounded custom-item registration/acquisition process.
2. [Weapons](WEAPONS.md) — native weapon graph plus implemented registrar evidence, with the complete generic importer still partial.
3. [Armour](ARMOUR.md) — importer/Kandra/native-clothes evidence lanes, with end-to-end target armour still partial.
4. [Creatures](CREATURES.md) — evidence-backed CI1→CI5 creature-injection process with lane-specific runtime proof.

Spells, recipes, vendors and world content still require their own separately proven processes rather than generic instructions.
