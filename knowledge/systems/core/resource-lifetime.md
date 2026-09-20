# Asset and Resource Lifetime

> **Reference page.** Use this when loading Addressables, AssetBundles, runtime prefabs, renderer resources, or asynchronous presentation assets.

## What this system is

Loading is only half of resource integration.

A complete resource path includes:

~~~text
request
→ asynchronous load
→ cancellation checks
→ adoption by the intended owner
→ use
→ release / destruction
→ late-completion handling
~~~

## Who owns it in FoA

Ownership varies:

- mod/plugin for a private AssetBundle;
- Addressables handle/reference for addressed resources;
- native presentation owner for equipped/actor visuals;
- scene/renderer managers for systems such as Drake, Kandra, Leshy or Medusa.

Resource ownership may not match MVC Model ownership.

## Important identities, types, and methods

A strong native example is `AddressablesPooledInstance`, whose researched lifetime:

- checks cancellation after asset arrival;
- instantiates a disabled GameObject;
- requests linked/movable representation;
- waits a frame;
- checks cancellation again;
- destroys/releases on late cancellation;
- `Release()` cancels outstanding work, destroys instance and releases the reference.

Other relevant concepts include:

- `ARAssetReference`;
- Addressables async handles;
- scene-related lifetime tokens;
- renderer manager resource/refcount ownership;
- AssetBundle `Unload`.

## Where it exists in the lifecycle

Asynchronous work creates race windows:

~~~text
owner exists
→ load starts
→ owner/scene may disappear
→ load completes
→ code must re-check validity before adoption
~~~

A check performed only before starting the load is not enough.

## How we interact with it

### Check cancellation/owner validity after await/completion

Especially before instantiating or binding.

### Release through the same ownership system

If an Addressables reference loaded it, release the reference/handle appropriately.

If a native equip/renderer owner adopted it, teardown must respect that owner.

### Keep scene/domain/resource lifetime separate

Scene unload, MVC Model discard and renderer/ECS resource teardown are distinct mechanisms.

Do not assume one automatically proves the others completed.

## Why this route

The runtime-lifecycle research found several unresolved races precisely at ownership boundaries:

- asset completes after Model discard;
- renderer resource completes during scene teardown;
- linked request resolves after Unity owner destruction;
- Kandra/Drake work overlaps owner destruction.

The safe general rule is to re-check validity at every asynchronous handoff.

## What goes wrong

- owner valid before load but gone at completion;
- GameObject destroyed but Addressables handle leaked;
- handle released while native renderer still retains resource;
- plugin unload destroys presentation owned by native equip lifecycle;
- scene unload assumed to clean a session-owned object with a different lifetime;
- `AssetBundle.Unload(true)` destroys objects still in active use.

## How to verify

For every loaded resource, answer:

- who requested it?
- who owns the handle?
- who owns the instantiated object?
- what cancels outstanding work?
- what happens if owner disappears mid-load?
- what exact release call runs?
- can release be called twice safely?
- after cleanup, is the resource/object still referenced?

## Current proof boundary

The handbook can state general lifetime rules from native/source evidence, but renderer-specific final teardown (especially some Drake/Kandra/ECS combinations) still contains unresolved edges and must not be guessed.
