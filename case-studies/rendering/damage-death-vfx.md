# Damage and Death VFX Sidecars

Tainted Blood demonstrates the useful pattern: keep FoA's damage/death mechanics native and attach mod-owned presentation to their lifecycle.

## Working implementation lineage

Tainted Blood's stable path keeps the game's native blood routes intact and adds custom presentation only on character damage and terminal death routes.

Its 1.0.x line moved terminal custom impact ownership onto `HealthElement.OnDeathEvents` and removed the problematic target-resolution patch from the stable path.

## Useful owners

- `HealthElement.OnDamage` — living character hit presentation
- `HealthElement.OnDeathEvents` — terminal character presentation
- `VFXManager.SpawnCombatVFX(...)` — native combat VFX route when reusing FoA blood/surface presentation

## Working architecture

~~~text
native combat determines hit/damage
→ character damage event
→ mod classifies character/surface/result
→ spawn bounded mod-owned hit presentation

native death completes
→ OnDeathEvents
→ spawn bounded death impact/pool presentation
→ cleanup owned presentation
~~~

## Important rule

A VFX sidecar should not change:

- damage amount;
- target selection;
- death outcome;
- corpse ownership;
- loot/rewards.

Presentation observes the lifecycle; it does not become the gameplay owner.

## Bounded presentation

Use caps and cleanup for:

- active particles;
- decals/pools;
- attached wounds;
- per-target repeated effects.

That keeps repeated combat from leaking runtime objects.
