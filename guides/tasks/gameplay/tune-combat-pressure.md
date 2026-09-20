# Tune Combat Pressure Without Raw Damage Multipliers

Use the native coordination and stamina systems instead of replacing combat AI or multiplying all damage/health.

Working lineage: [Combat Pressure Without Raw Damage Multipliers](../../../research/case-studies/combat/pressure-not-damage.md).

## Runnable source

Start with the [Combat pressure example](../../../examples/mono/combat/combat-pressure/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Runnable source

Start from the buildable example: [Combat pressure and poise](../../../examples/mono/combat/combat-pressure-poise/README.md). Build it unchanged first, confirm the documented behavior, then make one change at a time.


## Native surfaces

The working combat-feel implementation uses three existing FoA inputs:

- `Awaken.TG.Main.Settings.Gameplay.Difficulty.MaxEnemiesAttacking`
- `Awaken.TG.Main.Settings.Gameplay.Difficulty.AttackActionUnBookProlong`
- `Awaken.TG.Main.Character.CharacterStats.CharacterStatsWrapper.Initialize`

The first two are patched as getter postfixes. The third is patched after character stats initialize so a runtime-only stamina tweak can be attached to the **current** hero stat instance.

FoA consumes these values through its own combat systems:

- `AttackBehaviour.UseConditionsEnsured()` compares booked attacks with `MaxEnemiesAttacking`;
- `CombatDirector.UnBookAttackAction(...)` uses `AttackActionUnBookProlong` when releasing attack bookings;
- negative stamina use is multiplied through `CharacterStats.StaminaUsageMultiplier`.

## Enemy group pressure

Patch the `MaxEnemiesAttacking` getter and adjust only the returned value.

Keep the result bounded. You are changing how many attack actions can be booked at once, not replacing `CombatDirector`.

~~~text
native Difficulty.MaxEnemiesAttacking
→ postfix
→ bounded adjusted slot count
→ AttackBehaviour continues natively
~~~

## Attack-turnover pressure

Patch `AttackActionUnBookProlong` with the same result-adjustment pattern.

Lower values release an attack booking sooner; higher values keep it occupied longer.

~~~text
native attack completes
→ CombatDirector.UnBookAttackAction
→ active Difficulty.AttackActionUnBookProlong
→ booking released by native director
~~~

Do not manually book/unbook attack actions from the mod just to increase pressure.

## Action-stamina pressure

The working implementation patches:

~~~text
CharacterStats.CharacterStatsWrapper.Initialize(CharacterStats)
~~~

After initialization:

1. require `stats.ParentModel is Hero`;
2. target the current `stats.StaminaUsageMultiplier`;
3. remove any older mod-owned tweak;
4. attach one new runtime `StatTweak`;
5. use multiply semantics;
6. make the tweak non-saved.

The implementation shape is:

~~~csharp
private sealed class ActionStaminaTweak : StatTweak
{
    public override bool IsNotSaved => true;

    internal ActionStaminaTweak(Stat stat, float multiplier)
        : base(stat, multiplier, TweakPriority.Multiply, OperationType.Multi)
    {
        MarkedNotSaved = true;
    }
}
~~~

Attach the tweak to the current hero as an element so your mod can find and discard it cleanly.

## Reapply and cleanup

Character stats can be recreated. Never hold one `StaminaUsageMultiplier` reference forever.

When settings change or the wrapper reinitializes:

~~~text
current Hero
→ current CharacterStats
→ discard old ActionStaminaTweak
→ attach one tweak to current StaminaUsageMultiplier
~~~

When the multiplier is effectively 1.0, remove the tweak and leave the native stat alone.

## Keep the feature narrow

Do not mix this process with:

- global damage multipliers;
- NPC health changes;
- custom target selection;
- replacement AI;
- animation-state changes;
- item-template mutation.

Those are separate systems.

## Practical test

Change one lever at a time:

1. vanilla baseline;
2. `MaxEnemiesAttacking` only;
3. `AttackActionUnBookProlong` only;
4. action stamina only;
5. combine only after each individual change behaves as expected.

That makes it obvious which native input produced the change.
