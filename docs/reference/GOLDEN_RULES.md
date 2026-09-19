# Golden Rules of Tainted Grail Modding

> **Reference page.** These are the cross-cutting rules repeatedly established by successful paths, failed attempts, native lifecycle research, and compatibility work in the maintainer's working repository.

## What this system is

These rules are not a substitute for domain-specific research.

They are the invariants that should shape **how you investigate and implement** a FoA mod before you know every domain-specific detail.

## Who owns it in FoA

Each rule exists because FoA has real ownership, identity, lifecycle, serialization, rendering, UI, and save boundaries.

The game—not the mod—owns those boundaries. A good mod integrates at the correct boundary instead of simulating or bypassing it without evidence.

## Important identities, types, and methods

The recurring native surfaces behind these rules include:

- `World`, `Model`, `Element<TParent>`, `View`;
- `TemplatesLoader`, `TemplatesProvider`;
- exact template GUIDs and `TemplateReference`;
- `Hero.Current`, `HeroItems`;
- `Shop`, `RestockableStock`, `ShopUI`;
- `Location`, `LocationTemplate`;
- Harmony Prefix/Postfix targets;
- Addressables / `ARAssetReference`;
- native save/template restoration;
- scene services and domain teardown.

## Where it exists in the lifecycle

These rules apply from research through runtime:

~~~text
identify
→ inspect owner
→ map lifecycle
→ choose the smallest intervention
→ validate one stage
→ hand off to the native owner
→ verify downstream behavior
→ clean up
→ validate persistence separately
~~~

## How we interact with it

### Rule 1 — Copy the full proven path, not one method

A proven path is:

~~~text
entry point
→ owner API
→ lifecycle
→ dispatch/mutation
→ cleanup/restoration
→ validation
~~~

A matching method name, one Harmony target, one visible panel, or one log line is partial evidence only.

### Rule 2 — Use the actual native owner

If FoA already owns the lifecycle, use that owner.

Examples:

- registered item definition → native template system;
- runtime item → `World`;
- hero inventory → `HeroItems`;
- merchant item → `RestockableStock`;
- actor/location → `Location`;
- Model/View lifecycle → MVC APIs;
- equipment visuals → the proven native equip/render owner.

A disconnected Unity object is not automatically a game object in the FoA sense.

### Rule 3 — Exact identity beats display text

Keep these separate:

- native GUID;
- custom GUID;
- template name;
- display/localized name;
- Unity asset GUID;
- Addressables address;
- plug-in GUID;
- project-owned record ID.

Do not identify a native subject by display name alone when a stronger identity exists.

### Rule 4 — Never reuse the native identity for new content

New content gets a mod-owned identity.

A native prototype can be used as a structural baseline, but the new record must not silently claim the prototype's identity.

### Rule 5 — Preserve exact native references

Do not "clean up" a native GUID, key, object path or address.

Do not:

- change case;
- normalize separators;
- translate;
- strip prefixes;
- merge two refs because their names look alike.

Exact references are version-scoped evidence.

### Rule 6 — Lifecycle timing is part of the mechanism

The right action at the wrong time is still wrong.

Examples:

- template lookup before `TemplatesProvider.AllLoaded`;
- custom registration before the template loader is ready;
- stock mutation after the UI captured its list;
- inventory mutation before `Hero.Current` / `HeroItems` is valid.

Research the state immediately before and after a hook.

### Rule 7 — Asset loading is not gameplay registration

A model, prefab, icon, material or bundle can load successfully while the game still knows nothing about a new item, weapon, actor, spell or recipe.

Prove separately:

~~~text
asset transport
→ gameplay definition
→ native registration/resolution
→ runtime owner
→ presentation
~~~

### Rule 8 — Registration is not acquisition

A custom item can be registered without being:

- in inventory;
- sold by a vendor;
- in loot;
- craftable;
- rewarded;
- placed in the world.

Each acquisition route is a separate capability.

### Rule 9 — Runtime visibility is not persistence proof

Seeing the feature work in the current session does not prove:

- save/load;
- restart;
- missing-mod behavior;
- uninstall;
- downgrade;
- migration;
- orphan handling.

Persistence gets its own test.

### Rule 10 — Passing one lane does not authorize another

Examples:

- existing item grant ≠ custom item registration;
- custom item registration ≠ vendor injection;
- vendor injection ≠ recipe integration;
- visual transport ≠ actor construction;
- spawn proof ≠ population ownership;
- source inspection ≠ runtime execution.

Do not promote evidence sideways.

### Rule 11 — Start from one controlled proof

Prove one identity, one owner, one hook, one item, one actor, one route.

Start with one established mechanism before expanding to a batch.

