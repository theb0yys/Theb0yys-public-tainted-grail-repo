# What Works, What Does Not, and Why

> **Reference page.** This page connects successful routes and rejected or failed approaches to the rule each taught us. Scope and evidence differ by system, so every entry states what the result actually proves.

## What this system is

A useful FoA modding manual must preserve three states:

- **works in the bounded evidence**;
- **possible/source-confirmed but not runtime-proven**;
- **does not work / rejected for this purpose**, with the reason.

The point is not to collect failures. The point is to preserve the corrected model.

## Who owns it in FoA

A path works only when it respects the native owner involved: template system, MVC World, inventory, merchant stock, equipment/presentation, actor/location lifecycle, save system, UI, assets, or another domain owner.

## Important identities, types, and methods

### Custom item identity and merchant acquisition

**Works — bounded runtime proof:**

~~~text
reviewed native ItemTemplate
→ separate custom GUID
→ runtime clone
→ native template-map registration
→ TemplatesProvider re-resolution
→ World-owned Item
→ decompressed RestockableStock
→ insertion before ShopUI item-list snapshot
→ visible separate custom item
~~~

**Why it works:** every stage hands the result to the native owner expected by the next stage.

**Does not substitute for this path:**

- creating an `ItemTemplate`-shaped prefab in Merlin and assuming FoA registered new gameplay content;
- loading an icon/model bundle and assuming that created an item;
- changing only visible text and treating it as a new machine identity.

---

### Broad item batches before individual proof

**Observed problem:** widening the custom-item experiment to many descriptors made merchant/list/descriptor failures harder to isolate.

**Correction:** return to the single runtime-visible Apple route, prove each new descriptor independently, then broaden.

**Why:** one failure in a batch can hide whether the problem is identity, template shape, category, lifecycle, UI, or the descriptor itself.

**Golden rule:** **prove one, then generalize**.

---

### Merchant mutation at the wrong UI time

**Observed problem:** correct-looking stock changes can still be absent from the visible merchant list.

**Why:** the shop/UI lifecycle can snapshot the item list after stock decompression. Mutation after the relevant capture point leaves presentation stale.

**Correction:** the proven custom-item route performs insertion before the original `ShopUI.OnFullyInitialized` logic captures its list.

**Golden rule:** lifecycle timing is part of the mechanism.

---

### Existing item grant

**Source-backed shape:**

~~~text
TemplatesProvider resolves ItemTemplate
→ World.Add(new Item(...))
→ HeroItems.Add(...)
~~~

**What this proves:** a native ownership path for an already resolved item definition.

**What it does not prove:** custom template registration, custom identity persistence, or save safety.

---

### Runtime recipe append

**Source-backed:** create a runtime recipe using resolved existing item references and append it to the appropriate runtime recipe collection.

**Intentionally not claimed:** persistent learning/save.

**Why:** the proof uses a runtime-only known-recipe shim rather than writing durable player recipe state.

**Golden rule:** appearing in a runtime collection is not equivalent to durable progression.

---

### Persistent learning of an existing recipe

**Source-backed candidate:** resolve an existing recipe and call `HeroRecipes.LearnRecipe(IRecipe)` after already-known checks and mutation gates.

**Still required:** disposable save/reload validation before calling the path generally save-safe.

**Why:** the correct native mutation method and persistence validation are two different claims.

---

### Spell family classification by name

**Works for:** exploratory grouping and deciding what to investigate next.

**Does not work for:** exact native spell identity, effect ownership, or VFX ownership.

**Why:** template-name fragments are heuristics, not a native relationship.

**Golden rule:** a heuristic stays labelled as a heuristic.

---

### Spell VFX overlay

**Source-backed:** observe `VCCharacterMagicVFX.CastingBegun` and attach a temporary mod-owned VFX prefab.

**Does not prove:** native spell VFX replacement, native effect identity, or ownership of the underlying spell semantics.

**Why:** adding presentation on top of a native event is not the same as replacing the native presentation/effect pipeline.

---

### Direct custom weapon visual shortcuts

**Rejected as a general production route:** a direct Unity renderer/hand-socket fallback that bypasses the real native equipment/Drake presentation owner.

