# Change Carry Capacity Safely

FoA's current carry-capacity owner is `HeroStats.EncumbranceLimit`. Apply a runtime tweak to the **current** stat instance each time hero stats initialize.

Working lineage: [Carry Tweak Must Follow the Current Stat Instance](../../../research/case-studies/stats/carry-stale-tweak.md).

## Runnable source

Start with the [Carry capacity example](../../../examples/mono/gameplay/carry-capacity-tweak/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Runnable source

Start from the buildable example: [Carry capacity](../../../examples/mono/gameplay/carry-capacity/README.md). Build it unchanged first, confirm the documented behavior, then make one change at a time.


## Patch target

The working implementation uses:

~~~text
Awaken.TG.Main.Character.HeroStats.HeroStatsWrapper.Initialize(HeroStats)
~~~

with a Harmony postfix.

Inside the postfix:

1. require a valid `HeroStats`;
2. require `heroStats.EncumbranceLimit`;
3. obtain `Hero hero = heroStats.ParentModel`;
4. read `EncumbranceLimit.BaseValue`;
5. calculate the additive modifier required to reach the configured total;
6. discard older mod-owned carry tweaks;
7. attach one new non-saved additive tweak to the current `EncumbranceLimit`.

## Runtime tweak

The implementation uses an additive `StatTweak`:

~~~csharp
private sealed class CarryWeightLimitTweak : StatTweak
{
    public override bool IsNotSaved => true;

    internal CarryWeightLimitTweak(Stat stat, float modifier)
        : base(stat, modifier, null, OperationType.Add)
    {
        MarkedNotSaved = true;
    }
}
~~~

Then:

~~~csharp
hero.AddElement(new CarryWeightLimitTweak(
    heroStats.EncumbranceLimit,
    modifier));
~~~

Do not edit item weights to change carry capacity.

## Remove stale tweaks first

Before attaching a new one, enumerate your own `CarryWeightLimitTweak` elements on the hero and discard them.

The working implementation also checks the current `HeroEncumbered` element for an old nested carry tweak and discards that too.

This matters because the hero stat wrapper can rebuild the underlying stat instance. A tweak attached to yesterday's `EncumbranceLimit` object can still exist while no longer affecting the live stat.

## Reapply without stacking

For config changes, use the same route:

~~~text
Hero.Current
→ Hero.HeroStats
→ current EncumbranceLimit
→ discard old owned tweak
→ calculate modifier from current BaseValue
→ attach one new tweak
~~~

If the calculated modifier is effectively zero, remove the old tweak and stop.

## Native consumers remain intact

FoA still owns:

- `HeroItems.CurrentWeight`;
- encumbrance comparison;
- `HeroTweaks.RefreshEncumbrance()`;
- encumbered behavior/UI.

Your mod changes only the capacity stat they read.
