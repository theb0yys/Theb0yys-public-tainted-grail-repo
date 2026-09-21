# Tainted Armour Evidence, Provenance, and Transition-Authorisation Architecture — Cleaned Deep Research Derivative

## Document control

- Document ID: `tainted-armour-evidence-provenance-transition-authorisation-cleaned-2026-08-20`
- Status: `under-evaluation`
- Authority state: `intake`
- Knowledge class: cleaned derivative of a returned ChatGPT Deep Research report
- Research method: ChatGPT Deep Research
- Deep Research execution state: `RETURNED`
- Deep Research session ID: `6a874b6b-40d0-83eb-864f-f2cb7b3deed8`
- Report returned: 2026-08-20
- Source locator: report pasted into the controlling conversation on 2026-08-20 after execution of [`armour-evidence-provenance-transition-authorisation-deep-research-brief-2026-08-20.md`](armour-evidence-provenance-transition-authorisation-deep-research-brief-2026-08-20.md)
- Original byte preservation: unavailable; the report arrived as conversation text rather than a downloadable raw artifact
- Original SHA-256 and byte size: unavailable and not inferred
- Derivative created: 2026-08-20
- Product owner: `mods/tainted-armour/`
- Research owner: `documents/research/frameworks/tainted-armour/`
- Governing process:
  - `documents/process/research-lifecycle.md`
  - `documents/process/research-standard/README.md`
  - `documents/process/research-standard/deep-research-brief-standard.md`
  - `documents/process/research-standard/deep-research-report-intake-standard.md`
- Parent H0 derivative: [`h0-armour-framework-lifecycle-hypothesis-review-cleaned-2026-08-20.md`](h0-armour-framework-lifecycle-hypothesis-review-cleaned-2026-08-20.md)
- Allowed usage:
  - evidence-architecture claim review;
  - provenance and authorisation research planning;
  - preparation of later domain and independent review;
  - source navigation;
  - blocking unsafe inference or self-authorisation.
- Forbidden usage:
  - schema, serializer, policy-engine, cache, transaction, or coded-gate implementation authority;
  - conversion or package generation;
  - runtime, item, equipment, or save mutation;
  - compatibility, performance, promotion, or release claims;
  - treating DSSE, Sigstore, transparency logging, OPA, or any proposed record set as mandatory architecture;
  - converting missing evidence into `PASS` through human override;
  - automatically executing the report's roadmap or next research topic.
- Supersedes: none
- Superseded by: none

## Intake disposition

The returned report contains useful external research and a credible leading architecture candidate, but it is a mixed `E1` artifact. It correctly supports separation of artefacts, executions, agents, assertions, verification, policy decisions, and promotion. It also materially overstates several conclusions and contains unsafe state/permission semantics.

This derivative:

1. preserves useful findings;
2. removes temporary conversation-native citation markers;
3. replaces them with durable primary-source locators;
4. updates stale source versions where a current specification exists;
5. separates exact content identity, semantic identity, lineage, and physical execution identity;
6. separates evidence state from permission;
7. removes the claim that a human override may convert `INDETERMINATE` into `PASS`;
8. corrects `NOT_RUN`, `NOT_APPLICABLE`, and `UNSUPPORTED` semantics;
9. narrows DSSE, Sigstore, signatures, transparency logs, and policy engines to threat-model-dependent candidates;
10. removes unsupported implementation timelines and prototype execution recommendations;
11. preserves remaining proprietary FoA/Kandra work as downstream evidence requests.

The original conversation report remains the source artifact. This file is not a byte-identical copy.

## Executive verdict

**Research state: PARTIAL.**

The report supports a stage-count-independent evidence architecture built around distinct artefacts, activities/executions, responsible agents, assertions, independent verification, review, bounded authorisation decisions, correction events, and human promotion.

The strongest current candidate is:

```text
PROV-like artefact / activity / agent relations
+ content-addressed exact artefact identities
+ domain-separated semantic record identities
+ separate physical execution records
+ producer assertions that have no self-authority
+ independently produced verification records where policy requires them
+ cross-cutting policy evaluation
+ immutable bounded transition-decision records
+ separate human promotion
+ append-only correction and supersession events
+ computed current/stale/blocked views
```

The report does **not** establish that every record must be signed, that DSSE is mandatory, that Sigstore or a transparency log is required, that OPA/Rego is the correct policy implementation, or that one fixed record schema is ready for implementation.

