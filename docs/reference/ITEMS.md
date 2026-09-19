# Items: Proven Custom Item Integration

> **Reference/process page.** This is the first fully reasoned new-content path. It teaches the proven runtime custom-item mechanism and the evidence explaining each step.

## What this system is

The proven baseline creates a **new FoA item template identity at runtime** by cloning a reviewed native prototype, registering the custom definition in FoA's loaded template maps, constructing a normal native `Item`, and giving that item to one controlled native acquisition surface.

The clearest bounded runtime proof is **Green Avalon Apple**.

~~~text
native ItemTemplate
→ clone
→ new mod-owned GUID/name
→ validate clone
→ register in native template map
→ resolve through TemplatesProvider
→ World.Add(new Item(...))
→ RestockableStock.AddItem(...)
→ shop UI shows separate custom item
~~~

This is different from replacing an existing Addressable with Merlin Workshop.

## Who owns it in FoA

The important owners are:

- `ItemTemplate` — reusable item definition;
- `TemplatesLoader` — native template-map insertion/loading;
- `TemplatesProvider` — normal template lookup;
- `World` — native/MVC item-instance ownership;
- `RestockableStock` — merchant stock owner for the first proven acquisition route;
- `ShopUI` — presentation owner that captures the stock item list.

## Important identities, types, and methods

Proven example identities:

~~~text
source:
ItemTemplate_Crafting_Cooking_Apple
1527b1864369efd48b49abb54f1e42e4

custom:
ItemTemplate_Mod_FoodDrink_AppleGreen
fdaf0000000000000000000000000001

display:
Green Avalon Apple
~~~

Important methods/surfaces:

- `TemplatesProvider.AllLoaded`
- `TemplatesProvider.Get<ItemTemplate>(guid)`
- `TemplatesLoader.FinishedLoading`
- private `TemplatesLoader.AddToMap(string, ITemplate)`
- `World.Add(new Item(template, quantity))`
- `RestockableStock.AddItem(item, allowStacking: true)`
- `ShopUI.OnFullyInitialized()`

## Where it exists in the lifecycle

The proven merchant path is:

~~~text
BepInEx plug-in loaded
→ FoA templates finish loading
→ source native ItemTemplate resolves
→ custom clone is validated and registered
→ custom GUID resolves through TemplatesProvider
→ Shop.OpenShop decompresses stock
→ ShopUI.OnFullyInitialized Prefix
→ create Item through World
→ add Item to RestockableStock
→ original ShopUI captures the item list
→ player sees the separate custom item
~~~

Timing is part of the process.

## How we interact with it

### Step 1 — Pick a reviewed native prototype

**Do:** resolve one existing `ItemTemplate` whose native behavior is close to what you want.

**Why:** an item template carries more than name and stats. It participates in native classification, attachments, UI behavior and other contracts.

**What goes wrong:** constructing an arbitrary item shape from assumptions can omit behavior/attachments that native code expects.

**Verify:** exact source GUID resolves and returns the expected `ItemTemplate`.

**Boundary:** no custom item exists yet.

### Step 2 — Create a separate mod-owned identity

**Do:** clone `sourceTemplate.gameObject`, obtain the cloned `ItemTemplate`, assign a new stable custom GUID/template name, and leave the source untouched.

**Why:** a new item must be distinguishable from its prototype. Reusing/mutating the native identity changes or collides with the original.

**What goes wrong:** source GUID = custom GUID creates replacement/collision risk; mutating the source changes the vanilla item.

**Verify:** source remains unchanged; clone has the new GUID/name and expected component shape.

**Boundary:** the clone still is not registered.

### Step 3 — Change only what the proof needs

**Do:** initially change the custom identity/presentation and keep native behavior/attachments from the safe source.

The Green Avalon Apple proof kept the source's native behavior, icon and ordinary item characteristics while proving the new identity path.

**Why:** fewer changed variables make a failure meaningful.

**What goes wrong:** changing identity, icon, value, effects, assets, recipe, vendor logic and behavior at once makes the failure impossible to localize.

**Verify:** clone contract checks pass and the native source was not mutated.

**Boundary:** custom visuals/effects are separate later integrations.

### Step 4 — Register the custom template

**Do:** after the template system is ready, insert the validated custom template into the native template maps.

Direct registration route:

~~~text
TemplatesProvider._loader
→ TemplatesLoader.AddToMap(customGuid, customTemplate)
~~~

**Why:** a cloned Unity object is not automatically discoverable by FoA's normal template lookup.

**What goes wrong:** without registration, downstream GUID lookup fails. Registering too early can race template readiness. Private reflection is patch-sensitive.

**Verify:** resolve the custom GUID back through `TemplatesProvider`.

**Boundary:** registration does not equal acquisition or save safety.

### Step 5 — Construct a normal FoA Item

