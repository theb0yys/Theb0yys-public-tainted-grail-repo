# Canonical Armour IR v1 Conformance Suite

Document control:

- Status: Canonical R1 conformance specification; not yet executed
- Contract under test: `canonical-armour-ir-v1-contract-decision-2026-08-10.md`
- Workstream: R1
- Successor implementation: R2
- Date: 2026-08-10

## 1. Purpose

This suite defines the minimum tests required to demonstrate that two independent implementations interpret, serialize, hash, and validate Canonical Armour IR v1 consistently.

A claimed implementation is not conformant merely because it can deserialize one example. It must prove:

```text
semantic interoperability
binary interoperability
identity determinism
manifest canonicality
structural validation
bind-space agreement
sparse-data agreement
extension preservation
security failure handling
separation of semantic and execution state
```

## 2. Conformance roles

An implementation may claim one or more roles:

| Role | Required capability |
|---|---|
| Writer | Produces canonical manifest and semantic blobs from a typed in-memory asset. |
| Reader | Reads canonical bytes and reconstructs the typed semantic asset. |
| Verifier | Independently validates hashes, canonicality, references, ranges, relationships, and invariants. |
| Identity producer | Produces deterministic `CanonicalObjectId` values from source object keys. |
| Diff consumer | Compares canonical assets by object and blob identities. |

R2 completion requires either:

1. two independently implemented writer/reader/verifier stacks; or
2. one writer/reader implementation and an independently written reference verifier that shares no serializer code path.

## 3. Required fixture package

The canonical fixture corpus MUST include:

```text
F00 minimal unskinned triangle
F01 indexed quad split into two triangles
F02 two meshes and two materials
F03 single-root skinned mesh
F04 multi-root skeleton
F05 more-than-four influences on one vertex
F06 more-than-255 synthetic provider-neutral influences on one vertex
F07 non-identity mesh-to-asset bind matrix
F08 non-uniform scale in bind chain
F09 shear in bind chain
F10 reflection in bind chain
F11 blendshape dense deltas
F12 blendshape sparse deltas
F13 all-zero blendshape frame
F14 F64 provider fixture
F15 optional extension fixture
F16 required extension fixture
F17 canonical material/resource fixture
F18 duplicate-looking sibling source objects with distinct source ordinals
F19 identity-map rename fixture
F20 deliberately malformed adversarial corpus
```

Every valid fixture MUST include expected:

```text
CanonicalObjectIds
manifest canonical bytes
manifest ArtifactId
blob byte sequences
blob ArtifactIds
structural-verifier result
semantic object counts
```

## 4. Identity tests

### ID-001 — independent first import

**Given:** identical source bytes, source-adapter contract ID, source-object keys, and canonicalisation contract in two clean Windows workspaces.

**Require:** every `CanonicalObjectId`, blob byte sequence, blob `ArtifactId`, manifest byte sequence, and root `ArtifactId` is identical.

**Rejects:** random or machine-local canonical IDs.

### ID-002 — path independence

Move the same source and repository to different absolute Windows paths.

**Require:** canonical identities are unchanged. Execution records may differ.

### ID-003 — timestamp independence

Change source and workspace timestamps without changing bytes.

**Require:** canonical identities are unchanged.

### ID-004 — host/process independence

Run on hosts with different names, users, process IDs, temporary directories, and execution IDs.

**Require:** semantic asset and receipt identities are unchanged.

### ID-005 — deterministic duplicate siblings

Use two otherwise identical unnamed sibling meshes distinguished only by source-preserved ordinal.

**Require:** unique, repeatable `CanonicalObjectId` values.

### ID-006 — duplicate source key failure

Provide two same-kind objects with the same `SourceObjectKey`.

**Require:** P3 `FAIL` with `DUPLICATE_SOURCE_OBJECT_KEY`; no suffix or random recovery.

### ID-007 — forced collision handling

Use an injected test digest provider that maps two distinct preimages to one digest.

**Require:** `CANONICAL_OBJECT_ID_COLLISION`; no canonical asset emitted.

