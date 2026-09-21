# Tainted Weapons 0.3.15 Runtime Visual Proof Deployment Receipt

## Purpose

- Deploy `0.3.15`, which keeps the registered Drake prototype, ECS completion, `DrakeRendererManualTag`, and retained-key unload guard, then appends focused read-only visual-proof fields to the existing registered runtime ECS receipt.

## Evidence

- Live `0.3.14` log showed `Tainted Weapons 0.3.14 loaded`.
- The registered Evil Greatsword renderer reached `ecsCompletion=applied:meshId=1597,materialId=441,manual=True,retention=retained=2`.
- Retained mesh/material unload attempts were blocked and the registered Drake entries stayed `counter=1`, `handleValid=True`, and `isLoaded=True`.
- User visual acceptance still failed after retained resources stayed loaded, so the next evidence gap is camera/frustum, ECS culling visibility flags, material/shader validity, and world position at the moment the weapon should be visible.

## Build

- Command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Result: passed with 0 warnings and 0 errors.

## Deployment

- Command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Source: `mods/tainted-weapons/src/bin/Release/netstandard2.1/TaintedWeapons.dll`
- Destination: `<local-path>`
- Source/live version: `0.3.15.0`
- Source/live SHA-256: `9BA4C560F7F62105FA0CAB728A7DD578939CA4BED4DB6E6E395C7D9FFD405A69`
- Result: source and destination hashes match.

## Next Validation

- Restart the game, equip the Evil Greatsword, and check for `Tainted Weapons 0.3.15 loaded` plus `equipped runtime Drake ECS view ready`.
- Expected visual-proof receipt: `visualProof=` with `camera=...frustumIntersectsWorldBounds=...`, `ecsVisibility=...`, `material=...shaderValid=...`, `shaderSupported=...`, `handPosition=...`, `renderTransformPosition=...`, `ecsWorldPosition=...`, and `worldBoundsCenter=...`.
