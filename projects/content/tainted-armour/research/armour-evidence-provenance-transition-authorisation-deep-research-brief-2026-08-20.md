# ChatGPT Deep Research Brief — Tainted Armour Evidence, Provenance, and Transition-Authorisation Architecture

## 1. Control block

- Date: 2026-08-20
- Requesting task: continue the corrected Tainted Armour research programme after H0 returned-report intake
- Repository: `theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods`
- Inspected branch: `tainted-armour`
- Inspected branch head before this brief: `e4ffe552070e907fb67563c398eab95b60d3dd11`
- Pull request: draft PR `#350`, `tainted-armour` -> `main`
- Product owner: `mods/tainted-armour/`
- Research owner and exact destination: `documents/research/frameworks/tainted-armour/`
- Protected paths affected: none
- Current authority chain:
  1. current user instruction;
  2. root `AGENTS.md`;
  3. `documents/AGENTS.md`;
  4. `documents/research/AGENTS.md`;
  5. `documents/process/research-lifecycle.md`;
  6. `documents/process/research-standard/README.md`;
  7. `documents/process/research-standard/deep-research-brief-standard.md`;
  8. `documents/process/research-standard/deep-research-report-intake-standard.md`;
  9. `documents/research/frameworks/tainted-armour/h0-armour-framework-lifecycle-hypothesis-review-cleaned-2026-08-20.md`.
- Intended consumer: H0 lifecycle correction, the eventual Tainted Armour research graph, and later bounded stage-contract research
- Intended usage lane: architecture research and planning only
- Deep Research execution scope: information-gathering, source inspection, comparison, contradiction analysis, and synthesis only
- Returned-report follow-on gate: review, clean, store in the same GitHub research owner, update its index, commit on `tainted-armour`, and update draft PR `#350` before any next action
- Maximum allowed use of this brief or returned report before review: `E1` research context only

## 2. Trigger and blocker

### Immediate research question

What is the most defensible, minimal, auditable, fail-closed evidence architecture for Tainted Armour to record immutable inputs, semantic artefacts, physical executions, policies, algorithms, evidence, independent verification, review, authorisation, correction, invalidation, and human promotion without allowing tests, implementation outputs, logs, agents, or stages to authorise themselves?

### Why available information is insufficient

The existing lifecycle and Canonical Armour IR research propose:

- deterministic stage receipts;
- separate physical execution records;
- provenance graphs;
- content-addressed identity;
- authorisation decisions;
- cache and invalidation concepts;
- explicit `PASS`, `FAIL`, and `INDETERMINATE` states.

H0 retained these as useful architectural hypotheses, but did not prove the correct object model, trust model, transition-decision model, signature model, correction model, or placement of authorisation. The current documents contain project-authored proposals and narrow implementation history. They do not establish that the proposed design is minimal, complete, non-circular, resistant to stale evidence, or superior to established provenance and attestation models.

H0 also corrected an overclaim: W3C PROV supports lifecycle-wide provenance, but does not decide whether Tainted Armour should use one explicit conversion-authorisation stage, a cross-cutting policy evaluator, explicit decision artefacts at every consequential transition, or a hybrid of those structures.

### Consequence of a wrong answer

A defective evidence architecture could allow any of the following invalid transitions:

```text
implementation exists
→ therefore design is valid

test passes
→ therefore native semantics are correct

payload generated
→ therefore conversion is authorised

renderer registered
→ therefore armour visibly renders

armour rendered once
→ therefore item/equip/save/teardown are valid

new game build installed
→ old target evidence remains current

agent recommendation
→ implementation permission
```

It could also produce irreproducible semantic hashes, circular provenance, evidence that cannot be independently inspected, unbounded authorisations, silent corrections, invalid cache reuse, or release decisions unsupported by current evidence.

### Required review level after the report returns

