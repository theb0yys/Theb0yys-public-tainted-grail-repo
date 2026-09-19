# Working, Partial, and Rejected Modding Patterns

> **Reference page.** This page summarizes reusable outcomes from the working research. The status applies only to the stated scope.

## What this system is

A technique can be:

- **working / bounded** — enough evidence exists for a specific use;
- **partial / source-static** — architecture or code exists, but required runtime/persistence proof is incomplete;
- **rejected** — research or failed attempts show the route violates ownership, lifecycle, or compatibility assumptions;
- **blocked** — potentially valid, but a required prerequisite or validation gate is still missing.

Do not collapse those states into "works" or "doesn't work."

## Who owns it in FoA

The status is always attached to the native system and use case.

A route can be valid for one purpose and invalid for another.

Example: an AssetBundle is valid for delivering a mod-owned prefab, but AssetBundle load success is not item registration.

## Important identities, types, and methods

The patterns below reference the same owner surfaces used throughout the handbook: templates, World/MVC, HeroItems, Stock, Location, Addressables, scene services, save services, Harmony and proprietary renderer systems.

## Where it exists in the lifecycle

The general evaluation sequence is:

~~~text
source/static understanding
→ build
→ load
→ controlled runtime proof
→ lifecycle/cleanup proof
→ persistence proof if applicable
→ promotion to reusable process
~~~

## How we interact with it

## Working / bounded patterns

### Exact template lookup after readiness

**Works for:** resolving loaded native/custom templates after `TemplatesProvider.AllLoaded`.

**Why:** provider explicitly enforces loading state and exact GUID/type lookup.

**Do not generalise to:** registration or persistence.

---

### Native Model/World ownership

**Works for:** entering FoA logical runtime ownership through `World.Add`, Model/Element lifecycle, native inventory/stock/location owners.

**Why:** the native owner supplies initialization, events, parent lifetime and cleanup semantics.

**Wrong alternative:** disconnected Unity object used as a substitute for a native gameplay object.

---

### Native-derived custom item clone + runtime registration

**Works for:** bounded Mono/BepInEx 5 custom-item identity proof.

**Shape:**

~~~text
resolve native ItemTemplate
→ clone GameObject
→ new custom GUID/name
→ validate source/clone
→ AddToMap
→ resolve custom GUID
→ World.Add(Item)
→ native acquisition owner
~~~

**Why:** it preserves a known-good native item contract while introducing a separate identity.

**Limit:** private loader map is patch-sensitive; general cold-save/uninstall safety remains unproven.

---

### Merchant insertion before UI snapshot

**Works for:** bounded custom-item merchant visibility.

**Shape:**

~~~text
Shop.OpenShop
→ stock decompressed
→ ShopUI.OnFullyInitialized Prefix
→ add Item
→ original UI captures item list
~~~

**Why:** downstream UI must see the mutation before it captures the list.

**Known correction:** later/incorrect list assumptions produced stale or broken merchant presentation.

---

### Normal item grant path

**Works for:** creating an `Item` from a resolved template and adding it through the hero inventory owner.

~~~text
World.Add(new Item(template, quantity))
→ HeroItems.Add(item)
~~~

**Limit:** source path alone is not universal save-safety proof for custom template GUIDs.

---

### Native event / lifecycle observation

**Works for:** observing Model lifecycle, save completion, crafting/recipe events, exact gameplay hooks when the target is known.

**Why:** event/lifecycle boundaries provide stronger state guarantees than broad polling.

---

### Session-only actor/location lifecycle

**Works for:** controlled one-session actor/companion proofs that explicitly use native `Location` ownership, identity validation, `MarkedNotSaved`, and `Location.Discard()`.

**Why:** persistence is deliberately excluded until separately proved.

**Limit:** not generic population/spawner/save authority.

---

### Mod-owned AssetBundle loading

**Works for:** delivering mod-owned visual/audio/UI assets when built for the correct Unity target and loaded through the mod's asset route.

**Why:** asset transport is an independent capability.

**Limit:** does not create a gameplay definition or native owner.

---

### ModService-compatible Addressables transport

**Works for:** bounded asset catalogue/build/load/release proof where the exact ModService-compatible layout was validated.

**Limit:** asset transport does not prove actor/item/spell registration.

---

### Exact Harmony Prefix/Postfix

**Works for:** narrow behavior interception/observation when exact type, method, overload, lifecycle and downstream effect are understood.

**Why:** Prefix/Postfix can preserve the original native path while changing one bounded behavior.

**Limit:** private/internal targets are version-sensitive.