The returned recommendation of a hybrid policy evaluator plus immutable per-transition decisions remains the leading candidate, but it requires project threat-model review, transition inventory, domain review, independent review, and human acceptance.

## Findings retained

### 1. Artefacts, executions, and responsible agents are distinct

W3C PROV-DM is a stable W3C Recommendation that distinguishes `Entity`, `Activity`, and `Agent`, with relations including generation, usage, derivation, attribution, association, delegation, revision, specialization, and invalidation. This maps cleanly to an asset compiler at a conceptual level:

```text
source / semantic asset / payload / package / evidence record  -> Entity-like
physical import / transform / verify / load / equip / save     -> Activity-like
software component / operator / reviewer / owner               -> Agent-like
```

This mapping is a domain profile of PROV, not a proof that all PROV concepts must be implemented.

### 2. Exact artefact identity and physical execution identity must be separate

Reproducible-build practice separates specified source, environment, instructions, and outputs from logs and other ancillary execution data. SLSA Build Provenance similarly separates `BuildDefinition` from `RunDetails`, including invocation and timestamps.

Therefore two identities are required:

```text
exact or semantic artefact identity
physical execution identity
```

Repeated physical executions may reproduce the same semantic artefact and stage assertion while retaining distinct execution identities.

### 3. Producer statements are not independent verification

The in-toto Attestation Framework binds a statement to immutable subjects by digest and identifies the predicate type. The wider in-toto supply-chain model distinguishes functionary link metadata from project-owner layouts and client-side verification/inspection. Current in-toto predicates also include verification-summary concepts.

The Armour model should therefore distinguish:

```text
producer StageAssertion
independent IndependentVerificationRecord
policy / review decision
```

The exact independence rule remains to be researched. A separate process, component identity, machine, operator, or organization may be required depending on claim risk.

### 4. Cross-cutting policy plus durable transition decisions is the leading model

A single late authorisation gate is too narrow for transitions that may independently mutate assets, packages, runtime state, equipment, saves, or release state.

The leading candidate is:

```text
stage-count-independent policy evaluator
        +
immutable decision record for each consequential requested action
```

The evaluator is a computed mechanism; the decision record preserves the exact request, evidence snapshot, policy identity, dependency identities, decision, reasons, scope, and invalidation triggers.

This remains an architecture candidate rather than implementation authority.

### 5. Corrections must preserve history

Prior records must remain inspectable. Corrections, supersession, withdrawal, version splitting, and restoration should be represented as new events or replacement records linked to the old identity.

PROV `wasRevisionOf` and `wasInvalidatedBy` may provide useful relations. `specializationOf` must not be used as a generic synonym for supersession: it expresses a more specific entity sharing aspects with a more general entity, not an arbitrary correction or replacement decision.

## Material corrections to the returned report

### Correction A — `PASS` is necessary but never sufficient for permission

The report repeatedly implies:

```text
StageAssertion = PASS
→ ALLOW next action
```

That is unsafe.

The corrected rule is:

```text
all required assertions PASS or are proven NOT_APPLICABLE
+ required evidence maturity satisfied
+ dependencies current and identity-matched
+ required independent verification present
+ required review complete
+ current policy evaluates the exact action as allowed
= TransitionAuthorisationDecision.ALLOW
```

A `PASS` assertion establishes only the assertion's bounded claim. It does not independently grant permission.

### Correction B — Human override cannot convert `INDETERMINATE` into `PASS`

The returned report recommends that a human may issue a one-time `PASS` override for incomplete evidence. That collapses factual state into permission and violates the programme's truth boundary.

Corrected rule:

- `INDETERMINATE` remains `INDETERMINATE` until claim-fit evidence resolves it.
- A human may change project policy, narrow scope, waive a repository process rule, or explicitly accept a bounded risk where permitted.
- Any such decision must be a separate exception or risk-acceptance record.
- It must not rewrite the technical assertion to `PASS`.
- It must not support validity, compatibility, runtime-safety, persistence, performance, or release claims that the evidence does not prove.

For the current Armour programme, unresolved material technical claims remain blocking.

### Correction C — `NOT_RUN` and `NOT_APPLICABLE` were conflated

Correct semantics:

