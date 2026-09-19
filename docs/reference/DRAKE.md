# Drake and MergedDrake: FoA's ECS Rigid-Mesh Rendering System

> **Proprietary-system reference.** This page explains Drake as a system first and a modding target second. It combines Questline's public Merlin source surface with current-build static/runtime research and the weapon-importer failure history. It does not reproduce proprietary game source.

## Document status

**System:** Drake / MergedDrake  
**Primary owner:** `Awaken.ECS.DrakeRenderer`  
**Purpose:** rigid mesh rendering through Unity ECS / Entities Graphics while preserving selected GameObject-side ownership and lifecycle  
**Public first-party source:** Questline Merlin Workshop  
**Current-build runtime architecture:** strongly mapped for the inspected Mono build  
**MergedDrake reader/archive grammar:** mapped for the inspected build  
**Historical writer/build pipeline:** recovered from Questline's public historical source and structurally matched to installed payload framing  
**Safe arbitrary custom-resource provider:** **NOT YET GENERALLY PROVEN**  
**Weapon presentation integration:** **PARTIAL PATH PROVEN**  
**Hot unload/reload:** **NOT PROVEN**  
**IL2CPP equivalence:** **NOT PROVEN**

The most important thing to understand is this:

> Drake is not just a component that draws a mesh. It is an ownership system that converts authoring data into ECS render entities, owns mesh/material resource identity and reference counts, publishes Entities Graphics state only after resources are ready, manages LOD-driven load/unload, and tears resources down through its own lifecycle.

If you bypass that ownership, you can make something visible while still being wrong.

---

# 1. What Drake is

Questline built Drake to replace much of the ordinary Unity runtime path for **rigid mesh rendering**.

A normal Unity authoring object might begin with:

~~~text
GameObject
├─ Transform
├─ MeshFilter
├─ MeshRenderer
└─ optional LODGroup
~~~

Drake turns that into a hybrid model:

~~~text
Unity authoring / gameplay owner
        │
        ▼
DrakeMeshRenderer / DrakeLodGroup
        │
        ▼
Questline Drake manager
        │
        ├─ resource identity + reference counts
        ├─ Addressables mesh/material loads
        ├─ ECS archetype selection
        ├─ scene lifetime
        ├─ LOD/load state
        └─ material/mipmap integration
        │
        ▼
Unity Entities Graphics
        │
        ▼
render entities / BatchMeshID / BatchMaterialID
        │
        ▼
GPU rendering
~~~

Drake therefore sits between **gameplay-owned objects** and **Unity's ECS rendering substrate**.

It is not a replacement for Unity itself.

Unity still provides:

- GameObjects and authoring;
- Addressables;
- ECS;
- Entities Graphics;
- transforms;
- materials;
- meshes;
- player-loop systems;
- rendering.

Drake supplies Questline's rules for converting, registering, loading, representing, updating, and cleaning up rigid render content.

---

# 2. What Drake is not

Drake is not:

- the item registry;
- the weapon definition system;
- the inventory owner;
- the combat system;
- the skinned-character renderer;
- the save system;
- a generic custom-asset loader;
- a single call that means "mesh is ready."

For skinned character/clothing rendering, FoA uses **Kandra**, which is a separate proprietary system.

For weapon gameplay, the durable object remains the native `Item` and native equip lifecycle.

Drake owns the rigid presentation layer once that presentation enters Drake correctly.

---

# 3. Why Drake exists

Ordinary Unity rendering attaches one or more `Renderer` components to GameObjects.

That is convenient, but for a large open-world game it can create heavy runtime overhead:

- many managed components;
- many GameObjects;
- per-object update/lifecycle cost;
- expensive large-scene authoring/runtime representation;
- duplicated mesh/material loading state;
- less efficient bulk culling/render preparation.

Drake moves the render-facing part into a more compact ECS representation while retaining enough object-oriented linkage for game systems that still need:

- scene lifetime;
- transforms;
- enable/disable control;
- material changes;
- LOD behavior;
- runtime prefab ownership;
- item/equipment presentation.

MergedDrake goes further for large static populations by skipping much of the temporary per-renderer authoring-object runtime cost.

---

# 4. Ordinary Drake versus MergedDrake

There are two related but different paths.

## Ordinary Drake

Use this mental model for:

- movable rigid objects;
- gameplay-linked objects;
- equipped weapons;
- runtime-spawned rigid presentation;
- renderers that need object-level ownership or linkage.

~~~text
DrakeMeshRenderer / DrakeLodGroup
→ manager registration
→ ECS entity creation
→ resource load
→ spawned/render-ready state
→ LOD/load updates
→ owner-led unload/scene cleanup
~~~

## MergedDrake

Use this mental model for large scene-static content.

~~~text
many static Drake authoring objects
→ scene build merge
→ compact binary scene payload
→ merged_drakes.arch
→ scene runtime reader
→ bulk ECS entity creation
→ ordinary Drake resource realization
~~~

MergedDrake is primarily a **build-time compaction and runtime bulk-instantiation route**.

It is not the path a weapon mod should start from.

---

# 5. Where Drake sits in FoA

A useful cross-system view is:

~~~text
gameplay definition / scene / item
        │
        ▼
native presentation owner
        │
        ├─ weapon: ItemEquip → CharacterHandBase
        ├─ runtime rigid prefab
        ├─ scene static content
        └─ critter / special gameplay consumer
        │
        ▼
Drake authoring bridge
        │
        ▼
Drake ECS/runtime managers
        │
        ▼
Addressables + Entities Graphics + mipmap system
        │
        ▼
rendered rigid geometry
~~~

Drake is therefore a **presentation owner**, not the upstream gameplay definition owner.

That distinction explains many failed custom-weapon attempts.

---

# 6. First-party public surface

Questline's Merlin Workshop exposes the public shape of the system.

Important public types include:

- `DrakeMeshRenderer`;
- `DrakeLodGroup`;
- `DrakeRendererManager`;
- `DrakeRendererLoadingManager`;
- `DrakeRendererComponentsManager`;
- `DrakeRendererEntitiesManager`;
- `DrakeRendererArchetypeKey`;
- `DrakeMergedRenderersLoading`;
- `DrakeMergedRenderersRoot`;
- `DrakeSceneBaker`;
- baking-step interfaces;
- material-override types;
- runtime-spawning types.

Questline's later public source intentionally leaves many implementation bodies as throwing shells, but the types, fields, signatures, ownership boundaries, and build orchestration are still useful first-party evidence.

---

# 7. DrakeMeshRenderer

`DrakeMeshRenderer` is the central ordinary authoring/runtime bridge.

The public surface stores or exposes concepts including:

- local-to-world state;
- local-to-world offset;
- archetype keys;
- serialized render description;
- mesh `AssetReference`;
- material `AssetReference[]`;
- visible range;
- bounds;
- LOD mask;
- parent `DrakeLodGroup`;
- baked state;
- linked entity access;
- linked lifetime;
- runtime material overrides.