### ID-008 — one source byte changes

Change one source vertex component.

**Require:** the affected position blob and root asset `ArtifactId` change. Unaffected object IDs remain stable when source-object keys remain stable.

### ID-009 — rename without identity map

Rename one joint in a new source revision and supply no continuity evidence.

**Require:** no cross-revision lineage claim is inferred from name similarity.

### ID-010 — rename with identity map

Supply explicit `SameEntity` evidence linking old and new joint object IDs.

**Require:** content identity changes; the shared `LineageId` is preserved externally.

### ID-011 — unresolved matching

Provide two plausible target objects for one prior object.

**Require:** `Unresolved`; no guessed mapping.

### ID-012 — lineage excluded from asset hash

Change only the `IdentityMapV1` or `LineageId` assignment.

**Require:** canonical asset bytes and `ArtifactId` remain unchanged.

## 5. Manifest and JCS tests

### JCS-001 — property insertion order

Construct the same semantic manifest through multiple object insertion orders.

**Require:** identical RFC-8785 bytes and hash.

### JCS-002 — noncanonical whitespace

Provide semantically equivalent JSON with whitespace.

**Require:** reader may parse for diagnostics, but canonical verifier returns `NONCANONICAL_SERIALIZATION` until bytes equal JCS output.

### JCS-003 — lexical negative zero

Provide `-0` in input JSON.

**Require:** noncanonical document rejected; canonical writer emits `0`.

### JCS-004 — duplicate property

Provide duplicate JSON object properties.

**Require:** reject before semantic interpretation.

### JCS-005 — malformed Unicode

Provide a lone surrogate or malformed UTF-8 sequence.

**Require:** canonicalisation fails; no `ArtifactId` emitted.

### JCS-006 — Unicode preservation

Use canonically distinct but visually similar Unicode strings.

**Require:** strings are not normalised; distinct input strings produce distinct semantic bytes.

### JCS-007 — integer safe range

Use a count or byte offset greater than `2^53 - 1`.

**Require:** structural reject.

### JCS-008 — own-hash exclusion

Attempt to insert root `ArtifactId` into `canonical.asset.json`.

**Require:** schema reject; root hash is external.

## 6. Binary and accessor tests

### BIN-001 — endian golden vectors

Serialize known I16, U32, F32, and F64 values.

**Require:** exact little-endian golden bytes.

### BIN-002 — matrix scalar order

Serialize a MAT4 containing values 0 through 15 assigned by `(row,column)`.

**Require exact order:**

```text
m00 m10 m20 m30
m01 m11 m21 m31
m02 m12 m22 m32
m03 m13 m23 m33
```

### BIN-003 — language-layout independence

Serialize equivalent matrices from two C# representations with different internal layouts.

**Require:** identical canonical bytes.

### BIN-004 — one blob per dense accessor

Provide a valid dense accessor represented through a shared/interleaved view.

**Require:** canonical writer repacks it to one dedicated blob/view; canonical verifier rejects interleaved canonical output.

### BIN-005 — exact dense length

Add trailing capacity bytes to an accessor blob.

**Require:** reject because blob length exceeds `Count * elementByteSize`.

### BIN-006 — nonzero padding

Place nonzero bytes in storage-container padding.

**Require:** storage verifier rejects the container. Semantic blob remains defined only by canonical bytes.

### BIN-007 — negative zero

Provide IEEE negative zero in F32/F64 blob data.

**Require:** canonical verifier rejects; canonical writer materialises positive zero.

### BIN-008 — non-finite float

Provide NaN, positive infinity, or negative infinity in geometry, matrix, weight, or delta data.

**Require:** structural reject.

### BIN-009 — subnormal preservation

Provide finite F32/F64 subnormal values.

**Require:** exact bit preservation.

### BIN-010 — checked overflow

Craft values that overflow `count * elementSize`, `offset + length`, or sparse-size arithmetic.

**Require:** deterministic structural reject without allocation or out-of-range read.

### BIN-011 — hash mismatch

