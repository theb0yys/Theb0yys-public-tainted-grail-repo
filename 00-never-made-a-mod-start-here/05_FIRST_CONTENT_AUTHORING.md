# Add Your First New Item

## What you're doing

You are proving the smallest currently documented route for adding a **separate new FoA item identity** rather than replacing an existing asset.

The exact public proof behind this lesson is a **Mono / BepInEx 5** runtime path. It creates a new runtime `ItemTemplate` by cloning a safe native prototype, gives the clone a new stable GUID, registers it in FoA's loaded template maps, creates a normal FoA `Item`, and inserts that item into one controlled merchant stock.

This is not a Merlin Workshop replacement mod.

## What you need

- a working Tainted Grail **Mono** installation;
- BepInEx 5 on the supported Mono lane;
- a C# plug-in project with local references to the required FoA/BepInEx assemblies;
- one exact native `ItemTemplate` GUID to use as the prototype;
- one new mod-owned custom GUID that you will keep stable;
- the [Items: Proven Custom Item Integration](../docs/reference/ITEMS.md) reference page open beside this tutorial.

If your installed game is IL2CPP, the **concepts** in the handbook still matter, but this exact private-loader registration route is not presented as an IL2CPP-proven process.

## What you'll learn

You will learn the difference between:

- a native template and a runtime `Item`;
- a cloned Unity object and a **registered** FoA definition;
- a native GUID and a custom/mod-owned GUID;
- registration and acquisition;
- item identity and asset presentation;
- current-session success and persistence proof.

## Steps

### 1. Understand the identity you are copying and the identity you are creating

For the bounded public example, the researched native prototype was:

~~~text
ItemTemplate_Crafting_Cooking_Apple
1527b1864369efd48b49abb54f1e42e4
~~~

The separate custom proof identity was:

~~~text
ItemTemplate_Mod_FoodDrink_AppleGreen
fdaf0000000000000000000000000001
Green Avalon Apple
~~~

Read [Identity: GUIDs, Names, Addresses, and Stable IDs](../docs/reference/IDENTITY_GUIDS_NAMES.md) before changing these concepts.

The important rule is:

**do not reuse or mutate the native template identity.**

### 2. Wait for the native template system

Resolve the source through the loaded template system only when FoA reports that templates are ready.

The researched lifecycle uses:

- `TemplatesProvider.AllLoaded`; and/or
- the `TemplatesLoader.FinishedLoading` lifecycle boundary as a registration retry point.

Why: a GUID lookup before the native template system is ready can fail even when the GUID itself is correct.

See [Templates and Registries](../docs/reference/TEMPLATES_REGISTRIES.md).

### 3. Resolve one known-good native prototype

Resolve the exact source `ItemTemplate` by GUID through `TemplatesProvider`.

Do not find the prototype by display-name guessing when you already have an exact GUID.

Your first success check is simple:

- the GUID resolves;
- the returned object is the expected `ItemTemplate`;
- you can identify the source template by exact native identity.

### 4. Clone the prototype instead of changing it

Clone the source template's `GameObject` and get the cloned `ItemTemplate`.

The proven shape is conceptually:

~~~csharp
GameObject cloneObject = Object.Instantiate(sourceTemplate.gameObject);
ItemTemplate customTemplate = cloneObject.GetComponent<ItemTemplate>();
~~~

Keep the clone alive for the runtime registration lifecycle.

Do **not** edit the source template.

Why: the native template is the prototype. Mutating it changes the original game item rather than creating a new item.

### 5. Give the clone its own stable identity

Set the custom clone's:

- template/object name;
- new custom GUID;
- player-facing name;
- only the minimum visibility/drop fields required for this proof.

Keep the native prototype's behavior/attachments initially.

Why: the first experiment should answer one question—**can FoA recognize and use a separate new identity?**

Do not add custom icons, models, recipes, loot and effects in the same first test.

### 6. Validate the clone before registration

Before native insertion, verify:

- source GUID and custom GUID differ;
- source object was not mutated;
- clone still contains the expected `ItemTemplate`;
- required component/attachment shape survived cloning;
- the custom identity is not already registered.