Its public `Setup(...)` signature accepts:

~~~text
MeshRenderer
MeshFilter
parent DrakeLodGroup
LOD mask
local-to-world offset
mesh AssetReference
material AssetReference[]
~~~

That signature is extremely important.

It tells us what Drake considers authoring input.

---

# 8. DrakeLodGroup

`DrakeLodGroup` is Drake's bridge for Unity LOD authoring.

It owns concepts including:

- child `DrakeMeshRenderer[]`;
- serialized LOD data;
- group size;
- baked state;
- entity access;
- linked lifetime;
- enable/disable lifecycle;
- occlusion integration.

The public `Setup(LODGroup, DrakeMeshRenderer[])` shows the ordinary conversion relationship:

~~~text
Unity LODGroup
→ DrakeLodGroup
→ child DrakeMeshRenderer representations
~~~

---

# 9. DrakeRendererManager

`DrakeRendererManager` is the main managed owner.

Its public surface exposes:

- singleton instance;
- loading manager;
- components manager;
- entities manager;
- renderer/group registration;
- resource start-loading;
- resource readiness query;
- unload;
- scene lifetime ID;
- initialization update.

The manager is the coordination point between authoring data, resource ownership, ECS entity creation, and scene lifetime.

---

# 10. DrakeRendererLoadingManager

This manager owns the resource-loading records.

The public type exposes mesh and material loading data.

Each loading row conceptually contains:

~~~text
runtime key
loading handle
ushort owner/reference counter
~~~

The current-build static implementation establishes the important ownership rule:

~~~text
first owner: counter 0 → 1
    starts load

additional owner: counter N → N+1
    shares same resource/handle

release owner: counter N → N-1

last owner: counter 1 → 0
    releases handle/resource
~~~

This is one of Drake's most important invariants.

---

# 11. Resource identities

Drake uses compact runtime indices internally.

The current runtime has separate mesh and material identity spaces.

The key chain is conceptually:

~~~text
stable resource key
→ Drake ushort runtime index
→ loading row
→ loaded Unity Mesh/Material
→ Entities Graphics ID
→ ECS renderer component
~~~

Do not confuse:

- an Addressables key;
- an AssetReference GUID;
- a Drake ushort runtime index;
- `BatchMeshID`;
- `BatchMaterialID`;
- an ECS entity ID.

They are different identity layers.

---

# 12. Mesh keys

Current MergedDrake evidence shows mesh keys can take an Addressables-style form:

~~~text
asset-guid
~~~

or for subobjects:

~~~text
asset-guid[subobject-name]
~~~

That matters because a mesh can be a subasset rather than the main asset at a GUID.

A provider that only understands base GUIDs may resolve the wrong object or fail to resolve the mesh.

---

# 13. Material keys

MergedDrake stores material asset GUID identity.

The runtime formats material keys as lowercase compact GUID strings for lookup.

Again, this is asset/resource identity.

It is not a weapon/item GUID.

---

# 14. Archetype selection

Drake does not create one giant ECS component set for every renderer.

The runtime chooses an archetype based on dimensions such as:

- static versus movable;
- transparent versus opaque;
- LOD versus no LOD;
- motion-pass participation;
- light-probe mode;
- shadow override;
- local-to-world offset.

The inspected runtime materializes 256 combinations from these axes.

The important idea is:

> Renderer features are encoded structurally into the ECS archetype.

Changing presentation properties therefore may affect more than one field.

---

# 15. Base renderer entity state

The common renderer entity includes concepts such as:

- world render bounds;
- mesh/material compact identity;
- culling tag;
- local/world transforms;
- mipmap factor state;
- render filter settings;
- scene/lifetime ownership;
- processed-shadow state.

Additional components depend on the selected archetype.

---

# 16. Static versus movable representation

Static and movable Drake are not equivalent.

Movable variants need transform linkage and runtime bounds updates.

Static variants can use cheaper assumptions.

A weapon is not scene-static merely because the mesh itself is rigid.

Equipped weapons move with the character and belong to the movable/native equip presentation route.

---

# 17. Linked lifetime

Drake can preserve a lifetime relationship with the GameObject-side owner.

For native equipped weapons, this is important because the GameObject/View still owns gameplay presentation lifecycle while ECS entities own the render-facing representation.

The native weapon route uses representation options that include linked lifetime and movement.

That is a strong clue for safe custom weapon integration:

~~~text
preserve the native hand/view owner
preserve linked lifetime
let Drake own render entities underneath
~~~

---

# 18. Linked entity access

Some authoring profiles can request access to linked ECS entities.

This is not automatically required for every Drake object.

The MergedDrake writer specifically excludes some content that requires entity access because that content needs a more direct object/ECS relationship.

Do not generalize linked entity access as mandatory.

---

# 19. Scene lifetime

Drake entities carry Questline scene-lifetime ownership.

The manager subscribes to scene unload.

When the owning scene disappears, scene-lifetime entities are destroyed and remaining resources are cleaned through the Drake systems.

This is why a mod should not create unowned render entities and assume scene teardown will discover them.

---

# 20. Bootstrap

The current managed lifecycle begins before ordinary scene content is fully active.

Conceptually:

~~~text
AwakenEcsBootstrap.Initialize
→ default ECS world exists
→ DrakeRendererManager.Create
→ Drake initialization update added to Unity player loop
→ manager owns default EntityManager / EntitiesGraphicsSystem / support managers
→ scene-unload subscription active
~~~

Drake is therefore a process-level rendering service with scene-owned content.

---

# 21. Ordinary authoring conversion

The recovered Questline editor path begins with ordinary Unity render components.

Conceptually:

~~~text
MeshRenderer + MeshFilter
(+ optional LODGroup)
→ editor admission checks
→ create/resolve Addressables references
→ create DrakeMeshRenderer
→ call DrakeMeshRenderer.Setup
→ create/convert DrakeLodGroup where needed
→ source Unity renderer representation removed/replaced
~~~

The exact current editor implementation is not public, but the historical first-party path matches the current reader/runtime model closely enough to explain the architecture.

---

# 22. Historical editor admission rules

Questline's recovered historical converter checked things such as:

- source asset is a persistent project asset;
- mesh exists;
- at least one material exists;
- materials meet DOTS/renderer expectations;
- Addressables settings/group exists;
- asset GUID can be resolved;
- subobject name is recorded when needed;
- existing addressable ownership is preserved;
- missing entries can be assigned to the Drake group;
- LOD membership is converted into a mask;
- parent/child transform offset is captured.

These are historical first-party implementation details.

They explain the authoring contract, but they are not automatically the exact current editor implementation.

---

# 23. Why Drake uses Addressables

Ordinary Drake does not own raw mesh files.

