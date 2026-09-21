# Tainted Armour Production Pipeline

## Ownership

`mods/tainted-armour/` owns the importer, contracts, deformation stage, tests,
and future adapters. Tainted Framework, Dragon Knight, Unity commands, and
Kandra runtime code are not dependencies of the production core.

## Production flow

`ArmorImporter.Import` is the canonical entry point. It accepts one typed
`ArmorImportRequest`, invokes the framework-owned deformation stage, and returns
one typed `ArmorImportResult` containing the stage result and pipeline blockers.

The production core remains provider-neutral:

1. `UnityGeometrySnapshotAdapter`, owned by Tainted Armour, bakes renderer
   geometry and adapts Unity positions and submesh topology into the core
   geometry contracts.
2. `ArmorSourceContract` identifies the source mesh, material slot, rig,
   bind-pose, bone-weight, influence, and geometry counts.
3. `ArmorTargetContract` identifies the native target, variant, candidate-map
   state, and fingerprints.
4. `DeformationValidationRequest` carries baseline/posed geometry snapshots,
   evaluated triangle and vertex sets, and the metric policy.
5. `DeformationValidationStage` performs production control calibration,
   displacement measurement, collapsed-triangle classification, normalized
   normal-dot orientation classification, aggregation, and blocker generation.
6. `VisualRuntimeObservationRequest` is the optional A2K-T34 live-observer
   contract. When supplied, it enters `ArmorImporter.Import` through the normal
   result flow and can block before conversion. The diagnostic-tool transport
   builds this contract from live FoA body/equip observations through the
   established manual diagnostic dump route; it adds no Tainted Armour hotkey.
7. `ArmorImportResult` preserves candidate-map application, conversion,
   sidecars, runtime mutation, game writes, and release readiness as false.
8. `KandraWriterStage` is a separate writer slice for already-packed Kandra
   payload sections. It emits loose `.mdkandra`/`.ixkandra` files in the
   recovered `KandraMesh.ReadSerializedData` order and targets
   `modDirectory/Kandra/<Name>`. It is not a Unity-to-Kandra semantic
   converter and does not authorize archive mutation, runtime registration,
   item/equip/save mutation, or candidate-map application.
9. `KandraPackageValidationStage` is the no-write loose-package validation
   gate. It consumes the writer result plus a live `ArmorImporter.Import`
   result, then reads the loose files back to verify the recovered runtime
   seam, file presence, byte counts, hashes, writer false-boundary state, and
   retained live Kandra `IsRegistered`/`TryGetMeshMemory` proof. Passing this
   gate still leaves custom armour registration, conversion, candidate-map
   application, item/equip/save mutation, archive mutation, and game writes
   false.
10. `KandraRegistrationPreflightStage` is the no-write registration preflight.
    It compares the selected custom `KandraMesh` metadata contract
    (`modDirectory`, `name`, vertex/index/bind-pose/blend-shape counts, and
    payload layout version) against an accepted loose-package validation result
    before any registration call. Passing this preflight still leaves runtime
    registration, conversion, candidate-map application, item/equip/save
    mutation, archive mutation, and game writes false.
11. `KandraRegistrationCandidateStage` is the no-write candidate-object
    boundary. It consumes only an accepted preflight result and constructs the
    typed custom registration candidate request containing the `KandraMesh`
    metadata identity, validated loose package paths, byte counts, hashes, and
    layout version. The object records the live registration call contract as
    unproven and keeps invocation, conversion, candidate-map application,
    item/equip/save mutation, archive mutation, and game writes false.
