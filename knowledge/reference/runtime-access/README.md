# Runtime Access

Use this page when you know **what runtime object you need** and want the shortest known way to reach it.

Finding an object does not prove that it is initialized, still valid, or the real owner of the behavior you want to change. Use [Systems](../../systems/README.md) when you need ownership and lifecycle details.

## Common access patterns

| Need | Access pattern | Important note |
| --- | --- | --- |
| Current Hero | `Hero.Current` | Common direct access; non-null does not mean every Hero subsystem is ready |
| Any Hero Model | `World.Any<Hero>()` | Useful null-checkable world lookup |
| One expected Model | `World.Only<T>()` | Used when the game expects a single Model of that type |
| All Models of a type | `World.All<T>()` | Prefer for startup/discovery or bounded work, not blind continuous polling |
| Shared service | `World.Services.Get<T>()` | Service availability depends on lifecycle timing |
| Optional service | `World.Services?.Get<T>()` | Useful when startup timing is uncertain |
| Required child Element | `model.Element<T>()` | Use when one related Element is expected |
| Child Elements | `model.Elements<T>()` | Enumerate Elements owned by a Model |
| Optional child Element | `model.TryGetElement<T>()` | Common safe lookup for optional Elements |
| Location Element | `location.TryGetElement<T>()` | Public mods use this for things such as `NpcElement` |
| Element owner | `element.ParentModel` | Resolve an Element back to its Model |
| Model View | `World.View<T>(model)` | Resolve a FoA View associated with a Model |
| Template by GUID | `World.Services.Get<TemplatesProvider>().Get<T>(guid)` | Wait for template readiness first |
| All loaded templates of a type | `TemplatesProvider.GetAllOfType<T>()` | Treat large enumeration as bounded work |
| Typed template reference | `TemplateReference.TryGet<T>()` | Resolves the template behind a reference |
| Active scene | `World.Services.Get<SceneService>().ActiveSceneRef` | Scene identity is not the same as full scene readiness |
| Actor from `ActorRef` | `ActorRef.Get()` | Public source routes this through `ActorsRegister` |
| Main UI canvas | `Services.Get<ViewHosting>().OnMainCanvas()` | UI-host lookup; does not imply gameplay ownership |
| Global event listener | `World.EventSystem.ListenTo(...)` | Install after the EventSystem is actually ready |
| Remove event listener | `World.EventSystem.RemoveListener(listener)` | Keep handles for cleanup |

## Hero-owned state

Useful Hero-related objects include:

| Need | Access / lifecycle point | Important note |
| --- | --- | --- |
| Hero items | `HeroItems.OnRestore` in the public IL2CPP-native example | Good restore-time owner capture; pointer lifetime still matters |
| Known-item state | `KnownItems` | Used by public mods for reconstruction/backfill |
| Learn an existing recipe | `HeroRecipes.LearnRecipe` | Does not register a new recipe definition |
| Hero stats | `Hero.Current.HeroStats` | Use after the relevant Hero/stat initialization point |
| RPG stats | `Hero.Current.HeroRPGStats` | Same lifecycle caution |
| Hero storage | `HeroStorage.Items` | Documented in the storage context |

## Useful services

Frequently encountered services include:

- `TemplatesProvider`
- `TweakSystem`
- `SceneService`
- `ViewHosting`
- `ActorsRegister`
- `NpcGrid`
- `DroppedItemSpawner`
- `UnityUpdateProvider`
- `GameConstants`
- `GameplayMemory`

See [Services](../services/README.md) for what they are used for.

## Timing matters

A useful mental model is:

~~~text
plugin loaded
→ World/services begin coming online
→ Hero becomes available
→ Hero.OnFullyInitialized
→ Hero-owned subsystems finish their own initialization/restore
→ UI/scene-specific systems may become ready later
~~~

Examples:

- a public damage-number mod waits for `Hero.OnFullyInitialized` before installing HUD/event work;
- public stat mods wait for `HeroRPGStats.AfterHeroFullyInitialized` before applying stat tweaks;
- a public IL2CPP-native recipe mod captures `HeroItems` at `HeroItems.OnRestore`.

## Unsafe shortcuts

Avoid these assumptions:

- `Hero.Current != null` means all Hero systems are ready;
- a pointer/type lookup proves a native IL2CPP object is still alive;
- a cached native pointer stays valid indefinitely;
- access to a View or presentation object means it owns the underlying gameplay state;
- a large `World.All<T>()` scan is appropriate every frame.

A public IL2CPP example hit stale/freed entries while traversing `CraftingTemplate.recipes`; pointer/class checks alone were not enough to prove lifetime.

## Evidence

This reference combines Questline public source, public FoA mod source, preserved developer lifecycle documentation, and exact-build Mono static inspection.

See [Public FoA Symbol Baseline](../../../research/sources/public-symbol-baseline.md) and [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).

Exact signatures and cross-runtime equivalence should be rechecked when the game/runtime changes.
