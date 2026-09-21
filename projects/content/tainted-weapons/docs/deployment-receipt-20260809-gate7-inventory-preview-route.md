# Gate 7 Inventory Preview Route Deployment Receipt

## Scope

- Task: capture the inventory/equipment preview route for a visible vanilla two-handed weapon and Evil Greatsword, then patch only the proven framework-owned mismatch.
- Framework version: `0.3.20`.
- Live destination authorized by the user: `<local-path>`.
- User controls FoA startup, gameplay, and shutdown. Codex may inspect logs and TSVs only.

## Evidence Boundary

- Static inspection of `TG.Main.dll` identified the inventory/equipment preview owner route as `HeroRenderer`/`VHeroRenderer` plus `CustomHeroClothes`, not the live FPP/TPP hand.
- The native preview boundary is `CharacterHandBase.AttachToCustomHeroClothes(CustomHeroClothes, ItemEquip)`.
- The implemented framework hook records preview owner path, camera mask, render layer visibility, Drake transform/bounds, ECS render-filter layer state, and whether the route is the separate inventory hero-renderer clone.
- Vanilla two-handed preview capture remains read-only.
- Registered Evil Greatsword preview repair is limited to a Unity/ECS layer/mask mismatch against the native `CustomHeroClothes` owner contract.

## Build

- Command: `dotnet build mods\tainted-weapons\src\TaintedWeapons.csproj -c Release -p:FoAGameRoot="<local-path>"`
- Result: passed with 0 warnings and 0 errors on 2026-08-09.
- Built DLL: `<local-path>`.

## Deployment

- Source SHA-256: `D60E15081EA7E3A5F537D62D40E1103538FCD7063D175E5F2021C9B372BB1816`.
- Live SHA-256: `D60E15081EA7E3A5F537D62D40E1103538FCD7063D175E5F2021C9B372BB1816`.
- Source length: `256000`.
- Live length: `256000`.
- Source/live freshness: passed.
- Previous live backup: `<local-path>`.
- Previous live backup SHA-256: `639659673C293AF96513CB70E63167D32DB57532B9479C613F669B645DA118AE`.
- Intermediate `0.3.20` live backup before final rebuild redeploy: `<local-path>`.
- Intermediate `0.3.20` live backup SHA-256: `A20FF8526AF2F932DFAE7A5FF37FC415A9688AEA0CC3316FC2C47B1C036E8416`.
- Stale pre-fix `0.3.20` live backup before real source deployment: `<local-path>`.
- Stale pre-fix `0.3.20` live backup SHA-256: `557491D38739D0E7E054908E0E5DE7E12F7CE4DDD825530CFFF8D99170C5FE42`.
- Functional pre-safety-string `0.3.20` live backup before final source-matching deployment: `<local-path>`.
- Functional pre-safety-string `0.3.20` live backup SHA-256: `A082ADF4B4523E32B2A9FAA0D27CCD4D7EA178A2D4DB878120410C0FB9B03F41`.
- Plugin duplicate scan: found one `TaintedWeapons.dll` under `BepInEx\plugins` after deployment.

## Not Yet Validated

- FoA has not been manually restarted after this deployment.
- Runtime receipt capture for `Tainted Weapons 0.3.20 loaded` has not run.
- Runtime inventory/equipment preview receipts for Evil Greatsword and a visible vanilla two-handed weapon have not run.
- User visual confirmation has not run.

## Next Runtime Evidence

Expected live validation after the user manually starts FoA, opens inventory/equipment, compares Evil Greatsword against a visible vanilla two-handed weapon, and closes FoA normally:

- `Tainted Weapons 0.3.20 loaded`.
- Framework capability line includes `inventory-preview-route`.
- Evil Greatsword `inventory/equipment preview route` receipt.
- Evil Greatsword `inventory/equipment preview Drake ECS view` receipt.
- Visible vanilla two-handed `vanilla inventory/equipment preview route` receipt.
- Visible vanilla two-handed `vanilla inventory/equipment preview Drake ECS view` receipt.
- No camera-mask mutation, transform mutation, save mutation, inventory mutation, combat/animation mutation, native registration invocation, or direct Unity renderer fallback.
