# Items: Proven Custom Item Integration

> **Reference/process page.** This page documents the complete currently proven custom-item path reconstructed from the maintainer's private working evidence. It preserves the distinction between source/static evidence, runtime evidence, controlled validation, and still-unproven persistence behavior. It does not publish private source code or proprietary game material.

## Document status

**Domain:** items / inventory / merchant acquisition  
**Primary lane:** `custom_item_registration`  
**Adjacent lanes:** `existing_item_grant`, `vendor_injection`, `asset_localisation_injection`  
**Current public conclusion:** **PARTIAL PATH PROVEN, WITH A BOUNDED RUNTIME-PROVEN ITEM REGISTRATION + MERCHANT ACQUISITION SLICE**

The strongest end-to-end runtime proof is the Green Avalon Apple merchant path. Independent source-inspected consumers such as Tainted Lockpick and Realistic Longsword confirm that the registration and native `Item` construction shape is not Apple-specific.

This page deliberately does **not** claim that the complete durable custom-item problem is solved. Cold save/load restoration, disabled-mod behavior, orphaned saved items, migration, cross-mod collision handling, and universal acquisition surfaces remain separate proof requirements.

---

# 1. What this system is

FoA separates an **item definition** from a **runtime item instance**.

A new custom item therefore needs two different things:

1. a registered `ItemTemplate` identity that native lookup can resolve; and
2. a normal runtime `Item` created from that template and handed to a native acquisition owner.

The proven baseline is:

~~~text
reviewed native ItemTemplate prototype
→ separate mod-owned GUID + template name
→ clone native template GameObject
→ preserve native component/attachment shape
→ apply bounded custom identity/presentation fields
→ wait for native template registry readiness
→ insert custom template into native template maps
→ resolve custom GUID back through TemplatesProvider
→ create normal Item through World.Add(new Item(...))
→ hand Item to one native acquisition owner
→ verify the downstream UI/gameplay owner sees the same runtime item
~~~

The first runtime-proven merchant variant continues:

~~~text
Shop.OpenShop()
→ Stock.ShopOpened()
→ RestockableStock decompressed
→ ShopUI.OnFullyInitialized Prefix
→ create World-owned Item
→ RestockableStock.AddItem(...)
→ original ShopUI builds tab/list snapshots
→ custom item appears as a separate normal shop item
~~~

These stages are not interchangeable.

~~~text
template object exists
!= template registered
!= template resolves
!= Item exists
!= inventory/merchant owns Item
!= UI has captured Item
!= Item persists across save/load
~~~

That separation is the central rule of the item process.

---

# 2. Who owns each stage in FoA

## 2.1 Definition owner — ItemTemplate

`ItemTemplate` is the reusable item definition.

It carries or participates in:

- GUID identity;
- template name;
- abstract-template lineage;
- tags and inherited tags;
- localization-backed item name;
- optional description/flavour;
- icon references;
- economy values;
- weight and quality;
- classification;
- drop/pickable references;
- attachment groups;
- equip data when present;
- magic/cast metadata when present;
- item-specific native logic inherited through components and attachments.

A custom item that only copies the visible name and price is not equivalent to a native item template.

## 2.2 Registry owner — TemplatesLoader

The inspected Mono build shows `TemplatesLoader` owns:

~~~text
guidMap : Dictionary<string, ITemplate>
typeMap : MultiMap<Type, ITemplate>
~~~

Its private `AddToMap(string guid, ITemplate template)` performs the native map insertion used by the direct registration precedent.

Static evidence establishes that the method:

1. adds the template to the GUID map;
2. adds it under its runtime type;
3. assigns `template.GUID = guid`.

The direct route is powerful but patch-sensitive because the method and provider loader field are private implementation surfaces.

## 2.3 Lookup owner — TemplatesProvider

`TemplatesProvider` is the normal lookup service.

Important behavior:

- `AllLoaded` mirrors loader readiness;
- normal lookup validates state;
- lookup before template loading is complete is invalid;
- successful custom registration must be proven by resolving the new GUID back through this provider.

The provider is the round-trip proof that the new identity entered the native lookup model.

## 2.4 Runtime object owner — World

The proven native item-construction shape is:

~~~csharp
Item item = World.Add(new Item(template, quantity));
~~~

The `Item` is a normal FoA MVC Model, not a loose Unity object and not a private mod DTO.

## 2.5 Acquisition owners

Different acquisition surfaces own different transitions.

Proven or source-inspected examples include:

- `HeroItems.Add(item)` for direct hero inventory acquisition;
- `RestockableStock.AddItem(item, allowStacking: true)` for merchant stock;
- other loot/reward/container/world-pickup lanes are separate and must not be inferred from these two.

## 2.6 Presentation owners

Item presentation is downstream of registration.

Depending on context it may involve:

- inventory list/tooltip code;
- shop tab/list snapshots;
- native template icon references;
- localization/token text;
- equipment representation for equippable items;
- custom runtime icon overrides in separately proven integrations.

A registered template with a broken icon is still a registered template. A beautiful icon with no registered template is not a custom item.

---

# 3. Important identities, types, and methods

## 3.1 Green Avalon Apple bounded proof

~~~text
source template name:
ItemTemplate_Crafting_Cooking_Apple

source GUID:
1527b1864369efd48b49abb54f1e42e4

custom template name:
ItemTemplate_Mod_FoodDrink_AppleGreen

custom GUID:
fdaf0000000000000000000000000001

display name:
Green Avalon Apple

