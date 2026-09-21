# Weapon Importer Pipeline

Tainted Weapons owns the custom-weapon package, native item-template, and Drake presentation path.

There are two important truths to keep separate:

1. the framework already implements substantial package, registration, and presentation machinery;
2. the fully consolidated end-to-end importer process is still a reviewed target process, not a claim that every stage is complete today.

## Current implemented route

The current framework contains these major pieces:

~~~text
weapon package definition
→ canonical identity reservation
→ cooked bundle/prefab resolution
→ explicit native template registration request
→ wait for TemplatesProvider readiness
→ source ItemTemplate resolution
→ bounded clone + collision checks
→ TemplatesLoader.AddToMap
→ provider lookup verification
→ native equip path
→ CharacterHandBase / Drake prototype rebinding
→ registered mesh/material loading bridge
→ post-conversion Drake readiness
→ FPP/TPP runtime evidence
→ inventory/equipment preview evidence
~~~

## 1. Package identity comes first

A weapon definition should reserve one immutable identity covering the package, custom template GUID/name, runtime prototype address, mesh/material keys, and definition hash.

Changed definitions under an already-known identity should fail instead of silently becoming a different weapon.

## 2. Registration is explicit

Registering the presentation definition should not secretly mutate the game template registry.

The native registrar is a separate request:

- wait for the template provider to be ready;
- resolve the exact source template;
- reject GUID/name collisions;
- compare bounded source/custom structure;
- clone only the approved template profile;
- perform one native insertion;
- verify lookup returns the inserted template;
- retain the successful clone for the session.

A denial should perform no native insertion.

## 3. Keep acquisition outside registration

Template registration answers:

> Does this custom weapon identity exist in the native template system?

It does not answer:

> How did the player obtain it?

Vendor stock, grants, loot, recipes, quests, and other acquisition routes remain separate consumers.

## 4. Reuse the native equip lifecycle

The presentation route should follow FoA's existing weapon lifecycle rather than creating an unrelated Unity renderer beside it.

The current framework redirects registered custom identities through a cloned/authorable `CharacterHandBase` path with Drake mesh/material references, then lets native equip and hand lifecycle code own placement, hide/show, and teardown.

If the custom prototype cannot be built safely, fall back to the original native source presentation instead of spawning a broken parallel renderer.

## 5. Prove Drake loading without breaking unrelated Addressables

Serve only registered framework mesh/material keys through the narrow Drake loading path.

Do not globally coerce Addressables loads by type. A broad hook can break unrelated UI, creator, or game assets.

Record readiness against the existing linked Drake renderer rather than synthesizing a second ECS presentation truth.

## 6. Treat FPP, TPP, and inventory preview as separate views

A weapon appearing in one view does not prove the others.

Validate separately:

- first-person equipped presentation;
- third-person equipped presentation;
- inventory/equipment preview;
- layer/camera visibility;
- transform/bounds;
- cleanup and re-equip.

Repairs should apply only to the registered custom clone that differs from the native owner contract.

## Target end-to-end importer

The proposed consolidated process extends the current framework into this full lifecycle:

~~~text
source intake
→ deterministic authoring inputs
→ target-platform bundle build
→ filesystem + manifest validation
→ isolated bundle inspection
→ runtime package admission
→ asset lifetime ownership
→ native template registration
→ FPP presentation
→ TPP presentation
→ inventory preview
→ acquisition
→ save/load/reload
→ missing-package + migration handling
→ multi-package compatibility
→ release package validation
~~~

That full sequence is a **target process**. Do not report later states merely because an earlier state succeeded.

## Proposed first product envelope

The proposed first complete profile is intentionally narrow:

- rigid melee weapons;
- Windows/Mono first;
- one cloned native `ItemTemplate` per weapon;
- one immutable custom GUID;
- one cooked presentation prefab;
- static mesh/material presentation;
- FPP, TPP, and inventory preview;
- process-session package/template lifetime.

Bows, projectiles, shields, dual-wield coordination, skinned weapon rigs, weapon-local animation controllers, arbitrary scripts in bundles, hot reload, IL2CPP, and broader gameplay logic require separate profiles.

## Persistence and missing packages are not optional finishing touches

A weapon importer is not complete merely because the item can be equipped once.

Before claiming a complete import path, validate:

- save with the custom item;
- quit/reload;
- item reconstruction order;
- missing-package behaviour;
- package-version migration;
- identity collision handling;
- multiple custom weapon packages together.

Until those are proved, describe the supported stage precisely rather than calling the whole importer complete.
