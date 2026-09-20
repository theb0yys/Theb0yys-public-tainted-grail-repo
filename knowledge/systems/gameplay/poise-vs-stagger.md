---
document_type: system
scope: NPC poise-break versus stagger ownership
runtime: mono
evidence:
  static: DECOMPILED
last_verified: 2026-09-20
---

# Poise Break vs Stagger

FoA's NPC **poise break** and **stagger** are different systems.

## Poise break

Damage carries `DamageParameters.PoiseDamage`.

`NpcGeneralFSM.OnDamageTaken(...)` routes the damage outcome to the enemy combat owner, which accumulates poise through `EnemyBaseClass.DealPoiseDamage(...)`.

`NpcStats.PoiseThreshold` behaves as an accumulated limited meter. When it reaches its upper limit, the meter resets and a directional poise-break behaviour can run.

## Stagger

`StaggerBehaviour` is a separate combat state associated with stamina-depletion/explicit stagger routes.

It is not simply “poise threshold reached”.

## Consequence

Do not implement a “stagger multiplier” by blindly modifying `NpcStats.PoiseThreshold`.

Decide whether you actually mean:

- incoming poise damage;
- poise-break threshold/accumulation;
- stamina-driven stagger;
- an explicit visual-script stagger.
