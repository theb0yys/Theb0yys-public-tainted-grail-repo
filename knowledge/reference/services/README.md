# Services

Canonical ownership: [Scenes/services/templates](../../systems/core/scenes-services-templates.md).

## Publicly visible service-access pattern

Questline public source and public Mono mods repeatedly use:

~~~csharp
World.Services.Get<T>()
~~~

Some public mods use a null-conditional variant while initialization is uncertain:

~~~csharp
World.Services?.Get<T>()
~~~

Treat service availability as lifecycle-scoped. A service type being known does not prove it is available during plugin `Awake()`.

## Useful publicly demonstrated services

| Service | Publicly demonstrated role / access |
| --- | --- |
| `TemplatesProvider` | typed template lookup by GUID via `Get<T>(guid)` |
| `TweakSystem` | native stat-tweak creation/registration after hero stats initialize |
| `SceneService` | active-scene identity, load timing and additive-scene state |
| `ViewHosting` | UI host resolution such as `OnMainCanvas()` |
| `ActorsRegister` | actor-reference resolution; `ActorRef.Get()` delegates to it in Questline source |
| `NpcGrid` | spatial NPC query; Questline source uses `GetHearingNpcs(position, range)` |
| `DroppedItemSpawner` | dropped-item parent/ownership surface |
| `UnityUpdateProvider` | runtime update registration for location spawners |
| `GameConstants` | shared game constants, publicly used for gem costs |
| `GameplayMemory` | gameplay context/memory access |
| `LargeFilesStorage` | public source uses it for sketch file removal |

## Scene-service examples

Questline public source exposes:

- `SceneService.ActiveSceneRef`
- `SceneService.ActiveSceneLoadTime`
- `SceneService.IsAdditiveScene`

These are useful exact lookup surfaces, not proof that every scene transition should be implemented through direct service calls.

## Evidence boundary

This page records publicly visible names and access patterns. Supported mod-facing API status is separate from public visibility.
