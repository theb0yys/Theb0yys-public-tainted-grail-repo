# MVC, GameObject, ECS and Runtime Lifetime

## What it is

FoA uses several simultaneous lifetime domains rather than one universal Unity GameObject lifecycle.

A useful ownership map is:

~~~text
World / Model
├─ Elements
├─ optional Views
│  └─ Unity GameObjects / ViewComponents
├─ scene/domain services
├─ renderer-specific state
│  ├─ Drake ECS entities
│  ├─ Kandra renderer/rig state
│  ├─ Leshy scene service
│  ├─ Medusa scene-static state
│  └─ HLOD controller/resource state
└─ asynchronous asset/file operations
~~~

## MVC Model lifetime

`Model` owns logical game state and follows its own discard protocol.

Model disposal can include:

- pre-discard hooks;
- events;
- relation cleanup;
- Element teardown;
- tweak cleanup;
- World deregistration;
- final reference cleanup.

Destroying a Unity GameObject is not equivalent to discarding the Model.

## View / GameObject lifetime

Views are optional Unity-facing representations.

A Model may have zero, one or several Views. A View can be destroyed, retained or delayed independently depending on its binding/lifecycle policy.

`ViewComponent.OnDestroy()` is adapter cleanup; it is not a replacement for the Model discard protocol.

## Scene and Domain lifetime

Logical Domain teardown and Unity scene unload are separate mechanisms.

That matters when a mod creates:

- scene-owned runtime objects;
- global Models;
- ECS resources;
- async Addressables objects;
- presentation that must survive or die across scene transitions.

## Renderer lifetime is system-specific

There is no single rule such as “destroy the GameObject and every renderer resource is gone.”

Examples:

- Drake can keep linked ECS entities/resources;
- Kandra owns renderer/rig registrations;
- Leshy and Medusa are scene services;
- critters can have separate gameplay, controller and visual entities.

## Modding relevance

When adding runtime objects, decide who owns:

1. logical state;
2. Unity representation;
3. renderer resources;
4. scene/domain lifetime;
5. cleanup.

Only tear down what your mod owns.

## Related systems

- [Runtime orchestration](../runtime-orchestration/README.md)
- [Drake](../drake/README.md)
- [Kandra](../kandra/README.md)
- [Medusa](../medusa/README.md)
- [Leshy](../leshy/README.md)
