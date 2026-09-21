# Tainted Weapons Gate 7 Visual Identity Deployment Receipt

Date: 2026-08-09

## Scope

This receipt covers the framework-owned repair for Evil Greatsword equipped visual identity resolution in `Tainted Weapons` version `0.3.18`.

The repair is scoped to the equipped visual render path. User-observed attack animations, attack logic, and damage are treated as working and were not modified. This receipt does not authorize direct native registration calls, save mutation, game-process control, combat rewiring, animation rewiring, or restoration of the rejected Evil legacy Unity renderer fallback.

## Authority and boundaries

- Current user instruction identifies the active failure as visual-only: the Evil Greatsword item exists, attacks, animates, and deals damage, but the equipped render is invisible.
- `mods/evil-greatsword-moveset/docs/research.md` assigns equipped visual ownership to the Tainted Weapons framework route and rejects the Evil local fallback.
- `mods/tainted-weapons/docs/research.md` authorizes the framework registry, native equipped prefab capture, Drake prototype rebinding, Drake mesh/material key service, runtime view receipts, ECS readiness/retention receipts, and visual-proof receipts.
- Codex did not control FoA, mutate saves, or invoke native registration directly.

## Pre-fix evidence

- Live log evidence showed `Tainted Weapons 0.3.17 loaded`, eight framework equipped prototype hooks patched, and Evil Greatsword legacy equipped visual hooks disabled in the consumer.
- Evil Greatsword registration was accepted with `assetResolved=True`, `drakeReady=True`, custom template `ItemTemplate_Mod_EvilGreatsword`, GUID `e6e91100000000000000000000000001`, mesh `Evil_eye_greatsword_LowUV1`, and material `MI_EvilGreatsword`.
- The final native registrar receipt reported `status=template_registered`, `accepted=True`, `registered=True`, `reasonCode=registered`, `addToMapInvocationCount=1`, and `nativeMutationOccurred=True`.
- A bounded 60-second live log watch observed no new Evil visual route markers.
- Latest lifecycle TSV evidence showed Evil framework rows but `redirectCount=0`, `builtPrototypeHandleCount=0`, and `runtimeViewObservationCount=0`.
- A plugin scan after the duplicate-source cleanup found only one Evil Greatsword DLL under `BepInEx\plugins`.

## Implementation

- Added a framework-owned custom template name index alongside the existing custom template GUID index.
- Equipped visual redirect and runtime-view/ECS observation now resolve registered records by custom template GUID first, then by reserved custom template name.
- Redirect/runtime-view/ECS receipts now include `identityMatch` so the next live run can prove whether the runtime item matched by GUID or by template name.
- The repair stays inside the existing Tainted Weapons Drake route and does not restore direct Unity renderer fallback behavior.

## Build

- Command: `dotnet build "mods\tainted-weapons\src\TaintedWeapons.csproj" -c Release -p:FoAGameRoot="<local-path>"`
- Result: passed with 0 warnings and 0 errors on 2026-08-09.
- Built DLL: `mods\tainted-weapons\src\bin\Release\netstandard2.1\TaintedWeapons.dll`
- Built SHA-256: `C8AE4D17C55AD3D8FD21F115D8450A6501C7ECB17AD80CD72B21ED85F7FFA530`

## Deployment

- Live destination: `<local-path>`
- Live SHA-256 after final copy: `C8AE4D17C55AD3D8FD21F115D8450A6501C7ECB17AD80CD72B21ED85F7FFA530`
- Original `0.3.17` backup: `<local-path>`
- Original `0.3.17` backup SHA-256: `21C1C51C4B879FD5BD0D0B62472B85E15DAD9A78DF4936FE268D8D8FC181C818`
- Intermediate backup: `<local-path>`
- Intermediate backup SHA-256: `032E85E8FAA9F3106B4F07BDEB2B9AD1F7BDE59EFEF7F23A041895B32A4A6504`
- Plugin scan: one `TaintedWeapons.dll` found under `BepInEx\plugins`.

## Not yet validated

- Fresh FoA startup loading `Tainted Weapons 0.3.18`.
- User-controlled equip/re-equip producing `equipped visual redirect` with `identityMatch=custom-template-guid` or `identityMatch=custom-template-name`.
- `Drake prototype built`, runtime view, runtime Drake ECS view, mesh/material key service, and visual-proof receipts for this exact `0.3.18` deployment.
- User screenshot/video acceptance of the equipped visual render.
- Save persistence and release packaging.

## Next validation

After the user manually starts FoA and equips or re-equips Evil Greatsword, inspect `BepInEx\LogOutput.log` and the newest Tainted Weapons lifecycle TSVs for:

- `Tainted Weapons 0.3.18 loaded`
- `equipped visual redirect` with `identityMatch=custom-template-guid` or `identityMatch=custom-template-name`
- `Drake prototype built`
- `equipped runtime view`
- `equipped runtime Drake ECS view`
- `framework Drake mesh key served`
- `framework Drake material key served`
- non-zero lifecycle counters for redirect, prototype handle, and runtime view observation
