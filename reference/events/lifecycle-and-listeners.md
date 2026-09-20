# Event Lifecycle and Listener Ownership

Use this page when you need to identify **which FoA event boundary to observe** and how its listener should be owned.

Canonical architecture: [FoA MVC: Models, Elements, Views, Events, and Services](../../systems/core/mvc-models-elements-events.md).

## Core event lifecycle

For a FoA `Model`, the researched lifecycle exposes boundaries including:

- `BeforeFullyInitialized`
- `AfterFullyInitialized`
- `AfterChanged`
- `BeforeDiscarded`
- `BeingDiscarded`
- `AfterDiscarded`
- `AfterElementsCollectionModified`

These names describe lifecycle boundaries. They do not imply that every gameplay action you care about is represented by one of them.

## Listener ownership

Prefer listeners with a stable FoA owner.

Why:

- owner-backed listeners participate in native cleanup;
- discarded owners can remove their associated listeners;
- ownership makes teardown diagnosable.

Avoid long-lived ownerless listeners unless the lifecycle is explicitly controlled and cleanup is proven.

## Choose the narrowest event

| Need | Prefer |
| --- | --- |
| react to one known model | target-specific listener |
| observe a class of source models | wildcard/source-type route when proven |
| react to parent-owned functionality | the owning `Element` lifecycle/event |
| react to presentation only | the View/presentation owner, not a guessed Model event |
| react to scene lifecycle | the scene/service lifecycle, not broad event polling |

Do not replace a precise event with continuous `World.All<T>()` polling when a stable owner/event exists.

## Verification checklist

For a new event integration, record:

1. exact event identity;
2. source/target model or service;
3. listener owner;
4. registration point;
5. expected fire condition;
6. duplicate/re-entry behavior;
7. owner discard behavior;
8. explicit unregister path if native owner cleanup is not sufficient;
9. runtime/build tested.

## Boundaries

A listener firing proves that event observation path only. It does not prove:

- persistence;
- scene ownership;
- safe mutation;
- compatibility with another runtime/build;
- that the event is the earliest or only owner of the gameplay transition.

For task procedures, use [How-to guides](../../how-to/README.md). For unknown ownership, use [Investigate](../../investigate/README.md).
