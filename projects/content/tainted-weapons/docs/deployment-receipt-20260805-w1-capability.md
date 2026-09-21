# Tainted Weapons W1 Capability Deployment Receipt

## Scope

- Date: 2026-08-05
- Receipt type: `TaintedWeaponsDeploymentReceipt`
- Purpose: exact `0.3.5` deployment plus W1 capability validation.
- Status: deployed, hash-verified, live W1 capability startup log observed, and clean-shutdown marker verified.

## Evidence Basis

- Weapons report line 181 requires `TaintedWeaponsCapabilityReceipt` to evaluate mandatory capabilities before registration/equip, return immutable booleans plus denial reasons, and block when any required capability is false.
- Weapons report line 187 requires exact DLL, repository SHA, package hashes, configuration, FoA build/branch/DLC, runtime track, BepInEx version, installed consumers, plugin startup `0.3.5`, capability receipt, and clean-shutdown log.
- Tainted Weapons research line 50 requires the user to start or restart the game manually; Codex must not control the live game process.

## Repository And Build

- Repository SHA: `b563573fa556b4d2b4a1b97d07d9f667f6a8b5aa`
- Built project: `mods/tainted-weapons/src/TaintedWeapons.csproj`
- Build command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Build result: passed, 0 warnings, 0 errors.

## Runtime Environment

- Game path: `<local-path>`
- Steam app id: `1466060`
- Steam app name: `Tainted Grail: The Fall of Avalon`
- Steam build id: `24270691`
- Steam beta key: `mono`
- Runtime family: FoA Mono / BepInEx 5
- Game executable: `Fall of Avalon.exe`
- Game executable file version: `6000.0.41.4645959`
- Game executable product version: `6000.0.41f1 (46e447368a18)`
- BepInEx file version: `5.4.23.3`
- BepInEx product version: `5.4.23.3+cd593c47566adfe971f1d96225fa82d1864ae877`

## Deployed Artifact

- Source DLL: `<local-path>`
- Live DLL: `<local-path>`
- Live DLL file version: `0.3.5.0`
- Live DLL product version: `0.3.5+b563573fa556b4d2b4a1b97d07d9f667f6a8b5aa`
- Source SHA-256: `6D9578B61CA7D046185AA8A220B5D709E43258418CF09713940E7A0AAEDEEBEE`
- Live SHA-256: `6D9578B61CA7D046185AA8A220B5D709E43258418CF09713940E7A0AAEDEEBEE`
- Hash verdict: pass.

## Configuration

- Config path: `<local-path>`
- Config header still says plugin `v0.3.4`; this is expected until BepInEx rewrites/refreshes config metadata on a later run.
- `WeaponMaterialProbe.Enabled = false`
- `WeaponMaterialProbe.AutoRunOnce = false`
- `WeaponMaterialProbe.TriggerKey = F9`
- `WeaponMaterialProbe.InitialDelaySeconds = 8`
- `WeaponMaterialProbe.TargetFilters = Equipable_Weapon|Weapon_`
- `WeaponMaterialProbe.TargetPriorityFilters = Longsword|LongSword|Long_Sword|DullLongsword|Dull Longsword|Dull_Longsword|Medium_Tier2_Longsword|Medium_Tier1_DullLongsword`

## Installed Consumer Snapshot

- Evil Greatsword source DLL SHA-256: `E9B77A882A1348668618DCF31041234DFEB609DA02E5EF8CDBB2E7C09532B4E2`
- Evil Greatsword live DLL SHA-256: `9092BAA1AD8B6168A3463F4D1E2FF4A2B37B1D316EA318F022F416FB922BD46C`
- Consumer verdict: live Evil Greatsword does not match current source output. No consumer DLL was changed in this receipt.

## Live W1 Validation

- Game process state during deployment: not running.
- Game process state during live check: running, responding.
- Live log: `<local-path>`
- Live log timestamp checked: 2026-08-06 00:39 local time.
- Observed startup: `Tainted Weapons 0.3.5 loaded`.
- Observed patch count: `Tainted Weapons: patched 4 framework equipped prototype hooks`.
- Observed capability receipt: `version=1; ready=True; denialReasons=ok`.
- Observed mandatory capabilities:
  - `item-equip-redirect`: available `True`, reason `ok`
  - `prototype-handle`: available `True`, reason `ok`
  - `asset-reference-handle`: available `True`, reason `ok`
  - `drake-mesh`: available `True`, reason `ok`
  - `drake-material`: available `True`, reason `ok`
  - `counter-instrumentation`: available `True`, reason `ok`
- Observed optional capabilities:
  - `multi-renderer`: available `False`, reason `not-implemented-current-single-drake-renderer-path`
- Observed registered consumer: Evil Greatsword was accepted with `assetResolved=True` and `drakeReady=True`.
- Negative log scan: no `framework runtime capabilities blocked`, `capability-blocked`, `framework registration blocked`, `could not patch`, `Type=UnityEngine.Material`, or `InvalidKeyException` lines matched during this check.
- Live W1 verdict: pass for startup capability receipt on the deployed `0.3.5` DLL.

## Clean Shutdown Validation

- User reported the game closed after W1 validation.
- Post-close process check: no `Fall`, `Avalon`, `Tainted`, or `UnityCrashHandler` process was found.
- Clean-shutdown marker path: `<local-path>`
- Session id: `ab3025d35dab417f8229c659647c9ffe`
- Session start UTC: `2026-08-05T23:02:48.5576845Z`
- Session end UTC: `2026-08-05T23:55:54.0986455Z`
- Clean-shutdown marker: `cleanShutdown = true`
- Last active scene at shutdown: `CampaignMap_HOS`
- Last main-thread heartbeat frame: `154376`
- Latest Avalon Exceptions incident files remained timestamped `2026-08-06T00:14:39` local, before the `2026-08-06T00:55:54` clean-shutdown marker update.
- Clean-shutdown verdict: pass.

## Remaining Blocks

- Codex did not start the game because `mods/tainted-weapons/docs/research.md:50` requires the user to start or restart the game manually.
- W1 proves startup capability readiness and clean shutdown only. Visual/equip W6-W11, multi-renderer W12-W13, and counter/resource W14-W17 remain unvalidated.
