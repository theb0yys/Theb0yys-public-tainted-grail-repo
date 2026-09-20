# Merchants, Loot, Rewards, and Distribution

Use this page when the content already exists and you need to decide **where and how the player encounters it**.

Registration and distribution are different problems:

> Registration makes the game know the definition. Distribution gives the player a way to obtain it.

## Common distribution routes

FoA has separate native paths for:

- merchant stock;
- containers;
- corpse/search loot;
- world pickups/drops;
- crafting outputs;
- quest/reward grants;
- direct inventory grants.

Each has different timing and persistence behavior.

## Merchant route

The proven custom-item merchant timing is:

~~~text
Shop.OpenShop
→ live stock decompression
→ ShopUI.OnFullyInitialized Prefix
→ create native Item
→ RestockableStock.AddItem
→ original UI snapshots stock
~~~

Important surfaces include:

- `Shop.OpenShop()`
- `ShopUI.OnFullyInitialized()`
- `RestockableStock.AddItem(...)`
- `World.Add(new Item(...))`

For the proven route, modifying the decompressed live stock is easier to reason about than mutating compressed/template data whose restock/persistence semantics are not fully mapped.

## Containers and corpse/search loot

Useful surfaces include:

- `ContainerUI.TakeItemFromContainer(...)`
- `ContainerUI.TakeAllItems(...)`
- `SearchAction.ShowContainerContents(...)`
- `ItemSpawningDataRuntime`

A transfer hook tells you what happened after contents existed.

It does not prove how the loot table/generated contents were authored.

## World pickups

`PickItemAction.OnStart` is useful for pickup/interact behavior.

It is not an item registrar and does not define world-spawn ownership by itself.

## Treat the distribution system as a consumer

Before injecting content:

1. make sure the underlying template resolves;
2. identify the exact native distribution owner;
3. choose the lifecycle point;
4. define duplicate behavior;
5. define whether the change is session-only or durable;
6. verify what the player/UI sees;
7. leave unrelated distribution routes alone.

## Duplicate behavior matters

A mod that runs on every shop open/container refresh can easily insert the same item repeatedly.

Use exact identity checks and decide whether uniqueness is:

- per session;
- per shop;
- per container;
- per refresh;
- persistent.

## Common mistakes

- vendor injection used as a substitute for template registration;
- editing compressed stock without understanding restock ownership;
- adding the same item on every open;
- calling a container-transfer hook a loot-generation hook;
- calling a supplemental grant a true drop-probability change;
- generalizing one merchant implementation to every merchant.

## How to verify

For any distribution route, check:

- content identity already resolves;
- exact distribution owner;
- timing;
- duplicate rules;
- before/after contents;
- UI/player observation;
- reopen/restock/repeat behavior;
- save/load only if the route changes durable state;
- compatibility with other mods targeting the same owner.

## Evidence limits

Bounded working examples include:

- live merchant-stock insertion before UI snapshot;
- normal item grants;
- several container/corpse/world-pickup observation/transfer points.

Still partial:

- reflected merchant restock/category filtering internals.

Not yet a general process:

- arbitrary loot-table registration;
- persistent reward-table mutation;
- every merchant/container family.
