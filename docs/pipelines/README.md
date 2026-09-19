# FoA Authoring Pipelines

> **Reference library.** Open only the pipeline for the content type you are currently making. The guided content-learning path starts at [Make Your First Piece of Content](../../00-never-made-a-mod-start-here/05_FIRST_CONTENT_AUTHORING.md).

These notes describe the public-safe authoring routes visible in **Merlin Workshop**, the Tainted Grail: The Fall of Avalon modding toolkit.

## Source snapshot

Source repository:

`theb0yys/merlin-workshop`

Pinned commit:

`073bdab3e09d6adad5003339fc49b021738d71e6`

This documentation was produced from direct inspection of that source tree. No FOA-SDK material is used here.

## How far these guides were checked

The authoring structure in these guides was confirmed by direct inspection of the pinned Merlin Workshop source.

Fresh end-to-end editor/game execution was not part of that documentation pass unless an individual page explicitly says otherwise.

For the formal status labels used in deeper reference material, see [Testing and Evidence Status](../EVIDENCE.md).

## Pipelines

### Items

[ITEMS.md](ITEMS.md)

Core `ItemTemplate` creation, inheritance, icons, economy fields, drop/pick-up prefabs, attachments and addressable expectations.

### Weapons

[WEAPONS.md](WEAPONS.md)

Weapon-specific item inheritance, equipment slot/type, stats, weapon representation prefab, collider and animation weapon type.

### Armour

[ARMOUR.md](ARMOUR.md)

Armour-specific item inheritance, slot assignment, light/medium/heavy classification, per-NPC equipment representation and armour stats.

### Creatures / NPCs / Kandra

[CREATURES_KANDRA.md](CREATURES_KANDRA.md)

The two-stage Merlin authoring route: prepare the visual prefab for Kandra/Animator requirements, then duplicate and wire the NPC spec/template/fighting-style chain.

## Why these are separate

Merlin does not model weapons and armour as unrelated standalone systems.

- A weapon is an `ItemTemplate` specialized through abstract-template inheritance and item attachments, plus a weapon representation prefab.
- Armour is also an `ItemTemplate`, specialized through abstract inheritance, equipment type and item attachments.
- A creature/NPC uses a separate NPC template/spec/visual-prefab chain and requires additional Animator/Kandra preparation.

That distinction is important when creating generic examples: duplicate the **shape** of the authoring contract, not a specific game's content.
