# Teardown and Restoration Across Runtime Owners

Use this page when a mod creates or changes state that must be cleaned up on discard, scene unload, disable or shutdown.

Canonical overview: [MVC, GameObject, ECS and Runtime Lifetime](README.md).

## First identify every owner

A feature may own several resources simultaneously:

```text
mod feature
├─ FoA Model / Element
├─ Unity GameObject / component
├─ event listener/subscription
├─ renderer/ECS resource
├─ Addressables/file handle
└─ shared-infrastructure registration
```

Cleanup must follow each actual owner.

## Teardown rule

Restore only state your mod changed, and destroy/release only resources your mod owns.

Do not:

- destroy a native GameObject to compensate for missing Model cleanup;
- discard a Model because you only replaced a material;
- decrement renderer-internal resource ownership directly;
- restore a hardcoded default instead of the captured previous value.

## Teardown ordering

A useful pattern is:

1. stop new work;
2. unregister callbacks/actions/providers;
3. release shared scopes/leases;
4. restore modified native references/state;
5. discard/destroy mod-owned logical/Unity objects through their owner;
6. release renderer/asset/file resources;
7. clear cached references.

The exact owner may require a different ordering; follow its native lifecycle when known.

## Verification

Test at least:

- normal feature disable/close;
- scene transition;
- model discard where applicable;
- plugin disable/unload;
- failure during partial initialization;
- repeated enable/disable.

“No exception on unload” is not sufficient. Verify that no duplicate listener, entity, UI scope or visual resource survives.
