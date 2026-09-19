# Templates and Registries

> **Reference page.** Use this when working with `ItemTemplate`, `NpcTemplate`, `LocationTemplate`, recipes, or any system where a definition must be discoverable by the game.

## What this system is

A template is a reusable FoA definition. A runtime object can refer back to its template for identity and behavior.

For the proven item path, the important distinction is:

~~~text
Unity clone exists in memory
≠
FoA template is registered

registered template
=
FoA native template maps can resolve the custom GUID
~~~

## Who owns it in FoA

For the researched item route:

- `TemplatesLoader` owns template-map construction;
- `TemplatesProvider` owns normal lookup of loaded templates;
- `TemplatesProvider._loader` reaches the loader used by the direct proof route;
- private `TemplatesLoader.AddToMap(string, ITemplate)` is the historical direct insertion mechanism used by several working-repo implementations.

The current internal architecture is moving toward shared registrar ownership so individual consumer mods do not each reflect into `AddToMap`.

## Important identities, types, and methods

Important surfaces:

- `TemplatesProvider.AllLoaded`
- `TemplatesProvider.Get<T>(guid)`
- `TemplatesLoader.FinishedLoading`
- `TemplatesLoader.AddToMap(string, ITemplate)`
- `ItemTemplate`
- `TemplateReference`

The direct clone route also depends on preserving a valid component/attachment shape from a reviewed native prototype.

## Where it exists in the lifecycle

The useful ordering is:

~~~text
native templates loading
→ TemplatesLoader.FinishedLoading = true
→ TemplatesProvider resolves stable loaded definitions
→ custom registration retry/insert
→ normal provider lookup of the custom GUID
→ runtime consumers create/use instances
~~~

Several working implementations use the `FinishedLoading` setter as a retry boundary.

## How we interact with it

Historical proven/custom-item pattern:

1. wait for template readiness;
2. resolve a safe native source template;
3. clone its `GameObject`;
4. assign a new mod-owned GUID/name/presentation;
5. validate that the source was not mutated and the clone shape is acceptable;
6. insert the custom template into the loader map;
7. resolve the custom GUID back through `TemplatesProvider`;
8. only then create runtime items or feed downstream consumers.

## Why this route

`TemplatesProvider` is a lookup surface. Merely constructing an `ItemTemplate`-shaped object does not automatically make the provider aware of it.

This is the missing step that simple editor/prefab authoring guidance tends to hide.

## What goes wrong

Known failure classes:

- lookup before `AllLoaded` / loader readiness;
- custom object exists but was never registered;
- source and custom GUID are the same;
- another mod already owns the custom GUID/name;
- the clone lost or altered required component/attachment shape;
- direct reflective access breaks after a game update;
- registration occurs too late for a consumer that already attempted resolution;
- registration is mistaken for proof of save safety.

## How to verify

A minimum registration check should prove:

1. source template resolves;
2. custom identity does not collide with source;
3. source stays unchanged;
4. clone has expected type/component shape;
5. native insertion reports success;
6. normal provider lookup returns the custom template;
7. a downstream controlled consumer can use it.

## Current proof boundary

The direct private-loader route has source and bounded runtime evidence through custom item implementations. It is patch-sensitive because it uses private/native internals.

The planned shared native-item registrar has stronger collision/idempotency/save-restoration requirements but is **not yet promoted as a generally proven public runtime registrar**.

See [Items](ITEMS.md) for the full reasoned item process.
