# Combat, Stats, Costs, Poise, and Stagger

> **Reference page.** Use this when changing combat feel, stamina costs, parry/block behavior, poise, stagger, or item-driven combat values.

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

## How we interact with it

### Prefer stat ownership for persistent combat-feel changes

Examples:

- action stamina multiplier → CharacterStats;
- parry window → HeroStats;
- parry/block/hold costs → ItemStats;
- equipped-weapon cost effect → `ItemStat.ModifiedValue` with strict owner/type checks.

### Scope tweaks to the real owner

If a tweak should affect only the hero:

- verify `Hero.Current`;
- verify item owner;
- verify equipped slot when relevant;
- mark temporary tweak not saved where appropriate;
- remove it when no longer applicable.

### For event-local changes, restore after the event

The poise research chose a narrow prefix/postfix strategy:

1. capture original `Damage.Parameters.PoiseDamage`;
2. write a scaled copy only for the intended processing call;
3. allow native poise logic to run;
4. restore original payload afterward.

This avoids leaking the multiplier to unrelated later listeners.

## Why this route

The research repeatedly avoided broader rewrites:

- parry timing can be changed through `ParryWindowBonus` instead of animation rewrites;
- block/parry costs can use native ItemStats rather than template mutation;
- poise damage can be scoped at the damage-processing boundary instead of modifying the NPC poise meter/animation state directly.

This preserves more of the native combat lifecycle.

## What goes wrong

### Poise and stagger treated as the same system

They are separate native behaviors.

Directly forcing stagger because you want stronger poise effects changes a different system.

### Modifying `NpcStats.PoiseThreshold` as if it were only a threshold scalar

Research indicates it is an accumulated `LimitedStat` meter with an upper limit. Mutating it can alter current poise state, not just difficulty.

### Item template mutation for temporary combat tuning

Changes the definition and can have wider/save-visible effects when a runtime stat tweak would suffice.

### Global hot getter patch without ownership checks

`ItemStat.ModifiedValue` is broad. A safe patch must verify:

- stat type;
- item owner;
- equipped state;
- intended hero/mod condition.

### Action rewrite when post-action correction is enough

Some movement-cost experiments snapshot native stamina before/after the action, then adjust only the observed cost. That can be safer than replacing movement logic, but still needs exact action validation.

## How to verify

For a stat-based change, prove:

1. correct native stat owner;
2. tweak added once;
3. `ModifiedValue` changes as intended;
4. native action consumes the changed value;
5. unrelated owners/items remain unchanged;
6. disable/config change removes or updates tweak;
7. save state does not accidentally persist temporary tweaks;
8. combat result matches expected direction.

For poise:

1. classify attacker/damage type;
2. capture original poise damage;
3. apply scoped multiplier;
4. native poise meter changes;
5. poise-break state triggers as expected;
6. original damage payload is restored;
7. stagger is not unintentionally forced.

## Current proof boundary

This page summarizes source/decompilation-backed combat ownership and implemented patterns from the working repository.

Exact gameplay feel, boss exclusions, tooltip refresh behavior, and cross-build compatibility remain feature-specific runtime validation questions.
