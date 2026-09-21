# Research: Offensive Creature Roster Expansion

Date: 2026-06-15
Scope: expand Avalon Companions from the initial animal roster into the full clean `ReviewQueueNonUniqueSpawnerBacked` creature set from FOA-Diagnostic Tool 0.4.3.

## Evidence read

- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/research/animal-roster-expansion-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `mods/template-diagnostics/docs/research.md`
- `mods/template-diagnostics/docs/validation-plan.md`
- Runtime dump: `<local-path>`

## Candidate review

All rows below came from `creature_templates.csv` with `reviewStatus=ReviewQueueNonUniqueSpawnerBacked`, `npcUnique=false`, `riskFlags=none`, and `safeSpawnCandidate=false` / `rosterApproved=false`.

| Candidate | Template | GUID | Spawner refs | Role |
| --- | --- | --- | ---: | --- |
| Wolf | `Spec_AnimalWolf` | `9086dee514edc644b9b55890d885db3f` | 26 | animal pet |
| Bear | `Spec_AnimalBear` | `c45508309b84907429f83d1361918fc2` | 6 | animal pet |
| Bear interaction | `Spec_AnimalBear_Interactions` | `63fa2f502163174468799959dd025319` | 1 | animal pet variant |
| Deer | `Spec_AnimalDeer` | `467a9c9208854394cbb77032251288a1` | 14 | passive animal pet |
| Cow | `Spec_AnimalCow` | `6217a5ecb1ff2914b854b9304d9bd54b` | 3 | passive animal pet |
| Pig | `Spec_AnimalPig` | `3e24c74d7d86f6743b05d4c3587de57d` | 3 | passive animal pet |
| Corpse eater | `Spec_EnemyMonster_T1_CorpseEater` | `1a41678c288c2264c8bcfad7a6eb3ba3` | 11 | offensive creature |
| Redcap | `Spec_EnemyMonster_T1_Redcap` | `a32e5074492cce34f89ff0667fdb41b7` | 10 | offensive creature |
| Flamegobbler | `Spec_EnemyMonster_T1_Flamegobbler` | `b6bf58d3c36663048bc83341ff0111d2` | 7 | offensive creature |
| Grindylow | `Spec_EnemyMonster_T1_Grindylow` | `fa79aaa0bff59484dab2cf35c5ea805c` | 6 | offensive creature |
| Wyrdspirit | `Spec_EnemyMonster_T1_Wyrdspirit` | `843643575fa01ba4292e60afb9291fea` | 6 | offensive creature |
| Sharg | `Spec_EnemyMonster_T2_ShargHoS` | `324e9b5ed131ce34eb12a520cdb2b52a` | 1 | offensive creature |
| Ogre | `Spec_EnemyMonster_T3_Ogre` | `3f7d4ccf62c440b40b1fca822ef6ac1b` | 1 | offensive creature |
| Bullrat | `Spec_EnemyMonster_T4_Bullrat` | `dc69c95f2c2930841aab6b7cfe48b4d5` | 1 | offensive creature |
| Zombie | `Spec_EnemyZombie_T1_Classic` | `1d110a8ec95ab1745a364562ec311e50` | 13 | undead/offensive |
| Harder zombie | `Spec_EnemyZombie_T1_ClassicHarder` | `2b46e149155c56441b7c91cb1745ec6c` | 7 | undead/offensive |
| Drowner | `Spec_EnemyZombie_T1_Drowner` | `bb613531c5d3bf5499ea3b8103a4024e` | 19 | undead/offensive |
| Armored drowner | `Spec_EnemyZombie_T1_DrownerFullArmor` | `b131b8352811a1b4488c21ae8682fbbe` | 1 | undead/offensive |
| Head-armored drowner | `Spec_EnemyZombie_T1_DrownerHeadArmor` | `cb6b4e81043784946bcbf56b91ebc968` | 6 | undead/offensive |
| Skeleton 1H | `Spec_EnemySkeleton_Melee1H` | `386190b9f098e414c8d88472306aaad8` | 10 | undead/offensive |
| Skeleton 2H | `Spec_EnemySkeleton_Melee2H` | `6af0ddfc613f7e44a81d40e2979f514b` | 8 | undead/offensive |
| Floatling | `Spec_SoS_EnemyMonster_T4_Floatling_WithSpawnAnimation` | `560b1076391307948b338acf33d85a1a` | 2 | offensive creature |
| Tadpole hatchery | `Spec_SoS_EnemyMonster_T4_TadpoleHatchery` | `cea93c44977ce3940be214156db7a0e1` | 3 | offensive creature |

## Decision

Add the missing rows from the table to the active Avalon Companions roster as explicit one-session candidates only. Existing wolf, bear, deer, pig, cow, and bullrat rows remain on the same path.

Every candidate must keep the same runtime boundary:

1. Spawn only after explicit player command.
2. Mark the spawned `Location` not saved.
3. Require the spawned location to expose `NpcElement`.
4. Apply the native summon faction override and `NpcHeroPetAlly`.
5. Track only the current live `Location` in memory.
6. Attach only the runtime-only `AvalonCompanionCommandAction`.
7. Support recall, dismiss, catch-up, and explicit Follow/Stay/Defend modes.
8. Use `NpcHeroPetAlly.EnterCombat()` only when the hero already has live attackers.

## Explicit exclusions

Do not add:

- `NeedsManualRiskReview` rows such as arena variants, Merlin trial variants, special Wyrd deer, swarm bees, or named/elite rat rows,
- `BlockedUniqueNpc` rows,
- `BlockedMissingNpcActorProof` rows,
- taxonomy-only rows without loaded spawner evidence,
- humanoid companion rows.

## Boundary

This does not approve:

- persistence or save/load support,
- custom combat target selection,
- custom attack commands,
- defend hotkeys,
- wild creature conversion,
- humanoid companions,
- quest/story actor edits,
- vanilla serialized interaction-list edits,
- multi-companion active squads.

## Validation needed

- Throwaway-save smoke test every new candidate: summon, confirm no immediate hostility, recall, dismiss, and inspect BepInEx log.
- For offensive and undead candidates, confirm Defend mode enters native combat only after the hero has a live attacker.
- Confirm no candidate attacks civilians or neutral actors without a hero-attacker relation.
- Confirm the native `Companion` prompt appears only on the active managed actor and disappears after dismiss.
- Confirm save/load is not used as a persistence claim.
