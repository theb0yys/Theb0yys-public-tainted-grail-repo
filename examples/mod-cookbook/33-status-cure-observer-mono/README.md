# 33 — Status Cure/Removal Observer

**Category:** statuses / consumable delta observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example snapshots:

~~~text
Hero.Statuses.AllStatuses
~~~

before and after a hero-owned consumable-like `Item.Use()` call.

It reports two deliberately narrow outcomes:

- negative status instances that were present before use and absent afterwards;
- positive status instances that were absent before use and present afterwards.

That provides an action-level view of an immediate cure/removal or buff gain without editing the native status collection.

## Identity rule

The comparison uses runtime `Status` object identity.

That is intentional. It avoids pretending that one status type or template always maps to one active instance, stack or renewal lifecycle.

A status that changes internally while retaining the same runtime instance may not appear as an add/remove delta here. Example 28 remains the better surface for native `CharacterStatuses.AddStatus(...)` application outcomes.

## Attribution boundary

This example observes only synchronous deltas visible when `Item.Use()` returns. Delayed removals, delayed buffs, periodic effects or later scripted changes require a different correlation window.

## Build

~~~powershell
dotnet build .\StatusCureObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example does not cleanse, add, prolong, replace or stack statuses.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
