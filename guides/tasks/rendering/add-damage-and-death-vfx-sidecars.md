# Add Damage and Death VFX Sidecars

Use this guide when you want custom blood/impact/death presentation without taking ownership of combat, target selection or death outcomes.

Working lineage: [Damage and Death VFX Sidecars](../../../research/case-studies/rendering/damage-death-vfx.md).

## Runnable source

Start from the minimal public example: [Damage and death VFX example](../../../examples/mono/rendering/damage-death-vfx/README.md). Build it unchanged first, confirm the documented log/result, then make one change at a time.

## What you will build

```text
native combat resolves hit/damage
→ character damage lifecycle
→ mod classifies supported character result
→ spawn bounded mod-owned hit presentation

native death completes
→ terminal death lifecycle
→ spawn bounded death presentation
→ clean up mod-owned effects
```

The stable Tainted Blood route moved terminal ownership to `HealthElement.OnDeathEvents` and removed an earlier semantically wrong target-resolution hook.

## Step 1 — leave target resolution native

Do not patch an early target-resolution method merely because it exposes convenient hit data.

Your VFX layer should observe accepted gameplay outcomes, not help decide them.

## Step 2 — attach living-hit presentation after damage

Use the reviewed character damage lifecycle such as `HealthElement.OnDamage`.

Before spawning:

- confirm supported character target;
- inspect the actual damage/result;
- classify surface/context if needed;
- reject unsupported resource/non-character hits.

## Step 3 — attach terminal presentation to death

Use `HealthElement.OnDeathEvents` for one terminal character effect.

That keeps terminal presentation attached to the real death owner instead of guessing from pre-damage/target state.

## Step 4 — keep presentation mod-owned

Your sidecar may create:

- particles;
- decals/pools;
- attached wounds;
- other bounded visual objects.

It should not change:

- damage amount;
- target selection;
- death outcome;
- corpse ownership;
- loot;
- rewards.

## Step 5 — cap active effects

Repeated combat can leak objects quickly.

Define explicit bounds for:

- active particles;
- decals;
- per-target wounds;
- lifetime of pooled/death effects.

Destroy/release every object your mod owns.

## Step 6 — keep native VFX reuse separate

If you call a native route such as `VFXManager.SpawnCombatVFX(...)`, treat that as using a native presentation owner.

If you instantiate your own effect, your mod owns its lifecycle.

Do not confuse the two.

## Verification checklist

1. living character hit triggers one expected custom impact;
2. non-character/resource hits stay outside character-blood logic;
3. damage value remains native;
4. target selection remains native;
5. terminal death triggers the death effect once;
6. corpse/loot/reward behaviour remains normal;
7. active-effect cap works during repeated combat;
8. owned effects expire/destroy cleanly;
9. removing the mod leaves combat functional.

## Common mistake: patching too early

A previous Tainted Blood route patched `Damage.DetermineTargetHit` for corpse-hit behaviour and produced a user-reported damage/stone-hit regression.

The correction was to remove that patch from the stable route and move presentation onto the real damage/death lifecycle.

## Evidence boundary

**Proven:** stable character damage/death presentation sidecar architecture and the correction away from the target-resolution regression.

**Not claimed:** repeated corpse-hit support, combat ownership, damage modification, corpse ownership or loot/reward changes.
