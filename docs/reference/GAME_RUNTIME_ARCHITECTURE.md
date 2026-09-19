# Game and Runtime Architecture

> **Reference page.** Use this when you need to understand where a mod is running, which layer owns an object, or why a Unity object, FoA template, and runtime game object are not the same thing.

## What this system is

Tainted Grail: The Fall of Avalon is a Unity game with several layers a mod can interact with:

~~~text
operating system / loader
    ↓
BepInEx plug-in
    ↓
Unity runtime objects and APIs
    ↓
FoA services, templates and MVC World
    ↓
runtime game objects such as Item, Hero, Shop and Location
~~~

Mono and IL2CPP are separate loader/runtime lanes. The game concepts above still matter in both lanes, but the assemblies and access mechanisms differ.

## Who owns it in FoA

Important FoA ownership surfaces include:

- `World` — the MVC/runtime owner used when creating and registering many live game objects;
- `World.Services` — access to game services such as `TemplatesProvider`;
- `TemplatesLoader` — native template loading/map construction;
- `TemplatesProvider` — normal loaded-template lookup;
- `Hero.Current` — current player-character runtime owner;
- `HeroItems` — current hero inventory ownership;
- `Shop` / `Stock` — merchant runtime ownership;
- `Location` / `LocationTemplate` — world actor/location ownership.

A Unity `GameObject` can contain a FoA component, but merely creating a `GameObject` does not make it a registered FoA template or a live FoA-owned object.

## Important identities, types, and methods

Common types:

- `Awaken.TG.MVC.World`
- `Awaken.TG.Main.Templates.TemplatesLoader`
- `Awaken.TG.Main.Templates.TemplatesProvider`
- `Awaken.TG.Main.Heroes.Hero`
- `Awaken.TG.Main.Heroes.Items.ItemTemplate`
- `Awaken.TG.Main.Heroes.Items.Item`
- `Awaken.TG.Main.Locations.Shops.Shop`
- `Awaken.TG.Main.Locations.Shops.Stocks.RestockableStock`
- `Awaken.TG.Main.Locations.LocationTemplate`

Common ownership calls include `World.Add(...)`, `HeroItems.Add(...)`, `Stock.AddItem(...)`, and location spawn/discard APIs.

## Where it exists in the lifecycle

A useful high-level order is:

~~~text
loader starts
→ plug-in starts
→ FoA services/templates become ready
→ hero/world becomes available
→ gameplay systems create/use runtime objects
→ scene/save transitions occur
→ cleanup/shutdown
~~~

Do not assume a service, template, hero or shop exists merely because the plug-in has loaded.

## How we interact with it

Use the narrowest owner that actually owns the thing being changed.

Examples:

- resolve a definition through `TemplatesProvider`;
- create a runtime `Item` through `World.Add(new Item(...))`;
- add it to the hero through `HeroItems.Add(...)`;
- add it to merchant stock through `RestockableStock.AddItem(...)`;
- patch a verified lifecycle method with Harmony when no public/event surface exists.

## Why this route

FoA runtime objects participate in ownership, initialization, event, serialization and cleanup systems. Using the native owner keeps those systems in the loop.

The working-repo research repeatedly distinguishes **loading/instantiating a Unity object** from **integrating that object into the game system that owns it**.

## What goes wrong

Typical mistakes:

- creating a Unity object and assuming FoA has registered it;
- resolving a template before `TemplatesProvider` is ready;
- granting an item before `Hero.Current` or `HeroItems` is valid;
- mutating a UI-facing collection after the UI has already taken its snapshot;
- treating a loaded asset as a registered item, weapon, NPC or recipe;
- mixing Mono and IL2CPP loader/project assumptions.

## How to verify

Verify the specific layer you changed:

- loader: BepInEx startup and plug-in log;
- service/template: exact service ready and GUID resolves;
- runtime object: object was created through the native owner and remains valid;
- acquisition: inventory/shop/other owner contains it;
- presentation: correct UI/world representation appears;
- persistence: separate save/load proof when durable behavior is claimed.

## Current proof boundary

This page describes a shared model supported by inspected repository implementations and native-contract research. It does not imply every FoA subsystem has a public registration API or a proven custom-content path.

For runtime-lane details, see [Runtime Guide](../RUNTIME_GUIDE.md).
