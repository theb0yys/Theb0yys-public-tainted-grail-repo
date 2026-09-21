# Research: Runtime Pet Diagnostics

Date: 2026-06-14
Scope: validate Avalon Companions diagnostics in game after deploying the plugin.
Environment: local Mono install at `<local-path>`; exact in-game version not verified.

## Evidence

- `<local-path>`
- `<local-path>`
- `<local-path>`

## Log result

- Avalon Companions loaded successfully.
- `PetElements=0`
- `PetVariants=0`
- `HeroSummons=0`
- `HeroPetAllies=0`
- `CommonReferences.PetBaseVariant.IsSet=True`
- `QrkoMountTemplates=4`
- Loaded pet templates:
  - `Spec_Pet_Qrko` / `cc30c92e0699e3c41be92d1e99057354`
  - `Spec_Pet_Qrko_02` / `6e22cbf2d8d366f4ea674e822d40904a`
  - `Spec_Pet_Qrko_03` / `61304dcf14d3fff40892fea15be634c2`
  - `Spec_Pet_Qrko_05` / `1356b1187a7dd5f458b10d47ab016297`

## CSV result

- Avalon Companions wrote four GUID-backed `Spec_Pet_*` rows.
- All four rows were `LocationTemplate`.
- All four rows were `Regular`.
- All four rows were non-abstract.
- Avalon Companions marked all four rows as `safeSpawnCandidate=false`.
- Template Diagnostics also listed the same four rows as loaded regular non-abstract `LocationTemplate` rows.

## Decision

The diagnostics support a controlled Qrko pet companion roster because canonical regular non-abstract Qrko `LocationTemplate` rows exist and the built-in pet common references are available.

This does not approve humanoid companion behavior. The pet roster must require explicit companion gates, avoid unmanaged pets, and mark managed spawned locations not saved until persistence is intentionally designed.

## Prototype result

- Prototype config was enabled with `ResearchModeOnly=false`, `EnableQrkoPetPrototype=true`, and `SpawnQrkoPetShortcut=F8`.
- BepInEx log reported the prototype ready message.
- After pressing `F8`, BepInEx log reported `Spec_Pet_Qrko` spawned with GUID `cc30c92e0699e3c41be92d1e99057354`.
- The log reported the spawned location was marked not saved.
- No Avalon Companions warning or exception was observed in the filtered log output.
- After this validation, the default and live prototype shortcut were moved from `F8` to `KeypadPlus` because `F8` conflicts with Template Diagnostics and Multi-Pin Map Notes.
- After this validation, the one-pet prototype was expanded into a four-pet roster with summon/swap, selection, recall, dismiss, and follow/stay controls.

## Next validation

- Smoke-check roster controls in game.
- Confirm visual spawn and follow behavior.
- Inspect BepInEx log after each step.
