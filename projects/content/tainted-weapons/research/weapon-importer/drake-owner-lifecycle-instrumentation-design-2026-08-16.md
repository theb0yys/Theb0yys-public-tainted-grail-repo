# DR-4 — Environment-Bound Owner-Led Drake Lifecycle Instrumentation Design — 2026-08-16

Document control:

- Status: `DR-4_INSTRUMENTATION_DESIGN_REVIEWED`
- Authority state: `reviewed` for research, instrumentation-planning and DR-5-planning use only
- Owner: Tainted Weapons / Weapon Importer
- Canonical branch: `tainted-weapons`
- Repository source baseline inspected: `main@675eed5fa26b6c446b90ed573da53a9787edf9f4`
- Tainted Weapons source version: `0.3.34`
- Tainted Weapons framework blob: `30e45368b269d5f68170a67354de8ef3bb56b2c3`
- Tainted Weapons plugin blob: `f0ee8db72dca0c25c2019d835aa497729d267846`
- Drake owner binary: `Awaken.ECS.dll`
- Drake owner SHA-256: `b74a5bf79e93a75b11c5b70ace44d85e5a88650168077c6503dd10f4d2abd6f6`
- Drake owner MVID: `60d8ad80-f4c1-4edd-922f-3fe730daa98f`
- Static dependency manifest aggregate: `bac9e000268ee5f59a7f6a51a87bb04ad323bd6589e1bdcac210ef64b4a2216e`
- Evidence basis: current repository source plus `E5` decompilation-static evidence
- Exact installed-build association: `PARTIAL`
- Static hook-manifest verification: `PASSED_LOCAL_E5`
- Instrumentation implementation: `NOT_RUN`
- FoA runtime capture: `NOT_RUN`
- Controlled validation: `NOT_RUN`
- Independent review: `NOT_RUN`
- Human promotion: `NOT_RUN`
- Source-correction authority: `NOT_GRANTED`
- Instrumentation-implementation authority: `NOT_GRANTED`
- Runtime-mutation authority: `NOT_GRANTED`

Companion artifacts:

- [Machine instrumentation contract](drake-owner-lifecycle-instrumentation-contract-v1.json)
- [Static dependency identity manifest](drake-owner-static-dependency-manifest-v1.json)
- [Research review record](../../research/reviews/weapon-importer-drake-owner-lifecycle-instrumentation-design-2026-08-16.review.json)

## 1. Purpose

DR-4 defines the exact observation architecture required to measure the **owner-led** Drake renderer lifecycle without reproducing the private lifecycle interventions identified by DR-3.

The observed lifecycle is:

```text
manager creation and initialization
-> authoring input
-> native Drake registration
-> LoadRequest / owner acquisition
-> mesh/material counter and Addressables handle lifecycle
-> manager completion polling
-> graphics-ID publication
-> Loading -> Spawned transition
-> perspective-specific active ownership
-> UnloadRequest / owner release
-> final graphics-ID and handle release
-> scene-lifetime cleanup
-> process shutdown marker
```

This artifact designs the probe, correlation model, evidence packet and later DR-5 matrix. It does not implement the probe, execute FoA, alter production Tainted Weapons source, or claim that a runtime transition occurred.

```text
static target identified
!= passive instrumentation implemented
!= runtime event captured
!= controlled validation passed
!= production correction authorised
```

## 2. Corrections incorporated

This reviewed revision resolves the defects found in the initial DR-4 draft:

1. Every hook row now records the actual **IL code size** hashed by the manifest, not a different method-body length metric.
2. `DR4-H04` is consistently `postfix` only in Markdown and the machine contract.
3. `DrakeRendererManager.Create()` and `Initialize()` are added as passive bootstrap markers.
4. The aggregate dependency fingerprint now has a durable manifest and reproducible canonicalization rule.
5. A claim-level research review record now governs the planning-only authority state.
6. The weapon-importer owner README routes this design and its companion contracts.
7. The canonical branch was reconciled with current `main` before correction.
8. The implementation planning gate is recorded explicitly as `RESEARCH_REQUIRED`; no implementation status is implied.

