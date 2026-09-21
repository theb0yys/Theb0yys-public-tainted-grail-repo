# Validation Plan

## Required Level

- Level: build plus live BepInEx log evidence for registered weapon prototype redirection, Drake mesh/material loading, post-conversion Drake ECS readiness, native perspective/path visual proof, lifecycle receipt evidence for W7-W10/W14-W17, and separate explicitly authorized T0-T14 registrar receipts.
- Reason: `0.3.20` patches the equipped weapon prefab route, spawned `CharacterHandBase` runtime boundary, FoA's native inventory/equipment preview clone boundary, and Drake's mesh/material loading-manager route for registered custom templates, reserves canonical weapon identities before registration/equip proceeds, observes Drake unload calls for counter receipts, reloads changed config before lifecycle `RequestId` polling, activates the framework-owned cloned `CharacterHandBase`/Drake presentation hierarchy before native equip instantiation, activates registered spawned hand presentation nodes after `OnWeaponLoaded`/`OnMount`, schedules registered-only post-conversion Drake ECS readiness receipts, completes existing linked renderer entities through native Drake manager mesh/material IDs when available, tags those completed registered renderers with `DrakeRendererManualTag`, retains the exact registered Drake mesh/material keys used by completed renderers, blocks unload calls for retained registered keys, appends focused visual-proof fields to the existing runtime ECS receipt, adds native `Hero.TppActive`, FPP/TPP path classification, hand/render layers, camera culling mask, and camera layer-visibility checks, captures read-only vanilla inventory/equipment preview route evidence, normalizes only registered custom preview clones whose Unity/ECS layer state mismatches the native preview owner layer/mask contract, and must prove normal UI Addressables loads are no longer coerced into `UnityEngine.Material`.

## Build Validation

- Command: `dotnet build mods/tainted-weapons/src/TaintedWeapons.csproj -c Release -p:FoAGameRoot="<local-path>"`
- Result: first `0.3.10` build failed because `Plugin.cs` lacked the `Awaken.TG.Main.Heroes.Combat` import for `CharacterHandBase`; final `0.3.10` build passed with 0 warnings and 0 errors on 2026-08-06. First `0.3.11` build failed on the shipped bounds type/reference boundary (`AABB` and `Unity.Mathematics.Extensions`); final `0.3.11` build passed with 0 warnings and 0 errors on 2026-08-06. First `0.3.12` build failed because writing native `MipmapsMaterialComponent` required an `Awaken.Utility.dll` reference; final `0.3.12` build passed with 0 warnings and 0 errors on 2026-08-06. `0.3.13` build passed with 0 warnings and 0 errors on 2026-08-06. `0.3.14` build passed with 0 warnings and 0 errors on 2026-08-06. `0.3.15` build passed with 0 warnings and 0 errors on 2026-08-06. `0.3.16` build passed with 0 warnings and 0 errors on 2026-08-06. Gate 4 source hardening for collision-safe canonical hash receipts and fail-closed capability states passed build with 0 warnings and 0 errors on 2026-08-08. The Gate 6 native title-screen shutdown repair passed build with 0 warnings and 0 errors on 2026-08-09. The first `0.3.17` integrated registrar build failed on the exact `IAttachmentGroup` namespace, static inspection resolved it to `Awaken.TG.Main.Locations.Setup`, and the targeted rebuild passed with 0 warnings and 0 errors on 2026-08-09. The `0.3.18` visual-identity repair build passed with 0 warnings and 0 errors on 2026-08-09. The `0.3.19` vanilla visual comparison diagnostic build passed with 0 warnings and 0 errors on 2026-08-09. The `0.3.20` inventory/equipment preview route capture and registered preview-owner repair build passed with 0 warnings and 0 errors on 2026-08-09.

## Live DLL Deployment

