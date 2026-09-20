# Wait for Scene Readiness Before World Work

Use this when a mod needs to inspect or modify scene-owned objects after a load or transition.

Canonical lifecycle: [Scene, Service, and Template Lifecycle](../../systems/core/scenes-services-templates.md).

## Why timing matters

The useful scene path is layered:

```text
scene reference resolves
→ Addressables scene load
→ Unity scene exists
→ FoA scene/domain services initialize
→ SceneLoaded
→ SceneInitialized
→ gameplay owners become usable
```

A Unity scene being present does not prove every FoA service/model is ready.

## Procedure

1. Identify the exact scene/domain owner needed by the feature.
2. Enter through the native scene lifecycle when the feature depends on FoA services/domains.
3. Wait for the owner-specific readiness boundary.
4. Resolve the target/service **after** that boundary.
5. Apply only the bounded feature intervention.
6. subscribe to unload/transition cleanup;
7. release scene-owned references and mod-owned objects.

## Do not cache scene owners indefinitely

After a transition, re-resolve any service/model/object whose lifetime belongs to the previous scene/domain.

Avoid assuming instance identity survives travel.

## For session-only world objects

Make the persistence boundary explicit.

If a proof object should not become durable game state:

- use a known non-saved/native disposable path where proven;
- destroy/discard it through its real owner;
- verify it is absent after transition/reload.

Do not infer “not saved” from the fact that your mod did not write a file.

## Verification

Test:

1. cold game/load;
2. target scene entry;
3. readiness reached before lookup;
4. target resolved exactly once/as intended;
5. feature executes;
6. leave/re-enter scene;
7. no stale references/duplicates;
8. teardown on mod disable where applicable;
9. save/load separately if durability is claimed.

For unknown world owners, start with [Investigate](../../investigate/README.md).
