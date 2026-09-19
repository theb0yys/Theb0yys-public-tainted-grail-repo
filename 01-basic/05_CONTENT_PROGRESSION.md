# 05 - Content Progression

Do not learn all content pipelines simultaneously.

Use this order.

## 1. Item

Read docs/pipelines/ITEMS.md.

Learn:

- ItemTemplate creation;
- inheritance/category;
- localization;
- icon/addressable concept;
- economy fields;
- optional attachments;
- equipment versus world representation.

Do not worry about custom combat behaviour yet.

## 2. Weapon

Read docs/pipelines/WEAPONS.md.

A weapon builds on the item pipeline.

Add only the weapon-specific responsibilities:

- correct abstract weapon family;
- ItemEquipSpec;
- EquipmentType;
- weapon representation prefab;
- Weapon component;
- WeaponType;
- collider;
- combat stats.

Test the logical item before tuning every stat.

## 3. Armour

Read docs/pipelines/ARMOUR.md.

Armour also builds on the item pipeline.

Add:

- armour inheritance/weight class;
- equipment slot;
- per-NPC visual representation where required;
- armour stats;
- optional requirements.

Keep worn representation and drop/pick-up representation conceptually separate.

## 4. Creature / NPC

Read docs/pipelines/CREATURES_KANDRA.md.

Only start here once the authoring workflow feels familiar.

The source-confirmed preparation route includes:

1. **TG -> Assets -> Prefabs -> Prepare NPC Prefab**
2. prepare/verify Animator and Kandra renderer requirements;
3. fix root/head/torso/collider/ragdoll warnings;
4. **TG -> Assets -> Prefabs -> Prepare NPC Spec**
5. duplicate from a behaviourally close known-good base spec;
6. preserve the duplicated fighting-style chain initially;
7. prove spawn/idle/movement;
8. then test combat, weapons, loot and death;
9. only then introduce custom fighting-style/animation changes.

## Evidence warning

The current public pipeline documents are source-contract documentation. Their headers state whether fresh editor/game execution was actually performed.

When you successfully run one, record the exact toolkit/game versions and result.
