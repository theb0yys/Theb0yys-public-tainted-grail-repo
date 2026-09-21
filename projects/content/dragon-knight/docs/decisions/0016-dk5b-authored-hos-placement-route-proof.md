# 0016: DK5B Authored HOS Placement Route Proof

Status: accepted source gate; exact Unity authoring route still unproven; Unity mutation and live proof remain blocked

Date: 2026-08-03

## Context

The user explicitly approved DK5B because the current docs blocked the authored pack/live proof. The user also stated that the exact Unity authoring route for placing a FoA `LocationTemplate` into `CampaignMap_HOS` is still unproven, even though the Unity project path exists.

DK5 already implements the read-only owned placement observer, but the authored Unity/ModService HOS placement pack and live observation are still pending. Decision `0015` selects the production route: a Dragon Knight-owned placement pack targeting the existing `CampaignMap_HOS` scene at Ancient Cromlech. Decision `0152` adds an Avalon Core manifest validation layer for authored scene overlays, but it is not a placement executor.

Read-only DK5B discovery did not prove an existing Dragon Knight placement-pack source inside `<local-path>`.

## Decision

Open DK5B as a source-gated route-proof step.

DK5B may:

- record the exact route-proof requirements for a Dragon Knight-owned authored placement pack;
- run read-only discovery against the Unity project and FoA ModService output;
- add repo-local documentation and fixtures that lock the required source route and stop conditions.

DK5B may not mutate the Unity project, build an authored pack, deploy to FoA, launch FoA, or claim live actor proof until the exact authoring route is proven.

## Required DK5B route proof

The next source packet must identify:

| Required proof | Required value or shape |
|---|---|
| Unity project root | `<local-path>` |
| Target scene | `CampaignMap_HOS` |
| Placement ID | `dragon-knight.vaelor.cromlech.center` |
| LocationTemplate | `Spec_DragonKnight_DK4A` / `d7b09116519f7564593be62781bee3db` |
| NpcTemplate | `NPCTemplate_DragonKnight_DK4A` / `00f608ee051b57748a6a9ed8dae28678` |
| Altar reference | `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`; not boss root; not trigger |
| Position | `-1795.209|80.243|-2915.928` |
| Rotation | `0|-140.441|0` |
| Output | exact authored scene or ModService pack folder, catalogue files, bundle files, and hashes |
| Live proof | `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED` from Dragon Knight `0.2.4` observer |

The packet must also prove how the authored placement serializes FoA ownership, save-owned state, duplicate policy, cleanup/rollback identity, and route activation metadata.

## Stop conditions

Stop if the proposed route:

- calls or names `LocationTemplate.SpawnLocation`;
- uses `BaseLocationSpawner`;
- injects or edits vanilla `LocationSpawner` slots;
- uses Hollow Druid, bear, shrine, discovery, or prompt objects as production placement owners;
- makes Avalon Core, Wyrd Hunt, Living Avalon, Avalon Awakened, Avalon Companions, Avalon AI Runtime, or FoAHost the runtime placement owner;
- changes movement, attacks, hitboxes, damage, phase combat, companion protection, items, rewards, corpse handling, persistence, save writes, cleanup, rollback, Rabbit, GOAP, PlayMaker, Blaze, or live AI dispatch.

## Consequences

DK5B replaces the old dead-end wording that simply blocked authored Unity/ModService pack work. It does not prove the route yet. The next allowed work is exact route proof; only after that may a separate gate authorize Unity source mutation and an authored pack build.

## Protected files

No Tales from the Age of Men, Age of Men, or overhaul files are part of this decision.
