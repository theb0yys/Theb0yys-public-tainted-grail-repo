# FoA MVC: Models, Elements, Views, Events, and Services

> **Reference page.** This is the core native lifecycle model behind many FoA runtime systems.

## What this system is

FoA uses its own logical model layer on top of Unity.

The important distinction is:

~~~text
logical game lifetime
≠
Unity GameObject lifetime
~~~

Core concepts include:

- `Model` — logical game object owned by `World`;
- `Element<TParent>` — functionality/state that shares a parent model's lifecycle;
- `View` — Unity-facing presentation associated with a Model;
- `World.Events` / EventSystem — owner-aware event routing;
- Services — type-keyed shared runtime owners.

## Who owns it in FoA

`World` owns Model registration/removal and associated lifecycle.

A Model can own Elements.

A View has its own Unity lifetime but can be bound to a Model through FoA's View APIs.

The EventSystem tracks event listeners and listener owners.

Services are registered and resolved by type/key and may have domain/scene lifetime implications.

## Important identities, types, and methods

### Model creation

~~~csharp
World.Add(model);
~~~

The documented lifecycle is:

1. register model;
2. initialize model;
3. spawn default Views;
4. initialize Elements;
5. invoke `OnFullyInitialized`.

### Element creation

~~~csharp
model.AddElement(element);
~~~

The element is registered to the parent. If the parent Model is not yet fully initialized, Element initialization is deferred until the parent's element-initialization stage; otherwise the Element is added through `World.Add` immediately.

### Model removal

~~~csharp
model.Discard();
~~~

Documented teardown includes:

1. `OnBeforeDiscard`;
2. `OnDiscard`;
3. break relations;
4. `BeingDiscarded`;
5. remove/discard Elements;
6. remove owned listeners;
7. `World.Remove(model)`;
8. `AfterDiscarded`;
9. remove remaining listeners;
10. `OnFullyDiscarded`.

### Access

- `World.All<T>()`
- `model.Element<T>()`
- `model.Elements<T>()`
- `element.ParentModel`
- `World.View<T>(model)`

### Events

Model events include:

- `BeforeFullyInitialized`
- `AfterFullyInitialized`
- `AfterChanged`
- `BeforeDiscarded`
- `BeingDiscarded`
- `AfterDiscarded`
- `AfterElementsCollectionModified`

Listening patterns include target-specific and wildcard-source listeners.

## Where it exists in the lifecycle

Model lifecycle is separate from Unity scene/GameObject lifecycle.

A logical Model can be invalid/discarded while a Unity presentation is still in a transition/exit animation.

Likewise a renderer/ECS representation may have its own resource/lifetime owner.

This is why cleanup must follow the relevant owner at each layer.

## How we interact with it

### Use Model when you need FoA logical lifetime

A mod-defined logical object that needs:

- World registration;
- parent/element semantics;
- event cleanup;
- relations;
- FoA lifecycle hooks;

should use the FoA Model/Element path rather than only a GameObject.

### Use Element when behavior should share a parent lifetime

Unity transform parenting does not reproduce Element ownership.

### Use FoA View binding when you need Model↔View semantics

Directly instantiating a View prefab does not automatically establish World association or standard teardown.

### Use owner-backed event listeners

A stable owner allows bulk cleanup when the owner is discarded.

## Why this route

The native lifecycle already solves:

- registration;
- initialization ordering;
- parent ownership;
- event ownership;
- listener cleanup;
- discard ordering.

Bypassing it creates hidden lifecycle work for the mod.

## What goes wrong

### Raw GameObject parenting

It looks similar to parent ownership but does not reproduce Model/Element semantics.

### `Destroy(view.gameObject)` instead of `View.Discard()`

This can bypass:

- View hooks;
- World deregistration;
- event cleanup;
- registered asset release;
- deferred/retained GameObject policy.

### Ownerless listeners

They cannot benefit from normal owner-based cleanup and can survive longer than intended.

### Broad `World.All<T>()` polling

Useful for startup/discovery, but expensive and fragile as an ongoing reaction system compared with events.

### Assuming render objects are Views

Drake/Kandra/Leshy/Medusa have their own rendering lifetimes. A rendered object is not automatically an MVC View.

## How to verify

For Model/Element work, verify:

- `World.Add` occurred;
- expected initialization completed;
- Elements are owned by the intended parent;
- expected events fire;
- View association is correct if used;
- discard calls the native teardown path;
- listener/resource cleanup occurs;
- no orphan Unity/ECS representation remains.

## Current proof boundary

Model/Element/Event lifecycle is supported by preserved official developer documentation and source/decompilation work.

Saved-Model restoration was not documented in the preserved official lifecycle material and should not be invented from the runtime creation path.