12. `KandraRuntimeRegistrationDryRunStage` is the first guarded
    runtime-registration dry-run gate. It consumes an accepted candidate build
    result, the validated loose package result, and the live
    `KandraRendererManager.Register(KandraRenderer)` contract fingerprint. It
    rechecks the candidate/package identity, loose file presence, byte/hash
    identity, exact recovered method fingerprint, `Awaken.Kandra` assembly hash,
    required registration surface, and no-write diagnostic boundary. Passing
    this dry-run marks only the contract fingerprint and planning boundary as
    accepted. It still has no runtime invocation approval field and
    keeps `Register`, `CanRegister`, object creation/activation, conversion,
    candidate-map application, archive mutation, item/equip/save mutation, and
    game writes false.
13. `KandraRuntimeRegistrationInvocationStage` is the explicit runtime
    invocation approval gate after the accepted dry-run boundary. It requires a
    matching approval record pinned to the same dry-run request, loose package
    hashes, recovered `Register(KandraRenderer)` fingerprint, and
    `Awaken.Kandra` assembly hash before it will call a host-supplied invoker.
    The stage records the invoker's `Register` call marker plus
    `IsRegistered`/`TryGetMeshMemory` proof, but still refuses candidate-map
    application, conversion, item/equip/save mutation, native game-file writes,
    and non-registration downstream writes. The provider-neutral framework does
    not construct or activate FoA `KandraRenderer` objects itself.
14. `KandraRuntimeRegistrationHostProofStage` consumes a durable FoA
    host-invoker receipt as a typed framework proof artifact. The first accepted
    receipt is for `AvalonAwakenedProof/RealSkinnedTriangle` and proves only that
    the recovered host path invoked `Register(KandraRenderer)`, reached
    `IsRegistered=True`, reached `TryGetMeshMemory=True`, and kept
    non-registration downstream writes false.
15. `KandraTargetRuntimeRegistrationPlanStage` consumes accepted host proof, an
    accepted target-package dry run, and matching explicit approval. It refuses a
    target dry run that reuses the `RealSkinnedTriangle` proof identity or
    payload hashes, then constructs only the typed target invocation context. It
    does not call the host invoker, create or activate FoA objects, apply
    candidate maps, run conversion, mutate item/equip/save state, or write
    downstream state.
16. The FoA observer owns the no-write registration-contract diagnostic packet.
    Current static inspection of `Awaken.Kandra.dll` recovers the registration
    owner and method as `KandraRendererManager.Register(KandraRenderer)`, called
    by `KandraRenderer.OnEnable()`. `Register` queues the renderer and
    `FinalizeRegistration()` performs manager-chain readiness checks during the
    manager's EarlyUpdate path before marking the renderer fully registered. The
    diagnostic fingerprints this type/method/member surface by reflection and
    assembly hash, but it never invokes `Register`, `CanRegister`, object
    creation, activation, conversion, candidate-map application, archive
    mutation, item/equip/save mutation, or game writes.
17. `KandraSameMeshAbProofStage` is the controlled same-mesh Kandra A/B package
    builder. It consumes original runtime `.mdkandra` bytes, original
    `.ixkandra` indices, mesh counts, blend-shape names, and the live
    registration-contract fingerprint, then emits exactly four loose package
    variants under the normal `modDirectory/Kandra/<Name>` seam:
    `original_bytes`, `bar_geometric_plus16`, `duplicate_normal_plus16`, and
    `recovered_octahedral_roundtrip_plus16`. The original variant preserves all
    bytes exactly. The other variants are allowed to mutate only compressed
    vertex byte offset `+16`; all other mesh bytes, index bytes, additional
    data, weights, bind poses, and blend-shape/trailing sections must remain
    byte-identical. This is still an A/B proof boundary, not production armour
    conversion.
18. `Tainted.Armour.FoA` owns the no-hotkey same-mesh A/B runtime proof runner.
    The `[Kandra-Same-Mesh-Ab-Proof]` config route snapshots one explicitly
    selected active live Kandra renderer through `StreamingManager.LoadMeshData`
    and `LoadIndicesData`, builds the four framework variants above, then
    registers each proof mesh through the already guarded host invoker and writes
    one receipt containing `IsRegistered` plus `TryGetMeshMemory` for every
    variant. Reusing the same `ModDirectory` and `MeshNamePrefix` overwrites the
    same eight proof sidecar files instead of accumulating stale packages.
    Candidate-map application, conversion, item/equip/save mutation, archive
    mutation, native game-file writes, and all non-registration downstream
    writes remain false.
