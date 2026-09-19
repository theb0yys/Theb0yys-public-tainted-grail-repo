<!-- Canonical Wave 5 native-system page split from docs/reference/COMBAT_DAMAGE.md. -->
# Combat, Damage, Death, and Attribution

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/combat/damage-and-death-hooks.md).

## What this system is

FoA combat is not one method.

A useful high-level model is:

~~~text
attack/action owner
→ hit/target resolution
→ Damage object/context
→ HealthElement.TakeDamage
→ health/reaction/state changes
→ lethal transition
→ NPC death processing
→ dummy/corpse/loot/encounter consequences
~~~

Different damage categories can enter through specialised upstream utilities before converging on health.

## Who owns it in FoA

Important owners include:

- weapon/action systems such as `CharacterWeapon`;
- `Damage` and source/target context;
- `HealthElement`;
- hero/NPC stat owners;
- `NpcElement`;
- `DeathElement`;
- `NpcDummy`;
- `Corpse`;
- combat AI/state owners;
- fall-damage/mining/other specialised upstream routes.

## Important identities, types, and methods

Research-backed surfaces include:

- `HealthElement.TakeDamage(Damage)`
- `FallDamageUtil.DealFallDamage(...)`
- `NpcElement.DeathNonCriticalFunctions(...)`
- `Damage.TargetPure`
- `Damage.DamageDealerPure`
- `ItemStat.ModifiedValue`
- native weapon stamina-cost stats
- native death/corpse transitions
- `NpcAI.EnterCombatWith(...)`

## Where it exists in the lifecycle

### Damage observation

A Postfix on `HealthElement.TakeDamage` can observe damage after the native call while leaving the input unchanged.

This is used in several diagnostics/features for hero-involved damage context.

### Damage modification

A Prefix that changes `Damage` has different compatibility implications.

Upstream specialised routes can be safer for narrow effects.

Example: fall damage has its own `FallDamageUtil.DealFallDamage` path before calling health.

### NPC death observation

`NpcElement.DeathNonCriticalFunctions` is a useful researched death-processing observation point.

It exposes enough context for some hero-credited death/hunt resolution logic.

It is **not** automatically an exactly-once universal death event for every actor class/path.

## Current proof boundary

The repository has many concrete source/runtime uses of specialised combat/damage hooks.

The public handbook treats `HealthElement.TakeDamage` and `NpcElement.DeathNonCriticalFunctions` as valuable shared surfaces, but not stable universal mod APIs or complete combat ownership contracts.
