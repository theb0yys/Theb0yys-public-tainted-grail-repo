# Tainted Weapons Gate 7 Vanilla Comparison Check Receipt

Date: 2026-08-09

## Scope

This receipt records the read-only `0.3.19` live comparison check requested after the user-controlled FoA run. It does not authorize or perform a visual patch.

## Runtime state

- FoA process observed running: `<local-path>`.
- BepInEx log path: `<local-path>`.
- Tainted Weapons loaded: `Tainted Weapons 0.3.19 loaded`.
- Evil native registrar final receipt observed: `status=template_registered`, `registered=True`, `addToMapInvocationCount=1`, `nativeMutationOccurred=True`.

## Vanilla player two-handed baseline

Template:

- `ItemTemplate_Weapon_2H_Sword_Heavy_Tier0_DullBroadsword`
- GUID: `43114d8790291d647a6291f002227513`

Runtime view:

- Route: `CharacterHandBase.OnMount` and `ItemEquip.OnWeaponLoaded`
- Hand path: `/HeroSpawned/Hero:0[VHeroController]/FppParent/FppArms(Clone)/.../RightHandSlot/MainHand/Equipable_Weapon_2H_Sword_T1_Broadsword(Clone)`
- Path class: `handPathClass=fpp`, `renderPathClass=fpp`
- Layers: `handLayer=2`, `renderLayer=2`

Drake ECS view:

- Ready: `True`
- Attempts: ready at `attempt=4`, `waitedFrames=45`
- Linked entities: `linkedEntityCount=2`
- Group/render enabled: `groupEnabled=True`, `renderEnabled=True`
- Mesh/material IDs: `meshId=1636`, `materialId=471`
- Ready components: `readyComponents=True`
- Transitional tags: `False`
- Mesh/material source: native non-framework keys, both loaded.
- Camera: `activeCameraCount=1`, `cullingMask=-1342276129`
- Layer visibility: `handLayerVisible=True`, `renderLayerVisible=True`
- Frustum: `frustumIntersectsWorldBounds=True`
- Viewport: `viewportIn01=False`

## Evil Greatsword framework path

Template:

- `ItemTemplate_Mod_EvilGreatsword`
- GUID: `e6e91100000000000000000000000001`

Runtime view:

- Route: `CharacterHandBase.OnMount` and `ItemEquip.OnWeaponLoaded`
- Hand path: `/HeroSpawned/Hero:0[VHeroController]/FppParent/FppArms(Clone)/.../RightHandSlot/MainHand/TaintedWeapons_evil-greatsword_b0b9f30f0ec1_RuntimeEquippedPrototype(Clone)`
- Path class: `handPathClass=fpp`, `renderPathClass=fpp`
- Layers: `handLayer=2`, `renderLayer=2`

Drake ECS view:

- Ready: `True`
- Attempts: ready at `attempt=0`, `waitedFrames=1`
- Linked entities: `linkedEntityCount=2`
- Group/render enabled: `groupEnabled=True`, `renderEnabled=True`
- Mesh/material IDs: `meshId=1647`, `materialId=460`
- Ready components: `readyComponents=True`
- Transitional tags: `False`
- Mesh/material source: registered framework keys, both loaded and retained.
- Camera: `activeCameraCount=1`, `cullingMask=32`
- Layer visibility: `handLayerVisible=False`, `renderLayerVisible=False`
- Frustum: `frustumIntersectsWorldBounds=False`
- Viewport: `viewportIn01=False`

## Comparison result

Proven matches:

- Both player paths use the FPP hand route.
- Both use `handLayer=2` and `renderLayer=2`.
- Both create the linked Drake entity pair.
- Both reach ready ECS state with non-zero mesh/material IDs.
- Both have loaded mesh/material handles.

Proven differences:

- Vanilla Dull Broadsword was captured with `cullingMask=-1342276129`, which includes layer `2`; Evil Greatsword was captured with `cullingMask=32`, which does not include layer `2`.
- Vanilla Dull Broadsword had `handLayerVisible=True`, `renderLayerVisible=True`, and `frustumIntersectsWorldBounds=True`; Evil Greatsword had all three as `False`.
- Vanilla reached ready at `attempt=4`; Evil reached ready at `attempt=0`. Timing differs, but the current evidence does not prove timing is causal.

## Patch status

Blocked.

The comparison proves a camera/layer/frustum mismatch in the captured runtime state, but it does not yet prove the inventory/equipment preview owner route shown in the user's screenshot. A framework patch that mutates render layers, camera masks, transforms, or preview ownership would still be inferred rather than proven.

## Required unblock evidence

Capture the inventory/equipment preview route for both:

- a visible vanilla two-handed weapon, and
- Evil Greatsword.

Required fields:

- preview owner/object path,
- camera name and culling mask,
- hand/preview path class,
- render layer and layer visibility,
- Drake entity transform and bounds,
- whether the preview uses the equipped FPP hand, a TPP body path, or a separate inventory preview clone.