- Source: `mods/tainted-weapons/src/bin/Release/netstandard2.1/TaintedWeapons.dll`
- Destination: `<local-path>`
- Current source version: `0.3.17.0`
- Current source SHA-256 after Gate 4 source build: `4FD051E8B0279CC60B7F118FF909E4EB68F897B19F83C34E8ABB94511B11EFFF`
- Current live version: not checked by the Gate 4 source-hardening task.
- Current live SHA-256: not checked by the Gate 4 source-hardening task.
- Result: source was built only. Live DLL deployment, source/live hash equality, BepInEx load, and exact runtime identity validation were not run or claimed by the Gate 4 source-hardening task.
- Gate 6 shutdown-repair source/live version: `0.3.16.0`.
- Gate 6 shutdown-repair source/live SHA-256: `1BD36F49F4248C6479C6ADE0C468736534B66DC5025071EFC6CD5953EA93CD80`.
- Gate 6 previous-live backup: `<local-path>`, SHA-256 `B5B6D1F9E0B48E36F01E884A9DFE7A5A333AC0B08EFD82080AC5351D465FFC72`.
- Gate 6 deployment result: FoA process was not found before copy; source/live path containment and SHA-256 equality passed. Plugin load and `native-title-screen-exit` teardown receipt remain pending a manual run and normal user-controlled close.
- Gate 7 registrar-service source/live version: `0.3.17.0`.
- Gate 7 registrar-service source/live SHA-256: `21C1C51C4B879FD5BD0D0B62472B85E15DAD9A78DF4936FE268D8D8FC181C818`.
- Gate 7 previous-live backup: `<local-path>`, SHA-256 `1BD36F49F4248C6479C6ADE0C468736534B66DC5025071EFC6CD5953EA93CD80`.
- Gate 7 deployment result: FoA process was not found before copy; source/live SHA-256 equality passed. The deployed registrar remains inert without an explicit API request. Plugin load, Gate 6 teardown receipt, consumer migration, native registration, and T0-T14 remain unrun and unclaimed.
- Gate 7 visual-identity source/live version: `0.3.18.0`.
- Gate 7 visual-identity source/live SHA-256: `C8AE4D17C55AD3D8FD21F115D8450A6501C7ECB17AD80CD72B21ED85F7FFA530`.
- Gate 7 vanilla-comparison source/live version: `0.3.19.0`.
- Gate 7 vanilla-comparison source/live SHA-256: `639659673C293AF96513CB70E63167D32DB57532B9479C613F669B645DA118AE`.
- Gate 7 inventory-preview-route source/live version: `0.3.20.0`.
- Gate 7 inventory-preview-route source/live SHA-256: `D60E15081EA7E3A5F537D62D40E1103538FCD7063D175E5F2021C9B372BB1816`.
- Gate 7 inventory-preview-route previous-live backup: `<local-path>`, SHA-256 `639659673C293AF96513CB70E63167D32DB57532B9479C613F669B645DA118AE`.
- Gate 7 inventory-preview-route intermediate-live backup before final rebuild redeploy: `<local-path>`, SHA-256 `A20FF8526AF2F932DFAE7A5FF37FC415A9688AEA0CC3316FC2C47B1C036E8416`.
- Gate 7 inventory-preview-route stale-prefinal backup before real source deployment: `<local-path>`, SHA-256 `557491D38739D0E7E054908E0E5DE7E12F7CE4DDD825530CFFF8D99170C5FE42`.
- Gate 7 inventory-preview-route functional pre-safety-string backup before final source-matching deployment: `<local-path>`, SHA-256 `A082ADF4B4523E32B2A9FAA0D27CCD4D7EA178A2D4DB878120410C0FB9B03F41`.
- Gate 7 inventory-preview-route deployment result: FoA process was not found before copy; source/live SHA-256 equality passed; a plugin scan found only one `TaintedWeapons.dll` under `BepInEx\plugins`; fresh user-controlled inventory/equipment preview capture has not run.

## Load Validation

