# Tainted Gems World Placement Embedded Polish Validation 2026-08-07

## Change

This validation confirms the `0.1.4` release remains embedded-only, keeps automatic world placement active, and uses filtered world prefabs instead of spawning the full source gem sheet.

## Evidence

- `WorldPlacement.AutoPlaceOnWorldReady = true`
- `WorldPlacement.AllowHotkeyPlacement = false`
- `WorldPlacement.PlacementHotkey = None`
- `WorldPlacement.SpawnScaleMultiplier = 0.35`
- Embedded bundle resource: `TaintedGems.Assets.tainted_gems.bundle`
- Build-time bundle source: `mods/tainted-gems/src/bin/Release/netstandard2.1/assets/tainted_gems.bundle`
- No `mods/tainted-gems/assets` folder is required.

## AssetBundle build

Command:

```powershell
mods/tainted-gems/tools/Build-TaintedGemsBundle.ps1
```

Result:

- Passed.
- Manifest marker: `TAINTED_GEMS_BUNDLE_PASS`.
- Bundle SHA-256: `356AE8426D3A3ED60CA324EAF1F73FDE310D8C971DC658C70B9C750FD8619103`.
- Bundle bytes: `17138090`.
- World prefab filter log:
  - `TaintedGems_DiamondShapeSet1`: `keptRenderers=12`
  - `TaintedGems_DiamondShapeSet2`: `keptRenderers=10`
- Bundle manifest contains both world prefabs and `42` `TaintedGemsIcon_` sprite assets.

## Icon validation

Scanned `<local-path>`.

Result:

- Sprite files: `42`.
- Multi-gem component captures: `0`.
- Blank sprites: `0`.
- Under-threshold sprites below `1024` visible pixels: `0`.
- All-magenta shader-error sprites: `0`.

## Static build

Command:

```powershell
dotnet build mods\tainted-gems\src\TaintedGems.csproj -c Release -p:FoAGameRoot='<local-path>
```

Result:

- Passed.
- Warnings: `0`.
- Errors: `0`.
- Built DLL SHA-256: `02D04120325E1849BE31B2BF544D9DA6A1D5EFF0BD3893E6D2EB9A1F099CDFBF`.
- Embedded resource length: `17138090`.

## Release package

- Package: `mods/tainted-gems/release/TaintedGems-0.1.4-embedded.zip`.
- Package SHA-256: `9CD2B5CD21EA8F0DB8820D81C50BA9DBF82033869C67E36AE62B1276F916BBBA`.
- Package bytes: `14798498`.
- Package contents: `TaintedGems.dll`.

## Live deployment

Copied `0.1.4` to:

```text
<local-path>
```

Result:

- Copy succeeded.
- Live DLL SHA-256: `02D04120325E1849BE31B2BF544D9DA6A1D5EFF0BD3893E6D2EB9A1F099CDFBF`.
- Live loose `assets` directory: absent.
- Live config confirms embedded-first automatic placement with `SpawnScaleMultiplier = 0.35`.

## Runtime

Not run in this validation pass. Runtime checks still needed:

- Plugin load log showing `Tainted Gems 0.1.4`.
- Bundle load log showing `source=embedded:TaintedGems.Assets.tainted_gems.bundle`.
- Automatic world placement log showing `placement=auto-hero-ground`.
