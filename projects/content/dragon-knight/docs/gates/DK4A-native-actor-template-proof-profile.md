# DK4A: Native Actor/Template Proof Profile

Status: passed isolated template proof. Dragon Knight now has load-proven DK4A native template GUIDs. Runtime source, deployment, live spawning, and AI remain blocked until later gates.

Date: 2026-08-01

## Purpose

DK4A exists because DK4 cannot prove actor observation or host ownership until Dragon Knight has a proved native `NpcTemplate` and `LocationTemplate`, or an exact existing live `Location` actor source.

This gate defines the required Dragon Knight native actor/template proof path before any source file calls `LocationTemplate.SpawnLocation`.

## Current Evidence

- DK2 proves only visual Iron/Fire prefab transport through a Dragon Knight-owned AssetBundle.
- DK2 does not prove native actor construction, native template registration, native visual addressability, combat, attacks, phase behavior, companion behavior, items, armor, save behavior, or roaming.
- DK4 source remains blocked until exact Dragon Knight actor/template source and exact target source evidence exist.
- The read-only DK4A candidate scan found candidate native baselines, but selected none.
- The read-only Foredweller T6 Knight serialized native-template probe supplied exact candidate NPC/location values, but did not select the Dragon Knight baseline.
- Decision 0008 selects the Foredweller T6 Knight pair as the disposable Dragon Knight proof profile and retains the serialized Foredweller T6 Knight stat values as the approved boss-level stat baseline.
- Decision 0009 records the isolated template proof preparation and source-gate packet.
- Decision 0010 records the isolated Unity authoring/build/finalized load-release proof and supplies the final Dragon Knight template GUIDs.

## Candidate Baseline Policy

Foredweller T6 Knight is selected for the next isolated Dragon Knight template proof.

Selected source pair:

- `NPCTemplate_EnemyForedweller_T6_Knight` GUID `439a2dc3cf64e4e4aa792be5b4034cf1`;
- `Spec_EnemyForedweller_T6_Knight` GUID `24ee850d0ffdbf64ea2b07b6a718d63b`.

Approved Dragon Knight proof-profile values are recorded in:

`mods/dragon-knight/docs/research/dk4a-foredweller-boss-profile-approval-2026-08-01.md`

Any future replacement baseline, higher numeric stat override, different faction, custom loot, custom corpse policy, native Dragon Knight visual root, or alternative overlay policy requires another explicit written decision.

## Required Proof Values

Decision 0008 locks:

- approved baseline `NpcTemplate` source and GUID;
- approved baseline `LocationTemplate` source and GUID;
- display name and localization key;
- level, health, stamina, stamina regeneration, damage, weight, poise, XP, faction, abstract types, fighting style, loot tables, corpse/death policy, and reward policy;
- exact location component order;
- attachment policy;
- native bootstrap visual address;
- Dragon Knight visual overlay policy for Iron and Fire;
- template pack folder, address prefix, catalogue names, and hash plan;

Decision 0010 locks:

- final Dragon Knight `NpcTemplate` GUID;
- final Dragon Knight `LocationTemplate` GUID;
- save-exclusion and cleanup policy;
- validation markers.

A later live diagnostic gate must still lock save-exclusion and cleanup behavior against a real actor.

## Visual And Native Actor Policy

The first native actor proof may not treat the Dragon Knight DK2 AssetBundle prefab as a native actor-controller root.

Unless a separate native ModService/Addressables Dragon Knight visual proof is accepted first, the live actor path must use an approved native bootstrap visual from the selected baseline for `SpawnLocation`, then attach Dragon Knight visuals as overlays only after native actor initialization succeeds.

Initial overlay policy:

- Iron visual: phase-1 presentation only.
- Fire visual: phase-2 presentation placeholder only.
- No attack, damage, phase transition, death, or reward behavior may be inferred from either visual.

## Authored Template Artifacts

Decision 0010 created and load-proved:

- `NPCTemplate_DragonKnight_DK4A.prefab` GUID `00f608ee051b57748a6a9ed8dae28678`, address `dragon-knight/boss/dk4a/npc-template--00f608ee051b57748a6a9ed8dae28678`;
- `Spec_DragonKnight_DK4A.prefab` GUID `d7b09116519f7564593be62781bee3db`, address `dragon-knight/boss/dk4a/location-template--d7b09116519f7564593be62781bee3db`;
- template pack folder: `DragonKnightDK4ATemplate`
- address prefix: `dragon-knight/boss/dk4a`

## Marker Contract

The earlier blocked marker was:

```text
DRAGON_KNIGHT_DK4A_NATIVE_TEMPLATE_PROFILE_BLOCKED baseline=unapproved npc-template=blocked location-template=blocked native-bootstrap-visual=blocked dragon-overlay=visual-only template-authoring=0 runtime-source=0 spawn=0 ai=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 armor=0 save=0
```

After Decision 0010, the proof-profile marker is:

```text
DRAGON_KNIGHT_DK4A_NATIVE_TEMPLATE_PROFILE_READY baseline=foredweller-t6-knight npc-template-source=439a2dc3cf64e4e4aa792be5b4034cf1 location-template-source=24ee850d0ffdbf64ea2b07b6a718d63b dragon-npc-template=00f608ee051b57748a6a9ed8dae28678 dragon-location-template=d7b09116519f7564593be62781bee3db native-bootstrap-visual=b4d3a4e9a58fb1c4ea4c239f267faa56 iron-overlay=Assets/Dragon Knight/Prefabs/Dragon_Knight_Iron_Weapon.prefab fire-overlay=Assets/Dragon Knight/Prefabs/Dragon_Knight_Fire_Weapon.prefab template-authoring=passed runtime-source=0 spawn=0 ai=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 armor=0 save=0
```

The isolated template proof marker is:

```text
DRAGON_KNIGHT_DK4A_TEMPLATE_FINALIZED_PASS explicitAssets=2; loadCycles=2; templateRegistered=false; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false
```

## Completed DK4A Template Checks

Decision 0010 produced:

- candidate-scoped serialized template probe output;
- explicit proof-profile approval;
- isolated Unity template authoring log;
- finalized template report;
- deterministic load/release proof for the two authored template assets;
- source hash checks for the baseline templates and metas;
- package hash checks for generated bundle, catalogue JSON, catalogue hash, and direct-root discovery copies when required;
- confirmation that no runtime actor was constructed and no save was accessed during template proof.

Later DK4 live actor work still needs eligible target source evidence, target `Location.ID`, actor `Location.ID`, FoAHost ownership/activation proof, cleanup proof, and a no-save runtime diagnostic before any movement, attacks, companion protection, phase combat, or AI source can proceed.

## Explicitly Still Not Authorized

This gate still does not authorize:

- changing Dragon Knight runtime source;
- changing Dragon Knight AI package source;
- changing Avalon Awakened, Avalon Companions, or Avalon AI Runtime source;
- mutating source Dragon Knight assets or source Unity packages;
- additional Unity project edits outside the completed Decision 0010 isolated template proof;
- additional `NpcTemplate` or `LocationTemplate` authoring outside the completed Decision 0010 isolated template proof;
- deployment of generated catalogues into FoA;
- launching FoA;
- opening, reading, or writing saves;
- calling `LocationTemplate.SpawnLocation`;
- implementing DK4 live actor observation source;
- actor movement, attacks, damage, death, loot, corpse, roaming, companion protection, follower mechanics, weapon items, armor items, phase combat, Rabbit, GOAP, PlayMaker, Blaze, or FoAHost binding.

## Exit Criteria

DK4A has passed isolated template proof because Decision 0010 supplies load-proven Dragon Knight `NpcTemplate` and `LocationTemplate` GUIDs.

DK4 source and any `SpawnLocation` path remain blocked until the later live diagnostic gate accounts for exact eligible target source evidence, target `Location.ID`, host ownership, activation, cleanup, no-save proof, and stop conditions.
