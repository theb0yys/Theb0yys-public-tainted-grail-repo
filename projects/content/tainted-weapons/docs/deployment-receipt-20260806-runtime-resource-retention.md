# Tainted Weapons 0.3.14 Runtime Resource Retention Deployment Receipt

## Purpose

- Deploy `0.3.14`, which keeps the registered Drake prototype route, the existing-linked-renderer ECS completion path, and `DrakeRendererManualTag`, then retains the exact registered Drake mesh/material keys after ECS readiness so the first registered unload does not release the framework handles.

## Evidence

- Live `0.3.13` log showed `Tainted Weapons 0.3.13 loaded` and `equipped runtime Drake ECS view ready` with `ecsCompletion=applied:meshId=1595,materialId=443,manual=True`.
- The same `0.3.13` receipt showed the completed renderer had `DrakeRendererManualTag`, `DrakeRendererSpawnedTag`, `MaterialMeshInfo`, `MipmapsMaterialComponent`, and `UVDistributionMetricComponent`.
- The same `0.3.13` log still showed the first registered mesh/material unload ran from counter `1` to `0` and left `handleValid=False`, `isLoaded=False`. The zero-counter underflow guard then blocked the repeated unload but was too late to preserve the first loaded handles.

## Build

- Command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Result: passed with 0 warnings and 0 errors.

## Deployment

- Command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Source: `mods/tainted-weapons/src/bin/Release/netstandard2.1/TaintedWeapons.dll`
- Destination: `<local-path>`
- Source/live version: `0.3.14.0`
- Source/live SHA-256: `772BF1A5BD05E6B85C317D0A5E5B01CC5566E0C91261D0671F367224A2DD5A3F`
- Result: source and destination hashes match.

## Next Validation

- Restart the game, equip the Evil Greatsword, and check for `Tainted Weapons 0.3.14 loaded` plus `equipped runtime Drake ECS view ready`.
- Expected readiness receipt: `ecsCompletion=applied:meshId=...,materialId=...,manual=True,retention=retained=2`.
- Expected retention behavior: if Drake tries to unload the retained registered mesh/material keys from counter `1`, the plugin logs `retention-blocked` and the unload result remains unchanged instead of releasing the handle.
