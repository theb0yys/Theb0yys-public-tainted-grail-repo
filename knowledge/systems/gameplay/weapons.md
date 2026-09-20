# Weapons

Use this page when you want to add or substantially change a **FoA weapon**, especially when custom models or native equip behavior are involved.

The first thing to understand is:

> A weapon is not just an item with a mesh.

FoA splits weapon ownership across item definition, runtime item state, equip logic, hand Views, combat logic, and renderer-specific presentation.

## The native weapon chain

~~~text
ItemTemplate
→ Item
→ ItemEquipSpec
→ ItemEquip
→ CharacterHandBase
→ CharacterWeapon / combat systems
→ presentation owner such as Drake
~~~

The durable gameplay object is still the native `Item`.

There is no need to invent a separate universal weapon Model.

## What each layer does

- `ItemTemplate` — weapon definition, classification, attachments, UI/economy references.
- `Item` — runtime item Model and saved instance state.
- `ItemEquipSpec` — marks the item as equippable and describes representation selection.
- `ItemEquip` — runtime Element that owns equip/unequip representation lifecycle.
- `CharacterHandBase` — native held-weapon View.
- `CharacterWeapon` — melee-specific combat implementation.
- Drake/other renderer systems — presentation/resource lifetime for the selected visual path.

## Native equip flow

The researched path is roughly:

~~~text
Item.Events.Equipped
→ ItemEquip selects representation
→ ARAssetReference.LoadAsset<GameObject>()
→ verify owner/item is still valid and equipped
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

A custom weapon needs to survive this entire lifecycle, not merely appear once in the hand.

## Start from a known weapon family

For the first reusable custom weapon, use one current-build native archetype whose behavior is already understood.

The existing research treats a rigid one-handed sword as a good first bounded family.

Clone/preserve the complete native template/component graph rather than copying only visible fields.

## Keep the native contract intact first

Preserve things such as:

- abstract-template lineage;
- attachment groups;
- equip spec;
- combat references;
- animation/controller profile;
- audio/trail/finisher/hit-stop configuration.

Change one layer at a time.

A mesh replacement should not also silently redefine combat geometry, audio, equip semantics, and persistence in the same proof.

## Give the weapon a separate identity

Use a new stable mod-owned GUID/name and register it using the custom-item rules in [Items](items.md).

Then create a native `Item` and use one controlled acquisition route.

Do not build a parallel weapon-object system.

## Let ItemEquip own equipping

If the native equip system already selects representations, loads them, binds the View, attaches the hand object, and releases assets on unequip, keep that ownership.

Do not bypass it simply because manually parenting a mesh under the hand looks easier.

## Rendering is a separate layer

For Drake-backed rigid weapons, the visual must respect Drake's entity/resource/lifetime rules.

A plain Unity renderer fallback may be useful for experimentation, but it is not automatically the reusable production path.

## Combat geometry is not render geometry

The imported mesh bounds do not define the native melee hitbox/sweep.

`CharacterWeapon` owns the melee combat behavior.

For a first custom visual, keep the native source weapon's combat profile until you have a separate reason to change it.

## Common failures

### Direct mesh replacement only

Something appears in the hand, but equip ownership, View binding, FPP/TPP behavior, combat, and cleanup may all be wrong.

### Building hitboxes from mesh bounds

The render mesh is not the native melee sweep definition.

### Cloning only obvious fields

Hidden serialized references/attachments can matter.

### Assuming late template registration is globally visible

Provider lookup can succeed even if another native system cached data earlier.

### Assuming the template can be unregistered cleanly

The direct private map route has no generally proven hot-unregister path. Treat process-session registration as effectively immutable unless you prove otherwise.

### Failure after AddToMap

Direct insertion is not transactional. A later failure can leave native maps changed.

## What to verify

A complete weapon proof should check:

1. exact source template/archetype;
2. custom GUID registration and lookup;
3. native `Item` construction/acquisition;
4. native equip event;
5. representation load;
6. `CharacterHandBase` View;
7. `World.BindView` / character attach;
8. FPP/TPP/inventory-preview behavior;
9. renderer/Drake readiness;
10. native attack/sweep/hit behavior;
11. audio/trail/animation events;
12. unequip cleanup and asset release;
13. save/load only if you intend to claim it.

## Evidence limits

Strongly mapped:

- `Item` → `ItemEquip` → `CharacterHandBase` / `CharacterWeapon`;
- archetype-preserving custom-weapon direction.

Partially implemented:

- private Mono registration;
- substantial Drake presentation work.

Not yet universal:

- transactional registration;
- late-registration visibility in every consumer;
- generic Item/acquisition/equip proof for all weapon families;
- cold-save ordering;
- missing-package behavior;
- hot unload;
- cross-build/IL2CPP equivalence.

Do not call custom weapons "solved" because a custom template or mesh exists.