- `RH0`: returned-report intake and source preservation
- `RH1`: claim-level evaluation against inspected underlying sources
- `RH2`: domain review for provenance, attestations, reproducible builds, policy evaluation, and asset-pipeline applicability
- `RH3`: independent review of the recommended evidence and authorisation model
- `RH4`: validation review before any schema, policy engine, or coded gate gains action-level authority
- `RH5`: named human promotion for any current architecture decision

## 3. Scope and non-scope

### In-scope information-gathering questions

1. How should the framework distinguish:
   - immutable source artefacts;
   - canonical or other semantic artefacts;
   - target payloads and packages;
   - evidence items;
   - stage assertions;
   - independent verification results;
   - physical execution attempts;
   - policies and algorithms;
   - human and agent identities;
   - review findings;
   - transition-authorisation decisions;
   - promotion decisions?

2. Which established models are applicable, including:
   - W3C PROV-DM and PROV-O;
   - in-toto attestations and layouts;
   - SLSA provenance;
   - DSSE or equivalent signed-envelope models;
   - The Update Framework trust/delegation concepts where relevant;
   - reproducible-build definitions and practices;
   - Nix derivations/store identity;
   - Bazel action keys, content-addressable storage, and remote-execution metadata;
   - Sigstore identity/signing/verification concepts;
   - NIST secure-development and supply-chain guidance;
   - policy-as-code and fail-closed policy evaluation, using primary specifications or official documentation.

3. How should semantic identity differ from:
   - source-local identity;
   - lineage across revisions;
   - exact byte identity;
   - policy identity;
   - algorithm identity;
   - target-build identity;
   - physical execution identity?

4. What is the minimum provenance graph required to answer:
   - which source bytes produced an artefact;
   - which interpreter and settings were used;
   - which target profile was consumed;
   - which adaptation plan was applied;
   - which algorithms and policies were active;
   - which independent verifier accepted or rejected it;
   - which exact runtime build was observed;
   - who reviewed and authorised the next operation;
   - which evidence later invalidated or superseded that decision?

5. How should deterministic semantic receipts be separated from non-deterministic run telemetry?

6. How should `PASS`, `FAIL`, `INDETERMINATE`, `UNSUPPORTED`, `NOT_RUN`, and `NOT_APPLICABLE` interact with evidence maturity and permission?

7. How should a stage result be prevented from self-authorising its own consumer or next transition?

8. Which transition model is best:
   - one explicit conversion-authorisation stage;
   - a cross-cutting policy evaluator;
   - explicit decision artefacts at every consequential transition;
   - a hybrid model?

9. Which transitions require independent or human authorisation, including:
   - interpretation to semantic capture;
   - compatibility to adaptation;
   - adaptation to target encoding;
   - payload verification to package construction;
   - package verification to runtime loading;
   - render proof to item/equip mutation;
   - equip proof to save mutation;
   - complete evidence to release promotion?

10. How should the model prevent:
    - circular provenance;
    - circular attestations;
    - producer and verifier collapsing into one unreviewed authority;
    - evidence laundering through polished summaries;
    - broad permissions inferred from narrow evidence;
    - stale evidence remaining actionable after a build, policy, algorithm, source, target, or schema change?

11. How should corrections, supersession, withdrawal, restoration, and version-splitting be represented without silently rewriting history?

12. What information belongs in:
    - deterministic semantic records;
    - physical execution records;
    - diagnostics;
    - review records;
    - authorisation decisions;
    - human promotion records?

13. What cryptographic properties are necessary and sufficient:
    - full-content hashes;
    - domain separation;
    - canonical encoding;
    - signatures;
    - identity binding;
    - timestamping;
    - transparency logs;
    - revocation or expiry?

14. What is the smallest evidence model that remains complete for an asset compiler without importing unnecessary software-supply-chain complexity?

15. How should the evidence architecture remain independent of the unresolved final Armour operational-stage count?

### Out-of-scope operational tasks

Deep Research must not:

- implement schemas, serializers, validators, policy engines, caches, or gates;
- edit repository files;
- run tests, builds, scripts, validators, or benchmarks;
- execute decompilers, extractors, the Unity editor, or the game;
- inspect fresh installed bytes through execution;
- generate Kandra payloads;
- mutate packages, runtime state, inventory, equipment, or saves;
- decide the final P/F stage count;
- claim implementation, runtime, save, compatibility, or release authority.

### Version/build/tool boundaries

- Repository evidence is bounded to the exact refs and blobs in the GitHub ledger below.
- Existing proprietary evidence is bounded to the exact fingerprints recorded in those documents.
- No claim may silently upgrade older recorded build evidence to the current installed game.
- External research should prefer current standards/specifications while identifying version splits and deprecated models.

## 4. GitHub citation ledger

| ID | Repository path / durable locator | Ref or blob | Lane | Evidence class | What it establishes | What it does not establish |
|---|---|---|---|---|---|---|
| EP-AUTH-01 | `documents/research/frameworks/tainted-armour/h0-armour-framework-lifecycle-hypothesis-review-cleaned-2026-08-20.md`; headings `Executive verdict`, `Corrected H0 architecture position`, `Remaining research programme`, `Implementation threshold` | branch `tainted-armour`; blob `b1af4746ea913b856bb10c8f6130dc56bb3b9134` | research context | H0 corrections, provisional architecture, remaining evidence-governance package, implementation threshold | Final evidence schema, final authorisation model, implementation permission |
| EP-AUTH-02 | `documents/research/frameworks/tainted-armour/canonical-importer-lifecycle-production-grade-armour-asset-import-pipeline-2026-08-10.md`; headings `Deterministic semantic receipts`, `Execution records are separate`, `Content-addressed cache and dependency invalidation`, `Research boundaries that remain unproven` | main baseline `adf1b1c57493b3b6d5961750ccb27032161b81ea`; blob `c1394fdbf6d7675e598422bc21ee127f6a92dfbe` | research context | Existing receipt/execution/provenance/authorisation hypothesis and admitted limits | Optimality, final object model, native correctness, action authority |
| EP-AUTH-03 | `documents/research/frameworks/tainted-armour/canonical-armour-intermediate-representation-and-provenance-pipeline-2026-08-10.md`; headings `Corrected identity model`, `Receipts, authorisation and execution`, `Provenance model`, `Policy, cache and invalidation` | same baseline; blob `f0e8f09a90ff0a74f05dd758d567f076131c0b46` | research context | Candidate identities, receipt/decision separation, provenance concepts | Accepted final evidence architecture or permission to implement |
| EP-AUTH-04 | `documents/research/frameworks/tainted-armour/programme/r1-canonical-ir-v1-contract-decision/canonical-armour-ir-v1-contract-decision-2026-08-10.md`; sections defining `CanonicalObjectId`, `LineageId`, `ArtifactId`, and target-extension boundaries | same baseline; blob `6095929a043bf7384b5ac2d0b6e2f608c82a4fde` | project-decision context | Current narrow R1 identity and semantic-contract decisions consumed by R2 | H0 representational completeness or final lifecycle placement |
| EP-AUTH-05 | `documents/research/frameworks/tainted-armour/programme/r2-deterministic-serialization-identity-content-addressing/README.md`; headings `Purpose`, `Implemented capabilities`, `Validation state`, `Promotion conditions` | same baseline; blob `3e99a9fbaae6669b2aacdb9b292532307e7b9abb` | programme/validation context | Narrow R2 mechanism history and limits | Evidence that the surrounding architecture is correct; tests/builds are excluded as research proof for H0 |
| EP-AUTH-06 | `documents/process/research-standard/README.md`; headings `Research object hierarchy`, `Independent classification axes`, `Gate sequence`, `Immediate fail-closed conditions` | repository process; inspected on branch `tainted-armour` | process authority | Claim-level review, evidence fit, permission separation, human promotion and fail-closed requirements | Native game or technical provenance facts |
| EP-AUTH-07 | `documents/process/research-standard/deep-research-brief-standard.md`; headings `Evidence-domain lanes`, `GitHub citation ledger requirement`, `Mandatory brief structure`, `Handling returned Deep Research reports` | repository process | process authority | Requirements for this brief and returned-report handling | Technical answer to the evidence architecture question |
| EP-AUTH-08 | `documents/process/research-standard/deep-research-report-intake-standard.md`; headings `Returned-report intake hard stop`, `Mandatory report review`, `GitHub persistence gate` | repository process | process authority | Required preservation, cleaning, storage, PR and follow-on locks | Technical answer or implementation authority |
| EP-AUTH-09 | `documents/official/proprietary-systems/rendering/Questline Kandra Decompiled Runtime Lifecycle.md`; symbols `KandraRendererManager.Register`, `FinalizeRegistration`, `FinalizeUnregistration`, `StreamingManager`, `SkinnedBatchRenderGroup` | main baseline; blob `b95eda0f3f31d1fbe88d023028ae6bae00e2b326` | decompilation-static | Existing examples of distinct submitted, loaded, registered, rendering, unregister and disposal transitions | Writer correctness, visible custom armour, save or release safety |
| EP-AUTH-10 | `documents/official/proprietary-systems/compatibility/Questline Exact Mono Build 24270691 Provenance and Managed Implementation Equivalence.md`; headings `Executive verdict`, `Current unresolved evidence` | main baseline; blob `2371b8e7badc4a0ab681e2675eb79a9290da09e4` | decompilation-static / recorded installation | Bounded template/save lookup and the need for exact build identity | Equip restoration, current runtime order, persistence safety or current-build closure |

