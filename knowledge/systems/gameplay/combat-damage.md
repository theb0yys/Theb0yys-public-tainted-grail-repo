# Combat, Damage, Death, and Attribution

Use this page when your mod needs to **observe or change damage, death, fall damage, kill attribution, or other combat outcomes**.

The most important rule is:

> Patch the narrowest native stage that actually owns the effect you want to change.

FoA damage is a pipeline, not one method.

## A useful combat model

~~~text
attack/action owner
→ hit/target resolution
→ Damage context
→ health/damage handling
→ reactions/state changes
→ lethal transition
→ NPC death processing
→ corpse/loot/encounter consequences
~~~

Specialized damage such as fall damage can enter through narrower upstream methods before reaching shared health handling.

## Important types and methods

Useful researched surfaces include:

- `HealthElement.OnDamage(Damage)`
- `HealthElement.TakeDamage(Damage)`
- `FallDamageUtil.DealFallDamage(...)`
- `NpcElement.DeathNonCriticalFunctions(...)`
- `Damage.TargetPure`
- `Damage.DamageDealerPure`
- `NpcAI.EnterCombatWith(...)`

Related state is also owned by Hero/NPC stats, death Elements, corpse/dummy state, AI/combat state, and weapon/action systems.

## Observe damage vs change damage

A later hook such as a Postfix around `HealthElement.TakeDamage` is useful when you need to observe the outcome while leaving the input untouched.

A Prefix that changes `Damage` changes the calculation path and has greater compatibility implications.

For narrow mechanics, prefer a narrower upstream method when one exists.

Example:

~~~text
fall-damage mechanic
→ FallDamageUtil.DealFallDamage
→ shared health/damage path
~~~

A fall-damage mod normally has less reason to patch all damage globally.

## Preserve source and target identity

Do not infer "player damage" or "enemy damage" from object names.

Use the native dealer/target/context information carried by the damage path.

Attribution matters for:

- player-dealt vs player-received damage;
- summons;
- mining/resource targets;
- kill credit;
- rewards/loot;
- combat-state reactions.

## Let native death own the death lifecycle

Observing a death method does not give your mod ownership of:

- the `Location`;
- corpse retention;
- encounter completion;
- loot/rewards;
- actor cleanup.

When native death is the intended lifecycle, let the native death/corpse path finish and observe the state you need.

## Common mistakes

### Global damage patch for a narrow feature

This can affect unrelated damage categories and interact with other multipliers.

### Multiple Prefixes mutating the same Damage payload

Patch order/load order can change results.

### Treating a death callback as an exactly-once universal death event

Different actor families or lifecycle paths may not behave identically.

### Copying summon assumptions to ordinary NPCs

Summons/allies can have different markers and corpse/death handling.

### Treating animation/VFX as hit proof

A rendered attack is not proof that native damage/health state changed.

## How to verify combat changes

Check:

1. exact damage/action category;
2. dealer identity;
3. target identity;
4. damage value before your intervention;
5. exact native method that fired;
6. resulting health;
7. reaction/combat state;
8. lethal path if applicable;
9. death/corpse transition;
10. rewards/loot separately;
11. interaction with other relevant patches.

## Evidence limits

The repository has concrete uses of several combat hooks and strong static/source mapping of the shared damage path.

`HealthElement.OnDamage`, `HealthElement.TakeDamage`, and `NpcElement.DeathNonCriticalFunctions` are useful targets, not guaranteed universal mod APIs for every actor/build/runtime.
