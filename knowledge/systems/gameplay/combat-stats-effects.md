# Combat Stats, Costs, Poise, and Stagger

Use this page when you want to change **stamina costs, parry/block behavior, attack feel, poise damage, or other combat values that are already exposed as native stats**.

The practical rule is:

> If FoA already represents the behavior as a native stat, change that stat or apply a scoped tweak before rewriting the action/animation that consumes it.

## Where combat values live

FoA separates several layers:

- action/state execution;
- `CharacterStats` / `HeroStats`;
- `ItemStats` / `ItemStat`;
- temporary `StatTweak` modifiers;
- `Damage` / `DamageParameters`;
- NPC combat/AI state.

Useful researched examples include:

- `CharacterStats.StaminaUsageMultiplier`
- `HeroStats.ParryWindowBonus`
- `HeroStats.ParryStaminaDamageMultiplier`
- `HeroStats.BlockingStaminaDamageMultiplier`
- `ItemStats.ParryStaminaCost`
- `ItemStats.BlockStaminaCostMultiplier`
- `ItemStats.HoldItemCostPerTick`
- `ItemStats.PoiseDamage`
- `ItemStats.PoiseDamageHeavyAttackMultiplier`
- `ItemStats.PoiseDamagePushMultiplier`
- `ItemStat.ModifiedValue`
- `DamageParameters.PoiseDamage`
- `NpcStats.PoiseThreshold`

## Character/Hero stat tweaks

A common route is:

~~~text
native stat wrapper initializes
→ mod adds non-saved StatTweak
→ native action reads ModifiedValue
→ config/owner changes
→ tweak updates or is removed
~~~

Useful examples:

- broad action stamina multiplier → `CharacterStats`;
- parry window → `HeroStats`;
- block/parry/hold costs → `ItemStats`.

## Item stat tweaks

For item-owned combat values:

1. identify the exact `ItemStat`;
2. verify the item owner;
3. verify equipped state/slot when relevant;
4. apply the scoped runtime tweak;
5. remove it when the item/owner no longer qualifies.

Avoid mutating the `ItemTemplate` for a temporary runtime effect when an instance/stat tweak can express the change.

## Poise is not stagger

The researched poise path is:

~~~text
DamageParameters.PoiseDamage
→ NpcGeneralFSM.OnDamageTaken
→ EnemyBaseClass.DealPoiseDamage
→ NpcStats.PoiseThreshold accumulated meter
→ poise break
~~~

Stagger is a different behavior with different conditions.

Do not force stagger just because you want stronger poise effects.

## Event-local poise changes

One narrow pattern is:

1. capture the original `Damage.Parameters.PoiseDamage`;
2. apply a scaled value for the intended processing call;
3. let native poise logic run;
4. restore the original payload.

That avoids leaking a temporary multiplier to later consumers.

## Common mistakes

### Treating NpcStats.PoiseThreshold as a simple threshold constant

Research indicates it behaves as an accumulated `LimitedStat` meter. Writing it can change current poise state, not merely difficulty.

### Patching ItemStat.ModifiedValue globally without checks

This getter is broad. Verify:

- stat type;
- item owner;
- equipped state;
- intended feature condition.

### Rewriting combat actions when a native stat already controls the value

That increases compatibility/lifecycle risk unnecessarily.

### Saving temporary tuning

A runtime combat tweak should not silently become persistent base-state mutation unless that is intentional.

## How to verify stat-based changes

Verify:

1. correct stat owner;
2. tweak added once;
3. `ModifiedValue` changes as expected;
4. the intended native action reads that value;
5. unrelated actors/items remain unchanged;
6. config/disable updates or removes the tweak;
7. temporary state is not accidentally saved.

For poise, also prove the original damage payload is restored and stagger is not forced unintentionally.

## Evidence limits

The ownership/stat mappings above are source/decompilation-backed and used by project implementations.

Exact combat feel, exclusions, tooltip/UI refresh, and cross-build behavior still require feature-specific runtime testing.
