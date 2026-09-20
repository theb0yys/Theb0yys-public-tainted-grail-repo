# Models, Elements, Views, and Events

Use this page when you need to understand **what a FoA object actually is, who owns it, and when it is safe to use or discard**.

FoA does not treat every piece of game state as a Unity `GameObject`. Much of the game runs through its own Model/Element/View system.

The important distinction is:

~~~text
FoA game-state lifetime
≠
Unity GameObject lifetime
~~~

## The main pieces

- `Model` — a logical game object registered with `World`.
- `Element<TParent>` — state or behavior attached to a parent Model and tied to that Model's lifecycle.
- `View` — Unity-facing presentation associated with a Model.
- `World.Events` / EventSystem — event routing with listener ownership.
- services — shared runtime objects resolved by type.

## Creating a Model

A Model normally enters the game through:

~~~csharp
World.Add(model);
~~~

The documented order is:

1. register the Model;
2. initialize it;
3. spawn its default Views;
4. initialize its Elements;
5. call `OnFullyInitialized`.

If your code needs a fully usable Model, do not assume construction alone is enough.

## Adding an Element

~~~csharp
model.AddElement(element);
~~~

The Element is registered with the parent Model.

If the parent is still initializing, the Element's own initialization is deferred until the parent's Element-initialization stage. If the parent is already fully initialized, the Element can be added through `World.Add` immediately.

This matters when a mod adds behavior dynamically: **parent existence is not the same as parent readiness**.

## Discarding a Model

Use the FoA lifecycle rather than destroying only its Unity presentation:

~~~csharp
model.Discard();
~~~

The documented teardown order includes:

1. `OnBeforeDiscard`;
2. `OnDiscard`;
3. relation cleanup;
4. `BeingDiscarded`;
5. Element removal/discard;
6. removal of listeners owned by the Model;
7. `World.Remove(model)`;
8. `AfterDiscarded`;
9. cleanup of remaining listeners tied to the discarded source or its Elements;
10. `OnFullyDiscarded`.

If your mod owns listeners, Elements, or other Model-linked state, this lifecycle is the cleanup path you need to respect.

## Finding Models, Elements, and Views

Useful access methods include:

- `World.All<T>()` — enumerate registered Models of a type;
- `model.Element<T>()` — get one related Element when one is expected;
- `model.Elements<T>()` — enumerate related Elements;
- `element.ParentModel` — get the Element's owning Model;
- `World.View<T>(model)` — resolve an associated View.

Use broad `World.All<T>()` enumeration mainly for startup, discovery, diagnostics, or genuinely bounded operations. For ongoing reactions, prefer events when an appropriate event exists.

## Events

Documented Model lifecycle events include:

- `BeforeFullyInitialized`
- `AfterFullyInitialized`
- `AfterChanged`
- `BeforeDiscarded`
- `BeingDiscarded`
- `AfterDiscarded`
- `AfterElementsCollectionModified`

Typical listening forms include target-specific listeners, limited listeners, and global/any-source listeners.

Listener ownership is important because normal Model teardown can remove listeners owned by the discarded Model.

## When to use each layer

Use a **Model** when the thing needs FoA logical lifetime, World registration, relations, Elements, or game events.

Use an **Element** when behavior should live and die with an existing Model.

Use a **View** when you need FoA-aware Unity presentation tied to a Model.

Use a plain **GameObject** only when you intentionally own its Unity lifetime yourself and do not need Model/Element semantics.

## Common mistakes

### Treating transform parenting as Model ownership

Putting one GameObject under another does not create Model/Element ownership.

### Destroying only the View GameObject

`Destroy(view.gameObject)` can bypass FoA View/Model cleanup, listener cleanup, and resource-release behavior.

### Leaving listeners without a useful owner

Owner-backed listeners can be cleaned up automatically with their owner. Long-lived ownerless listeners are easier to leak.

### Assuming every rendered object is a View

Drake, Kandra, Leshy, Medusa, and other renderer systems have their own lifetimes. A visible object is not automatically an MVC View.

## What to verify

For Model/Element work, verify:

- the Model was added to `World`;
- the expected initialization stage completed;
- Elements belong to the intended parent;
- the expected events fire;
- the View association is correct if one is used;
- discard follows the native teardown path;
- listeners/resources are cleaned up;
- no Unity or ECS representation is left orphaned.

## Evidence limits

The creation, Element, event, and discard lifecycle above is supported by preserved developer documentation plus source/decompilation work.

The preserved developer material did **not** document saved-Model restoration in the same detail. Do not infer restore ordering from the normal creation lifecycle.
