# Startup Order and Readiness

Use this page when a feature works after a delay but fails during startup.

Canonical overview: [Runtime Orchestration, Scenes and Templates](README.md).

## Distinct readiness layers

FoA startup contains multiple readiness boundaries:

```text
subsystem registration
→ shared manager/service initialization
→ Addressables/catalogue availability
→ scene reference discovery
→ scene load
→ scene initialization
→ template loading complete
→ feature-specific owner ready
```

Do not replace those boundaries with an arbitrary timer when a native readiness signal exists.

## Choose the owner-specific signal

Examples:

- template lookup → `TemplatesProvider.AllLoaded`;
- scene-owned work → scene/service initialization boundary;
- renderer work → renderer manager + scene/resource owner ready;
- mod Addressables → catalogue/locator available before the native lookup.

## Retry discipline

A retry loop is acceptable when there is no event/callback and the loop is bounded by an exact readiness predicate.

A retry loop should not:

- swallow permanent identity/type failures;
- continue forever after an incompatible build;
- perform duplicate registration on every tick;
- turn a missing dependency into a delayed null reference.

## Validation

Record:

1. earliest attempted point;
2. native readiness predicate;
3. first successful point;
4. duplicate/retry behavior;
5. scene/load transition behavior;
6. build/runtime scope.

For scene/service/template specifics, see [Scene, Service, and Template Lifecycle](../scenes-services-templates.md).
