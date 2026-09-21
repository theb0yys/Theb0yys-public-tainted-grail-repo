# DK5C Altar Reference / Boss Root Split

Date: 2026-08-03

## Scope

Correction to the DK5C placement packet after the latest Ancient Cromlech dump proved the old placement coordinate was the live altar interactable.

## Evidence

- Dump folder: `<local-path>`.
- `summary.txt` scene: `Horns of the South [CampaignMap_HOS]`.
- `summary.txt` hero position: `-1801.847|81.026|-2923.964`.
- `cromlech_target_proof.csv` line 21: `AltarInteract`, `Location.ID` `CM_Stonehenge_0_2258891627046429048_1`, target ID `foa.location:CM_Stonehenge_0_2258891627046429048_1`, component path `/SpawnedLocations_30/AltarInteract`, position `-1792.661|79.942|-2912.844`, action `Interact`, availability `Available`.

## Correction

`AltarInteract` remains required evidence, but only as an altar reference:

- reference ID: `dragon-knight.vaelor.cromlech.altar-reference`;
- source: `AltarInteract`;
- live Location.ID: `CM_Stonehenge_0_2258891627046429048_1`;
- coordinate: `-1792.661|79.942|-2912.844`;
- boss placement root: false;
- encounter trigger: false.

The boss placement root is a separate free point:

- placement ID: `dragon-knight.vaelor.cromlech.center`;
- boss root: `-1795.209|80.243|-2915.928`;
- rotation: `0|-140.441|0`;
- derivation: altar reference plus `4m` toward dumped hero position `-1801.847|81.026|-2923.964`.

## Boundary

This correction does not authorize Unity mutation, authored pack build, deployment, live actor proof, save writes, movement, attacks, phase combat, companion protection, items, Rabbit writes, GOAP dispatch, PlayMaker calls, Blaze control, FoAHost activation, cleanup, or rollback.
