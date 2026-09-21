# Armour Stage 6 — Item Identity and Native Clothes/Equip

## Objective

Connect the validated armour presentation to a real custom item/equipment identity and let FoA's native clothing owner perform equip/stitch lifecycle work.

## Native chain

    registered custom ItemTemplate
    → normal Item/equipment ownership
    → BaseClothes
    → target KandraRig
    → clothing asset/reference
    → ClothStitcher.Stitch
    → Kandra renderer redirect/stitch
    → native clothing presentation

Unequip must return through BaseClothes cleanup and resource release.

## Procedure

1. Create/register a separate custom ItemTemplate using the proven item-registration discipline.
2. Preserve the reviewed native armour/equipment attachments needed by the selected archetype.
3. Point the custom clothing presentation to the validated package/resource identity.
4. Equip through the normal equipment path.
5. Confirm BaseClothes owns the operation.
6. Confirm a valid target KandraRig is used.
7. Confirm ClothStitcher / renderer redirection performs the presentation handoff.
8. Unequip and verify native cleanup/release.

## Validation gate

PASSED only when the custom logical armour item and the Kandra presentation meet inside the native clothes/equip owner and unequip reverses the ownership correctly.

## Does not prove

One equip does not prove deformation under motion, body-cover/culling/material correctness, repeated equip, scenes, persistence, or another body/armour family.

## Next

Proceed to [runtime validation](../07-runtime-validation/README.md).
