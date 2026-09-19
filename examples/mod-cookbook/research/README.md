# Research & Candidate Hooks

This section contains useful modding research that has **not** earned working-pattern status for the claimed behaviour.

Research material belongs here when its current ceiling is source inspection, build, load/registration, partial runtime observation, or an unresolved evidence gate.

## Status and character candidates

- [28 Status-application observer](../28-status-application-observer-mono/README.md)
- [29 Active-status observer](../29-active-status-observer-mono/README.md)
- [30 Character-state observer](../30-character-state-observer-mono/README.md)
- [Buff/debuff tuning evidence gate](../recipes/10-buff-debuff-tuning/README.md)

## Consumable candidates

- [31 Consumable-use observer](../31-consumable-use-observer-mono/README.md)
- [32 Healing/recovery observer](../32-healing-recovery-observer-mono/README.md)
- [33 Status cure/removal observer](../33-status-cure-observer-mono/README.md)
- [Consumable effect attribution](../recipes/11-consumable-effect-attribution/README.md)

Do not transfer evidence from one consumable execution seam to another. If a working owner proves `Item.PerformImmediate`, that does not automatically prove a public `Item.Use` adaptation.

## Equipment and weapon-state candidates

- [34 Equipment-change observer](../34-equipment-change-observer-mono/README.md)
- [35 Main/off-hand observer](../35-hand-item-observer-mono/README.md)
- [36 Weapon visibility/state observer](../36-weapon-visibility-state-observer-mono/README.md)
- [Equipment lifecycle attribution](../recipes/12-equipment-lifecycle-attribution/README.md)

These are useful state/lifecycle research. They are not evidence that custom equip UI, renderer attachment, animation ownership or registration is solved.

## Combat-action candidates

- [37 Combat-state observer](../37-combat-state-observer-mono/README.md)
- [38 Guard/block/parry observer](../38-guard-block-parry-observer-mono/README.md)
- [39 Attack/cast action observer](../39-attack-cast-action-observer-mono/README.md)
- [Combat action lifecycle attribution](../recipes/13-combat-action-lifecycle-attribution/README.md)

Some individual read surfaces have stronger evidence than the whole public example. Keep claims at the narrowest proved scope.

## Poise, stagger and knockback

- [40 Poise-break observer](../40-poise-break-observer-mono/README.md)
- [41 Stagger observer](../41-stagger-observer-mono/README.md)
- [Downstream combat outcome attribution](../recipes/14-downstream-combat-outcome-attribution/README.md)

Poise and stagger have identified native seams but remain below working-pattern status in the public cookbook.

**Knockback remains blocked.**

Known force-related fields do not establish the exact runtime consumer that converts those values into actor displacement. There is intentionally no public example 43.

## Death

- [42 Character death observer](../42-character-death-observer-mono/README.md)

Unlike poise/stagger, the underlying terminal death seam has runtime precedent in a working damage/death VFX owner. The public rewrite itself remains **NOT_RUN**.

## Promotion rule

A research candidate moves into a normal section as a runtime-backed or advanced pattern only when there is either:

1. a working-mod precedent for the exact mechanism/scope; or
2. direct runtime validation of the exact public path.

Build success alone is not enough.
