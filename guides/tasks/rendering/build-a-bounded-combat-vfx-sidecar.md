# Build a Bounded Combat VFX Sidecar

**Evidence status: PARTIAL.** The owner-preserving pattern is documented, but this generic case does not carry a separate runtime validation receipt beyond the related working Tainted Blood lineage.

Working lineage: [Combat VFX Sidecars](../../../research/case-studies/rendering/combat-vfx.md).

## Goal

Attach mod-owned combat presentation to real native damage/death lifecycle events.

## Architecture

```text
native hit/damage
→ classify target/result
→ spawn bounded mod-owned impact

native death
→ spawn bounded terminal presentation
→ timed/owner cleanup
```

## Rules

- living-hit visuals attach to the native character damage event;
- terminal visuals attach to the native death event;
- non-character resource hits stay out of character-blood logic;
- active effects are capped;
- every mod-owned effect is destroyed/released;
- target resolution remains native;
- corpse, loot, rewards and persistence remain separate.

## Minimal pseudocode

```csharp
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
```

## Verification

Prove:

- one living hit → expected effect;
- one death → one terminal effect;
- unsupported target → no character VFX;
- caps hold under sustained combat;
- cleanup removes effects;
- native combat/death/loot remains unchanged.

## Current proof boundary

The lifecycle architecture is strongly grounded. A generic public implementation still needs its own bounded runtime receipt.
