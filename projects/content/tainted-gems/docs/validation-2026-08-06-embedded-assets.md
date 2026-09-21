# Tainted Gems Embedded Assets Validation 2026-08-06

## Change

Version `0.1.2` embeds the cooked `tainted_gems.bundle` inside `TaintedGems.dll` as manifest resource `TaintedGems.Assets.tainted_gems.bundle`.

The plugin now loads the embedded bundle first when `Bundle.PreferEmbedded = true`. The plugin-relative `assets/tainted_gems.bundle` path remains as fallback only.

## Embedded source

- Source bundle: `mods/tainted-gems/assets/tainted_gems.bundle`
- Source bundle SHA-256: `7289F28A4B150F8D2F51BD840446F8702D193C4A89A4936B2AE4A3B11AE21740`
- Source bundle bytes: `16117536`

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
- Built DLL bytes: `16174592`
- Built DLL SHA-256: `3B6C34B15C6E2CC97084E2F71D96A07BB3483F4B26BB4D56C837CAEADD1B49B9`

## Resource verification

Checked the built DLL manifest resource names and confirmed:

```text
TaintedGems.Assets.tainted_gems.bundle
```

## Live deployment

Deployed file:

- `<local-path>`

Live config set:

- `Bundle.PreferEmbedded = true`
- `ShopStock.TemplatesPerOpen = 42`

The old loose bundle remains deployed as fallback, but the release DLL is self-contained.

## Release package

- Package: `mods/tainted-gems/release/TaintedGems-0.1.2-embedded.zip`
- Package SHA-256: `12C17DFE4B26A46A6EBB7EC2113DFB7753806C5B05A2FCB9D611D3782A947AA8`

## Runtime

Not run in this validation pass. Runtime checks still needed:

- Plugin load log showing `Tainted Gems 0.1.2`.
- Bundle load log showing `source=embedded:TaintedGems.Assets.tainted_gems.bundle`.
- F9 placement check.
- Shop UI check for the 42-entry Tainted Gem batch.
