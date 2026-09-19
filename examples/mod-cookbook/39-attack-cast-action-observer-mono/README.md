# 39 — Attack / Cast Action Observer

**Category:** combat / action lifecycle observation  
**Source-path evidence:** SOURCE_CONFIRMED lifecycle seams  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example avoids input guesses and watches native action-lifecycle seams for three families.

## Melee

~~~text
CharacterWeapon.AttackBegun(...)
CharacterWeapon.AttackEnded()
~~~

Native animation research establishes these as the open/close boundaries for the `CharacterWeapon` attack window driven by attack-release and attack-recovery animation events.

The example filters to weapon instances attached to the hero and requires current melee hand context.

A melee-start row does **not** prove that anything was hit.

## Ranged

~~~text
BowPull.AfterEnter(float)
BowFSM.EndBowDrawState()
~~~

These report bow-draw start and draw end.

Important: `EndBowDrawState()` can close a bow draw for more than one reason. This public example therefore logs `draw-end`; it does **not** falsely label every draw end as a successful arrow release.

Actual projectile/shot resolution requires its own evidence.

## Spell casting

~~~text
Item.StartPerforming(ItemActionType.CastSpell)
Item.EndPerforming(ItemActionType.CastSpell)
Item.CancelPerforming(ItemActionType.CastSpell)
~~~

The observer reports start, end and cancel-path invocations only for hero-owned items.

A cast start is not cast success. The native deferred lifecycle has separate start/end/cancel handling.

Some magic-FSM exit paths can invoke cancellation defensively, so `cancel-path` is deliberately not phrased as "the player canceled the spell" without a narrower correlation signal.

## Build

~~~powershell
dotnet build .\AttackCastActionObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The observer does not dispatch input, change animation state, execute attacks, register hits, spend stamina/mana, alter projectiles or modify damage.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
