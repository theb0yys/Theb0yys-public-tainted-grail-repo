# Recipes, Merchants, and Economy Integration

> **Reference page.** Use this before combining items with crafting, vendors, loot or rewards. These are separate capability lanes.

## What this system is

Economy/content integration is not one action.

The working governance separates at least:

- existing item grant;
- existing recipe learn;
- runtime recipe append;
- custom item registration;
- custom recipe registration;
- asset/localisation injection;
- vendor injection;
- loot injection;
- reward injection.

Passing one lane does not prove another.

## Who owns it in FoA

Relevant owners include:

- `TemplatesProvider` — item/recipe/template lookup;
- `HeroItems` — player inventory;
- `HeroRecipes` — known/persistently learned recipe state;
- `AlchemyTemplate` and recipe collections — crafting availability surface;
- `Shop` / `RestockableStock` — merchant acquisition;
- loot/reward owners — separate systems that require their own evidence.

## Important identities, types, and methods

Researched mechanics include:

- `HeroRecipes.LearnRecipe(IRecipe)` for an **existing loaded recipe**;
- runtime `AlchemyRecipe` construction/append using reviewed existing item refs;
- a runtime-only "known recipe" shim used specifically to avoid claiming persistence;
- `Shop.OpenShop`;
- `ShopUI.OnFullyInitialized`;
- `RestockableStock.AddItem(...)`;
- private/reflected merchant capacity/compressed-stock surfaces in some experiments.

## Where it exists in the lifecycle

### Existing recipe learn

~~~text
hero valid
→ existing recipe resolves
→ HeroRecipes available
→ skip if already known
→ gated LearnRecipe
→ save/load proof required for durable claim
~~~

### Runtime recipe append

~~~text
templates ready
→ ingredients/outcome/station resolve
→ owner-scoped runtime recipe constructed
→ append to runtime recipe collection
→ optional runtime visibility shim
~~~

This is intentionally different from durable recipe registration.

### Merchant custom item insertion

~~~text
item definition registered
→ Shop.OpenShop decompresses stock
→ ShopUI.OnFullyInitialized Prefix
→ World-owned Item added to RestockableStock
→ UI captures list
~~~

## How we interact with it

### Start by naming the lane

Bad:

> add item and recipe to vendor

Better:

> register one custom item, then separately inject that resolved item into one merchant

or:

> append one runtime-only recipe using existing reviewed item GUIDs, without learning or saving it

### Resolve every dependency exactly

For a recipe, this can include:

- recipe identity;
- input item GUIDs;
- output GUID;
- station/template;
- duplicate key;
- localization/icon if shown;
- persistence owner.

### Define duplicate behavior

Decide what happens when:

- recipe already exists;
- player already knows it;
- vendor already contains item;
- another mod claims same key/GUID;
- source item is missing.

## Why this route

The research explicitly separates runtime recipe visibility from persistent learning.

That matters because a recipe can appear usable in-session while still having no valid durable registration or restoration contract.

Likewise merchant stock is an acquisition route, not an item-definition mechanism.

## What goes wrong

### Runtime append mistaken for persistent registration

The proof recipe explicitly says **do not learn or save it**.

### UI "known" shim mistaken for persistence

A runtime predicate override can make a recipe look known without writing `HeroRecipes`.

### Existing recipe learn mistaken for custom recipe support

`HeroRecipes.LearnRecipe` targets an existing loaded `IRecipe`; it does not prove durable custom recipe registration.

### Merchant private internals treated as stable

Reflection over capacity or compressed rows is build-sensitive and has incomplete runtime proof in the extracted mechanic.

### One shop success generalized to loot/reward

Vendor, loot and reward surfaces have different owners/lifecycles.

## How to verify

### Existing recipe learning

Verify:

- exact recipe resolves;
- not already known;
- mutation runs only on authorized disposable save;
- recipe appears in expected UI/station;
- save/restart/load preserves state;
- duplicate attempt is safe;
- invalid GUID fails closed.

### Runtime recipe append

Verify:

- all exact refs resolve;
- duplicate append is prevented;
- UI shows correct ingredients/output;
- crafting executes with correct quantities;
- disabling/recreating session has documented behavior;
- no persistence claim is made unless separately proven.

### Merchant injection

Verify:

- exact shop;
- decompressed stock;
- item template resolves;
- stock count/identity before and after;
- duplicate guard;
- UI visibility;
- reopen/restock behavior if claimed.

## Current proof boundary

Existing-recipe learning has a source-backed native route but live save/reload validation remains pending in the extracted evidence.

Runtime recipe append is a deliberate runtime-only proof.

Custom item merchant insertion has stronger bounded runtime-visible evidence than the generic merchant-restock/filter mechanics.

Custom recipe registration, generic loot injection and generic reward injection require separate proven processes.
