# DK4A Isolated Template Proof

Date: 2026-08-01

Result: **PASS for isolated Unity authoring/build/finalized load-release only.**

This gate executed the separately prepared Dragon Knight DK4A `NpcTemplate`/`LocationTemplate` proof using the approved Foredweller T6 Knight disposable profile. It did not write runtime plugin source, write AI package source, deploy to FoA, launch FoA, access saves, register templates in the installed game, construct an actor, call `LocationTemplate.SpawnLocation`, use F-keys, spawn, place, test combat/death, validate loot drops, prove retained corpse behavior, or integrate population.

## Inputs

- Preparation contract: `mods/dragon-knight/docs/research/dk4a-isolated-template-proof-preparation-2026-08-01.md`
- Source-gate packet: `mods/dragon-knight/docs/gates/DK4A-isolated-template-proof-source-gate-packet.md`
- Prepared JSON: `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-proof-preparation-2026-08-01.json`
- Authoring source: `mods/dragon-knight/tools/template-proof/DragonKnightDK4ATemplateAuthoring.cs`
- Isolated Unity copy: `<local-path>`
- Isolated Unity proof project: `<local-path>`
- Unity version: `6000.0.64f1`

The copied isolated source SHA-256 matched the repository source:

- `C9EBB090B1B16F7CCBFBB59D256B660BEB8E4A04CFBAE6EFC1485E83C295BF88`

## Execution Notes

Unity emitted known isolated-project bootstrap noise also seen in prior proven template proofs, including missing package-cache test DLL paths, `Unity.Entities` empty-buffer validation exceptions for `DrakeStaticPrefabData`, and a missing `.git\config` read. Those messages did not stop this proof.

Required markers were reached:

```text
DRAGON_KNIGHT_DK4A_TEMPLATE_BUILD_PASS explicitAssets=2; npcTemplateCreated=true; locationTemplateCreated=true; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false
```

```text
DRAGON_KNIGHT_DK4A_TEMPLATE_FINALIZED_PASS explicitAssets=2; loadCycles=2; templateRegistered=false; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false
```

Logs and reports:

| Artifact | SHA-256 |
|---|---|
| `<local-path>` | `3761D2E6E75A43E0A00703AF51C7A4E018D367C0D4786B5779E91E6718A7CE2D` |
| `<local-path>` | `111D2BCD0B32A9CDCC58A1EFAD8956B773BF83A93A8DD65D3578D7F2DC65CA86` |
| `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-build-2026-08-01.json` | `A2BC89842C7CE55DE8039C3479E255D1D0ED47F6D2BFE6FCE82E8324F75FF285` |
| `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-finalized-2026-08-01.json` | `9096E105E398B6F2B907AE13C5B8C8E8334DD8327CE186759D19043139EC34E4` |

## Final Template Sources

| Kind | Asset | GUID | Address |
|---|---|---|---|
| NPC template | `Assets/DragonKnightDK4ATemplate/Templates/NPCTemplate_DragonKnight_DK4A.prefab` | `00f608ee051b57748a6a9ed8dae28678` | `dragon-knight/boss/dk4a/npc-template--00f608ee051b57748a6a9ed8dae28678` |
| Location template | `Assets/DragonKnightDK4ATemplate/Templates/Spec_DragonKnight_DK4A.prefab` | `d7b09116519f7564593be62781bee3db` | `dragon-knight/boss/dk4a/location-template--d7b09116519f7564593be62781bee3db` |

These are new pack-owned Unity GUIDs. They do not reuse the Foredweller source NPC GUID `439a2dc3cf64e4e4aa792be5b4034cf1`, source location GUID `24ee850d0ffdbf64ea2b07b6a718d63b`, or abstract parent GUID `d11cfa3551773034eacc4e3d4cec7183`.

## Built Artifacts

Catalogue:

- `<local-path>`
- SHA-256: `6D91697F8EFF45E0DF8599958519BEF7162E371433D9321723F6C73AA0B25558`

Built artifact hashes:

| Relative path | Bytes | SHA-256 |
|---|---:|---|
| `StandaloneWindows64/catalog_dragon-knight-dk4a-template-v1.hash` | 32 | `E862C7E8A8319E076082CBFC95C61887C7D1C0CFB9BBE2A436F4B789F7477B69` |
| `StandaloneWindows64/catalog_dragon-knight-dk4a-template-v1.json` | 4456 | `6D91697F8EFF45E0DF8599958519BEF7162E371433D9321723F6C73AA0B25558` |
| `StandaloneWindows64/dragonknightdk4atemplateproof_assets_all_9664841dc2352b4460b15fd0c54863f1.bundle` | 15491 | `59A9E14CFDBDC9CDA05CE39650F4CD7303242E6C82338F43B467A8B494446278` |