The custom-item history showed why: batch expansion hid which descriptor or lifecycle assumption actually failed.

### Rule 12 — One changed variable makes failures useful

If you simultaneously change:

- identity;
- hook;
- assets;
- values;
- behavior;
- acquisition;
- persistence;

a failure tells you very little.

Keep experiments narrow enough that failure changes the model.

### Rule 13 — A failure must teach something

Record:

- attempted path;
- expected invariant;
- observed failure;
- corrected explanation;
- next changed experiment;
- resulting rule.

Do not preserve "didn't work" without a lesson.

### Rule 14 — Do not guess relationships from names

Name fragments can be useful for discovery, but they are not proof of:

- spell family;
- template inheritance;
- ownership;
- actor class;
- native VFX mapping;
- save identity;
- scene relationship.

Use exact relationships where available.

### Rule 15 — Private/reflected APIs are patch-sensitive

Private surfaces such as loader maps and reflected stock fields can work, but they carry compatibility risk.

When using one:

- record exact type/member;
- bind it to a game/assembly version;
- fail closed if it disappears;
- revalidate after updates;
- prefer a reviewed shared owner when one becomes available.

### Rule 16 — Native cleanup matters as much as creation

Creation without teardown is incomplete.

Examples:

- use `View.Discard()` rather than assuming `Destroy(view.gameObject)` reproduces MVC cleanup;
- remove event listeners through their owner lifecycle;
- discard session-only Locations;
- release asset handles/resources;
- unpatch Harmony where supported.

### Rule 17 — Unity hierarchy is not FoA ownership

Parenting a GameObject does not reproduce:

- Domain membership;
- Model registry membership;
- Element parent lifetime;
- event-owner cleanup;
- save ownership.

Use the FoA owner when those semantics matter.

### Rule 18 — Event ownership must be explicit

Use a stable listener owner where practical.

Ownerless/modal listeners require explicit cleanup and are easier to leak across lifecycle transitions.

### Rule 19 — Prefer event/lifecycle hooks over broad polling

Broad `World.All<T>()` scans are useful for bounded discovery/startup, not as the default hot-path architecture.

Prefer:

- creation events;
- owner events;
- one-time registration;
- cached references;
- throttled diagnostics.

### Rule 20 — Build/load/runtime/save are separate claims

Keep the claim ladder explicit:

~~~text
source inspected
≠ build passed
≠ plug-in loaded
≠ hook fired
≠ behavior observed
≠ persistence validated
≠ release proven
~~~

### Rule 21 — Version scope is part of every patch-sensitive fact

A type/method/GUID/field can be correct for one build and stale later.

Record:

- game build/version;
- Mono or IL2CPP;
- loader version;
- relevant assembly/hash when practical;
- date/source.

### Rule 22 — Mono and IL2CPP are separate supported lanes

Do not call Mono "legacy."

Do not assume:

- assembly shape;
- generated types;
- loader API;
- hook availability;
- private member layout;

is identical between the lanes.

### Rule 23 — Merlin Workshop is evidence and replacement tooling, not universal injection

Merlin is valuable for:

- official structures;
- GUIDs;
- addresses;
- source relationships;
- supported Addressables replacement work.

It does not by itself solve arbitrary new-content registration.

### Rule 24 — Fail closed when a prerequisite is missing

If the expected:

- type;
- method;
- GUID;
- owner;
- component;
- asset;
- lifecycle state;

is missing, stop that operation.

Do not "find something similar" and mutate it automatically.

### Rule 25 — Stronger architecture can come later

A shared framework should emerge from multiple proven consumers, not from preference.

First prove the path. Then identify duplicated ownership/collision/lifecycle problems. Then promote a shared owner with its own validation.

## Why this route

Every rule above comes from a recurring pattern in the working evidence: the failed approaches generally crossed an ownership, identity, lifecycle, or proof boundary too early.

The successful approaches narrowed the operation until each boundary was explicit.

## What goes wrong

Most hard-to-debug FoA mods violate more than one rule at once:

- name-based target;
- wrong lifecycle;
- direct Unity object;
- broad Harmony patch;
- no cleanup;
- no exact build scope;
- and a save claim based on one runtime screenshot.

That creates apparent randomness where the actual problem is uncontrolled assumptions.

## How to verify

Before calling a path reusable, answer:

- What exact native owner is responsible?
- What exact identity is used?
- When is that owner ready?
- What hook/API enters the path?
- What downstream owner consumes the result?
- What failed alternatives shaped the rule?
- What cleanup is required?
- What proof actually ran?
- What remains unproven?

## Current proof boundary

These are handbook-level rules derived from repository research and implementations. A golden rule does not replace domain-specific evidence or grant permission to mutate an unproven system.