## Partial / source-static patterns

### Shared collision-safe native item registrar

**Status:** architecture/source implementation exists in later framework work.

**Why not yet public baseline:** full runtime/save/missing-registrar gate remains incomplete.

**Use now:** design direction and requirements, not blanket runtime guarantee.

---

### Persistent existing-recipe learning

**Status:** native `HeroRecipes.LearnRecipe(IRecipe)` path and serialization contract are source-inspected.

**Missing:** final live disposable-save/reload proof for the generalized claim.

---

### Custom external scene through native SceneService

**Status:** coherent source/static path exists for mod-catalogue scene discovery and native additive-scene lifecycle.

**Missing:** locked empirical custom-scene gate remains NOT_RUN.

---

### Sidecar persistence

**Status:** native save lifecycle and candidate capture/success/load/apply stages have been narrowed.

**Missing:** controlled runtime/crash proof required before a standard sidecar implementation is called proven.

---

### Weapon Drake prototype integration

**Status:** substantial framework/source/runtime research exists.

**Reason for caution:** weapon presentation includes native item/equip ownership, Drake resources/entities, FPP/TPP/preview lifecycles and cleanup. Individual visual corrections do not automatically equal a generic weapon importer.

---

### Custom armour/Kandra integration

**Status:** native clothes/Kandra stitching/equip teardown has strong static/decompile evidence.

**Missing:** general custom-armour runtime deformation/equip/persistence proof.

## Rejected / unsafe patterns

### "Merlin created a template, therefore the game has new content"

**Rejected because:** official replacement/tooling structure does not establish arbitrary new runtime registration.

Use Merlin for supported replacement work and first-party information.

---

### Reusing a native GUID for new content

**Rejected because:** it creates identity collision/replacement semantics rather than a separate mod-owned definition.

---

### Display-name or filename heuristic as native identity

**Rejected because:** names can collide, change, localize or describe relationships incorrectly.

Use exact native references and preserve version scope.

---

### Direct Unity renderer fallback as a generic equipped-weapon solution

**Rejected as framework/production default because:** it bypasses native equipment/presentation ownership and Drake lifecycle.

A visible mesh is not equivalent to a native equipped-weapon representation.

---

### Synthetic Drake ECS entity creation without full native owner graph

**Rejected because:** renderer ownership also involves resources, loading, scene lifetime, linked-object bookkeeping, refcounts and cleanup.

---

### Broad generic Addressables hooks

**Rejected where they intercept unrelated global asset loads.**

**Why:** global interception can break unrelated UI/game assets and obscures ownership.

Use registered/owner-scoped asset keys and narrow integration points.

---

### Raw `Destroy(view.gameObject)` as a substitute for `View.Discard()`

**Rejected because:** it can bypass World association, events, View hooks and registered asset cleanup.

---

### Runtime recipe append treated as persistent recipe registration

**Rejected because:** the proof route deliberately avoids persistent learning/save semantics.

Runtime visibility is not persistence.

---

### Native custom save-domain injection as a generic mod save API

**Blocked/rejected for the current researched binary because:** the native save-domain set is fixed and no mutable domain registrar was recovered.

Do not patch SaveWriter/SaveReader globally to invent one.

---

### Broad per-frame reflection/scanning

**Rejected as default architecture because:** expensive, noisy and usually a sign that lifecycle/event research was skipped.

Use cached, event-driven or throttled boundaries.

---

### Patching a semantically similar method without full-path equivalence

**Rejected because:** the method may belong to the wrong stage/owner.

Examples include confusing door animation with scene travel, or stock mutation with content registration.

## Why this route

The strongest reusable knowledge in the research is not a list of clever patches.

It is knowing which shortcuts failed because they skipped:

- identity;
- ownership;
- lifecycle;
- cleanup;
- or proof boundaries.

## What goes wrong

A rejected route often appears to work partially:

- a mesh becomes visible;
- an object exists;
- a recipe appears;
- a log line fires.

That partial result can hide the missing native lifecycle until save/load, scene travel, equip/unequip, UI refresh or cleanup exposes it.

## How to verify

When evaluating a technique, record:

- exact intended capability;
- native owner;
- evidence state;
- runtime result;
- cleanup result;
- persistence result if needed;
- failure/negative test;
- version/build scope.

Then assign **working**, **partial**, **rejected**, or **blocked** for that exact capability.

## Current proof boundary

This page summarizes the current reusable patterns extracted from the working research. It should be revised as stronger runtime evidence promotes or rejects individual lanes.
