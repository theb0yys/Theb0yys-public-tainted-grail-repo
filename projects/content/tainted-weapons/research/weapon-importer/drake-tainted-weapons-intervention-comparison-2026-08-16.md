# DR-3 — Tainted Weapons Intervention Comparison Against the Drake Owner Contract — 2026-08-16

Document control:

- Status: `DR-3_STATIC_COMPARISON_COMPLETE`
- Authority state: `reviewed` for research and correction-planning use only
- Owner: Tainted Weapons / Weapon Importer
- Repository source baseline: `main@144923147a4751b50bf5d0fdce32e6c4af655e25`
- Tainted Weapons source: `mods/tainted-weapons/src/TaintedWeaponFramework.cs` blob `30e45368b269d5f68170a67354de8ef3bb56b2c3`
- Tainted Weapons plugin source: `mods/tainted-weapons/src/Plugin.cs` blob `f0ee8db72dca0c25c2019d835aa497729d267846`
- Drake owner binary: `Awaken.ECS.dll`
- Drake owner SHA-256: `b74a5bf79e93a75b11c5b70ace44d85e5a88650168077c6503dd10f4d2abd6f6`
- Drake owner MVID: `60d8ad80-f4c1-4edd-922f-3fe730daa98f`
- Evidence class: `E5 decompilation-static`
- Runtime observation: `NOT_RUN`
- Controlled validation: `NOT_RUN`
- Implementation authority: `NOT_GRANTED`
- Source-correction authority: `NOT_GRANTED`
- Save, compatibility, performance, and release authority: `NOT_GRANTED`
- Independent review: `NOT_RUN`
- Human promotion: `NOT_RUN`

## 1. Purpose

DR-3 compares every identified Drake-facing intervention in current Tainted Weapons source against the statically reconstructed `Awaken.ECS` owner contract.

This is a static comparison. It does not establish that any path executed in FoA, rendered correctly, persisted, recovered from failure, or remained compatible across lifecycle transitions.

The required distinction remains:

```text
source intervention identified
!= owner-contract compatibility proven at runtime
!= controlled validation passed
!= implementation or release permission
```

## 2. Owner contract used for comparison

The admitted owner binary establishes the following central static ownership model:

```text
Drake authoring registration
-> DrakeRendererLoadingSystem owns LoadRequest -> Loading transition
-> DrakeRendererManager.StartLoading owns mesh/material counter acquisition
-> DrakeRendererLoadingManager owns Addressables handles and reference counters
-> DrakeRendererComponentsManager owns pending-load bitmasks and graphics registration
-> DrakeRendererManager.TryGetMaterialMesh publishes completed graphics identities
-> DrakeRendererLoadingSystem owns render-component publication
-> DrakeRendererLoadingSystem owns Loading -> Spawned transition
-> DrakeRendererStateSystem owns ordinary LOD load/unload requests
-> DrakeRendererLoadingManager.UnloadMesh/UnloadMaterial own counter release
-> final owner release unregisters graphics identities and releases handles
-> scene-lifetime systems own orphan cleanup
```

Additional static constraints:

- mesh and material counters are `ushort` ownership counters;
- no zero-counter guard was found in the owner unload methods;
- no central terminal failed-load transition was found in the inspected completion polling;
- `DrakeRendererManager.Destroy()` is not a complete hot-restart teardown;
- registration and loading are not a broad transaction with rollback;
- the presentation command-buffer cache is frame scoped.

These constraints make private duplicate mutation of counters, pending bitmasks, loading tags, spawned tags, graphics components, or unload flow high risk.

## 3. Disposition vocabulary

Each intervention receives exactly one DR-3 disposition:

- `REQUIRED_BY_OWNER_CONTRACT` — belongs in the importer architecture because the owner contract requires this boundary or identity.
- `COMPATIBLE_BUT_UNNECESSARY` — does not contradict the owner contract, but is diagnostic, profile-specific, or not required in the production path.
- `INCORRECT` — duplicates, bypasses, or contradicts an owner-controlled transition.
- `UNPROVEN_RUNTIME_DEPENDENT` — statically plausible but requires direct runtime and controlled lifecycle evidence.
- `REMOVE_PENDING_VALIDATION` — should not remain in the production path unless a later controlled validation demonstrates a specific necessity that cannot be satisfied through the owner path.

## 4. Complete intervention ledger

