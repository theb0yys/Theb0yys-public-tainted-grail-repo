# Weapon Pipeline

> **Reference page.** Use this after the base item workflow makes sense and you need weapon-specific fields, representation, stats, or validation details. The learning path is [Move from Items to Weapons, Armour, and Creatures](../../01-basic/05_CONTENT_PROGRESSION.md).

Evidence state: **STATIC_CONFIRMED**  
Fresh editor/game execution for this document: **NOT_RUN**

Source snapshot: `theb0yys/merlin-workshop@073bdab3e09d6adad5003339fc49b021738d71e6`

## Model

In Merlin, a weapon is fundamentally an **item pipeline plus weapon specialization**.

There is no need to invent a second item database. A weapon begins as an `ItemTemplate`, inherits from the appropriate abstract weapon family, receives equipping/stat attachments, and references a weapon representation prefab.

## 1. Create the item template

Create an `ItemTemplate` with:

`Assets → TG Data → Miscellaneous → Item`

Choose the correct abstract family. Merlin's item classification exposes families such as:

- one-handed;
- two-handed;
- dagger;
- axe;
- sword;
- blunt;
- polearm;
- shield;
- ranged/bow;
- arrow;
- throwable;
- magic/rod families.

The exact parent matters because gameplay/editor behavior asks the template whether it **inherits from** those abstract references.

## 2. Add equipping data

Add `ItemEquipSpec`, the attachment that describes how the item is equipped and represented.

Select an `EquipmentType`, the equipment category/handling role, appropriate to the weapon:

- `OneHanded`;
- `TwoHanded`;
- `Magic`;
- `MagicTwoHanded`;
- `Shield`;
- `Rod`;
- `Bow`;
- ammo/throwable types where appropriate.

The attachment also carries the visual representation list used for NPC/equipment presentation.

The representation prefab is an addressable reference in the weapon-oriented addressable group.

## 3. Prepare the weapon representation prefab

The actual visual weapon prefab carries a `Weapon` component.

Here, the **Weapon component** is the Unity component that gives the visual representation its weapon-specific runtime data.

Static-confirmed fields include:

- `WeaponType`;
- collider;
- left-handed flag.

The `WeaponType` identifies the weapon family used by the animation/handling path. It maps weapon families to animator-layer behavior. Merlin defines types for one-handed, dagger, sword, axe, two-handed variants and ranged bow/crossbow variants.

This means a mesh alone is not the whole weapon pipeline: the representation prefab must also satisfy the runtime/editor weapon component contract.

## 4. Add stats

Add `ItemStatsAttachment`.

For weapons this contains the meaningful combat surface, including:

- light/heavy attack stamina costs;
- damage range and damage gain;
- heavy/push/backstab modifiers;
- armor penetration;
- critical/weak-spot/sneak modifiers;
- damage type and damage subtypes;
- blocking values and angles;
- force/ragdoll behavior;
- poise damage;
- NPC damage multiplier;
- attacks-per-second data;
- ranged zoom/draw modifiers;
- magic cast costs where applicable.

Merlin also computes/uses poise and force values from the weapon family, so weapon inheritance and stats should agree rather than conflict.

## 5. Optional stat requirements

Add `ItemStatsRequirementsAttachment` when the weapon requires character attributes.

## 6. World representation

The `ItemTemplate` may separately reference drop/pick-up prefabs.

Keep this distinction clear:

- **equipment representation** — used while equipped / per NPC representation;
- **drop/pick-up representation** — used as a world item.

They may point at different prefab shapes.

## Validation checklist

A local weapon test should prove, in order:

1. template resolves;
2. item is classified as the intended weapon family;
3. icon resolves;
4. equipment slot/type is correct;
5. representation prefab resolves;
6. collider and `WeaponType` are correct;
7. equipping/unequipping works;
8. animation family is correct;
9. attacks register expected damage type/range;
10. stamina/block/poise behavior is sensible;
11. dropped/pick-up representation works if supplied.

## Merlin source anchors

- `Assets/Code/Main/Heroes/Items/ItemTemplate.cs`
- `Assets/Code/Main/Heroes/Items/Attachments/ItemEquipSpec.cs`
- `Assets/Code/Main/Heroes/Items/Attachments/ItemStatsAttachment.cs`
- `Assets/Code/Main/Heroes/Items/Weapons/Weapon.cs`
- `Assets/Code/Main/Heroes/Items/Weapons/WeaponType.cs`
- `Assets/Code/Main/Heroes/Items/Weapons/ItemStats.cs`
- `Assets/Code/Main/Heroes/Items/EquipmentType.cs`
