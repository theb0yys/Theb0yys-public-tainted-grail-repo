# Combat VFX Sidecars

A combat VFX mod should attach presentation to the game's real damage/death lifecycle instead of replacing combat ownership.

## Working lifecycle

~~~text
native hit/damage event
→ classify target and result
→ spawn bounded mod-owned impact presentation
→ terminal death event
→ spawn bounded death presentation
→ timed/owner cleanup
~~~

## Rules

- use the native character damage event for living-hit presentation;
- use the native terminal death event for terminal character presentation;
- keep non-character resource hits out of character-blood logic;
- cap active effects;
- destroy/release every mod-owned effect;
- never use a visual hook as a substitute for native target resolution;
- keep corpse, loot, rewards and persistence separate.

## Minimal structure

~~~csharp
void OnCharacterDamaged(Damage damage)
{
    if (!IsSupportedCharacterDamage(damage))
        return;

    SpawnBoundedImpact(damage);
}

void OnCharacterDeath(HealthElement health, DamageOutcome outcome)
{
    if (!IsSupportedCharacterDeath(health, outcome))
        return;

    SpawnBoundedDeathEffect(health, outcome);
}
~~~

The exact rendering implementation can be particles, decals, meshes or another owned presentation system. The lifecycle ownership stays the same.