**Do:**

~~~csharp
Item item = World.Add(new Item(customTemplate, quantity));
~~~

**Why:** the custom definition should enter the same runtime item ownership model as native items.

**What goes wrong:** a disconnected Unity object or private POCO is not a native inventory/shop item.

**Verify:** returned `Item` is valid and points at the custom template identity.

**Boundary:** the item still needs an acquisition owner.

### Step 6 — Integrate through one acquisition surface

For the first proven path, use merchant stock.

**Do:** after stock decompression but before the original shop UI captures its item list, add the native `Item` to a decompressed `RestockableStock`.

~~~csharp
stock.AddItem(item, allowStacking: true);
~~~

**Why:** registration and acquisition are different capabilities. The shop owner must receive the runtime item, and the UI must see it before its list snapshot.

**What goes wrong:** later mutations can leave a stale UI list; broader first-batch injection exposed descriptor/timing assumptions and merchant-menu failures.

**Verify:** stock contains the custom GUID and the shop UI visibly shows the separate custom item.

**Boundary:** this proves merchant acquisition, not loot/recipe/reward/world-pickup integration.

### Step 7 — Prove one item before generalising

**Do:** keep one known-good descriptor until the mechanism is visible and stable.

**Why:** the Apple proof established the mechanism. Later Bread/Cheese/other experiments tested whether it generalized.

**What goes wrong:** the early batch expansion outpaced per-item proof. The process had to narrow back to the screenshot-proven Apple path before broadening again.

**Verify:** one item succeeds end-to-end; each new family/profile proves its own preconditions.

**Boundary:** one successful prototype does not prove every `ItemTemplate` family behaves identically.

### Step 8 — Add assets, icons and gameplay separately

**Do:** only after identity/registration/acquisition works, add custom icon, model, effects or other presentation/behavior as separate lanes.

**Why:** later Gems work demonstrates that custom assets/icons/effects can be layered onto a working registered identity.

**What goes wrong:** if registration and custom presentation are introduced together, a white icon or missing model can be mistaken for registration failure.

**Verify:** test each layer independently.

**Boundary:** an AssetBundle loading does not register an item.

### Step 9 — Treat persistence as a separate gate

**Do:** if the item must survive save/load, validate that exact lifecycle separately.

**Why:** native-contract research shows item template identity is serialized by GUID and restored through template lookup.

**What goes wrong:** a saved item can reference a GUID that is unavailable during restoration.

**Verify:** disposable cold save/load plus documented missing/disabled-mod behavior.

**Boundary:** the current public item baseline does **not** claim universal uninstall/missing-mod/save safety.

## Why this route

This process is the smallest path that survived both successful and failed experiments:

- native item grant behavior was already understood;
- vendor lifecycle was probed read-only;
- one custom native-derived template was introduced;
- Green Avalon Apple became visible as a separate shop item;
- broad batch assumptions caused problems;
- the process narrowed;
- decompilation/lifecycle research corrected UI timing;
- later architecture work added collision/idempotency/persistence requirements without pretending those later gates were already passed.

## What goes wrong

The major failure lessons are:

- **prefab/template shape without registration:** FoA cannot resolve the new identity;
- **native GUID reuse:** collision/replacement risk;
- **too-early lookup:** provider/templates not ready;
- **too-late shop mutation:** UI can hold a stale list;
- **batch-first development:** one bad descriptor obscures which assumption failed;
- **asset-success confusion:** a loaded icon/model does not prove item registration;
- **runtime-success confusion:** visible current-session item does not prove save/uninstall safety.

## How to verify

Minimum first-item proof:

1. source native GUID resolves;
2. source remains unchanged;
3. custom GUID is unique and stable;
4. clone passes shape/profile checks;
5. registration succeeds;
6. custom GUID resolves through provider;
7. `World.Add(new Item(...))` succeeds;
8. controlled owner receives item;
9. downstream UI/gameplay sees it;
10. exact proof boundary is recorded.

Runtime evidence for the Green Avalon Apple established a separate visible custom identity in merchant UI with inherited native behavior/presentation characteristics.

## Current proof boundary

**Proven/bounded:** native-prototype clone, separate custom GUID, runtime registration mechanism, native Item creation, controlled merchant stock insertion, visible separate custom item.

**Source/multi-consumer evidence:** similar clone/registration shape appears in Lockpick, Longsword and later custom-content implementations.

**Not yet a general public guarantee:** cold-save restoration, missing-mod behavior, uninstall/orphan cleanup, cross-mod collision handling across all consumers, migration, arbitrary from-scratch ItemTemplate construction, universal loot/recipe/reward/world-pickup integration.

The internal project is moving toward a shared collision-safe registrar, but that registrar's full runtime/save validation gate is not yet promoted as the public baseline.
