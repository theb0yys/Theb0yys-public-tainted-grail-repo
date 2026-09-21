# Tainted Weapons 0.3.9 Presentation Activation Deployment Receipt

## Scope

- Purpose: deploy `0.3.9`, which activates the framework-owned cloned `CharacterHandBase`/`DrakeLodGroup`/`DrakeMeshRenderer` presentation hierarchy after Drake authoring rebinding and before vanilla `ItemEquip` instantiates the prototype.
- Route: template clone plus Drake prototype rebinding remains the only equipped visual route.
- Exclusions: no hand-socket `MeshRenderer` fallback, no synthetic Drake ECS construction, no global Addressables mesh/material hooks, no combat or animation wiring.

## Build

- Command: `dotnet build mods/tainted-weapons/src/TaintedWeapons.csproj -c Release -p:FoAGameRoot="<local-path>"`
- Result: passed with 0 warnings and 0 errors on 2026-08-06.
- Source DLL: `mods/tainted-weapons/src/bin/Release/netstandard2.1/TaintedWeapons.dll`
- Source version: `0.3.9.0`
- Source SHA-256: `5B5F5EC73B59496F3A1A05EDBA95C5C500D89A8768B026E16DEC47C480F30DBE`

## Deployment

- Process guard: no running FoA process was found under `<local-path>`; deployment proceeded.
- Destination: `<local-path>`
- Live version: `0.3.9.0`
- Live SHA-256: `5B5F5EC73B59496F3A1A05EDBA95C5C500D89A8768B026E16DEC47C480F30DBE`
- Result: source and live hashes match.

## Pending Live Validation

- User must start or restart the game manually.
- Expected startup log: `Tainted Weapons 0.3.9 loaded`.
- Expected equip log: `Drake prototype built` includes `activatedPresentationNodes` plus Drake renderer `activeSelf`/`activeInHierarchy` fields.
- Expected custom Drake route: `framework Drake mesh key served` and `framework Drake material key served` only for registered Tainted Weapons keys.
- Expected UI isolation: no `InvalidKeyException` lines where UI/creator TextAsset/Sprite/GameObject keys are requested as `UnityEngine.Material`.