Alter one blob byte without changing its manifest `ArtifactId`.

**Require:** `MISSING_OR_CORRUPT_ARTIFACT`/hash mismatch; asset not accepted.

### BIN-012 — compression independence

Store the same semantic blob with two codecs or compressor versions.

**Require:** same semantic `ArtifactId`, different storage-object IDs, correct `expandsTo` relation.

## 7. Mesh tests

### MESH-001 — minimal triangle

Verify F00.

**Require:** `POSITION` F32/F64 VEC3, U16 index accessor, one binding, exact bounds, clockwise front-face fixture.

### MESH-002 — missing position

Remove `POSITION`.

**Require:** structural reject.

### MESH-003 — attribute count mismatch

Make normal count differ from vertex count.

**Require:** structural reject.

### MESH-004 — index out of range

Use index equal to `VertexCount`.

**Require:** structural reject.

### MESH-005 — nontriangle count

Use index count not divisible by three.

**Require:** structural reject.

### MESH-006 — unsupported topology

Use lines, points, strips, or fans.

**Require:** canonical v1 structural reject.

### MESH-007 — index-width minimum

Use U32 for a mesh whose maximum index is 65535 or less.

**Require:** noncanonical reject. At maximum index 65536, U32 is required.

### MESH-008 — bounds reconstruction

Change declared bounds without changing positions.

**Require:** structural reject.

### MESH-009 — primitive order

Swap semantically ordered source submesh slots.

**Require:** root asset identity changes and semantic diff reports primitive-order change.

### MESH-010 — missing/duplicate mesh binding

Provide zero or two bindings for one mesh.

**Require:** structural reject.

## 8. Skeleton, bind, and matrix tests

### BIND-001 — identity bind

Use one root joint and identity mesh binding.

**Require:** inverse bind equals inverse root bind within zero mathematical error in golden data.

### BIND-002 — nonidentity mesh root

Use translation/rotation in `MeshToAssetBindMatrix`.

**Require:** P9 equation reconstructs expected inverse bind.

### BIND-003 — parent-child composition

Use root and child matrices with known product.

**Require:** `JointToAssetBind[child] = parent * local` under column-vector convention.

### BIND-004 — altered mesh bind

Keep bones, weights, and inverse bind values constant but alter `MeshToAssetBindMatrix`.

**Require:** asset identity changes and P9 relation fails.

### BIND-005 — local rest count mismatch

Make local-rest matrix count differ from joint count.

**Require:** structural reject.

### BIND-006 — inverse bind count mismatch

Make inverse-bind count differ from skin-joint count.

**Require:** structural reject.

### BIND-007 — parent ordering

Place child before parent.

**Require:** noncanonical reject.

### BIND-008 — cycle

Create joint-parent cycle.

**Require:** structural reject.

### BIND-009 — root/sibling ordering

Provide otherwise valid hierarchy with roots/siblings not sorted by `CanonicalObjectId`.

**Require:** noncanonical reject.

### BIND-010 — singular skinned matrix

Use a singular joint bind matrix in a required bind chain.

**Require:** structural reject.

### BIND-011 — non-uniform scale

Use nonsingular non-uniform scale.

**Require:** representable; no forced TRS normalization.

### BIND-012 — shear

Use nonsingular affine shear.

**Require:** representable; matrix preserved.

### BIND-013 — reflection

Use nonsingular reflected bind basis.

**Require:** representable; canonicalisation evidence must record reflection/winding implications.

### BIND-014 — nonaffine matrix

Use perspective/nonaffine final row.

**Require:** structural reject.

## 9. Skin tests

### SKIN-001 — more than four influences

Use five or more influences on a vertex.

**Require:** round-trip without truncation.

### SKIN-002 — more than 255 influences

Use a synthetic provider-neutral fixture with more than 255 influences on one vertex within selected security policy.

**Require:** canonical representation succeeds; a Unity adapter may separately report materialisation capability `INDETERMINATE`/unsupported.

### SKIN-003 — offsets length

Use offsets count other than `VertexCount + 1`.

