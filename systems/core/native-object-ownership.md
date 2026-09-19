<!-- Canonical documentation path. Migrated from docs/reference/NATIVE_OBJECT_OWNERSHIP.md. -->
# Native Object Ownership

> **Reference page.** Use this when you have a valid definition or asset but need to make it participate in FoA gameplay.

## What this system is

FoA distinguishes definitions, Unity objects, and live game-owned instances.

Examples:

~~~text
ItemTemplate → Item → HeroItems / Stock
LocationTemplate → Location → world/location lifecycle
asset/prefab → loaded GameObject → presentation only unless a game owner adopts it
~~~

A disconnected Unity instance can render without being a valid game item, actor, shop entry or save-owned object.

## Who owns it in FoA

Common native owners:

- `World` — MVC object creation/ownership;
- `HeroItems` — hero inventory;
- `Stock` / `RestockableStock` — shop inventory;
- `Location` — world location/actor lifecycle;
- native equip systems — weapon/armour representation ownership;
- native save systems — serialized state and restore.

## Important identities, types, and methods

Proven/researched examples include:

- `World.Add(new Item(template, quantity))`
- `hero.HeroItems.Add(item)`
- `RestockableStock.AddItem(item, allowStacking: true)`
- `LocationTemplate.SpawnLocation(...)`
- `Location.MarkedNotSaved = true`
- `Location.Discard()`

## Where it exists in the lifecycle

Ownership should be established after the required definition/service is ready and before downstream consumers expect the object.

For a custom item:

~~~text
template registered/resolved
→ Item constructed through World
→ Item handed to inventory/stock owner
→ UI/gameplay consumes it
~~~

## How we interact with it

Use the owner that already implements the lifecycle you need.

For example:

- do not simulate inventory by keeping a private list when the item should be in `HeroItems`;
- do not simulate merchant stock with a UI overlay when the item should be in `RestockableStock`;
- do not treat a spawned visual prefab as a native NPC when no `Location`/`NpcElement` lifecycle exists.

## Why this route

Native ownership gives you the game's initialization, events, queries, cleanup and—where applicable—serialization behavior.

It also makes failures local: if registration works but inventory insertion fails, those are two separate stages to debug.

## What goes wrong

Common mistakes:

- loading a prefab and calling it an item/actor;
- creating an `Item` but never adding it to a native owner;
- adding to the wrong owner or before that owner is ready;
- constructing actors without required native components/lifecycle;
- forgetting explicit cleanup for session-only objects;
- assuming native ownership automatically means save-safe.

## How to verify

Verify both identity and owner:

- object references the intended template/custom GUID;
- native owner contains it;
- expected native UI/gameplay can see it;
- duplicate behavior is controlled;
- cleanup/removal uses the same ownership model;
- persistence is validated separately if claimed.

## Current proof boundary

Item creation/inventory/shop ownership has concrete source and bounded runtime evidence. Other systems such as custom armour, weapons and actors have separate ownership contracts and must not be inferred from the item path.
