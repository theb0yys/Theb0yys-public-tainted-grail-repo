# Mechanics Catalogue: What Exists, What Is Proven, and What Is Not

> **Reference page.** This catalogue summarizes reusable mechanics extracted from the working repository. It is deliberately conservative: a source implementation can be useful evidence without being promoted as a universal public recipe.

## What this system is

The mechanics catalogue answers:

~~~text
What can a mod do?
→ who owns it?
→ which native types/methods are involved?
→ what lifecycle must be true?
→ where has it been attempted?
→ what has actually been validated?
→ what is still forbidden to generalise?
~~~

## Who owns it in FoA

The native owner varies by mechanic: templates, items, inventory, merchant stock, recipes, actors, save services, Addressables, etc.

This page does not change that ownership. It only routes readers to the correct subsystem and evidence boundary.

## Important identities, types, and methods

| Family | Mechanic | Evidence state in working corpus | Public interpretation |
|---|---|---|---|
| Items | Resolve `ItemTemplate` by GUID through `TemplatesProvider` | Source-inspected | Useful lookup pattern; must wait for `AllLoaded` |
| Items | Create `Item` and add to hero inventory | Source-inspected | Native ownership shape exists; save semantics separate |
| Items | Register project-owned custom `ItemTemplate` | Source-inspected in multiple consumers + bounded item runtime proof | Mechanism exists; direct private-map route is patch-sensitive and not universal save authority |
| Hooks | Suppress lockpick durability at `ConsumePickHP(float)` | Source-inspected | Exact target known; cross-build/runtime behavior must be revalidated |
| Inventory | Classify item templates using native helpers/attachments | Source-inspected | Good discovery aid; do not generalise semantics blindly |
| Armour | Native clothes equip/unequip through `BaseClothes` + Kandra stitching | Decompiled static contract | Real owner path identified; custom armour runtime proof not yet established |
| Weapons | Custom longsword template + hero grant | Source-inspected | Item identity/grant shape exists; equipped custom presentation is separate |
| Weapons | Runtime equipped prototype / native presentation rebinding | Source-inspected | Intended route identified; legacy direct-renderer fallback is not the production proof |
| Recipes | Runtime alchemy recipe append | Source-inspected validation prototype | Runtime-only lane; explicitly not persistent proof |
| Recipes | Runtime "known" shim | Source-inspected | UI/access shim is not recipe persistence |
| Merchants | Restock on `Shop.OpenShop` | Source-inspected, owner runtime unverified | Hook exists; not a promoted general restock recipe |
| Merchants | Filter restock categories via private stock internals | Source-inspected, runtime unverified | High patch risk because private fields/backing fields are reflected |
| Spells | Classify spell family from template-name fragments | Source-inspected heuristic | Discovery only; **not native spell/VFX identity truth** |
| Spells/VFX | Overlay project VFX on `VCCharacterMagicVFX.CastingBegun` | Source-inspected | Exact hook exists; semantic spell mapping remains separate |
| Actors | Project-owned companion definition/bootstrap | Source-inspected | Project API example, not proof of native identity |
| Actors | One-session companion death/dismiss/cleanup | Source-inspected | Useful lifecycle design input; generic persistent actor authority not implied |
| Assets | Wrap runtime object in completed Addressables handle | Source-inspected | Concrete bridge for one use case; not arbitrary Addressables substitution authority |
| Save | Observe completed slot write via concrete `EndSave(string)` | Source-inspected + decompiled target research | Real save-completion observation seam; not custom serialization |
| Save | Persistence boundary between runtime mutation and durable state | Multiple evidence lanes | Treat item, recipe and actor persistence separately; generic save extension remains unresolved |

## Where it exists in the lifecycle

Each mechanic has its own prerequisite.

Common examples:

- template mechanics require template readiness;
- hero grants require a valid non-discarded hero/inventory;
- merchant mechanics require the correct stock/UI phase;
- one-session actor mechanics require resolved template + runtime owner and explicit not-saved/cleanup posture;
- save observation happens after native write completion, not during arbitrary gameplay;
- Addressables bridges require the underlying runtime asset to exist and have a clear release owner.

## How we interact with it

Use catalogue entries to decide **where to research**, not to skip research.

Before using a mechanic:

1. open the owning handbook/domain page;
2. inspect the exact version/build boundary;
3. confirm the lifecycle precondition;
4. distinguish source evidence from runtime validation;
5. check known failures and persistence impact;
6. implement only the smallest validated lane.

## Why this route

The working mechanics library intentionally marks many rows as intake because repository source can demonstrate a path without proving:

- current-build compatibility;
- live runtime success;
- save safety;
- cross-mod safety;
- generalisation beyond the original use case.

That is the correct distinction for a public handbook.

## What goes wrong

The most common misuse is to read:

> "there is source code that calls this"

as:

> "this is the universal supported way to do it."

Those are different claims.

Another frequent failure is promoting a **partial path**:

- recipe visible → assumed persistent;
- asset loaded → assumed registered;
- actor spawned → assumed population-safe;
- shop hook exists → assumed duplication/restock-safe;
- save callback exists → assumed custom serialization possible.

## How to verify

For a mechanic to move from catalogue evidence to a reusable process, require:

- exact owner;
- exact identity;
- exact lifecycle;
- complete call path;
- cleanup;
- failure behavior;
- version scope;
- runtime evidence appropriate to the claim;
- persistence evidence if durable;
- explicit boundary.

## Current proof boundary

This public catalogue is a human-readable view of the working repository's mechanics extraction. It intentionally does not expose private code or promote intake-only mechanics into unsupported recipes.
