# Research-Backed Weapon Importer Engineering Process

**Date:** 2026-08-10  
**Domain:** Weapon importer framework  
**Authority:** Canonical process candidate; Deep Research synthesis with canonicalisation corrections  
**Research maturity:** `RESEARCH SUFFICIENT FOR BOUNDED IMPLEMENTATION`  
**Execution state:** `BLOCKED`  
**Blocker:** `RU-GOV-001` — repository integrity

## Purpose

The ten existing weapon-importer gates remain the product-maturity model. This document defines the mandatory evidence-backed engineering process that operates inside every gate.

```text
INTEGRITY ADMISSION
→ PRODUCT GATE
→ RESEARCH UNITS
→ EVIDENCE CLASSES
→ CLAIM REGISTRY
→ HYPOTHESES + FALSIFIERS
→ RISK CLASSIFICATION
→ ADR / DECISION
→ IMPLEMENTATION AUTHORITY
→ BOUNDED IMPLEMENTATION
→ VERIFICATION RECEIPTS
→ DECISION MATURITY
→ PROMOTION
→ DEPENDENCY + INVALIDATION GRAPH
```

No implementation, decompiled signature, build, runtime observation, or historical receipt may promote a claim beyond what its evidence proves.

## 1. Integrity admission

Repository integrity precedes all product gates. Admission requires zero relevant merge markers, immutable repository identity, new build provenance after source reconciliation, and an exact runtime fingerprint before new live evidence is promoted.

Current execution is blocked because audited weapon source and evidence contain unresolved merge/stash conflicts. Resolution must be semantic: preserve compatible identity/comparator and preview-route work rather than selecting an entire merge side. Historical DLL hashes cannot validate newly reconciled source.

## 2. Product gates

1. Authority, evidence, and scope freeze.
2. Private package contract and canonical identity.
3. Offline validator and fixture corpus.
4. Runtime identity, capability, and source hardening.
5. Typed Drake provider and lifetime ledger.
6. Native item registrar contract.
7. Registrar implementation and first-consumer migration.
8. Weapon presentation manifest and equip lifecycle.
9. Acquisition, persistence, and missing-package safety.
10. Compatibility, public preview, and stable API release.

Every gate executes this process. Existing code does not bypass it.

## 3. Research Units and claims

Every material engineering question becomes a Research Unit (RU) containing: question, rationale, subsystem, existing claim, competing hypotheses, sources, evidence classes, exact source/game version, direct and contradicting observations, confidence, limitations, failure consequences, supported/unsupported decisions, required runtime verification, dependencies, revalidation triggers, maturity, and status.

RUs answer questions. Individual assertions are stored separately in a Claim Registry:

```yaml
claimId: CLM-...
statement:
scopeTuple:
researchUnits: []
supportingEvidence: []
contradictingEvidence: []
confidence:
status: proposed|supported|contradicted|stale|withdrawn
maturity: D0-D7
risk: R0-R4
dependencies: []
invalidationTriggers: []
decisionRefs: []
```

This permits fine-grained promotion and invalidation instead of treating an entire research document as one claim.

## 4. Evidence Classes

E0–E8 are classes, **not a linear hierarchy**.

| Class | Evidence |
|---|---|
| E0 | Exact environment/artefact fingerprint |
| E1 | Current-build native body or immutable Questline source |
| E2 | Immutable project executable source |
| E3 | Controlled live runtime observation |
| E4 | Durable machine receipt |
| E5 | Official platform documentation |
| E6 | Differential native comparator experiment |
| E7 | Derived inference |
| E8 | Unknown or contradictory |

Different claims require different combinations. High-risk native mutation normally requires exact provenance plus source/native evidence, runtime evidence, and durable receipts rather than any single evidence class.

## 5. Decision maturity

