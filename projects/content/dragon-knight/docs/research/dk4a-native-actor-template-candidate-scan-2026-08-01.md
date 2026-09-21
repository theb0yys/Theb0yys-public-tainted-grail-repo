# DK4A Native Actor/Template Candidate Scan

Date: 2026-08-01

Result: **PASS for read-only candidate scan only; no Dragon Knight native baseline selected.**

No source, Unity project, template asset, catalogue, game install, config, save, runtime actor, `LocationTemplate.SpawnLocation` call, AI package, combat, companion, item, armor, roaming, or release artifact was changed.

## Scope

The current Dragon Knight blocker is native actor evidence. DK4 cannot observe or own a real Dragon Knight actor until Dragon Knight has either an existing live `Location` source or a proved `NpcTemplate` / `LocationTemplate` pair.

The scan therefore looked for native FoA actor/template candidates that could become a disposable Dragon Knight proof baseline in a later, separately authorized template-profile gate.

## Sources Reviewed

- `mods/dragon-knight/docs/research.md`
- `mods/dragon-knight/docs/decisions/0002-dk2-visual-assetbundle-proof.md`
- `mods/dragon-knight/docs/gates/DK4-live-actor-observation-host-ownership-source-gate-packet.md`
- `mods/avalon-awakened/docs/research/ci4-successor-goblin-serialized-native-template-probe-2026-07-31.md`
- `mods/avalon-awakened/docs/research/ci4-successor-goblin-highwayman-proof-profile-approval-2026-07-31.md`
- `mods/avalon-awakened/docs/research/ci4-successor-goblin-template-proof-2026-07-31.md`
- `mods/avalon-awakened/docs/research/ci4-successor-goblin-native-bootstrap-overlay-correction-2026-07-31.md`
- Latest installed template diagnostic dump: `<local-path>`

## Established Dragon Knight Facts

- The Dragon Knight asset proof is visual-only. The imported character prefabs have only `Animator`, `CapsuleCollider`, `Cloth`, `SkinnedMeshRenderer`, and `Transform` component types, the only usable imported clip is `Pose_Check`, and the controller has zero parameters and zero states.
- DK2 does not prove native actor construction, `NpcTemplate`, `LocationTemplate`, attack animation mapping, phase state transitions, health behavior, faction, loot, corpse, roaming, companion, or item behavior.
- DK4 source is explicitly blocked until Dragon Knight supplies exact `LocationTemplate` and `NpcTemplate` evidence, or an exact existing live `Location` actor source.
- No Dragon Knight source file may call `LocationTemplate.SpawnLocation` until the Dragon Knight native actor/template proof exists.

## Latest Native Candidate Rows

The latest local diagnostic dump has these relevant native candidates:

| Candidate | Evidence | Use status |
|---|---|---|
| Foredweller T6 Knight | `npc_audio_archetypes.csv:197` links `NPCTemplate_EnemyForedweller_T6_Knight` GUID `439a2dc3cf64e4e4aa792be5b4034cf1` to `Spec_EnemyForedweller_T6_Knight` GUID `24ee850d0ffdbf64ea2b07b6a718d63b`; the row is `npcType=Normal`, `isHumanoid=true`, `isSummon=false`. `templates.csv:7118` records the location as a regular non-unique `LocationTemplate` with `RepetitiveNpcAttachment`. | Best Dragon Knight-shaped candidate from name and native flags, but it still needs a serialized native-template probe before adoption. |
| Highwayman 1H | `npc_audio_archetypes.csv:499` links `NPCTemplate_Enemy_Generic_Tier2_Highwayman_1H` GUID `a679d49cb1406e9429ff9534aa282515` to six repetitive actor templates; `templates.csv:7434` records `Spec_Enemy_Generic_Tier2_Highwayman_1H` GUID `c7b737af7d133f64f9c088fcf74242c2` as regular, non-unique, actor-like, and `RepetitiveNpcAttachment`. | Fastest proven fallback because the Goblin lane already serialized this exact pair. Needs explicit Dragon Knight approval because it carries human-bandit stats, faction, loot, and fighting-style assumptions. |
| Drowned Knight T6 | `npc_audio_archetypes.csv:886-892` record Drowned Knight scalable NPC templates as humanoid elite/miniboss candidates; `templates.csv:8000-8006` records matching regular, non-unique, repetitive location templates. | Strong thematic candidate, but it is a scaling/elite/mini-boss family and needs a serialized probe before any proof-profile decision. |
| Skeleton Archmage Boss | `npc_audio_archetypes.csv:181` links `NPCTemplate_EnemyBoss_Skeleton_Archmage` GUID `6cf2a0025d3646c45a42fa15b153ef57` to `Spec_EnemyBoss_Skeleton_Archmage` GUID `26c0fc959a7c18c48858bb1ea0fb8fff`; `templates.csv:7084` records the location as regular, non-unique, and repetitive. | Candidate only. It is a boss/skeleton archetype, not a knight baseline, and needs a serialized probe plus explicit approval. |
| Green Knight Sir Bertilak | `npc_audio_archetypes.csv:112` links `NPCTemplate_Cuanacht_GreenKnight_SirBertilak` GUID `4fd2d5887d6cfef469edacc6c6228784` to `Spec_NPC_Special_GreenKnight_SirBertilak` GUID `d182cd1da53ddc54dbfecc7ccc34e1b1`; `templates.csv:7938` records the location as unique. | Blocked for disposable proof unless explicitly approved despite unique quest/NPC risk. |
| Summon Knight | `npc_audio_archetypes.csv:731` links `NPCTemplate_Summon_Knight_Placeholder` GUID `7f020d39fe704274d80ac10881601431` to `Spec_Summon_Knight` GUID `fc2ce417d7c1cde459a96424c95b1de4`; `templates.csv:8067` records the location as non-unique and repetitive. | Not selected for boss proof because the name marks it as a summon/placeholder lane. |