- Game version: `1.25.030`, from Template Diagnostics.
- Branch: local Steam install, exact branch not proven.
- BepInEx version: `5.4.23.3`, recorded in `deployment-receipt-20260806-identity-policy.md`.
- Log evidence: support bundle `20260804-230756Z-9eabf954` showed `0.3.0` was loaded and failed with `InvalidKeyException` on the framework mesh key. Support folder `20260804-232048Z-eed636d8` and live `BepInEx/LogOutput.log` showed `0.3.1` loaded, built the framework Drake prototype, then failed with `InvalidKeyException` on the framework material key after Unity clone/reference load. Support bundle `20260805-004506Z-28419419` showed `0.3.2` loaded, patched four hooks, built the framework Drake prototype, then still failed on the framework mesh/material keys. Live logs `08_05_2026_02_03_44.txt` and `08_05_2026_02_09_57.txt` showed `0.3.3` caused unrelated TextAsset/Sprite/GameObject loads to fail as `Type=UnityEngine.Material`. The `20260805` W1 receipt proved `0.3.5` startup capability and clean-shutdown validation. The `20260806` identity receipt proved `0.3.6` startup, capability, identity-policy registration, and clean shutdown. The `20260806` W6-W11 receipt proved log-level W6 custom Drake route and partial W11 UI isolation.
- `0.3.8` harness note: `lifecycle-validation-harness-20260806.md` documents the default-off W7-W10/W14-W17 receipt harness, observe-only Drake unload counter hooks, and live config reload before `RequestId` polling.
- `0.3.7` deployment receipt: `deployment-receipt-20260806-lifecycle-harness.md` records source/live version and SHA-256 match.
- First report: `weapon-material-probe-20260803-035017.tsv`, scene `BuildInitialScene`, frame `1`, `rendererRows=0`.
- Successful `0.1.0` report: `weapon-material-probe-20260803-233146.tsv`, scene `CampaignMap_HOS`, frame `7440`, `rendererRows=160`, `componentRows=160`, `candidateRoots=46`.
- `0.1.1` report evidence: `weapon-material-probe-20260804-001113.tsv`, scene `CampaignMap_HOS`, `rendererRows=160`, `drakeMaterialRows=9`, `componentRows=160`, `candidateRoots=49`, and `Longsword` hits `0`.

## Feature Validation

