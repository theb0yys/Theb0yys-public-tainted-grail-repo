# Weapon Stage 3 — Native Equip Lifecycle

## Objective

Ensure the custom weapon enters FoA's normal equip ownership rather than creating a parallel hand/weapon system.

## Native chain

    registered ItemTemplate
    → runtime Item
    → ItemEquipSpec
    → ItemEquip
    → representation selection
    → asset/presentation load
    → CharacterHandBase
    → World.BindView(Item, Hand)
    → Character.AttachWeapon(Hand)

Unequip must reverse ownership:

    Character.DetachWeapon
    → hand/view discard
    → presentation/resource release

Melee implementations continue through CharacterWeapon or the native class selected by the archetype.

## Procedure

1. Create/acquire a normal runtime Item from the registered custom template.
2. Equip it through the normal inventory/equipment route.
3. Observe the native equip event and representation selection.
4. Validate that asynchronous representation load still belongs to the currently equipped item/owner before binding.
5. Confirm CharacterHandBase-compatible representation is produced.
6. Confirm native view binding and character attachment occur.
7. Unequip and verify native detach/discard/release.

## Validation gate

PASSED when equip and unequip complete through native ownership with no manually parented substitute renderer standing in for the weapon view.

## Does not prove

A successful native equip can still present the wrong mesh/material, use incomplete Drake resources, have incorrect combat behaviour, or fail preview/persistence.

## Next

Proceed to [Drake presentation](../04-drake-presentation/README.md).