bounded merchant:
Shop_Vendor_Tier1

merchant GUID:
75a071140bc819d4ab6e9e37abfdfa59
~~~

## 3.2 Tainted Lockpick cross-consumer source evidence

~~~text
source GUID:
4874d14cab8060c4a981ae4440298096

custom GUID:
10b2665ab64c75f51aaf836574aa0dd1

custom template name:
ItemTemplate_Mod_TaintedLockpick
~~~

The Tainted Lockpick implementation independently demonstrates:

- wait/retry around template readiness;
- exact native source resolution;
- `GameObject` cloning;
- separate custom GUID/name;
- component-type shape comparison;
- private loader-map insertion;
- provider re-resolution;
- `World.Add(new Item(...))`;
- `HeroItems.Add(...)`;
- inventory duplicate checking by template GUID.

This is source evidence, not a universal save-safety proof.

## 3.3 Important native surfaces

- `TemplatesLoader.FinishedLoading`
- `TemplatesProvider.AllLoaded`
- `TemplatesProvider.Get<ItemTemplate>(guid)`
- `TemplatesProvider.GetAllOfType<ItemTemplate>()`
- private `TemplatesProvider._loader`
- private `TemplatesLoader.AddToMap(string, ITemplate)`
- `ItemTemplate`
- `Item`
- `World.Add(...)`
- `HeroItems.Add(...)`
- `Shop.OpenShop()`
- `Stock.ShopOpened()`
- `Stock.Items`
- `RestockableStock`
- `RestockableStock.AddItem(...)`
- `ShopUI.OnFullyInitialized()`
- `ItemsListUI.SetupItemByTabs()`
- `ItemsListUI.GetViewParentForItem(Item)`
- `ItemsTabType`

## 3.4 Save-relevant surfaces

Static current-build evidence additionally identifies:

- `SaveWriter.WriteTemplate<T>()`;
- `SaveReader.ReadTemplate<T>()`;
- `TemplatesUtil.Load<T>()`;
- `Item.Serialize` / `Deserialize`;
- `Item.OnPreRestore()`;
- `Item.OnRestore()`;
- `Item.OnFullyInitialized()`.

These are persistence evidence, not persistence validation.

---

# 4. Lifecycle map

## 4.1 Native template startup

The inspected static template load lifecycle is:

~~~text
TemplatesLoader.CreateAndLoad()
→ LoadAssets()
→ LoadAssetsInBuild()
→ Addressables locations for "template" + "templateSO"
→ load native template assets
→ AddToMap(nativeGuid, nativeTemplate)
→ both label groups finish
→ TemplatesLoader.FinishedLoading = true
→ TemplatesProvider.AllLoaded = true
~~~

A custom-template consumer must respect that readiness boundary.

## 4.2 Custom registration lifecycle

The direct working pattern is:

~~~text
plugin loads
→ install readiness retry hook and/or update-loop retry
→ wait until TemplatesProvider exists
→ require TemplatesProvider.AllLoaded
→ check whether custom GUID already resolves
→ resolve reviewed native source template
→ validate source is usable for this lane
→ clone source GameObject
→ obtain cloned ItemTemplate
→ assign custom identity/presentation
→ compare required shape/profile
→ invoke TemplatesLoader.AddToMap(customGuid, customTemplate)
→ resolve customGuid through TemplatesProvider
→ retain clone for required process lifetime
~~~

## 4.3 Native Item lifecycle

Static item evidence shows the runtime object is saved and restored as an MVC Model.

Its lifecycle includes:

~~~text
construct Item(template, quantity, ...)
→ World adopts Model
→ attachment tracker initialized from template
→ native item state/listeners initialized
→ native inventory/stock owner adopts Item
→ quantity/equip/use behavior runs through native systems
→ serialization records Item state + template identity
~~~

## 4.4 Merchant acquisition lifecycle

Corrected runtime/decompilation route:

~~~text
Shop.OpenShop()
→ each Stock.ShopOpened()
→ stock CreateAllItems()/decompression
→ Shop UI begins initialization
→ ShopUI.OnFullyInitialized Prefix
    → verify exact target shop
    → verify stock is RestockableStock
    → verify stock is decompressed
    → ensure/resolve custom ItemTemplate
    → duplicate check
    → World.Add(new Item(customTemplate, quantity))
    → stock.AddItem(...)
→ original ShopUI.OnFullyInitialized continues
→ ShopUITabs / ItemsUI / ItemsListUI initialize
→ ItemsListUI snapshots current items
→ player sees custom item
~~~

This timing was not guessed. It was derived from a concrete failure.

---

# 5. Proven process — preparation gate

Before writing registration code, define the exact lane.

For this page the lane is:

~~~text
register one custom item cloned from reviewed prototype P
and expose it through one separately named acquisition surface
~~~

Do **not** use scopes such as:

~~~text
add a new item everywhere
make custom consumables
make an item system
~~~

Those phrases hide multiple independent capabilities.

Record before implementation:

- source prototype identity;
- custom GUID;
- custom template name;
- display/localization policy;
- duplicate key;
- expected native component/attachment shape;
- acquisition lane;
- asset/icon policy;
- session/durable intent;
- rollback/non-reversibility posture;
- failure behavior;
- known compatibility assumptions.

If any exact identity required by the selected lane is unknown, that is a blocker for that claim.

---

# 6. Proven process — choose a reviewed native prototype

## What to do

Select one existing native `ItemTemplate` whose behavior is already close to the target item.

Resolve it by exact GUID through `TemplatesProvider`.

