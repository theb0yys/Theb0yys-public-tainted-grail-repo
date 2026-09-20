# Replace a Rendering Resource Safely

Use this for a bounded runtime visual replacement such as a skybox/material/texture owned by your mod.

First identify the real renderer owner:

- [Drake](../../systems/presentation/drake/README.md)
- [Kandra](../../systems/presentation/kandra/README.md)
- [Medusa](../../systems/presentation/medusa/README.md)
- [Leshy](../../systems/presentation/leshy/README.md)
- [HLOD](../../systems/presentation/hlod/README.md)

## Ownership pattern

For a mod-owned replacement:

```text
capture previous state
→ create/load mod-owned resource
→ apply only at the proven owner
→ use
→ restore previous state
→ destroy/release only mod-owned resource
```

Do not destroy a native resource merely because your mod temporarily replaced the reference.

## Procedure

1. Identify the exact renderer/material/skybox owner.
2. Capture the previous value only once per ownership interval.
3. Create or load the replacement under mod ownership.
4. Apply after the owning system is ready.
5. Track every object/handle created by the mod.
6. On disable/scene teardown, restore the prior native reference where still valid.
7. destroy/release only resources the mod owns.
8. re-resolve after owner/scene recreation.

## Renderer-specific caution

Drake, Kandra, Medusa, Leshy and HLOD do not share one universal renderer lifecycle.

A method that works for a normal Unity `Renderer` should not be assumed to work for a specialized native/ECS renderer.

## Verification

Check:

- exact owner identified;
- replacement visible;
- native source remains intact;
- scene/load transition does not duplicate the replacement;
- disable/unload restores native state;
- no leaked material/texture/GameObject/handle;
- target build/runtime recorded.

See [Rendering case studies](../../case-studies/rendering/README.md) for owner-specific lessons.
