# Item Stage 3 — Native Template Registration

## Objective

Make the custom template discoverable through the same template lookup path used by native game systems.

## Native ownership

The public Mono evidence identifies TemplatesLoader as the template-map owner and TemplatesProvider as the normal lookup surface.

The bounded direct route is conceptually:

    TemplatesProvider loader
    → TemplatesLoader.AddToMap(customGuid, customTemplate)
    → TemplatesProvider.Get<ItemTemplate>(customGuid)

Registration is timing-sensitive. The template system must be ready before mutation.

## Procedure

1. Wait for the documented template-readiness boundary.
2. Verify the custom GUID is not already occupied.
3. Register the validated custom template into the native template maps through the current supported/shared registrar when available.
4. Immediately resolve the custom GUID back through TemplatesProvider.
5. Record a registration receipt containing identity, source profile, registration path, and result.
6. Treat post-insertion failure as significant: direct map mutation is not assumed transactional.
7. Do not assume a native unregister/hot-reload path exists unless separately proven.

## Check

Confirm that the custom GUID resolves back through the normal provider to the expected custom ItemTemplate.

## Failure conditions

- template system not ready;
- duplicate GUID/name collision;
- source/custom identity confusion;
- insertion succeeds but provider resolution fails;
- failure occurs after a non-transactional insertion;
- an unverified reflection/private-API assumption changed with the game build.

## Before continuing

Registration does not cover Item construction, merchant/inventory acquisition, UI visibility, presentation, effects, or persistence.

## Next

Proceed to [runtime Item construction and acquisition](../04-runtime-acquisition/README.md).
