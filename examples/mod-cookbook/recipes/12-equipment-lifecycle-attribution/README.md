# Recipe — Equipment Lifecycle Attribution

**Category:** equipment / weapon presentation  
**Source-path evidence:** SOURCE_CONFIRMED lifecycle map  
**This public recipe:** NOT_RUN

Equipment is not one boolean.

A safe observer or future feature should keep these ownership layers separate:

~~~text
native equip request
    ↓
slot/loadout resolution
    ↓
item slot mutation
    ↓
Item.Events.Equipped / Unequipped
    ↓
ItemEquip visual lifecycle
    ↓
CharacterHandBase / clothing presentation
    ↓
draw-sheathe / animation / renderer state
    ↓
cleanup on unequip
~~~

## Layer 1 — native equip request and rules

FoA's higher-level equipment owners include native loadout and inventory helpers.

This layer owns concerns such as:

- whether a slot accepts an item;
- locked loadouts or equipment restrictions;
- multi-slot resolution;
- displaced occupants;
- return-to-inventory behavior.

Do not replace this layer with a direct call to `Item.EquipInSlot` from a custom UI.

## Layer 2 — item slot truth

The item-level lifecycle includes:

~~~text
Item.EquipInSlot(EquipmentSlotType)
Item.UnequipInSlot(EquipmentSlotType)
Item.IsEquipped
~~~

These are useful observation seams for example 34.

They also participate in saved equipped-slot state, so treating them as casual mutation helpers would cross into persistence behavior.

## Layer 3 — hero hand projection

For current weapon/hand context, prefer native read surfaces such as:

~~~text
Hero.MainHandItem
Hero.OffHandItem
~~~

These answer "what item does the hero currently expose in this hand projection?"

They do not prove that every renderer, animation or hand object has completed a presentation transition.

## Layer 4 — weapon visibility and draw/sheathe state

Useful reads include:

~~~text
Hero.WeaponsVisible
Hero.IsWeaponEquipped
Hero.PullingRangedWeapon
~~~

These are presentation/context signals.

They are not substitutes for inspecting the actual native weapon-view lifecycle when building a visual feature.

## Layer 5 — presentation ownership

Maintainer research maps native weapon presentation into systems including `ItemEquip` and `CharacterHandBase`.

That lifecycle can include:

- weapon asset loading;
- hand attachment;
- animation overrides;
- animator-layer changes;
- weapon GameObject visibility;
- quiver behavior;
- renderer/Kandra handling;
- release and cleanup.

A hand-item reference changing is therefore not proof that every presentation step completed.

## Layer 6 — cleanup and unequip

Treat unequip as a lifecycle, not merely a slot going empty.

A visual sidecar must define how it responds when:

- the item leaves a slot;
- the hand projection changes;
- weapons are sheathed;
- the native weapon view is destroyed/replaced;
- perspective changes;
- the hero/session unloads.

Do not leave plugin-owned renderers or objects attached to stale weapon views.

## Observer attribution pattern

A useful diagnostic chain is:

~~~text
item equip/unequip event
    ↓
slot + Item.IsEquipped
    ↓
main/off-hand projection change
    ↓
WeaponsVisible / IsWeaponEquipped transition
    ↓
optional later presentation evidence
~~~

Each observer reports its own layer. Do not collapse all rows into a claim that "equip succeeded visually" unless the full path was actually observed.

## Mutation boundary

This recipe authorizes no equipment mutation.

It does not demonstrate:

- a custom equipment UI write adapter;
- custom weapon registration;
- direct `EquipInSlot` calls;
- renderer activation;
- weapon reparenting;
- Kandra changes;
- animation injection;
- save-safe custom equipment.

Those require their own researched and validated lifecycle.

## Validation ladder for a future equipment feature

1. Observe one native equip and unequip transition.
2. Confirm the expected slot and `IsEquipped` state.
3. Confirm main/off-hand projections update as expected.
4. Confirm draw/sheathe state independently.
5. If visuals matter, inspect native weapon-view creation and cleanup separately.
6. Test perspective transitions separately.
7. Test save/reload only when the feature actually crosses persistence.
8. Verify plugin-owned objects, if any, clean up on unequip, replacement and unload.

This preserves native equipment truth while giving later features clear attribution boundaries.
