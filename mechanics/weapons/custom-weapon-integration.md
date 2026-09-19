<!-- Canonical mechanic. Migrated from docs/reference/WEAPONS.md. -->
# Weapons: Native Architecture and Current Proven Boundary

> **Reference/process page.** Weapon integration is not just "an item with a mesh." The current research establishes a substantial native path, but the complete reusable custom-weapon importer remains **PARTIAL PATH PROVEN**.

## What this system is

FoA weapon ownership is distributed across the item, equip, View, combat, and renderer systems.

The researched native path is:

~~~text
ItemTemplate registry
→ Item : MVC Model
→ ItemEquip : Element<Item>
→ actor/gender/hand representation selection
→ equipped GameObject load
→ CharacterHandBase : View<Item>
    └─ melee implementation: CharacterWeapon
→ SetUnityRepresentation(linkedLifetime=true, movable=true)
→ renderer-specific representation such as Drake
→ native inventory/equipment/save ownership
~~~

There is no need to invent a second universal runtime `Weapon` Model.

The durable gameplay object is the native `Item`.

## Who owns it in FoA

- `ItemTemplate` — identity, classification, abstract-template lineage, attachments, UI/economy references.
- `Item` — runtime MVC Model and saved item state.
- `ItemEquipSpec` — attachment that makes an item equippable and defines representation selection.
- `ItemEquip` — `Element<Item>` that owns equipped representation lifecycle.
- `CharacterHandBase` — native equipped View.
- `CharacterWeapon` — melee-specific hand implementation.
- Drake/native renderer owner — presentation/resource lifetime where selected.
- native inventory/equipment/save systems — durable ownership.

## Important identities, types, and methods

Key native concepts:

- `ItemTemplate`
- `Item`
- `ItemEquipSpec`
- `ItemEquip`
- `CharacterHandBase`
- `CharacterWeapon`
- `ARAssetReference`
- `World.BindView(Item, Hand)`
- character attach/detach weapon ownership
- `View.Discard()`
- asset-reference release

The equip flow selects representation by actor/gender/hand and validates asynchronous load state before binding it.

## Where it exists in the lifecycle

The native equip lifecycle is conceptually:

~~~text
Item.Events.Equipped
→ ItemEquip selects representation
→ ARAssetReference.LoadAsset<GameObject>()
→ validate item still equipped / owner still valid
→ instantiate under native socket
→ require CharacterHandBase
→ SetUnityRepresentation(...)
→ World.BindView(Item, Hand)
→ Character.AttachWeapon(Hand)

unequip
→ Character.DetachWeapon(Hand)
→ Hand/View.Discard()
→ release asset reference
~~~

A custom weapon must survive this lifecycle rather than merely render a mesh once.

## How we interact with it

### 1. Start from a known native archetype

For the first reusable weapon, choose one exact current-build weapon family whose complete behavior is already known.

The current research recommends a rigid one-handed sword as the first bounded consumer.

### 2. Preserve the full item/template contract

Clone the complete native `ItemTemplate` component graph, not only visible fields.

Keep:

- abstract-template lineage;
- attachment groups;
- equip spec;
- combat-related references;
- source animation/controller profile;
- audio/trail/finisher/hit-stop profile;

unless a later gate explicitly changes one.

### 3. Give it a separate custom identity

Use a stable mod-owned GUID/name and register the template through the same item-registration principles documented in [Items](../../docs/reference/ITEMS.md).

### 4. Create a native Item and use a bounded acquisition route

Do not build a separate "weapon object" system.

### 5. Let native ItemEquip own equipping

The source item/equip path already handles representation selection, load validity, socket parenting, View binding, attach/detach and release.

### 6. Replace presentation through the actual renderer owner

For Drake-backed rigid weapons, presentation must respect Drake resource/entity/lifetime ownership.

A direct Unity renderer fallback is not the reusable production route.

### 7. Preserve native combat geometry first

The imported mesh does **not** automatically define the melee hitbox.

`CharacterWeapon` owns native sweep/hit behavior. The safest first custom visual therefore reuses the source combat profile.

## Why this route

The weapon research found that the visible model is only one stage of a larger owner chain.

The reliable path is **archetype preserving**:

~~~text
known native sword
→ custom identity
→ native Item
→ native ItemEquip
→ native CharacterWeapon combat/lifecycle
→ custom rigid presentation
~~~

That keeps animation, combat events, audio, sweep/hitbox behavior and cleanup inside systems that already know how to own them.

## What goes wrong

### Direct Unity mesh/renderer replacement

It can make something visible while bypassing:

- ItemEquip ownership;
- View binding;
- perspective/FPP/TPP rules;
- Drake resource lifetime;
- combat profile;
- equip/unequip cleanup.

### Deriving hitbox from custom mesh bounds

Native melee sweep geometry is not the imported render bounds; preserve the native combat geometry owner.

### Cloning only a few template fields

The current clone-profile work proves topology better than full semantic equality. Hidden serialized references can still matter.

### Late template insertion assumed globally visible

Provider map visibility is proven; every downstream cache is not.

### Hot reload/unregister assumptions

No proven native unregister exists for the direct template map route. Process-session registration should be treated as immutable until restart.

### Post-insertion failure

A direct `AddToMap` mutation is not transactional. Failure after insertion can leave native maps changed.

A reliable registrar therefore needs a fail-stop or proven rollback policy.

## How to verify

A complete weapon proof should separately verify:

1. exact source template/profile;
2. custom identity registration and provider lookup;
3. native Item construction/acquisition;
4. native equip event;
5. representation load and `CharacterHandBase` type;
6. `World.BindView` / character attach;
7. correct FPP/TPP/inventory-preview ownership;
8. renderer/Drake resource readiness;
9. native melee attack/sweep/hit behavior;
10. audio/trail/animation events;
11. unequip cleanup and asset release;
12. save/load only when explicitly claimed.

## Current proof boundary

**Strongly understood:** native Item → ItemEquip → CharacterHandBase/CharacterWeapon architecture and the archetype-preserving direction.

**Implemented/partial:** private-Mono native registrar and a substantial Drake presentation framework.

**Still incomplete for a generic public importer:** transactional registration, system-wide late-registration visibility, full semantic clone profile, generic native Item/acquisition/equip proof, cold-save ordering, missing-package behavior, hot-unload, complete localisation, cross-build/IL2CPP equivalence, and release-grade validation.

Do not present custom weapons as "solved" merely because a custom `ItemTemplate` or mesh exists.
