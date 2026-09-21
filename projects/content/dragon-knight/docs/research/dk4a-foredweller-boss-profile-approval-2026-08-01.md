# DK4A Foredweller Boss Profile Approval

Date: 2026-08-01

Result: **PASS for proof-profile approval only. Template authoring, runtime source, deployment, live spawning, and AI remain blocked.**

## Authorization

The user approved the Foredweller path with overrides and requested boss-level stats.

The only exact boss-level stat values currently proven for this path are the serialized `NPCTemplate_EnemyForedweller_T6_Knight` values from the DK4A Foredweller probe. No exact numeric stat overrides were supplied beyond using boss-level stats, so this approval keeps the probed Foredweller T6 Knight stat values unchanged for the disposable Dragon Knight proof profile.

This approval closes the explicit profile-selection blocker. It does not create Dragon Knight templates and does not authorize any `LocationTemplate.SpawnLocation` path.

## Evidence Inputs

- Gate: `mods/dragon-knight/docs/gates/DK4A-native-actor-template-proof-profile.md`
- Candidate scan: `mods/dragon-knight/docs/research/dk4a-native-actor-template-candidate-scan-2026-08-01.md`
- Serialized probe: `mods/dragon-knight/docs/research/dk4a-foredweller-serialized-native-template-probe-2026-08-01.md`
- Generated probe report: `mods/dragon-knight/docs/generated/dragon-knight-dk4a-foredweller-template-probe-2026-08-01.json`
- Generated report SHA-256: `FAB32552B56DE4BFC5FC8B367A89FD2EDCC01672AB913785CC9A6579FFDC417F`

## Approved Proof Profile

| Field | Approved value |
|---|---|
| Baseline label | `foredweller-t6-knight` |
| Baseline NPC template | `NPCTemplate_EnemyForedweller_T6_Knight` |
| Baseline NPC template GUID | `439a2dc3cf64e4e4aa792be5b4034cf1` |
| Baseline location template | `Spec_EnemyForedweller_T6_Knight` |
| Baseline location template GUID | `24ee850d0ffdbf64ea2b07b6a718d63b` |
| Dragon Knight display name | `Dragon Knight` |
| Dragon Knight localization key | `Template/displayName_dragon_knight_dk4a` |
| Future template pack folder | `DragonKnightDK4ATemplate` |
| Future address prefix | `dragon-knight/boss/dk4a` |
| Future NPC template asset name | `NPCTemplate_DragonKnight_DK4A.prefab` |
| Future location template asset name | `Spec_DragonKnight_DK4A.prefab` |

Final Dragon Knight `NpcTemplate` and `LocationTemplate` GUIDs are not selected by this approval. They must be generated and recorded by the later isolated template proof.

## Approved Boss-Level Stat Policy

Use the direct serialized Foredweller T6 Knight stat values unchanged unless a later written override replaces them.

| Field | Approved value |
|---|---|
| Level | `40` |
| Health | `10000` |
| Stamina | `250` |
| Stamina regen per tick | `5` |
| Melee / ranged / magic damage | `113` / `50` / `60` |
| Weight | `300` |
| Poise threshold | `1000` |
| XP level / tier / reward | `8` / `0` / `640` |
| Faction GUID | `4c90d92d219d54a4f8918310ba9998a7` |
| Fighting style GUID | `471a14b9dbb41b146a82cb5750afe26a` |
| `npcType` serialized value | `2` |
| Dead body lootable | `1` |
| Abstract type GUIDs | `2d89c9bd6158a1049b7460cee96ed7dd`, `30fee903493e89e4089f2aeeb5e42d2a`, `d11cfa3551773034eacc4e3d4cec7183` |

The candidate scan records the diagnostic row for this candidate as `npcType=Normal`, `isHumanoid=true`, and `isSummon=false`. This approval does not reinterpret the serialized `npcType` value as a proven FoA boss enum. For DK4A, "boss-level stats" means the high-health, high-poise, high-level Foredweller T6 Knight values above.

## Approved Location And Visual Policy

The later isolated template proof may copy the Foredweller location component order:

1. `Transform`
2. `LocationSpec`
3. `LocationTemplate`
4. `RepetitiveNpcAttachment`
5. `IdleDataAttachment`
6. `CustomCombatAttachment`
7. `AliveAudioAttachment`
8. `MarkerAttachment`

The native bootstrap visual address is approved only for native initialization proof:

- native visual prefab address: `b4d3a4e9a58fb1c4ea4c239f267faa56`
- simplified dead-body prefab address: `cee084d53322ec04ca39752e9754314a`
- hit VFX address: `6435b816784ea434589f4b48e31ccf70`
- snap to ground: `1`
- weapons always equipped: `1`

Dragon Knight Iron/Fire visuals remain DK2 visual overlays only:

- Iron overlay: `Assets/Dragon Knight/Prefabs/Dragon_Knight_Iron_Weapon.prefab`
- Fire overlay: `Assets/Dragon Knight/Prefabs/Dragon_Knight_Fire_Weapon.prefab`

The overlay policy does not prove attack animation mapping, damage, corpse, loot, reward, roaming, companion protection, or phase-combat behavior.

## Loot And Corpse Policy

The serialized probe did not find loot-table references in the probed chain. The later DK4A template proof must therefore preserve source-parity only:

- retain `isDeadBodyLootable=1` from the source NPC template;
- retain the simplified dead-body prefab address from the source location template;
- do not add custom loot tables;
- do not add Dragon Knight item, armor, weapon, reward, or boss-drop behavior.

Any custom Dragon Knight loot or reward route requires a later item/equipment and boss-reward gate.

## Ready Marker

The proof-profile approval marker is:

```text
DRAGON_KNIGHT_DK4A_NATIVE_TEMPLATE_PROFILE_READY baseline=foredweller-t6-knight npc-template-source=439a2dc3cf64e4e4aa792be5b4034cf1 location-template-source=24ee850d0ffdbf64ea2b07b6a718d63b dragon-npc-template=pending-isolated-template-proof dragon-location-template=pending-isolated-template-proof native-bootstrap-visual=b4d3a4e9a58fb1c4ea4c239f267faa56 iron-overlay=Assets/Dragon Knight/Prefabs/Dragon_Knight_Iron_Weapon.prefab fire-overlay=Assets/Dragon Knight/Prefabs/Dragon_Knight_Fire_Weapon.prefab template-authoring=allowed runtime-source=0 spawn=0 ai=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 armor=0 save=0
```

## Still Not Authorized

- Editing runtime source.
- Editing AI package source.
- Editing source Dragon Knight assets or Unity packages.
- Editing a Unity project outside a later isolated template proof.
- Authoring `NpcTemplate` or `LocationTemplate` assets in this approval step.
- Building or deploying template catalogues.
- Launching FoA.
- Reading or writing saves.
- Calling `LocationTemplate.SpawnLocation`.
- DK4 live actor observation source.
- Actor movement, attacks, damage, death, custom loot, rewards, corpse logic, roaming, companion protection, follower mechanics, weapon items, armor items, phase combat, Rabbit, GOAP, PlayMaker, Blaze, or FoAHost binding.

## Next Safe Gate

The next allowed work item is a separate DK4A isolated template proof packet that defines:

- exact authoring source files;
- generated Dragon Knight template GUID policy;
- authoring project path;
- source hash checks;
- template asset paths and metas;
- catalogue/build output names;
- load/release proof markers;
- cleanup checks;
- explicit confirmation that no runtime actor is constructed and no save is accessed.
