# Tune Poise Without Rewriting Stagger

FoA poise break and stamina-driven stagger are separate systems. Tune the incoming poise contribution instead of treating `NpcStats.PoiseThreshold` as a simple threshold setting.

Working lineage: [Poise Is Not Stagger](../../../research/case-studies/combat/poise-not-stagger.md).

## Native poise path

The useful runtime chain is:

~~~text
Damage.Parameters.PoiseDamage
→ NpcGeneralFSM.OnDamageTaken(DamageOutcome)
→ EnemyBaseClass.DealPoiseDamage(...)
→ NpcStats.PoiseThreshold accumulated meter
→ native PoiseBreakBehaviour
~~~

`NpcStats.PoiseThreshold` is an accumulated limited meter with an upper limit. Directly stat-tweaking it can change the current meter itself, which is not the same thing as changing how much poise damage an attack contributes.

## Patch the damage gateway

The working implementation patches:

~~~text
Awaken.TG.Main.Animations.FSM.Npc.Machines.NpcGeneralFSM.OnDamageTaken(DamageOutcome)
~~~

with a Harmony **prefix and postfix**.

The prefix:

1. gets `damageOutcome.Damage`;
2. ignores null/non-poise damage;
3. optionally limits the change to hero-originated damage;
4. copies `Damage.Parameters`;
5. stores the original `PoiseDamage`;
6. writes a scaled `PoiseDamage` into the parameters;
7. returns a small state object containing the original value.

The postfix restores the original value immediately after native NPC damage handling.

~~~csharp
private static void Prefix(DamageOutcome outcome, ref PatchState? __state)
{
    Damage damage = outcome.Damage;
    if (damage == null || damage.PoiseDamage <= 0f)
        return;

    DamageParameters parameters = damage.Parameters;
    float original = parameters.PoiseDamage;

    parameters.PoiseDamage = Math.Max(0f, original * multiplier);
    damage.Parameters = parameters;

    __state = new PatchState(damage, original);
}

private static void Postfix(PatchState? __state)
{
    if (__state == null)
        return;

    DamageParameters parameters = __state.Damage.Parameters;
    parameters.PoiseDamage = __state.OriginalPoiseDamage;
    __state.Damage.Parameters = parameters;
}
~~~

This lets the native `OnDamageTaken` → `DealPoiseDamage` route consume the temporary value without permanently rewriting the damage object.

## Per-hit categories already available

The working process can classify from `DamageParameters`:

- `IsHeavyAttack`;
- `IsPush`;
- `IsFromProjectile`;
- `IsDamageOverTime`;
- `DamageType.MagicalHitSource`;
- projectile presence on `Damage.Projectile`.

That makes it possible to compose a base poise multiplier with heavy/push/projectile/magic/DOT-specific multipliers while still handing the result back to the same native poise system.

## What not to modify

Do not turn this into a stagger rewrite.

Stagger has separate stamina-depletion/explicit-enter behavior. Leave these alone unless you are deliberately building a stagger mod:

- `EnemyBaseClass.OnStaminaChanged`;
- `EnemyBaseClass.EnterPoise`;
- `StaggerBehaviour.UpdateStaggerDuration`.

## Practical test

Use one known enemy and one attack type first:

1. log native and applied `PoiseDamage`;
2. verify ordinary damage is unchanged;
3. verify poise break occurs earlier/later as expected;
4. verify stamina stagger behavior did not change;
5. disable the multiplier and confirm native behavior returns.
