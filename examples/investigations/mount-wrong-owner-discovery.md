# Investigation Case — The Expected Mount Behaviour Was on the Wrong Owner

Document type: **investigation case**.

## Question

Where does mounted movement state actually live, and what should be observed before attempting to modify it?

## Initial model

The obvious place to look was the horse/mount actor graph.

Passive observation found mount-related runtime objects such as the mount View/component stack.

The investigation expected to find the mounted movement strategy there.

It did not.

## Contradictory evidence

The passive actor-graph observation did **not** expose the expected `MountedMovement`.

That absence mattered.

Instead of treating it as a reason to search more aggressively or patch arbitrary fields, the investigation reopened the ownership model.

## Corrected owner model

Source inspection established that:

- `MountedMovement` belongs to the **hero movement system**;
- it is initialized through `HeroMovementSystem`;
- the native mount object still owns other distinct surfaces such as running/turning velocity;
- NPC/root-motion writers can be separate again.

The feature called “mount movement” therefore spans multiple owners.

```text
hero movement strategy
≠ mount velocity properties
≠ NPC/root-motion transform writers
```

## Next step after the correction

The corrected model justified a **no-mutation transition probe** rather than a broader patch.

Separate bounded work later proved that native mount running/turning velocity getter paths are exercised during mounted play.

That result was not generalized into proof of:

- full mount actor ownership;
- stable/marker/travel ownership;
- persistent mount state;
- every locomotion writer on a converted custom mount.

## Second ownership lesson

Later custom/wolf-mount work exposed another important trap:

> an internal log saying “movement suspended” is not proof that the correct live controller/root-motion/navigation writer was disabled.

A Boolean owned by the mod proves the mod's state, not the native owner's state.

## Reusable investigation pattern

```text
expected behaviour missing from obvious object
→ treat wrong owner as a real hypothesis
→ reconstruct source/call flow
→ identify the actual owner(s)
→ observe the corrected owner without mutation
→ only then choose the smallest intervention
```

## Diagnostic handoff

Use [Expected behaviour is missing from the object you inspected](../../diagnose/ownership/expected-behaviour-missing.md).

## Canonical research pages

- [Research Method](../../investigate/research-method.md)
- [Native Object Ownership](../../systems/core/native-object-ownership.md)
- [Intervention Selection](../../mechanics/intervention-selection.md)

## Evidence status

Underlying case: **source inspection, passive observation, and bounded runtime hook-path proof**.

This public rewrite: **documentation-only**.

Broader mount persistence/travel/ownership remains outside this case.
