# Tainted Weapons 0.3.10 Runtime Hand Lifecycle Deployment Receipt

## Scope

- Purpose: deploy `0.3.10`, which observes registered custom weapon runtime views at `ItemEquip.OnWeaponLoaded` and `CharacterHandBase.OnMount`.
- Fix target: registered spawned `CharacterHandBase` presentation chain after vanilla equip instantiation.
- Route: template clone plus Drake prototype rebinding remains the equipped visual route.
- Exclusions: no hand-socket `MeshRenderer` fallback, no synthetic Drake ECS construction, no global Addressables mesh/material hooks, no inventory, combat, save, or animation mutation.

## Build

- Command: `dotnet build mods/tainted-weapons/src/TaintedWeapons.csproj -c Release -p:FoAGameRoot="<local-path>"`
- First result: failed because `Plugin.cs` lacked the `Awaken.TG.Main.Heroes.Combat` import for `CharacterHandBase`.
- Final result: passed with 0 warnings and 0 errors on 2026-08-06.
- Source DLL: `mods/tainted-weapons/src/bin/Release/netstandard2.1/TaintedWeapons.dll`
- Source version: `0.3.10.0`
- Source SHA-256: `D4EBC478B430C681E1EEB684970D04230A0945ED6B1B96D92AEDB3770BF566F6`

## Deployment

- Process guard: no running FoA process was found under `<local-path>`; deployment proceeded.
- Destination: `<local-path>`
- Live version: `0.3.10.0`
- Live SHA-256: `D4EBC478B430C681E1EEB684970D04230A0945ED6B1B96D92AEDB3770BF566F6`
- Result: source and live hashes match.

## Pending Live Validation

- User must start or restart the game manually.
- Expected startup log: `Tainted Weapons 0.3.10 loaded`.
- Expected capability log: `id=runtime-hand-lifecycle; mandatory=True; available=True; reason=ok`.
- Expected equip log: `equipped runtime view` from `ItemEquip.OnWeaponLoaded` and/or `CharacterHandBase.OnMount`.
- Expected custom Drake route: `framework Drake mesh key served` and `framework Drake material key served` only for registered Tainted Weapons keys.
- Expected UI isolation: no `InvalidKeyException` lines where UI/creator TextAsset/Sprite/GameObject keys are requested as `UnityEngine.Material`.