It owns resource keys that resolve through Unity Addressables.

Conceptually:

~~~text
Drake resource key
→ Addressables
→ AsyncOperationHandle<Mesh/Material>
→ loaded Unity object
→ Drake resource manager
→ Entities Graphics registration
~~~

This is critical for modding.

A custom `Mesh` object sitting in memory is not automatically a Drake resource.

---

# 24. First-owner load behavior

Current static evidence establishes:

~~~text
counter 0
→ StartLoadingMesh/Material
→ Addressables.LoadAssetAsync
→ counter becomes owned
→ handle tracked
~~~

The manager's first-owner transition owns handle creation.

That is why replacing `StartLoadingMesh` or `StartLoadingMaterial` is dangerous.

---

# 25. Shared-owner behavior

When the same resource index has multiple owners, Drake increments the counter rather than creating an independent load for every entity.

This provides sharing.

A custom provider must preserve that ownership model.

---

# 26. Last-owner release behavior

The last release transitions:

~~~text
counter 1 → 0
→ graphics/resource unregister as required
→ Addressables handle release
→ loading state cleared
~~~

Skipping that release because a mod wants to “retain” the asset breaks owner symmetry.

---

# 27. Counter hazard

The inspected methods use `ushort` counters and do not expose an obvious underflow/overflow guard in the relevant bodies.

That means duplicate release is not something a mod should paper over.

If the counter is unexpectedly zero when an owner tries to release:

~~~text
stop
record the anomaly
find the duplicate release
~~~

Do not silently fabricate counter state.

---

# 28. Components manager

`DrakeRendererComponentsManager` converts loaded Unity resources into Entities Graphics registrations.

On successful completion:

~~~text
loaded Mesh
→ EntitiesGraphicsSystem.RegisterMesh
→ BatchMeshID

loaded Material
→ EntitiesGraphicsSystem.RegisterMaterial
→ BatchMaterialID
→ Questline mipmap-material registration
~~~

It also tracks UV distribution data used by the mipmap system.

---

# 29. Resource-ready components

Only after the graphics resources exist can Drake publish render-facing components such as:

- `MaterialMeshInfo`;
- mipmap material state;
- UV distribution metric.

The important rule is:

~~~text
resource key exists
!= asset loaded
!= Entities Graphics ID exists
!= renderer entity is spawned/ready
~~~

---

# 30. Drake entity lifecycle tags

Drake uses ECS tags/components as a state machine.

The important conceptual states are:

~~~text
unloaded
→ load requested
→ loading
→ spawned/resource-ready
→ unload requested
→ unloaded
~~~

Manual/debug/freeze tags can alter normal participation.

These tags are owner state.

A mod must not use them as convenient booleans to fake progress.

---

# 31. Load-request state

An eligible renderer inside its visibility range can receive a load request.

This is a request, not completion.

At this point the renderer may still have:

- no loaded mesh;
- no loaded material;
- no graphics IDs;
- no final spawned state.

---

# 32. Loading state

The loading system starts resource ownership and replaces the request state with loading state.

Completion waits for both resource sides to be ready.

---

# 33. Spawned state

The terminal ordinary render-ready state requires the native loading system to publish the resource-facing components and add the spawned state.

That is the point at which Drake considers the render entity fully realized for the current load cycle.

A mod adding the spawned tag itself does not make this true.

---

# 34. LOD and visibility state system

Drake owns distance-based load/unload decisions.

For perspective cameras, the runtime compares camera distance against prepared visible ranges.

For orthographic cameras, the behavior differs and eligible unloaded entities can be requested more aggressively.

The exact behavior matters for:

- world objects;
- previews;
- presentation cameras;
- debugging.

---

# 35. Visible range

`DrakeRendererVisibleRangeComponent` represents visibility/load-distance policy.

It is not merely a Unity `Renderer.enabled` flag.

Loading, resource residency, and visible range are related but separate concepts.

---

# 36. LOD hysteresis

Current runtime evidence shows Drake prepares separate visible/preload ranges and applies hysteresis.

This prevents constant load/unload thrashing around a distance boundary.

A custom path that overwrites range state casually can cause lifecycle churn.

---

# 37. Freeze behavior

Game systems can freeze Drake LOD/load state during sensitive transitions.

Examples of known consumers include:

- loading screens;
- interior transitions;
- UI/presentation states;
- lockpicking-related presentation.

This shows Drake's state system is part of broader game presentation coordination.

---

# 38. Mipmap integration

When materials become ready, Drake registers them with Questline's shared mipmap-demand system.

Therefore material lifetime is not just:

~~~text
Addressables loaded
~~~

It also participates in:

~~~text
renderer ownership
→ material registration
→ mipmap-demand ownership
→ later material removal
~~~

A custom path must not forget this downstream owner.

---

# 39. Runtime materials

Drake also supports runtime material identities.

The current implementation can represent a runtime material through an immediately completed ResourceManager operation.

Runtime material identity differs from serialized Addressables material identity.

Do not treat runtime material hash identity as a stable cross-session asset ID.

---

# 40. Material property overrides

Drake exposes ECS material-property components for properties including:

- alpha;
- VAT animation;
- dissolve transition;
- emission intensity;
- emissive color;
- ghost transparency;
- base color;
- transition;
- rotation speed.

This lets gameplay alter shader-facing state without replacing the whole material asset.

---

# 41. Material replacement

Material replacement must account for the renderer's current resource state.

Current static evidence shows replacement logic distinguishes states such as:

- unloaded;
- unload-requested;
- load-requested;
- loading;
- spawned.

That is another example of lifecycle-aware mutation.

Replacing an index without considering current ownership can leak or double-release resources.

---

# 42. Runtime prefab spawning

Drake also has a runtime prefab route.

The system can create Drake entity prefab data and later instantiate render entities from it.

Game-side consumers include specialized runtime systems such as critters and elevator chains.

The existence of this route does not make it the correct route for equipped weapons.

Weapons already have a native `ItemEquip → CharacterHandBase` owner.

---

# 43. Transform synchronization

Runtime Drake content can synchronize ECS visual entity transforms with GameObject-side transforms where the profile requires it.

This is another reason not to write `LocalToWorld` arbitrarily.

A persistent transform should be expressed through the owning input/linkage mechanism.

---

# 44. Scene cleanup

Drake has an explicit scene cleanup system.

When scene-lifetime ownership disappears, the system cleans remaining loading/spawned resources and removes cleanup state.

A correct custom integration should leave this owner intact.

---

# 45. Process shutdown

The manager has process-level teardown.

Static evidence does not establish that calling a destroy method is a complete safe **hot restart** contract.

Therefore:

~~~text
process shutdown cleanup
!= supported in-session Drake restart
~~~

Do not claim hot reload from shutdown APIs.

---

# 46. The native weapon route into Drake

For weapons, Drake is downstream of the native equipment system.

