# Armour, Clothes, Kandra, and Native Equip Lifecycle

> **Reference page.** Use this when investigating wearable armour, custom cloth visuals, equipment slots, or Kandra-backed character presentation.

## What this system is

A FoA armour item crosses at least two distinct systems:

~~~text
ItemTemplate / equipment identity
→ native item/equip state
→ clothes/equipment presentation owner
→ Kandra rig/stitch/render lifecycle
→ unequip teardown
~~~

A skinned mesh loading successfully is not enough to prove custom armour integration.

## Who owns it in FoA

Important researched owners include:

- `ItemTemplate` and equipment attachments for the logical item;
- native equip/inventory owners;
- `BaseClothes` for clothes/equip presentation lifecycle;
- `ClothStitcher` for stitching a clothing GameObject onto a Kandra rig;
- `KandraRig`;
- `KandraRenderer`;
- native asset-reference/load/release ownership.

## Important identities, types, and methods

Static native-contract research identified:

- `BaseClothes.EquipTask`
- `BaseClothes.Equip`
- `BaseClothes.Unequip`
- `BaseClothes.SafeUnequip`
- `ClothStitcher.Stitch(GameObject, KandraRig)`
- `KandraRenderer.RedirectToRig`

The native clothes path loads an equipment visual through an asset reference, stitches it to the character rig, redirects Kandra rendering state, and tears the stitched representation down on unequip.

## Where it exists in the lifecycle

A simplified native clothes path is:

~~~text
armour Item equipped
→ native equip owner selects cloth representation
→ asset reference loads GameObject
→ BaseClothes / ClothStitcher binds to KandraRig
→ KandraRenderer redirects/uses target rig
→ item remains equipped
→ unequip
→ stitched representation destroyed
→ asset/reference released
~~~

The item save/equipment state and the rendered cloth lifetime are separate concerns.

## How we interact with it

### Prove the logical item before custom deformation/presentation

First establish:

- stable item identity;
- equipment slot/classification;
- item registration/acquisition if custom;
- native equip state.

Then tackle the clothes/Kandra presentation layer.

### Reuse the native clothes lifecycle

If a custom visual is meant to behave like native armour, it should fit the same:

- load;
- stitch;
- rig redirect;
- visibility/equip;
- unequip;
- cleanup;

sequence.

### Treat Kandra as a real rendering system

Kandra is not a synonym for `SkinnedMeshRenderer`.

The researched architecture includes:

- Kandra mesh metadata;
- packed mesh/index resources;
- rig mapping;
- GPU deformation;
- material brokerage;
- culling;
- visibility;
- mipmap integration.

### Keep body culling separate

Native clothing can carry precomputed body-triangle culling relationships.

A cloth visually fitting the body does not prove body culling is correct.

## Why this route

The native decompilation shows the game already has a clothes-specific owner and Kandra stitching route.

That means a direct Unity fallback that simply parents a skinned mesh to the character can bypass:

- Kandra rig mapping;
- native clothes lifecycle;
- culling;
- visibility;
- cleanup;
- preview/equipment ownership.

The research therefore treats native clothes/Kandra integration as the target, not raw Unity attachment.

## What goes wrong

### "Armour is just an ItemTemplate with a mesh"

The item identity is only one layer.

### Arbitrary FBX → Kandra assumed possible

The public Questline source exposes Kandra shell/metadata and archive/build surfaces, but the production converter/serializer that generates full Kandra packed mesh data is not available.

That means:

**blocked:** reliably generating arbitrary new production Kandra mesh payloads from an FBX using only the currently recovered public tooling.

### Hand-authoring Kandra metadata only

Insufficient. Runtime Kandra depends on streamed packed vertex/index/blendshape/bind-pose data and manager/resource state.

### Direct SkinnedMeshRenderer fallback called equivalent

No proven production equivalence to Kandra.

### Unequip without native cleanup

Can leave stitched objects/resources or renderer state behind.

### Item equip proof called deformation proof

An item can equip logically while the visual fails or deforms incorrectly.

## How to verify

A custom armour path should eventually prove:

1. custom/native item identity;
2. equipment slot and native classification;
3. equip event/state;
4. exact visual asset/reference;
5. correct rig target;
6. cloth stitch succeeds;
7. Kandra renderer redirects correctly;
8. pose/deformation during movement/animation;
9. body culling where required;
10. inventory/equipment preview;
11. unequip/re-equip;
12. repeated cycles;
13. scene transition;
14. resource cleanup;
15. save/load equipped and unequipped if durable.

## Current proof boundary

The **native armour/clothes/Kandra lifecycle is statically well mapped**.

A generic custom-armour runtime process—especially arbitrary new Kandra mesh generation—is **not yet proven**. The handbook therefore teaches the real owner/lifecycle and the blockers instead of inventing an authoring recipe.