- Scenario: equip the candidate weapon, load into gameplay, and wait for the auto-run report.
- Expected: a `weapon-material-probe-*.tsv` report appears under `BepInEx/config/kane.tgfoa.tainted-weapons/` and logs candidate renderer/material/texture rows.
- Actual: first auto-run was too early in `BuildInitialScene`; later run was still too early in `TitleScreen`. The `20260803-233146` gameplay report did find weapon evidence, but not `Longsword`, `DullLongsword`, or `Dull Longsword`.
- `0.3.16` expected framework behavior after deployment: log `Tainted Weapons 0.3.16 loaded`, patch eight framework equipped prototype/runtime/lifecycle hooks, include capability `runtime-hand-lifecycle`, reserve and log an identity receipt for the registered weapon, log that generic AssetReference/Addressables mesh-material hooks are disabled, log `Drake prototype built` with `activatedPresentationNodes` and Drake renderer active-state fields, log `equipped runtime view` from `ItemEquip.OnWeaponLoaded` and/or `CharacterHandBase.OnMount`, log `equipped runtime Drake ECS view ready` or a bounded `waiting`/`not ready`/`terminal` receipt with linked entity details and `ecsCompletion=...manual=True,retention=retained=2`, include `visualProof=` with camera/frustum, ECS visibility/culling, material/shader, world-position, `Hero.TppActive`, FPP/TPP path classification, and camera layer-visibility fields, log `framework Drake mesh key served` and `framework Drake material key served` only for registered Tainted Weapons keys, reload changed plugin config before lifecycle `RequestId` polling, log `retention-blocked` if Drake tries the first retained registered unload, log `underflow-blocked` only if Drake repeats a registered zero-counter unload, and not emit `InvalidKeyException` lines where UI/creator TextAsset/Sprite/GameObject keys are requested as `UnityEngine.Material`.
- `0.3.17` registrar source expectation: startup logs one installed `TemplatesLoader.FinishedLoading` readiness hook, one registrar status with `NativeItemRegistrationAutoInvoke=false`, and no registrar request/receipt unless an explicit API caller submits one. A separately authorized request must emit exactly one final machine-readable registrar receipt and never call `AddToMap` for a denial or idempotent repeat.
- `0.3.18` visual identity expectation: after the registered custom template is equipped or re-equipped, the log should include `Tainted Weapons 0.3.18 loaded`, `equipped visual redirect` with `identityMatch=custom-template-guid` or `identityMatch=custom-template-name`, then either `Drake prototype built` and runtime view/Drake ECS view receipts or a bounded framework-owned fallback/block reason. It must not restore Evil legacy Unity renderer fallback.
- `0.3.19` vanilla comparison expectation: after a user-controlled run equips Evil Greatsword and a visible vanilla two-handed weapon, logs/TSVs should include `Tainted Weapons 0.3.19 loaded`, the Evil `equipped runtime Drake ECS view ready` receipt, and read-only `vanilla equipped visual comparison runtime view` plus `vanilla equipped visual comparison Drake ECS view` receipts. The comparison receipts must not mutate renderer, camera, layer, transform, inventory, combat, animation, saves, or native registrar state.
- `0.3.20` inventory/equipment preview expectation: after a user-controlled run opens the inventory/equipment screen with Evil Greatsword and a visible vanilla two-handed weapon available for comparison, logs/TSVs should include `Tainted Weapons 0.3.20 loaded`, an optional framework capability for `inventory-preview-route`, registered `inventory/equipment preview route` and `inventory/equipment preview Drake ECS view` receipts for Evil Greatsword, and read-only `vanilla inventory/equipment preview route` plus `vanilla inventory/equipment preview Drake ECS view` receipts for the visible vanilla two-handed weapon. Evil Greatsword receipts must show the preview owner path, camera mask, layer visibility, Drake entity transform/bounds, ECS render-filter layer state, and a separate inventory hero-renderer clone classification. Any repair must be limited to the registered custom preview clone's owner layer/mask mismatch and must not mutate vanilla previews, camera masks, transforms, inventory, combat, animation, saves, or native registrar state.
- `0.3.6` observed framework behavior: `Tainted Weapons 0.3.6 loaded`, four framework hooks patched, Evil Greatsword identity receipt reserved, generic AssetReference/Addressables mesh-material hooks disabled, `framework Drake mesh key served`, `framework Drake material key served`, and no `InvalidKeyException` or `Type=UnityEngine.Material` lines in the validation scan.
- UI screenshot/video evidence: not applicable for this probe.

## Release Validation

- Package checked: not applicable for this slice.
- README/changelog/manifest aligned: README and docs only.
- Known limitations documented: yes.

## Not Run

- `0.3.16` live startup/equip runtime-view and Drake ECS visual-proof validation after game restart.
- `0.3.18` live startup/equip visual-identity validation after user-controlled FoA restart.
- `0.3.19` live startup/equip vanilla two-handed visual comparison after user-controlled FoA restart.
- `0.3.20` live startup/inventory-equipment preview route validation after user-controlled FoA restart.
- Gate 4 source hardening live deployment, BepInEx load, source/live hash equality, and exact runtime identity validation.
- Full W7-W10 lifecycle fixture validation through `lifecycle-validation-*.tsv` receipts.
- W14-W17 mesh/material counter, bundle lifetime, and plugin teardown validation through lifecycle receipts.
- Fresh normal title-screen exit validation proving one `native-title-screen-exit` before/after teardown receipt from the repaired framework lifecycle path.
- Any `0.3.17` live native registration request or `TemplatesLoader.AddToMap` invocation.
- Gate 7 Evil Greatsword consumer migration.
- T0-T14 native registrar validation, MVC item creation, copied-save timing, and missing-registrar behavior.
- Full W11 creator, inventory, map, and menu screen coverage.
- In-game visual acceptance screenshot/video validation.
