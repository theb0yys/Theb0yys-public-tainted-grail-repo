# Find the Animation Owner Before Patching It

Use this when you need to change or observe an animation but do not yet know which system owns the transition.

Animation failures are often ownership failures: a visible `Animator`, clip or GameObject may be presentation output rather than the gameplay/lifecycle owner.

## Procedure

1. Identify the exact visible transition you care about.
2. Determine what gameplay/model event causes it.
3. Trace from that native owner toward the presentation layer.
4. Identify whether the final animation is owned by:
   - a FoA Model/View path;
   - an Animator/Animancer route;
   - a renderer-specific presentation system;
   - a Story/dialogue step;
   - another component-specific owner.
5. Choose the smallest intervention point that preserves the native lifecycle.
6. Restore/unsubscribe on the corresponding teardown boundary.

## Do not start from clip names alone

Names are useful search leads, not ownership proof.

Two objects may play similar clips through different state machines, layers, controllers or runtime systems.

## Observe before replacing

Prefer a read-only observation/probe first:

```text
native action/event
→ presentation owner reached
→ expected animation transition observed
```

Only then decide whether you need:

- timing observation;
- parameter/value adjustment;
- clip replacement;
- sidecar VFX/audio;
- full presentation override.

## Verify

Record:

- gameplay/model owner;
- animation/presentation owner;
- exact method/state/parameter/clip identity used;
- transition in/out;
- interruption/cancel behavior;
- scene/discard cleanup;
- runtime/build.

If ownership remains unknown, use [Finding the native owner](../../investigate/finding-the-native-owner.md) rather than patching the first `Animator` you find.