**Why:** something can be visible while failing native equip lifecycle, FPP/TPP presentation, previews, renderer ownership, cleanup, and Drake resource semantics.

**Research direction:** template identity plus native/Drake presentation rebinding.

**Golden rule:** visible is not integrated.

---

### Native armour presentation

**Established statically:** `BaseClothes` loads a clothing GameObject, stitches through `ClothStitcher`, redirects Kandra presentation, and tears the object/resources down on unequip.

**Does not yet prove:** arbitrary custom armour works at runtime.

**Why this matters:** decompilation can identify the correct owner/lifecycle without proving a custom implementation.

---

### One-session custom actors

**Reusable source-backed shape:**

~~~text
resolve exact LocationTemplate
→ native Location spawn/owner
→ validate expected actor identity/components
→ MarkedNotSaved
→ native combat/death/corpse lifecycle
→ explicit dismiss/failure cleanup
→ Location.Discard
~~~

**Why it is safer for proof work:** persistence is explicitly excluded.

**Does not prove:** persistent population integration, generic spawn safety, or save-owned actors.

---

### Creature visual transport

**Can work:** custom visual roots/assets can build, load, render and release.

**Does not prove:** a valid FoA creature.

A creature still requires its own template/actor/AI/combat/death/spawn/cleanup ownership chain.

**Golden rule:** visual transport is one gate, not the actor.

---

### Save completion observation

**Works for:** observing completion of native slot writes through researched concrete `EndSave(string)` methods.

**Does not provide:** a general custom serialization domain.

**Why:** observing native save completion is not the same native surface as registering new serialized domains.

---

### Generic custom native save domain

**Current research outcome:** no generally promoted mutable native save-domain registration route.

**Implication:** do not invent a generic custom serializer by extrapolating from unrelated save hooks.

**Golden rule:** a missing extension point is a real blocker.

---

### Private reflection

**Can work:** private loader maps and merchant internals appear in researched paths.

**Risk:** those members are patch-sensitive.

**Why:** private implementation layout can change between game versions.

**Required posture:** exact member/version evidence, fail-closed behavior, and revalidation after updates.

---

### Unity hierarchy as lifecycle ownership

**Does not substitute for FoA ownership.**

Parenting a GameObject does not automatically reproduce:

- MVC Model/Element lifetime;
- Domain ownership;
- event-owner cleanup;
- save membership;
- equipment lifecycle.

**Why:** Unity transform hierarchy and FoA runtime ownership are different systems.

---

### Broad polling

**Useful for:** bounded diagnostics, startup discovery, one-time inventories.

**Bad default for:** hot-path production ownership when FoA exposes lifecycle/events.

**Why:** broad scans add overhead and can observe objects without understanding owner transitions.

---

### Build success

**Works for:** proving source compiles against the selected references.

**Does not prove:** loader success, hook invocation, game behavior, visual result, persistence, or release compatibility.

**Golden rule:** evidence level must match the claim.

## Where it exists in the lifecycle

Most failures reduce to one of these questions:

~~~text
wrong identity?
wrong native owner?
wrong lifecycle point?
wrong evidence level?
wrong persistence assumption?
~~~

Use those five questions before adding more code.

## How we interact with it

When a new attempt fails:

1. preserve the attempted route;
2. identify the assumption it tested;
3. record the first meaningful failure;
4. update the model;
5. change only the variable relevant to that model;
6. attach the lesson to the appropriate system/process page.

## Why this route

The research corpus is valuable precisely because it contains both successes and corrections. Removing the failed paths would remove the explanation for the final design.

## What goes wrong

Bad documentation:

- says only "doesn't work";
- silently replaces an old diagnosis;
- calls static reasoning a runtime result;
- publishes a workaround without identifying the native owner it bypasses.

Good documentation preserves **attempt → evidence → correction → rule**.

## How to verify

Every works/fails entry should include:

- exact scope;
- evidence level;
- relevant build/version where patch-sensitive;
- native owner;
- observed result;
- reason or current best-supported explanation;
- what remains unproven.

## Current proof boundary

This page contains reusable conclusions from inspected research and implementations. It will expand as the broader research corpus is distilled into the public handbook.