Questline's public `ItemEquip` source shows the normal path.

~~~text
Item equipped
→ ItemEquip selects ARAssetReference
→ asset loads
→ GameObject instantiated under native item socket
→ require CharacterHandBase
→ SetUnityRepresentation(linkedLifetime=true, movable=true)
→ World.BindView(Item, CharacterHandBase)
→ character.AttachWeapon(...)
→ Drake authoring components underneath enter Drake
~~~

This is the route a custom rigid weapon should preserve.

---

# 47. Why CharacterHandBase matters

The equipped prefab is not merely a visual shell.

`CharacterHandBase` is the native weapon View.

The game binds it to the `Item`.

The character attaches it through native ownership.

Drake then renders the rigid presentation underneath that owner.

A mod that skips `CharacterHandBase` can render a mesh while bypassing the actual equipped-weapon presentation lifecycle.

---

# 48. Why SetUnityRepresentation matters

The native equip path requests Unity representation options that include:

~~~text
linkedLifetime = true
movable = true
~~~

That tells the presentation system this is not scene-static content.

The object needs a relationship to its moving owner and its lifecycle.

---

# 49. FPP and TPP presentation

The native weapon owner also changes shadow/render presentation based on perspective.

The public item/equip path walks child Drake renderers and modifies their serialized render description for the current perspective.

This proves that FPP/TPP presentation policy is above the raw resource-loading layer.

A custom weapon must validate FPP and TPP separately.

---

# 50. Inventory preview is another owner

Inventory/equipment preview has its own native presentation owner and camera/layer contract.

A weapon looking correct in FPP does not prove preview correctness.

Drake resource state can be shared while the presentation context differs.

---

# 51. The safe conceptual mod entry point

For a rigid custom weapon, the safest currently researched direction is:

~~~text
native weapon prototype
→ preserve CharacterHandBase / Drake authoring hierarchy
→ replace only the intended presentation asset identity
→ call the normal DrakeMeshRenderer authoring boundary
→ provide custom mesh/material resources through a typed provider
→ let native Drake loading/resource systems run
→ let native equip/view ownership continue
~~~

The key phrase is:

> **Let Drake own Drake.**

---

# 52. What the mod should own

A custom weapon importer may reasonably own:

- package identity;
- weapon identity;
- custom mesh/material assets;
- package validation;
- custom resource key namespace;
- a provider/resource-locator boundary;
- native prototype selection;
- the authoring-time rebind request;
- receipts/diagnostics.

It should not own a second copy of Drake's internal state machine.

---

# 53. What Drake should continue to own

Drake should continue to own:

- resource index allocation;
- first-owner load transition;
- reference counters;
- loading handles;
- graphics registration;
- resource-ready publication;
- load/loading/spawned/unload lifecycle tags;
- LOD-driven load/unload;
- last-owner release;
- scene-lifetime cleanup.

This is the central owner boundary.

---

# 54. Preferred weapon authoring strategy

The current importer research favors:

~~~text
clone reviewed native CharacterHandBase weapon prefab
→ preserve DrakeLodGroup / DrakeMeshRenderer structure
→ identify the supported renderer profile
→ call DrakeMeshRenderer.Setup(...) with custom resource references
→ let native equip load the prototype
→ let Drake register and load normally
~~~

This preserves:

- native hand/view ownership;
- native item/equip lifecycle;
- native movement/lifetime;
- native Drake authoring shape.

---

# 55. Conservative first profile

The first reusable rigid-melee profile currently constrains the prototype to a simple shape such as one accepted Drake renderer.

That is an **importer safety constraint**, not a universal Drake requirement.

Drake itself supports more complex render structures.

The importer starts narrow because the validation surface is smaller.

---

# 56. The remaining provider problem

The largest unresolved custom-resource problem is not “how do we create a Drake entity?”

The larger problem is:

> How do custom mesh/material keys resolve through a supported provider while preserving Drake's normal load/counter/release ownership?

The desired direction is:

~~~text
mod package
→ typed resource/provider integration
→ normal Drake resource key
→ native StartLoading
→ native handle ownership
→ native graphics registration
→ native release
~~~

That provider route still requires controlled proof before it can be presented as universally solved.

---

# 57. Why synthetic handle injection is weak

A previous implementation created synthetic completed AssetReference/operation state through reflection.

That can make the next layer believe an asset is ready.

But it does not automatically prove:

- normal provider ownership;
- cancellation behavior;
- failure behavior;
- handle release symmetry;
- repeated-load semantics;
- duplicate-owner semantics;
- compatibility after updates.

Therefore it remains an unproven runtime-dependent bridge, not the preferred long-term contract.

---

# 58. Why global Addressables interception is wrong

Broad generic Addressables hooks can affect unrelated game loads.

This contaminates the system boundary.

The rule is:

~~~text
custom Drake asset keys
→ typed/narrow provider route

not

all Addressables<Mesh/Material> calls
→ maybe ours
~~~

---

# 59. Why replacing StartLoadingMesh is wrong

`StartLoadingMesh` owns:

- the first-owner transition;
- handle creation;
- counter state;
- resource-key state.

Suppressing it and emulating its work creates a second owner.

That is not merely untested.

It contradicts the owner model.

---

# 60. Why replacing StartLoadingMaterial is wrong

The same rule applies to material loading.

Material loading also interacts with load events and mipmap ownership.

A replacement can leave the resource apparently visible while breaking later release or material-dependent systems.

---

# 61. Why writing private loading rows is wrong

Direct reflection writes into Drake's private mesh/material loading arrays bypass:

- index allocation assumptions;
- handle ownership;
- counter transitions;
- events;
- version encapsulation.

Read-only inspection may be useful for diagnostics.

Mutation is not a supported custom-content contract.

---

# 62. Why raising internal pending bits is wrong

The components manager owns pending-load state.

Directly raising its bitmasks fabricates native work.

It does not prove the normal transition happened.

---

# 63. Why calling UpdateLoadings manually is wrong

The manager's polling runs in its scheduled player-loop phase.

Calling it from a weapon observer changes timing and can cause reentrancy/order differences.

Observe the owner.

Do not become the owner.

---

# 64. Why publishing MaterialMeshInfo manually is wrong

The native loading system publishes render-facing components only after resource completion.

Manually adding:

- `MaterialMeshInfo`;
- mipmap component;
- UV metric;

creates the appearance of completion without the preceding native lifecycle.

---

# 65. Why adding DrakeRendererSpawnedTag manually is wrong

`Spawned` is a lifecycle result.

It is not an instruction.

If a mod adds the tag itself, it can erase evidence that:

- loading failed;
- graphics IDs are zero;
- handles never completed;
- unload work is pending.

---

# 66. Why suppressing unload is wrong

A “retained custom key” does not override Drake owner counts.

If Drake acquired a resource owner, that owner must later release it.

Skipping unload can cause:

- counter mismatch;
- retained handles;
- retained graphics IDs;
- stale mipmap ownership;
- failed remount;
- leaks.

Provider retention and Drake ownership are separate layers.

---

# 67. Why direct LocalToWorld writes are wrong

`LocalToWorld` and world bounds are derived/owned by transform and rendering systems.

Direct writes can be overwritten or conflict with:

- linked transform;
- culling;
- bounds updates;
- perspective presentation.

Express placement through the owning authoring/linkage input.

---

# 68. Why forcing the hierarchy active is weak

If the native owner deliberately hides or disables presentation for a perspective/state, forcing nodes active may bypass the correct presentation policy.

It can make the custom model visible while producing:

- FPP/TPP duplication;
- preview leakage;
- hidden weapon state errors.

Visibility must be proven at the correct owner boundary.

---

# 69. The major custom-weapon failure lesson

One custom weapon experiment made a normal Unity renderer visible while the original weapon still rendered.

That observation proved:

~~~text
custom renderer visibility
!= replacement of native Drake presentation
~~~

The original visible weapon was still owned by the native Drake path.

This failure is one of the strongest pieces of evidence for preserving the Drake owner.

---

# 70. Another failure: inactive native presentation root

A custom child was attached beneath a native Drake-authored hierarchy that was inactive.

This showed that ordinary Transform parenting did not reproduce the renderer lifecycle.

The correction was to move toward the actual Drake representation boundary rather than keep patching the hierarchy.

---

# 71. Another failure: apparent readiness from hooks

A framework once described presentation as ready because the required Harmony/reflection surfaces were present.

That proves only:

~~~text
structural hook surface available
~~~

It does not prove:

- resources load;
- graphics IDs exist;
- entity becomes spawned;
- visible output is correct;
- unload works.

Readiness vocabulary must match the evidence.

---

# 72. Accepted authoring conditions

For the ordinary Drake route, accepted inputs conceptually include:

- a baked/valid `DrakeMeshRenderer`;
- coherent mesh/material resource references;
- valid render description;
- valid bounds;
- valid LOD/profile data when applicable;
- a supported scene/lifetime owner;
- a supported archetype combination.

---

# 73. Rejected conditions

Current static evidence shows registration rejects at least an unbaked renderer condition.

Frameworks should additionally fail closed on their own unsupported profiles.

Examples:

- missing mesh identity;
- missing materials;
- unknown provider key;
- custom importer profile mismatch;
- ambiguous native prototype;
- unsupported renderer topology.

Keep native rejections and framework safety restrictions labelled separately.

---

# 74. Queued/deferred conditions

Drake's important deferred conditions are resource and lifecycle states.

Examples:

~~~text
load requested
→ waiting for load to start

loading
→ waiting for mesh/material handles

registered entity
→ waiting for resource-facing components / spawned state
~~~

A queued state is not failure.

It is also not success.

---

# 75. Resource load failure

If a mesh or material handle fails, the correct test should observe the failure without repairing native state from the probe.

The system must not be advanced manually to spawned.

Failure needs to remain visible.

---

# 76. Dirty/partial failure

Some private intervention attempts can leave mixed state because they write owner arrays, counters, tags, or graphics components directly.

Those failures are particularly dangerous because a visible output may coexist with broken owner bookkeeping.

The rule is:

> If owner state was manually fabricated, do not use that run as evidence that the owner lifecycle works.

---

# 77. Ordinary Drake terminal success state

For one load cycle, a strong render-ready state requires:

- native owner accepted authoring/entity state;
- required mesh/material owners acquired;
- handles succeeded;
- graphics IDs registered;
- native loading system published render-facing components;
- native spawned state present;
- no contradictory transitional state;
- presentation visible in the intended context.

This is stronger than `TryGetMaterialMesh` returning data once.

---

# 78. Ordinary Drake terminal cleanup state

A strong cleanup state requires:

- renderer owner released;
- mesh/material counters balanced;
- last owner unregisters graphics identities;
- last owner releases handles;
- no stale spawned/loading entity;
- scene lifetime removed where expected;
- provider ownership also reaches its expected terminal state.

Creation and teardown are symmetrical claims.

---

# 79. MergedDrake purpose

MergedDrake exists because large static scenes can contain huge numbers of Drake renderers.

Loading thousands of authoring MonoBehaviours simply to generate ECS entities is wasteful.

MergedDrake serializes the spawn description ahead of time.

---

# 80. MergedDrake build path

Recovered Questline first-party history shows:

~~~text
static scene hierarchy
→ StaticObjects root
→ DrakeMergedRenderersRoot
→ scene baker
→ eligible static Drake records
→ Library/DrakeMR/<guid>.data
→ Unity Archive build
→ StreamingAssets/DrakeMR/merged_drakes.arch
→ Addressables content build
~~~

This is a build pipeline.

It is not a recommended runtime mod injection seam.

---

# 81. MergedDrake eligibility

The recovered historical writer only merged content satisfying constraints such as:

- static;
- correct merged root ownership;
- no required entity access;
- no incompatible baking steps;
- accepted modification-step behavior.

Those rules explain why MergedDrake is specialized.

---

# 82. MergedDrake payload grammar

For the inspected build, each payload contains five length-prefixed arrays:

~~~text
LOD groups
mesh records
material GUIDs
renderer definitions
renderer instances
~~~

The current reader and installed payloads agree on this framing.

This is useful architecture knowledge.

It is **not** permission to generate and install replacement archives.

---

# 83. What MergedDrake records contain

The current payload carries compact render-spawn information such as:

- LOD data;
- mesh resource key;
- mesh bounds;
- UV distribution;
- material GUID ranges;
- render filter settings;
- LOD mask;
- light probe mode;
- transparency mask;
- world transform;
- definition indexes.

The mesh/material asset bytes remain resolved through the ordinary resource system.

---

# 84. MergedDrake archive

The inspected build stores MergedDrake data inside:

~~~text
StreamingAssets/DrakeMR/merged_drakes.arch
~~~

The outer container is Unity Archive / UnityFS-oriented infrastructure.

The proprietary part is the MergedDrake record content, not the fact that the outer archive exists.

---

# 85. MergedDrake runtime path

Conceptually:

~~~text
scene load
→ DrakeMergedRenderers init
→ mount/open archive
→ resolve scene data GUID
→ read five arrays
→ resolve mesh/material keys through Drake resource owner
→ create static archetype variants
→ bulk-create entities
→ dispose temporary source arrays
→ ordinary Drake loading/graphics state realizes resources
~~~

---

# 86. MergedDrake is not a custom weapon path

A weapon is:

- movable;
- gameplay linked;
- equipped/unequipped;
- perspective dependent;
- View owned.

MergedDrake is optimized for static scene populations.

Do not use the existence of a binary writer to justify putting weapons into MergedDrake.

---

