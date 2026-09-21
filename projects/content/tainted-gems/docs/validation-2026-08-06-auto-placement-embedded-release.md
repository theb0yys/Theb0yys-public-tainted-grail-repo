# Tainted Gems Auto Placement Embedded Release Validation 2026-08-06

## Change

Version `0.1.3` keeps the embedded `tainted_gems.bundle` release shape and changes world placement from manual hotkey proof behavior to automatic hero-ground placement.

The release defaults are:

- `WorldPlacement.AutoPlaceOnWorldReady = true`
- `WorldPlacement.AllowHotkeyPlacement = false`
- `WorldPlacement.PlacementHotkey = None`

Shop and inventory UI icons remain inherited native gem icons. Custom UI-icon binding is not implemented in this release.

## Embedded source

- Source bundle: `mods/tainted-gems/assets/tainted_gems.bundle`
- Source bundle SHA-256: `036EE4BC86CA7B11B0D463DE18E984E4B760B6C879622A339FAEE352114D7A42`
- Source bundle bytes: `16115784`

## Static build

Command:

```powershell
dotnet build mods\tainted-gems\src\TaintedGems.csproj -c Release -p:FoAGameRoot='<local-path>
```

Result:

- Passed.
- Warnings: 0.
- Errors: 0.
- Built DLL: `mods/tainted-gems/src/bin/Release/netstandard2.1/TaintedGems.dll`
- Built DLL bytes: `16177152`
- Built DLL SHA-256: `217425BE2B0FFBA6BFC14CDED177E092072EE2A9BDA4ECE2CCF7223E0B234030`

## Resource verification

Checked the built DLL manifest resource names and confirmed:

```text
TaintedGems.Assets.tainted_gems.bundle
```

## Release package

- Package: `mods/tainted-gems/release/TaintedGems-0.1.3-embedded.zip`
- Package SHA-256: `7531F0AA97F864A7D1375C58C1540709F0411B0E4A42568003FBBBD4D584691B`
- Package contents: `TaintedGems.dll`

## Live deployment

Copied `0.1.3` to:

```text
<local-path>
```

Result:

- Copy succeeded.
- Live DLL SHA-256: `217425BE2B0FFBA6BFC14CDED177E092072EE2A9BDA4ECE2CCF7223E0B234030`
- Live config was staged for `0.1.3`: `PreferEmbedded=true`, `AutoPlaceOnWorldReady=true`, `AllowHotkeyPlacement=false`, `PlacementHotkey=None`, and `TemplatesPerOpen=42`.

## Runtime

Not run in this validation pass. Runtime checks still needed:

- Plugin load log showing `Tainted Gems 0.1.3`.
- Bundle load log showing `source=embedded:TaintedGems.Assets.tainted_gems.bundle`.
- Automatic world placement log showing `placement=auto-hero-ground` without pressing a key.
- Shop UI check for the 42-entry Tainted Gem batch.
