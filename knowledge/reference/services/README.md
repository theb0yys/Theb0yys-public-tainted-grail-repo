# Services

Use this page when you need a **shared FoA runtime service** and want to know how it is normally obtained and what it is used for.

The common access pattern is:

~~~csharp
World.Services.Get<T>()
~~~

During uncertain startup timing, public mods also use a null-conditional form:

~~~csharp
World.Services?.Get<T>()
~~~

A service type being known does not mean the service is available during plugin `Awake()`.

## Useful services

| Service | What it is used for |
| --- | --- |
| `TemplatesProvider` | typed template lookup and enumeration |
| `TweakSystem` | applying native stat tweaks after the relevant stats are initialized |
| `SceneService` | current scene/domain identity and scene lifecycle information |
| `ViewHosting` | locating UI hosts such as the main canvas |
| `ActorsRegister` | resolving `ActorRef` to actor data |
| `NpcGrid` | native spatial NPC queries such as hearing-range queries |
| `DroppedItemSpawner` | dropped-item parent/placement ownership |
| `UnityUpdateProvider` | registering certain native update-driven owners |
| `GameConstants` | shared game constants |
| `GameplayMemory` | gameplay-context access |
| `LargeFilesStorage` | large-file storage/removal operations used by game systems |

## SceneService quick reference

Documented/publicly exposed members include:

- `MainSceneRef`
- `AdditiveSceneRef`
- `ActiveSceneRef`
- `ActiveSceneLoadTime`
- `IsAdditiveScene`
- `IsOpenWorld`
- `AllowsWyrdnight`
- `IsPrologue`

`ActiveSceneRef` resolves to the active additive scene when one is present, otherwise the main scene.

These values tell you about scene state. They do not mean every scene-owned service or Story has finished initializing.

## When to cache a service

Caching a stable service reference can be reasonable when its lifetime is understood.

Do not assume every service survives every domain/scene transition. If a service is scene-owned or domain-owned, use the lifecycle page for that system before keeping a long-lived reference.

See [Scenes, Services, and Templates](../../systems/core/scenes-services-templates.md).

## Evidence

Service names and access patterns come from Questline public source, public mods, and exact-build static review.

That evidence shows the service exists and how callers obtain it; it does not automatically make every method a stable public mod API.

See [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