# 87. Golden Rule — use the actual owner

For Drake:

~~~text
Drake loading manager
owns resource acquisition

Drake components manager
owns graphics registration

Drake loading system
owns Loading → Spawned

Drake state system
owns LOD/load requests

Drake scene system
owns scene cleanup
~~~

Do not duplicate these owners.

---

# 88. Golden Rule — visible is not correctly owned

A Unity renderer can be visible while the Drake-owned original is also visible.

That is a failed integration, not a partial success.

---

# 89. Golden Rule — registration is not readiness

Entity admission is earlier than resource readiness.

Resource handles must complete.

Graphics identities must exist.

Native spawned state must be published.

---

# 90. Golden Rule — asset exists is not provider ownership

A `Mesh` or `Material` object can exist in memory without participating in Drake's expected resource-key and release lifecycle.

---

# 91. Golden Rule — state tags are evidence, not buttons

`LoadRequest`, `Loading`, `Spawned`, and unload state belong to the native state machine.

Do not write them to make a test pass.

---

# 92. Golden Rule — first-owner/last-owner symmetry

Every resource acquired through Drake ownership needs a matching release.

One missing or duplicated release can invalidate later lifecycle evidence.

---

# 93. Golden Rule — provider lifetime and Drake lifetime are distinct

A mod package may keep an AssetBundle mounted while Drake has zero owners.

Or Drake may still own a loaded asset while the package layer wants to unmount.

The provider and renderer lifetimes must be coordinated explicitly.

---

# 94. Golden Rule — perspective is a separate proof lane

FPP, TPP, and inventory preview may share assets but not necessarily:

- hierarchy state;
- layer state;
- camera visibility;
- transforms;
- shadow policy;
- lifecycle timing.

Test them independently.

---

# 95. Golden Rule — no MergedDrake writer claim from reader knowledge

Understanding the installed reader and historical writer framing is not the same as proving a newly generated current-build archive is valid.

Writer validation needs its own controlled bake/runtime proof.

---

# 96. Golden Rule — version scope matters

Drake is private, complex, and tied to:

- Questline assemblies;
- Unity version;
- Entities Graphics;
- Addressables;
- archive format;
- player-loop behavior.

A game update can invalidate private layout assumptions without changing public names.

---

# 97. Proven rigid-weapon process through Drake

The best current process is intentionally owner-preserving.

## Stage 1 — choose one reviewed native weapon presentation

**Do:** pick a known rigid weapon whose equipped prefab already uses the desired native Drake route.

**Native contract:** establishes a real `CharacterHandBase` / Drake authoring baseline.

**Why:** you need a native presentation object whose lifetime, perspective behavior, and equip ownership are already valid.

**Failure:** choosing a special/bespoke weapon can import hidden behavior you do not understand.

**Verify:** exact source identity and equipped presentation structure are recorded.

**Boundary:** no custom asset yet.

---

# 98. Stage 2 — preserve the native View owner

**Do:** keep the native `CharacterHandBase` structure rather than constructing a visual-only object.

**Native contract:** `ItemEquip` requires `CharacterHandBase` and binds it to the `Item`.

**Why:** this preserves equip/view/character ownership.

**Failure:** direct mesh attachment can render while bypassing native View lifecycle.

**Verify:** custom prototype still has the required hand/view type.

**Boundary:** does not prove Drake resources.

---

# 99. Stage 3 — preserve the Drake authoring hierarchy

**Do:** keep the reviewed `DrakeLodGroup` / `DrakeMeshRenderer` structure required by the profile.

**Native contract:** Drake registration starts from these authoring components.

**Why:** synthetic ECS construction duplicates internal archetype/resource logic.

**Failure:** manually creating entities/components can produce a shape that looks right but is not owner-equivalent.

**Verify:** expected authoring components and profile cardinality are present.

---

# 100. Stage 4 — allocate stable custom resource identity

**Do:** create deterministic package-owned mesh/material keys.

**Native contract:** Drake resource tables require stable keys.

**Why:** idempotency, diagnostics, sharing, and release all depend on identity.

**Failure:** runtime-random or colliding keys make resource ownership ambiguous.

**Verify:** keys are unique, deterministic, and tied to package/weapon identity.

---

# 101. Stage 5 — validate custom source assets

**Do:** validate bundle/platform, exact mesh/material asset types, dependencies, hashes, renderer profile, and unsupported components.

**Native contract:** Drake expects a resolvable Mesh and Material resource.

**Why:** a bad asset should fail before it enters native resource state.

**Failure:** wrong platform, missing material, unsupported shader, ambiguous subasset.

**Verify:** isolated asset/provider checks pass.

**Boundary:** not yet a Drake lifecycle pass.

---

# 102. Stage 6 — rebind through DrakeMeshRenderer.Setup

**Do:** use the owner-facing authoring boundary to bind the custom mesh/material references while preserving LOD/profile data.

**Native contract:** `Setup` is the public authoring relationship between Unity renderer input, resource refs, LOD mask, and transform offset.

**Why:** this keeps authoring data coherent.

**Failure:** direct private-field edits can miss derived state/archetype data.

**Verify:** resulting authoring profile matches the intended source profile except approved presentation changes.

---

# 103. Stage 7 — supply resources through a typed provider

**Do:** expose the custom keys through the selected narrow provider/locator contract.

**Native contract:** Drake must be able to resolve the keys during its normal load transition.

**Why:** Drake should still own counters/handles/graphics registration.

**Failure:** synthetic handle injection or global Addressables interception creates an unproven second resource lifecycle.

**Verify:** custom keys resolve only through the intended typed route.

**Boundary:** this stage is the largest remaining generic proof gap.

---

# 104. Stage 8 — let native equip enter Drake

**Do:** allow `ItemEquip` to load and instantiate the custom prototype through the native equipped-weapon path.

**Native contract:** native item/equip ownership creates the moving/linked presentation.

**Why:** this preserves gameplay lifecycle.

**Failure:** bypassing native equip can create a disconnected renderer.

**Verify:** `CharacterHandBase` is instantiated and bound to the expected custom `Item`.

---

# 105. Stage 9 — observe native Drake registration

**Do:** observe authoring registration and resulting entity identity without mutating owner state.

**Native contract:** Drake manager owns entity admission.

**Why:** registration is the first native rendering transition.

**Failure:** missing/bad authoring state is rejected or produces no correct entity.

**Verify:** expected linked/render entities can be correlated unambiguously.

---

# 106. Stage 10 — observe owner resource acquisition

**Do:** observe native mesh/material owner counters and handles.

**Native contract:** first-owner transition starts native resource loading.

**Why:** this proves Drake, not the mod, owns the resource transition.

**Failure:** no load begins, wrong key, duplicate owner, failed handle.

**Verify:** one first-owner acquisition per resource and expected sharing behavior.

---

# 107. Stage 11 — observe Loading → Spawned