## 5. Known claim ledger before Deep Research

| Claim ID | Claim | Lane needed | Current support | Disposition | Missing evidence | Maximum permitted use now |
|---|---|---|---|---|---|---|
| EP-C01 | Semantic artefact identity should be separate from physical execution identity | external research + project review | Candidate architecture and R2 mechanism history | `partially-supported` | Comparative standards review and minimal project model | Planning |
| EP-C02 | Stage assertions, authorisation decisions, and human promotion are distinct record types | external research + project review | Candidate architecture and repository process | `partially-supported` | Comparative attestation/policy review and accepted schema | Planning |
| EP-C03 | Provenance must apply across the lifecycle rather than live in one stage | external research | W3C-PROV analogy and H0 finding | `partially-supported` | Exact applicability and minimum model | Planning |
| EP-C04 | A stage result must not authorise itself | external research + project-decision review | Current user direction and repository process | `supported as governance rule` | Operational decision contract | Planning and blocking unsafe inference |
| EP-C05 | A single P12 conversion-authorisation stage should be removed | external research + project review | H0 rejected the definitive replacement claim | `unresolved` | Compare single-stage, per-transition and hybrid models | None beyond research planning |
| EP-C06 | Every consequential transition needs an explicit bounded decision | external research + project review | H0 correction and process principles | `partially-supported` | Define which transitions, decision inputs, expiry and delegation | Planning |
| EP-C07 | Evidence maturity is independent of gate result | external research + project review | Repository process and candidate architecture | `partially-supported` | Accepted vocabulary and transition semantics | Planning |
| EP-C08 | Tests, builds, logs, generated files, and agent notes cannot be technical authority by themselves | current user direction + repository process | Directly established for this programme | `supported` | No external proof needed for repository governance; technical model must enforce it | Repository research governance only |
| EP-C09 | Build, source, target, policy, algorithm, schema, or verifier changes should invalidate dependent decisions | external research + later project validation | Candidate architecture | `partially-supported` | Formal dependency/expiry model; full cache semantics deferred | Planning |
| EP-C10 | Producer and verifier independence must be represented and checked | external research + project review | H0 and research standard | `partially-supported` | Independence groups, trust roots and exception policy | Planning |
| EP-C11 | Corrections and supersession must preserve history | external research + repository process | Research lifecycle | `supported as governance rule` | Technical event/record model | Planning |
| EP-C12 | Cryptographic signatures are mandatory for every local record | external research | None sufficient | `unresolved` | Threat model, trust boundary, cost/benefit | None |
| EP-C13 | Transparency logs or timestamp authorities are required | external research | None sufficient | `unresolved` | Threat model and deployment context | None |
| EP-C14 | R2 canonical JSON/SHA-256 mechanisms belong unchanged in the final evidence architecture | external research + H0/project review | Narrow mechanisms exist; architectural placement unproven | `unresolved` | Compare alternatives and define domains | Planning only |
| EP-C15 | The evidence architecture can be finalised without knowing the final operational stage count | external research + project review | H0 leading candidate | `partially-supported` | Stage-agnostic transition model | Planning |

