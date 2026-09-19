# Weapons: Native Item, Equip, Combat, and Presentation Lifecycle

> **Reference page.** This is the native weapon ownership graph extracted from the working research. It is not yet a complete public custom-weapon tutorial.

## What this system is

A native FoA weapon is not a separate universal `Weapon` Model.

The durable gameplay object is still an `Item`, with weapon meaning derived from its template/attachments.

The researched chain is:

~~~text
ItemTemplate registry
→ Item MVC Model
→ ItemEquipSpec
→ ItemEquip : Element<Item>
→ equipped GameObject load
→ CharacterHandBase View<Item>
→ native hand/combat/presentation lifecycle
→ renderer-specific representation such as Drake
→ native inventory/equipment/save state
~~~

## Who owns it in FoA

Important owners:

- `ItemTemplate` — weapon definition/classification;
- `Item` — runtime/saved gameplay object;
- `ItemEquipSpec` — equippability and representation definition;
- `ItemEquip : Element<Item>` — equip/unequip representation lifecycle;
- `CharacterHandBase : View<Item>` — equipped hand/weapon runtime View;
- native character/hand/combat systems;
- Drake/native renderer provider when the representation uses that path.

## Important identities, types, and methods

Native weapon classification can derive from:

- ItemTemplate inheritance;
- tags;
- attachments;
- helper properties such as weapon/one-handed/sword/etc.

`ItemEquip` selects an appropriate representation, loads a GameObject reference, instantiates it under the native socket, requires `CharacterHandBase`, binds it through World, and attaches it through the character owner.

The item template GUID remains important for save restoration.

## Where it exists in the lifecycle

### Equip

~~~text
Item equipped event
→ ItemEquip selects representation
→ load prefab/reference
→ verify item still equipped and owner valid
→ instantiate under native socket
→ require CharacterHandBase
→ World.BindView(Item, Hand)
→ character AttachWeapon
→ native combat/presentation owns it
~~~

### Restore

The intended researched restore dependency is:

~~~text
save load
→ custom template registered early enough
→ ReadTemplate(customGuid)
→ Item restore
→ attachments restore
→ equipped slot restore
→ ItemEquip.OnRestore
→ native equipped View rebuilt
~~~

The exact ordering between custom registration and first saved custom-GUID resolution remains a key persistence gate.

## How we interact with it

### Separate weapon layers

A custom weapon needs at least:

1. item/template identity;
2. registration;
3. acquisition;
4. equip compatibility;
5. native combat classification/stats;
6. presentation/prototype;
7. renderer resources;
8. inventory preview;
9. FPP/TPP behavior;
10. save/migration.

Do not compress these into "load mesh and equip."

### Reuse native ItemEquip where possible

Let native equip ownership handle:

- socketing;
- View binding;
- character hand lifecycle;
- hide/show;
- unequip;
- combat callbacks.

### Treat presentation provider lifetime separately

A custom AssetBundle/Drake prototype path needs:

- resource ownership;
- leases/refcounts;
- multiple simultaneous instances;
- scene transition while equipped;
- final cleanup.

## Why this route

The weapon research explicitly rejected direct Unity renderer attachment as a generic production path.

It can make a mesh visible while bypassing:

- native equip;
- preview;
- first-/third-person routes;
- combat hand components;
- Drake resource ownership;
- scene/resource cleanup.

The native `ItemEquip` path already owns much of this lifecycle.

## What goes wrong

### "Weapon = custom ItemTemplate"

Registration is only the first layer.

### Direct mesh under hand

Can look correct in one camera while failing native equip/preview/resource lifecycle.

### Late custom template registration

Provider visibility after insertion does not prove every downstream system observes late registration, especially save restore.

### Hot unregister expectation

The researched direct native registration has no proven native unregister. Inserted registrations are effectively process-session immutable in the current design.

### Definition hash ignores source/build drift

A custom package can look unchanged while its native prototype or equipped representation changed across game builds.

## How to verify

A release-grade custom weapon process should eventually prove:

1. exact source template/profile;
2. stable custom GUID/name;
3. collision/idempotency;
4. normal provider resolution;
5. bounded acquisition;
6. inventory UI;
7. equip through native `ItemEquip`;
8. `CharacterHandBase`/combat path;
9. expected mesh/material/prototype;
10. FPP/TPP/preview;
11. attacks/hit detection;
12. repeated equip/unequip;
13. two simultaneous instances if supported;
14. scene transition while equipped;
15. save/load unequipped;
16. save/load equipped;
17. missing package behavior;
18. migration/reinstall/upgrade;
19. final resource cleanup.

## Current proof boundary

The native weapon architecture is well mapped.

The internal project has substantial prototype/registrar/Drake work, but a generic release-ready public custom-weapon framework is still blocked on registration atomicity, acquisition, persistence, provider lifetime, localisation, current-build compatibility, and end-to-end runtime receipts.
