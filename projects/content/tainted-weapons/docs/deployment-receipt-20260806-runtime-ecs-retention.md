# Tainted Weapons 0.3.13 Runtime ECS Retention Deployment Receipt

## Purpose

- Deploy `0.3.13`, which keeps the registered Drake prototype route and the `0.3.12` existing-linked-renderer ECS completion path, then retains the completed registered renderer with `DrakeRendererManualTag` and blocks registered zero-counter unload underflow.

## Evidence

- Live `0.3.12` log showed `Tainted Weapons 0.3.12 loaded`, Evil Greatsword visual redirection to the framework prototype, Drake prototype build, registered mesh/material key service, and `equipped runtime Drake ECS view ready` with `ready=True`, non-zero mesh/material IDs, `readyComponents=True`, and `ecsCompletion=applied`.
- The same live `0.3.12` log then immediately showed registered mesh/material unloads and repeated zero-counter unloads that underflowed counters from `0` to `65535`.
- Decompiled `Awaken.ECS.dll` showed `DrakeRendererStateSystem` excludes entities tagged with `DrakeRendererManualTag` from its distance/state load and unload queries, and `DrakeRendererManualTag` is an empty `IComponentData` tag.

## Build

- Command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Result: passed with 0 warnings and 0 errors.

## Deployment

- Command: `dotnet build 'mods\tainted-weapons\src\TaintedWeapons.csproj' -c Release -p:FoAGameRoot='<local-path>`
- Source: `mods/tainted-weapons/src/bin/Release/netstandard2.1/TaintedWeapons.dll`
- Destination: `<local-path>`
- Source/live version: `0.3.13.0`
- Source/live SHA-256: `57D79B1D9D61A446F8F76466EE2D6AF6B9747BCE90D7E94F2793FAA9480AEBB9`
- Result: source and destination hashes match.

## Next Validation

- Restart the game, equip the Evil Greatsword, and check for `Tainted Weapons 0.3.13 loaded` plus `equipped runtime Drake ECS view ready`.
- Expected readiness receipt: `ecsCompletion=applied:meshId=...,materialId=...,manual=True`.
- Expected retention behavior: no repeated registered unload underflow from `0` to `65535`; if Drake repeats a registered zero-counter unload, the plugin should log `underflow-blocked` and skip the native unload call.