## Why

The source template carries native behavior beyond visible fields.

Depending on the item it may encode:

- abstract lineage;
- category behavior;
- attachment groups;
- tool or lockpick logic;
- consumable behavior;
- equipment behavior;
- UI category membership;
- value/weight defaults;
- use text;
- icon references;
- drop/pickable behavior;
- effect references.

## What not to do

Do not build an arbitrary `GameObject + ItemTemplate` shape from guessed fields and assume it is native-equivalent.

Do not select a source only because its display name sounds similar.

## Verification

Minimum source preflight:

- provider exists;
- provider is loaded;
- exact source GUID resolves;
- resolved object is an `ItemTemplate`;
- source is not abstract when the target requires a normal visible item;
- source is not hidden when the target must appear in UI;
- source is droppable/usable as required by the selected lane;
- source template family matches the intended behavior closely enough for the bounded proof.

## Proof boundary

Source resolution proves only that the prototype exists and can be read.

It does not prove cloning, registration, acquisition, assets, or save behavior.

---

# 7. Proven process — allocate a separate mod-owned identity

## Required identity separation

A genuine new item must have a different persistent template identity from its native prototype.

Use:

- a stable custom GUID;
- a stable custom template name;
- a distinct package/mod ownership convention.

## Why

Reusing the native GUID creates replacement or collision behavior rather than a distinct item.

Mutating the original template also changes vanilla behavior globally for every consumer of that native identity.

## Proven rule

~~~text
native source identity remains native-owned
custom item receives mod-owned identity
~~~

## Verification

Before registration:

- source GUID and custom GUID differ;
- no existing template resolves under the custom GUID;
- custom template name is not the native template name;
- source template object remains unchanged;
- identity is stable across mod versions unless a migration plan explicitly changes it.

## Failure behavior

A collision is a hard stop for that registration attempt.

Do not silently choose another GUID at runtime. That would break persistence and cross-session identity.

---

# 8. Proven process — clone the native template object

## Direct route

The established Mono implementation pattern clones the source template's `GameObject`.

Conceptually:

~~~text
sourceTemplate.gameObject
→ Unity clone
→ cloned ItemTemplate component
→ mod-owned custom template
~~~

## Why clone the whole object

The source object's component graph may contain native logic that is not represented by a few public `ItemTemplate` fields.

Tainted Lockpick explicitly compares component-type sets between source and clone before accepting the clone.

That check is bounded—it does not prove every serialized reference is semantically equivalent—but it prevents an obvious class of accidental topology loss.

## Lifetime

The clone must remain alive for as long as native lookup may return it.

Existing direct implementations use a non-scene lifetime strategy for successful registered clones.

Do not register an object and then destroy it as if the registry copied the definition.

## Verification

Check:

- clone was created;
- cloned object contains exactly one intended `ItemTemplate`;
- required native components remain present;
- clone has not accidentally inherited the original object name as its final project identity;
- source object was not mutated;
- clone lifetime is explicitly owned.

---

# 9. Proven process — modify only bounded fields

For the first proof, modify the minimum required state.

Typical first-proof changes:

- custom GUID;
- template/object name;
- visible display name;
- description/fallback text where required;
- bounded flags required for a normal visible/droppable item.

Keep native behavior from the reviewed prototype unless a separate lane proves a modification.

The Green Avalon Apple proof intentionally retained source behavior and ordinary item characteristics.

The first-batch food/drink work also deliberately kept:

- inherited icons;
- values;
- weight;
- use text;
- attachments;
- gameplay logic.

## Why

If identity, assets, effects, value, recipe integration, vendor integration, and UI behavior all change together, a failure cannot be localized.

The working process therefore establishes one variable class at a time.

---

# 10. Proven process — validate clone shape before registration

Validation should happen **before** native map mutation.

Recommended bounded checks from current evidence:

- source/custom GUID separation;
- custom identity not already registered;
- expected runtime type;
- expected component-type profile;
- expected attachment/classification assumptions;
- no source mutation;
- required fields populated;
- no unsupported family/profile mismatch.

Weapon-specific registrar work uses stronger bounded clone-profile comparisons; generic item documentation should not claim those weapon profiles are automatically sufficient for every item family.

The principle is reusable:

~~~text
validate before native visibility
~~~

because `AddToMap` is not a transactional API with a proven general rollback path.

---

# 11. Proven process — wait for TemplatesProvider readiness

## Required readiness

Do not perform normal template resolution until:

~~~text
TemplatesProvider exists
AND TemplatesProvider.AllLoaded == true
~~~

Static evidence shows provider lookup validates this state and may throw before loading is complete.

## Working retry strategies

Source precedent includes:

- a Harmony postfix on `TemplatesLoader.FinishedLoading`;
- a bounded periodic retry from the plugin update loop.

The hook is a readiness signal, not the actual registration contract.

## Failure handling

If the provider or loader is unavailable:

- do not invent a fallback registry;
- do not create a disconnected private template map;
- do not assume the next frame is safe without rechecking;
- report a bounded “not ready” condition and retry only according to the owning process.

---

# 12. Proven process — register in the native template maps

## Direct historical route

The direct proven Mono route obtains the loader from the provider and calls the private native insertion method:

~~~text
TemplatesProvider._loader
→ TemplatesLoader.AddToMap(customGuid, customTemplate)
~~~

## What static evidence establishes

The inspected current-build body inserts into:

- GUID lookup;
- runtime-type lookup;

and assigns the GUID onto the template object.

