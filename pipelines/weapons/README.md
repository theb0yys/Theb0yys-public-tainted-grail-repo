# Weapon Importation Pipeline

This pipeline reconstructs a custom rigid-weapon path while preserving FoA's native item, equip, combat, and presentation ownership.


## Stage map

1. [Native archetype and source profile](01-native-archetype/README.md)
2. [Custom ItemTemplate registration](02-template-registration/README.md)
3. [Native equip lifecycle](03-equip-lifecycle/README.md)
4. [Drake presentation](04-drake-presentation/README.md)
5. [Combat, animation, audio, and hit geometry](05-combat-animation/README.md)
6. [FPP, TPP, and inventory preview](06-perspectives-preview/README.md)
7. [Persistence, package lifetime, and release](07-persistence-release/README.md)
8. [Validation matrix](validation/README.md)
9. [Known failure modes](failures/README.md)

## Canonical technical background

- [Weapons system](../../knowledge/systems/gameplay/weapons.md)
- [Native weapon integration](../../knowledge/systems/gameplay/native-weapons/README.md)
- [Drake presentation](../../knowledge/systems/presentation/drake/README.md)
- [Rigid weapon runnable example](../../examples/mono/items/custom-rigid-weapon-presentation/README.md)

## Core ownership rule

The render mesh is not the weapon owner. Inventory identity, equip legality, combat state, hand selection, animation, saving, and cleanup remain in the native item/equip/combat path.
