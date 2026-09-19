# Saving and Persistence

> **Reference page.** Use this before claiming that custom content survives save/load, uninstall, downgrade, missing dependencies, or game restart.

## What this system is

Persistence is a separate capability from runtime success.

A custom item can:

- register successfully;
- appear in inventory or a shop;
- work during the current session;

and still be unsafe or unresolved when a save is loaded later.

## Who owns it in FoA

Static native-contract research for items establishes:

- `Item.Serialize` writes its template relationship and quantity;
- template serialization writes the template GUID;
- `Item.Deserialize` restores an `ItemTemplate`;
- template restore resolves the saved GUID through the native template lookup path.

FoA save-slot lifecycle is owned by native save services. A completed native save observation is not the same as a custom serialization extension point.

## Important identities, types, and methods

Relevant researched surfaces include:

- `SaveWriter.WriteTemplate<T>`
- `SaveReader.ReadTemplate<T>`
- `TemplatesUtil.Load<T>`
- `Item.Serialize`
- `Item.Deserialize`
- `TemplatesLoader.FinishedLoading`
- concrete cloud-service `EndSave(string)` methods as completed-save observations

For disposable/session-only world objects, some proven paths explicitly use `Location.MarkedNotSaved = true`.

## Where it exists in the lifecycle

For a saved custom item, the critical dependency is:

~~~text
game starts
→ native templates load
→ custom template must become resolvable
→ save restores item GUID
→ provider resolves custom template
→ item restoration can continue
~~~

If the custom definition is unavailable when restore needs it, the runtime-visible success from the previous session does not guarantee a safe load.

## How we interact with it

Treat persistence as an explicit design choice.

For durable custom content, define:

- stable identity;
- registration timing;
- save owner;
- missing-mod behavior;
- duplicate/replay behavior;
- migration across versions;
- uninstall/orphan behavior;
- rollback/non-reversibility.

For proofs that do not need persistence, prefer an explicit session-only/disposable boundary rather than accidentally creating save-owned state.

## Why this route

Your working-repo research found enough of the native item serialization contract to explain **why early registration matters**, but not enough live evidence to claim universal save-safe custom item registration.

That distinction prevents a current-session success from becoming an unsupported persistence claim.

## What goes wrong

Known risks:

- a save contains a custom template GUID but the mod is missing;
- registration occurs after item restoration tried to resolve that GUID;
- the custom GUID changes between versions;
- two mods claim the same GUID;
- a runtime-only recipe/item is mistaken for a persistent one;
- session-only actors are allowed into save-owned state accidentally;
- uninstall/downgrade behavior is never tested.

## How to verify

For a durable claim, test separately:

1. create/use the custom content;
2. save on a disposable slot;
3. exit/restart;
4. load with the same mod/version;
5. verify identity and behavior;
6. test duplicate/replay behavior;
7. test the documented missing/disabled-mod case;
8. test migration when identity/schema changes;
9. record exact game/mod versions and hashes.

## Current proof boundary

Custom item GUID serialization/lookup is supported by static native-contract research. The general public custom-item path in this repository must **not** claim cold-save, missing-mod or uninstall safety until those exact tests are recorded.

Native save-domain injection is not presented here as a proven generic modding route.
