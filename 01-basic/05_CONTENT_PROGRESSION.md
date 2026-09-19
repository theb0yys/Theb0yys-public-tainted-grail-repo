# Move from Items to Weapons, Armour, and Creatures

## What you're doing

You are learning the content-authoring paths in an order that adds one layer of responsibility at a time instead of trying to learn every pipeline simultaneously.

## What you need

- a working Merlin Workshop authoring environment;
- a completed first item-authoring session;
- the public pipeline references under `docs/pipelines/`;
- a habit of noting whether something came from source inspection, an editor test, or an in-game test.

## What you'll learn

You will learn how the content paths build on each other:

~~~text
item -> weapon / armour -> creature / NPC
~~~

You will also learn which responsibilities are new at each stage.

## Steps

### 1. Learn the item pipeline

Read [ITEMS.md](../docs/pipelines/ITEMS.md).

Learn:

- `ItemTemplate` creation;
- inheritance/category;
- localization;
- icon/addressable concept;
- economy fields;
- optional attachments;
- equipment versus world representation.

Do not worry about custom combat behaviour yet.

### 2. Add weapon-specific responsibilities

Read [WEAPONS.md](../docs/pipelines/WEAPONS.md).

A weapon builds on the item pipeline.

Add only the weapon-specific responsibilities:

- correct abstract weapon family;
- `ItemEquipSpec`;
- `EquipmentType`;
- weapon representation prefab;
- `Weapon` component;
- `WeaponType`;
- collider;
- combat stats.

Test the logical item before tuning every stat.

### 3. Add armour-specific responsibilities

Read [ARMOUR.md](../docs/pipelines/ARMOUR.md).

Armour also builds on the item pipeline.

Add:

- armour inheritance/weight class;
- equipment slot;
- per-NPC visual representation where required;
- armour stats;
- optional requirements.

Keep worn representation and drop/pick-up representation conceptually separate.

### 4. Move to a creature or NPC last

Read [CREATURES_KANDRA.md](../docs/pipelines/CREATURES_KANDRA.md).

Only start here once the authoring workflow feels familiar.

The preparation route visible in inspected Merlin Workshop source includes:

1. **TG -> Assets -> Prefabs -> Prepare NPC Prefab**
2. prepare/verify Animator and Kandra renderer requirements;
3. fix root/head/torso/collider/ragdoll warnings;
4. **TG -> Assets -> Prefabs -> Prepare NPC Spec**
5. duplicate from a behaviourally close known-good base spec;
6. preserve the duplicated fighting-style chain initially;
7. prove spawn/idle/movement;
8. then test combat, weapons, loot and death;
9. only then introduce custom fighting-style/animation changes.

### 5. Keep track of what you actually tested

The public pipeline documents were built from inspected toolkit source. Individual pages say when an editor or in-game run was also performed.

When you successfully run one, record the exact toolkit/game versions and what you observed. Formal status labels are documented separately in [Testing and Evidence Status](../docs/EVIDENCE.md).

## What success looks like

You can explain what the base item pipeline owns, what weapons and armour add, and why creatures/NPCs are a larger authoring problem.

You can also work through one layer at a time without treating something found in source as if it has already worked in the game.

## Common problems

**You start with a creature before understanding items:** too many new systems become unknown at once.

**You tune every weapon or armour field before proving the logical definition:** validate the smallest working shape first.

**You treat something seen in source or the editor as if it already worked in the game:** keep those checks separate.

**You change inheritance, visuals, stats, and behaviour in one test:** reduce the change until a failure tells you something useful.

## Where to go next

When the staged content loop makes sense, continue to **[Understand How Mods Work](../02-foundational/README.md)** or follow the specific pipeline reference for the content you are building.