| ID | Current Tainted Weapons intervention | Owner-contract comparison | DR-3 disposition | Required action before implementation |
|---|---|---|---|---|
| `TW-DR3-01` | Reserve immutable package/weapon identity plus unique mesh and material key namespaces. | Drake loading data and graphics registration require stable, collision-free keys. This does not mutate owner state. | `REQUIRED_BY_OWNER_CONTRACT` | Retain; bind keys to a typed importer provider and versioned package identity. |
| `TW-DR3-02` | Importer loads the package AssetBundle and resolves the declared presentation prefab. | A provider must own source assets before Drake can consume mesh/material references. | `REQUIRED_BY_OWNER_CONTRACT` | Retain the provider boundary; add schema, CRC, dependency, platform and asset-type validation separately. |
| `TW-DR3-03` | Clone the native `CharacterHandBase` prefab and preserve its `DrakeLodGroup`/`DrakeMeshRenderer` hierarchy. | Reusing native authoring and equip ownership is consistent with Drake registration; synthetic ECS entity construction is unnecessary. | `REQUIRED_BY_OWNER_CONTRACT` | Retain as the preferred presentation-authoring route. |
| `TW-DR3-04` | Rebind the one accepted native `DrakeMeshRenderer` through `DrakeMeshRenderer.Setup(...)`. | `Setup` is the owner-facing authoring boundary for mesh/material references, LOD mask and local-to-world offset. | `REQUIRED_BY_OWNER_CONTRACT` | Retain the authoring call; replace unsupported provider/handle plumbing beneath it. |
| `TW-DR3-05` | Reject native prototypes with zero or multiple Drake renderers. | This is a conservative importer profile, not a Drake requirement. | `COMPATIBLE_BUT_UNNECESSARY` | Keep for `rigid-melee-weapon/v1`; do not generalise it as a Drake invariant. |
| `TW-DR3-06` | Create synthetic completed `AssetReference` objects and prime `OperationHandle` through reflection. | Drake can consume an `AssetReference`, but reflection-primed completed operations are not established as a supported provider contract. | `UNPROVEN_RUNTIME_DEPENDENT` | Replace with a typed provider/resource-locator route or validate exact handle ownership, release and failure semantics. |
| `TW-DR3-07` | Prefix `ARAssetReference.LoadAsset<GameObject>` and return a completed operation for the synthetic runtime prototype address. | This redirects native equip loading without modifying Drake counters, but handle ownership and native lifecycle equivalence are not proven. | `UNPROVEN_RUNTIME_DEPENDENT` | Keep isolated from Drake internals; require load, cancel, destroy and repeated-equip validation. |
| `TW-DR3-08` | Fall back to the original native equipped source when custom prototype construction fails. | This is fail-closed from the custom presentation perspective and leaves native ownership intact. | `COMPATIBLE_BUT_UNNECESSARY` | Retain as fallback, but never count the custom package as presentation-proven on this route. |
| `TW-DR3-09` | Keep generic global `AssetReference`/`Addressables` mesh/material Harmony hooks disabled. | Global closed-generic interception is outside the Drake owner boundary and previously contaminated unrelated loads. | `REQUIRED_BY_OWNER_CONTRACT` | Keep disabled; use an importer-owned typed key/provider boundary only. |
| `TW-DR3-10` | Prefix and suppress `DrakeRendererLoadingManager.StartLoadingMesh(ushort)` for registered keys. | The owner method owns first-consumer handle creation and counter acquisition. Replacing it duplicates owner logic. | `INCORRECT` | Remove from the production path; supply custom assets through a supported provider while allowing owner loading code to run. |
| `TW-DR3-11` | Prefix and suppress `DrakeRendererLoadingManager.StartLoadingMaterial(ushort)` for registered keys. | Same conflict as mesh loading, including owner event and counter semantics. | `INCORRECT` | Remove from the production path. |
| `TW-DR3-12` | Reflect and write private `_meshLoadingData` / `_materialLoadingData` entry keys, handles and counters. | These lists are authoritative owner state. Direct writes bypass owner invariants and versioned encapsulation. | `INCORRECT` | Prohibit direct mutation; retain read-only inspection only in diagnostics. |
| `TW-DR3-13` | Invoke private `OnStartedLoadingMaterial` manually when injecting a material handle. | The event belongs to the owner’s first-load transition and is coupled to its state changes. | `INCORRECT` | Remove with the private loading replacement. |
| `TW-DR3-14` | From the ECS observation path, call `DrakeRendererManager.StartLoading(...)` when both counters appear zero. | The loading system owns when the load starts. An observation path can double-acquire or conceal an earlier transition failure. | `REMOVE_PENDING_VALIDATION` | Remove from production; instrument the owner request/start path instead. |
| `TW-DR3-15` | Reflect `_meshesToLoad` and `_materialsToLoad` and raise private `UnsafeBitmask` bits. | Pending-load bitmasks are owned by `DrakeRendererComponentsManager`. Direct arming fabricates owner work. | `INCORRECT` | Remove. |
| `TW-DR3-16` | Call `DrakeRendererComponentsManager.UpdateLoadings()` directly from a weapon presentation observer. | Owner polling is scheduled in the manager/player-loop contract. Direct calls alter timing and reentrancy. | `INCORRECT` | Remove; observe the scheduled owner update. |
| `TW-DR3-17` | Poll `DrakeRendererManager.TryGetMaterialMesh(...)` for readiness. | Read-only use of the owner query is compatible when it does not publish or mutate completion state. | `COMPATIBLE_BUT_UNNECESSARY` | Retain only as bounded instrumentation; do not use it to force completion. |
| `TW-DR3-18` | Directly add or replace `MaterialMeshInfo`, `MipmapsMaterialComponent` and `UVDistributionMetricComponent`. | `DrakeRendererLoadingSystem` owns publication of these components after owner completion. | `INCORRECT` | Remove; wait for the native system transition. |
| `TW-DR3-19` | Add `DrakeRendererSpawnedTag` and remove `LoadRequest`, `Loading` and `UnloadRequest` tags manually. | The owner systems use these tags as the lifecycle state machine. Manual mutation fabricates completion and erases pending owner work. | `INCORRECT` | Remove. |
| `TW-DR3-20` | Add `DrakeRendererManualTag` to completed registered renderers. | The tag changes state-system/LOD participation. It may be appropriate for bounded FPP or preview ownership but is not proven. | `UNPROVEN_RUNTIME_DEPENDENT` | Validate separately for FPP, TPP and preview; do not apply globally. |
| `TW-DR3-21` | Mark importer registry entries as retained after apparent readiness. | Provider-level asset retention is compatible, but it must remain separate from Drake counter and unload ownership. | `COMPATIBLE_BUT_UNNECESSARY` | Keep only as a provider ledger; it must not suppress owner release. |
| `TW-DR3-22` | Prefix `UnloadMesh` / `UnloadMaterial` and skip every unload for retained registered keys. | Owner counters require one release per acquired owner. Skipping valid unloads prevents counter symmetry, graphics unregister and handle release. | `INCORRECT` | Remove broad retention suppression. |
| `TW-DR3-23` | Block a registered-key unload when the observed owner counter is already zero. | This protects against the statically observed owner underflow hazard, but it is a defensive diagnostic rather than normal lifecycle ownership. | `COMPATIBLE_BUT_UNNECESSARY` | Keep only as a fail-stop safety assertion with an error receipt; investigate the duplicate release rather than silently continuing. |
| `TW-DR3-24` | Prefix/postfix unload methods to capture counter-before/counter-after evidence while otherwise allowing the original. | Read-only lifecycle instrumentation is compatible. | `COMPATIBLE_BUT_UNNECESSARY` | Retain only in diagnostics or validation builds. |
| `TW-DR3-25` | Reflect loading arrays to snapshot runtime key, counter, handle-valid and loaded state. | Read-only inspection does not alter the owner contract. | `COMPATIBLE_BUT_UNNECESSARY` | Retain for environment-bound evidence; treat reflection failure as an instrumentation failure. |
| `TW-DR3-26` | On plugin/framework disposal, destroy prototypes, release synthetic handles and clear the registry without first proving Drake entity/graphics unregister. | Owner graphics IDs and counters can outlive these provider objects; the order is not statically safe. | `REMOVE_PENDING_VALIDATION` | Define and validate an explicit owner-led teardown sequence or adopt process-session immutability with no hot unload claim. |
| `TW-DR3-27` | Call `AssetBundle.Unload(false)` while detached mesh/material objects may still be registered with Drake. | Unity allows detached objects to survive, but ownership, duplication and later reload behaviour are not reconciled with Drake IDs/counters. | `REMOVE_PENDING_VALIDATION` | Do not claim safe unload/remount until E7 lifecycle and duplicate-identity tests pass. |
| `TW-DR3-28` | Force inactive authoring and spawned presentation hierarchy nodes active. | Drake does not require the importer to override native equip hide/show or perspective ownership. | `REMOVE_PENDING_VALIDATION` | Remove from normal presentation; use it only in a bounded diagnostic experiment if needed. |
| `TW-DR3-29` | Add or update `LinkedTransformLocalToWorldOffsetComponent` from the native authoring offset or stored prototype offset. | This uses an owner-defined component and may be a valid presentation input, but exact update timing and ownership are runtime-sensitive. | `UNPROVEN_RUNTIME_DEPENDENT` | Validate against native FPP, TPP and preview transforms without direct downstream transform writes. |
| `TW-DR3-30` | Directly write ECS `LocalToWorld` and `WorldRenderBounds` to reposition the renderer. | Transform and bounds systems own these derived components. Direct writes can be overwritten or conflict with culling/update systems. | `INCORRECT` | Remove; express persistent placement through the owner input component or authoring transform. |
| `TW-DR3-31` | Mutate Unity layers and ECS `RenderFilterSettings` for registered equipped FPP renderers. | This is presentation policy outside the central loading contract and requires exact owner/camera evidence. | `UNPROVEN_RUNTIME_DEPENDENT` | Validate FPP-only ownership, layer source, camera transitions and negative TPP controls. |
| `TW-DR3-32` | Capture a visible vanilla FPP contract and move custom bounds to a camera-derived target. | The comparator is useful, but applying camera-local placement is not a Drake owner requirement and can be state/camera specific. | `UNPROVEN_RUNTIME_DEPENDENT` | Keep capture read-only; validate any applied placement separately with negative and lifecycle controls. |
| `TW-DR3-33` | Mutate preview Unity layers, rendering masks and ECS `RenderFilterSettings` to match `CustomHeroClothes`. | This may be correct for the preview clone, but route identity and lifecycle are not statically proven. | `UNPROVEN_RUNTIME_DEPENDENT` | Validate only at the exact native preview-owner boundary; exclude FPP/TPP entities. |
| `TW-DR3-34` | Reflect `LinkedEntitiesAccess._linkedEntities` to identify native-owned ECS entities. | Read-only ownership inspection is compatible but private and version-sensitive. | `COMPATIBLE_BUT_UNNECESSARY` | Retain as diagnostic instrumentation only. |
| `TW-DR3-35` | Read Drake material references and already-loaded materials in the default-off material probe. | The probe is read-only and does not alter owner state. | `COMPATIBLE_BUT_UNNECESSARY` | Retain as bounded evidence tooling; it proves only the captured state. |
| `TW-DR3-36` | Report `PresentationReady` when mandatory Harmony/reflection surfaces are present. | Hook availability does not establish that owner loading, graphics publication, presentation, teardown or failure handling works. | `INCORRECT` | Rename to a structural/hook-availability state; reserve presentation readiness for E7 proof. |
| `TW-DR3-37` | Produce lifecycle snapshots and event receipts from provider, prototype and Drake counters. | Instrumentation structure is compatible, but generated rows are not controlled validation by themselves. | `COMPATIBLE_BUT_UNNECESSARY` | Retain as DR-4/DR-5 instrumentation; require predeclared controls and observed results. |
| `TW-DR3-38` | Store the runtime prototype under a `DontDestroyOnLoad` process-session root. | This is consistent with the apparent process-lifetime manager, but scene, equip and teardown interactions remain unproven. | `UNPROVEN_RUNTIME_DEPENDENT` | Validate scene changes, repeated equip, package absence and process shutdown. |
| `TW-DR3-39` | Capture visible vanilla FPP layer/bounds/filter state as a read-only comparator. | A bounded comparator does not interfere with Drake and improves differential validation. | `COMPATIBLE_BUT_UNNECESSARY` | Retain as evidence tooling; do not automatically promote captured values into mutation policy. |
| `TW-DR3-40` | Inspect `SystemRelatedLifeTime<DrakeRendererManager>` and scene-lifetime component presence. | Read-only lifetime inspection is compatible and useful for confirming owner cleanup. | `COMPATIBLE_BUT_UNNECESSARY` | Retain in diagnostics; validate actual scene teardown ordering at runtime. |

