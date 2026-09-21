# Tainted Weapons 0.3.11 Runtime ECS Readiness Deployment Receipt

## Scope

- Purpose: deploy `0.3.11`, which keeps the 0.3.10 registered spawned-hand runtime hooks and adds a registered-only post-conversion Drake ECS readiness receipt.
- Boundaries: no direct Unity renderer equipped visual fallback, no synthetic Drake ECS entity construction, no shared material edits, no inventory/combat/save/animation mutation, and no equip/unequip fixture driving.
- Reason: live 0.3.10 reached the spawned `CharacterHandBase` runtime view and served framework Drake mesh/material keys, but user visual acceptance still failed. Prior Drake lifecycle research says render proof must move past authoring components to the linked ECS entity pair.

## Build

- Command: `dotnet build mods\tainted-weapons\src\TaintedWeapons.csproj -c Release -p:FoAGameRoot="<local-path>"`
- First result: failed on the shipped render-bounds type/reference boundary (`AABB` and `Unity.Mathematics.Extensions`).
- Final result: passed with 0 warnings and 0 errors on 2026-08-06.
- Source: `mods/tainted-weapons/src/bin/Release/netstandard2.1/TaintedWeapons.dll`
- Source version: `0.3.11.0`
- Source SHA-256: `3A5BD1C10CFB8875132FEF6F471AA891956EA319ADBFDB183D88CBC79C26E336`

## Deployment

- Command: `dotnet build mods\tainted-weapons\src\TaintedWeapons.csproj -c Release -p:FoAGameRoot="<local-path>" -p:DeployOnBuild=true`
- Destination: `<local-path>`
- Live version: `0.3.11.0`
- Live SHA-256: `3A5BD1C10CFB8875132FEF6F471AA891956EA319ADBFDB183D88CBC79C26E336`
- Result: source and live DLL hashes match.

## Expected Live Evidence

- Startup log: `Tainted Weapons 0.3.11 loaded`.
- Equip logs: `equipped runtime view` from `ItemEquip.OnWeaponLoaded` and/or `CharacterHandBase.OnMount`.
- ECS readiness logs: `equipped runtime Drake ECS view ready`, or bounded `waiting`, `not ready`, or `terminal` receipts with linked entity count, group/render entity roles, material/mesh IDs, render bounds, linked transforms, and load/unload tag state.
- Drake key logs: `framework Drake mesh key served` and `framework Drake material key served` for the registered Evil Greatsword keys.

## Not Run

- Live 0.3.11 startup/equip visual validation; the game must be restarted and the Evil Greatsword equipped manually.
