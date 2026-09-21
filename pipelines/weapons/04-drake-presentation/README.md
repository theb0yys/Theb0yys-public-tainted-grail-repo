# Weapon Stage 4 — Drake Presentation

## Objective

Replace only the weapon's presentation through the Drake-compatible route while retaining native item/equip ownership.

## Working shape

The public rigid-weapon example preserves normal equip initiation and redirects the equipped presentation to framework-owned Drake resources.

A successful prototype shape is expected to contain the native/Drake presentation components required by the selected route and should not fall back to an ordinary Unity MeshRenderer as the final production owner.

## Procedure

1. Capture a valid native rigid-weapon presentation as the structural source.
2. Register a package-owned presentation identity/address for the custom weapon.
3. Redirect only the custom ItemTemplate's equipped presentation route.
4. Build/serve a Drake-compatible prototype with the required CharacterHands / Drake renderer structure.
5. When Drake requests mesh/material keys, return the exact package-owned resources.
6. Retain resource handles while Drake owns them.
7. Release resources when the native/framework presentation lifetime ends.
8. Do not manually fabricate Drake ECS lifetime state or edit global resource counters.

## Check

Confirm that:

- native equip requests the custom presentation;
- the custom mesh/material is served through Drake ownership;
- the final equipped object participates in the Drake route;
- resource lifetime is balanced across equip/unequip.

## Failure conditions

- manually parenting a MeshRenderer to the hand;
- bypassing ItemEquip;
- producing a visible object that is not the native hand/view owner;
- leaking or prematurely releasing mesh/material resources;
- modifying global Drake unload behaviour.

## Before continuing

Visible presentation does not cover melee sweep geometry, damage, animation events, audio, trail/finisher timing, preview, or persistence.

## Next

Proceed to [combat and animation](../05-combat-animation/README.md).