## What this proves

After a successful insertion and round-trip lookup, the custom template is visible through normal native lookup in that process session.

## What it does not prove

It does not prove:

- every downstream system refreshes late-added definitions;
- persistence works;
- hot unregister works;
- duplicate insertion is safe;
- failed partial insertion can be rolled back;
- another game build uses the exact same private members;
- IL2CPP uses an equivalent callable surface.

## Non-transactional risk

The static implementation performs multiple mutations.

The broader registrar research therefore treats post-insertion uncertainty as dangerous.

For a robust registrar:

- collision checks happen before insertion;
- profile checks happen before insertion;
- failures before insertion should leave native state untouched;
- uncertainty after native visibility should fail-stop rather than “try another item and hope”;
- process restart may be the only safe recovery until rollback is proven.

The public generic item process must retain this limitation.

---

# 13. Proven process — round-trip through TemplatesProvider

Registration is not considered successful merely because reflection returned without throwing.

Immediately verify:

~~~text
TemplatesProvider.Get<ItemTemplate>(customGuid)
~~~

and require the resolved template to match the inserted custom identity.

## Why

The point of registration is not “we called AddToMap.”

The point is:

~~~text
normal FoA template lookup can now resolve the new definition
~~~

## Verification questions

- Does the custom GUID resolve?
- Is the resolved object an `ItemTemplate`?
- Does its GUID equal the intended custom GUID?
- Does its template/name profile match the custom definition?
- Is the source template still separately resolvable under its original GUID?

If the round trip fails, do not continue into acquisition.

---

# 14. Proven process — create a normal native Item

Once the template resolves through the provider, construct a native runtime item.

Proven shape:

~~~csharp
Item item = World.Add(new Item(customTemplate, quantity));
~~~

## Why World ownership matters

`Item` is an MVC Model with:

- initialization;
- attachment tracking;
- quantity logic;
- inventory events;
- equip events when applicable;
- serialization behavior;
- native text setup;
- native owner/listener integration.

A raw `new Item(...)` that never enters `World`, or an unrelated Unity prefab instance, is not equivalent to the normal runtime ownership chain.

## Verification

Before handing the item to an acquisition owner:

- `Item` creation succeeded;
- returned model is not discarded;
- `Item.Template` points to the custom template;
- quantity is valid;
- selected acquisition owner is available.

---

# 15. Acquisition lane A — direct hero inventory grant

Independent source precedent uses:

~~~text
World.Add(new Item(template, quantity))
→ HeroItems.Add(item)
~~~

## Required preconditions

- `Hero.Current` exists;
- hero is not discarded;
- `HeroItems` exists;
- `HeroItems` is not discarded;
- custom template resolves;
- duplicate policy is evaluated before grant.

## Duplicate behavior

Tainted Lockpick uses template GUID scanning to determine whether the hero already owns the custom item before granting another.

That is a concrete example of an important rule:

~~~text
registration idempotency
and acquisition idempotency
are different problems
~~~

A template should normally be registered once per process identity.

A player grant may have quantity/duplicate semantics specific to that item.

## Proof boundary

Hero grant proves the inventory owner accepted the item.

It does not prove:

- vendor behavior;
- world pickup;
- loot tables;
- recipe output;
- reward integration;
- persistence.

---

# 16. Acquisition lane B — merchant stock

Merchant integration produced the most important lifecycle lesson in the item process.

## 16.1 First stock prerequisite — decompression

Static research established:

- `Stock.Items` is not safely available while compressed;
- `Stock.ShopOpened()` creates/decompresses live stock items;
- `RestockableStock.AddItem(...)` is the correct live-owner mutation used by the proof.

Therefore:

~~~text
do not inject into compressed stock assumptions
do not rewrite ShopTemplate arrays for this route
wait for decompressed RestockableStock
~~~

## 16.2 Early working route

The first mutation proof added one item after shop opening:

~~~text
World.Add(new Item(...))
→ Stock.AddItem(...)
~~~

Native Apple proved that the basic decompressed-stock ownership path worked.

## 16.3 Batch expansion failure

The first-batch custom work expanded too quickly.

Twelve custom identities were prepared for:

- Apple;
- Bread;
- Cheese;
- Carrot;
- Tomato;
- Beef;
- Wine;
- Ale;
- Whisky;
- three bottle/material descriptors.

The registration/stock lane remained config-gated and clone-based, but the batch widened assumptions faster than each descriptor had been individually proven.

The process was deliberately narrowed again.

## Rule produced by that failure

~~~text
one known-good descriptor first
→ validate end to end
→ add one next descriptor/family
→ validate its own category/behavior assumptions
~~~

Batch success must not be used to hide per-item failures.

---

# 17. The stale shop UI snapshot failure

This is one of the most important failure-derived rules in the entire item process.

## 17.1 Observed symptom

A custom Wine item could be present in live stock and later appear under Crafting/Alchemy, while the initial shop UI hit a `KeyNotFoundException` involving:

~~~text
ItemsTabType.None
~~~

An early hypothesis treated this as though the item had an invalid “None” category field.

That hypothesis was wrong.

## 17.2 Decompiled UI behavior

The corrected analysis established:

1. `Shop.OpenShop()` calls `Stock.ShopOpened()` before building shop UI.
2. Stock is decompressed before the UI is constructed.
3. `ItemsListUI.SetupItemByTabs()` snapshots current item references into tab lists.
4. `GetViewParentForItem(Item)` searches those **captured runtime item references**.
5. If the current live item is absent from every captured list, the fallback attempts `ItemsByTab[ItemsTabType.None]`.
6. The initial All-tab setup did not create a `None` list.
7. Adding the custom item **after** the initial list snapshot therefore created a stale-list mismatch.