**Do:** wait for the native loading system to publish graphics components and spawned state.

**Native contract:** resource completion is owner-driven.

**Why:** this is the terminal resource-ready transition.

**Failure:** spawned with zero graphics ID, loading stall, failed handle, ambiguous entity.

**Verify:** non-zero graphics identities, no contradictory transitional state, expected spawned marker.

---

# 108. Stage 12 — verify visible presentation

**Do:** inspect the actual rendered custom weapon in the intended perspective.

**Native contract:** rendering output should correspond to the custom Drake resource identity.

**Why:** lifecycle correctness still needs visual proof.

**Failure:** old native weapon visible, duplicate renderer, wrong material, wrong bounds/transform.

**Verify:** custom presentation visible and native duplicate absent.

---

# 109. Stage 13 — verify FPP

Check independently:

- intended custom mesh;
- material;
- socket/transform;
- shadow policy;
- camera/layer visibility;
- no original duplicate;
- attacks remain aligned.

Do not copy TPP results.

---

# 110. Stage 14 — verify TPP

Check independently:

- third-person visibility;
- correct socket/offset;
- locomotion/attack alignment;
- shadow/filtering;
- no FPP-only policy leaked into TPP.

---

# 111. Stage 15 — verify inventory preview

Check independently:

- native preview owner;
- preview camera;
- layers/render filter;
- open/close lifecycle;
- no leaked world/FPP entity;
- no retained resource owner after preview closes unless legitimately shared.

---

# 112. Stage 16 — verify resource sharing

Use at least two simultaneous legitimate owners where the profile requires it.

Verify:

- counter increments;
- same handle/resource reused;
- first release leaves shared resource alive;
- final release performs terminal cleanup.

---

# 113. Stage 17 — verify unequip

Observe:

~~~text
native item unequip
→ hand/view cleanup
→ Drake owner release
→ counters decrement
→ graphics IDs eventually unregister on final owner
→ handles release
~~~

A hidden mesh is not cleanup proof.

---

# 114. Stage 18 — repeat equip/unequip

Repeated lifecycle testing is required because reference-count and entity leaks may not appear on the first cycle.

Verify no monotonic growth in:

- tracked owners;
- handles;
- graphics IDs;
- spawned entities;
- stale prototypes.

---

# 115. Stage 19 — weapon swap

A swap creates overlapping lifecycle windows.

Verify:

- old weapon begins release;
- new weapon acquires;
- shared resource counts remain coherent;
- no duplicate final release;
- no retained old presentation.

---

# 116. Stage 20 — scene transition

Verify:

- linked scene/lifetime ownership;
- equipped presentation remains correct or rebuilds as expected;
- old scene entities clean;
- resource counts remain coherent;
- no orphan entity survives the stop point.

---

# 117. Stage 21 — failed resource load

Use a disposable failure fixture.

Verify:

- failure remains visible;
- no probe repairs native state;
- no spawned claim is emitted;
- counters/handles return to a valid terminal state;
- later clean run is not contaminated.

---

# 118. Stage 22 — process shutdown

Verify process-level cleanup separately from hot restart.

Do not claim in-session reload/unload simply because shutdown succeeds.

---

# 119. Test matrix — static/source

A Drake integration should statically verify:

- exact assembly/version scope;
- required public/private member identity;
- authoring profile;
- resource-key identity;
- no duplicate custom key;
- no prohibited global Addressables hook;
- no private loading-array writes;
- no StartLoading suppression;
- no pending-bit mutation;
- no direct UpdateLoadings call;
- no manual graphics-component publication;
- no manual lifecycle-tag completion;
- no unload suppression;
- no direct persistent `LocalToWorld`/bounds repair path.

Static pass proves structure only.

---

# 120. Test matrix — vanilla baseline

Before custom proof, capture a vanilla weapon using the same native profile.

Record:

- source template;
- hand/view path;
- authoring renderer count;
- linked entity mapping;
- mesh/material indices;
- graphics IDs;
- owner counters;
- FPP/TPP classification;
- preview route;
- load/spawn/unload event order;
- final cleanup.

Without a baseline, custom differences are hard to interpret.

---

# 121. Test matrix — single custom equip

Minimum success:

- package/provider ready;
- native item/equip valid;
- custom prototype instantiated;
- authoring identity correlated;
- owner starts mesh/material loads;
- handles succeed;
- graphics IDs non-zero;
- loading becomes spawned;
- custom weapon visible;
- no old native duplicate;
- no invasive patch used.

---

# 122. Test matrix — repeated lifecycle

Recommended bounded repetition:

- multiple equip/unequip cycles;
- identical expected owner acquisition/release cardinality;
- no counter underflow;
- no retained final handle;
- no retained final graphics ID;
- no stale spawned entity.

One cycle is insufficient for lifetime claims.

---

# 123. Test matrix — FPP/TPP transition

Repeat perspective changes.

Verify independently:

- identity mapping;
- visibility;
- layer/filter policy;
- shadows;
- transform;
- no duplicate owners;
- no leaked perspective-specific state.

---

# 124. Test matrix — preview

Open and close preview repeatedly.

Verify:

- correct preview presentation;
- exact preview owner;
- resource sharing if assets are already loaded;
- cleanup after close;
- no mutation of FPP/TPP entity policy.

---

# 125. Test matrix — swap

Swap between:

- custom and native;
- native and custom;
- custom A and custom B where supported.

Verify overlapping counters and final owners.

---

# 126. Test matrix — scene transition

Use a controlled copied save/environment.

Verify scene ownership and final cleanup.

No scene-lifetime entity should remain after the declared stop point unless another valid owner exists.

---

# 127. Test matrix — duplicate request guard

Trigger the same custom presentation request twice.

Verify:

- no double first-owner acquisition;
- no duplicate authoring registration;
- no duplicate presentation;
- deterministic idempotency result.

---

# 128. Test matrix — failed key

Use one intentionally invalid custom resource key.

Verify:

- handle failure;
- no manual repair;
- no spawned state;
- useful diagnostic;
- clean later retry/process state according to policy.

---

# 129. Test matrix — last-owner release

This is mandatory for release-grade lifetime claims.

Verify:

~~~text
counter 1 → 0
→ graphics unregister
→ handle released/reset
→ no owner row retained
~~~

If any remains, cleanup is incomplete.

---

# 130. Test matrix — counter negative control

Deliberately exercise duplicate/unbalanced release detection in a disposable validation harness.

The test must fail closed rather than allowing wrap/underflow to become a valid-looking state.

---

# 131. Test matrix — package/provider teardown

After all Drake owners are gone:

- release provider lease;
- unmount/unload according to provider policy;
- remount if supported;
- confirm no duplicate asset identity;
- confirm next Drake acquire behaves like a clean first owner.

Do not run this test if the provider contract has not defined safe remount.

---