## 3. DR-3 input and correction boundary

DR-3 established this defensible production direction:

```text
stable importer identity and typed provider
-> native CharacterHandBase prototype clone
-> DrakeMeshRenderer.Setup
-> vanilla equip / authoring registration
-> Drake-owned loading
-> Drake-owned graphics completion
-> Drake-owned Spawned transition
-> Drake-owned unload and scene cleanup
```

DR-4 therefore observes and never reproduces these owner-conflicting interventions:

- replacement or suppression of `StartLoadingMesh` or `StartLoadingMaterial`;
- private loading-entry key, handle or counter writes;
- private pending-bitmask writes;
- direct `UpdateLoadings()` invocation;
- manual publication of `MaterialMeshInfo`, mipmap/UV components or Drake lifecycle tags;
- broad retained-key suppression of owner unload;
- direct writes to derived `LocalToWorld` or `WorldRenderBounds`;
- readiness claims derived only from hook/reflection availability.

## 4. Contamination gate

Current Tainted Weapons `0.3.34` cannot serve as an uncontaminated owner-led custom-weapon subject while its private Drake lifecycle interventions are active.

```text
vanilla owner baseline with Tainted Weapons disabled
    ELIGIBLE_AFTER_PROBE_IMPLEMENTATION_AND_ENVIRONMENT_ADMISSION

custom run with current private intervention path active
    CONTAMINATED_NOT_USABLE_FOR_OWNER_LED_CLAIM

custom run with separately authorised provider-only/observation-only subject
    ELIGIBLE_AFTER_PROVIDER_DECISION_IMPLEMENTATION_AND_PATCH_INVENTORY_PASS
```

Before arming, the future probe must inspect Harmony patch ownership for every core target. Any non-probe patch capable of skipping, replacing or mutating owner lifecycle work invalidates the run.

An anomaly is evidence. The probe must never repair the anomaly during the same run.

## 5. Environment admission

Every run binds to one immutable environment packet containing at minimum:

- game distribution, build ID and executable fingerprint;
- Unity version and `UnityPlayer.dll` fingerprint;
- Mono or IL2CPP track;
- `TG.Main.dll`, `Awaken.ECS.dll`, `Awaken.Utility.dll` and `Awaken.PackageUtilities.dll` hashes and MVIDs;
- Unity Entities, Entities Graphics, Collections, Mathematics, Transforms, CoreModule, Addressables and ResourceManager hashes and MVIDs;
- BepInEx and Harmony versions and hashes;
- exact probe source commit, built DLL hash and configuration;
- exact Tainted Weapons source commit and deployed DLL hash, or an explicit disabled state;
- package manifest and AssetBundle SHA-256/CRC for custom scenarios;
- save-copy identity, scene, camera/perspective, world state and DLC set;
- complete loaded-plugin and Harmony patch inventories;
- capture date, host OS, CPU/GPU and graphics API.

The durable candidate set is recorded in `drake-owner-static-dependency-manifest-v1.json`. Its canonical assembly-record aggregate is:

```text
sha256:bac9e000268ee5f59a7f6a51a87bb04ad323bd6589e1bdcac210ef64b4a2216e
```

The candidate set is internally coherent for static analysis. It does **not** establish that every supplied binary came from one current installed folder. A missing or mismatched material fingerprint invalidates a future run before hooks attach.

## 6. Probe architecture

### 6.1 Deployment

The probe is a separate, default-off diagnostic assembly. It is not an always-on production importer feature.

Required defaults:

```text
enabled = false
autoStart = false
writeFiles = false
captureVanilla = false
captureCustom = false
maxBufferedEvents = bounded
samplingFilter = explicit identities only
```

Arming requires one explicit request ID and one declared scenario ID. The request expires after one process or the declared stop condition.

### 6.2 Hook discipline

Only passive Harmony prefixes/postfixes are permitted.

