<!-- Canonical mechanic. Migrated from docs/reference/ARMOUR.md. -->
# Armour: Native Clothes, Kandra, and Importer Boundary

> **Reference/process page.** The current armour work has significant real implementation and Kandra runtime-registration evidence, but **full custom armour equip/deformation/persistence is not yet a general proven process**.

## What this system is

Custom armour crosses several separate problems:

~~~text
source geometry
→ canonical/import representation
→ deformation/compatibility proof
→ Kandra payload/renderer registration
→ item/equipment definition
→ native clothes/equip lifecycle
→ Kandra stitching/presentation
→ unequip cleanup
→ persistence
~~~

A skinned mesh loading successfully proves only one part.

## Who owns it in FoA

Static native evidence identifies:

- `ItemTemplate` — logical armour/equipment definition;
- `BaseClothes` — native clothing presentation/equip owner;
- `KandraRig` — target character rig owner;
- `ClothStitcher` — native stitching path;
- `KandraRenderer` — character/clothing render representation;
- `KandraRendererManager` — runtime Kandra registration owner.

Current Kandra inspection identifies normal renderer registration entry through `KandraRenderer.OnEnable()`, with manager-side finalisation in its update lifecycle.

## Important identities, types, and methods

Native/static surfaces include:

- `BaseClothes.EquipTask`
- `BaseClothes.Equip`
- `BaseClothes.Unequip`
- `BaseClothes.SafeUnequip`
- `ClothStitcher.Stitch(GameObject, KandraRig)`
- `KandraRenderer.RedirectToRig`
- `KandraRendererManager.Register(KandraRenderer)`
- `KandraRenderer.OnEnable()`
- manager-side `FinalizeRegistration()`
- `KandraRendererManager.IsRegistered`
- Kandra mesh-memory lookup/verification surfaces

## Where it exists in the lifecycle

### Native clothes path

The decompiled native shape is:

~~~text
equipped armour item
→ BaseClothes resolves/loads clothing asset
→ target KandraRig is available
→ ClothStitcher.Stitch(...)
→ Kandra renderer is redirected/stiched to target rig
→ native clothing presentation remains active

unequip
→ native clothes cleanup
→ stitched clothing destroyed/released
→ asset reference released
~~~

### Current custom Kandra research path

The standalone Tainted Armour importer has progressively separated:

~~~text
source/import
→ deformation validation
→ canonical package
→ loose Kandra package writer
→ package validation
→ registration preflight
→ registration candidate
→ runtime registration dry run
→ explicit invocation approval
→ FoA host registration proof
→ same-mesh A/B decode/visual proof
→ later target conversion/equip
~~~

The first accepted live host-registration proof is a proof mesh, **not a production custom armour item**.

## How we interact with it

### 1. Treat deformation as a first-class gate

Before runtime equip, prove that source geometry can actually deform correctly on the target rig.

### 2. Keep conversion/provider logic separate from FoA runtime ownership

The importer core should not need to create live FoA objects merely to define canonical geometry/compatibility.

### 3. Prove the Kandra registration contract separately

A custom Kandra package being structurally valid is not the same as a runtime `KandraRenderer` successfully registering.

### 4. Use the native clothes owner for actual equipping

When target armour is ready, the native `BaseClothes` / `ClothStitcher` / Kandra lifecycle is the owner to integrate with.

### 5. Preserve unequip/release behavior

Do not stop after "it appears on the body."

## Why this route

The armour research found several distinct failure classes that cannot be solved by one generic "import mesh" step:

- geometry may be incompatible with the target rig;
- packed Kandra representation may be wrong;
- registration metadata may not match payload;
- renderer registration may fail;
- a registered renderer may still not be a valid equipped clothing item;
- deformation can look wrong even if bytes/register calls are accepted;
- unequip/cleanup and save state are separate.

The staged importer exists to prevent one pass from being mistaken for all of them.

## What goes wrong

### Treating Kandra registration as armour integration

A registered proof renderer is not a custom armour item.

### Treating a skinned Unity mesh as native clothing

Native clothes include rig/stitch/renderer ownership and teardown.

### Mutating native game archives directly

Current work deliberately uses mod-owned/loose proof packages and explicit gates rather than patching native Kandra archives as a shortcut.

### Skipping deformation validation

A mesh can register and still collapse, reverse triangles, clip badly or bind incorrectly.

### Reusing proof identity/payload as a target item

The current gates explicitly reject using the proof fixture as if it were a target armour package.

## How to verify

A complete custom-armour process eventually needs:

1. source/provenance and geometry capture;
2. target body/rig identity;
3. deterministic deformation/compatibility result;
4. Kandra payload/metadata consistency;
5. runtime Kandra registration;
6. visual A/B evidence;
7. custom item/equipment identity;
8. native BaseClothes equip;
9. stitch/rig correctness;
10. body-cover/culling/material correctness;
11. unequip/re-equip cleanup;
12. scene/load transitions;
13. save/load and missing-mod behavior.

## Current proof boundary

**Proven in current work:** importer pipeline infrastructure, real geometry fixtures, substantial Kandra package/registration contract recovery, and at least one guarded live Kandra host-registration proof for a proof mesh.

**Static native contract:** BaseClothes → ClothStitcher → Kandra rig/renderer equip/unequip ownership.

**Not yet a general public custom-armour process:** production target conversion, target armour runtime registration, item/equip integration, deformation acceptance across real armour families, persistence, uninstall/migration, or universal Kandra writer semantics.

The public rule is therefore: **do not teach "make a skinned mesh and equip it." Teach the staged ownership/proof problem.**
