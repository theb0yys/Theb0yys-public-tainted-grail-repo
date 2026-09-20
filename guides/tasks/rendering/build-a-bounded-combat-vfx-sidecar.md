# Build a Bounded Combat VFX Sidecar

Attach mod-owned visuals after FoA accepts character damage and after FoA enters the terminal death lifecycle. Do not patch target resolution.

Working lineage: [Combat VFX Sidecars](../../../research/case-studies/rendering/combat-vfx.md).

## Runnable source

Start from the buildable example: [Damage and death VFX sidecar](../../../examples/mono/rendering/damage-death-vfx/README.md). Build it unchanged first, confirm the documented behavior, then make one change at a time.


## Living-hit hook

Patch:

~~~text
Awaken.TG.Main.Character.HealthElement.OnDamage
~~~

with a postfix.

The maintained Tainted Blood handler resolves the target from:

~~~text
damage.TargetPure
or
healthElement.ParentModel
~~~

Then it can reject non-character targets before doing any character-blood work.

Useful filters already used by the implementation include:

- minimum damage amount;
- IsBlocked;
- IsParried;
- IsDamageOverTime;
- require-character-target;
- per-target/global spawn budgets.

## Resolve the presentation position

Use the accepted damage context to derive:

- impact position;
- impact forward;
- HitCollider;
- character target.

Then add only small presentation offsets such as vertical/forward offsets before spawning the mod-owned effect.

Do not use this hook to change the Damage result.

## Spawn through a bounded presentation path

The implementation can use either:

- a mod-owned bundled impact; or
- the native VFXManager.SpawnCombatVFX presentation route.

If using VFXManager, resolve only the supported overload shape and supply the surface/asset, position, forward, collider, parent and lifetime data it expects.

The VFX call remains presentation-only.

## Terminal death hook

Patch:

~~~text
HealthElement.OnDeathEvents
~~~

with a postfix.

The death handler:

1. reads the terminal outcome/damage;
2. rejects unsupported DOT/low-value cases according to policy;
3. resolves the target;
4. deduplicates by target identity so one death produces one terminal sequence;
5. resolves a ground/impact position;
6. spawns the bounded death presentation.

The maintained implementation can create:

- one custom death impact;
- bounded particle layers;
- a mod-owned procedural ground pool.

Keep these separate from native corpse/loot ownership.

## Never patch target resolution for cosmetic VFX

The current implementation explicitly does **not** patch:

~~~text
Damage.DetermineTargetHit
~~~

Target resolution is damage-critical. A visual mod does not need to own it.

If you need to understand native VFX traffic, patch supported VFXManager.SpawnCombatVFX overloads diagnostically instead.

## Caps

Use explicit limits such as:

- maximum active impacts;
- per-target effects per second;
- maximum procedural death pools;
- lifetime per effect.

When a budget is exhausted, skip the new visual rather than allowing unbounded GameObjects/particles.

## Cleanup

Every object/texture/instance created by the mod must have a release path.

On unload, the maintained pattern:

~~~text
Harmony.UnpatchSelf()
→ destroy custom pool textures
→ destroy custom impact instances
→ destroy surface decals
→ destroy volumetric impact instances
~~~

Short-lived effects should also self-destroy after their configured lifetime.

## Ownership rule

~~~text
native damage/death decides what happened
→ mod observes accepted lifecycle event
→ mod adds presentation
→ mod cleans up presentation
~~~

Damage, target selection, death, corpse, loot, XP and saves stay native.
