# Armour Pipeline Validation Matrix

| Gate | Required proof |
| --- | --- |
| Source/provenance | Exact authorised source revision recorded |
| Source geometry | Deterministic geometry/skeleton facts captured |
| Target contract | Exact target rig/body/build scope recorded |
| Bone/weight mapping | Required influences accounted for explicitly |
| Deformation | Representative poses pass acceptance criteria |
| Kandra conversion | Canonical/packed output generated from the accepted candidate |
| Package validation | Writer output re-read and structurally validated |
| Registration preflight | Metadata matches the exact package candidate |
| Runtime registration | Kandra runtime owner recognises the exact candidate |
| Item identity | Separate custom ItemTemplate resolves normally |
| Native clothes/equip | BaseClothes / ClothStitcher / target KandraRig path observed |
| Runtime visuals | Material, culling, body-cover, LOD, and deformation rows tested |
| Unequip/re-equip | Native cleanup and resource release pass repeated cycles |
| Scene transitions | Worn state/resources recover correctly if claimed |
| Persistence | Cold save/load if claimed |
| Missing package | Explicit disabled/missing behaviour if claimed |
| Migration | Upgrade path if claimed |
| Compatibility | Claimed game/runtime combinations tested |

A package validating does not satisfy runtime registration. Runtime registration does not satisfy item/equip. A visible worn mesh does not satisfy persistence.
