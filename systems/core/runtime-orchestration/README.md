# Runtime Orchestration, Scenes and Templates

## What it is

FoA has a managed startup and service layer that brings core systems online, loads scenes, and populates native template registries.

## Startup orchestration

`Awaken.Orchestrating.Orchestrator.Initialize()` runs at subsystem registration and initializes shared runtime services in a defined order, including:

1. allocation/logging infrastructure;
2. game configuration;
3. shared mipmap textures;
4. Kandra renderer manager;
5. player-loop lifetime support;
6. HLOD manager;
7. animation disposal tracking;
8. Unity update provider.

This means renderer/runtime services can exist before ordinary scene content begins using them.

## Scene service

`SceneService` owns Addressables scene discovery and load/unload operations.

Important responsibilities include:

- indexing scene references;
- preventing duplicate loads;
- `Addressables.LoadSceneAsync`;
- `Addressables.UnloadSceneAsync`;
- scene loaded/initialized handoff;
- initialization of load-based behaviours.

Scene-bound proprietary systems such as Medusa or MergedDrake initialise through this broader scene lifecycle.

## Template loading

`TemplatesLoader` and `TemplatesProvider` own native template loading.

Build/runtime flow:

~~~text
Addressables label "template" / "templateSO"
→ resource locations
→ load by GUID
→ validate ITemplate
→ add to GUID map
→ add to concrete-type map
→ assign template.GUID
→ TemplatesProvider.AllLoaded
~~~

This is the basis for native item/NPC/template lookup.

## Main types

- `Awaken.Orchestrating.Orchestrator`
- `SceneService`
- `TemplatesLoader`
- `TemplatesProvider`
- `TemplateService`

## Deeper reference

- [Startup order and readiness](startup-order-and-readiness.md)

## Modding relevance

Use these owners when you need to know:

- whether native templates are ready;
- who owns a scene transition;
- why a scene-bound service appears only after scene initialization;
- why a template GUID lookup can fail before `AllLoaded`.

## Related systems

- [Scenes Baking](../../world/scenes-baking/README.md)
- [Runtime lifetime](../runtime-lifecycle/README.md)
- [Native weapons](../../gameplay/native-weapons/README.md)
- [Babel](../../world/babel/README.md)
