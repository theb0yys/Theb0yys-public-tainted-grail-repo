# DK4A Foredweller Serialized Native-Template Probe

Date: 2026-08-01

Result: **PASS for read-only serialized native-template evidence; Dragon Knight baseline not selected.**

No Unity project was modified. Unity was not launched. No template asset, catalogue, game install, config, save, runtime source, actor construction, `LocationTemplate.SpawnLocation` call, AI package, movement, attack, phase-combat, companion, item, armor, roaming, or release artifact was changed.

## Inputs

- Gate: `mods/dragon-knight/docs/gates/DK4A-native-actor-template-proof-profile.md`
- Candidate scan: `mods/dragon-knight/docs/research/dk4a-native-actor-template-candidate-scan-2026-08-01.md`
- Parser reference: `mods/avalon-awakened/tools/creature-successor/GoblinSerializedNativeTemplateProbe.py`
- Isolated native project: `<local-path>`

Generated report:

- `mods/dragon-knight/docs/generated/dragon-knight-dk4a-foredweller-template-probe-2026-08-01.json`
- SHA-256: `FAB32552B56DE4BFC5FC8B367A89FD2EDCC01672AB913785CC9A6579FFDC417F`

The probe used the proven Goblin serialized-template parser against the isolated project's Unity YAML prefab and `.meta` files. The bounded GUID index covered `Assets\Data` and `Assets\Code`, count `17107`, so source templates and script component paths could resolve without crawling unrelated Unity packages.

## Files Inspected

| Role | Asset | GUID | SHA-256 | Bytes |
|---|---|---|---|---:|
| Abstract parent | `Assets\Data\Templates\NpcTemplates\Abstracts\Abstract_NPCTemplate_Foredweller.prefab` | `d11cfa3551773034eacc4e3d4cec7183` | `AC3FF9FE750BF5820F14FF52A8B2BFBF63C0D951D022E0B17F0746119BEF4608` | 4635 |
| Candidate NPC | `Assets\Data\Templates\NpcTemplates\Enemies\Foredwellers\NPCTemplate_EnemyForedweller_T6_Knight.prefab` | `439a2dc3cf64e4e4aa792be5b4034cf1` | `95E860FC25705F3D8FAD997B20CD260D1A38271723B587CEA47BDE741106E933` | 6366 |
| Candidate location | `Assets\Data\LocationSpecs\AI\Enemies\ForeDwellers\Spec_EnemyForedweller_T6_Knight.prefab` | `24ee850d0ffdbf64ea2b07b6a718d63b` | `03C7E647295DF1D09541139D9280C3A4DDD356EB543BF060B9CCE455630A91BD` | 9302 |

All inspected GUIDs matched the expected diagnostic GUIDs.

## Serialized NPC Findings

`NPCTemplate_EnemyForedweller_T6_Knight` is direct-serialized, not a prefab variant in the probed chain.

Effective component order:

- `Transform`
- `NpcTemplate`

Serialized values:

| Field | Value |
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
| `npcType` | `2` |
| Dead body lootable | `1` |
| Abstract type GUIDs | `2d89c9bd6158a1049b7460cee96ed7dd`, `30fee903493e89e4089f2aeeb5e42d2a`, `d11cfa3551773034eacc4e3d4cec7183` |

The probe did not find serialized loot-table references in the probed chain. That means loot/reward behavior is not closed by this probe and still needs an explicit policy before template authoring.

## Serialized Location Findings

`Spec_EnemyForedweller_T6_Knight` is direct-serialized, not a prefab variant in the probed chain.

Effective component order:

- `Transform`
- `LocationSpec`
- `LocationTemplate`
- `RepetitiveNpcAttachment`
- `IdleDataAttachment`
- `CustomCombatAttachment`
- `AliveAudioAttachment`
- `MarkerAttachment`

Serialized values:

| Field | Value |
|---|---|
| Display-name override | `enemy_foredweller_knight` |
| Display-name ID | `Template/displayName_9102533663763092846_24ee850d0ffdbf64ea2b07b6a718d63b` |
| Linked NPC template | `439a2dc3cf64e4e4aa792be5b4034cf1` |
| Native visual prefab address | `b4d3a4e9a58fb1c4ea4c239f267faa56` |
| Simplified dead-body prefab address | `cee084d53322ec04ca39752e9754314a` |
| Hit VFX address | `6435b816784ea434589f4b48e31ccf70` |
| Snap to ground | `1` |
| Weapons always equipped | `1` |

The probe did not find serialized values for `potentialActors.Array.size`, `usesCombatMovementAnimations`, `usesAlertMovementAnimations`, or `canMoveInSpawn` in the probed chain.

## Decision

This probe resolves the narrow blocker that Foredweller T6 Knight lacked serialized template evidence.

It does **not** select Foredweller T6 Knight as the Dragon Knight baseline. It supplies enough evidence for the next proof-profile decision.

The next safe gate is explicit profile approval:

1. approve the Foredweller-derived disposable Dragon Knight profile values as written here;
2. approve the same baseline but override exact stats, faction, loot, corpse, display name, native bootstrap visual, or overlay policy in writing; or
3. reject Foredweller T6 Knight and select another exact native baseline for probe.

## Remaining Blockers

- Final Dragon Knight `NpcTemplate` GUID is not selected.
- Final Dragon Knight `LocationTemplate` GUID is not selected.
- Display name, localization key, faction policy, loot policy, corpse policy, reward policy, and stat policy are not approved for Dragon Knight.
- The native bootstrap visual address is known from Foredweller but not approved for Dragon Knight use.
- Dragon Knight Iron/Fire visual overlay policy remains visual-only and not native-template-ready.
- No live Dragon Knight `Location.ID` exists.
- No eligible target source or target `Location.ID` is proven.
- No attack, damage, death, loot, corpse, companion, armor, item, roaming, AI, or real two-phase combat behavior is proven.

## Not Authorized

- Template authoring.
- Runtime source or DK4 diagnostic source.
- Catalogue build or deployment.
- FoA launch.
- Save access.
- `LocationTemplate.SpawnLocation`.
- AI host binding, Rabbit, GOAP, PlayMaker, Blaze, movement, attacks, phase combat, companion protection, follower mechanics, weapon items, armor items, roaming, loot, reward, corpse, or release packaging.
