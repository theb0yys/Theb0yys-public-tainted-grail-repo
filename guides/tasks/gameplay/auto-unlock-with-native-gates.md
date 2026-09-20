# Auto-Unlock Without Bypassing Native Lock Rules

Skip the lockpicking interaction only after FoA has already established that the current lock is a normal lockpickable target.

Working lineage: [Auto-Unlock Without Bypassing Every Lock Rule](../../../research/case-studies/lockpicking/auto-unlock-boundary.md).

## Patch target

The working implementation patches:

~~~text
Awaken.TG.Main.Locations.Actions.Lockpicking.LockAction.OnStart(
    Hero,
    IInteractableWithHero)
~~~

with a Harmony prefix.

It also resolves three existing `LockAction` members:

- private/property getter `HeroCanLockpick`;
- private/property getter `WillBeOpenWithKey`;
- method `Unlock(bool)`.

If any of those members cannot be resolved, Auto Unlock is disabled and vanilla lockpicking continues.

## The gate

Before skipping the interaction, check all of these:

~~~text
AutoUnlock enabled
+ no LockpickingInteraction already attached
+ WillBeOpenWithKey == false
+ HeroCanLockpick == true
~~~

If any check fails, return `true` from the prefix and let `LockAction.OnStart` run normally.

The working shape is:

~~~csharp
if (__instance.ParentModel.HasElement<LockpickingInteraction>())
    return true;

if (WillBeOpenWithKey(__instance))
    return true;

if (!HeroCanLockpick(__instance))
    return true;
~~~

That preserves key-driven and non-lockpickable cases.

## Complete through the native lock owner

For the supported case:

~~~csharp
UnlockMethod.Invoke(__instance, new object[] { false });
CommitCrime.Lockpicking(__instance.ParentModel);
return false;
~~~

The mod skips the minigame, but it still uses the lock's own unlock method and the game's existing lockpicking-crime route.

Do not set a generic `Locked=false` field or edit arbitrary quest objects.

## Related difficulty tweaks

The same implementation also demonstrates two separate lockpicking adjustments:

- postfix `LockAction.Tolerance` to replace the native `LockTolerance` result;
- prefix `LockpickingInteraction.ConsumePickHP(float)` to scale pick-damage time.

Keep those features separate from Auto Unlock.

## Failure behavior

If reflection or invocation fails:

- log once;
- return to vanilla `LockAction.OnStart`;
- do not attempt a second unlock strategy.

That is the correct fallback for a patch-sensitive private member.

## Practical test

Test at least:

- normal lockpickable target;
- key-opened target;
- target the hero cannot lockpick;
- criminal lockpicking target;
- repeated interaction after unlock.

The important check is that only the ordinary lockpicking interaction is skipped.
