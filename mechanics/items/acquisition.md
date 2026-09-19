<!-- Canonical documentation path. Migrated from docs/reference/LOOT_REWARDS_ACQUISITION.md. -->
# Loot, Rewards, Containers, Pickups, and Acquisition

> **Reference page.** Use this when deciding how an item enters the player's possession: direct grant, world pickup, container, corpse loot, vendor, reward, or crafting output.

## What this system is

**Item definition** and **item acquisition** are separate systems.

An item can exist as a valid registered template without being obtainable.

Acquisition lanes include:

- direct existing-item grant;
- world pickup;
- container transfer;
- corpse/search loot;
- vendor stock;
- quest/reward grant;
- crafting output;
- custom loot-table integration.

Each has a different owner and proof requirement.

## Who owns it in FoA

Important owners/surfaces include:

- `TemplatesProvider` — resolves the item definition;
- `World` — constructs live `Item` instances;
- `HeroItems` — player inventory;
- `PickItemAction` / `Pickable` — world pickup;
- `ContainerUI` / container owners — inventory transfer from containers;
- native loot/search owners such as searched locations/corpses;
- `RestockableStock` — vendor acquisition;
- recipe/crafting owners;
- quest/reward systems for story-driven acquisition.

## Important identities, types, and methods

The best-established direct grant shape is:

~~~csharp
Item item = World.Add(new Item(template, quantity));
hero.HeroItems.Add(item);
~~~

Working reward/browser research additionally filters out unsafe loaded templates before granting, including classes such as:

- abstract;
- hidden;
- cannot-drop;
- debug/test/tutorial;
- quest/story-sensitive;
- explicitly unsafe/unbalanced rows.

Other exact surfaces found include:

- `HeroItems.Remove(item, discard, announce)`;
- `PickItemAction.OnStart`;
- `ContainerUI.TakeItemFromContainer`;
- `ContainerUI.TakeAllItems`;
- `SearchAction.ShowContainerContents` in loot/search research;
- NPC template loot/corpse-loot references;
- merchant stock insertion;
- recipe outcome references.

## Where it exists in the lifecycle

### Direct grant

~~~text
templates ready
→ exact ItemTemplate resolves
→ validate item is appropriate for grant
→ World.Add(Item)
→ HeroItems.Add
→ inventory/UI/gameplay observes item
→ save/load if durable
~~~

### World pickup

~~~text
world owner/pickable
→ interaction
→ illegal/crime check if applicable
→ PickItemAction
→ Item enters Hero inventory
→ world pickup owner cleans up
~~~

### Container

~~~text
container owns Item
→ UI prompt/action
→ theft/pickpocket classification if applicable
→ TakeItemFromContainer
→ optional StolenItemElement / crime
→ Hero inventory
~~~

### Loot/corpse

~~~text
actor/container/search owner
→ loot definition/reference
→ search/loot generation
→ live Item(s)
→ transfer to hero/container
→ persistence/respawn/death policy
~~~

## How we interact with it

### Use the direct grant path for controlled proofs

When proving an **existing loaded item** can be acquired, use exact template resolution + native item creation + `HeroItems.Add`.

This is narrower than altering loot tables, quest rewards, or vendors.

### Filter grantable templates

A template existing in the provider does not mean it is safe to grant.

Exclude:

- abstract definitions;
- hidden/system rows;
- cannot-drop rows where inappropriate;
- debug/test/tutorial;
- quest/story-owned content;
- known unsafe identities.

### Preserve native transfer/crime behavior for world/container pickups

If changing interaction timing or UI, keep the native item/crime transfer owner where possible.

### Treat corpse loot as part of actor design

A custom creature's loot/corpse-loot policy belongs in the creature/NPC contract and death lifecycle.

Do not bolt random loot onto a creature before the actor/death/corpse owner is proven.

## Why this route

The working research repeatedly uses direct grants as a **controlled acquisition proof** because it avoids conflating:

- custom item registration;
- vendor behavior;
- loot generation;
- story rewards;
- recipes.

That allows the item itself to be tested before its final distribution method is chosen.

It also shows why a "safe item browser" filters loaded templates: the provider contains content whose existence does not imply safe player ownership.

## What goes wrong

### Loaded template = safe reward

False. Quest/debug/abstract/system templates can resolve perfectly and still be unsafe to grant.

### Direct grant = loot-table integration

False. It proves inventory acquisition, not loot ownership or drop probabilities.

### Container UI hook = loot generation

A UI transfer occurs after the container/loot owner has already determined what exists.

### Corpse loot changed without death/corpse lifecycle

Can create ownership or persistence inconsistencies.

### Quest item granted as ordinary loot

Can bypass story gates or create impossible quest state.

### Custom item save safety assumed from direct grant

The inventory can contain a custom GUID during the current session while cold restore remains unproven.

## How to verify

For any acquisition route, prove:

1. exact item template identity;
2. source/definition is valid;
3. native `Item` creation;
4. acquisition owner;
5. duplicate/stack behavior;
6. legal/crime ownership if relevant;
7. UI visibility;
8. item use/drop behavior;
9. source container/world cleanup;
10. save/load if durable;
11. missing-mod behavior for custom definitions;
12. no quest/story bypass.

For loot tables, additionally validate:

- table identity;
- weights/counts;
- eligibility conditions;
- respawn/repeat behavior;
- corpse/container ownership;
- economy sanity.

## Current proof boundary

Direct existing-item grants have strong source/runtime precedent.

Container/world pickup ownership is well mapped for several interaction paths.

A generic **custom loot-table/reward-table injection framework** is not yet promoted from the current evidence and must remain a separate domain process.
