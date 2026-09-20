# Domain Packet — Armour

## Subject

Armour/equipment identity, source geometry, deformation compatibility, Kandra payload/registration, native `BaseClothes` equip/stitch lifecycle, teardown and persistence.

## Reader questions

1. What is the difference between a valid source mesh, a valid Kandra payload, a registered renderer, and equipped armour?
2. Who owns the native clothing/equip lifecycle?
3. What contract does Kandra stitching require?
4. Why is deformation a separate proof problem?
5. What remains unproven for a general armour importer?

## Archetypes required

- **System hub:** armour/equipment and Kandra ownership map.
- **System:** native clothes/Kandra stitching lifecycle.
- **Mechanic:** staged custom-armour integration.
- **Investigation/case:** registration/deformation contract corrections.
- **Reference:** Kandra/proprietary rendering terms where already public.
- **Learning journey:** armour after item/equipment fundamentals.

## Canonical native owners

- `ItemTemplate` — logical armour/equipment definition.
- `BaseClothes` — native clothing presentation/equip owner.
- `KandraRig` — target rig owner.
- `ClothStitcher` — native stitching path.
- `KandraRenderer` — clothing/character render representation.
- `KandraRendererManager` — runtime registration owner.

## Required prerequisites

- [Items packet](items.md)
- [Native object ownership](../../../systems/core/native-object-ownership.md)
- current public proprietary-rendering documentation
- [Saving and persistence](../../../systems/world/saving-persistence.md)

## Public source set

- `docs/reference/ARMOUR.md`
- `docs/reference/ARMOUR_KANDRA_LIFECYCLE.md` (legacy shim only)
- `docs/reference/PROPRIETARY_RENDERING_SYSTEMS.md`
- public asset/resource-lifetime pages.

## Private-evidence conclusions to preserve

- native `BaseClothes` loads clothing assets and owns equip/unequip;
- `ClothStitcher.Stitch(GameObject, KandraRig)` is the native stitching route;
- Kandra renderers redirect/merge against the target rig;
- native unequip destroys/releases stitched clothing/resources;
- ordinary `SkinnedMeshRenderer` presence is not the native Kandra clothing contract;
- package/renderer registration, deformation acceptance, item/equip integration and persistence are different evidence lanes;
- some Kandra registration proof exists, but that does not establish a production-ready custom armour item.

## Claims to teach

- “mesh loads” is not “armour integrates”;
- “Kandra registers” is not “armour equips”;
- deformation compatibility must be demonstrated separately;
- framework/importer validation must remain separate from native equip ownership;
- cleanup/release matters on unequip/re-equip;
- persistence/missing-mod behaviour remains separate.

## Failure/correction history to preserve

- generic HDRP/ordinary skinned-mesh assumptions do not substitute for Kandra contracts;
- proof fixtures must not be promoted to production armour identities;
- invalid assumptions about target-rig shape/count can reject valid native states;
- registration acceptance can coexist with bad deformation;
- a visually attached mesh can still bypass native clothes ownership.

## Required diagrams

1. **Ownership:** item/equip definition → `BaseClothes` → `ClothStitcher` → `KandraRig/KandraRenderer`.
2. **Pipeline:** source geometry → deformation proof → package/registration → native equip/stitch → cleanup.
3. **Evidence:** registration, visual/deformation, equip, persistence as separate lanes.

## Worked-example candidate

A future public case should focus on “registration is not equip/deformation”, not expose private armour source.

## Evidence lanes

- native clothes/Kandra lifecycle: static/native strong;
- Kandra package/registration: substantial;
- production target armour equip/deformation: partial;
- persistence/uninstall/migration: not general public proof.

## Canonical target map

- `systems/armour/README.md`
- `systems/armour/kandra-clothes-lifecycle.md`
- `mechanics/armour/custom-armour-integration.md`
- `learn/content-authoring/armour/README.md`
- proprietary renderer detail remains linked from existing canonical reference/system material until separately migrated.

## Completion checks

- native equip/stitch lifecycle is canonical and separate from importer process;
- staged mechanic does not claim production armour completion;
- deformation and registration are not collapsed;
- cleanup and persistence boundaries remain explicit.