The item itself was not categorized as None.

## 17.3 Corrected lifecycle sequence

The fix was not:

- invent a tab property;
- patch a permanent None category;
- swallow the exception;
- patch `GetViewParentForItem`;
- mutate unrelated item metadata.

The correct fix was to place stock insertion earlier:

~~~text
after Stock.ShopOpened() decompression
but before ItemsListUI captures the item references
~~~

The narrow hook selected was:

~~~text
ShopUI.OnFullyInitialized() Prefix
~~~

## Rule produced by the failure

When a downstream UI snapshots owner state:

~~~text
mutate the native owner before the snapshot
not after the snapshot
~~~

This is a general lifecycle principle, but the exact hook remains version-sensitive.

---

# 18. Correct merchant insertion process

For the bounded known merchant path:

## Step 1

Confirm the opened shop is the exact intended shop/template.

## Step 2

Find the live stock owner and require the expected `RestockableStock`.

## Step 3

Require that stock is already decompressed.

## Step 4

Ensure the custom template is registered.

## Step 5

Resolve the custom template through `TemplatesProvider`.

## Step 6

Check whether that exact custom template is already represented in the live shop state according to the lane's duplicate policy.

## Step 7

Create the native `Item` through `World`.

## Step 8

Call:

~~~text
RestockableStock.AddItem(item, allowStacking: true)
~~~

before the original shop initializer builds its item-list snapshots.

## Step 9

Allow the original native UI initialization to continue.

## Step 10

Verify:

- custom item exists in live stock;
- no duplicate was added on reopen;
- UI shows the item;
- expected category/tab matches inherited item behavior;
- no `ItemsTabType.None` exception occurs;
- compressed stock and `ShopTemplate` were not mutated.

---

# 19. Category behavior and why prototype choice matters

The Wine incident also demonstrated that category behavior came from the inherited native item contract.

The source Wine clone continued to satisfy native classification such as Crafting/Alchemy behavior.

This matters because the UI did not need a fabricated category field.

The lesson is:

~~~text
preserve a correct native prototype first
before overriding classification
~~~

If the target must change category semantics, that becomes another separately researched lane.

Do not assume category membership is a single editable enum.

---

# 20. Assets are a separate lane

The item process began alongside AssetBundle experiments, which created a recurring confusion:

~~~text
asset loaded successfully
!= gameplay item registered
~~~

A prefab or icon can load without any `ItemTemplate` registration.

A custom template can register while still inheriting a native icon/model.

Therefore asset integration must be staged separately.

## Asset lane questions

- Who owns the file/package?
- How is it loaded?
- What is its lifetime?
- Which exact item field or runtime presentation owner consumes it?
- What happens when it is absent?
- Is fallback allowed?
- Is the asset legal to redistribute?
- Does the target runtime/render pipeline accept it?

The first item registration proofs deliberately inherited native presentation to isolate registry/acquisition behavior.

---

# 21. Localization is a separate lane

A runtime clone can use bounded fallback text for proof purposes, but a release-quality localized item requires its own localization policy.

Do not confuse:

- template name;
- custom GUID;
- user-facing display name;
- localization key/token;
- fallback text.

Changing one does not register the others.

A public process should state which level is actually implemented.

---

# 22. Item-specific gameplay behavior is a separate lane

The clone process preserves source behavior unless a later gate changes it.

Examples of later item-specific work may include:

- custom use effects;
- skill-reference rewrites;
- lockpick-specific mechanics;
- consumable effects;
- spell graphs;
- equipment behavior;
- custom durability;
- status application.

Each has its own native owner.

Do not treat “the custom item appears” as proof that custom gameplay logic is correct.

---

# 23. Duplicate and idempotency model

A reusable registrar/acquisition implementation must distinguish at least four duplicate classes.

## 23.1 Registry identity duplicate

Question:

~~~text
does custom GUID already resolve?
~~~

If yes, determine whether it is:

- the same intended definition;
- a compatible existing registration;
- a foreign/colliding definition;
- an inconsistent changed definition.

Do not blindly reinsert.

## 23.2 Source/custom identity collision

Reject:

~~~text
customGuid == sourceGuid
~~~

for a new distinct item.

## 23.3 Inventory duplicate

The hero may already own one or more runtime items with the custom template GUID.

The grant lane decides whether to:

- skip;
- top up to a minimum;
- stack;
- grant another distinct item.

That policy belongs to acquisition, not registration.

## 23.4 Merchant duplicate

A shop may be reopened in the same session.

The merchant lane must avoid repeatedly adding the same custom stock unintentionally.

Current source precedent uses bounded session duplicate state and/or live stock checks.

---

# 24. Failure handling before native visibility

Before `AddToMap`, failure should be clean.

Examples:

- provider unavailable;
- provider not loaded;
- source not found;
- source unsafe for selected lane;
- loader reflection surface missing;
- `AddToMap` method missing;
- clone missing `ItemTemplate`;
- custom identity collision;
- component/profile mismatch.

The expected behavior is:

~~~text
log bounded reason
destroy unregistered temporary clone when appropriate
leave native maps unchanged
do not continue to acquisition
~~~

---

# 25. Failure handling after native visibility

This is much more serious.

The native insertion path is not proven transactional.

