# Case Packet — Wrong-Owner Discovery from Mount Research

## Reader symptom

> The behaviour you expect is not present on the obvious visible actor/object.

## Canonical owners

This case deliberately distinguishes more than one owner:

- hero movement state/strategy;
- native mount movement/velocity surfaces;
- NPC controller/root-motion writers;
- visual hierarchy.

## Evidence-backed case history

Reviewed mount research initially inspected the horse/mount actor graph for mounted movement state.

The passive observation found:

- mount objects and mount-related views/components;
- no `MountedMovement` on the horse actor graph where it had been expected;
- a mount controller reference that did not establish the assumed ownership.

Source review corrected the model:

- `MountedMovement` belongs to the hero movement system and is initialized through `HeroMovementSystem`;
- native mount velocity getters such as `VMount.RunningVelocity` and `VMount.TurningVelocity` are a different, narrower surface;
- later bounded live work proved those getter paths were exercised, without turning that result into a universal statement about every mount subsystem.

A later wolf-mount ownership brief adds another warning: logging an internal “movement suspended” flag is not proof that the correct live `NpcController`, root-motion component, or navigation writer actually stopped.

## What this case proves

- “not found” can be evidence that the ownership model is wrong;
- visible object hierarchy and state ownership are not the same thing;
- different pieces of one feature can have different owners;
- correct next step is source/call-flow reconstruction and observation, not broader mutation.

## What this case does not prove

- that one owner controls all mount behaviour;
- persistent mount ownership/save/travel/stable semantics;
- that a successful speed getter intervention solves actor-locomotion ownership.

## Canonical dependencies

- [Research method](../../../investigate/finding-the-native-owner.md)
- [Native object ownership](../../../systems/core/native-object-ownership.md)
- [Intervention selection](../../../mechanics/intervention-selection/README.md)

## Public outputs

- investigation case: `examples/investigations/mount-wrong-owner-discovery.md`
- diagnosis: `diagnose/ownership/expected-behaviour-missing.md`

## Evidence status

- underlying case: source inspection, passive observation and bounded runtime hook-path proof;
- public rewrite: documentation-only;
- persistence and broader mount ownership remain separate.
