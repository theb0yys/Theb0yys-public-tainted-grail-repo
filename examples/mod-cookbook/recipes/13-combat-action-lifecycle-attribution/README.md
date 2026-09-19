# Recipe — Combat Action Lifecycle Attribution

**Category:** combat / action lifecycle  
**Source-path evidence:** SOURCE_CONFIRMED lifecycle map  
**This public recipe:** NOT_RUN

Combat attribution is safest when each native subsystem owns only the fact it can actually prove.

A useful chain is:

~~~text
combat state
    ↓
action starts
    ↓
weapon / spell context
    ↓
native action lifecycle
    ↓
damage / block / parry result
    ↓
action cleanup
~~~

That is an attribution chain, not one giant "combat happened" event.

## Stage 1 — combat state

Use the game's native combat-state surface rather than a custom nearby-hostile guess:

~~~text
Hero.HeroCombat.IsHeroInFight
~~~

Combat state tells you the hero is considered in combat. It does not identify a particular attack, hit, cast, block or status effect.

## Stage 2 — action lifecycle

Different action families have different native owners.

### Melee

~~~text
CharacterWeapon.AttackBegun(...)
CharacterWeapon.AttackEnded()
~~~

These bound the native CharacterWeapon attack window.

Attack start is not hit confirmation.

### Bow

~~~text
BowPull.AfterEnter(float)
BowFSM.EndBowDrawState()
~~~

These bound bow-draw activity.

Draw end is not automatically a successful projectile release. Release, projectile creation and eventual hit resolution remain separate concerns.

### Spell

~~~text
Item.StartPerforming(CastSpell)
Item.EndPerforming(CastSpell)
Item.CancelPerforming(CastSpell)
~~~

This is a deferred lifecycle. Start means the native cast path began; it does not mean the cast completed.

Treat cancellation carefully because native FSM cleanup can also call cancellation paths defensively.

## Stage 3 — context

Attach only useful read-only context:

- current main/off-hand item;
- melee/ranged/magic classification;
- native combat state;
- weapon visibility where relevant.

Context explains an event. It should not become a second source of truth for the action itself.

## Stage 4 — native result

The existing damage path owns damage-result facts:

~~~text
HealthElement.TakeDamage(Damage)
~~~

Useful result fields include:

- target/dealer;
- amount;
- critical / weak-spot flags;
- blocked;
- parried.

A damage row can prove a damage-resolution event. It cannot retroactively prove every earlier input, animation or action state unless you actually correlate them.

## Stage 5 — keep result owners separate

For one combat exchange, multiple systems may report:

~~~text
melee attack window
damage result
block/parry result
status application
combat VFX
audio
death lifecycle
~~~

Do not award, mutate or spawn the same semantic effect independently from every observer.

If a larger mod needs correlation, create a short-lived mod-owned correlation record and attach evidence rows to it. Do not rewrite native combat state merely to obtain one universal event.

## Ownership boundaries

Keep these lanes distinct:

- **input:** what the player/controller requested;
- **animation:** semantic state and animation events;
- **weapon/spell action:** active attack or cast lifecycle;
- **hit registration:** what contact/projectile logic resolved;
- **damage:** what `Damage` reported;
- **defence:** block/parry outcome;
- **status:** `CharacterStatuses` lifecycle;
- **VFX/SFX:** presentation;
- **death:** terminal character lifecycle.

One lane should not silently claim another lane's result.

## Cleanup

Any mod-owned combat correlation should be short-lived and cleared when:

- the action ends/cancels;
- the hero becomes unavailable;
- combat exits where appropriate;
- the scene/session changes;
- the plugin unloads.

Do not persist transient combat-action state unless persistence is an explicit researched feature.

## Validation ladder

1. Observe combat enter/exit without mutation.
2. Observe melee start/end with no hit.
3. Observe a melee action that produces one damage result.
4. Observe guard entry followed by one native block or parry result.
5. Observe bow draw start/end and separately verify any future projectile-release seam before calling it a shot.
6. Observe cast start/end and an interrupted cast path.
7. Correlate status/VFX/death observers only after their own owner events are independently understood.
8. Confirm no observer changes input, animation, stamina/mana, damage, status, equipment or saves.

This keeps combat diagnostics composable without collapsing FoA's native lifecycle into speculative shortcuts.