| State | Meaning | Default permission effect |
|---|---|---|
| `PASS` | The declared assertion was evaluated and satisfied within its scope | Necessary input only; not self-authorising |
| `FAIL` | The assertion was evaluated and contradicted | Blocks the dependent action |
| `INDETERMINATE` | Required evidence or interpretation is missing, ambiguous, stale, or insufficient | Blocks the dependent action |
| `UNSUPPORTED` | The feature or condition is outside the declared supported capability/scope | Blocks this route; may select a separately researched alternative |
| `NOT_RUN` | The stage/assertion was not executed | Blocks when required; cannot be treated as not applicable |
| `NOT_APPLICABLE` | A current policy/applicability decision has established that the stage/assertion is not required for this case | May satisfy only that explicitly waived dependency; is not semantically `PASS` |

`NOT_APPLICABLE` itself requires a current applicability decision and dependency scope.

### Correction D — DSSE is an optional signing envelope, not a complete trust model

DSSE signs exact payload bytes together with a payload type and deliberately avoids requiring canonicalization. Its own scope excludes key management and PKI.

Consequences:

- DSSE may be useful when an Armour record crosses a trust boundary and requires a signature.
- DSSE does not establish who should be trusted, how keys are managed, whether signing is necessary, or whether the payload's semantic representation is deterministic.
- DSSE does not replace RFC 8785/JCS or another canonical representation when the project needs stable semantic content identity across independently produced JSON serializations.

The report's recommendation that every `ExecutionRecord`, `StageAssertion`, and transition decision must be DSSE-signed is **not established**.

### Correction E — Sigstore and transparency logs are threat-model-dependent

Sigstore's public model combines OIDC identity, short-lived certificates, ephemeral keys, and public transparency logs. Its security documentation also makes clear that signatures prove authentication at a time, not that the signer should have signed, that policy allowed the action, or that the artefact is technically good.

For a private/local Armour pipeline:

- public OIDC identity may be unnecessary;
- public disclosure in Rekor may be undesirable;
- a public transparency log may add operational and privacy costs;
- repository history, branch/PR review, access control, and local signatures may be sufficient for some threat models;
- release artefacts distributed to third parties may justify stronger identity/signature/logging controls than local research records.

Therefore:

```text
mandatory Sigstore keyless signing       NOT_ESTABLISHED
mandatory public transparency logging    NOT_ESTABLISHED
mandatory signature on every record      NOT_ESTABLISHED
```

A dedicated Armour threat model must decide where authenticity, non-repudiation, witness, timestamp, revocation, or public audit are required.

### Correction F — SLSA source version was stale and its scope is narrower

The returned source table cited SLSA provenance `v0.1` and discussed older `v1.1` material. SLSA `v1.2` is the current approved specification as of this intake.

SLSA Build Provenance is designed for software-build provenance. Its useful concepts include:

- subjects identified by digests;
- a build definition;
- external and internal parameters;
- resolved dependencies;
- builder identity;
- run-specific metadata.

Those concepts are analogies and reusable patterns. They do not establish Armour runtime, equip, save, or target-format semantics.

### Correction G — Nix was oversimplified

The report implies that Nix generally provides a content-addressed store whose identifiers can be adopted directly as semantic identities.

Current Nix documentation distinguishes input-addressed and content-addressed store objects. Some content-addressed derivation modes remain experimental. A content address identifies store-object content and references; it does not provide cross-revision lineage, technical validity, policy acceptance, or authorisation.

Armour should retain separate concepts for:

```text
exact bytes / content identity
semantic object identity
cross-revision lineage
execution identity
authorisation identity
```

### Correction H — deterministic fields and exact record integrity were confused

The returned object table incorrectly labels several inherently operational or human fields as deterministic, including timestamps, review comments, and correction timestamps.

Correct distinction:

1. **Semantic deterministic payload** — fields expected to reproduce identically for the same semantic inputs, policy, algorithm, and environment.
2. **Operational record** — physical run identity, timestamps, host, paths, commands, stdout/stderr references, operator, and other attempt-specific data.
3. **Exact-record integrity digest** — a hash of the exact serialized record, which can protect integrity even when the record contains non-deterministic fields.

A record may be content-addressed for integrity without being semantically reproducible.

### Correction I — diagnostics and comments do not belong in semantic assertion identity by default

Localized error text, stack traces, absolute paths, timing, stdout/stderr, review comments, and free-form rationale should not normally alter the semantic identity of a stage assertion.

The assertion should contain stable machine-readable assertion codes, values, evidence references, and policy identities. Diagnostics and commentary should be referenced separately or carried in a non-semantic envelope.

