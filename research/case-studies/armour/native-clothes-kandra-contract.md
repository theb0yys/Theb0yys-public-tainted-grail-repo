# Native Clothes/Kandra Static Contract

Armour research corrected a common assumption: “skinned mesh” is not the complete native integration contract.

The inspected native route uses `BaseClothes`, an equipped cloth asset reference, `ClothStitcher`, `KandraRig` and `KandraRenderer.RedirectToRig`.

The key lesson is that a custom cloth asset must satisfy the **native Kandra presentation contract**, not merely render correctly in an isolated Unity scene.

This case remains static evidence until a custom armour runtime equip/deformation/unequip sequence is proven.
