# Armour and Native Clothing

Use this page when you are trying to build or integrate **custom wearable armour/clothing**, especially where Kandra deformation is involved.

The key rule is:

> A skinned mesh loading successfully is only one step. It is not proof of a complete FoA armour item.

## The full problem

A custom armour path can involve:

~~~text
source geometry
→ import/canonical representation
→ deformation compatibility
→ Kandra payload/renderer registration
→ item/equipment definition
→ native clothes/equip lifecycle
→ Kandra stitching/presentation
→ unequip cleanup
→ persistence
~~~

Treat each step separately.

## Native clothing owners

Static evidence identifies:

- `ItemTemplate` — armour/equipment definition;
- `BaseClothes` — native clothing presentation/equip owner;
- `KandraRig` — character rig;
- `ClothStitcher` — native stitching path;
- `KandraRenderer` — clothing/character render representation;
- `KandraRendererManager` — runtime Kandra registration.

Useful methods/surfaces include:

- `BaseClothes.EquipTask`
- `BaseClothes.Equip`
- `BaseClothes.Unequip`
- `BaseClothes.SafeUnequip`
- `ClothStitcher.Stitch(GameObject, KandraRig)`
- `KandraRenderer.RedirectToRig`
- `KandraRenderer.OnEnable()`
- `KandraRendererManager.Register(...)`
- `KandraRendererManager.IsRegistered`

## Native clothes lifecycle

The inspected shape is:

~~~text
equipped armour item
→ BaseClothes loads/resolves clothing asset
→ target KandraRig available
→ ClothStitcher.Stitch(...)
→ Kandra renderer redirected/stiched to rig
→ clothing remains active

unequip
→ native clothes cleanup
→ stitched clothing destroyed/released
→ asset reference released
~~~

A production custom armour route needs to work with this lifecycle, not only with a standalone renderer proof.

## Prove deformation before equip

Before integrating with a live armour item, prove the source geometry can deform correctly on the target rig.

A mesh can be structurally accepted and still:

- collapse;
- invert triangles;
- clip badly;
- bind to the wrong bones;
- deform incorrectly.

Registration is not visual correctness.

## Separate conversion from runtime ownership

Importer/conversion tooling should be able to produce and validate canonical geometry/package data without creating live FoA objects.

Then test runtime registration separately.

That keeps format/conversion bugs separate from live equip/lifecycle bugs.

## Prove Kandra registration separately

A structurally valid Kandra package is not the same thing as a `KandraRenderer` successfully registering in the live game.

Likewise, a registered proof renderer is not yet an armour item.

## Let native clothes own actual equip

When the target armour is ready, integrate through:

~~~text
BaseClothes
→ ClothStitcher
→ KandraRig / KandraRenderer
~~~

Do not stop at "the renderer appeared."

## Preserve unequip/release

A correct armour integration must also:

- detach/unstitch cleanly;
- destroy/release owned presentation;
- survive re-equip;
- avoid leaked renderer/mesh registrations.

## Common mistakes

### "Kandra registered, therefore armour works"

False. Registration proves only that part of the renderer path.

### "I have a skinned Unity mesh, so I have native clothing"

FoA clothing also has native equip/stitch/cleanup ownership.

### Editing native archives as a shortcut

Current work deliberately uses mod-owned proof packages and gated registration instead of mutating native archives.

### Skipping deformation validation

A package can load and still render incorrectly.

### Reusing a proof fixture as the production item

Proof identity/payload does not establish final armour semantics.

## What to verify

A complete armour proof eventually needs:

1. source/provenance;
2. target body/rig;
3. deformation compatibility;
4. Kandra payload/metadata consistency;
5. live Kandra registration;
6. visual A/B check;
7. custom item/equipment identity;
8. native `BaseClothes` equip;
9. stitch/rig correctness;
10. body-cover/culling/material behavior;
11. unequip/re-equip cleanup;
12. scene/load transitions;
13. save/load and missing-mod behavior if claimed.

## Evidence limits

Proven in current work:

- importer/tooling infrastructure;
- real geometry fixtures;
- substantial Kandra package/registration contract recovery;
- guarded live host registration for a proof mesh.

Static native contract:

- `BaseClothes` → `ClothStitcher` → Kandra rig/renderer ownership.

Not yet a universal custom-armour process:

- production target conversion;
- final item/equip integration;
- deformation acceptance across armour families;
- persistence;
- uninstall/migration;
- universal Kandra writer semantics.