## 6. External-research questions

### Question family A — Provenance object model

- What do W3C PROV-DM/PROV-O, in-toto, SLSA provenance, and comparable primary specifications distinguish as artefacts/entities, activities/executions, agents/identities, plans/policies, attestations, and derivations?
- Which relations are required to express use, generation, derivation, revision, association, delegation, invalidation, and specialisation?
- Which parts map cleanly to an asset compiler and which introduce unnecessary complexity?

Required source types:

- standards and specifications;
- official project documentation;
- peer-reviewed or primary design papers where standards do not answer the question.

Required citations:

- precise section headings or specification anchors;
- version/date;
- explicit quote-free paraphrase of what the source establishes and does not establish.

Exclusions:

- generic blog summaries when primary specifications are available;
- vendor marketing claims without technical contracts;
- unsourced “best practices.”

Expected information output:

- comparison matrix;
- minimal candidate object model;
- rejected concepts with reasons;
- unresolved project choices.

### Question family B — Attestations, verification, and non-circular trust

- How do in-toto/SLSA/DSSE/Sigstore or comparable systems separate producer statements from verifier trust?
- What prevents an artefact producer from being treated as an independent verifier?
- How are identity, signature, delegation, revocation, expiry, and trust roots handled?
- Which controls are necessary for a local/private project versus a public distributed supply chain?
- When is a signature materially useful, and when is a content hash plus controlled repository history sufficient?

Expected information output:

- threat-model-driven recommendation;
- independence-group model;
- signature and trust-root recommendation;
- explicit non-recommendations for unnecessary infrastructure.

### Question family C — Reproducible builds, content addressing, and semantic identity

- How do Reproducible Builds, Nix derivations/store paths, Bazel action keys/CAS, and similar primary systems define inputs, environments, commands, outputs, and cache identity?
- How do they separate exact bytes from logical lineage or version identity?
- What dependencies must be captured to make reuse safe?
- How are non-deterministic execution fields excluded from semantic identity?
- What lessons apply to Unity/source interpretation and target conversion without assuming those systems are equivalent to software compilation?

Expected information output:

- semantic identity model;
- execution identity model;
- dependency-set principles;
- boundary with the later cache/invalidation brief.

### Question family D — Policy evaluation and transition authorisation

- What primary models support fail-closed policy evaluation over evidence?
- How should an authorisation decision enumerate:
  - exact requested action;
  - required evidence;
  - evaluated policy;
  - current dependency identities;
  - allowed and forbidden operations;
  - expiry/invalidation triggers;
  - reviewer or human approvals?
- Should decisions be recomputed views, immutable decision artefacts, or both?
- Compare:
  1. one explicit conversion-authorisation stage;
  2. one cross-cutting policy engine;
  3. explicit per-transition decisions;
  4. hybrid cross-cutting evaluation plus immutable transition-decision records.

Expected information output:

- comparative trade-off table;
- recommended stage-agnostic model;
- decision evaluation algorithm in precise pseudocode;
- exact fail-closed cases.

### Question family E — Evidence states and permissions