## 5. Disposition totals

```text
REQUIRED_BY_OWNER_CONTRACT   5
COMPATIBLE_BUT_UNNECESSARY  12
INCORRECT                   11
UNPROVEN_RUNTIME_DEPENDENT   8
REMOVE_PENDING_VALIDATION    4
TOTAL                       40
```

## 6. Principal findings

### 6.1 The retained architecture is narrower than the current implementation

The defensible static architecture is:

```text
importer package/provider identity
-> native CharacterHandBase prototype clone
-> DrakeMeshRenderer.Setup with provider-owned references
-> vanilla equip/authoring registration
-> owner loading system
-> owner manager/components completion
-> owner loading system Spawned transition
-> owner state/unload/scene teardown
```

Tainted Weapons should not independently own a second loading state machine.

### 6.2 Four current intervention clusters contradict the owner contract

The following clusters are not merely unproved; they duplicate owner-controlled state:

1. `StartLoadingMesh` / `StartLoadingMaterial` Harmony replacements and private counter/handle writes.
2. Private component-manager pending-bit manipulation and direct `UpdateLoadings()`.
3. Manual publication of graphics components and lifecycle tags.
4. Retained-key suppression of valid owner unloads.

These clusters must not be used as the basis for implementation or readiness claims.

### 6.3 The provider problem remains real

