# Scenes, Services, and Templates

Use this page when your mod depends on **a service being available, templates being loaded, or a scene being ready**.

These three areas are related because FoA brings them online in stages. A plugin can be loaded while the game systems it wants are still unavailable.

## Template loading

FoA templates are Addressables-backed definitions loaded into native lookup maps.

The researched Mono path is:

~~~text
TemplatesLoader.CreateAndLoad
→ load Addressables label "template"
→ load Addressables label "templateSO"
→ AddToMap(guid, template)
→ FinishedLoading = true
→ normal TemplatesProvider lookup
~~~

GameObject templates must contain an `ITemplate`. ScriptableObject templates must implement `ITemplate`.

`AddToMap(guid, template)` stores the template by GUID and concrete type, then assigns `template.GUID`.

## Looking up templates

`TemplatesProvider` is the normal lookup service.

Useful members include:

- `AllLoaded`
- `Get<T>(guid)`
- `GetAllOfType<T>()`

Do not call `Get<T>` just because you already know the GUID. The provider can exist before its templates are ready.

## Services

Shared game systems are commonly resolved through:

~~~csharp
World.Services.Get<T>()
~~~

Service presence is lifecycle-dependent. A known service type is not proof that the service is available during plugin `Awake()`.

Examples documented elsewhere include:

- `TemplatesProvider`
- `SceneService`
- `TweakSystem`
- `ActorsRegister`
- `NpcGrid`
- `ViewHosting`

## Scene loading

`SceneService` owns FoA's scene discovery and managed load/unload flow.

The researched path includes:

~~~text
Addressables locations labelled "scene"
→ SceneService scene-reference discovery
→ LoadSceneAsync
→ Unity scene load
→ MapScene / AdditiveScene setup
→ SceneLoaded
→ scene initialization
→ SceneInitialized
→ later readiness milestones
→ UnloadSceneAsync
~~~

If you need FoA scene ownership, loading UI, domains, services, and cleanup, prefer this native path over calling Unity's scene loader directly.

## Scene identity is not full readiness

A scene can be known before every system in that scene is ready.

Treat these as different questions:

- Which scene/domain is active?
- Has the Unity scene loaded?
- Has FoA initialized the scene?
- Have later game/story systems finished?
- Is the specific service/model your feature needs available?

Choose the milestone that matches your actual dependency.

## Adding custom templates

If you use a private map-insertion route:

1. wait for native template loading to finish;
2. register the custom template;
3. immediately resolve the custom GUID back through `TemplatesProvider`;
4. only then allow downstream consumers to use it.

Successful insertion is not proof of save/reload safety.

## Adding custom scenes or Addressables

Keep these separate:

~~~text
catalogue/locator installed
≠
asset address resolves
≠
FoA scene reference exists
≠
scene initializes correctly
≠
gameplay objects are registered
~~~

A custom scene may also need native metadata such as `SceneConfig` before travel works correctly.

## Common failures

- template lookup before `AllLoaded`;
- a prefab loads but is not a registered template;
- a custom template is inserted before the native maps stabilize;
- Unity loads a scene but FoA's scene/domain lifecycle is bypassed;
- scene address and Unity scene name do not match;
- a catalogue is installed successfully but no game system actually consumes the content.

## How to verify

For templates:

1. loader finished;
2. source/native GUID resolves;
3. custom insertion succeeds if used;
4. custom GUID resolves through `TemplatesProvider`;
5. returned type is correct.

For scenes:

1. the catalogue/locator resolves the scene;
2. FoA has a scene reference;
3. the transition uses the native path;
4. Addressables loads the scene;
5. `SceneLoaded` / `SceneInitialized` complete;
6. player control resumes where expected;
7. unload/return cleans up correctly.

## Evidence limits

Template-loader/provider behavior is strongly established for the inspected Mono build.

Custom template registration has working implementation evidence but still uses patch-sensitive/private internals and does not have one universal save-safety guarantee.

Scene loading has strong static/source evidence, but each custom-scene route still needs its own runtime validation.