### Correction J — rollback belongs to the later transaction/security research package

The authorisation evaluator may deny or block a transition. It does not itself prove or implement atomic rollback.

Correct boundary:

```text
Evidence/provenance/authorisation research:
    whether an operation is permitted and why

Cache/transaction/rollback/security research:
    how a permitted operation is staged, committed, quarantined, reverted, or recovered
```

The returned pseudocode improperly treats rollback as an already available consequence of denial.

### Correction K — the internal runtime sequence remains a capture hypothesis

The report lists package resolution, stream loading, renderer construction, registration, draw, item/equip, save/reload, and teardown as an ordered sequence.

The Kandra managed lifecycle supports several internal transitions, but the representative native armour presentation/equip order is not yet proven. The sequence must be written as a transition inventory to observe, not as a complete established native call order.

### Correction L — implementation roadmaps and time estimates are removed

The returned report recommends schemas, policy rules, verifier prototypes, policy-engine prototypes, runtime instrumentation, and a one-to-two-month implementation/research timeline.

Those actions and estimates were not authorised or evidence-backed. They are removed from the cleaned derivative. No prototype or implementation follows from this intake.

## EP-C01 through EP-C15 claim review

| Claim | Cleaned disposition | Maximum use now |
|---|---|---|
| `EP-C01` Semantic artefact identity should be separate from physical execution identity | **SUPPORTED as an architectural requirement**; exact schema unresolved | Architecture planning |
| `EP-C02` Stage assertions, authorisation decisions, and human promotion are distinct record types | **PARTIALLY SUPPORTED / leading model**; exact record boundaries and merge opportunities unresolved | Architecture planning |
| `EP-C03` Provenance applies across the lifecycle rather than one stage | **SUPPORTED conceptually**; minimum profile unresolved | Architecture planning |
| `EP-C04` A stage result must not authorise itself | **SUPPORTED governance invariant** | Block unsafe inference |
| `EP-C05` A single P12 stage should be removed | **UNRESOLVED**; hybrid per-transition decisions are leading candidate | Comparative research only |
| `EP-C06` Every consequential transition needs an explicit bounded decision | **PARTIALLY SUPPORTED**; exact transition inventory unresolved | Planning |
| `EP-C07` Evidence maturity is independent of gate result | **SUPPORTED governance invariant**; final vocabulary unresolved | Planning and review |
| `EP-C08` Tests, builds, logs, generated files, and agent notes are not technical authority by themselves | **SUPPORTED under current user direction and repository process** | Block unsafe inference |
| `EP-C09` Material dependency changes invalidate dependent decisions | **SUPPORTED conceptually**; precise invalidation model deferred | Planning |
| `EP-C10` Producer/verifier independence must be represented | **SUPPORTED for policy-selected claims**; degree and independence groups unresolved | Planning |
| `EP-C11` Corrections and supersession preserve history | **SUPPORTED governance invariant** | Planning and intake |
| `EP-C12` Cryptographic signatures are mandatory for every local record | **NOT SUPPORTED** | None |
| `EP-C13` Transparency logs or timestamp authorities are required | **NOT SUPPORTED as a universal requirement** | Threat-model research only |
| `EP-C14` R2 JCS/SHA-256 mechanisms belong unchanged in the final evidence architecture | **UNRESOLVED / PARTIAL**; useful mechanisms, final placement not proven | Planning only |
| `EP-C15` Evidence architecture can be stage-count independent | **SUPPORTED as a design goal**; exact transition semantics unresolved | Architecture planning |

## Corrected candidate object model

This is a research candidate, not an implementation schema.

