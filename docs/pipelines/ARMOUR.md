# Armour Pipeline

> **Reference page.** Use this after the base item workflow makes sense and you need armour-specific inheritance, slots, visuals, or stats. The learning path is [Move from Items to Weapons, Armour, and Creatures](../../01-basic/05_CONTENT_PROGRESSION.md).

Evidence state: **STATIC_CONFIRMED**  
Fresh editor/game execution for this document: **NOT_RUN**

Source snapshot: `theb0yys/merlin-workshop@073bdab3e09d6adad5003339fc49b021738d71e6`

## Model

Merlin does **not** require a separate `Armor.cs` authoring object for ordinary wearable armour.

Armour is an `ItemTemplate` specialized through:

- abstract-template inheritance;
- an armour `EquipmentType`;
- `ItemEquipSpec`;
- `ItemStatsAttachment`;
- per-NPC visual representation references.

This is why generic armour examples should be derived from the item pipeline rather than treated as unrelated standalone prefabs.

## 1. Create the item template

Create an item using:

`Assets → TG Data → Miscellaneous → Item`

Assign the appropriate armour inheritance.

Merlin's `ItemTemplate` determines:

- whether the template is armour;
- whether it belongs to light, medium or heavy armour inheritance.

Weight class is therefore part of the template lineage, not just a display label.

## 2. Select the equipment slot

Add/configure `ItemEquipSpec`, the attachment that describes how the item is equipped and represented, with the intended armour `EquipmentType`.

For armour, **EquipmentType** is the worn equipment slot/category such as cuirass, helmet, gauntlets, greaves, boots, or back.

Merlin defines armour slots including:

- `Cuirass`;
- `Helmet`;
- `Gauntlets`;
- `Greaves`;
- `Boots`;
- `Back`.

The equipment type determines the main equipment slot and category.

## 3. Assign the visual representation

Armour visuals are supplied through the equippable item's representation list.

Each representation can be constrained by abstract NPC templates, allowing different visual prefabs for different NPC families while keeping one logical item template.

The item preview/mesh path also treats armour specially: equipped armour representation is taken from the equipping representation rather than simply assuming the world-drop prefab is the worn mesh.

## 4. Set armour stats

Use `ItemStatsAttachment`.

For an armour item, the attachment exposes armour-oriented values such as:

- armor;
- armor gain;
- force-related values where relevant.

The base `ItemTemplate` also owns weight and weight-loss behavior.

The hero's armour-weight system sums equipped items that classify as armour and uses that equipment weight in the wider armour-weight/encumbrance calculation.

## 5. Optional stat requirements

Use `ItemStatsRequirementsAttachment` for strength/dexterity/etc. gates when appropriate.

## 6. Keep worn and dropped representations separate

A wearable armour prefab and a world pick-up/drop prefab serve different roles.

Do not assume that a character-equipment representation automatically satisfies world-item collision/pick-up requirements.

## Validation checklist

A local armour test should verify:

1. item template resolves;
2. armour inheritance is correct;
3. light/medium/heavy classification is correct when applicable;
4. equipment slot is correct;
5. representation resolves for the intended NPC/body family;
6. the equipped mesh appears correctly;
7. armor/stat values apply;
8. item weight contributes correctly;
9. unequip/re-equip is stable;
10. world drop/pick-up works if supplied.

## Merlin source anchors

- `Assets/Code/Main/Heroes/Items/ItemTemplate.cs`
- `Assets/Code/Main/Heroes/Items/Attachments/ItemEquipSpec.cs`
- `Assets/Code/Main/Heroes/Items/Attachments/ItemStatsAttachment.cs`
- `Assets/Code/Main/Heroes/Items/EquipmentType.cs`
- `Assets/Code/Main/Heroes/Items/ArmorWeight.cs`
