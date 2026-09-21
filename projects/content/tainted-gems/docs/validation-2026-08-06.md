# Tainted Gems Validation 2026-08-06

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

## AssetBundle cook

Command:

```powershell
mods/tainted-gems/tools/Build-TaintedGemsBundle.ps1
```

Result:

- Passed after adding the missing `System.Collections.Generic` using to the Unity builder.
- Unity version: `6000.0.64f1`.
- Bundle: `mods/tainted-gems/src/bin/Release/netstandard2.1/assets/tainted_gems.bundle`
- Bundle bytes: `16117536`.
- Bundle SHA-256: `7289F28A4B150F8D2F51BD840446F8702D193C4A89A4936B2AE4A3B11AE21740`.
- Source file count: `10`.
- Source hash: `F4DA16D4A36FFE0D3AC0944085122ED65CD643E7EAEAAE2F62179B7A4001AA1F`.
- Manifest: `docs/generated-tainted-gems-bundle-manifest.json`.

## Live deployment

Deployed files:

- `<local-path>`
- `<local-path>`

## Runtime

Not run in this validation pass. No FoA process was running and the current BepInEx log did not contain Tainted Gems load lines before launch.

Runtime checks still needed:

- Plugin load log.
- Bundle load log.
- `F9` world placement.
- Shop open and `TaintedGemsShopStock added` log.
- Shop UI visibility of inserted gem stock.
