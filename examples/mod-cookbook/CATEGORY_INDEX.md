# Cookbook Category Index

This index is the broad map. A category appears here even when its current evidence ceiling is too low for a drop-in example.

| Category | Public starting point | Current source-path ceiling |
| --- | --- | --- |
| Config / hotkeys | level 00/01 guides | runtime-smoke pattern |
| Harmony postfix/prefix | proven-path templates | mechanism-backed |
| Stamina | 01 stamina drain | SOURCE_BUILD_EVIDENCED |
| Carry capacity | 02 carry capacity | LOAD_EVIDENCED |
| Skill caps / progression | recipe 01 | LOAD_EVIDENCED |
| Fall damage | 03 no fall damage | SOURCE_BUILD_EVIDENCED |
| Magic projectile speed | 04 projectile speed | RUNTIME_EVIDENCED |
| Mana cost | 09 mana cost | SOURCE_BUILD_EVIDENCED |
| Magic damage | 10 magic damage | SOURCE_BUILD_EVIDENCED |
| Status buildup | 13 status buildup | SOURCE_BUILD_EVIDENCED |
| Status application observation | 28 status application observer | SOURCE_BUILD_EVIDENCED |
| Active status observation | 29 active status observer | SOURCE_BUILD_EVIDENCED |
| Character state observation | 30 character-state observer | SOURCE_BUILD_EVIDENCED for selected state reads |
| Buff/debuff tuning | recipe 10 | SOURCE_CONFIRMED stat surfaces; consumer semantics NOT_PROVEN |
| Consumable use observation | 31 consumable use observer | SOURCE_BUILD_EVIDENCED |
| Healing/recovery observation | 32 healing and recovery observer | SOURCE_BUILD_EVIDENCED |
| Consumable status deltas | 33 status cure/removal observer | SOURCE_BUILD_EVIDENCED |
| Consumable effect attribution | recipe 11 | SOURCE_BUILD_EVIDENCED attribution shape; exact public recipe NOT_RUN |
| Equipment change observation | 34 equipment change observer | SOURCE_CONFIRMED item-level equip lifecycle |
| Main/off-hand observation | 35 main/off-hand observer | SOURCE_BUILD_EVIDENCED selected read surfaces |
| Weapon visibility/state observation | 36 weapon visibility/state observer | LOAD_EVIDENCED read surface; exact public example NOT_RUN |
| Equipment lifecycle attribution | recipe 12 | SOURCE_CONFIRMED lifecycle map; public recipe NOT_RUN |
| Combat state observation | 37 combat state observer | SOURCE_BUILD_EVIDENCED |
| Guard / block / parry observation | 38 guard / block / parry observer | SOURCE_BUILD_EVIDENCED guard entry; RUNTIME_EVIDENCED damage-result fields |
| Attack / cast lifecycle observation | 39 attack / cast action observer | SOURCE_CONFIRMED lifecycle seams |
| Combat action lifecycle attribution | recipe 13 | SOURCE_CONFIRMED lifecycle map; public recipe NOT_RUN |
| Item stats | content/01 | STATIC_CONFIRMED |
| Weapons | content/02 | STATIC_CONFIRMED |
| Armour | content/03 | STATIC_CONFIRMED |
| Creatures / NPCs | content/04 | STATIC_CONFIRMED |
| Merchant gold | 11 merchant gold floor | SOURCE_CONFIRMED |
| Merchant stock | 12 restock on open | SOURCE_BUILD_EVIDENCED |
| Illegal pickup / interaction guard | 06 | RUNTIME_EVIDENCED |
| HUD | 05 | RUNTIME_EVIDENCED |
| Small runtime overlay | proven-path/03 | mechanism-backed |
| Footstep replacement | 07 | RUNTIME_EVIDENCED |
| Contextual music | 19 context lane observer + recipe 03 | RUNTIME_EVIDENCED for selected lane decisions |
| Skybox ownership | proven-path/05 | source mechanism with owner-side live apply evidence |
| Held/helper lighting | 18 personal helper light + recipe 02 | SOURCE_BUILD_EVIDENCED |
| Movement extra jump | 08 | LOAD_EVIDENCED |
| Movement FOV kick | 17 | LOAD_EVIDENCED |
| Camera shake | 20 | LOAD_EVIDENCED |
| FOV transition duration | 21 | SOURCE_BUILD_EVIDENCED |
| Native music suppression | 22 | LOAD_EVIDENCED |
| Character damage observation | 23 | RUNTIME_EVIDENCED |
| Combat VFX sidecars | recipe 09 | RUNTIME_EVIDENCED owner path; public recipe NOT_RUN |
| Head bob | 24 | LOAD_EVIDENCED |
| Motion blur | 25 | LOAD_EVIDENCED |
| Move/sprint speed | 26 | LOAD_EVIDENCED |
| Damage numbers | 27 | LOAD_EVIDENCED |
| Save lifecycle observation | 14 | LOAD_EVIDENCED |
| Save backups | recipe 07 | LOAD_EVIDENCED; archive creation not proved |
| Dialogue offered | 15 | LOAD_EVIDENCED |
| Quest completion candidates | 16 | LOAD_EVIDENCED |
| Dialogue/quest mutation | recipe 08 | observation paths only; mutation needs separate proof |
| Loot / corpse loot | recipe 04 | STATIC_CONFIRMED authoring surface |
| Crafting recipes | recipe 05 | SOURCE_CONFIRMED validation prototype; persistence unproved |
| Spell VFX overlay | recipe 06 | SOURCE_CONFIRMED |
| Weather | existing weather/sky docs; no tiny promoted recipe yet | mixed/partial |
| Frameworks | 04-framework | architecture guidance, not a finished framework copy |

## Rule

The table is not a ranking of maturity. It is an evidence map.

When a source path says `LOAD_EVIDENCED`, that means the underlying mod reached load/registration evidence—not that every feature behavior was proven.

Every rewritten public example remains `NOT_RUN` until that exact public file set is built and exercised. The cookbook uses `NEEDS_VALIDATION` as a workflow marker for that pending work; it does not replace the formal `NOT_RUN` execution status.