If an exception or mismatch occurs after one or more native registry mutations:

- do not pretend registration rolled back;
- do not keep registering unrelated definitions as though the registry were certainly clean;
- do not hot-unregister without proof;
- record the process/session state as uncertain or poisoned;
- require restart for a clean native registry when the owning registrar policy demands it.

This fail-stop principle is especially explicit in the weapon registrar work and is relevant to any shared registrar built on the same private native seam.

---

# 26. Process-session immutability

No proven general native unregister path has been established for the direct `TemplatesLoader.AddToMap` route.

Therefore a conservative rule is:

~~~text
successful custom registration is process-session state
not a hot-reload toy
~~~

Plugin shutdown can release plugin-owned side assets and patches, but removing a template identity already referenced by runtime/saved objects is a separate unresolved problem.

---

# 27. Persistence architecture — what static evidence proves

Current-build static research establishes that `Item` is saved as a runtime Model and includes its template identity.

The key chain is:

~~~text
Item saved
→ Item.Template serialized as template GUID
→ SaveWriter.WriteTemplate<T>()
→ GUID bytes stored
...
load/restore
→ SaveReader.ReadTemplate<T>()
→ TemplatesUtil.Load<T>()
→ template GUID must resolve
→ Item attachment/state reconstruction continues
~~~

This is the architectural reason custom template identity is persistence-critical.

---

# 28. Persistence architecture — what is not yet proven

The static chain does **not** establish:

- exact failure behavior when the custom GUID is missing;
- safe disabled-mod behavior;
- safe uninstall;
- safe downgrade;
- migration from one custom GUID to another;
- whether registration always occurs before every relevant restore point;
- equipped custom-item restoration;
- orphan cleanup;
- cross-mod collision recovery.

Therefore:

~~~text
current-session visibility
!= save/load proof
~~~

The public baseline must keep persistence explicitly gated.

---

# 29. Required persistence validation for durable claims

A durable custom-item claim should eventually include at minimum:

## Positive control

- clean start;
- custom template registers;
- player acquires custom item;
- save completes;
- game/process exits;
- cold restart;
- template registration occurs in the required order;
- save loads;
- item restores with correct custom GUID;
- quantity/state are correct;
- use/equip behavior still works if claimed.

## Missing-mod negative control

- save containing the custom item exists;
- mod/template registration is deliberately absent;
- observed load behavior is recorded;
- failure is bounded;
- recovery expectations are documented.

## Version migration control

If schema/identity changes:

- old version save created;
- new version loads it;
- alias/migration behavior is explicit;
- no silent item duplication/loss;
- downgrade policy is explicit.

## Uninstall/orphan policy

State whether removal is:

- unsupported;
- requires removing custom items before uninstall;
- automatically migrated;
- automatically cleaned;
- safely ignored.

Do not leave this implicit.

---

# 30. Save ordering hazard

The static architecture reveals a critical ordering requirement:

~~~text
custom template GUID must resolve
before a saved Item that references that GUID is restored
~~~

That means template registration timing is not merely startup convenience.

For durable custom items it becomes part of the save contract.

A future shared registrar therefore needs runtime ordering evidence, not only source inspection.

---

# 31. Cross-mod collision requirements

A custom item ecosystem cannot assume one mod owns the whole GUID space.

A release-grade shared registrar should define:

- stable owner namespace;
- collision detection;
- same-owner same-definition idempotency;
- same-GUID different-definition rejection;
- template-name collision policy;
- cross-mod diagnostics;
- migration aliases if ever supported;
- deterministic handling across load order.

Current direct consumer-local implementations are evidence sources.

They are not yet a universally promoted shared collision-safe registrar.

---

# 32. Compatibility requirements

The direct route relies on private members.

That makes compatibility sensitive to:

- game build;
- Mono vs IL2CPP;
- `TemplatesProvider` field layout;
- `TemplatesLoader.AddToMap` signature/behavior;
- template startup ordering;
- Item constructor/runtime behavior;
- shop lifecycle;
- UI snapshot behavior.

A new build requires revalidation of these assumptions.

Do not label the route universally compatible because it worked in one inspected/runtime environment.

---

# 33. Why the Apple proof matters

The Green Avalon Apple proof established several facts together in one narrow environment:

- a distinct custom GUID could be registered;
- normal provider lookup could resolve it;
- a normal native `Item` could be built from it;
- decompressed merchant stock could own that `Item`;
- the item could appear as a separate visible merchant entry;
- inherited native item characteristics remained usable.

That is stronger than “a clone object existed.”

It is still bounded to its tested lane.

---

# 34. Why the Lockpick proof matters

Tainted Lockpick independently establishes a second consumer shape:

~~~text
native prototype
→ clone
→ custom GUID
→ native logic/component-shape preservation
→ native map insertion
→ provider resolution
→ World-owned Item
→ HeroItems owner
~~~

That supports the claim that the core definition/instance separation is reusable.

It does not prove every source family is safe to clone.

---

# 35. Why the Longsword proof matters—and what it does not prove

Realistic Longsword independently uses the custom registration/native Item/acquisition pattern.

That is useful item-layer evidence.

Its equipped-weapon presentation introduces additional owners:

- `ItemEquip`;
- `CharacterHandBase`;
- `CharacterWeapon`;
- renderer/Drake;
- perspective and preview surfaces.

Those are weapon-domain problems and must not be back-projected into the generic item process.

---

# 36. Failure history and the rule each failure produced

## Failure: “clone exists, so item exists”