```text
original executes exactly once
prefix never suppresses original
arguments are not mutated
result is not mutated
exceptions are not swallowed or replaced
no owner lifecycle method is invoked by the probe
no private owner state is written
no ECS component/tag/state is written
no Addressables handle is created, released or completed
no synchronous disk I/O occurs inside an owner hook
```

Transpilers, finalizers, reverse patches and private-field writes are prohibited.

### 6.3 Buffered capture

Hooks append bounded scalar events to a preallocated or bounded in-memory queue. Serialization and file I/O occur outside owner methods on the probe flush boundary.

A dropped event invalidates the run. The probe emits one terminal `EVENT_DROPPED` marker and stops accepting evidence rather than silently sampling.

### 6.4 No active waiting or gameplay driving

The probe must not call:

- `WaitForCompletion`;
- `JobHandle.Complete`;
- Drake `StartLoading`, `UpdateLoadings`, `Unload` or scene lifecycle methods;
- `TryGetMaterialMesh` as a completion mechanism;
- `Addressables.Load*` or `Addressables.Release`;
- entity-command-buffer playback;
- scene load/unload APIs;
- equip, inventory, preview or camera-control APIs.

The operator or predeclared scenario drives gameplay. The probe only observes.

## 7. Exact hook manifest

The future implementation must bind by exact assembly fingerprint, declaring type, method signature, metadata token, RVA, IL code size and IL SHA-256. Any mismatch blocks attachment.

`DR4-H19` and `DR4-H20` were added after the original H01-H18 numbering to preserve the established hook IDs while closing the manager-bootstrap gap. `lifecycle_order` in the machine contract is authoritative for ordering.

