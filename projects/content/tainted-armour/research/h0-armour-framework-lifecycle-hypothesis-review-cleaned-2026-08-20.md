# H0 Armour Framework Lifecycle-Hypothesis Review — Cleaned Deep Research Derivative

Document control:

- Document ID: `tainted-armour-h0-lifecycle-hypothesis-review-cleaned-2026-08-20`
- Status: `under-evaluation`
- Authority state: `intake`
- Knowledge class: cleaned derivative of a returned ChatGPT Deep Research report
- Research method: ChatGPT Deep Research
- Source execution state: `RETURNED`
- Source returned: 2026-08-20
- Source locator: the H0 Deep Research report pasted into the controlling conversation on 2026-08-20
- Original byte preservation: unavailable; the report arrived as conversation text, not as a downloadable raw artifact
- Original SHA-256 and byte size: not available and not inferred
- Derivative created: 2026-08-20
- Product owner: `mods/tainted-armour/`
- Research owner: `documents/research/frameworks/tainted-armour/`
- Governing process:
  - `documents/process/research-lifecycle.md`
  - `documents/process/research-standard/README.md`
  - `documents/process/research-standard/deep-research-report-intake-standard.md`
- Existing hypotheses reviewed:
  - [`canonical-importer-lifecycle-production-grade-armour-asset-import-pipeline-2026-08-10.md`](canonical-importer-lifecycle-production-grade-armour-asset-import-pipeline-2026-08-10.md)
  - [`canonical-armour-intermediate-representation-and-provenance-pipeline-2026-08-10.md`](canonical-armour-intermediate-representation-and-provenance-pipeline-2026-08-10.md)
  - [`programme/README.md`](programme/README.md)
- Allowed usage:
  - H0 lifecycle correction and claim review
  - research sequencing
  - source navigation
  - preparation of bounded stage-specific research briefs after H0 acceptance
- Forbidden usage:
  - implementation authority
  - coded-gate authority
  - conversion or package generation
  - runtime or save mutation
  - compatibility or release claims
  - treating the provisional stage count as final
  - automatically executing any next research task
- Supersedes: none
- Superseded by: none

## Intake disposition

The returned report contains substantial useful research, but it is a mixed `E1` research artifact. It correctly challenges several assumptions in the existing P0–P20 lifecycle, while overclaiming several replacements.

This cleaned derivative:

1. preserves the useful findings;
2. removes temporary conversation-native citations;
3. replaces them with durable repository paths and primary external sources;
4. narrows claims that exceeded the inspected evidence;
5. separates research establishment from operational asset processing;
6. records remaining proprietary proof as explicit blockers;
7. keeps the recommended architecture and stage count provisional.

The original conversation report remains the source artifact. This file is not represented as a byte-identical copy.

## Executive verdict

**H0 state: PARTIAL.**

The current P0–P20 lifecycle must not be adopted unchanged. It identifies most major problem families, but its exact stage boundaries, ordering, abstractions, and stage count are not established.

The strongest current architecture candidate is:

```text
hybrid evidence-producing asset compiler
+ parallel source and FoA/Kandra target research
+ native vertical control assets
+ a provider-neutral semantic boundary only if source/target evidence sustains it
+ a target-specific Kandra backend
+ independent producer/verifier separation
+ distinct runtime, gameplay, persistence, teardown, and release proof domains
+ lifecycle-wide provenance and authorisation
```

This is a leading candidate, not a final promoted decision.

The returned proposal of 22 operational stages is **provisional**. The final operational stage count and the final research-stage count remain **UNKNOWN**.

## Evidence model

Only the following lanes may support H0 claims.

| Lane | Evidence | Maximum claim type |
|---|---|---|
| `A — decompilation-static` | Exact-build DLLs, SHA-256/MVID, types, fields, method bodies, manifests and static asset metadata | Proprietary ownership, reader contracts, field relationships, call order and static lifecycle |
| `B1 — native-static` | Fingerprinted native armour, Kandra streams, skeletons, bind poses, materials, templates, package aliases and canonical captures | Exact data values and static target invariants |
| `B2 — runtime-observation` | Controlled in-game registration, rendering, equip and lifecycle captures | The exact runtime behaviour observed in the recorded environment |
| `B3 — persistence` | Controlled save/load/restart/migration and missing-content captures | The exact persistence behaviour observed |
| `B4 — capacity-performance` | Controlled resource, memory, timing, capacity and repeated-cycle measurements | The measured capacity and performance envelope |
| `B5 — controlled-validation` | Independent mathematical, format, package and scenario validation against characterised controls | The exact validation claim and environment recorded |
| `C — external-research` | Primary specifications, official platform documentation and primary technical literature gathered through Deep Research | General methods, platform semantics, alternative architectures and research candidates |

