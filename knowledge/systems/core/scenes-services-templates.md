# Scene, Service, and Template Lifecycle

> **Reference page.** Use this when a mod depends on startup ordering, scene readiness, Addressables-backed templates, or scene transitions.

## What this system is

FoA startup and scene content are orchestrated through several layers:

~~~text
Unity subsystem registration
→ Questline service/renderer managers initialize
→ application/map scene services initialize
→ mod catalogues / Addressables locators
→ scene reference discovery
→ scene load
→ scene load-based behaviors
→ templates/services/runtime gameplay
~~~

Templates are Addressables-backed native definitions loaded into GUID/type maps.

Scenes are also Addressables-driven through `SceneService`.

## Who owns it in FoA

Important owners:

- `Awaken.Orchestrating.Orchestrator` — subsystem-registration startup;
- `SceneService` — scene reference discovery/load/unload;
- `TemplatesLoader` — Addressables template load and maps;
- `TemplatesProvider` — template lookup;
- `TemplateService` — selected abstract/template references;
- `ApplicationScene` / MapScene/AdditiveScene — scene-level service/lifecycle orchestration.

## Important identities, types, and methods

### Startup orchestrator

The researched Mono build initializes shared systems including:

- configuration;
- mip streaming;
- Kandra renderer manager;
- player-loop lifetime;
- HLOD manager;
- Animancer disposal tracking;
- Unity update provider.

This ordering is useful context, not permission to mutate those managers.

### Template loading

`TemplatesLoader.LoadAssetsInBuild()` loads two Addressables labels:

~~~text
template
templateSO
~~~

GameObject templates must contain an `ITemplate`.

ScriptableObject templates must implement `ITemplate`.

`AddToMap(guid, template)`:

- stores in GUID map;
- stores by concrete type;
- assigns `template.GUID = guid`.

`FinishedLoading` becomes true after both passes.

### Provider

`TemplatesProvider.AllLoaded` reflects loader completion.

`Get<T>(guid)` rejects access before readiness and performs GUID + type validation. Exact Mono inspection also exposes `GetAllOfType<T>()` for typed enumeration of loaded templates.

### Scene loading

`SceneService.InitAllSceneReferences()` discovers Addressables locations labelled:

~~~text
scene
~~~

`LoadSceneAsync(sceneRef, mode)` uses Addressables and tracks operations by scene name.

## Where it exists in the lifecycle

### Templates

~~~text
StartLoading
→ Addressables resource locations
→ load template/templateSO assets
→ AddToMap
→ FinishedLoading = true
→ normal provider lookup
~~~

### Scenes

~~~text
mod locators/catalogues installed
→ SceneService builds scene reference list
→ LoadSceneAsync
→ Unity scene creates MapScene/AdditiveScene
→ SceneLoaded
→ scene initialization
→ SceneInitialized
→ later readiness milestones as applicable
→ eventual UnloadSceneAsync
~~~

## How we interact with it

### Template consumers

Wait for readiness. Do not suppress the native readiness exception and proceed with null/default data.

### Custom runtime registration

If using a private map-insertion route, do it only after native readiness and immediately verify normal provider lookup.

### Scene work

Prefer the native scene/loading lifecycle over a parallel `SceneManager.LoadScene` path when you need FoA scene ownership, loading UI, domains, services, and cleanup.

### Mod Addressables

Installed mod locators can contribute Addressables locations before later services query labels. Treat catalogue installation, asset address, and gameplay registration as separate responsibilities.

## Why this route

The source/decompilation work explains several failures that otherwise look random:

- template lookup before readiness throws;
- an asset without the expected `ITemplate` shape is not a valid template;
- a scene address/name mismatch can break native operation tracking;
- a custom scene can need a `SceneConfig` before native travel uses it;
- Addressables catalogue discovery does not prove game-system integration.

## What goes wrong

- calling `TemplatesProvider.Get` before `AllLoaded`;
- assuming a prefab in an Addressables bundle is a registered template;
- changing a custom template map before native load has stabilized;
- loading a custom scene through Unity but bypassing FoA's scene/domain lifecycle;
- mismatching Unity scene name and native scene address;
- treating a static/source-established custom scene route as runtime-proven before the empirical gate runs.

## How to verify

### Template path

Verify:

1. loader reports finished;
2. source/native GUID resolves;
3. custom insertion succeeds if used;
4. custom GUID resolves through provider;
5. correct concrete type is returned.

### Scene path

Verify:

1. mod catalogue/locator resolves the scene;
2. native scene reference exists;
3. transition begins through native lifecycle;
4. Addressables loads the scene;
5. SceneLoaded/SceneInitialized complete;
6. control resumes;
7. unload/return cleans up correctly.

## Current proof boundary

Template loading/provider lifecycle is strongly established for the researched Mono build.

Custom template registration has bounded implementation/runtime evidence but private API risk and incomplete general persistence proof.

