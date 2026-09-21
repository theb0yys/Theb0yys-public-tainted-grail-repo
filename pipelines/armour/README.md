# Armour Importation Pipeline

This pipeline reconstructs the custom-armour process as separate geometry, deformation, Kandra, item/equip, runtime, and persistence stages.

## Current proof boundary

Public evidence covers substantial importer/conversion infrastructure, deformation and Kandra package work, guarded runtime Kandra registration for proof geometry, and native BaseClothes / ClothStitcher / Kandra ownership research. A generic production-ready custom armour item with universal deformation, equip, persistence, migration, and uninstall guarantees is not established.

## Stage map

1. [Source intake and provenance](01-source-intake/README.md)
2. [Geometry, skeleton, and target-rig contract](02-geometry-rig/README.md)
3. [Deformation compatibility](03-deformation/README.md)
4. [Kandra conversion and package build](04-kandra-conversion/README.md)
5. [Kandra runtime registration](05-kandra-registration/README.md)
6. [Item identity and native clothes/equip](06-item-native-clothes/README.md)
7. [Runtime visual/equip validation](07-runtime-validation/README.md)
8. [Persistence, migration, and release](08-persistence-release/README.md)
9. [Known failure modes](failures/README.md)

## Canonical technical background

- [Armour system](../../knowledge/systems/gameplay/armour.md)
- [Kandra presentation](../../knowledge/systems/presentation/kandra/README.md)
- [Native object ownership](../../knowledge/systems/core/native-object-ownership.md)
- [Assets reference](../../knowledge/reference/assets/README.md)

## Core rule

A skinned mesh, a valid Kandra package, a registered KandraRenderer, and a functioning custom armour item are four different claims.