These lanes do not substitute for one another.

```text
static reader evidence     ≠ writer correctness
native stream bytes        ≠ visible rendering
visible rendering          ≠ item/equip correctness
one save reload            ≠ migration or missing-content safety
runtime success            ≠ release readiness
```

Implementation, tests, fixtures, probes, generated receipts, build output, CI output, agent notes, plans and code comments may later validate an accepted contract. They do not determine what the contract should be and are not H0 research authority.

## Findings retained from the returned report

### 1. P0–P20 is not proven

The candidate lifecycle is useful as a map of problem families. It is not an empirically established optimal process.

The lifecycle document itself is an architecture candidate and explicitly leaves the production deformation method, universal bone adaptation, full Kandra format, production converter, package aliases, runtime integration, persistence and future compatibility unresolved.

### 2. Source and target research should proceed in parallel

A source-first-only programme risks freezing source extraction, semantic representation, adaptation and validation before the target constraints are sufficiently known.

A target-first-only programme risks making one FoA/Kandra build the definition of all armour.

The leading research pattern is therefore:

```text
source-format and source-corpus evidence ─────┐
                                              ├─> semantic-boundary review
FoA/Kandra target and native-control evidence ─┘
```

The first required join is source-target compatibility analysis.

### 3. Unity is semantically consequential, but its mandatory status is not established

Unity documentation establishes that:

- asset postprocessors can alter model-import settings before import;
- imported GameObjects and meshes observed during postprocessing are transient;
- `ModelImporter.maxBonesPerVertex` can discard the lowest-weight influences;
- `Mesh.GetAllBoneWeights()` exposes all non-zero weights that remain in the imported mesh.

Therefore Unity must be treated as a versioned, configurable interpretation adapter.

H0 does **not** establish that Unity can never be the mandatory adapter for the eventual supported source scope. That decision requires comparison against the supported source corpus and credible alternative adapters.

### 4. Target characterisation must challenge upstream assumptions early

Exact-build Kandra evidence already exposes target constraints that affect earlier research:

- four packed influences in `PackedBonesWeights`;
- 16-bit bone indices in the packed runtime representation;
- U16 index loading;
- explicit bind-pose data;
- renderer-local bone mappings;
- rig, material, root and bounds requirements;
- multiple manager registrations before a renderer becomes fully registered.

These facts do not define the canonical source model. They do require early representational-completeness and compatibility review.

### 5. R9 and R10 are under-decomposed

`R9 Kandra reverse engineering` currently hides distinct questions:

- packed field and stream semantics;
- writer/encoder semantics;
- independent reader/verifier semantics;
- Kandra metadata and renderer construction;
- rig and renderer bone mapping;
- materials, textures and mip streaming;
- body culling;
- package identity and loader behaviour.

`R10 runtime registration/persistence/promotion` currently hides:

- full registration;
- visible Kandra rendering;
- item/template identity;
- inventory ownership and acquisition;
- equip and presentation;
- save/load and restart;
- migration and missing content;
- teardown and resource ownership;
- capacity, performance and repeated-cycle stability;
- release qualification.

No one result may authorise the others.

### 6. P18 and P19 are too broad

The following transitions must remain separate proof claims:

```text
file resolved
→ stream decoded
→ renderer submitted
→ renderer fully registered
→ skinning prepared
→ draw submitted
→ visible pixels
→ correct deformation/material/culling
→ item registered
→ inventory ownership
→ equip/unequip
→ save/load/restart
→ missing-content and migration safety
→ resource release and repeated-cycle stability
```

The exact final grouping and order remain under research.

### 7. Adaptation-method selection must remain open