Removing the private loading bridge does not remove the need to supply custom meshes and materials. It changes the required solution:

```text
unsupported private Drake state injection
->
typed importer provider / Addressables resource-locator integration
->
normal Drake owner loading and release
```

The provider must preserve:

- stable key identity;
- exact asset type;
- first-owner/last-owner semantics;
- valid handle ownership;
- failure visibility;
- package and bundle lifetime;
- release symmetry.

### 6.4 Presentation repair is a separate lane

Layer, render-filter, linked-offset, placement and preview interventions are not validated by the central Drake loading contract. Each requires its own FPP, TPP or preview evidence.

No result in this DR-3 comparison establishes a visible or correct weapon.

### 6.5 Current readiness wording is too broad

Static hook/reflection availability can support:

```text
STRUCTURAL_SURFACE_AVAILABLE
```

It cannot support:

```text
PRESENTATION_READY
IMPORTER_READY
```

without controlled runtime, teardown and negative-path validation.

## 7. Required correction boundary

DR-3 supports correction planning only. The smallest defensible correction boundary is:

1. preserve identity, package provider, native prototype clone and `DrakeMeshRenderer.Setup`;
2. introduce or select a supported typed asset-provider route;
3. remove private loading-list writes and `StartLoading*` replacements;
4. remove private component bitmask mutation and direct polling execution;
5. remove manual graphics-component and lifecycle-tag completion;
6. remove retained-key unload suppression;
7. retain read-only counter/handle instrumentation in a diagnostic lane;
8. split structural capability from runtime readiness;
9. place FPP, TPP and preview mutations behind separate evidence gates;
10. validate owner-led load, completion, unload and scene teardown before implementation promotion.