19. `KandraSameMeshVisualDecodeComparisonStage` is the receipt consumer for the
    next `+16` decision boundary. It consumes the successful same-mesh A/B host
    receipt facts, checks that all four variants registered and reached
    `TryGetMeshMemory`, compares the original and recovered roundtrip `+16`
    field-change counts, and keeps candidate-map application, production
    conversion, item/equip/save mutation, native game writes, and all
    non-registration downstream writes false. Decode acceptance alone is not an
    encoder decision: exactly one non-original variant still needs reviewed
    captured visual evidence tied to the same receipt/test ID before the stage
    can accept a production `+16` encoder. If reviewed captures reject every
    non-original candidate, the stage records that terminal rejection instead of
    selecting an encoder.
20. `KandraFullSectionPackageGenerationStage` is the first full-section Kandra
    candidate-package converter slice. It consumes complete imported source
    channels and an accepted `+16` encoder identity, writes loose
    `.mdkandra`/`.ixkandra` output through the existing writer, emits the
    matching `KandraMesh` registration metadata and recovered-layout
    metadata-to-stream map, and can feed the existing package-validation and
    registration-preflight gates. It is still a candidate package boundary:
    production conversion acceptance, runtime registration, candidate-map
    application, item/equip/save mutation, archive mutation, native game-file
    writes, and all non-registration downstream writes remain false.
21. `Tainted.Armour.FoA` owns the no-hotkey same-mesh visual evidence route.
    The `[Kandra-Same-Mesh-Visual-Evidence]` config route consumes a successful
    same-mesh A/B receipt path, re-registers the four receipt variants through
    the guarded host invoker, captures one screenshot per variant, records each
    screenshot path/hash against the same source receipt/test ID, and reruns the
    framework visual/decode comparison. It records a reviewed decision only when
    config explicitly names one non-original `AcceptedVariantId` or sets
    `RejectAllNonOriginalCandidates=true`; otherwise the route remains a
    capture-only evidence boundary. Candidate-map application, production
    conversion, item/equip/save mutation, archive mutation, native game-file
    writes, and all non-registration downstream writes remain false.

## Metric provenance

The default policy uses the observed T29/T31 constants and classification:

- displacement epsilon: `0.000001`;
- triangle-area epsilon: `0.00000001`;
- collapsed when posed triangle area is at or below the area epsilon;
- orientation-reversed when normalized baseline/posed normal dot is below zero.
- metric arithmetic follows the observed Unity `Vector3` single-precision
  operation order;
- normal normalization follows Unity's `0.00001` vector-magnitude cutoff.

Unity remains responsible for animation evaluation and renderer state. The
importer-owned adapter performs transient mesh baking, snapshot extraction,
topology adaptation, deterministic geometry fingerprinting, and transient mesh
cleanup. It exposes no candidate-map application, conversion, or write API.

The Unity 6000.0.64f1 Dragon Knight iron/no-cape validation fed six real
baseline/posed snapshot pairs through the adapter and `ArmorImporter.Import`.
It reproduced the T56 total of 444 orientation reversals and zero collapsed or
indeterminate triangles. Candidate-map application and every downstream result
remained false.

The current default T29/T31 normal-dot metric intentionally keeps the
out-of-plane reference-transport control as diagnostic evidence only. That
control exposes the known limitation of the active metric, but it does not block
identity, in-plane rigid-rotation, local-reversal, or collapse calibration while
the unsupported supplemental reference-transport metric remains disabled.

## R2 source/target compatibility slice