| ID | Role | Target/signature | Token | RVA | IL code bytes | IL SHA-256 | Patch |
|---|---|---|---:|---:|---:|---|---|
| `DR4-H19` | manager-create-boundary | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererManager.void Create()` | `0x060004FB` | `0x00017A6D` | 53 | `7ae3f88d919ffb3291e56c243753d40ed25d07a7eabd7548861fe80d8b5af2fd` | `prefix+postfix` |
| `DR4-H20` | manager-initialize-boundary | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererManager.void Initialize()` | `0x060004FC` | `0x00017AA3` | 46 | `d40f89b81808b901c9ae6dac978a3371e53650008eaf2876f791ecc4d34b9bad` | `prefix+postfix` |
| `DR4-H01` | authoring-input | `Awaken.ECS.DrakeRenderer.Authoring.DrakeMeshRenderer.void Setup(UnityEngine.MeshRenderer, UnityEngine.MeshFilter, Awaken.ECS.DrakeRenderer.Authoring.DrakeLodGroup, int, Unity.Mathematics.float4x4, UnityEngine.AddressableAssets.AssetReference, UnityEngine.AddressableAssets.AssetReference[])` | `0x0600049D` | `0x00015C34` | 225 | `a89e68d2d147fc7f7265b2a8daf21cad7869bcf8e073bc0271d7577c1dad5d0e` | `prefix+postfix` |
| `DR4-H02` | registration-boundary | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererManager.void Register(Awaken.ECS.DrakeRenderer.Authoring.DrakeMeshRenderer, UnityEngine.SceneManagement.Scene, Unity.Entities.Entity, ref Awaken.ECS.Authoring.LodGroupSerializableData, System.Nullable`1<bool>)` | `0x06000505` | `0x00017E5C` | 58 | `cae603d65d5ad579e77c3d1be22b9706b7c0445110687331351364cfc10e382d` | `prefix+postfix` |
| `DR4-H03` | resource-index-registration | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererComponentsManager.void Register(ref Awaken.ECS.DrakeRenderer.Components.DrakeMeshMaterialComponent)` | `0x060004C7` | `0x00016ABC` | 137 | `f8a6a128ce4d07c376ac26adb73aa85da92dd0ad317bdf0b780d072f5695abfa` | `prefix+postfix` |
| `DR4-H04` | lod-request-schedule-marker | `Awaken.ECS.DrakeRenderer.Systems.DrakeRendererStateSystem.void OnUpdate()` | `0x06000414` | `0x00013454` | 350 | `54b351642a5c111808fd71f5ba04e91f1f46f523443cd51a9f8210421d793bfd` | `postfix` |
| `DR4-H05` | entity-lifecycle-boundary | `Awaken.ECS.DrakeRenderer.Systems.DrakeRendererLoadingSystem.void OnUpdate()` | `0x060003D6` | `0x000126CC` | 248 | `95e3b8aad2693c250b471cb88490b7fa29912e08890e96cc91f3d89046a7eeac` | `prefix+postfix` |
| `DR4-H06` | owner-acquire-boundary | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererManager.void StartLoading(ref Awaken.ECS.DrakeRenderer.Components.DrakeMeshMaterialComponent)` | `0x06000507` | `0x000182A8` | 13 | `4e91efb6bd6dd211c2e7983f238e1a2da72b98c46336ebf0223327edf8aa8d70` | `prefix+postfix` |
| `DR4-H07` | mesh-counter-acquire | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererLoadingManager.bool StartLoadingMesh(ushort)` | `0x060004DF` | `0x00017454` | 73 | `bde28d10d437fbe44a40b076261c9faa6bea16334a317d7784cabcaf2fe50543` | `prefix+postfix` |
| `DR4-H08` | material-counter-acquire | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererLoadingManager.bool StartLoadingMaterial(ushort)` | `0x060004E0` | `0x000174AC` | 96 | `ee2cc64ffd179ed8a0079506a957282ad8bafffb628c5249e80eb4c8338eab14` | `prefix+postfix` |
| `DR4-H09` | manager-poll-boundary | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererManager.void InitializationUpdate()` | `0x06000500` | `0x00017B12` | 11 | `5c1aebbe70cbf13f0dfe29318e186c40a7ad4aab5c7b94eed990367574da76c5` | `prefix+postfix` |
| `DR4-H10` | graphics-registration-boundary | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererComponentsManager.void UpdateLoadings()` | `0x060004C6` | `0x00016988` | 294 | `b20ff46c78a174fd07068fe192ede10a0c9f91c9c7b18e559ff46404a02abc2c` | `prefix+postfix` |
| `DR4-H16` | presentation-sync-boundary | `Awaken.ECS.DrakeRenderer.Systems.DrakeRendererSyncPoint.void OnUpdate()` | `0x0600041D` | `0x000137D2` | 16 | `54f69ca282bfb8adbb638d35435cef8b010b20273fba1b2eac1c82e9049d250c` | `postfix` |
| `DR4-H17` | managed-readiness-query-observer | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererComponentsManager.bool TryGetMaterialMesh(ref Awaken.ECS.DrakeRenderer.Components.DrakeMeshMaterialComponent, ref Unity.Rendering.MaterialMeshInfo, ref Awaken.ECS.Mipmaps.Components.MipmapsMaterialComponent, ref Awaken.ECS.Mipmaps.Components.UVDistributionMetricComponent)` | `0x060004C8` | `0x00016B54` | 161 | `4a8a7190d33a7ceae23a55dbd90860ca0d4f4fd1e7116fd9637a8ba63e30ceeb` | `prefix+postfix` |
| `DR4-H11` | owner-release-boundary | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererManager.void Unload(Awaken.ECS.DrakeRenderer.Components.DrakeMeshMaterialComponent, bool)` | `0x06000509` | `0x000182C8` | 15 | `b2a663bda38df8bfaf8916b65bcf31153b0285fad49d3c9b7cc1df4c341e1cee` | `prefix+postfix` |
| `DR4-H12` | mesh-counter-release | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererLoadingManager.void UnloadMesh(ushort)` | `0x060004E2` | `0x00017530` | 72 | `30870f4b766568d3dde90ce5a3683e7edce554f0fae4167be8db6045f03d9549` | `prefix+postfix` |
| `DR4-H13` | material-counter-release | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererLoadingManager.void UnloadMaterial(ushort)` | `0x060004E5` | `0x000175B0` | 95 | `6e48f7a0eece8242434e51925608272de0478740bf8ed6da9c538dd5b339c907` | `prefix+postfix` |
| `DR4-H15` | scene-unload-trigger | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererManager.void OnSceneUnloaded(UnityEngine.SceneManagement.Scene)` | `0x060004FF` | `0x00017AFF` | 18 | `c53575af6dd25ba14ce8f29d98a450ae1cb5d7f76233074e3236b29ce7fa53fc` | `prefix+postfix` |
| `DR4-H14` | scene-orphan-cleanup-boundary | `Awaken.ECS.DrakeRenderer.Systems.DrakeRendererSceneManagementSystem.void OnUpdate()` | `0x0600040C` | `0x00013018` | 131 | `bac41816f0de43c07157c9477b61de3c954a38466d571d758950817868bec739` | `prefix+postfix` |
| `DR4-H18` | process-shutdown-boundary | `Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererManager.void Destroy()` | `0x060004FD` | `0x00017AD2` | 35 | `991def71c9537f0e7bf7da449a507d04761f51e99cbb5a3415b0f53e7d152f4e` | `prefix+postfix` |

### Hook-specific rules

- `H19` records the static manager-create invocation only. It must not call `DrakeRendererManager.Instance`.
- `H20` supplies the first instance-bound bootstrap marker through Harmony `__instance`; its postfix is the `DR4-W00` bootstrap window.
- `H04` is postfix-only and records only that the state system completed its scheduled update. It is not an immediate entity-tag proof.
- `H05` is the primary stable entity-state boundary after the inspected loading-system body has completed its own work.
- `H09` and `H10` observe manager-owned completion polling. The probe never calls either method.
- `H17` is a contamination/read-only-query marker, not a normal owner-completion step. A call from the current private Tainted Weapons path invalidates an owner-led custom claim.
- `H18` proves only that `Destroy()` was invoked. Static inspection does not establish complete hot-restart resource teardown.

## 8. Stable snapshot windows

| Window | Trigger | Read boundary | Limit |
|---|---|---|---|
| `DR4-W00` | `H20` postfix | manager instance identity and bootstrap marker | no broad ECS assertion |
| `DR4-W01` | `H05` postfix | tracked lifecycle tags, graphics components, linked ownership and referenced resource rows | primary load/unload/Spawned snapshot |
| `DR4-W02` | `H09` postfix | tracked counters, handles, loaded flags, graphics IDs and pending state | manager-poll snapshot |
| `DR4-W03` | `H14` postfix | tracked scene-lifetime entities and final resource rows | scene-cleanup snapshot |
| `DR4-W04` | `H16` postfix | frame marker and buffer-flush request only | no broad ECS assertion |

The probe never performs an unbounded world scan inside a hot owner method. It reads only the tracked set discovered from admitted authoring registration, linked ownership and referenced resource indices.

## 9. Correlation model

A mesh or material index is a shared resource identity, not a weapon-instance identity. Correlation combines:

- package/weapon identity when present;
- native template GUID/name;
- runtime prototype address or native source key;
- `DrakeMeshRenderer` and `CharacterHandBase` instance IDs and owner paths;
- scene handle/name;
- linked owner identity;
- ECS entity index/version;
- `DrakeMeshMaterialComponent` mesh/material/submesh indices;
- perspective classification.

One entity may share resource indices with another. The probe must not attribute every transition for one index to one weapon without matching event/ownership evidence.

Ambiguous correlation emits `ENTITY_MAPPING_AMBIGUOUS` and invalidates the affected claim. Guessing is forbidden.

## 10. Perspective separation

Every tracked view receives one classification:

```text
fpp
tpp
inventory-preview
world
unknown
```

Classification may use exact native owner types, `CharacterHandBase`/`CustomHeroClothes` ownership, native hero perspective state, scene/camera identity and inspected transform ancestry. String-name heuristics alone are leads, not proof.

`unknown` cannot pass FPP, TPP or preview gates. No value from one perspective may be copied into another; DR-4 is observation-only.

## 11. Event and packet contract

Events are UTF-8 JSON Lines with schema ID:

```text
foa-drake-owner-lifecycle-event-v1
```

Required core fields:

```text
schema_version
session_id
scenario_id
sequence
utc_time
unity_frame
realtime_seconds
thread_id
player_loop_phase
hook_id
event_kind
correlation_id
owner_identity
perspective
scene_handle
scene_name
authoring_instance_id
entity_index
entity_version
mesh_index
material_index
runtime_key
counter_before
counter_after
handle_valid
handle_done
handle_status
batch_mesh_id
batch_material_id
tags_before
tags_after
components_before
components_after
original_allowed
arguments_mutated
result_mutated
owner_state_writes
dropped_event_count
details
```

Every owner-hook event asserts:

```text
original_allowed = true
arguments_mutated = false
result_mutated = false
owner_state_writes = 0
```

Each run produces:

```text
environment.json
patch-inventory.json
events.jsonl
stable-snapshots.jsonl
summary.json
manifest.sha256
```

`summary.json` records terminal state, scenario/repetition, environment fingerprint, event/snapshot/anomaly/drop counts, tracked identities, first/final owner states, invariant results, limitations, operator interventions and packet hashes.

Dropped events, missing files, hash mismatch or an unhandled probe exception invalidate the packet.

## 12. Later DR-5 invariants

| ID | Required observation |
|---|---|
| `DR4-I01` | Manager `StartLoading` is followed by mesh and material owner acquisition for the tracked identity. |
| `DR4-I02` | `0 -> 1` starts one handle; `N > 0` increments without replacing an active handle. |
| `DR4-I03` | `LoadRequest` becomes `Loading` before any `Spawned` claim. |
| `DR4-I04` | Non-zero graphics identities appear only after successful owner handles. |
| `DR4-I05` | The owner loading system publishes render components and `Spawned`, then removes `Loading`. |
| `DR4-I06` | No accepted spawned entity retains a transitional tag or zero graphics identity. |
| `DR4-I07` | Every owner acquisition has exactly one matching release in the tested lifecycle. |
| `DR4-I08` | Final `1 -> 0` unregisters graphics IDs and releases/resets handles; non-final release retains them. |
| `DR4-I09` | No counter underflow, wrap or duplicate final release occurs. |
| `DR4-I10` | Scene unload removes tracked lifetime entities and leaves no tracked owner after the stop point. |
| `DR4-I11` | FPP, TPP and inventory preview are correlated and evaluated separately. |
| `DR4-I12` | Failed handles or loading stalls are reported without probe-driven repair. |

## 13. Stable anomaly codes

```text
BINARY_FINGERPRINT_MISMATCH
FOREIGN_INVASIVE_PATCH_PRESENT
CURRENT_TAINTED_WEAPONS_PRIVATE_PATH_ACTIVE
PROBE_ATTEMPTED_OWNER_MUTATION
ORIGINAL_SKIPPED
ARGUMENT_OR_RESULT_MUTATED
EVENT_DROPPED
ENTITY_MAPPING_AMBIGUOUS
SPAWNED_WITH_ZERO_GRAPHICS_ID
SPAWNED_WITH_TRANSITIONAL_TAG
COUNTER_DELTA_UNEXPECTED
COUNTER_UNDERFLOW_OR_WRAP
HANDLE_REPLACED_WITH_ACTIVE_OWNERS
FAILED_HANDLE_LOADING_STALL_CANDIDATE
LAST_OWNER_GRAPHICS_ID_RETAINED
LAST_OWNER_HANDLE_RETAINED
SCENE_LIFETIME_ENTITY_RETAINED
PERSPECTIVE_IDENTITY_UNKNOWN
PROBE_EXCEPTION
```

## 14. DR-5 matrix

| Scenario | Repetitions | Tainted Weapons state | Probe | Lane |
|---|---:|---|---|---|
| `DR5-V0` vanilla no-probe baseline | 2 | disabled | disabled | timing/control |
| `DR5-V1` vanilla non-interference pair | 2 | disabled | enabled | probe safety and owner baseline |
| `DR5-C0` custom owner-led single equip | 2 | provider-only/observation-only subject | enabled | acquisition and Spawned |
| `DR5-C1` repeated equip/unequip | 5 | provider-only/observation-only subject | enabled | counter/handle symmetry |
| `DR5-C2` FPP/TPP transition | 3 | provider-only/observation-only subject | enabled | perspective separation |
| `DR5-C3` preview open/close | 3 | provider-only/observation-only subject | enabled | preview ownership/teardown |
| `DR5-C4` weapon swap | 3 | provider-only/observation-only subject | enabled | overlapping lifecycle |
| `DR5-C5` scene transition | 2 | provider-only/observation-only subject | enabled | scene cleanup |
| `DR5-C6` duplicate request guard | 2 | provider-only/observation-only subject | enabled | no double acquisition |
| `DR5-C7` failed resource load | 2 | disposable failure fixture | enabled | failure observation only |
| `DR5-C8` process shutdown receipt | 2 | provider-only/observation-only subject | enabled | shutdown, not hot restart |

Each repetition uses a fresh process unless the scenario explicitly tests an in-process transition.

`V0` and `V1` must use the same environment, copied save, scene, camera route, weapon and operator script. Acceptance requires owner-consistent event order, identical acquisition/release cardinality and final cleanup, no drops/exceptions, and a separately approved performance threshold. DR-4 selects no threshold.

## 15. Result states and stop conditions

A run is `INVALID` when:

- an environment or method-body fingerprint is missing/mismatched;
- current owner-conflicting Tainted Weapons patches are active in an owner-led custom scenario;
- the probe skips an original, mutates arguments/results, writes owner state or calls owner lifecycle methods;
- an event is dropped;
- entity mapping is ambiguous;
- a paired run differs outside the declared probe state;
- an undeclared save, scene, package, perspective or manual intervention is used;
- the probe throws an unhandled exception;
- packet integrity fails.

A valid packet is `FAILED` when a declared invariant is not observed, `PARTIAL` when the scenario does not reach its stop point or an optional marker is absent, and `PASSED` only when every required marker/invariant passes all repetitions and independent validation accepts the packet.

## 16. No-game self-tests

Before FoA use, implementation must prove:

1. exact assembly/hash/MVID rejection;
2. exact type/signature/token/RVA/IL-size/IL-hash resolution;
3. refusal when invasive foreign patches are present;
4. all prefixes permit the original;
5. no argument/result mutation surface exists;
6. no owner lifecycle call exists in probe source/IL;
7. no ECS write API exists in probe source/IL;
8. bounded buffer and deterministic drop-stop;
9. event-schema serialization and monotonic sequence;
10. packet hashing/tamper detection;
11. scenario/request expiry;
12. default-off configuration.

These tests prove probe structure only.

## 17. Bounded implementation plan

Planning status:

```text
RESEARCH_REQUIRED
```

The planning output is intentionally split into two mutation scopes under the Tainted Weapons project branch.

### Scope A — passive probe

Proposed root:

```text
mods/tainted-weapons/drake-lifecycle-probe/
```

Planned paths:

```text
README.md
src/TaintedWeapons.DrakeLifecycleProbe.csproj
src/Plugin.cs
src/Admission/EnvironmentAdmission.cs
src/Admission/MethodBodyAdmission.cs
src/Admission/PatchInventoryGate.cs
src/Instrumentation/HookManifest.cs
src/Instrumentation/PassivePatches.cs
src/Instrumentation/TrackedIdentitySet.cs
src/Instrumentation/StableSnapshotReader.cs
src/Evidence/BoundedEventBuffer.cs
src/Evidence/EventRecord.cs
src/Evidence/PacketWriter.cs
src/Evidence/PacketIntegrity.cs
contracts/foa-drake-owner-lifecycle-event-v1.schema.json
contracts/foa-drake-owner-lifecycle-summary-v1.schema.json
tests/TaintedWeapons.DrakeLifecycleProbe.Tests.csproj
tests/AdmissionTests.cs
tests/NonMutationSurfaceTests.cs
tests/BufferAndPacketTests.cs
tools/Invoke-DrakeLifecycleProbeWindowsValidation.ps1
```

Build target: `netstandard2.1`, using `FoAGameRoot`, with deployment disabled unless an explicit validation command requests it.

Scope A may add passive prefixes/postfixes only. It may not reference production Tainted Weapons internals or mutate `mods/tainted-weapons/src/**`.

### Scope B — owner-led custom test subject

Proposed root:

```text
mods/tainted-weapons/owner-led-test-subject/
```

Planned paths:

```text
README.md
src/TaintedWeapons.OwnerLedTestSubject.csproj
src/Plugin.cs
src/FixtureManifest.cs
src/NativePrototypeAuthoring.cs
src/TypedAssetProvider.cs
src/EquippedReferenceBoundary.cs
src/SubjectReceipt.cs
tests/TaintedWeapons.OwnerLedTestSubject.Tests.csproj
tests/PackageAndIdentityTests.cs
tests/ForbiddenPatchInventoryTests.cs
```

Scope B is disposable validation infrastructure. It may:

- admit one exact rigid-melee fixture;
- clone the exact native `CharacterHandBase` prototype;
- call `DrakeMeshRenderer.Setup`;
- expose custom mesh/material identities through the selected typed provider;
- redirect only the bounded equipped prototype reference necessary for the subject.

It must not patch or call Drake loading, completion, counter, bitmask, tag, graphics, unload or scene-cleanup ownership surfaces.

### Blocking prerequisites

Implementation is not approved until all are satisfied:

1. **Provider route research:** select and statically validate a typed Addressables/ResourceManager locator/provider route, or another owner-compatible mechanism, for custom mesh/material keys.
2. **Environment admission:** capture current installed `TG.Main`, `Awaken.ECS`, BepInEx, Harmony and required Unity assembly identities from one target installation.
3. **Independent review:** independently review this corrected design, hook manifest, contamination gate and split plan.
4. **Explicit implementation authorization:** separately authorize Scope A and Scope B.
5. **Current-source revalidation:** confirm the Tainted Weapons source blobs and owner assembly fingerprints still match immediately before mutation.

Production Tainted Weapons correction, removal of private interventions, presentation changes, DR-5 execution, saves, compatibility, performance, consumer migration and release remain out of scope.

## 18. Evidence and authority result

```text
DR-4 corrected design                           PASSED_AS_REVIEWED_PLANNING
exact 20-hook owner manifest                    PASSED_LOCAL_E5
Markdown/machine hook consistency               PASSED_LOCAL
manager Create/Initialize boundary              INCLUDED_STATIC
durable dependency aggregate                    PASSED_LOCAL
contamination/non-interference rules            PASSED_AS_DESIGN
event and packet contract                       PASSED_AS_DESIGN
DR-5 matrix                                     PASSED_AS_PLANNING
implementation planning gate                    RESEARCH_REQUIRED

probe source                                    NOT_RUN
owner-led custom subject                        NOT_RUN
environment admission                           NOT_RUN
FoA runtime capture                             NOT_RUN
controlled validation                           NOT_RUN
production correction                           NOT_RUN
compatibility/performance/save/release           NOT_RUN

source-correction authority                     NOT_GRANTED
instrumentation-implementation authority         NOT_GRANTED
runtime-mutation authority                       NOT_GRANTED
```

## 19. Required next research transition

The exact next research task is to resolve the **typed mesh/material provider route** against the supplied `Unity.Addressables.dll`, `Unity.ResourceManager.dll`, `Awaken.ECS.dll` and current Tainted Weapons source, while preserving Drake’s own `StartLoading`, counter, completion and unload ownership.

That returned provider decision must identify the exact public API, locator/provider lifetime, key identity, handle acquisition/release semantics, failure path, duplicate-owner behaviour, target code paths and no-game/runtime proof required before Scope B can become `APPROVED_FOR_IMPLEMENTATION`.
