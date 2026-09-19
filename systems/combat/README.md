<!-- Canonical Wave 5 native-system page split from docs/reference/COMBAT_STATS_EFFECTS.md. -->
# Combat, Stats, Costs, Poise, and Stagger

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/combat/stat-and-poise-tuning.md).

## What this system is

FoA combat frequently separates:

- action/state execution;
- character stats;
- item stats;
- temporary stat tweaks;
- damage payloads;
- AI/combat state machines.

A useful rule from the research is:

> If the game already exposes the behavior through a native stat owner, prefer a scoped native stat/tweak route before rewriting the action or animation that consumes it.

## Who owns it in FoA

Relevant owners include:

- `CharacterStats` / `HeroStats`;
- `ItemStats` / `ItemStat`;
- `StatTweak`;
- hero combat states such as block/parry/heavy attack;
- `Damage` / `DamageParameters`;
- NPC combat/AI owners such as `NpcGeneralFSM` and `EnemyBaseClass`.

## Important identities, types, and methods

Researched examples include:

- `CharacterStats.StaminaUsageMultiplier`;
- `HeroStats.ParryWindowBonus`;
- `HeroStats.ParryStaminaDamageMultiplier`;
- `HeroStats.BlockingStaminaDamageMultiplier`;
- `ItemStats.ParryStaminaCost`;
- `ItemStats.BlockStaminaCostMultiplier`;
- `ItemStats.HoldItemCostPerTick`;
- `ItemStats.PoiseDamage`;
- `ItemStats.PoiseDamageHeavyAttackMultiplier`;
- `ItemStats.PoiseDamagePushMultiplier`;
- `ItemStat.ModifiedValue`;
- `DamageParameters.PoiseDamage`;
- `NpcStats.PoiseThreshold`.

Useful lifecycle/patch points found in the working research include stat-wrapper initialization, exact action methods, and the NPC damage-processing state.

## Where it exists in the lifecycle

### Hero/character stat tweaks

~~~text
native stat wrapper initializes
→ mod adds non-saved StatTweak
→ native action reads ModifiedValue
→ mod removes/discards tweak when disabled/owner changes
~~~

### Item stat tweaks

~~~text
ItemStats initializes
→ hero-owned item receives scoped non-saved tweak
→ attack/block/parry consumer reads ItemStat
→ owner/item changes
→ tweak removed
~~~

### Poise processing

The research distinguishes poise break from stagger:

~~~text
attack/item/damage source
→ DamageParameters.PoiseDamage
→ NpcGeneralFSM.OnDamageTaken
→ EnemyBaseClass.DealPoiseDamage
→ accumulated NpcStats.PoiseThreshold meter
→ poise break when meter reaches max
~~~

Stagger is a separate behavior tied to other conditions such as stamina depletion or explicit entry.

## Current proof boundary

This page summarizes source/decompilation-backed combat ownership and implemented patterns from the working repository.

Exact gameplay feel, boss exclusions, tooltip refresh behavior, and cross-build compatibility remain feature-specific runtime validation questions.