| Level | Meaning |
|---|---|
| D0 | Question recorded |
| D1 | Competing hypotheses stated |
| D2 | Primary-source support obtained |
| D3 | Bounded experiment implemented |
| D4 | Runtime result reproduced |
| D5 | Negative/failure/lifecycle cases pass |
| D6 | Advertised compatibility scope proven |
| D7 | Independently reviewed with migration/revalidation obligations |

Research maturity and execution state are independent. The project may have enough research for bounded implementation while execution remains blocked by integrity or provenance.

## 6. Risk classes

| Risk | Scope |
|---|---|
| R0 | Offline/read-only |
| R1 | Bounded runtime diagnostic |
| R2 | Reversible presentation mutation |
| R3 | Native process mutation such as `AddToMap` or item grant |
| R4 | Persistent/public contract/release |

Risk measures irreversibility and blast radius. R3/R4 require stronger evidence, disposable test scope where applicable, and independent/human promotion authority.

## 7. Mandatory research loop

```text
integrity + fingerprint
→ question decomposition
→ RUs
→ primary evidence
→ claims
→ alternatives
→ falsification criteria
→ risk classification
→ diagnostic/experiment design
→ bounded authority
→ instrumented implementation
→ static + fixture verification
→ controlled Windows build
→ deployment provenance
→ live positive evidence
→ negative + lifecycle evidence
→ persistence/compatibility evidence where applicable
→ independent review
→ maturity/promotion decision
→ dependency + revalidation registration
```

Hard stops include unresolved conflicts, unknown critical runtime identity, unclassified risk, scope expansion, deterministic fixture failure, build/deployment hash mismatch, lifecycle leak/underflow, dirty native mutation, and missing required review.

## 8. Target v1 architecture

The supported direction is a bounded hybrid native-clone architecture:

```text
Tainted.AssetPackages
  package trust / hashes / provenance / schema
        ↓
typed Drake weapon provider
  bundle + asset ownership
        ↓
Drake bridge
  registered keys only
        ↓
Native Item Registrar
  source profile / clone / identity / collision / AddToMap
        ↓
Native Item + ItemEquip
  native gameplay/equip ownership
        ↓
Tainted Weapons presentation adapter
        ├─ FPP
        ├─ TPP
        └─ inventory/equipment preview

Acquisition: separate adapter
Persistence: separate adapter
```

This direction does not prove clone semantic equality, registrar timing, Drake lifetime, presentation parity, persistence, IL2CPP, or broad compatibility.

A v1 clone requires an approved-field manifest and source-profile hash. Unapproved fields remain source-equal; approved differences require named reasons and validation obligations; source drift invalidates dependent claims.

## 9. Registrar and lifetime policy

Until unregister/rollback is proven, registration is session-immutable and fail-stop:

```text
preflight → reserve identity → transient clone → validate
→ exactly one native insertion → verify → Registered
```

Failure after native mutation without proven rollback produces `TerminalDirty`; further registration is denied for that process.

Provider ownership and native Drake ownership remain separate ledgers:

```text
Provider: package → bundle → asset → presentation consumer
Drake: key → native load count → handle → graphics ID → entity → unload count
```

Promotion requires counter symmetry and final teardown, not permanent retention or suppression of underflow.

## 10. Presentation and package rules

FPP, TPP, and inventory/equipment preview remain independent routes until evidence proves commonality. The `CustomHeroClothes` preview route is runtime-unproven until a clean post-reconciliation binary produces paired visible-vanilla/custom evidence under one exact fingerprint.

No broad camera, layer, transform, or Addressables mutation is authorised merely because a renderer is loaded but invisible.

`weapon-package-manifest/1` remains private/provisional. P0–P18 govern deterministic offline validation. Canonical JSON/hash changes require an explicit versioned ADR and migration decision.

## 11. Fail-closed invariants

