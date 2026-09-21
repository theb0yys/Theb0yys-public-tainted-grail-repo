# R2 Windows Validation Blocker — Runner and Billing Availability

Document control:

- Status: Current blocker receipt
- Workstream: R2 — Deterministic Serialization, Identity and Content-Addressing Pipeline
- Date: 2026-08-10
- Implementation pull request: `#84` — merged
- Merge commit: `b526a3c89a3bfd6e09d1fde623c2f055a00a9c8d`
- Validation-closure pull request: `#86` — draft
- Validation-closure branch: `agent/tainted-armour-r2-validation-closure`
- Workflow: `Tainted Armour R2 Windows Validation`

## Required validation

The Windows-only lane is intended to execute the R2-owned canonical/serialization closure only:

1. Windows `dotnet` SDK context capture, requiring an installed SDK capable of building and running the R2 target frameworks without requiring a specifically installed SDK `8.x`;
2. Release builds of the provider-neutral canonical and serialization projects plus the R2 serialization fixture executable;
3. the R2 synthetic fixture corpus in two independent Windows workspaces;
4. RFC-8785 Appendix B and UTF-16 ordering vectors;
5. equality comparison of the two emitted canonical manifest identities.

## Attempt 1 — GitHub-hosted Windows

```text
workflow run ID  31415196785
workflow job ID  93542440833
commit           773bb11a2b81b599742bacb6c1581f3711ed13de
```

GitHub created the run and job record but did not assign a runner. The job reported that recent account payments had failed or the spending limit needed to be increased.

Observed execution state:

```text
runner assigned            NO
steps created              NO
preflight executed         NO
projects built             NO
new fixtures executed      NO
existing fixtures executed NO
manifest identities        NOT PRODUCED
```

The GitHub conclusion was `failure`, but no source, build, fixture, or repository-governance command executed. This is external account/billing infrastructure evidence, not a code-test result.

## Attempt 2 — repository self-hosted Windows labels

The workflow was routed to repository self-hosted Windows labels. Multiple jobs were created but remained queued without beginning execution.

Latest validation-closure attempt:

```text
pull request       #86
head commit        6feaaf32f3df411e617ad9176822eaaa8d49e922
workflow run ID    31420500546
workflow job ID    93559792927
runner labels      self-hosted, Windows
observed status    queued
steps executed     ZERO
```

No self-hosted Windows runner had accepted the job at the time of this record.

## Repository merge state

PR `#84` merged the R2 implementation into `main` before the required Windows lane executed.

The merge itself establishes only that the source exists in repository history. It does not establish:

```text
compilation
fixture success
RFC-8785 conformance
binary verifier correctness
cross-workspace determinism
R2 completion
```

The merge commit has no attached passing CI status for the R2 lane.

## Static review performed

Static review corrected defects without claiming execution evidence:

- replaced potentially non-portable `IReadOnlyDictionary` copy constructors in the `netstandard2.0` contract project;
- aligned coordinate, accessor, primitive, mesh-bounds, and mesh-binding fields with the filed R1 schema;
- corrected all-zero storage selection to the R1-defined implicit-zero representation;
- selected sparse index width from the maximum changed index and rejected counts beyond U32 addressability;
- added duplicate source/generated canonical-key rejection and injected digest-collision tests;
- added RFC-8785 Appendix B and UTF-16 property-order fixtures;
- prevented fixture cleanup from deleting the caller-supplied workspace root;
- pinned workflow action dependencies;
- replaced implicit bounds-array node conversions with explicit `JsonValue.Create(...)` calls in PR `#86`.

These are source-review findings only. They do not substitute for compilation or fixture execution.

## Attempt 3 — controlled local Windows validation on the viable machine

Date/time: `2026-08-11T01:48:34.3856523+01:00`

Tested source commit:

```text
edcae2e7cb2703e111a39d49bf0ff8182e24bf6c
```

Validation definition note:

```text
The R2 lane was corrected in the working tree before execution to match the current R2-owned canonical/serialization boundary:
- no SDK-8-specific requirement;
- no repository-governance preflight in the R2 closure lane;
- no frozen A2K/importer core fixture in the R2 closure lane;
- explicit Release build of the serialization fixture executable before --no-build fixture runs.
```

Windows/toolchain context:

```text
COMPUTERNAME              DESKTOP-M5TON4G
OS                        Microsoft Windows NT 10.0.26200.0
PROCESSOR_ARCHITECTURE    AMD64
git                       2.54.0.windows.1
dotnet SDK                10.0.302
dotnet host               10.0.10 x64
python                    3.14.3
```

Executed R2-owned Release builds:

```text
dotnet build mods\tainted-armour\src\Tainted.Armour.Canonical\Tainted.Armour.Canonical.csproj -c Release
result: PASS, 0 warnings, 0 errors

dotnet build mods\tainted-armour\src\Tainted.Armour.Serialization\Tainted.Armour.Serialization.csproj -c Release
result: PASS, 0 warnings, 0 errors

dotnet build mods\tainted-armour\tests\Tainted.Armour.Serialization.Fixtures\Tainted.Armour.Serialization.Fixtures.csproj -c Release
result: PASS, 0 warnings, 0 errors
```

Executed R2 fixture corpus:

```text
workspace A: %TEMP%\tainted-armour-r2-a
workspace B: %TEMP%\tainted-armour-r2-b

dotnet run --no-build --project mods\tainted-armour\tests\Tainted.Armour.Serialization.Fixtures\Tainted.Armour.Serialization.Fixtures.csproj -c Release -- --workspace <workspace>
```

Both workspace runs passed:

- canonical object ID duplicate and collision rejection;
- RFC-8785 Appendix B and UTF-16 ordering vectors;
- canonical object ID derivation;
- SHA-256 known vector;
- RFC-8785 property ordering;
- RFC-8785 number vectors;
- canonical JSON verifier;
- manifest order independence;
- R1 manifest structure;
- MAT4 column-major scalar serialization;
- negative-zero normalization;
- non-finite rejection;
- independent binary verifier;
- implicit-zero accessor contract;
- deterministic sparse/dense selection;
- content-addressed blob storage;
- cross-workspace repeatability.

Manifest identity:

```text
workspace A R2_CANONICAL_MANIFEST=sha256:d95ae2176a125b85159fd8c46a84d6b3ec16f13265a0f45e60e258bbc0588f14
workspace B R2_CANONICAL_MANIFEST=sha256:d95ae2176a125b85159fd8c46a84d6b3ec16f13265a0f45e60e258bbc0588f14
manifest equality: PASS
```

## Classification

```text
validation outcome       PASSED FOR R2 CANONICAL/SERIALIZATION CLOSURE
blocker class            CLOSED FOR R2-OWNED BUILD/FIXTURE LANE
implementation on main   YES
source build status      PASS
fixture status           PASS
R2 completion            AUTHORISED FOR THE R2 CANONICAL/SERIALIZATION BOUNDARY ONLY
```

## Required closure

This blocker closes for the R2-owned canonical/serialization boundary when a controlled Windows environment executes the intended lane and records all commands and outcomes.

Required closure evidence:

- exact Windows runner identity and .NET/Python/git context, including the installed `dotnet` SDK used to build and run the R2 projects;
- successful Release builds;
- successful R2 fixture corpus, including RFC-8785 and collision vectors;
- identical `R2_CANONICAL_MANIFEST` output from two independent workspace roots;
- durable recorded output attached to the exact tested commit or working-tree validation definition.

Those results now exist for the R2-owned canonical/serialization lane. This does not validate Unity import, source/target compatibility, adaptation, deformation algorithms, Kandra conversion, packaging, registration, equip, save mutation, deployment, release, or the frozen A2K/importer track.