No source correction is authorised by this report.

## 8. Runtime and validation boundary

All runtime conclusions remain:

```text
plugin load                               NOT_RUN
custom key resolution                     NOT_RUN
owner StartLoading execution              NOT_RUN
counter increment symmetry                NOT_RUN
handle ownership                          NOT_RUN
graphics ID creation                      NOT_RUN
Loading -> Spawned transition             NOT_RUN
FPP presentation                          NOT_RUN
TPP presentation                          NOT_RUN
inventory preview                         NOT_RUN
weapon swap                               NOT_RUN
repeated equip/unequip                     NOT_RUN
scene transition                          NOT_RUN
failure and partial-load recovery          NOT_RUN
last-owner unload                          NOT_RUN
counter underflow negative control         NOT_RUN
graphics unregister                       NOT_RUN
bundle/provider teardown                   NOT_RUN
performance                               NOT_RUN
compatibility                             NOT_RUN
save/persistence                          NOT_RUN
```

Static findings must not be rewritten as observed runtime failures or successes.

## 9. DR-3 result

```text
DR-3 intervention inventory             PASSED
DR-3 owner-contract comparison          PASSED
DR-3 static dispositions                PASSED
runtime conclusions                     NOT_RUN
controlled validation                   NOT_RUN
independent review                      NOT_RUN
human promotion                         NOT_RUN
implementation permission               NOT_GRANTED
```

The output is suitable for correction planning and DR-4 instrumentation design only.