**Correction:** registration and runtime `Item` construction are separate.

## Failure: native GUID reused for a “new” item

**Correction:** custom content requires a separate mod-owned identity.

## Failure: provider accessed too early

**Correction:** wait for `TemplatesProvider.AllLoaded`.

## Failure: broad batch enabled before one-at-a-time proof

**Correction:** prove one descriptor/family at a time before expansion.

## Failure: merchant item added after initial UI snapshot

**Observed result:** stale runtime-reference snapshot and `ItemsTabType.None` fallback exception.

**Correction:** mutate decompressed stock before the UI captures lists.

## Failure: wrong diagnosis that Wine had a None tab field

**Correction:** trace the actual owner/snapshot chain instead of inventing metadata.

## Failure: asset success treated as gameplay-registration success

**Correction:** asset loading and template registration are separate lanes.

## Failure: current-session visibility treated as persistence proof

**Correction:** save/load/uninstall/migration require their own controlled validation.

---

# 37. Minimal one-item implementation sequence

For a new bounded custom item, the minimum process is:

1. Identify exact native source prototype.
2. Record source GUID and intended custom GUID/name.
3. Prove custom GUID is not the source GUID.
4. Wait for `TemplatesProvider.AllLoaded`.
5. Resolve source template.
6. Validate source suitability.
7. Clone the source object.
8. Get the cloned `ItemTemplate`.
9. Apply bounded custom identity/presentation fields.
10. Verify required component/attachment/profile assumptions.
11. Confirm custom GUID is not already occupied by a conflicting template.
12. Insert the validated clone into native template maps.
13. Resolve the custom GUID through `TemplatesProvider`.
14. Stop if round-trip resolution fails.
15. Construct a normal `Item` through `World`.
16. Hand it to exactly one selected acquisition owner.
17. Verify that owner now contains the same custom identity.
18. Verify the downstream UI/gameplay surface for that owner.
19. Verify duplicate behavior by repeating the relevant lifecycle event.
20. Record the exact evidence boundary.
21. Do not claim persistence unless the separate persistence gate ran.

---

# 38. Merchant-specific implementation sequence

1. Confirm exact target shop.
2. Let `Shop.OpenShop()` begin native opening.
3. Let `Stock.ShopOpened()` decompress stock.
4. Enter the `ShopUI.OnFullyInitialized()` **Prefix**.
5. Confirm the expected stock instance/type.
6. Confirm the stock is not compressed.
7. Ensure custom template registration is complete.
8. Resolve custom template normally.
9. Check stock/session duplicate policy.
10. Create `Item` through `World`.
11. Add to `RestockableStock`.
12. Let the original initializer build tabs and item-list snapshots.
13. Verify visible item.
14. Verify expected category behavior.
15. Close and reopen shop.
16. Verify no unintended duplicate.
17. Verify no shop UI exception.
18. Verify compressed stock / `ShopTemplate` were untouched.

---

# 39. Hero-grant-specific implementation sequence

1. Confirm `Hero.Current`.
2. Confirm hero and `HeroItems` are not discarded.
3. Ensure custom template registration.
4. Resolve custom template.
5. Count existing items matching custom template GUID.
6. Apply lane-specific quantity/duplicate rule.
7. Create required quantity through `World`.
8. Call `HeroItems.Add(...)`.
9. Require non-null/valid returned owner result where applicable.
10. Confirm inventory contains the custom GUID.
11. Confirm tooltip/name/description/use behavior required by the claim.
12. Keep save status separate.

---

# 40. Verification matrix

| Stage | Minimum verification | Evidence class needed for a runtime claim | Does not prove |
|---|---|---|---|
| Source selection | exact source GUID resolves | static/runtime lookup | custom identity |
| Identity | custom GUID/name distinct | source/static | collision-free ecosystem |
| Clone | required component/profile shape | source/static | registration |
| Readiness | provider loaded | runtime if timing claimed | later save order |
| Registration | native insertion + provider round trip | runtime for working claim | acquisition |
| Item creation | `World` owns `Item` | runtime | UI |
| Hero acquisition | `HeroItems` contains item | runtime | merchant |
| Merchant acquisition | decompressed stock contains item | runtime | correct UI snapshot |
| Shop UI | item visible/no exception | runtime | persistence |
| Asset override | asset loads/binds | runtime | registration |
| Save persistence | cold save/load passes | controlled validation | missing-mod safety |
| Missing-mod | bounded negative control | controlled validation | migration |
| Migration | version-to-version matrix | controlled validation | all future versions |

---

# 41. Evidence status vocabulary for this process

## RUNTIME_EVIDENCED

The bounded custom merchant item mechanism has live evidence.

The public documentation may state that this mechanism was observed in the maintainer's working environment.

## SOURCE_CONFIRMED / STATIC_CONFIRMED

Multiple consumers and current-build decompilation establish:

- registry architecture;
- lookup requirements;
- Item ownership shape;
- save GUID architecture;
- UI snapshot failure mechanism;
- direct registration implementation shape.

## NOT_PROVEN

Still not proven as a universal public guarantee:

- arbitrary from-scratch `ItemTemplate` construction;
- every `ItemTemplate` family;
- universal cross-build private API stability;
- IL2CPP equivalence;
- durable missing-mod behavior;
- universal uninstall;
- migration;
- all loot/reward/container/world-pickup integrations;
- cross-mod collision safety across all consumers.

---

# 42. Public clean-room boundary

The public process documents the mechanism and evidence-derived rules.

It must not publish:

