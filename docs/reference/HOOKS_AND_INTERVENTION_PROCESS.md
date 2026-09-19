# Hook and Intervention Process

> **Reference/process page.** Use this before adding a Harmony patch, event listener, reflected call, runtime mutation, or other intervention.

## What this system is

~~~text
goal → native owner → exact identity → lifecycle map
→ read-only observation → smallest intervention
→ downstream observation → cleanup → compatibility record
~~~

## Who owns it in FoA

The native method/type/event owns the lifecycle. Harmony, reflection or a framework API is only the access mechanism.

## Important identities, types, and methods

Record target assembly, fully qualified type, exact method/overload, patch kind, state before/after, original-method behavior, version/fingerprint, evidence, runtime validation, and cleanup.

## Where it exists in the lifecycle

A useful hook sits after the required state exists and before the downstream consumer commits or caches it.

Merchant example:

~~~text
Shop.OpenShop → stock usable/decompressed
→ ShopUI.OnFullyInitialized Prefix
→ stock mutation
→ original UI snapshots list
~~~

## How we interact with it

1. Define the exact effect.
2. Identify the native owner.
3. Map creation, readiness, mutation, consumption and cleanup.
4. Observe read-only first where practical.
5. Prefer native/public API, then events, then narrow Harmony, then documented reflection; transpiler last.
6. Preserve original behavior unless suppression is intentional.
7. Fail closed on missing owner, identity or readiness.
8. Verify the downstream owner, not merely the hook marker.
9. Clean up what the mod owns.
10. Revalidate patch-sensitive targets after updates.

## Why this route

Repository failures show that partial path copying is insufficient. One matching API call does not reproduce ownership, input, lifecycle, dispatch, or cleanup semantics.

## What goes wrong

Wrong overload; too-early/late hook; original accidentally skipped; UI already cached state; input never reaches handler; reflection drift; cleanup omitted; hook-fire mistaken for feature success.

## How to verify

Prove target resolution, hook invocation, preconditions, one mutation, downstream observation, cleanup/restoration, and exact build scope.

## Current proof boundary

See [Hook Catalogue](HOOK_CATALOGUE.md) for researched surfaces. Entries remain version-sensitive where appropriate.
