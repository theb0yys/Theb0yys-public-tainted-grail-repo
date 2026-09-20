# Runtime Access Reference

Exact lookup for obtaining important runtime owners and objects.

Access to an object does not by itself prove gameplay, persistence or lifecycle ownership. For ownership, use [Systems](../../systems/README.md).

## Managed runtime access patterns

| Need | Public access surface | What the public evidence establishes |
| --- | --- | --- |
| Current hero | `Hero.Current` | Widely used by Questline public source and public Mono mods |
| Any current hero model | `World.Any<Hero>()` | Public Mono mod uses this as a null-checkable hero lookup |
| Global/service owner | `World.Services.Get<T>()` | Used publicly for `TweakSystem`, `TemplatesProvider`, `SceneService`, `ActorsRegister`, `NpcGrid` and other services |
| Optional service lookup | `World.Services?.Get<T>()` | Public mods fail closed when the service collection/provider is not ready |
| Single world model | `World.Only<T>()` | Questline public source uses `World.Only<GameRealTime>()` |
| Element owned by a model | `model.TryGetElement<T>()` | Public source uses `Hero.Current.TryGetElement<HeroStats>()` and `hero.TryGetElement<ArmorWeight>()` |
| Template by GUID | `World.Services.Get<TemplatesProvider>().Get<T>(guid)` | Public Mono mods resolve status/item templates this way |
| Template reference | `TemplateReference.TryGet<T>()` | Public mod resolves `CommonReferences.Get?.OverEncumbranceStatus` to `StatusTemplate` |
| Active scene reference | `World.Services.Get<SceneService>().ActiveSceneRef` | Questline public source uses this for scene identity/current-scene access |
| Actor by actor reference | `ActorRef.Get()` → `World.Services.Get<ActorsRegister>().GetActor(this)` | Questline public source exposes the resolution path |
| Main UI canvas | `Services.Get<ViewHosting>().OnMainCanvas()` | Questline public source uses this from multiple views |
| World event subscription | `World.EventSystem.ListenTo(...)` | Public Mono mod installs event listeners after hero initialization |
| World event cleanup | `World.EventSystem.RemoveListener(listener)` | Same public mod removes the listener before replacing/disposal |

## Hero-owned runtime surfaces

| Subject | Public access surface | Scope / caveat |
| --- | --- | --- |
| Hero-owned items | `HeroItems.OnRestore` | IL2CPP-native availability point; receiving a valid instance does not prove indefinite pointer lifetime |
| Known item state | `KnownItems` | Public mods use it for startup reconstruction/backfill after player load |
| Recipe learning | `HeroRecipes.LearnRecipe` | Existing/native recipe learning; does not establish custom recipe registration |
| Stats | `Hero.Current.HeroStats`, `Hero.Current.HeroRPGStats` | Public Mono mods use these after hero/stat initialization |
| Storage | `HeroStorage.Items` | Public storage mod documents access in the storage UI context |

## Service examples

Useful publicly visible service types include:

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

See [Services](../services/README.md).

## Availability matters

Public mod evidence distinguishes plugin load from usable gameplay state.

~~~text
plugin loaded
→ world/services may still be incomplete
→ Hero.OnFullyInitialized
→ hero-dependent systems become usable
→ subsystem-specific restore/init may happen later
~~~

A public damage-number mod waits for `Hero.OnFullyInitialized` before using `World.EventSystem` and HUD-dependent state. Public stat mods wait for `HeroRPGStats.AfterHeroFullyInitialized` before resolving `TweakSystem`. A public IL2CPP mod uses `HeroItems.OnRestore` rather than an earlier generic lookup.

## Unsafe or misleading access patterns

- Do not treat `Hero.Current != null` as proof that every hero-owned subsystem is initialized.
- Do not treat a successful pointer/type lookup as proof that the object is currently valid.
- A public IL2CPP implementation reported stale/freed entries while traversing `CraftingTemplate.recipes` (`TemplateReference[]`); pointer/class checks alone were insufficient.
- A cached native pointer needs its own lifetime proof.
- Access to a presentation object does not prove ownership of the underlying gameplay state.

## Evidence provenance

Current public evidence comes from Questline's public Merlin Workshop source and public FoA mod source, including `kjharvey101/BepinexModUtilsFoA` and other publicly released mods. Exact signatures, assembly ownership and cross-runtime equivalence should be added only when independently established.
