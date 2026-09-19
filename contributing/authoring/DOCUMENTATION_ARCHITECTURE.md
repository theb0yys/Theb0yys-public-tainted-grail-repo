# Mandatory Public Documentation Architecture

Status: **Current proposed repository design for this migration branch**  
Scope: all public documentation added, moved, split, or materially rewritten under this architecture.

This repository is organized around **reader need and canonical knowledge ownership**, not a single linear skill ladder and not one universal page template.

## Core rule

**One canonical explanation for each piece of truth; multiple routes into it.**

A tutorial may link to a system explanation. A mechanic may link to exact reference data. A troubleshooting page may link to the failed lifecycle stage. They must not maintain competing copies of the same technical claim.

## Mandatory information surfaces

```text
learn/          guided learning and journeys
systems/        how native FoA works
mechanics/      how to perform a bounded capability safely
investigate/    how to discover unknown FoA behaviour
diagnose/       symptom → earliest failed owner/stage
reference/      exact IDs, types, hooks, versions and evidence status
examples/       complete evidence-backed examples and case histories
tooling/        loaders, frameworks, importers, SDK/tooling and maintenance
contributing/   authoring and documentation governance
```

The root `README.md` remains task-first. Existing `00` and `01` beginner paths remain valid learning routes during migration.

## Canonical reasoning model

Public technical material must preserve the private proven reasoning model:

```text
exact subject / identity
→ native owner
→ lifecycle and data contract
→ observation or source inspection
→ smallest justified intervention
→ preserve native ownership
→ downstream behaviour
→ cleanup / restoration
→ claim-fit proof

          ├── persistence proof, separately
          ├── compatibility proof, separately
          └── release/package proof, separately
```

This is a reasoning model, not a fixed number of public gates.

## Mandatory document archetypes

Every new technical page must declare or clearly behave as one of these types.

### 1. Native system

Use for: how FoA owns and executes a subsystem.

Required content:
- purpose and position in FoA;
- upstream/downstream owners;
- exact identities/data contracts;
- lifecycle/state transitions;
- normal vanilla entry;
- accepted/rejected/deferred states where relevant;
- cleanup/resource lifetime;
- evidence and unknowns.

Completion test: a capable reader should not need to rediscover the owner, readiness condition, required input, terminal transition, cleanup, or proof boundary.

### 2. Mechanic / capability

Use for: a bounded thing a mod can do.

Required content:
- goal and non-goals;
- prerequisites;
- canonical owning-system links;
- complete mechanic path;
- intervention seam and why it is used;
- bounded implementation or pseudocode;
- downstream success condition;
- failure branches;
- cleanup;
- persistence/compatibility effects;
- verification matrix.

A mechanic is incomplete if any relevant segment of this path is missing:

```text
entry → owner → action → downstream consumer → terminal success → cleanup → proof
```

### 3. Investigation

Use for: discovering an undocumented identity, owner, lifecycle, or intervention.

Required content:
- question;
- known facts;
- hypotheses;
- observation/source strategy;
- candidate owners;
- evidence gathered;
- rejected assumptions;
- corrected model;
- remaining unknowns;
- next evidence required.

Investigation pages must teach *why* candidates were accepted or rejected.

### 4. Case study

Use for: transferring expert reasoning through a real history.

Required content:
- intended capability;
- plausible initial model;
- observed symptom/result;
- evidence gathered;
- corrected ownership/model;
- implementation correction;
- proof;
- residual limits.

Preserve meaningful failure/correction history when it shaped the final process.

### 5. Troubleshooting

Use for: symptom-driven diagnosis.

Required content:
- symptom;
- what has definitely succeeded;
- earliest unproven transition;
- likely owner of that transition;
- observations that distinguish causes;
- diagnostic actions;
- stop conditions;
- canonical system/mechanic links.

Do not publish bags of unrelated “common fixes”.

### 6. Reference

Use for: fast exact lookup.

Required content:
- exact identity/type/method/hook;
- identity kind;
- concise semantics;
- version/runtime scope;
- evidence/status;
- canonical system/mechanic links.

Reference pages do not reproduce long conceptual explanations.

### 7. Evidence / compatibility note

Use for: recording what a specific claim actually proves.

Required content:
- exact claim;
- environment;
- artifact/version;
- evidence lane;
- action;
- expected and observed result;
- status;
- limitations;
- last verified.

Evidence from one lane must never be phrased as proof of another lane.

### 8. Framework / tooling contract

Use for: public contracts owned by modding infrastructure.

Required content:
- provider/owner;
- consumer contract;
- lifecycle;
- capabilities/version negotiation;
- dependencies;
- registration/unregistration;
- failure and cleanup;
- migration;
- readiness.

Owning a contract is not the same as proving every consumer capability.

## Common metadata/evidence shell

Pages may use prose or front matter, but the following information must be visible when relevant:

```text
document type
exact scope
game build / version scope
Mono / IL2CPP / both / unspecified
canonical owner page
static/source status
build status
loader status
runtime status
persistence status
compatibility status
release/package status
last verified
known limits
```

Use `UNSPECIFIED` or an explicit unknown rather than inventing scope.

## Diagram rules

Choose diagrams by question:

- ownership graph — **who owns this?**
- lifecycle/state diagram — **when and under what state does this happen?**
- sequence/call-flow diagram — **in what order do owners collaborate?**
- asset/data-flow diagram — **what resource crosses which boundary?**

No decorative architecture diagrams. Every diagram must answer a concrete reader question.

## Page splitting rule

Split by **information responsibility**, not file size.

A split is justified when one page contains several independently reusable truths with different maintenance/evidence lifecycles. A domain hub must preserve the complete front-to-back reading path after a split.

## Learning-route rule

`00-never-made-a-mod-start-here/` and `01-basic/` remain learning journeys during migration.

`02-foundational/` and `03-advanced/` are routes through canonical material, not permanent owners of technical truth.

`04-framework/` and `05-infrastructure/` are specialist/tooling routes, not mandatory higher levels of normal modding.

## Public-safe boundary

Do not publish:
- bulk decompiled game source;
- extracted game assets/bundles;
- game/Unity/BepInEx binaries;
- private saves or unrestricted logs;
- credentials or machine-specific secrets;
- private implementation source not explicitly approved for publication;
- unreviewed private research as current public truth.

Public examples must be independently authored teaching material and carry their own evidence status.

## Mandatory authoring workflow

Before a major technical page is written, prepare:

```text
Subject
Reader question
Document archetype
Canonical native owner
Required prerequisites
Private/public evidence sources
Claims to teach
Failures/corrections worth preserving
Required diagrams
Worked-example candidate
Evidence lanes
Public/private exclusions
Canonical cross-links
Completion criteria
```

Then follow the process in [PUBLICATION_WORKFLOW.md](PUBLICATION_WORKFLOW.md).

## Migration rule

Legacy paths are preserved with compatibility redirects until links and downstream consumers have moved.

Do not bulk-delete or silently rewrite history. Migrate in bounded waves:
1. establish canonical destination;
2. copy/rewrite with link validation;
3. replace old page with a redirect;
4. update primary navigation;
5. validate changed paths;
6. split large domain monoliths only after source mapping exists.

See [MIGRATION_MANIFEST.md](MIGRATION_MANIFEST.md).
