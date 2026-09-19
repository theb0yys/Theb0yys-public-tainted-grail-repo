# 35 — Main/Off-Hand Observer

**Category:** equipment / hand-state observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example watches the current hero's native hand-item projections:

~~~text
Hero.MainHandItem
Hero.OffHandItem
~~~

It samples at a bounded interval and logs only when either item reference changes.

For each hand it reports a small read-only classification based on native item/template properties:

- weapon;
- melee;
- shield;
- ranged;
- magic;
- currently equipped.

The observer does not enumerate the entire inventory and does not infer that a hand-item change is equivalent to a completed animation or renderer transition.

## Build

~~~powershell
dotnet build .\HandItemObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This example does not equip, unequip, swap, move or modify items.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
