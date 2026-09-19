<!-- Canonical Wave 5 mechanic page split from docs/reference/COMBAT_STATS_EFFECTS.md. -->
# Combat, Stats, Costs, Poise, and Stagger — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/combat/README.md) first for ownership, identities, lifecycle, and proof scope.

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

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/combat/README.md).