- private project source code merely because that code supplied evidence;
- proprietary game assemblies;
- bulk decompiled game source;
- proprietary extracted assets;
- user machine paths;
- private runtime dumps containing personal information.

Public examples should be independently authored and should identify their own execution status.

A rewritten public example is not automatically runtime-proven just because the underlying private mechanism is runtime-evidenced.

---

# 43. What a reusable shared registrar still needs

The direct consumer-local route established the mechanism.

A shared production registrar additionally needs reviewed behavior for:

- stable registration request contract;
- source profile declaration;
- custom identity ownership;
- pre-insertion validation;
- component/reference profile verification;
- collision handling;
- deterministic idempotency;
- post-insertion fail-stop policy;
- retained clone lifetime;
- diagnostics/receipts;
- startup ordering;
- persistence ordering;
- cross-mod ownership;
- compatibility/version checks;
- hot-disable policy;
- migration policy.

Until those gates are completed, do not describe the shared registrar as universally solved.

---

# 44. Complete first-item checklist

## Definition

- [ ] Exact native source GUID recorded.
- [ ] Exact source template name recorded.
- [ ] Intended item family understood.
- [ ] Exact custom GUID allocated.
- [ ] Exact custom template name allocated.
- [ ] Custom GUID differs from source GUID.
- [ ] Display/localization policy recorded.

## Readiness

- [ ] `TemplatesProvider` exists.
- [ ] `AllLoaded` is true.
- [ ] Native source resolves.
- [ ] Source suitability checks pass.

## Clone

- [ ] Source `GameObject` cloned.
- [ ] Clone contains intended `ItemTemplate`.
- [ ] Native source remains untouched.
- [ ] Required component/profile shape is preserved.
- [ ] Clone lifetime is owned.

## Registration

- [ ] Custom identity collision check passes.
- [ ] Loader insertion surface resolves for this supported build.
- [ ] Registration call succeeds.
- [ ] Custom GUID resolves through provider.
- [ ] Resolved identity matches inserted custom definition.
- [ ] Source GUID still resolves separately.

## Runtime Item

- [ ] `World.Add(new Item(...))` succeeds.
- [ ] Item points to custom template.
- [ ] Quantity is correct.
- [ ] Item is not discarded.

## Acquisition

- [ ] Exact acquisition owner selected.
- [ ] Duplicate behavior defined.
- [ ] Owner accepts item.
- [ ] Owner contains custom identity.
- [ ] Repeated lifecycle event does not create unintended duplicates.

## Presentation

- [ ] Required UI surface sees item.
- [ ] Expected category/tab behavior is observed.
- [ ] Name/description/icon state matches the actual claimed lane.
- [ ] No stale-list or owner mismatch occurs.

## Failure behavior

- [ ] Missing source fails closed.
- [ ] Missing provider/loader fails closed.
- [ ] Collision fails closed.
- [ ] Profile mismatch fails before native insertion.
- [ ] Post-insertion uncertainty is not treated as a rollback success.

## Persistence

- [ ] Session-only or durable intent stated.
- [ ] Cold save/load run if durable persistence is claimed.
- [ ] Missing-mod behavior run if safe disable/uninstall is claimed.
- [ ] Migration run if identity/schema changed.
- [ ] No untested persistence behavior described as safe.

---

# 45. Current proof boundary

## Proven / bounded

- Native `ItemTemplate` prototype selection by exact identity.
- Separate custom GUID/template identity.
- Whole-object native-template cloning precedent.
- Bounded native component/profile preservation checks.
- Template-system readiness requirement.
- Direct native map insertion mechanism in the inspected Mono path.
- Provider round-trip resolution.
- Native `Item` construction through `World`.
- Direct hero inventory acquisition source precedent.
- Decompressed `RestockableStock` merchant acquisition.
- Correct merchant timing before shop item-list snapshot.
- Visible separate custom merchant item in the bounded runtime proof.
- Duplicate prevention patterns at registration/acquisition levels.
- Failure-derived stale UI snapshot diagnosis and corrected lifecycle location.
- Static save architecture showing that custom template GUID availability is persistence-critical.

## Partially proven / implementation-specific

- Multiple item-family descriptors.
- Custom icon overrides.
- Item-specific effect rewrites.
- Consumer-local collision/idempotency handling.
- Shared registrar architecture.

These require exact consumer evidence and must not be generalized without review.

## Not yet generally proven

- Universal durable custom-item save/load safety.
- Missing-mod/orphan behavior.
- Universal uninstall safety.
- Package-version migration.
- Universal cross-mod identity collision recovery.
- Hot unregister.
- Arbitrary from-scratch `ItemTemplate` authoring.
- Every item category/family.
- Universal loot/container/pickable/reward/recipe integration.
- Cross-build private API stability.
- IL2CPP equivalence.

---

# 46. The rule to carry forward

The actual proven item process is not:

~~~text
make prefab
→ add item
~~~

It is:

~~~text
understand native prototype
→ allocate independent identity
→ preserve native definition contract
→ wait for registry readiness
→ validate before mutation
→ register definition
→ prove normal lookup
→ create World-owned runtime Item
→ hand it to exactly one native acquisition owner
→ mutate before downstream snapshots
→ verify the owner and presentation separately
→ treat assets, effects and persistence as separate lanes
→ record every proof boundary
~~~

That is the item reference implementation for later content-domain research. Weapons, armour, creatures, spells and recipes may reuse individual concepts such as identity discipline or native ownership, but they do not inherit the item process automatically.
