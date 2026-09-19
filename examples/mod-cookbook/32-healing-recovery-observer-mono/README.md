# 32 — Healing and Recovery Observer

**Category:** healing / item-use delta observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example answers a narrower question than "is this a healing potion?"

It snapshots the current hero's native health value before and after a hero-owned consumable-like `Item.Use()` call:

~~~text
Hero.Health.ModifiedValue
~~~

The resulting row reports:

- health before;
- health after;
- positive health delta;
- whether actual synchronous healing was observed;
- item quantity before and after.

A health-related template flag or item name is **not** treated as healing proof. `actualHealing=true` is emitted only when the measured health value increases by more than a small epsilon during the observed call.

## Important boundary

This is synchronous attribution around `Item.Use()`. A delayed, animation-driven, periodic or otherwise later health effect can occur outside this window and will not be credited by this example.

Likewise, a quantity decrease is useful context but does not by itself prove healing.

## Build

~~~powershell
dotnet build .\HealingRecoveryObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The observer never changes health, item quantity, effects or saves.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