The clean R2-backed implementation path is `Tainted.Armour.Compatibility`.
`SourceTargetCompatibilityStage.Evaluate` consumes `CanonicalArmorManifestV1`
plus the R2 `CanonicalManifestArtifact` identity for both source and target.
It verifies that the supplied manifest identities still match current canonical
bytes, derives a deterministic cache key from semantic dependencies, emits a
deterministic compatibility receipt, and classifies the pair as exact,
structural-equivalent, transfer-required, unresolved, or incompatible.

This slice reports compatibility evidence only. It does not call Unity, Kandra,
candidate-map application, conversion, item/equip/save mutation, runtime
registration, package writing, or any downstream writer.

`CompatibilityInputAdapter` is the framework-owned input adapter for this lane.
It accepts a `R2CanonicalArmorArtifactV1` source artifact and a
`FingerprintedTargetProfileV1`, verifies that each canonical artifact still
matches current R2 canonical manifest bytes, preserves the supplied target
profile fingerprint, and returns `SourceTargetCompatibilityRequestV1` with
candidate-map application, conversion, item/equip/save mutation, and downstream
writes all false.

`PersistedR2CanonicalPackageReader` is the persisted-package boundary for this
lane. It loads a durable R2 `canonical.asset.json` package, verifies that the
canonical bytes still match the supplied R2 artifact identity, verifies every
referenced content-addressed blob through the R2 blob store, and returns the
same `R2CanonicalArmorArtifactV1` contract used by the adapter.

`SourceTargetCompatibilityReceiptArtifactReader` and
`CompatibilityReceiptNoWriteConsumer` are the production receipt output
boundary for this lane. The reader loads the durable
`compatibility.receipt.json` artifact into typed source/target/identity/boundary
contracts. The consumer can accept an exact/pass/no-write receipt for reporting,
or block it when schema, gate, classification, receipt blockers, or no-write
boundary checks fail. The consumer still exposes no candidate-map application,
conversion, item/equip/save mutation, or downstream write authority.

The checked-in Dragon Knight T56 fixture under
`tests/real-samples/r2-canonical/` is a durable real-armour package derived from
the importer-owned Unity full-geometry capture for the real Dragon Knight T56
sample. It persists dense R2 blobs for baked positions, submesh-3 indices, and
the mesh-to-asset bind matrix, then validates the persisted package reader,
adapter routing, target-profile fingerprinting, and compatibility-stage result
flow. It is not Kandra conversion, runtime loading, equip/save mutation, or
in-game visual proof.

## Current hard boundary

Passing deformation does not authorize candidate-map application or conversion.
Passing or blocking the A2K-T34 visual observation stage also does not authorize
candidate-map application, Kandra conversion, item/equip/inventory/save
mutation, runtime-loader registration, or downstream writes.
Production conversion remains blocked until the semantic converter/codec,
metadata-to-stream correlation, current archive aliases, and same-mesh runtime
loading/registration evidence all exist. The loose-file Kandra writer slice
only serializes caller-supplied packed sections into the recovered runtime file
order. The package-validation slice proves only that those loose files match
the writer receipt and that a live imported observer result contains existing
Kandra registration/memory proof; it does not authorize custom registration.
The invocation approval stage authorizes only an explicitly approved
host-supplied invocation attempt; a FoA host invoker that safely creates and
populates a custom `KandraRenderer` is consumed through a durable host-proof
receipt. The current accepted live receipt is the `RealSkinnedTriangle` proof
package only. Registering the actual target armour remains gated on a validated
loose target `.mdkandra`/`.ixkandra` package and a later live host invocation for
that target package. The same-mesh A/B proof route narrows the disputed
`CompressedVertex +16` field by registering same-mesh controlled variants, but
it does not by itself authorize changed-topology/full-rebuild production
conversion or item/equip/save integration. The visual/decode comparison consumer
can accept the host proof and decode boundary, but it must continue to block
production encoder selection until visual evidence for exactly one non-original
candidate is attached to the same test ID.
