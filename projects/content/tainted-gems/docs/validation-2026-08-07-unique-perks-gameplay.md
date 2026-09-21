# Tainted Gems 0.1.5 Unique Perks Gameplay Validation

## Scope

Version `0.1.5` adds unique adaptive/maladaptive skill and perk descriptions to all 42 Tainted Gem clone identities, tunes clone prices, and adds active socketed-gem gameplay hooks for silent sneaking, higher jump launch, and adaptive/maladaptive trade multipliers.

## Research basis

- `mods/tainted-gems/docs/research.md`: user instruction, asset/shop route, and 2026-08-07 gameplay-effect evidence.
- `mods/tainted-gems/docs/design.md`: runtime clone, custom UI, shop route, and socketed-gem gameplay route.
- `docs/engineering-process.md`, `docs/foa-modding-environment.md`, `docs/mod-lifecycle.md`, and `docs/code-review-standard.md`: FoA mod research, versioning, build, validation, and live DLL freshness process.
- Local `TG.Main.dll` decompilation for `ItemTemplate`, `GemAttached`, `HeroItems`, `VHeroFootsteps`, `ThieveryNoise`, `HumanoidMovementBase`, `DefaultPriceProvider`, and `HeroPriceProvider`.

## Static validation

- Protected overhaul/Age of Men paths were not edited.
- `mods/tainted-gems/assets` does not exist.
- Live `BepInEx/plugins/TaintedGems/assets` does not exist.
- Package layout contains only `TaintedGems.dll`.

## Build validation

Command:

```powershell
dotnet build 'mods/tainted-gems/src/TaintedGems.csproj' -c Release -p:FoAGameRoot='<local-path>
```

Result:

- Passed.
- `0 Warning(s)`.
- `0 Error(s)`.

## Hash validation

- Built DLL SHA-256: `F0E1C868A525282344C1F63334918B69A611A46949204F9C165328BE8DC2706A`
- Live DLL SHA-256: `F0E1C868A525282344C1F63334918B69A611A46949204F9C165328BE8DC2706A`
- Embedded AssetBundle SHA-256: `356AE8426D3A3ED60CA324EAF1F73FDE310D8C971DC658C70B9C750FD8619103`
- Release package SHA-256: `02842703E9D218684405C4A9253D3B3AE8E7935581983A7A1A36D3C92274301A`
- Release package: `mods/tainted-gems/release/TaintedGems-0.1.5-embedded.zip`

## Live deployment validation

Copied:

```text
mods/tainted-gems/src/bin/Release/netstandard2.1/TaintedGems.dll
```

to:

```text
<local-path>
```

The live DLL hash matches the built DLL hash.

## Runtime validation not run

FoA was not launched from this task. Remaining live checks:

- Plugin log shows `Tainted Gems 0.1.5`.
- Embedded bundle loads from `TaintedGems.Assets.tainted_gems.bundle`.
- World placement auto-spawns without hotkey input.
- Shop UI shows the 42 custom-icon Tainted Gem batch.
- Item cards show unique adaptive/maladaptive skill and perk text.
- Socketed Tainted Gems suppress crouch footstep audio and crouch movement noise.
- Socketed Tainted Gems increase jump launch height.
- Adaptive and maladaptive socketed Tainted Gems adjust trade prices in the expected direction.
