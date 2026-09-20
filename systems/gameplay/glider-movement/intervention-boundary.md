# Glider Movement Intervention Boundary

Use this when building aerial traversal or modifying the native glider.

Canonical overview: [Native Glider Movement](README.md).

## Preserve physical movement ownership

The native path is:

```text
HeroMovementSystem
→ displacement request
→ VHeroController.PerformMoveStep(...)
→ character-controller movement/collision
→ ground/water state
```

If a feature wants altered glide behavior, prefer changing inputs/parameters at the movement-system layer while preserving `VHeroController` collision ownership.

Direct transform translation bypasses that path.

## Native policy coupling

Other systems inspect `MovementType.Glider`.

That can affect:

- glider action state;
- fall-damage behavior;
- quick-tool policy.

Therefore changing movement identity has consequences beyond movement.

## Powered flight is a separate capability

Do not describe “uses native glider movement” as “native free-flight API”.

A powered-flight feature may need:

- custom vertical/forward control policy;
- entry/exit rules;
- stamina/resource rules;
- collision/ground/water transitions;
- camera/input integration.

Reuse native movement ownership where possible, but document the additional mod-owned policy separately.

## Verification

For a glider intervention, test:

- enter movement mode;
- horizontal/vertical displacement;
- collision;
- ground/water exit;
- fall-damage interaction;
- quick-tool restrictions if relevant;
- return to default movement;
- death/load/scene transition;
- controller/keyboard paths claimed.
