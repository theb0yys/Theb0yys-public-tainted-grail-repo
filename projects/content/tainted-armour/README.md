# Tainted Armour

`mods/tainted-armour/` is the first-class owner of the standalone Tainted Armour
importer framework.

Tainted Armour is not a feature of Tainted Framework, Dragon Knight, or any
other architecture. Future optional integrations or reflection boundaries are
owned and initiated by Tainted Armour and must fail closed when the external
surface is absent or incompatible.

## Current state

- The provider-neutral `Tainted.Armour` production library exists under `src/`.
- `ArmorImporter.Import` is the canonical typed pipeline entry point.
- The importer-owned `Tainted.Armour.Unity` adapter captures baked Unity geometry
  and submesh topology into the provider-neutral contracts without mutating the
  source or executing downstream operations.
- The framework-owned deformation stage performs calibrated displacement,
  collapse, orientation-reversal, aggregation, and blocker evaluation.
- The optional A2K-T34 visual-runtime observation stage now accepts
  `VisualRuntimeObservationRequest` through `ArmorImporter.Import`; the current
  live transport is the established manual dump route in
  `mods/template-diagnostics`, not a Tainted Armour hotkey.
- Fixtures under `tests/` call that production entry point; they do not contain
  a replacement deformation implementation.
- The real Dragon Knight iron/no-cape T56 sample has executed six typed imports
  through `ArmorImporter.Import` in Unity 6000.0.64f1. The production flow
  reproduced the expected 444 orientation reversals and zero collapses, leaving
  all six imports deformation-blocked.
- `KandraWriterStage` is the first importer-owned Kandra writer slice. It
  writes loose `.mdkandra` and `.ixkandra` files from already-packed payload
  sections to the recovered runtime seam `modDirectory/Kandra/<Name>`. It does
  not convert Unity geometry into packed Kandra codecs, patch `kandra.arch`,
  register a runtime renderer, mutate equipment/items/saves, or apply candidate
  maps.
- `KandraPackageValidationStage` is the no-write package gate after the writer.
  It reads the loose files back from `modDirectory/Kandra/<Name>`, verifies the
  seam, file presence, byte counts, hashes, writer false-boundary state, and a
  live `ArmorImporter.Import` result containing Kandra `IsRegistered` and
  `TryGetMeshMemory` proof. It still does not authorize custom armour
  registration.
- `KandraRegistrationPreflightStage` is the next no-write registration
  readiness check. It compares a selected custom `KandraMesh` metadata contract
  (`modDirectory`, `name`, vertex/index/bind-pose/blend-shape counts, and
  payload layout version) against the validated loose package before any
  registration call. It still does not call registration, apply candidate maps,
  convert geometry, mutate items/equipment/saves, or write game state.
- `KandraRegistrationCandidateStage` builds the first custom registration
  candidate request object from an accepted preflight result. The request object
  carries the selected `KandraMesh` metadata identity, validated loose package
  paths, byte counts, hashes, and layout version, while marking the live
  registration call contract as unproven. It still does not invoke registration.
- `KandraRuntimeRegistrationDryRunStage` is the first guarded runtime
  registration dry-run gate. It consumes an accepted registration candidate, the
  validated loose Kandra package, and the live
  `KandraRendererManager.Register(KandraRenderer)` contract fingerprint. It can
  accept that dry-run planning boundary only when the exact recovered method
  fingerprint and `Awaken.Kandra` assembly hash match, but it still has no
  runtime invocation approval field and refuses to call `Register`, `CanRegister`, object
  creation/activation, conversion, candidate-map application, item/equip/save
  mutation, archive mutation, or game writes.
- `KandraRuntimeRegistrationInvocationStage` is the explicit invocation approval
  gate after the dry-run boundary. It requires an accepted dry run, a matching
  approval record pinned to the same loose package hashes and recovered
  `Register(KandraRenderer)` fingerprint, and a host-supplied invoker. It can
  call that invoker exactly after those checks pass, retaining returned
  `IsRegistered`/`TryGetMeshMemory` proof, while still refusing candidate-map
  application, conversion, item/equip/save mutation, and native game-file
  writes. The provider-neutral core does not construct FoA `KandraRenderer`
  objects by itself.
- `KandraRuntimeRegistrationHostProofStage` consumes the durable FoA host-invoker
  receipt for `AvalonAwakenedProof/RealSkinnedTriangle` and validates the first
  proven live host invocation: `Register(KandraRenderer)` invoked, runtime
  registration succeeded, `IsRegistered=True`, `TryGetMeshMemory=True`, and all
  non-registration downstream writes false.
- `KandraTargetRuntimeRegistrationPlanStage` is the controlled target-package
  registration planner. It requires accepted host proof, an accepted target
  dry-run package, and matching explicit approval, refuses reuse of the
  `RealSkinnedTriangle` proof identity or payload hashes as the target, and
  constructs only the typed target invocation context. It does not call the host
  invoker, apply candidate maps, run conversion, mutate item/equip/save state, or
  write downstream state.
