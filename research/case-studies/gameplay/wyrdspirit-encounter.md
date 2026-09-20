# Fixed Native Wyrdspirit Encounter

This is the proven narrow encounter pattern behind the first Wyrd Hunt live path.

## Working implementation lineage

The fixed `Spec_EnemyMonster_T1_Wyrdspirit` path was validated live. Follow-up reported:

- fight/kill working;
- leave/return working;
- save/load working for the tested path.

## Core pattern

~~~text
plugin-owned threat/condition state
→ exact approved native profile selected
→ native spawn request
→ native hostile/combat path
→ plugin tracks encounter state
→ native death evidence resolves encounter
→ cooldown before another request
~~~

## Identity

The working first target is exact:

~~~text
Spec_EnemyMonster_T1_Wyrdspirit
~~~

Do not replace this with family/name-fragment matching.

## Ownership split

The mod owns:

- threat/scent state;
- profile selection;
- cooldown/encounter bookkeeping.

FoA owns:

- the NPC template;
- spawn mechanics;
- NPC combat;
- damage/death.

## Why this pattern scales

An encounter director can be built around exact native profiles without creating a custom AI stack. Add another profile only when that profile's spawn/combat/lifecycle path has its own working evidence.