# 132. Test matrix — performance

A production importer should eventually compare:

- baseline frame behavior;
- owner transition cost;
- resource load latency;
- repeated lifecycle allocations;
- retained object counts;
- handle/resource counts.

Performance needs explicit thresholds.

“No obvious stutter” is not a performance pass.

---

# 133. Test matrix — compatibility

Revalidate when any material dependency changes:

- game build;
- `Awaken.ECS.dll`;
- Unity version;
- Entities Graphics version;
- Addressables version;
- resource-provider implementation;
- importer version;
- package format;
- native source prototype.

Private field offsets/member bodies must never be assumed stable across updates.

---

# 134. Test matrix — save/persistence

Drake itself is a presentation/resource system, but weapon persistence depends on it downstream.

After the custom item restores:

- native equip must rebuild;
- custom prototype/provider must resolve;
- Drake resources must reach spawned state;
- perspective presentation must be correct;
- cleanup must still work.

A save restoring the Item but showing the wrong weapon is not a complete persistence pass.

---

# 135. Diagnostics that matter

Useful Drake diagnostics include:

- authoring instance identity;
- linked owner;
- scene;
- perspective;
- ECS entity;
- mesh index;
- material index;
- runtime key;
- counter before/after;
- handle validity/status;
- graphics IDs;
- lifecycle tags before/after;
- whether original native method ran;
- whether arguments/result were mutated;
- owner-state write count.

Diagnostics should observe, not repair.

---

# 136. Strong anomaly conditions

Treat these as serious failures:

- binary fingerprint mismatch;
- invasive foreign patch on owner methods;
- custom private-loading path still active;
- probe skipping original;
- argument/result mutation;
- dropped diagnostic event;
- ambiguous entity mapping;
- spawned with zero graphics ID;
- spawned while still transitional;
- unexpected counter delta;
- counter underflow/wrap;
- active handle replaced while owners exist;
- failed-handle loading stall;
- final owner released but graphics ID remains;
- final owner released but handle remains;
- scene-lifetime entity retained;
- unknown perspective identity;
- probe exception.

---

# 137. What the public page can claim today

## Strong static/system claims

- Drake is FoA's rigid ECS rendering bridge.
- Ordinary Drake uses Addressables mesh/material resources.
- `DrakeMeshRenderer` and `DrakeLodGroup` are the main authoring bridge.
- Drake manager owns registration/resource coordination.
- resource ownership uses compact indices and reference counters.
- Entities Graphics receives registered mesh/material IDs.
- native loading/state systems own resource-ready lifecycle.
- scene lifetime has an explicit cleanup path.
- MergedDrake is the static bulk route.
- MergedDrake current reader/archive framing is mapped for the inspected build.
- native weapons enter Drake through the `ItemEquip → CharacterHandBase` presentation lifecycle.

## Strong failure-derived claims

- direct Unity renderer attachment is not equivalent to replacing native Drake presentation.
- private Drake loading-state replacement duplicates native ownership.
- manually publishing Drake lifecycle tags/components is invalid evidence of completion.
- broad unload suppression breaks owner symmetry.

---

# 138. What remains partial

- a clean, generic custom mesh/material provider route that lets arbitrary mod packages participate in normal Drake owner loading;
- release-grade FPP/TPP/preview proof for the generic importer;
- repeated lifecycle and scene-transition proof for the corrected owner-led provider architecture;
- package teardown/remount;
- current-build writer reproduction for MergedDrake;
- safe custom MergedDrake generation/injection.

---

# 139. What is not proven

- hot Drake restart;
- arbitrary direct ECS injection as a supported mod API;
- private loading-array mutation as safe;
- direct lifecycle-tag mutation as safe;
- generic custom MergedDrake installation;
- IL2CPP equivalence;
- cross-build private API stability;
- universal performance budgets.

---

# 140. Relationship to the weapon process

Read this page together with [Weapons](WEAPONS.md).

The weapon page owns the full gameplay chain:

~~~text
ItemTemplate
→ Item
→ ItemEquip
→ CharacterHandBase
→ combat/presentation
→ persistence
~~~

This Drake page owns the rigid rendering subsystem inside that chain:

~~~text
CharacterHandBase presentation
→ Drake authoring
→ Drake ECS/resource lifecycle
→ rendered mesh/material
→ Drake cleanup
~~~

Neither page replaces the other.

---

# 141. Relationship to Kandra

Drake and Kandra are separate systems.

Drake is the rigid-mesh ECS presentation path.

Kandra is the skinned-character/clothing path.

A prefab can contain or interact with both in different contexts, but their registration, resource layout, deformation, and teardown contracts are not interchangeable.

Do not apply Drake rules to custom armour merely because both end in GPU rendering.

---

# 142. Final mental model

If you remember only one diagram, use this:

~~~text
GAMEPLAY OWNER
Item / scene / runtime prefab
        │
        ▼
PRESENTATION OWNER
CharacterHandBase / scene object
        │
        ▼
DRAKE AUTHORING CONTRACT
DrakeMeshRenderer / DrakeLodGroup
        │
        ▼
DRAKE MANAGER
entity admission + resource identities
        │
        ▼
OWNER STATE MACHINE
LoadRequest → Loading → Spawned → Unload
        │
        ▼
RESOURCE OWNERSHIP
Addressables handles + refcounts
        │
        ▼
ENTITIES GRAPHICS
BatchMeshID + BatchMaterialID
        │
        ▼
RENDERED OUTPUT
        │
        ▼
OWNER-LED RELEASE
unregister graphics → release handle → scene/process cleanup
~~~

A successful custom Drake integration preserves that chain.

It does not skip to the middle because the middle happens to be patchable.

---

# 143. Current proof boundary

**System architecture:** STATIC_CONFIRMED / SOURCE_CONFIRMED for the scoped public and inspected current-build evidence.  
**Installed MergedDrake archive/reader framing:** STATIC_CONFIRMED for the inspected build.  
**Historical writer/build path:** SOURCE_CONFIRMED for the pinned Questline historical source; current-editor identity remains NOT_PROVEN.  
**Native weapon-to-Drake entry:** SOURCE_CONFIRMED.  
**Current owner-led custom rigid weapon direction:** SOURCE_CONFIRMED / PARTIAL.  
**Generic typed provider route:** NOT_PROVEN.  
**Corrected owner-led runtime matrix:** NOT_RUN as a generic public guarantee.  
**FPP/TPP/preview generic importer proof:** NOT_PROVEN.  
**Hot unload/remount:** NOT_PROVEN.  
**Custom MergedDrake generation:** NOT_PROVEN.  
**IL2CPP:** NOT_PROVEN.

The safe conclusion is:

> We now understand Drake well enough to know what a correct integration must preserve, which previous interventions were wrong, where a custom rigid-weapon presentation should enter, and exactly what still has to be proven before calling the importer complete.