Primary literature provides different candidate methods with different assumptions.

- Deformation transfer uses source-target correspondences and an optimisation process to transfer deformation between meshes that need not share connectivity.
- Bounded biharmonic weights construct smooth, bounded, shape-aware weights for handles such as bones, points and cages.
- Geodesic voxel binding targets production geometry that may be non-manifold, non-watertight, intersecting or multi-component.

These are research candidates, not interchangeable universal solutions.

The adaptation programme must identify armour classes, method assumptions, controls, failure modes and loss limits before selecting a production policy.

### 8. Robust sign predicates require specific research

Primary computational-geometry literature establishes that ordinary floating-point orientation predicates can become unreliable near degeneracy and that adaptive-precision predicates can provide dependable orientation decisions.

This supports research into robust inversion/orientation classification. It does not by itself establish the complete armour deformation metric, pose corpus or acceptance thresholds.

### 9. A provider-neutral semantic representation remains a leading candidate, not a proven final contract

LLVM and MLIR provide useful architectural analogies for shared intermediate representations and target-specific interfaces. glTF provides useful skinning and binary-buffer representation principles.

Those sources do not prove that Tainted Armour requires exactly one persistent IR, that the current R1 schema is complete, or that every source and Kandra semantic can be represented without loss.

R1 must be challenged against both supported source classes and the recovered FoA/Kandra target corpus.

### 10. R2 mechanisms remain technically defensible within a narrow boundary

RFC 8785 provides a canonical JSON representation for repeatable hashing. SHA-256 is a standard cryptographic hash.

Those primary sources support deterministic representation and content identity as mechanisms. They do not establish that the current R2 placement, object model or surrounding end-to-end architecture is final.

## Corrections applied to the returned report

### Unity conclusion

Returned wording: Unity as a mandatory importer is disproven.

Corrected disposition:

> Unity's mandatory status is `NOT_ESTABLISHED`. Unity is a candidate source adapter whose exact version and settings must be researched.

### Conversion authorisation

Returned wording: replace P12 with a cross-cutting authorisation policy.

Corrected disposition:

> Lifecycle-wide authorisation is supported. Whether explicit transition decisions are counted as stages remains `UNRESOLVED`.

The leading candidate is:

```text
cross-cutting authorisation evaluator
+
explicit decision artifact before each consequential boundary
```

Candidate boundaries include:

- adaptation execution;
- Kandra encoding;
- package installation/runtime loading;
- item/equip mutation;
- save mutation;
- release promotion.

### Final stage count

Returned wording: 22 operational stages.

Corrected disposition:

```text
existing candidate:       21 P-stages
returned alternative:     22 F-stages
accepted final count:     UNKNOWN
```

The 22-stage proposal is retained only as one decomposition candidate.

### Research lifecycle versus operational lifecycle

The returned report mixed two different processes.

Research establishment:

```text
research scope
→ external research
→ exact-build static evidence
→ native/control corpus
→ claim evaluation
→ architecture decision
→ stage-specific research
→ accepted stage contract
→ coded gate
```

Operational asset processing:

```text
source intake
→ interpretation
→ semantic capture
→ target compatibility
→ adaptation
→ correctness validation
→ target encoding
→ independent verification
→ package
→ runtime integration
→ gameplay integration
→ persistence
→ teardown/stability
→ promotion
```

They require separate graphs and separate counts.

### Runtime ordering

The returned report proposed:

```text
Kandra construction/registration
→ visible-render proof
→ item/inventory/equip proof
```

That sequence may be valid for a controlled renderer harness. It is not yet proven to be the native armour presentation sequence.

The research must distinguish:

- renderer-harness registration/rendering;
- native item/equip-driven presentation and rendering.

Their final order is `UNRESOLVED`.

### Capacity, ownership and performance

Capacity and resource ownership are not only late release concerns.

They affect:

- target compatibility;
- adaptation and influence limits;
- package design;
- renderer construction;
- rollback and cleanup.

A later stress/performance stage may still be required, but its constraints must enter earlier contracts.

## Corrected P0–P20 disposition

