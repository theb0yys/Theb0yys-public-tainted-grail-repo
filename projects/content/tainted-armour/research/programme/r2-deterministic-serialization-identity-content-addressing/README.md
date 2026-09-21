# R2 — Deterministic Serialization, Identity and Content-Addressing Pipeline

Document control:

- Status: **R2 canonical/serialization closure passed on the controlled local Windows machine; complete for the R2-owned boundary only**
- Workstream: R2
- Programme: `../README.md`
- Governing contract: `../r1-canonical-ir-v1-contract-decision/canonical-armour-ir-v1-contract-decision-2026-08-10.md`
- Governing conformance suite: `../r1-canonical-ir-v1-contract-decision/canonical-armour-ir-v1-conformance-suite-2026-08-10.md`
- Product owner: `mods/tainted-armour/`
- Implementation pull request: `#84` — merged
- Merge commit: `b526a3c89a3bfd6e09d1fde623c2f055a00a9c8d`
- Validation-closure branch: `agent/tainted-armour-r2-validation-closure`
- Last reviewed: 2026-08-11

## Purpose

R2 implements and proves the deterministic provider-neutral serialization boundary selected by R1.

The bounded implementation slice covers:

```text
CanonicalObjectId derivation
SHA-256 ArtifactId calculation
RFC-8785 canonical semantic JSON
canonical JSON verification
one-blob-per-accessor binary writing
independent binary verification
MAT4 scalar serialization
negative-zero normalization
NaN/Infinity rejection
deterministic implicit-zero/dense/sparse selection
content-addressed semantic blob storage
independent Windows workspace repeatability
```

R2 does not own Unity import, source/target compatibility, adaptation, deformation algorithms, Kandra conversion, packaging, registration, equip, save mutation, deployment, or release.

## Merged implementation boundary

```text
mods/tainted-armour/src/Tainted.Armour.Canonical/
    provider-neutral v1 identity and bounded canonical contracts

mods/tainted-armour/src/Tainted.Armour.Serialization/
    deterministic identity, JSON, binary, hashing, and blob-store implementation

mods/tainted-armour/tests/Tainted.Armour.Serialization.Fixtures/
    synthetic fixture executable and conformance vectors

.github/workflows/tainted-armour-r2-windows.yml
    self-hosted Windows canonical/serialization build and repeatability lane
```

The canonical contract project targets `netstandard2.0`. The serializer and fixture projects target `net8.0` and remain independent from Unity and FoA assemblies. The Windows validation lane requires an installed `dotnet` SDK capable of building and running those target frameworks; it must not require a specifically installed SDK `8.x` when a newer SDK on the controlled Windows machine can build the same projects.

## Implemented capabilities

The merged slice contains:

- source-object and generated-object `CanonicalObjectId` derivation using the R1 domain-separated preimages;
- duplicate source/generated key rejection and explicit digest-collision detection;
- algorithm-tagged SHA-256 `ArtifactId` values;
- RFC-8785/JCS object, string, and finite binary64 number canonicalisation for the semantic-manifest domain;
- exact R1 coordinate-system fields and canonical manifest nulls;
- indexed primitives, mesh bounds, and one explicit mesh binding per mesh;
- canonical little-endian F32, F64, U16, U32, and MAT4 writers;
- independent byte-length, digest, finite-value, and negative-zero verification;
- deterministic implicit-zero, dense, and sparse representation selection;
- immutable content-addressed storage under `blobs\sha256-<digest>.bin`;
- RFC-8785 Appendix B and UTF-16 ordering fixtures;
- insertion-order and workspace-path independence fixtures;
- pinned workflow actions and a self-hosted Windows-only validation route.

The JSON implementation is not treated as certified merely because the source is present. Its conformance vectors must execute successfully on Windows before R2 may be promoted.

## Validation state

The intended Windows lane performs the R2-owned canonical/serialization closure only:

```powershell
dotnet build mods\tainted-armour\src\Tainted.Armour.Canonical\Tainted.Armour.Canonical.csproj -c Release
dotnet build mods\tainted-armour\src\Tainted.Armour.Serialization\Tainted.Armour.Serialization.csproj -c Release
dotnet build mods\tainted-armour\tests\Tainted.Armour.Serialization.Fixtures\Tainted.Armour.Serialization.Fixtures.csproj -c Release

dotnet run --no-build --project mods\tainted-armour\tests\Tainted.Armour.Serialization.Fixtures\Tainted.Armour.Serialization.Fixtures.csproj -c Release -- --workspace <workspace-a>
dotnet run --no-build --project mods\tainted-armour\tests\Tainted.Armour.Serialization.Fixtures\Tainted.Armour.Serialization.Fixtures.csproj -c Release -- --workspace <workspace-b>
```

Observed results:

1. A GitHub-hosted Windows job was rejected before runner assignment because of an account billing/spending-limit restriction. No command executed.
2. Subsequent jobs targeting repository self-hosted Windows labels remained queued; no runner began execution.
3. The merge commit has no attached passing CI status.
4. Controlled local Windows validation on `DESKTOP-M5TON4G` executed the R2-owned canonical/serialization lane with `dotnet` SDK `10.0.302`; the canonical, serialization, and serialization-fixture Release builds passed with zero warnings and zero errors; two independent fixture workspaces passed and emitted identical `R2_CANONICAL_MANIFEST=sha256:d95ae2176a125b85159fd8c46a84d6b3ec16f13265a0f45e60e258bbc0588f14`.

Therefore:

```text
implementation present on main       YES
static R1/schema review performed    YES
Windows build observed               YES
fixture execution observed           YES
RFC-8785 vectors observed            YES
cross-workspace equality observed    YES
R2 complete                          YES, for the R2-owned canonical/serialization boundary only
```

The local Windows validation evidence is recorded in `r2-windows-validation-blocker-2026-08-10.md`. This does not validate Unity import, source/target compatibility, adaptation, deformation algorithms, Kandra conversion, packaging, registration, equip, save mutation, deployment, release, or the frozen A2K/importer track.

## Promotion conditions

R2 may be marked complete only after all of the following are observed on Windows:

1. Windows `dotnet` SDK context is captured and the installed SDK can build/run the R2 target frameworks;
2. the R2-owned library and fixture projects build in Release configuration;
3. the synthetic fixture executable exits zero;
4. RFC-8785 Appendix B and UTF-16 ordering fixtures pass;
5. duplicate/collision, malformed JSON, non-finite, negative-zero, sparse-selection, and corrupt-store rejection fixtures pass;
6. two independent workspace runs emit identical `R2_CANONICAL_MANIFEST` identities;
7. the implementation remains within the R1 contract boundary.

Those conditions are satisfied for the R2-owned canonical/serialization boundary only. R3 may consume R2 serialization outputs inside that boundary, but no later Unity, Kandra, runtime, item/equip, save, or promotion work is validated by R2.