- `KandraSameMeshAbProofStage` is now the framework-owned controlled same-mesh
  A/B package builder for the disputed Kandra `CompressedVertex +16` field. It
  takes original runtime mesh/index bytes and emits exactly four loose package
  variants: original bytes, BAR geometric `+16`, duplicate-normal `+16`, and
  recovered octahedral roundtrip `+16`. The non-original variants may change
  only the compressed-vertex `+16` field; every other mesh byte and all index
  bytes must remain identical.
- The FoA live observer now includes a reflection-only Kandra registration
  contract diagnostic. Current static inspection fingerprints the live owner as
  `Awaken.Kandra.KandraRendererManager.Register(KandraRenderer)`, with normal
  entry through `KandraRenderer.OnEnable()` and deferred finalisation through
  `FinalizeRegistration()` from the manager's EarlyUpdate hook. The diagnostic
  records the method/member surface and assembly fingerprint but still never
  invokes registration or readiness precheck methods.
- The FoA plugin now has a `[Kandra-Same-Mesh-Ab-Proof]` no-hotkey config route.
  It snapshots one selected live Kandra renderer, writes the four same-mesh
  proof packages through the framework stage, registers each proof package
  through the existing guarded host invoker, and emits one receipt with
  `IsRegistered`/`TryGetMeshMemory` for every variant. Reusing the configured
  mod directory and mesh-name prefix overwrites the same proof files instead of
  accumulating stale packages.
- `KandraSameMeshVisualDecodeComparisonStage` consumes that successful receipt
  as the host-registration proof for the next `+16` decision. It accepts the
  decode boundary only when the original/recovered roundtrip comparison stays
  within tolerance and all four variants retain registration/memory proof. It
  still refuses production encoder selection until exactly one non-original
  variant has reviewed captured visual evidence tied to the same test ID. A
  reviewed capture set may also reject every non-original candidate without
  selecting an encoder.
- `KandraFullSectionPackageGenerationStage` is the first full-section Kandra
  candidate-package converter slice. It consumes complete imported vertex
  channels, accepted `+16` encoder identity, indices, bindposes, and optional
  blendshape names, writes loose `.mdkandra`/`.ixkandra` candidate packages,
  emits the matching `KandraMesh` registration metadata and metadata-to-stream
  map, and now fixtures that output through the existing package-validation and
  registration-preflight gates. It still keeps production conversion,
  runtime registration, candidate-map application, item/equip/save mutation,
  native game-file writes, and non-registration downstream writes false.
- The FoA plugin also has a `[Kandra-Same-Mesh-Visual-Evidence]` no-hotkey
  config route. It consumes one successful same-mesh A/B receipt path,
  re-registers the four proof variants through the guarded host invoker,
  captures one screenshot per variant, attaches the screenshot hashes/paths to
  the same source receipt/test ID, and reruns
  `KandraSameMeshVisualDecodeComparisonStage`. The route records a reviewed
  decision only when config explicitly names one non-original `AcceptedVariantId`
  or sets `RejectAllNonOriginalCandidates=true`; otherwise it captures evidence
  and leaves encoder selection blocked.
- Candidate-map application, production conversion, target armour Kandra payload
  generation, item/equip/save mutation, native game-file writes, and all
  non-registration downstream writes remain false. Runtime registration
  invocation is represented by the explicit approval + host-invoker gate above;
  the first live proof is the checked-in `RealSkinnedTriangle` receipt, not a
  target armour package.
- `Tainted.Armour.Compatibility` is the first clean R2-consumer
  source/target compatibility slice. It compares source and target profiles
  from R2 canonical manifest artifacts, emits deterministic cache and receipt
  identities, reports exact/structural/transfer/unresolved/fail state, and
  authorises no candidate-map application, conversion, item/equip/save
  mutation, or downstream write.
- `CompatibilityInputAdapter` now converts a validated R2 canonical armour
  artifact plus a caller-fingerprinted target profile into
  `SourceTargetCompatibilityRequestV1`, with conversion and every downstream
  boundary fixed false.
- `PersistedR2CanonicalPackageReader` loads a durable R2
  `canonical.asset.json` package, verifies the canonical manifest artifact
  identity and referenced content-addressed blobs, and feeds that artifact into
  the compatibility adapter. The checked-in Dragon Knight T56 package fixture is
  derived from the importer-owned real Unity sample capture and persists dense
  position, submesh-index, and bind-matrix blobs through the R2 package writer.
- `Tainted.Armour.R2PackageBuilder` is the framework-owned tool route from
  full-geometry capture to durable R2 package plus fingerprinted target profile.
- Production Kandra conversion remains blocked on the separately recorded
  semantic converter/codec, metadata-to-stream, archive-alias, and same-mesh
  runtime registration evidence requirements. A FoA live invoker that safely
  creates/populates a custom `KandraRenderer` remains a separate runtime-host
  implementation boundary.

## Framework boundary

The first implementation slice must establish the framework's canonical entry
point, typed request and pipeline-result contracts, and a framework-owned
deformation-validation stage. Calibration checks may test that production stage,
but must not replace it with standalone test or editor scripts.

That first slice and its importer-owned Unity geometry adapter are implemented.
Their exact production boundary is recorded in `docs/design.md`; the live sample
receipt is under `tests/real-samples/`.

The researched ownership and stage boundary is recorded in
`docs/research/frameworks/armor-importer-first-class-framework-ownership-and-deformation-stage-reframe-2026-08-09.md`.