| Current stage | H0 disposition | Correction required |
|---|---|---|
| P0 Environment capture | `RETAIN_WITH_NARROWER_SCOPE` | Separate semantic dependencies from physical execution telemetry |
| P1 Source fingerprint | `RETAIN` | Define the complete source closure, not only one FBX |
| P2 Deterministic Unity import | `REPLACE` | Use a source-interpretation adapter contract; Unity is one candidate adapter |
| P3 Canonical IR extraction | `RETAIN_WITH_NARROWER_SCOPE` | Revalidate representational completeness against source and target evidence |
| P4 Structural validation | `RETAIN` | Separate contradiction, unsupported feature and unresolved semantics |
| P5 Target profile capture | `REORDER` | Run early and in parallel with source interpretation research |
| P6 Compatibility analysis | `RETAIN` | Remain observational and non-mutating |
| P7 Adaptation planning | `RETAIN` | Specify every algorithm, parameter, tie-break, loss and postcondition |
| P8 Adapted asset generation | `RETAIN` | Generate a new artefact from immutable source plus accepted plan |
| P9 Bind-space validation | `RETAIN` | Recover exact target conventions and justify numeric policy |
| P10 Pose/deformation validation | `RETAIN` | Research and calibrate metrics against characterised controls |
| P11 Visual readiness | `SPLIT` | Separate offline readiness from target-runtime visual correctness |
| P12 Conversion authorisation | `UNRESOLVED` | Retain lifecycle-wide authorisation; decide whether transition decisions are counted as stages |
| P13 Target-format conversion | `RETAIN_WITH_NARROWER_SCOPE` | Execute only after writer semantics are proven |
| P14 Conversion verification | `RETAIN` | Independent parser/reconciliation; writer self-check is insufficient |
| P15 Package construction | `RETAIN` | Build complete isolated dependency closure |
| P16 Package verification | `RETAIN` | Independently resolve paths, aliases, identities and dependencies |
| P17 Runtime registration | `RETAIN_WITH_NARROWER_SCOPE` | Prove full Kandra registration only; do not infer visible rendering |
| P18 Live runtime validation | `SPLIT` | Separate render proof, runtime visual correctness and gameplay integration |
| P19 Persistence and teardown | `SPLIT` | Separate persistence/migration from teardown/capacity/stability/performance |
| P20 Promotion | `RETAIN` | Human promotion over a complete, current, bounded evidence chain |

## Corrected R1–R10 programme disposition

| Workstream | H0 disposition |
|---|---|
| R1 Canonical Armour IR | `RETAIN_WITH_NARROWER_SCOPE` — candidate semantic boundary; representational completeness remains unproven |
| R2 Deterministic serialisation and content addressing | `RETAIN_WITH_NARROWER_SCOPE` — mechanisms remain useful; final architectural placement remains unproven |
| R3 Receipts, provenance, authorisation and cache | `SPLIT` — provenance/authorisation and cache/invalidation/transaction/security are cross-cutting research families |
| R4 Unity import and canonical extraction | `REPLACE` — source-interpretation architecture plus semantic-capture research |
| R5 Characterisation and compatibility | `SPLIT_AND_REORDER` — source characterisation and target recovery run in parallel before compatibility |
| R6 Rig adaptation and skin transfer | `SPLIT` — bone mapping, bind/geometry conversion, skin transfer, influence reduction and loss policy |
| R7 Bind-space and deformation validation | `SPLIT` — bind mathematics, deformation metrics, robust predicates, corpus calibration and independent verification |
| R8 Visual readiness | `SPLIT` — offline readiness versus live FoA visual correctness |
| R9 Kandra reverse engineering and verification | `SPLIT` — writer, verifier, renderer, materials, culling and package/loading |
| R10 Runtime registration, persistence and promotion | `SPLIT` — render, item/equip, persistence, teardown/stability/performance and release |

## Leading architecture candidate

### Research-establishment graph

```text
H0 claim review and human lifecycle decision
        ↓
evidence-governance contract + current exact-build baseline
        ↓
┌───────────────────────────────┬────────────────────────────────┐
│ source corpus and adapters    │ FoA/Kandra target recovery     │
│ source semantic observations  │ native armour control corpus   │
└───────────────────────────────┴────────────────────────────────┘
        ↓
semantic-boundary and compatibility review
        ↓
bounded stage-specific research
        ↓
accepted stage contracts
        ↓
coded gates and independent validators
```

