# DK4A: Isolated Template Proof Source Gate Packet

Status: closed by Decision 0010. Unity authoring, Addressables build, and finalized two-cycle load/release proof passed; runtime source, deployment, live actor observation, spawn, and AI remain blocked.

Date: 2026-08-01

## Purpose

This packet defined the only allowed source and execution shape for the DK4A isolated `NpcTemplate` / `LocationTemplate` proof. Its output is the final Dragon Knight template GUID pair required before any later DK4 actor observation or `LocationTemplate.SpawnLocation` path.

## Closure Evidence

Decision 0010 records the completed proof.

Repository source:

- `mods/dragon-knight/tools/template-proof/DragonKnightDK4ATemplateAuthoring.cs`
- copied isolated Unity project source SHA-256 matched repo source `C9EBB090B1B16F7CCBFBB59D256B660BEB8E4A04CFBAE6EFC1485E83C295BF88`

Generated templates:

- `NPCTemplate_DragonKnight_DK4A.prefab` GUID `00f608ee051b57748a6a9ed8dae28678`, address `dragon-knight/boss/dk4a/npc-template--00f608ee051b57748a6a9ed8dae28678`;
- `Spec_DragonKnight_DK4A.prefab` GUID `d7b09116519f7564593be62781bee3db`, address `dragon-knight/boss/dk4a/location-template--d7b09116519f7564593be62781bee3db`.

Generated reports:

- `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-build-2026-08-01.json`
- `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-finalized-2026-08-01.json`

Final catalogue:

- `<local-path>`
- SHA-256 `6D91697F8EFF45E0DF8599958519BEF7162E371433D9321723F6C73AA0B25558`

The finalized report passed with unchanged guarded source hashes, exactly two explicit template assets, two load cycles, released handles, no runtime source, no game deployment, no save access, no actor construction, and no spawn request.

## Allowed Source

The execution gate added exactly one repository authoring source file:

`mods/dragon-knight/tools/template-proof/DragonKnightDK4ATemplateAuthoring.cs`

That file was copied to the isolated Unity proof project as:

`Assets/Editor/DragonKnightDK4ATemplateAuthoring.cs`

It must target only:

- `NPCTemplate_DragonKnight_DK4A.prefab`
- `Spec_DragonKnight_DK4A.prefab`
- template root `Assets/DragonKnightDK4ATemplate/Templates`
- output root `<local-path>`

## Source Hash Guards

The execution gate must verify these source hashes before and after authoring:

| Source | SHA-256 |
|---|---|
| `Assets/Data/Templates/NpcTemplates/Abstracts/Abstract_NPCTemplate_Foredweller.prefab` | `AC3FF9FE750BF5820F14FF52A8B2BFBF63C0D951D022E0B17F0746119BEF4608` |
| `Assets/Data/Templates/NpcTemplates/Enemies/Foredwellers/NPCTemplate_EnemyForedweller_T6_Knight.prefab` | `95E860FC25705F3D8FAD997B20CD260D1A38271723B587CEA47BDE741106E933` |
| `Assets/Data/LocationSpecs/AI/Enemies/ForeDwellers/Spec_EnemyForedweller_T6_Knight.prefab` | `03C7E647295DF1D09541139D9280C3A4DDD356EB543BF060B9CCE455630A91BD` |

The execution gate must also verify that generated Dragon Knight GUIDs do not equal:

- source NPC GUID `439a2dc3cf64e4e4aa792be5b4034cf1`;
- source location GUID `24ee850d0ffdbf64ea2b07b6a718d63b`;
- abstract parent GUID `d11cfa3551773034eacc4e3d4cec7183`;
- each other.

## Required Addresses

The authoring output must use:

- `dragon-knight/boss/dk4a/npc-template--{npcTemplateGuid}`;
- `dragon-knight/boss/dk4a/location-template--{locationTemplateGuid}`.

Each address must end with the same generated GUID exposed by the asset meta.

## Required Marker Lines

```text
DRAGON_KNIGHT_DK4A_TEMPLATE_BUILD_PASS explicitAssets=2; npcTemplateCreated=true; locationTemplateCreated=true; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false
```

```text
DRAGON_KNIGHT_DK4A_TEMPLATE_FINALIZED_PASS explicitAssets=2; loadCycles=2; templateRegistered=false; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false
```

## Stop Conditions

Stop immediately if:

- Unity version is not `6000.0.64f1`;
- any guarded source hash changed;
- Addressables settings cannot be restored;
- the template folder contains any prefab except the two approved Dragon Knight templates;
- a `templateSO` is required;
- generated GUIDs collide or reuse native Foredweller GUIDs;
- the two-cycle load/release proof cannot release every handle;
- any actor is constructed;
- any spawn is requested;
- the game install is modified;
- a save is read or written.

## Not Authorized

- Runtime plugin source.
- AI package source.
- FoA deployment or launch.
- Save access.
- `LocationTemplate.SpawnLocation`.
- DK4 live actor observation source.
- Movement, attacks, damage, death, custom loot, rewards, corpse logic, roaming, companion protection, follower mechanics, weapon items, armor items, phase combat, Rabbit, GOAP, PlayMaker, Blaze, or FoAHost binding.
