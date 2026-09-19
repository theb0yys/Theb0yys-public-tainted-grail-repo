<!-- Canonical documentation path. Migrated from docs/reference/DISTRIBUTION_MERCHANTS_LOOT.md. -->
# Merchants, Loot, Rewards, and Distribution

> **Reference/process page.** Distribution answers **where existing content enters gameplay**. It does not create the underlying item/recipe/actor definition.

## What this system is

FoA exposes several separate distribution surfaces:

~~~text
merchant stock
container contents
corpse loot
world pickup/drop
recipe output
quest/reward flow
direct inventory grant
~~~

These are different native owners with different lifecycle and persistence semantics.

## Who owns it in FoA

Important owners/surfaces include:

- `Shop`
- `RestockableStock`
- `ShopUI`
- `ContainerUI`
- `SearchAction`
- runtime `ItemSpawningDataRuntime`
- corpse/template loot definitions
- `HeroItems`
- recipe/crafting owners
- quest/reward owners

The content definition itself still belongs to its template/registry owner.

## Important identities, types, and methods

Research-backed examples:

- `Shop.OpenShop()`
- `ShopUI.OnFullyInitialized()`
- `RestockableStock.AddItem(...)`
- `World.Add(new Item(...))`
- `ContainerUI.TakeItemFromContainer(...)`
- `PickItemAction.OnStart(...)`
- `SearchAction.ShowContainerContents(...)`
- `ItemSpawningDataRuntime`
- exact shop template GUIDs
- exact item template GUIDs

## Where it exists in the lifecycle

### Merchant route

The proven custom-item merchant timing is:

~~~text
Shop.OpenShop
→ stock decompression
→ ShopUI.OnFullyInitialized Prefix
→ World.Add(Item)
→ RestockableStock.AddItem
→ original UI captures list
~~~

### Container/corpse route

Research shows container/corpse UI can expose a saved/runtime list of `ItemSpawningDataRuntime` rows before `ContainerUI` presents them.

Observation of `TakeItemFromContainer` proves transfer, not loot-generation ownership.

### World pickup route

`PickItemAction.OnStart` is an interaction/pickup observation point.

It is not a general item-definition registrar.

## How we interact with it

Treat every distribution surface as a **consumer**.

Before injecting into a consumer:

1. make sure the underlying item/template already resolves;
2. identify the exact owner and lifecycle;
3. define duplicate behavior;
4. define session/durable behavior;
5. verify UI/downstream state;
6. keep unrelated distribution lanes unchanged.

For merchant proofs, prefer decompressed live stock rather than mutating compressed template arrays unless that exact persistence/restock contract is understood.

## Why this route

The item research exposed a major distinction:

> registration says the game knows the definition; distribution says a gameplay system gives the player a way to encounter it.

That separation made the merchant failure diagnosable.

A custom item could resolve correctly while still being absent from the visible shop because the stock/UI timing was wrong.

## What goes wrong

### Vendor injection treated as item registration

A shop row cannot make an unknown custom template resolvable.

### Mutating compressed/template stock without understanding restock ownership

Can conflict with native decompression/restock and persistence.

### Duplicate insertion on every open

Use exact duplicate/session/shop checks.

### Loot observer treated as loot-table authoring

Seeing `ContainerUI.TakeItemFromContainer` or corpse contents does not establish native loot-table generation semantics.

### Supplement grants described as true drop-probability edits

Adding an item before a container UI opens is a different mechanism from editing the source loot distribution.

### One shop proof generalized to every merchant

Shop type, stock implementation, restock behavior and UI can differ.

## How to verify

For a distribution lane, verify:

- underlying content identity resolves;
- exact owner is correct;
- lifecycle point is correct;
- duplicate behavior;
- before/after owner contents;
- UI/player observation;
- reopen/restock behavior where applicable;
- save/load only when the lane changes durable state;
- compatibility with other mods targeting the same owner.

## Current proof boundary

**Bounded-working:** decompressed merchant stock insertion before the UI snapshot, normal item grants, several read/observation points for container/corpse/world pickup.

**Source-inspected/partial:** merchant restock/category filtering through reflected private stock fields.

**Not a generic public process yet:** arbitrary loot-table registration, persistent reward-table mutation, all-merchant injection, container authoring, or cross-version vendor persistence.
