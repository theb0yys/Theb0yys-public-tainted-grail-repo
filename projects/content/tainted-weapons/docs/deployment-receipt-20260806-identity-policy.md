# Tainted Weapons 0.3.6 Identity Policy Deployment Receipt

## Scope

- Date: 2026-08-06
- Receipt type: `TaintedWeaponsDeploymentReceipt`
- Purpose: exact `0.3.6` deployment plus identity-policy live validation setup.
- Status: deployed, hash-verified, live identity-policy startup log observed, W6 log-level equipped Drake route observed, and clean-shutdown marker verified.

## Evidence Basis

- Weapons report line 182 requires `TaintedWeaponsIdentityPolicy` to canonicalise pack/item identity, hash the canonical definition with SHA-256, atomically reserve custom template GUID, template name, registry key, runtime prototype address, mesh keys, and material keys, return the same receipt for the same hash, and reject changed definitions.
- Weapons report line 187 requires exact DLL, repository SHA, package hashes, configuration, FoA build/branch/DLC, runtime track, BepInEx version, installed consumers, plugin startup, capability receipt, and clean-shutdown log.
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
- Source DLL file version: `0.3.6.0`
- Live DLL file version: `0.3.6.0`
- Source DLL product version: `0.3.6+b563573fa556b4d2b4a1b97d07d9f667f6a8b5aa`
- Live DLL product version: `0.3.6+b563573fa556b4d2b4a1b97d07d9f667f6a8b5aa`
- Source SHA-256: `7B835B6F38453A79B67D052593CE031A6BFE0A4748EB1DB4D7BF0BC5D0429D25`
- Live SHA-256: `7B835B6F38453A79B67D052593CE031A6BFE0A4748EB1DB4D7BF0BC5D0429D25`
- Hash verdict: pass.

## Configuration

- Config path: `<local-path>`
- Config header still says plugin `v0.3.5`; this is expected until BepInEx rewrites or refreshes config metadata on a later run.
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

## Live Identity Validation

- Game process state during deployment: not running.
- Initial post-deployment log poll: timed out waiting for `Tainted Weapons 0.3.6 loaded`.
- Initial post-deployment process state during poll timeout: no `Fall`, `Avalon`, `Tainted`, or `UnityCrashHandler` process was found.
- Live log checked: `<local-path>`
- Live log timestamp checked: `2026-08-06 01:51:18` local time.
- Observed startup: `Tainted Weapons 0.3.6 loaded`.
- Observed patch count: `Tainted Weapons: patched 4 framework equipped prototype hooks`.
- Observed capability receipt: `version=1; ready=True; denialReasons=ok`.
- Observed registered consumer: Evil Greatsword framework registration accepted with `assetResolved=True` and `drakeReady=True`.
- Observed identity receipt: `identity=version=1`.
- Observed identity fields:
  - `publicId=kane.tgfoa.evil-greatsword-moveset:evil-greatsword`
  - `templateGuid=e6e91100000000000000000000000001`
  - `templateName=ItemTemplate_Mod_e6e91100000000000000000000000001`
  - `definitionHash=e7bbc85788c0dffb2a91eb1996a3e9d2409126c1495a7b68722ca5b28c1d3368`
  - `runtimePrototypeAddress=mod://kane.tgfoa.tainted-weapons/equipped-prototype/kane.tgfoa.evil-greatsword-moveset/evil-greatsword-e7bbc85788c0`
  - `meshKey=mod://kane.tgfoa.tainted-weapons/equipped-prototype/kane.tgfoa.evil-greatsword-moveset/evil-greatsword-e7bbc85788c0/mesh`
  - `materialKeyPrefix=mod://kane.tgfoa.tainted-weapons/equipped-prototype/kane.tgfoa.evil-greatsword-moveset/evil-greatsword-e7bbc85788c0/material-`
- Negative log scan: no `identity-collision`, `queued-identity-collision`, `capability-blocked`, `framework runtime capabilities blocked`, `framework registration blocked`, `could not patch`, `Type=UnityEngine.Material`, or `InvalidKeyException` lines matched during this check.
- Live identity verdict: pass for startup identity-policy validation on the deployed `0.3.6` DLL.

## Clean Shutdown Validation

- Post-close process check: no `Fall`, `Avalon`, `Tainted`, or `UnityCrashHandler` process was found.
- Clean-shutdown marker path: `<local-path>`
- Session id: `4c6b4b1a790c4ae4a693f26fc948f258`
- Session start UTC: `2026-08-06T00:49:56.9822099Z`
- Session end UTC: `2026-08-06T00:51:20.0968563Z`
- Clean-shutdown marker: `cleanShutdown = true`
- Last active scene at shutdown: `CampaignMap_HOS`
- Last main-thread heartbeat frame: `2680`
- Latest Avalon Exceptions incident files were timestamped `2026-08-06T01:07:38` local, before the `2026-08-06T01:51:20` clean-shutdown marker update.
- Clean-shutdown verdict: pass.

## Remaining Blocks

- Codex did not start the game because `mods/tainted-weapons/docs/research.md:50` requires the user to start or restart the game manually.
- W6 log-level equip/presentation routing is recorded in `mods/tainted-weapons/docs/live-validation-receipt-20260806-w6-w11-evil-greatsword.md`.
- W7-W10 full lifecycle fixtures, W11 full UI screen coverage, multi-renderer W12-W13, and counter/resource W14-W17 remain unvalidated.
