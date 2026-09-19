# Item Pipeline

Evidence state: **STATIC_CONFIRMED**  
Fresh editor/game execution for this document: **NOT_RUN**

Source snapshot: `theb0yys/merlin-workshop@073bdab3e09d6adad5003339fc49b021738d71e6`

## What Merlin actually creates

Merlin exposes an editor menu for creating an item template:

`Assets → TG Data → Miscellaneous → Item`

The implementation creates an `ItemTemplate` as a prefab rather than a loose data file.

An **ItemTemplate** is the reusable game definition for an item. A **prefab** is the reusable Unity object that stores that definition in the authoring project. Template creation explicitly rejects the Unity `Resources` directory because templates are expected to participate in the project's addressable/template system.

An **addressable** is a Unity-managed lookup entry: the project gives an asset an address/group so other content can refer to it without depending on a raw filesystem path.

## Core item shape

A normal item starts with an `ItemTemplate`.

The template owns the broad item identity and presentation/economy data:

- template inheritance / abstract type;
- tags;
- localized name, description and flavour;
- icon reference;
- quality;
- price and buy-price behavior;
- weight;
- stackability and drop restrictions;
- damage-surface classification;
- drop/pick-up prefab references.

The item category is not merely a free text field. Merlin derives many categories from **template inheritance**. The same `ItemTemplate` type can therefore represent consumables, crafting items, weapons, armour, jewelry, keys and other categories depending on its abstract lineage and attachments.

## Icon pipeline

The custom `ItemTemplate` editor normalizes item icons into the item-icon addressable surface.

Static-confirmed behavior:

- icon group: `ItemsIcons`;
- icon address shape: `Icon/<asset-name>`;
- expected labels include `Item`, `Icon` and `Sprite`.

A public mod should create its own icon and let the local Merlin project manage the addressable entry. Do not copy a game icon into this repository.

## Base item workflow

1. Create a new `ItemTemplate` prefab from the Merlin editor menu.
2. Choose the appropriate existing abstract parent/inheritance family rather than reproducing a category with arbitrary strings.
3. Assign mod-owned localization and icon data.
4. Fill only the economy/weight/stack/drop behavior needed by the item.
5. Assign a mod-owned drop or pick-up prefab when the item needs a world representation.
6. Add attachments only for behavior the item actually supports.
7. Verify the resulting prefab is in the expected template/addressable system.
8. Test the smallest behavior first: template visibility/lookup, then inventory/world use, then any special attachment behavior.

## Common attachments

### Equippable items

Use `ItemEquipSpec`.

`ItemEquipSpec` is the attachment that describes how an item is equipped and which visual representation should be used.

It carries:

- an `EquipmentType` — the equipment category/slot or handling role;
- gem-slot count;
- weapon-specific finishing/hit-stop options;
- one or more visual item representations selected by NPC abstraction;
- equipment hand;
- an addressable item representation prefab.

### Stat-bearing items

Use `ItemStatsAttachment`.

It is explicitly defined for equippable item stats and exposes different fields according to the template category.

### Attribute requirements

Use `ItemStatsRequirementsAttachment` when the item requires RPG attributes such as strength, dexterity, spirituality, perception, endurance or practicality.

## Validation checklist

Before calling a custom item usable, verify locally:

- the template has a unique mod-owned identity;
- the inheritance family matches the intended item category;
- icon/addressable resolution works;
- required attachments are present;
- every referenced prefab is mod-owned or legally redistributable;
- the item can be resolved by the game/toolkit;
- inventory behavior works;
- drop/pick-up behavior works when applicable;
- no vanilla asset was copied merely to satisfy a reference.

## Merlin source anchors

- `Assets/Code/Editor/Assets/TemplateCreation.cs`
- `Assets/Code/Main/Heroes/Items/ItemTemplate.cs`
- `Assets/Code/Editor/Main/Heroes/Items/ItemTemplateEditor.cs`
- `Assets/Code/Main/Heroes/Items/Attachments/ItemEquipSpec.cs`
- `Assets/Code/Main/Heroes/Items/Attachments/ItemStatsAttachment.cs`
- `Assets/Code/Main/Heroes/Items/Attachments/ItemStatsRequirementsAttachment.cs`

These paths document the authoring contract. They do not authorize redistribution of unrelated game content.
