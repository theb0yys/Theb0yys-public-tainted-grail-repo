# Tainted Gems Custom UI Icons Validation 2026-08-06

## Change

Version `0.1.4` adds 42 embedded custom UI sprites for the Tainted Gem clone identities. The sprites are rendered from single imported gem renderers and source textures during the Unity bundle build, then resolved at runtime through `mod://kane.tgfoa.tainted-gems/icon/...` references.

## AssetBundle build

Command:

```powershell
mods/tainted-gems/tools/Build-TaintedGemsBundle.ps1
```

Result:

- Passed.
- Manifest marker: `TAINTED_GEMS_BUNDLE_PASS`.
- Unity version: `6000.0.64f1`.
- Bundle SHA-256: `ADA97E0D55A44F5B7E82E7EE5318A378748E02483296256B4D2BCC144F8C8AFF`.
- Bundle bytes: `17163207`.
- `uiIconCount`: `42`.
- `uiIconSource`: `rendered-imported-gem-prefabs`.

## Icon validation

Scanned `<local-path>`.

Result:

- Sprite files: `42`.
- Minimum nontransparent pixels: `2410`.
- Multi-gem component captures: `0`.
- Blank sprites: `0`.
- Under-threshold sprites below `1024` visible pixels: `0`.
- All-magenta shader-error sprites: `0`.
- Visual spot checks passed for `TaintedGemsIcon_StarbornEgg.png` and `TaintedGemsIcon_MarkOfThree.png`.

## Static build

Command:

```powershell
dotnet build mods\tainted-gems\src\TaintedGems.csproj -c Release -p:FoAGameRoot='<local-path>
```

Result:

- Passed.
- Warnings: `0`.
- Errors: `0`.
- Built DLL: `mods/tainted-gems/src/bin/Release/netstandard2.1/TaintedGems.dll`.
- Built DLL bytes: `17228800`.
- Built DLL SHA-256: `0BF31CAE33E159CDA94D3185D05EDE9696F34866FA62A72A9433A912893C6D17`.

## Resource verification

Checked the built DLL manifest resource names and confirmed:

```text
TaintedGems.Assets.tainted_gems.bundle
```

Embedded resource length: `17163207`.

## Release package

- Package: `mods/tainted-gems/release/TaintedGems-0.1.4-embedded.zip`.
- Package SHA-256: `830BBC2F9DA4A7384FD66AFD0058D6E43B7342CB6C122B4FE1A90FC4FEB39468`.
- Package bytes: `14819896`.
- Package contents: `TaintedGems.dll`.

## Live deployment

Copied `0.1.4` to:

```text
<local-path>
```

Result:

- Copy succeeded.
- Live DLL SHA-256: `0BF31CAE33E159CDA94D3185D05EDE9696F34866FA62A72A9433A912893C6D17`.

## Runtime

Not run in this validation pass. Runtime checks still needed:

- Plugin load log showing `Tainted Gems 0.1.4`.
- Bundle load log showing `source=embedded:TaintedGems.Assets.tainted_gems.bundle`.
- Icon patch log showing `Tainted Gems custom icon patch armed`.
- Shop UI check showing the 42-entry Tainted Gem batch with custom UI icons.
