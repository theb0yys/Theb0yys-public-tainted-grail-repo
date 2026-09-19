# Move Beyond the First Custom Item

## What you're doing

You are learning how to move from one proven custom-item mechanism to other content domains **without assuming they work the same way**.

The first item path teaches shared concepts: exact identity, registration, native ownership, lifecycle timing, assets, verification and proof boundaries.

It does **not** automatically authorize a weapon, armour, creature, spell or recipe process.

## What you need

- a completed or understood first custom-item path;
- the [technical handbook](../docs/REFERENCE_MAP.md);
- the [Content Domains](../docs/reference/CONTENT_DOMAINS.md) reference;
- a willingness to investigate the native owner before writing the next feature.

## What you'll learn

You will learn the repository's rule for expanding into a new content class:

~~~text
map native graph
→ identify exact identities
→ find registration/resolution
→ find runtime owner
→ find presentation/behavior owner
→ map lifecycle
→ run smallest controlled proof
→ record failures
→ publish only the proven process
~~~

## Steps

### 1. Separate the shared concepts from the item-specific mechanism

Shared concepts:

- stable custom identity;
- exact native references;
- lifecycle timing;
- native ownership;
- assets versus gameplay definitions;
- cleanup/persistence;
- observable verification.

Item-specific details include:

- `ItemTemplate`;
- `TemplatesLoader.AddToMap`;
- `World.Add(new Item(...))`;
- `RestockableStock.AddItem(...)`.

Do not assume those exact APIs are the answer for another domain.

### 2. For a weapon, map the extra owners

A weapon is not proven merely because it is also an item.

A complete weapon process must separately establish:

- item/template identity;
- equip attachment/representation;
- native weapon presentation owner;
- mesh/material/prototype route;
- animation/handling family;
- combat ownership;
- equip/unequip cleanup;
- persistence.

The handbook will publish this process only from the working-repo weapon evidence.

### 3. For armour, map the native clothes/Kandra lifecycle

Armour requires evidence for:

- item identity;
- equipment slot/restrictions;
- native clothes/equip owner;
- Kandra rig/stitching/presentation;
- unequip cleanup;
- stats;
- persistence.

A skinned mesh loading successfully is not an armour integration proof.

### 4. For creatures/NPCs, map actor ownership

A creature process must separate:

- source/visual asset;
- animation mapping;
- `NpcTemplate`;
- `LocationTemplate`;
- runtime actor construction;
- AI/combat/death/corpse ownership;
- spawn/population ownership;
- cleanup;
- save policy.

A visible prefab is not a native creature.

### 5. Apply the same rule to spells, recipes, vendors and world content

Each domain gets its own graph.

For example:

- a recipe appearing at runtime is not automatically learned/persistent;
- a spell name heuristic is not an exact spell/effect ownership map;
- vendor insertion is an acquisition route, not item registration;
- a world coordinate is not spawn/lifecycle proof.

### 6. Use failures as part of the documentation

When an attempt fails, record:

- what was attempted;
- which assumption it tested;
- what failed;
- the corrected model;
- how the next experiment changed.

That is how a private experiment becomes a public reusable process.

## What success looks like

You can take a proposed content feature and identify:

- its definition owner;
- its identity;
- its runtime owner;
- its presentation/behavior owner;
- its lifecycle boundaries;
- its persistence implications;
- which parts already have evidence;
- which parts are still unknown.

You do not copy the item process into an unrelated domain just because some class names look similar.

## Common problems

**"Weapons are items, so the item process is enough":** item registration is only one part of weapon integration.

**"The model loads, so the creature works":** asset transport is not actor ownership.

**"Merlin exposes the type, so Merlin can add it":** Merlin is an official replacement/source-information tool, not a universal new-content registrar.

**"It compiled, so the process is proven":** build, load, runtime behavior and persistence are separate claims.

## Where to go next

Use [Content Domains](../docs/reference/CONTENT_DOMAINS.md) as the map.

Then use [Research Method](../docs/reference/RESEARCH_METHOD.md) to extract the next proven process from the working evidence.
