# Tainted Gems 0.1.6 Store Split And Generic World Validation

## Scope

Version `0.1.6` keeps skill, perk, and trinket Tainted Gem versions in shop stock, adds generic valuable and generic low-value junk gem items, and keeps world placement on the embedded generic valuable gem display prefabs. Active attached-gem gameplay remains limited to skill/perk variants.

## Static Validation

- Read `AGENTS.md`, `mods/tainted-gems/docs/research.md`, `mods/tainted-gems/docs/design.md`, `mods/tainted-gems/docs/validation-plan.md`, and `docs/research/game-knowledge/CONTENT-ADDITION-WORKFLOW.md` before editing.
- Confirmed the 42 base native weapon gem clone rows are still present.
- Confirmed the store count formula is 42 skill + 42 perk + 42 trinket + 2 generic entries = 128.
- Confirmed existing `7a9e...` clone GUIDs remain the skill variants.
- Confirmed new `7a9f...` clone GUIDs are perk variants.
- Confirmed new `7aa0...` clone GUIDs are inert trinket variants.
- Confirmed `7aa10000000000000000000000000001` is the generic valuable gem.
- Confirmed `7aa10000000000000000000000000002` is the generic low-value junk gem.
- Confirmed `TaintedGemTemplatesByCustomGuid` only includes descriptors where `HasSocketedGameplay` is true, so trinket, valuable, and junk entries cannot contribute active attached-gem gameplay.
- Confirmed inert variants explicitly report neutral trade multipliers.

## Build Validation

Command:

```powershell
dotnet build 'mods/tainted-gems/src/TaintedGems.csproj' -c Release -p:FoAGameRoot='<local-path>
```

Result:

- Passed with 0 warnings and 0 errors.
- Output DLL: `mods/tainted-gems/src/bin/Release/netstandard2.1/TaintedGems.dll`

## Package Validation

- Package: `mods/tainted-gems/release/TaintedGems-0.1.6-embedded.zip`
- Package contents: `TaintedGems.dll` only.
- Package size: `14805915` bytes.
- DLL size: `17219072` bytes.
- Embedded bundle input size: `17138090` bytes.
- Package SHA-256: `C5F30CA85D00A2377A6B1719F6C664B1D3FD8B7A1AF8D6E06E87768D4ACED256`
- DLL SHA-256: `24526F7795E1FD4DC979B11A68F54B35F0C0FE8D08AA0CDF3F21ACCB96B4001A`
- Embedded AssetBundle SHA-256: `356AE8426D3A3ED60CA324EAF1F73FDE310D8C971DC658C70B9C750FD8619103`

## Live Deployment Validation

- Attempted to copy the built `0.1.6` DLL to `<local-path>`.
- First copy was blocked by Windows because `Fall of Avalon.exe` had the previous live DLL mapped.
- After `Fall of Avalon.exe` was no longer running, the final copy succeeded.
- Built DLL SHA-256: `24526F7795E1FD4DC979B11A68F54B35F0C0FE8D08AA0CDF3F21ACCB96B4001A`
- Live deployed DLL SHA-256: `24526F7795E1FD4DC979B11A68F54B35F0C0FE8D08AA0CDF3F21ACCB96B4001A`
- Live deployed DLL size: `17219072` bytes.

## Runtime Validation Still Needed

- Launch the game with the deployed `0.1.6` DLL.
- Confirm the log shows `Tainted Gems 0.1.6` and `storeVersions=skill-perk-trinket-generic-valuable-generic-junk`.
- Confirm automatic placement logs `placement=auto-hero-ground-generic-valuable-gem` and creates the generic valuable display without hotkey input.
- Open an Avalon shop and confirm the shop summary logs `candidateCount=128` and `perOpenLimit=128`.
- Confirm shop UI contains skill, perk, trinket, generic valuable, and generic low-value junk entries with custom icons.
- Confirm only socketed skill/perk entries trigger silent sneaking, higher jump, and adaptive/maladaptive trade behavior.
- Confirm trinket, generic valuable, and generic junk entries are inert.