### Candidate operational asset graph

This graph is provisional.

```text
environment/evidence context
        ↓
immutable source intake
        ↓
source interpretation
        ↓
semantic capture and structural validation
        ↓
consume current target profile
        ↓
compatibility analysis
        ↓
adaptation plan and adapted asset
        ↓
bind/deformation/offline-readiness proof
        ↓
Kandra encoding and independent payload verification
        ↓
package construction and independent package verification
        ↓
runtime construction / native presentation integration
        ↓
visible-render and gameplay proof
        ↓
persistence and migration proof
        ↓
teardown, capacity, stability and performance proof
        ↓
human promotion
```

The ordering of runtime construction, native equip presentation and representative render proof remains open.

### Cross-cutting governance

These concerns govern every transition and are not assumed to be one sequential stage:

- provenance;
- semantic receipt versus execution-record separation;
- authorisation;
- dependency invalidation;
- cache identity;
- isolated staging;
- atomic promotion and rollback;
- untrusted-input security;
- deterministic diagnostics;
- version/build applicability;
- unsupported-feature handling;
- evidence maturity;
- independent review;
- human promotion.

## Remaining research programme

The following is the current bounded research backlog after H0 intake. It is **not** a final operational stage list. H0 review may merge, split, reorder or remove packages.

### Phase A — H0 closure and evidence foundation

1. **H0 claim review and lifecycle decision** — inspect the returned report's sources claim by claim, resolve contradictions, compare lifecycle alternatives and obtain a human decision on the research and operational graphs.
2. **Evidence, provenance and transition-authorisation contract** — exact receipts, execution records, provenance relations, permissions and per-transition decision artifacts.
3. **Cache, invalidation, transaction, rollback and security contract** — semantic cache keys, stale detection, staging, atomic promotion, quarantine, parser isolation and resource limits.
4. **Current exact-build baseline** — current game/build identity, Unity player version, `ScriptingAssemblies.json`, relevant DLL SHA-256/MVID values, package identities and review triggers.
5. **Native armour control corpus** — representative body variants, armour slots, rigs, streams, materials, culling, packages, runtime states, items and saves.

### Phase B — Source interpretation and semantic boundary

6. **Supported-source and source-closure policy** — supported formats/features, companion resources, permissions, immutable intake and unsupported input classification.
7. **Source-adapter comparison and Unity import profile** — whether Unity is mandatory, optional or one of several adapters; exact versions/settings and clean-workspace repeatability.
8. **Canonical semantic-boundary completeness** — whether R1 preserves all required source semantics and all information needed by validation/adaptation/Kandra without target leakage.
9. **Canonical structural validation** — topology, attributes, hierarchy, skin, binds, influence slices, blendshapes, resources and unsupported-feature taxonomy.

### Phase C — FoA target and adaptation

10. **FoA body, rig, slot and presentation target contract** — body variants, skeletons, bind data, renderer metadata, equipment categories, root/bounds and presentation ownership.
11. **Source-target compatibility classification** — exact, equivalent, transferable, unresolved and incompatible states with deterministic reasons.
12. **Bone mapping and rig adaptation** — mapping rules, aliases, hierarchy constraints, helper/twist/accessory bones, missing/extra bones and deterministic tie-breaks.
13. **Coordinate, geometry, body-fit and bind conversion** — basis/units, rest alignment, mesh-to-target transforms, seams, topology policy, normals/tangents and loss accounting.
14. **Skin transfer and influence policy** — candidate methods, correspondence, pruning to target limits, weight ordering, quantisation, normalisation, zero-weight handling and repair.
15. **Material and body-culling adaptation planning** — source-to-target material remap, body coverage, hidden-triangle requirements and unsupported cloth/blendshape/accessory cases.

### Phase D — Mathematical and visual validation

16. **Bind-space mathematics and calibration** — exact spaces, matrix order, root treatment, native controls, singular/mirrored cases and justified tolerances.
17. **Deformation metrics and robust predicates** — pose corpus, inversion, collapse, stretch, seam and joint-region measures, robust sign policy and false-classification analysis.
18. **Independent validation control corpus** — known-good native controls, deliberately characterised failures, expected outcomes and verifier independence.
19. **Offline visual-readiness contract** — fixed scenarios, camera/pose evidence, clipping/seam/bounds/material checks and exact limits on claims.

