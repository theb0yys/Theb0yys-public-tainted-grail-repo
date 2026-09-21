# DK5: Owned HOS Placement Observer Source Gate Packet

Status: source implemented and fixture-gated for Dragon Knight `0.2.4`; authored Unity/ModService placement pack and live observation still pending.

Date: 2026-08-03

## Purpose

Define the production-facing placement proof path after the user rejected temporary DK4 spawning and vanilla population slots. This gate allows Dragon Knight source to observe an authored Dragon Knight-owned placement in the existing Horns of the South scene.

## Required Marker

```text
DRAGON_KNIGHT_OWNED_PLACEMENT_SOURCE_GATE_PASS fixtures=26 owner=dragon-knight.boss.host actor-source=dragon-knight-owned-hos-placement-pack scene=CampaignMap_HOS placements=1 multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 native-spawn=0 vanilla-slot=0 goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save-write=0
```

Runtime observation marker:

```text
DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED
```

The observation marker must include owner, encounter, placement, actor role, actor source, actor `Location.ID`, actor ID, scene, `LocationTemplate` GUID, `NpcTemplate` GUID, position, activation trigger, arena radii, `custom-owned=1`, `native-spawn=0`, `vanilla-slot=0`, and `save-owned=1`.

## Allowed Source

- `mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementContract.cs`
- `mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementObserver.cs`
- `mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementMarker.cs`
- minimal `mods/dragon-knight/src/Plugin.cs` wiring
- `mods/dragon-knight/tests/DragonKnight.OwnedPlacement.Fixtures/`

## Source Requirements

- Target existing scene `CampaignMap_HOS`.
- Use a list-shaped `DragonKnightOwnedNpcPlacement` manifest so later custom NPCs can be added without changing the observer architecture.
- Initial placement must be `dragon-knight.vaelor.cromlech.center`.
- Initial template pair must be `Spec_DragonKnight_DK4A` / `NPCTemplate_DragonKnight_DK4A`.
- Altar reference must be `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`, and must not be the boss root or encounter trigger.
- Initial boss root position must be `-1795.209|80.243|-2915.928`.
- Initial boss rotation must be `0|-140.441|0`.
- Edge witness must be `-1808.181|82.621|-2931.862`.
- Placement tolerance must be `3m`.
- Wake, fight, soft leash, and hard leash must remain `25m`, `5m`, `22m`, and `25m`.
- Observer must scan live `Location` models and derive actor ID as `foa.location:<Location.ID>`.
- Observer must require a live `NpcElement` and exact `NpcTemplate` GUID.
- Observer must reject `MarkedNotSaved` or `IsNotSaved` actors.

## Stop Conditions

Stop immediately if the source:

- calls or names `LocationTemplate.SpawnLocation`;
- uses `BaseLocationSpawner`;
- injects or edits `LocationSpawner` slots;
- references Hollow Druid, bear, or other vanilla spawner candidates as production placement;
- adds runtime dependencies on Wyrd Hunt, Living Avalon, Avalon Awakened, Avalon Companions, Avalon AI Runtime, or FoAHost;
- moves actors, attacks, phases, protects companions, creates items, writes saves, or registers live AI.

## DK5B Supersession Boundary

For DK5 alone, the following remain not authorized. DK5B supersedes this only for repo-local route-proof documentation, read-only Unity-route discovery, and the next source packet that must prove the exact authored placement route before any Unity mutation.

- Authored Unity scene or ModService pack build.
- Live runtime actor placement proof.
- AI host mapping.
- Movement, attacks, damage, hitboxes, phase combat, companion protection, follower mechanics, weapon items, armor items, rewards, corpse handling, persistence, roaming, Rabbit writes, GOAP dispatch, PlayMaker calls, Blaze control, or save writes.
