# Change Mount Running and Turning Speed

This is a good first gameplay-tweak mod because it changes two values the native mount controller already consumes instead of replacing mount movement.

The proven seam is:

- `VMount.RunningVelocity`
- `VMount.TurningVelocity`

The working mod used Harmony postfixes and validated a `1.25` multiplier in game.

See [Native Mount Velocity Tuning](../../../research/case-studies/gameplay/mount-velocity.md) and [Native Horse Velocity: Proof125](../../../research/case-studies/movement/native-horse-velocity-proof.md).

## Runnable source

Start from the minimal public example: [Mount velocity example](../../../examples/mono/gameplay/mount-velocity/README.md). Build it unchanged first, confirm the documented log/result, then make one change at a time.

## What you will build

```text
native VMount computes velocity
→ property getter returns native value
→ Harmony postfix receives the result
→ multiply the result by configured scalar
→ native movement continues with adjusted value
```

A multiplier of `1.0` means vanilla.

## Prerequisites

1. complete the [first Mono plug-in](../../getting-started/first-mono-plugin.md);
2. complete the [first real patch rules](../../learning-paths/everyday-modding/first-real-patch-rules.md);
3. use a disposable save for initial runtime validation.

## Step 1 — keep the feature narrow

For this mod, own only:

- running velocity scaling;
- turning velocity scaling.

Do not mix in:

- recall;
- mount ownership;
- stamina;
- armour;
- animation;
- custom mounts;
- pathfinding;
- input replacement.

Those have different native owners.

## Step 2 — add two configuration values

Start with:

```text
RunningMultiplier = 1.0
TurningMultiplier = 1.0
```

Use conservative bounds. A beginner test might allow something like `0.5–2.0` rather than arbitrary huge values.

The point of the first version is to prove the seam, not find the fastest possible mount.

## Step 3 — patch the native getters with postfixes

Patch the getters for `VMount.RunningVelocity` and `VMount.TurningVelocity`.

The semantic shape is:

```csharp
// Pseudocode: use the exact VMount type/namespace from your game references.

static void RunningVelocityPostfix(ref float __result)
{
    __result *= RunningMultiplier;
}

static void TurningVelocityPostfix(ref float __result)
{
    __result *= TurningMultiplier;
}
```

Use a **postfix** because the game should compute the native value first.

Your mod then scales the result instead of reconstructing movement logic.

## Step 4 — fail back to vanilla

If configuration is invalid or your patch hits an unexpected state:

- do not drive the Transform yourself;
- do not call internal movement transitions;
- leave the native result unchanged.

`1.0` should always be a clean vanilla-equivalent configuration.

## Step 5 — log only enough to prove activation

On startup, log:

- plug-in version;
- running multiplier;
- turning multiplier;
- whether both Harmony targets were found/patched.

Avoid logging every getter invocation. These properties can be hot paths.

## Verify in game

Use a controlled profile:

1. set both multipliers to `1.0`;
2. mount and verify vanilla behaviour;
3. set running to `1.25`, turning to `1.0`;
4. verify forward movement changes while turning remains baseline;
5. set running to `1.0`, turning to `1.25`;
6. verify turning changes independently;
7. test `1.25 / 1.25`;
8. dismount/remount;
9. cross a normal transition;
10. save/load;
11. quit/relaunch;
12. restore `1.0 / 1.0` and confirm vanilla behaviour.

The proven lineage exercised `1.25 / 1.25`, followed by a user smoke through mount, movement, transitions, save/load and quit/relaunch on the observed stack.

## Common mistakes

### Replacing the mount controller

You do not need a second movement controller to change a scalar the native controller already exposes.

### Using a prefix to invent the entire result

Let native code calculate its normal value, then adjust the returned scalar.

### Coupling running and turning

Keep them separate so users can tune one without unexpectedly changing the other.

### Claiming "custom mount support"

This guide proves a narrow velocity seam on the native mount path. It does not prove ownership, recall, custom mount registration or mount persistence.

## Evidence boundary

**Proven:** postfix scaling of native running and turning velocity getters, including a live `1.25 / 1.25` proof and broader smoke on the observed stack.

**Not claimed:** deeper mount systems, custom mounts, stamina, armour, recall, animation or ownership changes.

## Next steps

Once the scalar tweak is stable, use the repository's owner-first model before adding another mount feature. Treat each additional feature as a separate system until its real owner and lifecycle are established.
