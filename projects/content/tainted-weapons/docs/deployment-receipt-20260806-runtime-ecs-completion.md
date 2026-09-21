# Tainted Weapons 0.3.12 Runtime ECS Completion Deployment Receipt

## Purpose

- Deploy `0.3.12`, which keeps the registered Drake prototype route and adds a registered-only completion path for an already-created linked renderer entity when the native Drake manager has loaded mesh/material IDs.

## Evidence

- Live `0.3.11` log showed Evil Greatsword redirect, Drake prototype build, framework mesh/material key service, and a two-entity linked ECS set, but the render entity remained `meshId=0`, `materialId=0`, `readyComponents=False`, and `transitionalTags=False` before registered unload.
- Decompiled `Awaken.ECS.dll` shows `DrakeRendererComponentsManager.UpdateLoadings()` registers loaded handles, `TryGetMaterialMesh(...)` returns `MaterialMeshInfo`, `MipmapsMaterialComponent`, and `UVDistributionMetricComponent`, and `DrakeRendererLoadingSystem` adds those plus `DrakeRendererSpawnedTag`.

## Build

- First build: failed because writing `MipmapsMaterialComponent` required referencing `Awaken.Utility.dll`.
- Final command: `dotnet build mods\tainted-weapons\src\TaintedWeapons.csproj -c Release -p:FoAGameRoot="<local-path>"`
- Result: PASS, 0 warnings, 0 errors.

## Deployment

- Command: `dotnet build mods\tainted-weapons\src\TaintedWeapons.csproj -c Release -p:FoAGameRoot="<local-path>" -p:DeployOnBuild=true`
- Source: `mods/tainted-weapons/src/bin/Release/netstandard2.1/TaintedWeapons.dll`
- Destination: `<local-path>`
- Source/live version: `0.3.12.0`
- Source/live SHA-256: `497F95993CE3799F6D252EF72A37D6FBAA453747A14F1FF0012F4122B557F132`
- Process check after deployment: no Tainted Grail/FoA/Avalon/Fall process found, so live startup/equip validation requires a new game launch.

## Pending

- Restart the game, equip the Evil Greatsword, and check for `Tainted Weapons 0.3.12 loaded` plus `equipped runtime Drake ECS view ready`.
- If readiness still fails, inspect the `ecsCompletion=` field in the bounded ECS receipt.
