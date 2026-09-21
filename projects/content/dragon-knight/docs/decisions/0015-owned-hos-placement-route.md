# 0015: Dragon Knight-Owned HOS Placement Route

Status: superseded by `0018-dk5d-native-location-spawner-candidate-route`

Date: 2026-08-03

## Context

The user rejected the DK4 temporary no-save actor diagnostic and rejected vanilla population slots as the production route for the Dragon Knight boss. The selected encounter remains Ancient Cromlech in Horns of the South, using the existing runtime scene `CampaignMap_HOS`.

2026-08-04 update: the user explicitly approved the native `LocationSpawner` candidate placement route override. The authored-pack-only restriction below is retained as historical context and no longer governs DK5D.

DK4A already supplied Dragon Knight-owned template GUIDs:

- `NPCTemplate_DragonKnight_DK4A` GUID `00f608ee051b57748a6a9ed8dae28678`;
- `Spec_DragonKnight_DK4A` GUID `d7b09116519f7564593be62781bee3db`.

The Cromlech geometry gate initially used `-1792.661|79.942|-2912.844`; the later `20260803-032730` live dump proved that point is the `AltarInteract` reference, not the boss placement root. The corrected boss root is `-1795.209|80.243|-2915.928`, derived from the altar reference plus `4m` toward dumped hero position `-1801.847|81.026|-2923.964`. Edge witness remains `-1808.181|82.621|-2931.862`, with `25m` wake radius, `5m` inner fight radius, `22m` soft leash, and `25m` hard leash.

## Decision

Dragon Knight production placement must use a Dragon Knight-owned HOS placement pack targeting the existing `CampaignMap_HOS` scene. It must not use:

- DK4 `LocationTemplate.SpawnLocation` temporary diagnostic insertion;
- `BaseLocationSpawner` direct runtime placement;
- vanilla `LocationSpawner` slot injection;
- Hollow Druid shrine slots;
- bear or other creature slots;
- Wyrd Hunt, Living Avalon, Avalon Awakened, or Avalon Companions as runtime placement owners.

Dragon Knight `0.2.4` adds a read-only owned placement observer. It scans live `Location` models in `CampaignMap_HOS`, requires the exact Dragon Knight template pair, requires the placement to be near the corrected Cromlech boss root, rejects no-save placements, and logs `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED` with the live `Location.ID` and actor ID.

## Initial Placement Manifest

| Field | Value |
|---|---|
| Encounter ID | `dragon-knight.vaelor.cromlech` |
| Placement ID | `dragon-knight.vaelor.cromlech.center` |
| Actor role | `dragon-knight.boss` |
| Scene | `CampaignMap_HOS` |
| Display scene | Horns of the South |
| Arena | Ancient Cromlech / Stonehenge |
| LocationTemplate | `Spec_DragonKnight_DK4A` / `d7b09116519f7564593be62781bee3db` |
| NpcTemplate | `NPCTemplate_DragonKnight_DK4A` / `00f608ee051b57748a6a9ed8dae28678` |
| Altar reference | `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`; not boss root; not trigger |
| Position | `-1795.209|80.243|-2915.928` |
| Rotation | `0|-140.441|0` |
| Edge witness | `-1808.181|82.621|-2931.862` |
| Placement tolerance | `3m` |
| Wake / fight / leash | `25m` / `5m` / `22m` / `25m` |

## Boundaries

This decision authorizes source for the placement manifest and read-only observer only. It does not authorize movement, attacks, hitboxes, damage, phase combat, companion protection, item registration, armor registration, save writes, Rabbit writes, GOAP dispatch, PlayMaker procedure calls, Blaze control, or live AI runtime registration.
