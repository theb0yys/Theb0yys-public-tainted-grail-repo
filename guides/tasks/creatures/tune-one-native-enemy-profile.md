# Tune One Native Enemy Profile

Use this guide when you want one exact native enemy to behave differently without replacing its AI controller.

Working lineage: [Exact-Target NPC Tuning](../../../research/case-studies/gameplay/npc-tuning.md).  
Native actor ownership: [Creatures and NPCs](../../../knowledge/systems/gameplay/creatures-npcs.md).

## Runnable source

Start from the minimal public example: [Exact Drowner tuning example](../../../examples/mono/gameplay/exact-npc-tuning/README.md). Build it unchanged first, confirm the documented log/result, then make one change at a time.

## What you will build

```text
native NPC becomes available
→ exact template identity matches
→ runtime safety checks pass
→ adjust one or two native values already consumed by that profile
→ native movement/abilities/combat continue
```

The key rule is:

> Tune exact native knobs; do not replace the whole controller for a small behaviour change.

## Step 1 — pick one exact enemy

Start with a single known profile.

The working family has runtime-validated profiles for enemies including Outlaw, Bear, Drowner, Grindylow and Corpse Eater.

Use exact template identity—not a name fragment.

## Step 2 — build an exact activation gate

Before changing anything, require the expected identity and runtime state, such as:

- exact template name/GUID;
- alive;
- hostile;
- not hero-allied/summoned;
- expected native behaviour/profile.

If the actor does not satisfy the gate, leave it vanilla.

## Step 3 — change one native knob

Examples used by the working family include:

- `HyperAggressiveToHero`;
- `SightLengthMultiplier`;
- `MeleeDamage`;
- exact native cooldown/behaviour settings;
- melee combat-slot participation.

Choose only the value needed for your intended change.

## Step 4 — preserve unrelated native behaviour

For the validated Drowner profile, the mod changed a narrow set of aggression/melee values while explicitly preserving:

- native Fireball;
- native pursuit/return behaviour.

Write down what must remain native before testing.

## Step 5 — make activation observable

Log one bounded activation record containing:

- exact actor/template identity;
- values changed;
- old/new values where useful;
- build/profile identity.

Do not spam every AI tick.

## Verification checklist

For the exact target:

1. exact template gate matches only the intended enemy;
2. non-target enemies remain unchanged;
3. configured behaviour change is visible;
4. native movement still works;
5. native special abilities you intended to preserve still work;
6. pursuit/return behaviour remains correct;
7. combat-slot policy behaves as configured;
8. disabling the mod restores vanilla behaviour after a clean reload/recreation.

## Evidence boundary

**Proven:** exact-profile runtime tuning family, including a strong Drowner validation with matching build, exact activation, user-confirmed behaviour, Fireball preservation, pursuit/return preservation and melee-slot on/off testing.

**Not claimed:** generic AI replacement, every enemy archetype, custom pathfinding or universal behaviour knobs.
