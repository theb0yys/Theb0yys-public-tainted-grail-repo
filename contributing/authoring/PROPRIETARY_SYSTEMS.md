# Proprietary-System Authoring Standard

Document type: **native system authoring contract**.

Use this standard for Questline/Awaken Realms proprietary systems and other FoA internals that a technically capable reader cannot safely use without understanding ownership, lifecycle, data contracts, failure states, and proof boundaries.

The goal is to translate reverse-engineered or otherwise inspected evidence into a **system model**, not a class dump or unexplained gate list.

## Required reasoning order

```text
1. explain the purpose
2. place the system in FoA
3. identify native owners and identities
4. explain the input/data contract
5. trace the complete lifecycle
6. trace the normal vanilla route
7. identify bounded mod entry seams
8. explain accepted / rejected / deferred states
9. preserve failure history
10. define validation and proof boundaries
```

A process is only understandable after the reader understands the system the process must satisfy.

## Mandatory sections

### 1. What this system is

Explain:
- purpose;
- problem it solves for FoA;
- what it is not;
- simplest useful mental model;
- where it sits in the broader stack.

Do not begin with a wall of type names.

### 2. Where it sits in FoA

Show the upstream and downstream ownership chain.

Prefer a compact ownership flow:

```text
upstream definition / owner
→ system entry
→ internal lifecycle
→ downstream consumer
→ cleanup / persistence boundary
```

Name boundaries explicitly when ownership crosses systems.

### 3. Native owners, identities, types, and data contracts

Include only what is necessary to understand the contract:

- assemblies/namespaces;
- core types/managers/services;
- data structures;
- GUID/address/template identity;
- registry keys;
- relevant methods/fields;
- runtime IDs where meaningful.

For every important identity, explain what kind of identity it is.

For every important type, explain responsibility rather than merely listing it.

### 4. What the system expects

Answer:
- what must already exist?
- which references/counts/relationships must agree?
- which services/providers/scenes must be ready?
- what can change?
- what must remain stable?
- what is version-sensitive?

Label **native requirement** separately from **framework safety constraint**.

### 5. Lifecycle

Trace the complete lifecycle where applicable:

```text
creation / discovery
→ admission / registration request
→ queued / pending
→ validation
→ native registration
→ deferred finalisation
→ active use
→ update
→ disable / unregister
→ teardown / release
→ disposal
```

State which transitions are synchronous, deferred, repeated, or terminal when evidence supports that distinction.

### 6. Vanilla FoA route

Trace how the game itself enters and consumes the system.

The reader should understand the native success path before the mod path.

### 7. Safe mod entry

Document only evidence-backed seams.

For each seam:
- prerequisites;
- owner;
- action;
- downstream consumer;
- failure behaviour;
- cleanup;
- version/evidence scope.

Do not present private implementation detail as stable API.

### 8. Accepted, rejected, and deferred states

Explain:
- what is accepted;
- what is rejected;
- what is delayed until readiness;
- what produces partial/visible-but-not-integrated state;
- what failure looks like.

### 9. Failure history and diagnostics

Preserve meaningful failed or partial approaches when they explain the final model.

For each:
```text
attempt
→ observed failure
→ corrected ownership/lifecycle model
→ documentation rule
```

### 10. Verification

Separate proof lanes:

- static/source;
- build;
- loader;
- runtime;
- persistence;
- compatibility;
- release/package.

Never phrase one lane as proof of another.

### 11. Current proof boundary

End with:
- exact supported scope;
- current unknowns;
- version/runtime restrictions;
- unverified paths;
- related system/mechanic/reference links.

## Completeness rule

Do not shorten a proprietary-system page merely to make it cleaner.

The page is incomplete when a capable reader must still reverse engineer a material:

- owner;
- readiness condition;
- required identity/input;
- state transition;
- completion signal;
- rejection/defer behaviour;
- cleanup;
- or proof boundary

to use the documented route safely.

## Splitting rule

Large proprietary-system material may be split into a chapter family, but the domain hub must preserve a complete end-to-end reading route.

Split by independently reusable responsibility, not file size.

Related:
- [Mandatory documentation architecture](DOCUMENTATION_ARCHITECTURE.md)
- [Mandatory publication workflow](PUBLICATION_WORKFLOW.md)