- How should factual state, evidence maturity, review state, validation result, and permission remain separate?
- What semantics should be assigned to:
  - `PASS`;
  - `FAIL`;
  - `INDETERMINATE`;
  - `UNSUPPORTED`;
  - `NOT_RUN`;
  - `NOT_APPLICABLE`?
- Which states may authorise which classes of action?
- How should a stronger upstream evidence class avoid inflating downstream evidence maturity?

Expected information output:

- state-transition table;
- forbidden state collapses;
- recommended vocabulary compatible with repository governance.

### Question family F — Corrections, supersession, and auditability

- What models preserve history while allowing correction, supersession, withdrawal, version splitting, and restoration?
- How should a decision that was valid for an older game build remain historical without authorising the current build?
- How should dependent decisions be marked stale or invalidated?
- Should provenance records be append-only, event-sourced, snapshot-based, or hybrid?

Expected information output:

- correction/event model;
- dependency-impact procedure;
- audit-history requirements.

### Question family G — Minimal Armour-specific evidence architecture

Using the preceding research, propose the smallest complete model for Tainted Armour.

The report must define candidate records and their responsibilities, including at minimum:

```text
ArtifactIdentity
EnvironmentIdentity
ComponentIdentity
PolicyIdentity
AlgorithmIdentity
DependencySet
ExecutionRecord
StageAssertion
IndependentVerificationRecord
ReviewFinding
TransitionAuthorisationDecision
PromotionDecision
CorrectionOrSupersessionEvent
ProvenanceGraph
```

For each candidate record, state:

- semantic versus operational content;
- deterministic versus non-deterministic fields;
- exact identity mechanism;
- required relationships;
- permitted producers;
- required reviewers;
- maximum authority;
- invalidation triggers;
- whether it should exist as a durable record, a computed view, or both.

The report must not assume the old P0–P20 or provisional F0–F21 counts.

## 7. Internal-game evidence required

Deep Research must identify, but not execute, later internal proof needed to bind the general evidence model to FoA.

### Later runtime/diagnostic/validation proof required

1. A current target-build identity packet containing:
   - game/build/distribution identity;
   - Unity Player/runtime identity;
   - relevant package/archive identities;
   - exact managed assembly fingerprints.

2. A transition inventory from controlled native armour operations distinguishing:
   - package/file resolution;
   - Kandra stream loading;
   - renderer submission;
   - full registration;
   - skinning/rendering;
   - item acquisition;
   - inventory ownership;
   - equip;
   - save;
   - reload;
   - unequip;
   - unregister/disposal.

3. Controlled observations showing which environment or dependency changes invalidate:
   - source interpretation;
   - target profiles;
   - Kandra payloads;
   - packages;
   - item templates;
   - persistence evidence.

4. Negative controls for:
   - stale target profile;
   - mismatched payload/package pair;
   - missing verification record;
   - missing package;
   - outdated build fingerprint;
   - incomplete registration;
   - failed save restoration.

### Environment identity required

- exact installed build;
- exact distribution branch;
- exact Unity runtime;
- exact relevant DLL SHA-256/MVID values;
- exact Kandra package/stream identities;
- exact mod/framework versions;
- exact scenario/save state.

### Receipts/artifacts required later

- immutable raw capture manifest;
- environment manifest;
- transition observations;
- runtime evidence packet;
- save evidence packet;
- failure/negative-control packet;
- dependency-change comparison packet.

### Why external research cannot answer this lane

External standards can define how to record and evaluate evidence. They cannot establish which exact FoA/Kandra runtime transitions occur, which build dependencies are semantically material, or whether a custom armour operation succeeded in the installed game.

## 8. Decompilation-static evidence required

Deep Research must identify, but not execute, later static proof needed to define the actual proprietary transition boundaries.

### Later files, assemblies, symbols, manifests, or asset metadata required

