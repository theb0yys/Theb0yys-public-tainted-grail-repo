# Combat, Damage, Death, and Attribution

> **Reference page.** Combat hooks are high-value and high-risk because the same native damage/death surfaces are shared by many systems.

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

## How we interact with it

### Patch as high as necessary, as low as safe

For a narrow mechanic, prefer the specialised upstream owner when it cleanly identifies the effect.

Examples:

- fall damage → fall-damage utility;
- weapon stamina cost → exact item stat;
- mining candidate observation → damage context with exact target/source classification.

Use global `HealthElement.TakeDamage` only when the feature truly needs that shared boundary.

### Preserve attribution

Do not infer player damage from object names.

Use native source/target/dealer identities.

### Let native death own death

For custom actors where native death is the intended lifecycle, validate the full lethal transition and only accept it when the expected native dummy/corpse state exists.

## Why this route

The repository contains several examples of **not** patching `HealthElement.TakeDamage` because a narrower upstream path exists.

That is an important golden rule:

> A lower/shared method can technically work while being the wrong ownership boundary.

It increases compatibility collisions and makes it harder to distinguish combat, fall, mining and other contexts.

## What goes wrong

### Global damage patch for a narrow mechanic

Can affect unrelated damage and interact unpredictably with other multipliers.

### Prefix ordering conflicts

Multiple mods mutating `Damage.RawData` or skipping originals can make load order matter.

### Death observer treated as actor cleanup owner

Observing death does not automatically own `Location`, corpse retention, rewards or encounter lifecycle.

### Summon death assumptions copied to ordinary NPCs or vice versa

Summon/native ally markers change some death/corpse behavior.

### Rendered attack mistaken for damage proof

Animation/VFX playback does not prove native hit/health transition.

## How to verify

For combat work, verify:

- exact attack/damage category;
- dealer identity;
- target identity;
- damage before/after;
- native method actually invoked;
- health result;
- hit reaction;
- combat state;
- lethal path if applicable;
- death animation/state;
- dummy/corpse;
- rewards/loot separately;
- compatibility with other patches.

## Current proof boundary

The repository has many concrete source/runtime uses of specialised combat/damage hooks.

The public handbook treats `HealthElement.TakeDamage` and `NpcElement.DeathNonCriticalFunctions` as valuable shared surfaces, but not stable universal mod APIs or complete combat ownership contracts.