## Load/Release Proof

| Cycle | Loaded assets | Handles released | Fingerprint |
|---:|---:|---|---|
| 1 | 2 | `true` | `FECF3CFE4B90CD1B65F497A972FA2EE574A2A5406F063187DA42CEA067777371` |
| 2 | 2 | `true` | `FECF3CFE4B90CD1B65F497A972FA2EE574A2A5406F063187DA42CEA067777371` |

The two finalized load cycles produced the same fingerprint.

## Validated Profile

- Result: `PASS`.
- Approved profile valid: `true`.
- Unity version: `6000.0.64f1`.
- Level/health/stamina: `40` / `10000` / `250`.
- Stamina regen: `5`.
- Melee/ranged/magic damage: `113` / `50` / `60`.
- Weight/poise: `300` / `1000`.
- XP level/tier/reward: `8` / `0` / `640`.
- Serialized `npcType`: `2`.
- Dead-body lootable: `true`.
- Faction: `4c90d92d219d54a4f8918310ba9998a7`.
- Fighting style: `471a14b9dbb41b146a82cb5750afe26a`.
- Abstract types: `2d89c9bd6158a1049b7460cee96ed7dd`, `30fee903493e89e4089f2aeeb5e42d2a`, `d11cfa3551773034eacc4e3d4cec7183`.
- Native prefab reference: `83d478bb2e0ea6e4d9e1cc58f3ec6910`.
- Native visual address: `b4d3a4e9a58fb1c4ea4c239f267faa56`.
- Simplified dead-body prefab address: `cee084d53322ec04ca39752e9754314a`.
- Location component order preserved: `true`.
- Component order: `Transform`, `LocationSpec`, `LocationTemplate`, `RepetitiveNpcAttachment`, `IdleDataAttachment`, `CustomCombatAttachment`, `AliveAudioAttachment`, `MarkerAttachment`.
- Non-unique NPC attachment: `true`.
- `template` label valid: `true`.
- `templateSO` absent and valid: `true`.

## Source Hashes

All guarded source hashes were unchanged before and after authoring:

| Source | Before / after SHA-256 |
|---|---|
| `Assets/Data/LocationSpecs/AI/Enemies/ForeDwellers/Spec_EnemyForedweller_T6_Knight.prefab` | `03C7E647295DF1D09541139D9280C3A4DDD356EB543BF060B9CCE455630A91BD` |
| `Assets/Data/LocationSpecs/AI/Enemies/ForeDwellers/Spec_EnemyForedweller_T6_Knight.prefab.meta` | `C7D2F380F46ABAE2ECDE792F1D2F72216A50B1227E8D00F1B4516B7C4D184F69` |
| `Assets/Data/Templates/NpcTemplates/Abstracts/Abstract_NPCTemplate_Foredweller.prefab` | `AC3FF9FE750BF5820F14FF52A8B2BFBF63C0D951D022E0B17F0746119BEF4608` |
| `Assets/Data/Templates/NpcTemplates/Abstracts/Abstract_NPCTemplate_Foredweller.prefab.meta` | `B8250054760DA0B35A2DE21FFBB938D8EB848DC3F6E08D62D32494455922BC26` |
| `Assets/Data/Templates/NpcTemplates/Enemies/Foredwellers/NPCTemplate_EnemyForedweller_T6_Knight.prefab` | `95E860FC25705F3D8FAD997B20CD260D1A38271723B587CEA47BDE741106E933` |
| `Assets/Data/Templates/NpcTemplates/Enemies/Foredwellers/NPCTemplate_EnemyForedweller_T6_Knight.prefab.meta` | `F749F362BA48D8E712614200EBCF0CC156326C5ED42C671336ECD79DCDD1B1F1` |

## Remaining Boundary

This proof supplies a load-proven Dragon Knight native `NpcTemplate` / `LocationTemplate` pair for the later DK4 actor-observation gate.

It still does not prove live installed template discovery, runtime template registration, live `Location.ID`, target `Location.ID`, FoAHost ownership, movement, attacks, damage, death, custom loot, rewards, retained corpse behavior, roaming, companion protection, follower mechanics, weapon items, armor items, real phase combat, Rabbit, GOAP, PlayMaker, Blaze, or live AI behavior.
