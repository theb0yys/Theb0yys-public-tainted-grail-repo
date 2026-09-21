# Tainted Weapons 0.3.7 Lifecycle Harness Deployment Receipt

## Scope

- Date: 2026-08-06
- Receipt type: `TaintedWeaponsDeploymentReceipt`
- Purpose: exact `0.3.7` deployment for the W7-W10/W14-W17 lifecycle validation harness.
- Status: built, deployed, hash-verified, live `LifecycleValidation` config enabled, and live `0.3.7` startup verified; lifecycle fixture validation pending.

## Evidence Basis

- Weapons report line 184 requires `TaintedWeaponsLifecycleAndTeardownReport` snapshots for prototype objects, bundle owners, Addressables handles, mesh/material records, loading counters, and active consumers.
- Validation matrix lines 172-183 define W7-W10 and W14-W17 lifecycle/counter gates.
- Tainted Weapons research line 50 requires the user to start or restart the game manually; Codex must not control the live game process.

## Repository And Build

- Repository SHA: `b563573fa556b4d2b4a1b97d07d9f667f6a8b5aa`
- Built project: `mods/tainted-weapons/src/TaintedWeapons.csproj`
- Build command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Build result: passed, 0 warnings, 0 errors.

## Runtime Environment

- Game path: `<local-path>`
- Runtime family: FoA Mono / BepInEx 5
- BepInEx file version: `5.4.23.3`
- BepInEx product version: `5.4.23.3+cd593c47566adfe971f1d96225fa82d1864ae877`
- Deployment process guard: no `Fall`, `Avalon`, `Tainted`, or game-root `UnityCrashHandler64` process was found before copy.

## Deployed Artifact

- Source DLL: `<local-path>`
- Live DLL: `<local-path>`
- Source DLL file version: `0.3.7.0`
- Live DLL file version: `0.3.7.0`
- Source DLL product version: `0.3.7+b563573fa556b4d2b4a1b97d07d9f667f6a8b5aa`
- Live DLL product version: `0.3.7+b563573fa556b4d2b4a1b97d07d9f667f6a8b5aa`
- Source SHA-256: `6DF81E0F98152AF89703EFD18C0AB8D9F0C6C10D598D76FE8033E79A78F777E1`
- Live SHA-256: `6DF81E0F98152AF89703EFD18C0AB8D9F0C6C10D598D76FE8033E79A78F777E1`
- Hash verdict: pass.

## Expected Live Startup Evidence

- `Tainted Weapons 0.3.7 loaded`.
- `Tainted Weapons: patched 6 framework equipped prototype hooks`.
- Capability receipt `ready=True`.
- Evil Greatsword registration accepted with identity receipt.
- No global UI Addressables coercion errors.

## Live Startup Validation

- Checked log: `<local-path>`
- Log last write: `2026-08-06 12:26:14 +01:00`.
- Observed running process during check: `Fall of Avalon.exe`.
- Observed startup: `Tainted Weapons 0.3.7 loaded`.
- Observed patch count: `Tainted Weapons: patched 6 framework equipped prototype hooks`.
- Observed lifecycle config in startup log: `LifecycleValidation.Enabled=True`, `LifecycleValidation.RequestId=0`, `LifecycleValidation.WriteOnSceneChange=True`, `LifecycleValidation.WriteOnShutdown=True`.
- Observed capability receipt: `ready=True`; all mandatory capabilities available; optional `multi-renderer` remains unavailable with reason `not-implemented-current-single-drake-renderer-path`.
- Observed Evil Greatsword framework registration: `accepted=True`, `assetResolved=True`, `drakeReady=True`, `identityReservation=reserved`, `customVisualReady=True`.
- Observed Evil Greatsword native item registration later in startup: `custom weapon item registered`.
- Negative scan: no matches for `InvalidKeyException`, `Type=UnityEngine.Material`, capability block, Harmony patch failure, or Tainted Weapons error/failure patterns.
- Startup verdict: pass for deployed binary identity, capability gate, identity registration, lifecycle config, and UI Addressables regression check.

## Expected Lifecycle Receipt Evidence

- Live config path: `<local-path>`
- `LifecycleValidation.Enabled = true`
- `LifecycleValidation.RequestId = 0`
- `LifecycleValidation.WriteOnSceneChange = true`
- `LifecycleValidation.WriteOnShutdown = true`
- Increment `LifecycleValidation.RequestId` to write `lifecycle-validation-*.tsv` after fixture stages.
- Optional `LifecycleValidation.WriteOnSceneChange` records W10 scene transition snapshots.
- Optional `LifecycleValidation.WriteOnShutdown` records W17 before/after plugin teardown.
- W7-W10/W14-W17 remain pending until those live receipts exist and counters are inspected.

## Live Lifecycle Receipt Check

- Latest receipt: `<local-path>`
- Receipt reason: `scene-change:ApplicationScene->CampaignMap_HOS`.
- Receipt version: `0.3.7`.
- Summary state: `recordCount=1`, `runtimePrototypeRecordCount=0`, `runtimePrototypeObjectCount=0`, `registeredAssetKeyCount=0`, `frameworkMeshCounterTotal=0`, `frameworkMaterialCounterTotal=0`.
- Record state: Evil Greatsword is registered with `assetResolved=True` and `drakeReady=True`; no redirect, prototype build, cached handle, fallback handle, Drake mesh start, Drake material start, Drake mesh unload, or Drake material unload has occurred in this receipt.
- Lifecycle verdict: harness is live and writing receipts; W7-W10/W14-W17 are still not passed because the latest receipt does not include equip/unequip, two-instance, hide/show, scene-transition-after-equip, or Drake counter activity.
