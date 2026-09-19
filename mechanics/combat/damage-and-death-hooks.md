<!-- Canonical Wave 5 mechanic page split from docs/reference/COMBAT_DAMAGE.md. -->
# Combat, Damage, Death, and Attribution — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/combat/damage-death-attribution.md) first for ownership, identities, lifecycle, and proof scope.

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

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/combat/damage-death-attribution.md).