- current `ScriptingAssemblies.json`;
- current relevant DLL fingerprints;
- `Awaken.Kandra.dll` registration, loading, rendering, material, culling, and release symbols;
- `TG.Main.dll` item, equipment, presentation, template, save, load, and teardown symbols;
- package/archive loader and path-resolution symbols;
- native metadata and stream identities for representative armours;
- any source-generated serializer/deserializer metadata relevant to saved item/equipment identity.

### Exact identity/call-path/ownership questions

1. Which exact methods are consequential transition boundaries where the framework would need a current authorisation decision?
2. Which methods merely observe, and which mutate or allocate persistent/runtime state?
3. Which owner is responsible for rollback or cleanup when a transition fails?
4. Which fields or identities must be captured in an execution record to correlate static ownership with runtime observation?
5. Which dependencies should invalidate target, runtime, or persistence evidence?

### Fingerprints required

- file SHA-256;
- MVID where applicable;
- file length;
- build/distribution identity;
- decompiler/tool version;
- exact type/method/field locator;
- native data hash or asset identity.

### Why runtime evidence alone cannot answer this lane

Runtime observation can show that an event happened. It cannot by itself establish exact ownership, hidden call paths, serialized field meaning, all error branches, or the complete cleanup contract. Static ownership must remain a distinct evidence lane.

## 9. Conflicts, contradictions, and stale scope

### Known conflicts

1. The old candidate lifecycle uses one explicit P12 conversion-authorisation stage; H0 proposes lifecycle-wide authorisation but leaves the exact decision form unresolved.
2. The candidate architecture separates deterministic receipts and physical executions, but its exact schema and authority semantics have not been independently researched.
3. R1 uses deterministic revision-local identity plus external lineage; the best relation to provenance and revision models remains to be reviewed.
4. R2 demonstrates narrow deterministic mechanisms through implementation/validation history, but those results do not establish final architectural placement.
5. Supply-chain frameworks may assume organisational, signing, or distribution threats that do not apply to a local/private asset pipeline.
6. Repository review records and runtime evidence have different purposes and must not become one universal record type.

### Possible version splits

- W3C PROV recommendations versus later profiles or implementations;
- SLSA provenance versions;
- in-toto attestation versions;
- DSSE versions;
- Nix/Bazel documentation changes;
- Sigstore identity and transparency-log models;
- current versus historical FoA build transitions.

### Sources likely to disagree

- whether signed attestations are mandatory or optional;
- whether decisions should be durable artefacts or recomputed policy views;
- whether transparency logs are necessary in a local project;
- whether an append-only event model is preferable to corrected snapshots;
- how much supply-chain metadata is appropriate for an asset compiler.

### How later review should resolve them

- classify each source by threat model and deployment context;
- separate universally useful concepts from distributed-supply-chain assumptions;
- prefer minimal mechanisms that satisfy the exact Tainted Armour risk model;
- retain unresolved choices explicitly rather than adopting the most elaborate model.

## 10. Research-agent instructions

- Gather and synthesize information only.
- Do not execute runtime, decompilation, build, test, validation, mutation, deployment, or repository tasks.
- Open every cited source before relying on it.
- Prefer primary standards, official specifications, official technical documentation, and original design papers.
- Record version/date and precise section or anchor for each source.
- Preserve qualifiers, exclusions, and disagreements.
- Separate external findings from repository context and later proprietary proof requests.
- Do not treat a Deep Research report as independent corroboration for a source it summarizes.
- Identify independence groups and citation reuse.
- Do not assume that more metadata, signatures, or infrastructure are automatically better.
- Evaluate the models against Tainted Armour's actual needs:
  - immutable source interpretation;
  - semantic asset derivation;
  - target conversion;
  - independent verification;
  - proprietary runtime transitions;
  - item/equip/save mutation;
  - correction and patch invalidation;
  - human release promotion.
- Return unsupported internal claims as exact missing-proof requests, not guesses.
- Keep the recommendation independent of the unresolved final operational-stage count.

## 11. Expected Deep Research report output

The returned report must contain:

