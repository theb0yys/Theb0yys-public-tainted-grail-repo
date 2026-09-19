# Recipe — Downstream Combat Outcome Attribution

**Category:** combat / downstream outcomes  
**Source-path evidence:** SOURCE_CONFIRMED lifecycle map  
**This public recipe:** NOT_RUN

Keep downstream outcomes separate:

~~~text
action lifecycle
    ↓
damage result
    ↓
poise damage
    ├─→ poise break
    └─→ separate stamina/stagger lifecycle
    ↓
lethal resolution
    ↓
HealthElement.OnDeathEvents
    ↓
later NPC / corpse / loot / presentation cleanup
~~~

## Poise break

NPC poise handling runs through `NpcGeneralFSM.OnDamageTaken(DamageOutcome)`, `Damage.PoiseDamage`, and `EnemyBaseClass.DealPoiseDamage(...)`.

Observe actual poise-break entry at:

~~~text
EnemyBaseClass.EnterPoise(NpcStateType, bool)
~~~

Do not infer stagger from a poise break.

## Stagger

Native stagger has separate surfaces:

~~~text
EnemyBaseClass.EnterStagger(float?)
StaggerBehaviour.UpdateStaggerDuration(float?)
~~~

Stamina-driven stagger/rest decisions are a separate owner from the poise meter.

## Death

`HealthElement.OnDeathEvents(...)` is the common terminal health event. It does not prove the later NPC/corpse/loot lifecycle has completed.

Later NPC-specific owners include `NpcElement.DeathNonCriticalFunctions(DamageOutcome)`, `DeathElement`, `NpcDummy`, and `Corpse`.

## Knockback evidence gate

Known force-related data surfaces include:

~~~text
DamageParameters.ForceDamage
DamageParameters.ForceDirection
NPC/template heroKnockBack
NPC/template forceStumbleThreshold
~~~

These establish that force/knockback-related values exist.

They do **not** establish the exact native runtime consumer that turns those values into actor displacement.

Current state: **BLOCKED — exact native displacement consumer NOT_PROVEN**.

Therefore:

- there is no example 43 yet;
- do not call `ForceDamage > 0` a knockback event;
- do not infer displacement from a template value;
- do not use transform or physics mutation as a substitute for missing native evidence.

Promotion requires tracing the exact native consumer, proving target/direction/magnitude semantics, distinguishing stumble/ragdoll/forced movement, and then validating the exact public observer.

## Ownership boundary

Keep these owners separate:

- damage;
- poise;
- stamina;
- reaction animation;
- stagger/stumble/ragdoll;
- displacement;
- death;
- corpse creation;
- loot;
- VFX/SFX;
- rewards;
- persistence.

A row from one owner is not proof that every downstream owner completed.
