# Exact-Target NPC Tuning

NPC tuning is safest when the mod identifies one exact native actor profile and changes only native knobs already consumed by that actor.

## Working implementation lineage

Tainted Instincts has runtime-validated profiles for exact native enemies including Outlaw, Bear, Drowner, Grindylow and Corpse Eater.

The exact plain-Drowner 1.0.0 runtime pass included matching-hash deployment, diagnostics-off startup, exact activation, user-confirmed behaviour, native Fireball preservation, pursuit/return preservation and melee-slot on/off testing.

## Identity first

Use exact template identity, not a name fragment.

A target gate should include the exact template name/GUID plus runtime checks such as:

- alive;
- hostile;
- not hero-allied/summoned;
- expected native behaviour/profile.

## Native knobs used by the working family

Examples include:

- `HyperAggressiveToHero`
- `SightLengthMultiplier`
- `MeleeDamage`
- exact native cooldown/behaviour settings
- melee combat-slot participation

## Drowner example

The validated Drowner profile used a narrow set of changes:

- immediate combat on confirmed detection;
- sight multiplier;
- melee damage multiplier;
- the exact native approach cooldown signature;
- optional melee-slot removal.

It explicitly preserved the native Fireball path and native pursuit/return behaviour.

## Rule

Do not replace the AI controller because you want one enemy to feel more aggressive. Tune the exact native values and leave movement, abilities and unrelated behaviours alone unless your feature specifically owns them.
