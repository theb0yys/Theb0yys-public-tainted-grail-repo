# Armour Pipeline Failure Catalogue

## Mesh loads in Unity but fails later

Unity/skinned-mesh load is only a source/import check. Re-open geometry mapping, deformation, Kandra conversion, or native ownership according to the earliest failed gate.

## Package validates but renderer will not register

The writer/validator lane passed; runtime metadata/registration did not. Check the exact registration candidate, runtime owner expectations, and package metadata.

## Renderer registers but armour cannot equip

Registration is not item/equipment integration. Check custom ItemTemplate registration, BaseClothes ownership, clothing reference, target KandraRig, and ClothStitcher path.

## Armour appears but deforms badly

Do not patch around it in runtime. Return to source/target mapping and deformation validation.

## Armour appears but body/material/culling is wrong

Treat body-cover, material, culling, LOD, and renderer state as distinct runtime validation rows.

## Unequip leaves geometry behind

Native cleanup/resource ownership failed. Verify BaseClothes unequip, stitched renderer lifetime, and package/resource release.

## Works until restart

Persistence/registration ordering and package availability were not proven. Run the cold-load stage.

## Proof fixture works but production armour fails

Do not promote proof-fixture registration to production-armour completion. Re-run every target-specific stage using the production identity and geometry.