**Require:** structural reject.

### SKIN-004 — nonzero first offset

**Require:** structural reject.

### SKIN-005 — descending offset

**Require:** structural reject.

### SKIN-006 — terminal count mismatch

Make final offset differ from influence-array counts.

**Require:** structural reject.

### SKIN-007 — joint ordinal range

Use joint ordinal equal to joint count.

**Require:** structural reject.

### SKIN-008 — duplicate joint per vertex

Repeat a joint ordinal within one vertex slice.

**Require:** structural reject; no silent summation.

### SKIN-009 — noncanonical influence order

Order a smaller weight before a larger weight or violate ordinal tie-break.

**Require:** noncanonical reject.

### SKIN-010 — zero/negative/non-finite weight

**Require:** structural reject.

### SKIN-011 — observed nonunit sum

Use finite positive weights whose sum is not one.

**Require:** asset remains structurally representable; policy evaluation determines gate result. P3 must not change values.

### SKIN-012 — explicit normalization

Run an adaptation that normalizes SKIN-011.

**Require:** new asset identity, old/new values evidenced, precision effect recorded where applicable.

### SKIN-013 — joint ordinal width

Use U32 with joint count at or below 65535.

**Require:** noncanonical reject. Above that count, U32 required subject to security policy.

## 10. Sparse and blendshape tests

### SPARSE-001 — all-zero frame

**Require:** no base view and no sparse descriptor.

### SPARSE-002 — sparse smaller than dense

**Require:** sparse output using minimum legal index width.

### SPARSE-003 — dense tie

When dense and sparse byte lengths are equal, require dense output.

### SPARSE-004 — sparse larger than dense

Require dense output.

### SPARSE-005 — unordered indices

**Require:** structural reject.

### SPARSE-006 — duplicate indices

**Require:** structural reject.

### SPARSE-007 — out-of-range index

**Require:** structural reject.

### SPARSE-008 — nested sparse attempt

Attempt to represent sparse indices/values as ordinary or sparse accessors.

**Require:** schema/structural reject.

### SPARSE-009 — stride on sparse view

**Require:** structural reject.

### SPARSE-010 — delta precision mismatch

Use F64 base positions with F32 position deltas.

**Require:** structural reject unless an explicit extension contract owns a different semantic.

### BLEND-001 — frame ordering

Provide frames out of ascending weight/ID order.

**Require:** noncanonical reject.

### BLEND-002 — delta count mismatch

**Require:** structural reject.

## 11. Material, resource, and extension tests

### MAT-001 — known generic contract

Round-trip a material with scalar, vector, boolean, integer, string, and resource-binding values.

**Require:** identical semantic bytes across implementations.

### MAT-002 — source-specific shader data

Attempt to insert unclassified provider shader properties as universal properties.

**Require:** reject or require a namespaced extension.

### RES-001 — external URI

Insert file/network URI into semantic resource data.

**Require:** schema/structural reject.

### RES-002 — resource byte mismatch

**Require:** hash/length reject.

### RES-003 — unspecified texture interpretation

Use a texture binding whose resource interpretation is `UNSPECIFIED`.

**Require:** storage remains valid; dependent visual operation is `INDETERMINATE` under policy.

### EXT-001 — optional extension round-trip

Use an unknown optional extension represented by a canonical JSON artifact.

**Require:** reader preserves reference exactly when rewriting/migrating.

### EXT-002 — required unknown extension

**Require:** core storage can be inspected, but semantic interpretation returns `UNSUPPORTED_EXTENSION`/pipeline `INDETERMINATE`.

### EXT-003 — extension property order

Canonicalize semantically identical extension JSON with different insertion order.

**Require:** same extension `ArtifactId`.

### EXT-004 — extension overrides core

Provide an extension claiming a different vertex count, coordinate system, or bind equation.

**Require:** reject under extension/core conflict rule.

### EXT-005 — Kandra ignorance

Verify a complete valid armour asset with no Kandra extension.

**Require:** P3/P4 core validity succeeds. Kandra capability remains a separate P12 concern.

