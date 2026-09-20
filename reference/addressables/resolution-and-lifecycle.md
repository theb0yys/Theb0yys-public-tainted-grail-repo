# Addressables Resolution and Lifecycle

Use this page when working with an Addressables key/address, mod catalogue/locator, scene reference, or asset reference.

Canonical architecture:

- [Scene, Service, and Template Lifecycle](../../systems/core/scenes-services-templates.md)
- [Assets, Addressables, and Presentation](../assets/README.md)

## Keep four layers separate

```text
catalogue / locator available
→ address/key resolves
→ asset or scene loads
→ owning game system accepts/uses it
```

A pass at one layer does not prove the next.

## Native labels observed in the researched lifecycle

Important Addressables-driven categories include:

- `template`
- `templateSO`
- `scene`

These labels are native loading routes, not permission to add arbitrary content under them without validating the expected object/contract shape.

## Mod catalogue ordering

If a mod contributes Addressables locations, its locator/catalogue must be available before the native system performs the lookup that needs those locations.

For scene/template work, record whether catalogue installation happened before:

- `SceneService` scene-reference discovery;
- template resource-location discovery;
- the downstream lookup/consumer.

## Asset load is not registration

Examples:

- a loaded prefab is not automatically a registered FoA template;
- a loaded model is not automatically equipped by the native presentation owner;
- a resolvable scene asset is not automatically a safe native travel destination.

Follow the canonical owner after the load succeeds.

## Release and cleanup

Track the resource owner that acquired the Addressables handle/resource.

Verify:

- successful resolution;
- dependency closure;
- correct object/type;
- downstream binding;
- unload/release;
- scene transition behavior where applicable.

Do not leak handles/resources because the visible feature appeared to work.

## Failure checklist

When an address fails, check in order:

1. correct catalogue/locator loaded;
2. exact key/address/label;
3. expected asset type;
4. dependencies/shaders/scripts;
5. correct runtime/build target;
6. downstream game owner;
7. cleanup/release.

For task-oriented asset procedures use [Assets reference](../assets/README.md) and the relevant [How-to](../../how-to/README.md).