If a validation fails, destroy/reject the clone rather than inserting a malformed definition.

### 7. Register the new definition

The proven Mono path reaches the native loader and invokes the private template-map insertion:

~~~text
TemplatesProvider._loader
→ TemplatesLoader.AddToMap(customGuid, customTemplate)
~~~

This is a **private, patch-sensitive** native boundary.

Why it exists: a cloned Unity object is not automatically discoverable through FoA's normal template lookup.

After insertion, resolve the **custom GUID** through `TemplatesProvider`.

If it cannot be resolved through the normal provider, stop. Do not continue to inventory/vendor code.

### 8. Create a normal FoA runtime item

Once the custom template resolves normally, construct a native `Item` through `World`:

~~~csharp
Item item = World.Add(new Item(customTemplate, 1));
~~~

Why: the custom definition should participate in FoA's normal runtime object ownership rather than becoming a disconnected mod-only object.

See [Native Object Ownership](../docs/reference/NATIVE_OBJECT_OWNERSHIP.md).

### 9. Put the item into one controlled acquisition route

For the first proven public path, use merchant stock.

The corrected lifecycle is:

~~~text
Shop opens
→ RestockableStock decompresses
→ ShopUI.OnFullyInitialized Prefix
→ create/add custom Item
→ original ShopUI captures its item list
~~~

Add the `Item` to a decompressed `RestockableStock` with native stock ownership.

Why this timing matters: earlier experiments showed that changing the right data at the wrong UI lifecycle point can still leave the visible list stale.

See [Lifecycle and Hooks](../docs/reference/LIFECYCLE_HOOKS.md).

### 10. Prove one item in game

The first bounded runtime success should show:

- the custom GUID resolves;
- the stock receives exactly one item;
- the shop UI displays a **separate custom item**;
- the native prototype still exists unchanged;
- the new item inherits the expected native behavior/presentation for the proof.

The working-repo proof for Green Avalon Apple showed the separate custom item in merchant UI with its own name and inherited Apple characteristics.

### 11. Stop before broadening

Do not immediately turn one success into:

- twelve items;
- custom assets;
- recipes;
- loot;
- save claims;
- weapons;
- armour;
- creatures.

The original research broadened the custom-item batch too quickly, exposed descriptor/UI assumptions, then deliberately narrowed back to the one proven Apple path before expanding again.

That failure is part of the process.

## What success looks like

Your first new-item proof passes when you can show this complete chain:

~~~text
exact native prototype
→ separate stable custom GUID
→ valid clone
→ native registration
→ provider resolves custom GUID
→ World-owned Item
→ controlled merchant stock owner
→ visible separate custom item
~~~

You should also be able to explain **why each arrow exists**.

## Common problems

**You created an ItemTemplate-shaped object but cannot resolve its GUID:** the clone exists, but registration did not succeed.

**You changed the vanilla item instead of creating a separate one:** you mutated/reused the source identity.

**The stock contains the item but the UI does not show it:** check the shop/UI snapshot timing rather than immediately changing item metadata.

**A custom model/icon does not appear:** asset presentation is a separate layer. Prove item identity first.

**It works until you restart/load a save:** current-session registration is not the same as persistence proof. Read [Saving and Persistence](../docs/reference/SAVING_PERSISTENCE.md).

**You tried to apply the same process to a weapon/creature:** stop and map that domain's native owners first. See [Content Domains](../docs/reference/CONTENT_DOMAINS.md).

## Where to go next

- [Items: Proven Custom Item Integration](../docs/reference/ITEMS.md) — the complete rationale and proof boundary.
- [Failures and Constraints](../docs/reference/FAILURES_CONSTRAINTS.md) — why the process narrowed to this shape.
- [Hook Catalogue](../docs/reference/HOOK_CATALOGUE.md) — exact researched lifecycle surfaces.
- [Research Method](../docs/reference/RESEARCH_METHOD.md) — how to investigate the next content domain without inventing missing steps.