```text
conflict marker => no build/promotion
unknown critical runtime tuple => no native mutation
package validation failure => no bundle mount
unsupported runtime track => no private/native/presentation mutation
registrar preflight failure => native insertion count 0
same immutable identity + changed definition => reject absent migration authority
post-insertion verification failure => TerminalDirty
unregistered Drake/Addressables key => untouched native path
provider owners remain => bundle release blocked
counter underflow/unexplained owner => lifecycle failure
missing package on save restore => no silent destructive substitution
persistence experiment => copied disposable save only
public claim => no broader than validated compatibility scope
```

## 12. Verification levels

V0 unit/parser/property; V1 offline package fixtures; V2 source-linked contract; V3 controlled Windows build; V4 deployment/hash/startup; V5 read-only runtime diagnostic; V6 bounded mutation experiment; V7 vanilla/custom differential; V8 repeated lifecycle/performance; V9 copied-save/missing-package; V10 compatibility matrix; V11 release provenance.

Each level proves only its own boundary. Compilation is not runtime proof; one runtime success is not lifecycle, persistence, compatibility, or release proof.

## 13. Receipts, provenance, and freshness

All executable evidence uses a versioned machine-readable receipt envelope carrying receipt/session/request IDs, RUs, claims, gate, result, evidence classes, maturity before/after, risk, provenance, preconditions, operation, before/after state, mutations, warnings/failures, artefacts, and revalidation triggers.

Implementation-critical provenance binds, where applicable: repository/source hashes; game version; Steam branch/build/depot manifests; runtime track; Unity/BepInEx versions; native assembly hashes; framework DLL hash; package/manifest/bundle hashes; source-template and source-prefab profile hashes; contract versions; configuration hash; session/request IDs; timestamp; evidence paths; and research tool/version.

Freshness is primarily fingerprint-driven. Dependency changes automatically stale dependent claims.

## 14. Dependency model

```text
repository integrity
→ runtime provenance
→ current native bodies
→ native lifecycle
→ source archetype profile
→ registrar + presentation

package schema
→ offline validator
→ typed provider
→ provider lifetime
→ Drake lifetime
→ presentation

registrar + presentation + acquisition
→ persistence
→ missing-package/migration
→ compatibility
→ public preview/stable
```

## 15. ADR and review requirements

Major decisions record context, RUs, claims, exact evidence, alternatives, falsification conditions, decision, rejected alternatives, invariants, failure policy, validation requirements, revalidation triggers, and replacement/rollback strategy.

High-risk decisions may not be promoted solely by their original researcher.

## 16. Current status and next lane

```text
Research maturity: RESEARCH SUFFICIENT FOR BOUNDED IMPLEMENTATION
Execution state: BLOCKED
Immediate blocker: RU-GOV-001 repository integrity
Not qualified: end-to-end importer, persistence-safe importer,
IL2CPP compatibility, public preview, stable API
```

After integrity admission, the first bounded lane is:

```text
semantic conflict reconciliation
→ zero-marker verification
→ clean Windows build
→ new DLL provenance
→ exact current FoA runtime fingerprint
→ paired vanilla/custom inventory-preview experiment
```

Promotion ceiling: D4. This lane does not authorise acquisition, save mutation, combat/animation changes, broad camera/transform mutation, global Addressables hooks, unrelated registrar mutation, or public API promotion.

## 17. Document disposition

Keep the consolidated content-import process as parent architecture; keep the validation matrix as executable promotion authority; keep and amend the ten-gate process to invoke this protocol; preserve the original research brief as historical intake; supersede the unresolved-domain map operationally with structured RUs while retaining it historically; reconcile and split the Tainted Weapons research journal into chronological evidence and current authorisation; keep design as provisional with ADR links; replace narrative validation history with structured receipts; archive conflicting historical deployment hashes as superseded; and issue a new reviewed native-weapon architecture revision after the source is reconciled.

## 18. Governing principle

Every material implementation decision must have a named question, evidence basis, claim, falsifier, risk class, bounded authority, verification receipt, decision maturity, dependency set, and revalidation trigger. The framework advances only as far as those records justify.