1. Executive verdict bounded by the inspected information.
2. Source table with primary-source identity, version, exact section/anchor, independence group, and applicability.
3. Comparison of provenance, attestation, reproducible-build, policy, and trust models.
4. Threat model for Tainted Armour, distinguishing:
   - accidental error;
   - stale evidence;
   - circular evidence;
   - untrusted source input;
   - compromised tool/component;
   - unauthorised mutation;
   - malicious tampering;
   - mistaken human promotion.
5. Claim-by-claim findings for EP-C01 through EP-C15.
6. Recommended minimal Armour evidence object model.
7. Exact relation model or provenance graph.
8. Deterministic versus non-deterministic field classification.
9. Recommended identity, hashing, signing, expiry, and revocation policies.
10. Recommended independent-verifier and reviewer-independence model.
11. Comparative verdict for:
    - single P12 stage;
    - cross-cutting policy engine;
    - immutable per-transition decisions;
    - hybrid model.
12. Precise transition-authorisation pseudocode.
13. State and permission matrix for `PASS`, `FAIL`, `INDETERMINATE`, `UNSUPPORTED`, `NOT_RUN`, and `NOT_APPLICABLE`.
14. Correction, supersession, withdrawal, and restoration model.
15. Stage-count-independent dependency and invalidation model.
16. Explicit boundary with the later cache/transaction/rollback/security research package.
17. Contradiction map and unresolved choices.
18. Exact internal-game proof still missing.
19. Exact decompilation-static proof still missing.
20. Recommended RH1/RH2/RH3/RH4 path after mandatory returned-report intake.
21. Explicit list of mechanisms rejected as unnecessary or premature.

The report must include operational examples such as:

```text
source interpretation → semantic capture
adaptation plan → adapted asset generation
payload verification → package construction
package verification → runtime loading
render proof → item/equip mutation
equip proof → save mutation
complete evidence → release promotion
```

Each example must state the evidence consumed, decision produced, permission granted, permission withheld, and invalidation triggers.

## 12. Exact unblock criteria

### External-research unblock criteria

The information-gathering portion is complete only when:

- every question family A–G is answered from inspected primary sources or explicitly remains unresolved;
- every source has a durable locator and exact relevant section;
- competing models are compared under the Tainted Armour threat model;
- the recommended model is minimal and stage-count independent;
- the report distinguishes required mechanisms from optional or excessive supply-chain infrastructure;
- the P12/cross-cutting/per-transition-authorisation question receives a bounded verdict rather than an analogy.

### Internal-game unblock criteria

The report must specify the later proof required. It must not claim to execute or complete that proof.

No proprietary runtime transition receives action authority until the relevant current-build runtime evidence is captured and reviewed.

### Decompilation-static unblock criteria

The report must specify the later exact assemblies, symbols, metadata, fingerprints, and ownership questions. It must not claim to execute decompilation or extraction.

No proprietary mutation boundary is accepted merely because an external evidence model recommends one.

### Review/promotion criteria

After the report returns:

1. preserve the source identity;
2. inspect the report and cited sources;
3. create a labelled cleaned derivative;
4. classify claims and contradictions;
5. store it under `documents/research/frameworks/tainted-armour/`;
6. update the owner README and programme index;
7. commit to `tainted-armour`;
8. update draft PR `#350`;
9. complete required domain and independent review;
10. obtain a named human decision before the evidence architecture becomes current.

### Remaining forbidden uses

Until all required review and proof is complete, the brief/report may not authorise:

- schema implementation;
- policy-engine implementation;
- coded gates;
- cache or transaction implementation;
- conversion;
- runtime registration;
- item/equip mutation;
- save mutation;
- compatibility support;
- performance claims;
- deployment;
- release;
- automatic execution of the next research task.

## 13. Next researched task

**Cache, invalidation, transaction, rollback, and untrusted-input security architecture.**

This is one information-gathering topic only. It is not automatically authorised and must not execute until the returned evidence/provenance report completes mandatory intake and the current user separately authorises the next task.
