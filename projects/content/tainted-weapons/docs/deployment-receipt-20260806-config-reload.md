# Tainted Weapons 0.3.8 Config Reload Deployment Receipt

## Scope

- Date: 2026-08-06
- Receipt type: `TaintedWeaponsDeploymentReceipt`
- Purpose: deploy `0.3.8`, which reloads changed BepInEx config before lifecycle `RequestId` polling.
- Status: built, deployed, and hash-verified; live startup validation pending.

## Evidence Basis

- Weapons report line 184 requires lifecycle receipts with prototype, handle, Drake loading, and active-consumer state.
- Validation matrix lines 172-183 define W7-W10 and W14-W17 lifecycle/counter gates.
- Tainted Weapons research line 50 requires the user to start or restart the game manually; Codex must not control the live game process.
- Live `0.3.7` evidence showed Evil Greatsword equip redirect, Drake prototype build, framework mesh key serve, and framework material key serve occurred after the latest lifecycle receipt, but external `LifecycleValidation.RequestId = 1` did not produce a request-id receipt.

## Repository And Build

- Repository SHA: `b563573fa556b4d2b4a1b97d07d9f667f6a8b5aa`
- Built project: `mods/tainted-weapons/src/TaintedWeapons.csproj`
- Build command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Build result: passed, 0 warnings, 0 errors.

## Runtime Environment

- Game path: `<local-path>`
- Runtime family: FoA Mono / BepInEx 5
- Deployment process guard: no `Fall`, `Avalon`, `Tainted`, or game-root `UnityCrashHandler64` process was found before copy.

## Deployed Artifact

- Source DLL: `<local-path>`
- Live DLL: `<local-path>`
- Source DLL file version: `0.3.8.0`
- Live DLL file version: `0.3.8.0`
- Source SHA-256: `69B4A43E03DD403D1E4DA50F937294EDFA24256CBF173B4E009A35337BE1733E`
- Live SHA-256: `69B4A43E03DD403D1E4DA50F937294EDFA24256CBF173B4E009A35337BE1733E`
- Hash verdict: pass.

## Live Config State

- Config path: `<local-path>`
- `LifecycleValidation.Enabled = true`
- `LifecycleValidation.RequestId = 1`
- `LifecycleValidation.WriteOnSceneChange = true`
- `LifecycleValidation.WriteOnShutdown = true`

## Expected Live Startup Evidence

- `Tainted Weapons 0.3.8 loaded`.
- `Tainted Weapons: patched 6 framework equipped prototype hooks`.
- Capability receipt `ready=True`.
- Evil Greatsword registration accepted with identity receipt.
- Log entry `Tainted Weapons lifecycle validation config reloaded` after incrementing `LifecycleValidation.RequestId`.
- Request-id receipt path with reason `request-id-2` after incrementing `LifecycleValidation.RequestId` from `1` to `2`.
- No global UI Addressables coercion errors.

## Validation State

- Deployment: pass.
- Live startup: pending.
- Request-id config reload: pending.
- W7-W10/W14-W17 lifecycle fixture receipts: pending.