| Candidate record/concept | Purpose | Identity character | Authority ceiling before policy/review |
|---|---|---|---|
| `ExactArtifactIdentity` | Identity of exact immutable bytes or normalized binary object | domain-separated cryptographic digest | Integrity only |
| `SemanticObjectIdentity` | Revision-local identity of a semantic object/relationship | deterministic domain-separated derivation | Identity only |
| `LineageIdentity` | Cross-revision continuity supported by explicit mapping evidence | durable project identity plus mapping record | Continuity claim only |
| `EnvironmentIdentity` | Exact semantic environment/build/tool dependency set | content-addressed normalized environment manifest | Scope only |
| `ComponentIdentity` | Exact executable/library/tool source or binary identity | hash/version/MVID as applicable | Component identity only |
| `PolicyIdentity` | Exact resolved policy used for evaluation | content hash of resolved policy | Policy identity only |
| `AlgorithmIdentity` | Exact algorithm/version/parameters contract | content hash or stable versioned URI plus digest | Algorithm identity only |
| `DependencySet` | Role-labelled exact dependencies consumed by an assertion or decision | canonical set digest | Dependency identity only |
| `ExecutionRecord` | One physical attempt, including timestamps, host/run identity and observed input/output references | opaque execution ID plus exact-record integrity digest | Observation only |
| `StageAssertion` | Machine-readable bounded assertion about exact inputs/outputs under exact policy/algorithm | semantic assertion digest; diagnostics separated | No permission by itself |
| `IndependentVerificationRecord` | Separately produced verification of stated properties | exact record identity and verifier identity | Verification claim only |
| `ReviewFinding` | Claim-level evaluation, evidence fit, scope, limitations and review disposition | exact record identity; may contain human/non-deterministic content | Review context only |
| `TransitionAuthorisationDecision` | Policy decision for one exact requested action and evidence snapshot | exact decision-record identity | Exact action permission only |
| `RiskAcceptanceOrExceptionDecision` | Explicit bounded action permission despite known non-PASS state, where project policy permits | exact record identity | Does not change technical truth or permit validity claims |
| `PromotionDecision` | Named human decision over complete bounded evidence and support scope | exact record identity | Bounded current authority only |
| `CorrectionOrSupersessionEvent` | Links old and replacement records without rewriting history | exact event identity | Changes current view only after required review |
| `ProvenanceIndex` | Queryable view over artefact/activity/agent and decision relations | computed view; snapshots may be content-addressed | Navigation and analysis only |

## Corrected relation model

Minimum candidate relations:

```text
ExecutionRecord used Artifact / Environment / Component / Policy / Algorithm
ExecutionRecord generated Artifact or operational observation
StageAssertion asserts properties of exact subjects
StageAssertion wasAssociatedWith producing component/agent
IndependentVerificationRecord verifies exact subject and assertion/property set
ReviewFinding evaluates claims and underlying evidence
TransitionAuthorisationDecision evaluated exact requested action
TransitionAuthorisationDecision consumed exact assertion / verification / review / dependency identities
TransitionAuthorisationDecision permits or blocks one bounded action
Artifact wasDerivedFrom exact upstream artefacts where influence is established
Artifact wasRevisionOf prior semantic artefact where revision is established
CorrectionOrSupersessionEvent supersedes / withdraws / restores exact records
PromotionDecision promotes exact claim set for exact scope
```

`used` plus `wasGeneratedBy` does not automatically prove `wasDerivedFrom`; derivation requires an established influence relationship.

## Corrected transition-authorisation semantics

Candidate decision states:

```text
ALLOW
DENY
BLOCKED
```

- `ALLOW`: all policy requirements for the exact action are currently met.
- `DENY`: current evidence or policy contradicts the action.
- `BLOCKED`: required evidence, policy, identity, review, or system capability is missing, stale, ambiguous, or unavailable.

Candidate evaluation pseudocode:

```text
EvaluateTransition(request, currentEvidenceGraph):
    policy = ResolveExactPolicy(request.action, request.scope)
    if policy missing, invalid, stale, or evaluation errors:
        return BLOCKED

    required = policy.ResolveRequirements(request)
    evidence = ResolveExactEvidence(required, currentEvidenceGraph)

    if any required evidence is missing, inaccessible, stale, superseded,
       identity-mismatched, outside version scope, or citation-mismatched:
        return BLOCKED

    for each required assertion:
        if assertion.state == FAIL:
            return DENY
        if assertion.state in {INDETERMINATE, UNSUPPORTED, NOT_RUN}:
            return BLOCKED
        if assertion.state == NOT_APPLICABLE:
            require current applicability decision for this exact request
        if assertion.state != PASS and assertion.state != NOT_APPLICABLE:
            return BLOCKED

    if required independent-verifier, domain-review, validation-review,
       or human-promotion constraints are not satisfied:
        return BLOCKED

    decision = ALLOW for this exact action, scope, dependency snapshot,
               policy identity, and validity window
    persist immutable decision record
    return decision
```

A later dependency, build, policy, algorithm, verifier, source, target, or evidence change may make the decision stale. The decision record remains historical.

## Threat-model findings

The report identifies real risks but does not complete the required threat model.

Current threat families:

- accidental source or configuration error;
- stale evidence after build/tool/policy changes;
- circular evidence and self-attestation;
- untrusted source input;
- compromised or defective tool/component;
- unauthorised repository/runtime/save mutation;
- tampering with evidence or packages;
- mistaken or over-broad human promotion;
- distributed release authenticity and consumer verification.

Remaining threat-model decisions:

- local single-operator versus multiple independent operators;
- private repository versus public release distribution;
- trusted workstation versus potentially compromised host;
- whether non-repudiation is required;
- whether a public witness/transparency service is required;
- key custody, rotation, revocation, and recovery;
- privacy and redistribution implications of public logging;
- which records cross a trust boundary.

No signature or transparency mechanism may be made mandatory before those decisions are reviewed.

## External source ledger

### Primary standards and specifications

1. **W3C PROV-DM, Recommendation 30 April 2013**
   - URL: <https://www.w3.org/TR/prov-dm/>
   - Relevant sections: `2.1 PROV Core Structures`; `5.1 Entities and Activities`; `5.2 Derivations`; `5.3 Agents, Responsibility, and Influence`; `5.4 Bundles`; `5.5 Alternate Entities`.
   - Establishes: domain-agnostic entity/activity/agent and provenance relations.
   - Does not establish: Armour policy, trust, signatures, permissions, or final schema.

2. **DSSE — Dead Simple Signing Envelope**
   - URL: <https://github.com/secure-systems-lab/dsse>
   - Relevant files: `README.md`, `protocol.md`, `envelope.md`.
   - Establishes: type-bound signing of arbitrary payload bytes and avoidance of mandatory canonicalization.
   - Explicit limitation: key management and PKI are out of scope.

3. **in-toto Attestation Framework, Statement v1**
   - URL: <https://github.com/in-toto/attestation/blob/main/spec/v1/statement.md>
   - Establishes: immutable digest-bound subjects, predicate type, and predicate content.
   - Does not establish: Armour-specific predicates, trust roots, transition policy, or verifier independence.

4. **in-toto Supply Chain Specification**
   - URL: <https://github.com/in-toto/docs/blob/master/in-toto-spec.md>
   - Relevant concepts: layouts, steps, functionaries, links, thresholds, materials/products, inspections.
   - Establishes: separation of step evidence, authorised functionaries, project-owner requirements, and verification.
   - Does not establish: that Armour should directly adopt one in-toto layout.

5. **SLSA v1.2**
   - URL: <https://slsa.dev/spec/v1.2/>
   - Build provenance: <https://slsa.dev/spec/v1.2/build-provenance>
   - Verifying artifacts: <https://slsa.dev/spec/v1.2/verifying-artifacts>
   - Establishes: current SLSA build-provenance and verification concepts.
   - Does not establish: Armour runtime, equip, persistence, or proprietary semantics.

6. **Sigstore Overview, Security Model, and Threat Model**
   - URLs:
     - <https://docs.sigstore.dev/about/overview/>
     - <https://docs.sigstore.dev/about/security/>
     - <https://docs.sigstore.dev/about/threat-model/>
   - Establishes: Sigstore's OIDC identity, short-lived certificates, ephemeral keys, Rekor/Fulcio transparency model, and stated threat assumptions.
   - Does not establish: that these mechanisms are necessary for a local/private Armour pipeline or that a valid signature means an artefact is technically correct.

7. **The Update Framework Specification**
   - URL: <https://theupdateframework.github.io/specification/latest/>
   - Relevant concepts: trusted roles, threshold signatures, versioning, expiry, delegation, consistent snapshots, rollback/freeze protections.
   - Establishes: distributed software-update trust and freshness mechanisms.
   - Does not establish: Armour internal evidence states or that TUF should be used for every record.

8. **Reproducible Builds Definition**
   - URL: <https://reproducible-builds.org/docs/definition/>
   - Establishes: same source, environment, and instructions reproduce bit-identical specified artefacts; logs are normally ancillary.
   - Does not establish: semantic equivalence or runtime correctness.

9. **Nix Reference Manual**
   - URLs:
     - <https://nix.dev/manual/nix/2.35/glossary>
     - <https://nix.dev/manual/nix/2.32/store/derivation/outputs/content-address>
   - Establishes: input-addressed/content-addressed distinctions and derivation/output concepts.
   - Limitation: some content-addressed derivation capabilities are experimental; Nix identities are not Armour lineage or permission records.