## 12. Security-policy tests

### SEC-001 — policy-independent structural validity

Verify a structurally valid large fixture under two security policies.

**Require:** structural result identical; one policy may return `LIMIT_EXCEEDED` while the other accepts.

### SEC-002 — manifest size ceiling

**Require:** limit result before unbounded parsing/allocation.

### SEC-003 — blob size ceiling

**Require:** limit result before full allocation where possible.

### SEC-004 — hierarchy depth ceiling

**Require:** deterministic `LIMIT_EXCEEDED`, not stack overflow.

### SEC-005 — influence ceiling

**Require:** deterministic `LIMIT_EXCEEDED`, not schema corruption.

### SEC-006 — missing referenced artifact

**Require:** `MISSING_ARTIFACT`, no partial acceptance.

### SEC-007 — reference type confusion

Reference a mesh ID where an accessor ID is required.

**Require:** structural reject.

### SEC-008 — prohibited field injection

Inject absolute paths, timestamps, execution IDs, or receipts into the canonical manifest.

**Require:** schema reject through `additionalProperties: false` or field prohibition.

## 13. Precision and loss tests

### NUM-001 — Unity F32 observation

Provide known F32 bit patterns from a Unity-style adapter.

**Require:** exact bit preservation after coordinate operations that are identity.

### NUM-002 — genuine F64 provider

Provide F64 values not exactly representable as F32.

**Require:** no implicit downcast; exact F64 round-trip.

### NUM-003 — explicit F64-to-F32 materialisation

**Require:** new asset identity and `LossRecordV1` quantifying affected values/error.

### NUM-004 — policy threshold change

Change only a validation tolerance policy.

**Require:** canonical asset identity unchanged; affected receipt/cache identity changes.

### NUM-005 — deterministic reduction

Run authoritative reduction with different worker schedules.

**Require:** identical result and receipt bytes.

## 14. Evidence and authorisation tests

### EVD-001 — stage receipt isolation

Run the same P9 validation under two P12 authorisation policies.

**Require:** same P9 semantic receipt; different P12 `AuthorisationDecisionV1` where policy differs.

### EVD-002 — repeated physical execution

Execute the same stage twice.

**Require:** same semantic receipt ID; different `ExecutionId` values.

### EVD-003 — execution telemetry change

Change host, process, timestamps, paths, and log locations.

**Require:** semantic receipt identity unchanged.

### EVD-004 — Kandra capability absent

Use a valid canonical asset with no proven converter.

**Require:** P12 `INDETERMINATE`; P13 not invoked; canonical asset remains valid.

## 15. Interoperability acceptance

For every valid fixture:

```text
Writer A output -> Reader B -> Writer B
```

MUST produce:

```text
same canonical manifest bytes
same semantic blob bytes
same CanonicalObjectIds
same ArtifactIds
```

and the reverse direction MUST also pass.

For every invalid fixture, independent verifiers MUST agree on the primary error class:

```text
INVALID
UNSUPPORTED_EXTENSION
LIMIT_EXCEEDED
MISSING_ARTIFACT
NONCANONICAL_SERIALIZATION
```

Diagnostic wording may differ. Diagnostic codes used for authoritative receipts will be frozen in R3.

## 16. R2 exit criteria

R2 is not complete until:

- all golden fixture bytes and hashes are checked into the R2 evidence surface;
- identity tests pass in independent clean Windows workspaces;
- canonical JSON tests pass against RFC-8785 vectors relevant to the schema;
- binary golden vectors pass for every component/shape used by v1;
- every malformed fixture fails without unbounded allocation or unchecked arithmetic;
- sparse/dense choice is identical across independent implementations;
- matrix/bind fixtures reconstruct identically;
- F64 preservation and F32 Unity policy are both demonstrated;
- extension round-trip and Kandra ignorance tests pass;
- semantic receipts remain separate from execution records and authorisation decisions;
- no production or runtime claim is made from serializer conformance alone.

Until those results exist, the R1 contract is a decision, not a verified implementation.