### Phase E — Kandra backend

20. **Kandra writer-facing format semantics** — exact struct sizes, alignment, endianness, normal/tangent/UV/weight/blendshape codecs, limits and metadata-to-stream mapping.
21. **Independent Kandra parser and payload verifier** — decode/reconcile generated bytes without relying on the writer's implementation assumptions.
22. **Kandra renderer construction and registration** — valid `KandraMesh`, `KandraRig`, `KandraRenderer`, bone map, root/bounds, materials, blendshapes, capacities, async loading and rollback.
23. **Materials, shaders, textures and mip streaming** — shader identities, properties, keywords, texture slots, formats, colour space, transparency, reciprocal UV distribution and ownership.
24. **Body culling runtime contract** — culler/cullee identity, triangle numbering, authoring pose corpus, compressed ranges, runtime attachment and cleanup.
25. **Package, loading, aliases and cache behaviour** — loose/archive precedence, names, paths, metadata assets, hashes, async errors, stale/missing pairs, upgrades and uninstall.

### Phase F — Runtime, gameplay and release

26. **Live Kandra render proof** — distinguish file read, full registration, skinning, draw submission, visible pixels, deformation, materials, culling and bounds.
27. **Item, inventory, acquisition and equip integration** — template identity, category/slot, ownership, presentation loading, native stitching/redirection, equip/unequip and conflicts.
28. **Persistence, migration and missing-content safety** — equipped/unequipped saves, full restart, registration order, missing package/template, upgrade/downgrade and save-overwrite protection.
29. **Teardown, resource ownership, capacity, stability and performance** — unregister, pending reads, buffers, rigs, bones, blendshapes, materials, culling, repeated cycles, limits, memory and frame cost.
30. **Promotion, compatibility and revalidation** — supported builds/features, reproducibility, install/upgrade/uninstall, patch invalidation, evidence expiry and named human promotion.

There are currently **30 bounded research packages including H0 closure**. This is a planning backlog, not a final lifecycle count.

## Immediate proprietary blockers

External research cannot resolve these claims:

- exact current FoA build and relevant assembly/package fingerprints;
- complete Kandra writer codecs and physical byte semantics;
- metadata-to-stream correlation across representative native meshes;
- valid custom Kandra object construction and rollback;
- shader/material/texture/mip authoring expectations;
- body culling identity and runtime attachment;
- current package aliases, precedence and error behaviour;
- native armour presentation/equip ordering;
- save restoration, migration and missing-content behaviour;
- visible rendering through Kandra;
- teardown, capacities, resource release and performance.

They require exact-build `A` evidence and the appropriate `B1`–`B5` evidence.

## Implementation threshold

No new coded gate is implementation-authorised until:

1. H0's research and operational graphs are accepted;
2. the gate belongs to an accepted stage boundary;
3. a dedicated research package exists under `documents/`;
4. every material general claim has inspected primary external support;
5. every proprietary claim has exact-build static/native/runtime evidence fit for that claim;
6. exact inputs, outputs, units, spaces, layouts, limits and algorithms are specified;
7. no consequential instruction remains vague;
8. `PASS`, `FAIL` and `INDETERMINATE` are operationally distinguishable;
9. an independent verification criterion exists before producer implementation;
10. rollback and failure containment are specified;
11. version invalidation is specified;
12. contradictions are resolved or remain blocking;
13. the research has completed the required review hierarchy and gained explicit bounded implementation permission.

Existing code may be inspected as a hypothesis generator. It may not be used as evidence for its own correctness.

## Durable source ledger

### Repository evidence

