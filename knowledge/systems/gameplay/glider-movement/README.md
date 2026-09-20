# Native Glider Movement

Use this page when you are working with glider movement and need to understand which native movement state and inputs actually control it.

It is a system reference, not a generic movement tutorial, so use it when a glider-specific feature needs exact ownership or lifecycle detail.

## What it is

FoA contains a native **glider movement** system. It is a first-class hero movement type, not a general free-flight framework.

## Native surfaces

- `MovementType.Glider`
- `GliderMovement : HeroMovementSystem`
- `Hero.TrySetMovementType<T>()`
- `Hero.ReturnToDefaultMovement()`
- `VHeroController.PerformMoveStep(Vector3)`
- vertical-velocity publication
- ground/water checks
- `HeroGlideAction`
- glider-aware fall-damage and quick-tool policy

## Movement ownership

The native controller owns physical movement/collision:

~~~text
hero movement system
→ requested displacement
→ VHeroController.PerformMoveStep(...)
→ character-controller movement
→ ground/water state
~~~

## Glider policy integration

Other game systems inspect `MovementType.Glider`.

Examples include:

- glider action state;
- fall-damage handling;
- quick-tool restrictions.

That means a custom movement mode reporting the native glider identity can enter some existing FoA policy surfaces.

## Important distinction

Native gliding and powered/free flight are different products.

The native glider provides useful movement/controller integration, but it should not be described as a complete native free-flight API.

## Modding relevance

When building aerial traversal, reuse native movement/controller ownership for physical movement and preserve the hero movement lifecycle instead of translating the transform directly.

## Related systems

- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
- [Runtime orchestration](../../core/runtime-orchestration/README.md)
