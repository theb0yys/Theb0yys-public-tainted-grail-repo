# DK4A Isolated Template Proof Preparation

Date: 2026-08-01

Result: **PASS for isolated `NpcTemplate`/`LocationTemplate` proof preparation only. Unity authoring, Addressables build, finalized load/release, runtime source, deployment, live spawn, and AI were not run.**

Boundary: this gate prepares the exact offline proof contract for Dragon Knight templates. It does not mutate the Unity proof project, author templates, build a catalogue, deploy to Tainted Grail FoA, open a save, wire F-keys, construct an actor, call `LocationTemplate.SpawnLocation`, test combat/death, or place/populate the world.

## Authorization

The user identified the current blocker:

> Final Dragon Knight template GUIDs are still pending the isolated template proof step.

Decision 0008 closes the proof-profile approval blocker and selects Foredweller T6 Knight as the disposable Dragon Knight proof profile. The current step prepares the isolated template proof that can generate and record the final Dragon Knight template GUIDs in a later Unity execution gate.

## Evidence Used

- DK4A proof-profile gate: `mods/dragon-knight/docs/gates/DK4A-native-actor-template-proof-profile.md`
- DK4A Foredweller boss profile approval: `mods/dragon-knight/docs/research/dk4a-foredweller-boss-profile-approval-2026-08-01.md`
- DK4A Foredweller serialized native-template probe: `mods/dragon-knight/docs/research/dk4a-foredweller-serialized-native-template-probe-2026-08-01.md`
- Generated probe report: `mods/dragon-knight/docs/generated/dragon-knight-dk4a-foredweller-template-probe-2026-08-01.json`
- Proven method reference: `mods/avalon-awakened/docs/research/ci4-successor-goblin-template-proof-preparation-2026-07-31.md`
- Proven execution reference: `mods/avalon-awakened/docs/research/ci4-successor-goblin-template-proof-2026-07-31.md`

## Native Template Sources

The next execution step may copy only these reviewed native Foredweller sources in the isolated Unity proof project:

| Role | Path | GUID | SHA-256 |
|---|---|---|---|
| Abstract parent hash guard | `Assets/Data/Templates/NpcTemplates/Abstracts/Abstract_NPCTemplate_Foredweller.prefab` | `d11cfa3551773034eacc4e3d4cec7183` | `AC3FF9FE750BF5820F14FF52A8B2BFBF63C0D951D022E0B17F0746119BEF4608` |
| Source NPC template | `Assets/Data/Templates/NpcTemplates/Enemies/Foredwellers/NPCTemplate_EnemyForedweller_T6_Knight.prefab` | `439a2dc3cf64e4e4aa792be5b4034cf1` | `95E860FC25705F3D8FAD997B20CD260D1A38271723B587CEA47BDE741106E933` |
| Source location template | `Assets/Data/LocationSpecs/AI/Enemies/ForeDwellers/Spec_EnemyForedweller_T6_Knight.prefab` | `24ee850d0ffdbf64ea2b07b6a718d63b` | `03C7E647295DF1D09541139D9280C3A4DDD356EB543BF060B9CCE455630A91BD` |

The source NPC and source location are direct-serialized in the probed chain. No source parent rewrite is planned.

Read-only YAML checks also confirmed both copied template source types expose `metadata.notes`, so the later authoring script may use `metadata.notes` as a Dragon Knight proof marker.

## Prepared Proof Contract

The isolated authoring run must create only these Dragon Knight assets:

- `Assets/DragonKnightDK4ATemplate/Templates/NPCTemplate_DragonKnight_DK4A.prefab`
- `Assets/DragonKnightDK4ATemplate/Templates/Spec_DragonKnight_DK4A.prefab`

The isolated authoring run must generate new pack-owned Unity GUIDs. It must not reuse the native Foredweller GUIDs. The final template addresses must end in `--{same-guid}` so the native `TemplatesLoader` GUID extraction contract remains satisfied:

- `dragon-knight/boss/dk4a/npc-template--{npcTemplateGuid}`
- `dragon-knight/boss/dk4a/location-template--{locationTemplateGuid}`

The `template` label must expose exactly the two Dragon Knight template prefabs. No pack-owned `templateSO` is planned because the approved profile uses the native Foredweller fighting-style GUID `471a14b9dbb41b146a82cb5750afe26a`, not a custom `CustomFightingStyle`. If execution proves a pack-owned `templateSO` is required, that is a stop condition, not permission to fabricate a style asset.

## Required Field Plan

`NpcTemplate`:

- root name `NPCTemplate_DragonKnight_DK4A`;
- Dragon Knight-specific `metadata.notes` marker;
- exact approved values: level `40`, health `10000`, stamina `250`, stamina regen `5`, melee/ranged/magic `113/50/60`, weight `300`, poise `1000`, XP level/tier/reward `8/0/640`;
- faction `4c90d92d219d54a4f8918310ba9998a7`;
- abstract types `2d89c9bd6158a1049b7460cee96ed7dd`, `30fee903493e89e4089f2aeeb5e42d2a`, `d11cfa3551773034eacc4e3d4cec7183`;
- fighting style `471a14b9dbb41b146a82cb5750afe26a`;
- serialized `npcType=2` retained as source-parity only;
- offline corpse/loot parity only: `isDeadBodyLootable=1`, no custom loot-table insertion, no Dragon Knight weapon/armor/item/reward injection.

`LocationTemplate`:

- root name `Spec_DragonKnight_DK4A`;
- `displayName.IdOverride=dragon_knight_dk4a`;
- `displayName.ID=Template/displayName_dragon_knight_dk4a`;
- set `RepetitiveNpcAttachment.npcTemplate._guid` to the generated Dragon Knight `NpcTemplate` GUID;
- preserve source `prefabReference.address=83d478bb2e0ea6e4d9e1cc58f3ec6910`;
- preserve native bootstrap visual address `b4d3a4e9a58fb1c4ea4c239f267faa56`;
- preserve simplified dead-body prefab address `cee084d53322ec04ca39752e9754314a`;
- preserve hit VFX address `6435b816784ea434589f4b48e31ccf70`;
- preserve `snapToGround=true`;
- preserve `weaponsAlwaysEquipped=true`;
- preserve `potentialActors=[]`;
- preserve exact component order: `Transform`, `LocationSpec`, `LocationTemplate`, `RepetitiveNpcAttachment`, `IdleDataAttachment`, `CustomCombatAttachment`, `AliveAudioAttachment`, `MarkerAttachment`.

Dragon Knight Iron/Fire visuals remain overlays and are not written into the native `visualPrefab` field in this template proof.

## Execution Source Packet

The next execution gate may add and run only this authoring source:

- repository source: `mods/dragon-knight/tools/template-proof/DragonKnightDK4ATemplateAuthoring.cs`
- isolated Unity copy: `Assets/Editor/DragonKnightDK4ATemplateAuthoring.cs`
- proof project: `<local-path>`
- required Unity version: `6000.0.64f1`
- output root: `<local-path>`
- build folder name: `DragonKnightDK4ATemplate`
- proof label: `dragon-knight-dk4a-template-proof`
- catalogue player version: `dragon-knight-dk4a-template-v1`

The authoring source must be adapted from the proven Goblin template-authoring pattern, but with Dragon Knight-local constants, Foredweller source hashes, Foredweller component order, Foredweller approved stat policy, and Dragon Knight marker text.

## Pass Criteria For The Next Execution Gate

A future execution gate can only claim template-proof PASS after:

1. Unity `6000.0.64f1` runs in the isolated proof project, not the game install.
2. Reviewed Foredweller source files and metas are hash-guarded before and after authoring.
3. The two Dragon Knight templates are authored with new pack-owned GUIDs.
4. A JSON catalogue is built.
5. The catalogue loads through `BundledAssetProvider` in two finalized cycles.
6. Both templates load, validate exact field values/component order, and release every handle.
7. The generated template GUIDs and catalogue/artifact hashes are copied back into repo evidence.
8. The proof report confirms `actorConstructed=false`, `spawnRequested=false`, `gameDeployment=false`, and `saveAccess=false`.

## Required Markers

The build-stage pass marker must be:

```text
DRAGON_KNIGHT_DK4A_TEMPLATE_BUILD_PASS explicitAssets=2; npcTemplateCreated=true; locationTemplateCreated=true; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false
```

The finalized two-cycle load/release marker must be:

```text
DRAGON_KNIGHT_DK4A_TEMPLATE_FINALIZED_PASS explicitAssets=2; loadCycles=2; templateRegistered=false; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false
```

## Explicit Limits

This preparation does not prove runtime template registration, installed catalogue discovery, health-bar name rendering, native shell/Dragon Knight overlay compatibility, FoAHost ownership, AI ownership, combat, attacks, hit reaction, death animation, corpse retention, loot drops, save exclusion, teardown, placement, or population integration.

## Next Safe Gate

The next safe gate is separately running the isolated Unity Dragon Knight template authoring/build/two-cycle load-release proof using this preparation contract. It must still exclude runtime source, deployment, FoA launch, save access, F-key executor work, live spawn, placement, combat/death testing, AI, companion behavior, item behavior, armor behavior, and population work.
