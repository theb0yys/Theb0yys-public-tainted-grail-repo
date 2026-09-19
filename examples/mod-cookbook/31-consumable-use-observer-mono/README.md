# 31 — Consumable Use Observer

**Category:** items / consumable observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example observes the real hero item-use boundary:

~~~text
Awaken.TG.Main.Heroes.Items.Item.Use()
~~~

It filters to items whose owning character is the current hero, then keeps only consumable-like templates.

The log reports native item/template context such as:

- consumable;
- potion;
- plain food;
- dish;
- fish;
- alcohol;
- other potion;
- health-, mana- or stamina-related template flags.

It does **not** treat the call as proof that an effect happened. `Item.Use()` begins the native use route; effect and consumption attribution require later evidence.

That distinction is why examples 32 and 33 measure actual state deltas instead of inferring healing or status changes from the item name.

## Build

~~~powershell
dotnet build .\ConsumableUseObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The observer does not block use, change quantity, edit templates, alter effects or write saves.

Separate maintainer evidence has live diagnostic coverage for representative food, dish, potion and alcohol paths through `Item.PerformImmediate(ItemActionType)`. This rewritten example intentionally remains on the requested `Item.Use` attribution boundary and has not itself been run.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
