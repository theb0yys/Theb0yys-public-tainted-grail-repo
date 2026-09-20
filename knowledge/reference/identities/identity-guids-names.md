# Identity: GUIDs, Names, Addresses, and Stable IDs

> **Reference page.** Use this whenever a tutorial or hook requires an exact identity. Do not treat names, GUIDs, addresses and display text as interchangeable.

## What this system is

FoA modding uses several different kinds of identity at once.

Common examples:

~~~text
native template GUID
custom/mod-owned template GUID
template/object name
display/localized name
BepInEx plug-in GUID
Unity asset GUID
Addressables address
mod-owned catalogue/repository ID
~~~

Each solves a different problem.

## Who owns it in FoA

FoA template identity is primarily GUID-based at runtime. Loaded templates are resolved through native template systems.

BepInEx owns plug-in GUID uniqueness at the loader level.

Unity owns project asset GUIDs in `.meta` files.

Addressables owns address/key identity for assets loaded through that system.

Your mod owns any new synthetic/custom identifiers it introduces and must keep them stable when persistence or cross-reference depends on them.

## Important identities, types, and methods

Example from the proven custom-item path:

~~~text
native template name:
ItemTemplate_Crafting_Cooking_Apple

native template GUID:
1527b1864369efd48b49abb54f1e42e4

custom template name:
ItemTemplate_Mod_FoodDrink_AppleGreen

custom template GUID:
fdaf0000000000000000000000000001

display name:
Green Avalon Apple
~~~

These are different fields with different responsibilities.

## Where it exists in the lifecycle

Identity is used during:

- registration;
- lookup;
- duplicate/collision checks;
- inventory/stock ownership;
- save serialization and restoration;
- cross-mod integration;
- migration between mod versions.

Changing an identity after it has been used can break later lookup even if the visible name still looks correct.

## How we interact with it

Use exact stable identities for machine-facing relationships.

Prefer:

- exact GUID for native template lookup;
- stable plug-in GUID for BepInEx;
- stable custom template GUID for a registered custom definition;
- explicit Addressables address for an asset contract;
- display name only for player-facing text.

Do not use a display name as a substitute for a native GUID when an exact GUID is known.

## Why this route

Names are often presentation. GUIDs and addresses are machine relationships.

The custom-item research shows that a genuinely separate item requires a **separate custom GUID**, not merely a different display name.

Static save-contract research also shows item template identity is serialized/restored through the template GUID, which makes stability important beyond the current session.

## What goes wrong

Known identity failures include:

- reusing a native GUID for custom content, creating a collision/replacement risk;
- guessing identity from a template-name fragment;
- changing a plug-in GUID and accidentally creating what BepInEx sees as a different plug-in;
- changing a custom template GUID after a save has referenced it;
- treating a Unity asset GUID as if it were a FoA template GUID;
- treating an Addressables address as if it registered gameplay content;
- using string/name heuristics where exact native ownership has not been proved.

## How to verify

For any identity, record:

- identity kind;
- exact value;
- owner;
- source/evidence;
- version/build scope;
- what resolves it;
- what must remain stable;
- what happens if it is missing or duplicated.

For a custom template, verify the custom GUID can be resolved back through the normal template provider after registration.

## Current proof boundary

This repository can teach exact identities used by proven examples and how to discover/record identities. It should not present a guessed or name-derived GUID relationship as native truth.

See [Identity Catalogue](catalogue.md) for curated example identities.