## Proven-Method Implications

The Goblin path proves the required sequence:

1. run or read a candidate-scoped serialized native-template probe;
2. get explicit approval for the exact proof profile values;
3. author isolated pack-owned `NpcTemplate` and `LocationTemplate` assets;
4. keep runtime source, deployment, FoA launch, save access, F-key executor work, actor spawn, combat, death, loot, corpse, and population blocked until later gates.

The Goblin correction also proves that a visual-only custom prefab is not necessarily a native actor-controller root. The safe Dragon Knight actor proof must either:

- use a native bootstrap visual from the approved baseline for `SpawnLocation` and attach the Dragon Knight Iron/Fire visuals as proof overlays after native actor initialization; or
- separately prove a Dragon Knight native ModService/Addressables visual pack and native actor-controller composition before using the Dragon Knight visual directly in `LocationTemplate`.

DK2 only proves a Dragon Knight-owned AssetBundle visual path. It does not prove the native ModService/Addressables visual route required for a native template visual field.

## Decision

No Dragon Knight native baseline is selected by this scan.

The recommended next safe path is a DK4A serialized native-template probe against the Foredweller T6 Knight pair:

- `NPCTemplate_EnemyForedweller_T6_Knight` GUID `439a2dc3cf64e4e4aa792be5b4034cf1`
- `Spec_EnemyForedweller_T6_Knight` GUID `24ee850d0ffdbf64ea2b07b6a718d63b`

That probe must record exact stats, faction, abstract types, fighting style, loot/corpse policy, visual/bootstrap address, location component order, attachment policy, source hashes, and whether the template can be safely copied as a disposable Dragon Knight proof baseline.

The fallback fastest path is explicit user approval to adopt the already serialized Highwayman 1H proof values for a disposable Dragon Knight template proof. That is not selected here because it would be a Dragon Knight-specific design decision, not a scan result.

## Remaining Blockers

- No approved Dragon Knight native baseline.
- No serialized Foredweller T6 Knight profile yet.
- No approved Dragon Knight `NpcTemplate` GUID.
- No approved Dragon Knight `LocationTemplate` GUID.
- No approved native bootstrap visual address.
- No native ModService/Addressables proof for Dragon Knight visuals.
- No exact Dragon Knight live `Location.ID`.
- No exact eligible target source or target `Location.ID`.
- No animation/combat evidence for attacks or real two-phase behavior.

## Not Authorized By This Scan

- Template authoring.
- Runtime validator source.
- DK4 diagnostic source.
- FoA deployment or launch.
- Save access.
- `LocationTemplate.SpawnLocation`.
- Actor movement, attacks, damage, death, loot, corpse, roaming, companion protection, follower behavior, item registration, armor registration, or AI host binding.
