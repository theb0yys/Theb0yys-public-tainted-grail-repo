# Tainted Weapons Gate 7 Vanilla Visual Comparison Deployment Receipt

Date: 2026-08-09

## Scope

This receipt covers `Tainted Weapons` version `0.3.19`, an integrated read-only comparison capture for visible vanilla two-handed equipped weapon presentation versus the Evil Greatsword framework route.

This is not a visual fix. It does not mutate renderers, cameras, layers, transforms, inventory, combat, animation, saves, or native registration. It exists to prevent another presentation patch before the visible vanilla baseline is captured.

## Evidence before this diagnostic

- Fresh `0.3.18` runtime evidence proved the Evil Greatsword identity route fired with `identityMatch=custom-template-guid`.
- The same run proved `Drake prototype built`, `framework Drake mesh key served`, `framework Drake material key served`, and `equipped runtime Drake ECS view ready`.
- The remaining visual-proof mismatch was downstream of identity/prototype/material loading: `heroTppActive=False`, `handPathClass=fpp`, `renderPathClass=fpp`, `handLayerVisible=False`, `renderLayerVisible=False`, `viewportIn01=False`, and `frustumIntersectsWorldBounds=False`.
- No current evidence captured a visible vanilla two-handed equipped weapon with the same hand/preview owner, FPP/TPP path, layer, camera mask, bounds/frustum, Drake entity transform, and inventory preview route fields.

## Implementation

- Keeps the existing `ItemEquip.OnWeaponLoaded` and `CharacterHandBase.OnMount` hook boundary.
- If the item is not a registered Tainted Weapons record and its template/path matches a vanilla two-handed/greatsword/broadsword candidate, the framework logs:
  - `vanilla equipped visual comparison runtime view`
  - `vanilla equipped visual comparison Drake ECS view`
- The comparison receipts use the existing lifecycle event sink and include template identity, hand path, FPP/TPP classification, hand/render layers, active renderer counts, linked Drake entity roles, material/mesh IDs, world/render bounds, ECS visibility flags, and camera/frustum proof.
- Visual proof now includes all active cameras, not only the selected `Camera.main`/fallback camera.
- The registered Evil Greatsword path remains unchanged except for the additional all-active-camera fields in visual-proof receipts.

## Build

- Command: `dotnet build "mods\tainted-weapons\src\TaintedWeapons.csproj" -c Release -p:FoAGameRoot="<local-path>"`
- Result: passed with 0 warnings and 0 errors on 2026-08-09.
- Built DLL: `mods\tainted-weapons\src\bin\Release\netstandard2.1\TaintedWeapons.dll`
- Built SHA-256: `639659673C293AF96513CB70E63167D32DB57532B9479C613F669B645DA118AE`

## Deployment

- Live destination: `<local-path>`
- Live SHA-256 after copy: `639659673C293AF96513CB70E63167D32DB57532B9479C613F669B645DA118AE`
- Previous `0.3.18` backup: `<local-path>`
- Previous `0.3.18` backup SHA-256: `C8AE4D17C55AD3D8FD21F115D8450A6501C7ECB17AD80CD72B21ED85F7FFA530`
- Plugin scan: one `TaintedWeapons.dll` found under `BepInEx\plugins`.

## Not yet validated

- Fresh FoA startup loading `Tainted Weapons 0.3.19`.
- User-controlled equip/re-equip of Evil Greatsword under `0.3.19`.
- User-controlled equip of a visible vanilla two-handed weapon under `0.3.19`.
- Comparison of vanilla versus Evil hand/preview owner, FPP/TPP path, layer, camera mask, bounds/frustum, Drake entity transform, and inventory preview route.
- Any presentation fix.

## Next validation

After the user manually starts FoA, equip or re-equip Evil Greatsword, then equip a visible vanilla two-handed weapon. Inspect `BepInEx\LogOutput.log` and the newest lifecycle TSV for:

- `Tainted Weapons 0.3.19 loaded`
- Evil `equipped runtime Drake ECS view ready`
- `vanilla equipped visual comparison runtime view`
- `vanilla equipped visual comparison Drake ECS view`
- matching or differing values for `handPathClass`, `renderPathClass`, `handLayer`, `renderLayer`, camera visibility, viewport/frustum, linked entity transform, mesh/material IDs, and bounds