10. **Bazel Remote Caching / Remote Execution**
    - URLs:
      - <https://bazel.build/remote/caching>
      - <https://bazel.build/remote/rbe>
    - Establishes: action-key/result-cache and content-addressable-output patterns, explicit inputs, commands, environment, and outputs.
    - Does not establish: Armour semantic or authorisation contracts.

11. **Open Policy Agent documentation**
    - URLs:
      - <https://www.openpolicyagent.org/docs>
      - <https://www.openpolicyagent.org/docs/operations>
    - Establishes: separation of policy decision from enforcement and structured policy inputs; policy-less queries can return undefined.
    - Does not establish: that OPA/Rego is the required Armour implementation.

12. **NIST SP 800-204D**
    - URL: <https://csrc.nist.gov/pubs/sp/800/204/d/final>
    - Establishes: software supply-chain security strategies for CI/CD contexts.
    - Does not establish: Armour-specific record schemas or permissions.

## Proprietary evidence still required

External evidence architecture cannot identify the real FoA/Kandra transition boundaries by itself.

### Decompilation-static proof requests

- current `ScriptingAssemblies.json` and exact build identity;
- current `Awaken.Kandra.dll`, `TG.Main.dll`, loader, package, item/equipment, presentation, save/load, material, culling, and teardown identities;
- exact method and field locators for observation versus mutation boundaries;
- ownership of rollback and cleanup on failure;
- dependencies that invalidate target, runtime, item, persistence, or teardown evidence.

### Native static-data requests

- exact native armour streams and metadata;
- target body/rig/material/culling/package identities;
- native item/template/presentation identities;
- representative variant corpus.

### Runtime proof requests

Observe and distinguish, without assuming final order:

- package/path resolution;
- Kandra stream reads;
- renderer submission;
- completed registration;
- skinning and draw submission;
- visible pixels and material/culling correctness;
- item acquisition and inventory ownership;
- native equip/presentation integration;
- save and reload;
- unequip;
- unregister, disposal, and resource release.

### Persistence proof requests

- template registration relative to load;
- equipped and unequipped restoration;
- missing package/template behaviour;
- migration and incompatible version behaviour;
- save-overwrite protection.

### Performance/capacity proof requests

- manager capacity limits;
- memory/buffer ownership;
- repeated-cycle stability;
- load and frame-time envelope.

## Completion and review status

```text
Deep Research execution                     RETURNED
source preservation metadata                 PASSED within available limits
primary-source spot inspection               PARTIAL
claim decomposition EP-C01–EP-C15            PASSED in this derivative
useful finding retention                     PASSED
unsafe state/permission corrections          PASSED
final evidence object model                  NOT_RUN
final signature/trust policy                 NOT_RUN
final transition-authorisation architecture  NOT_RUN
current proprietary transition inventory     NOT_RUN
RH2 domain review                            NOT_RUN
RH3 independent review                       NOT_RUN
RH4 validation review                        NOT_RUN
RH5 human promotion                          NOT_RUN
implementation authority                     BLOCKED
```

## Remaining bounded research questions

1. Exact Armour threat model and trust boundaries.
2. Minimum required record set; which candidate records can merge without evidence leakage.
3. Exact independence requirements by claim risk.
4. Exact predicate schemas for assertions, verification, review, and decisions.
5. Whether transition decisions are always durable artefacts, recomputed views, or both.
6. Policy versioning, applicability, exception, expiry, and delegation semantics.
7. Whether any local records require signatures.
8. Whether release records require Sigstore, another PKI, or repository-bound signatures.
9. Whether any transparency or timestamp service is justified.
10. Provenance graph validity and cycle constraints.
11. Exact correction/supersession/withdrawal/restoration relations.
12. Boundary between evidence invalidation and cache invalidation.
13. Current FoA/Kandra mutation and observation transition inventory.
14. Exact implementation threshold after domain and independent review.

## Current conclusion

The report materially advances the Armour evidence architecture, but does not complete it.

The accepted provisional rule set is:

```text
artefact identity != execution identity
assertion != verification
verification != review
review != permission
permission != promotion
PASS != automatic ALLOW
INDETERMINATE cannot be rewritten to PASS by override
NOT_RUN != NOT_APPLICABLE
signatures and transparency are threat-model-dependent
historical records remain immutable; current views may change through explicit events
```

No schema, policy engine, signing system, cache, gate, runtime operation, or next research package is authorised by this intake.