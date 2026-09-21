# Safe Proof Spawn Target - 2026-06-15

## Purpose

Pick one explicit human NPC `LocationTemplate` GUID for the first throwaway-save proof spawn test.

This target is approved only for validating that the safe spawn command can resolve, instantiate, mark not saved, and cleanly log one non-unique NPC. It is not approved for companion behavior, recruitment, control, faction changes, route population, or save persistence.

## Source evidence

- `mods/template-diagnostics/docs/generated/route-validation-highwayman-bandit-outlaw-20260614-221744.md`
- `mods/template-diagnostics/docs/generated/route-validation-highwayman-bandit-outlaw-20260614-221744.csv`
- `mods/template-diagnostics/docs/generated/manual-gate-review-highwayman-bandit-outlaw-20260614-221744.md`
- `mods/template-diagnostics/docs/generated/manual-gate-review-highwayman-bandit-outlaw-20260614-221744.csv`

The manual gate evidence marks every reviewed highwayman, bandit, and outlaw row as `PassNpcUniqueFalse`. These rows are still blocked for placement, density, and save/load route prototypes.

## Selected target

| Field | Value |
| --- | --- |
| Validation row | `RD-HBO-047` |
| Template name | `Spec_Enemy_Generic_Tier1_Outlaw_1H` |
| Template GUID | `2bd34a05d1e1fb94f9770b9ee7f23be2` |
| Template CLR type | `Awaken.TG.Main.Locations.Setup.LocationTemplate` |
| Template type | `Regular` |
| Is abstract | `false` |
| Actor category | `actor-npc-repetitive` |
| Actor-like | `true` |
| NPC unique | `false` |
| NPC attachment | `Awaken.TG.Main.Fights.NPCs.RepetitiveNpcAttachment` |
| Host evidence | `/T1_SpawnerEnemies_Outlaw_1H` in `CampaignMap_HOS_merged` |
| Host position evidence | `-1936.794|69.519|-3575.957` |
| Manual gate status | `BlockedUntilPlacementDensitySaveLoadProof` |

## Selection rationale

- It is a generic tier-1 human enemy template, not a named, elite, boss, quest, tutorial, or special actor row.
- It has recorded `NpcUnique=false` and `RepetitiveNpcAttachment` evidence.
- It has vanilla spawner usage evidence, which reduces the chance that the template is editor-only or malformed.
- It is lower tier than the P1 highwayman rows, so it is the conservative first hostile-human proof target even though it remains blocked for route population.

## Required config for the throwaway-save proof

```ini
[Target]
TemplateName = Spec_Enemy_Generic_Tier1_Outlaw_1H
TemplateGuid = 2bd34a05d1e1fb94f9770b9ee7f23be2

[ProofSpawn]
EnableSafeSpawnCommand = true
SafeSpawnHotkey = F8
SafeSpawnDistance = 4
LimitOneProofSpawnPerSession = true
```

## Expected proof result

- The plugin resolves `Spec_Enemy_Generic_Tier1_Outlaw_1H`.
- The unique-template guard does not block it.
- One NPC location spawns behind the hero.
- The spawned location is immediately marked `MarkedNotSaved=true`.
- The post-spawn `NpcElement.IsUnique` guard passes.
- The log states that no recruitment, faction edit, ally marker, follow command, dialogue, story, crime, or persistence behavior was applied.

## Hard limits

- Use only on a throwaway save.
- Do not test in a settlement, quest scene, dialogue scene, cutscene, interior transition, boss arena, or crowded road encounter.
- Expect hostile behavior because this is an outlaw enemy template.
- Do not treat a successful spawn as follower proof.
- Do not keep playing from the test save after spawning.
- Delete or abandon the throwaway save after the proof.
