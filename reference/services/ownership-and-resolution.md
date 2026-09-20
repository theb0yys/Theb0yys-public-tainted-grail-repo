# Service Ownership and Resolution

Use this page when a mod needs a shared FoA service and you need to determine **who owns it, when it exists, and whether it survives a scene/domain transition**.

Canonical architecture: [Scene, Service, and Template Lifecycle](../../systems/core/scenes-services-templates.md).

## What a service means

FoA services are shared runtime owners resolved by type/key. A service reference is not just a convenient singleton: its lifetime can be tied to application, map, additive-scene, domain, or other native ownership.

Therefore:

```text
service can be resolved now
≠
service is valid forever
```

## Lookup questions

Before consuming a service, establish:

- exact service type;
- registration owner;
- initialization boundary;
- scene/domain lifetime;
- whether the service can be replaced/recreated;
- whether it exposes a supported operation or only internal state;
- teardown/re-resolution behavior.

## Cache rule

Do not cache a scene/domain-owned service across transitions unless its lifetime contract is proven.

A safe default is:

1. resolve after the owning lifecycle is ready;
2. use the service for the bounded operation;
3. release subscriptions/references on owner teardown;
4. re-resolve after a lifecycle transition when required.

## Service vs gameplay owner

A service being reachable does not mean it owns the gameplay fact you want to change.

Examples:

- `SceneService` owns scene loading/unloading orchestration;
- `TemplatesProvider` owns normal template lookup;
- another gameplay system may own the state consumed through that service.

Find the native owner before mutation.

## Verification checklist

Record:

1. service type/key;
2. where it is registered;
3. readiness condition;
4. resolved instance identity;
5. operation called;
6. scene/domain tested;
7. teardown behavior;
8. re-resolution after transition;
9. runtime/build scope.

## Failure patterns

- resolving before initialization;
- keeping a stale service reference after scene/domain replacement;
- treating an internal service method as a public modding API;
- using a service as a second owner for gameplay state;
- failing to remove callbacks/subscriptions.

For exact template lookup use [Templates](../templates/README.md). For scene/Addressables lifecycle use [Addressables](../addressables/README.md).