| ID | Repository source | Ref / blob | Lane | Establishes | Does not establish |
|---|---|---|---|---|---|
| H0-R1 | `documents/research/frameworks/tainted-armour/canonical-importer-lifecycle-production-grade-armour-asset-import-pipeline-2026-08-10.md` | `main@adf1b1c57493b3b6d5961750ccb27032161b81ea`; blob `c1394fdbf6d7675e598422bc21ee127f6a92dfbe`; headings `Executive summary`, `Research boundaries that remain unproven` | research context | Existing P0–P20 hypothesis and admitted gaps | Optimality, final stage count or proprietary correctness |
| H0-R2 | `documents/research/frameworks/tainted-armour/canonical-armour-intermediate-representation-and-provenance-pipeline-2026-08-10.md` | same ref; blob `f0e8f09a90ff0a74f05dd758d567f076131c0b46`; headings `Decision`, `Receipts, authorisation and execution` | research context | Candidate IR/provenance architecture | Representational completeness or final authority |
| H0-R3 | `documents/research/frameworks/tainted-armour/programme/README.md` | same ref; blob `7269146d448a9c9a532b5252448fd6936db08964`; headings `Workstreams`, `Current state` | programme context | Existing R1–R10 grouping and narrow R1/R2 status | Correct future sequencing |
| H0-A1 | `documents/official/proprietary-systems/rendering/Questline Kandra Decompiled Runtime Lifecycle.md` | same ref; blob `b95eda0f3f31d1fbe88d023028ae6bae00e2b326`; symbols `KandraMesh.ReadSerializedData`, `KandraRendererManager.FinalizeRegistration`, `StreamingManager` | decompilation-static | Exact managed reader layout, loading, registration, BRG path and cleanup for the fingerprinted DLL | Writer correctness, custom armour rendering, save or release safety |
| H0-A2 | `documents/official/proprietary-systems/compatibility/Questline Exact Mono Build 24270691 Provenance and Managed Implementation Equivalence.md` | same ref; blob `2371b8e7badc4a0ab681e2675eb79a9290da09e4`; headings `Executive verdict`, `Current unresolved evidence` | decompilation-static / recorded installation | Bounded template/save lookup contracts for the fingerprinted `TG.Main.dll` | Equip restoration, current runtime ordering, persistence safety or current-build closure |
| H0-P1 | `documents/process/research-standard/deep-research-report-intake-standard.md` | `main`; heading `Returned-report intake hard stop` | process authority | Required intake, cleaning, persistence and follow-on lock | Native technical truth |

### Primary external sources inspected

- Unity 6 `AssetPostprocessor`: <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetPostprocessor.html>
- Unity 6 `OnPostprocessModel`: <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetPostprocessor.OnPostprocessModel.html>
- Unity 6 `ModelImporter.maxBonesPerVertex`: <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ModelImporter-maxBonesPerVertex.html>
- Unity 6 `Mesh.GetAllBoneWeights`: <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Mesh.GetAllBoneWeights.html>
- Khronos glTF 2.0 specification, skins and skinned-mesh attributes: <https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html>
- W3C PROV-DM Recommendation: <https://www.w3.org/TR/prov-dm/>
- RFC 8785, JSON Canonicalization Scheme: <https://www.rfc-editor.org/rfc/rfc8785.html>
- NIST FIPS 180-4, Secure Hash Standard: <https://csrc.nist.gov/pubs/fips/180-4/upd1/final>
- LLVM Language Reference Manual: <https://llvm.org/docs/LangRef.html>
- MLIR Interfaces: <https://mlir.llvm.org/docs/Interfaces/>
- Sumner and Popović, *Deformation Transfer for Triangle Meshes*: <https://people.csail.mit.edu/sumner/research/deftransfer/>
- Jacobson et al., *Bounded Biharmonic Weights for Real-Time Deformation*: <https://igl.ethz.ch/projects/bbw/>
- Dionne and de Lasa, *Geodesic Voxel Binding for Production Character Meshes*: <https://doi.org/10.1145/2485895.2485919>
- Shewchuk, *Adaptive Precision Floating-Point Arithmetic and Fast Robust Geometric Predicates*: <https://people.eecs.berkeley.edu/~jrs/papers/robustr.pdf>

## Current conclusion

The original Armour programme is now a challenged hypothesis.

The leading correction is a hybrid parallel source/target research architecture with vertical native controls and independent verification. The semantic core, Unity role, authorisation structure, runtime order and final stage counts remain subject to claim review.

The immediate work is not implementation. It is to close H0, establish the current exact-build/native corpus, and then create bounded research packages for the accepted architecture.
