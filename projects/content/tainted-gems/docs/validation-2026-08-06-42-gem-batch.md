# Tainted Gems 42-Gem Batch Validation 2026-08-06

## Change

Version `0.1.1` expands the shop descriptor batch from the two proven entries to 42 named, non-fake native `item:gem` source templates from the runtime dump.

The first proven custom IDs remain stable:

- `7a9e0000000000000000000000000001`, `Tainted Garnet Shard`
- `7a9e0000000000000000000000000002`, `Tainted Crimson Cluster`

New custom IDs continue from `7a9e0000000000000000000000000003` through `7a9e000000000000000000000000002a`.

## Static build

Command:

```powershell
dotnet build mods/tainted-gems/src/TaintedGems.csproj -c Release -p:FoAGameRoot="<local-path>"
```

Result:

- Passed.
- Warnings: 0.
- Errors: 0.
- Built DLL: `mods/tainted-gems/src/bin/Release/netstandard2.1/TaintedGems.dll`

## Live deployment

Deployed file:

- `<local-path>`

The existing bundle remains unchanged:

- `<local-path>`

Live config updated:

- `<local-path>`
- `ShopStock.TemplatesPerOpen = 42`

## Runtime

Not run in this validation pass. FoA was not running during deployment.

Runtime checks still needed:

- Plugin load log showing `Tainted Gems 0.1.1`.
- Config load showing `TemplatesPerOpen=42`.
- Shop open log showing the 42-entry candidate count.
- Shop UI visibility of the expanded Tainted Gem stock batch.
