# Visual Effects Cookbook

Visual effects are presentation sidecars: blood, hit/death effects, spell effects, weapon trails and local helper lighting.

## Damage and death VFX

The strongest current lineage is the maintainer's working blood/VFX implementation:

~~~text
character damage
    ↓
damage-side VFX
    ↓
terminal death event
    ↓
death-side VFX / bounded pool or presentation lifecycle
    ↓
cleanup
~~~

Relevant public material:

- [23 Character damage observer](../23-character-damage-observer-mono/README.md)
- [42 Character death observer](../42-character-death-observer-mono/README.md)
- [Combat VFX sidecars](../recipes/09-combat-vfx/README.md)

The underlying damage and death seams have useful runtime precedent. The exact public rewrites remain **NOT_RUN**.

A death event still does not prove corpse creation, loot readiness, despawn, rewards or persistence completion.

## Spell VFX

- [Spell VFX overlays](../recipes/06-spell-vfx/README.md) — SOURCE_CONFIRMED public recipe; exact recipe NOT_RUN.

Do not bind a visual effect to a speculative cast/hit lifecycle when the actual action or damage owner is not proved.

## Helper lighting

- [18 Personal helper light](../18-personal-helper-light-mono/README.md) — SOURCE_BUILD_EVIDENCED.
- [Held/helper-light recipe](../recipes/02-helper-light/README.md).

## Weapon trails

The maintainer corpus contains weapon/presentation research, but this cookbook does not currently contain a clean runtime-proven weapon-trail pattern. Keep future work evidence-gated rather than inferring weapon-render lifecycle from equipment state alone.
