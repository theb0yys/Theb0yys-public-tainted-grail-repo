# Custom Items

Use this page when you want to add a **new runtime item identity** without replacing an existing vanilla item.

The proven baseline is:

~~~text
native ItemTemplate
→ clone it
→ give the clone a new GUID/name
→ register it
→ resolve it normally
→ create a native Item
→ hand that Item to one native acquisition owner
→ verify the player can see/use it
~~~

The clearest bounded proof is the **Green Avalon Apple** merchant example.

## What actually owns an item

The important parts are:

- `ItemTemplate` — reusable item definition;
- `TemplatesLoader` — template-map loading/insertion;
- `TemplatesProvider` — normal template lookup;
- `World` — runtime `Item` Model ownership;
- `RestockableStock` — merchant-stock owner in the proven example;
- `ShopUI` — UI that snapshots/displays the stock.

A cloned prefab by itself is not a registered FoA item.

## Proven example identity

~~~text
source
ItemTemplate_Crafting_Cooking_Apple
1527b1864369efd48b49abb54f1e42e4

custom
ItemTemplate_Mod_FoodDrink_AppleGreen
fdaf0000000000000000000000000001

display name
Green Avalon Apple
~~~

## Step 1: choose a native prototype

Resolve one existing `ItemTemplate` that already behaves close to the item you want.

Why this helps:

- you inherit native classification;
- expected attachments are already present;
- normal UI/economy behavior is more likely to remain intact.

Do not build a completely guessed item shape first unless you are specifically researching that path.

Verify the source GUID resolves to the expected `ItemTemplate`.

## Step 2: give the clone its own identity

Clone the source template's GameObject, obtain the cloned `ItemTemplate`, and assign:

- a new stable mod-owned GUID;
- a new template name;
- any deliberately changed fields.

Do **not** reuse the source GUID.

Verify the source template stays unchanged.

At this point the clone still exists only in memory; FoA cannot necessarily resolve it by GUID yet.

## Step 3: change as little as possible

For the first proof, change only what you need.

The Green Avalon Apple proof kept most native behavior/presentation from the source while proving the identity and registration path.

Avoid changing identity, icon, model, price, effects, recipe, vendor logic, and acquisition route all at once. If the proof fails, you want to know which step failed.

## Step 4: register the template

After native templates are ready, insert the custom template into the native maps.

The direct proven route is:

~~~text
TemplatesProvider._loader
→ TemplatesLoader.AddToMap(customGuid, customTemplate)
~~~

Then resolve the custom GUID back through `TemplatesProvider`.

If the provider cannot resolve it, downstream consumers will not have a normal native identity to work with.

This direct route uses private internals and is patch-sensitive.

## Step 5: create a real native Item

Use the game's runtime Item model:

~~~csharp
Item item = World.Add(new Item(customTemplate, quantity));
~~~

This gives the custom definition a normal FoA runtime item instance.

A loose Unity object is not an inventory/shop item.

## Step 6: use one native acquisition route

The first proven route uses merchant stock.

After stock is decompressed but before the shop UI takes its item-list snapshot:

~~~csharp
stock.AddItem(item, allowStacking: true);
~~~

Then allow the normal shop UI flow to continue.

This proves merchant acquisition only. It does not prove loot, crafting, quest rewards, world pickups, or every inventory route.

## Step 7: prove one item before making a framework

The first successful item should stay simple and visible.

The Apple proof worked because the process narrowed to one known-good item after broader batch experiments exposed timing/descriptor assumptions.

Generalize only after one item succeeds end to end.

## Step 8: add custom presentation later

Once identity, registration, and acquisition work, add custom:

- icon;
- mesh/model;
- material;
- VFX;
- special behavior.

Asset loading and item registration are separate problems. A custom icon or model loading successfully does not prove the item exists correctly in game state.

## Step 9: test persistence separately

Native item/template research shows that saved template references are restored by GUID.

That means a saved custom item can fail later if its custom template is not registered early enough during load.

If the item must survive restart/save/load, test that exact lifecycle on a disposable save.

## Useful methods

- `TemplatesProvider.AllLoaded`
- `TemplatesProvider.Get<ItemTemplate>(guid)`
- `TemplatesLoader.FinishedLoading`
- private `TemplatesLoader.AddToMap(string, ITemplate)`
- `World.Add(new Item(template, quantity))`
- `RestockableStock.AddItem(item, allowStacking: true)`
- `ShopUI.OnFullyInitialized()`

## Common failures

- custom prefab exists but was never registered;
- source and custom GUID collide;
- registration happens before templates are ready;
- stock is changed after the UI already cached its list;
- several new item families are tested at once;
- asset success is mistaken for gameplay registration;
- current-session success is mistaken for save safety.

## Minimum proof

Verify:

1. source GUID resolves;
2. source stays unchanged;
3. custom GUID is unique/stable;
4. clone has the expected native shape;
5. registration succeeds;
6. custom GUID resolves through `TemplatesProvider`;
7. native `Item` construction succeeds;
8. one native acquisition owner receives it;
9. downstream UI/gameplay sees the separate custom item;
10. the exact proof boundary is recorded.

## Evidence limits

Proven in a bounded Mono route:

- native-prototype clone;
- separate custom GUID;
- runtime registration;
- native `Item` creation;
- controlled merchant-stock insertion;
- visible separate custom item.

Similar clone/registration shapes also appear in other project implementations.

Not yet a universal guarantee:

- cold-save restoration;
- missing-mod/uninstall behavior;
- collision handling across all mods;
- arbitrary from-scratch `ItemTemplate` construction;
- every acquisition route;
- cross-runtime equivalence.
