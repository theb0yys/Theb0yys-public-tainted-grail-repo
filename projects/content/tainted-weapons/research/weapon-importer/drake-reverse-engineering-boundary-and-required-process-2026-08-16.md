# Drake Reverse-Engineering Boundary and Required Process — 2026-08-16

Document control:

- Status: `RESEARCH_REQUIRED`
- Authority state: `reviewed-planning-boundary`
- Owner: Tainted Weapons / Weapon Importer
- Architecture owner path: `documents/frameworks/weapon-importer/`
- Implementation owner path: `mods/tainted-weapons/`
- Canonical project branch: `tainted-weapons`
- Evidence model: repository `E0`–`E7`
- Evidence domains: `decompilation-static` then `internal-game`
- Drake reverse engineering: `NOT_RUN`
- Drake runtime validation: `NOT_RUN`
- Presentation implementation authority from Drake evidence: `NOT_GRANTED`
- Persistence/compatibility/release authority: `NOT_GRANTED`

## Purpose

This record prevents a critical research-state ambiguity from reappearing in the weapon-importer programme.

The current project has identified the assembly ownership boundary for the relevant Drake types, but it has **not reverse-engineered the authoritative Drake implementation** to the level required for weapon-importer implementation.

The following statements must remain distinct:

```text
Drake ownership location identified
!=
Drake implementation reverse-engineered
!=
Drake runtime lifecycle proven
!=
weapon presentation adapter validated
```

No earlier report, current source assumption, nearby assembly inspection, successful renderer appearance, or agent synthesis may collapse these states.

## Current evidence state

### What is established

Existing static investigation supports only this bounded conclusion:

- relevant Drake manager, renderer, authoring, and component references resolve to the `Awaken.ECS` assembly boundary;
- `TG.Main`, `HLOD`, and `MeshToTerrain` do not provide the authoritative owner implementation required for the importer contract;
- current Tainted Weapons code and historical weapon-importer research contain Drake-facing assumptions, interventions, and presentation hypotheses;
- those assumptions and hypotheses remain research context until matched against the authoritative owner assembly and runtime evidence.

This is **ownership discovery**, not reverse engineering of Drake.

### What is not established

```text
matching Awaken.ECS.dll identity          NOT_RUN / MISSING
Drake type and member inventory           NOT_RUN
manager ownership and singleton lifetime  NOT_RUN
renderer ownership model                  NOT_RUN
component storage and bitmask semantics   NOT_RUN
loading state machine                     NOT_RUN
load-start contract                       NOT_RUN
partial-load states                       NOT_RUN
completion contract                       NOT_RUN
handle ownership                          NOT_RUN
graphics-ID allocation/lifetime           NOT_RUN
entity conversion path                    NOT_RUN
reference/load counters                   NOT_RUN
unload ordering                           NOT_RUN
entity/component teardown                 NOT_RUN
double-start protection                   NOT_RUN
failure-state recovery                    NOT_RUN
rollback capability                       NOT_RUN
scene/perspective lifetime interaction     NOT_RUN
FPP Drake path                            NOT_RUN
TPP Drake path                            NOT_RUN
inventory-preview Drake path              NOT_RUN
runtime event ordering                    NOT_RUN
controlled positive/negative validation   NOT_RUN
repeated lifecycle validation             NOT_RUN
```

These states must not be upgraded by inference from Tainted Weapons source, `TG.Main`, historical reports, or a single successful presentation observation.

## Hard evidence blocker

The first required technical input is the **matching `Awaken.ECS.dll` from the exact target FoA installation/build**.

Required admission metadata:

- file path/source location;
- SHA-256;
- assembly name and version;
- MVID;
- metadata/runtime track;
- exact FoA game/distribution build association;
- Mono/IL2CPP track;
- relevant dependency assembly identities;
- capture/inspection date.

Until that binary is admitted, the exact Drake contract remains:

```text
BLOCKED_PENDING_MATCHING_AWAKEN_ECS_OWNER_BINARY
```

A different assembly cannot substitute for the owner binary merely because it references Drake types.

## Required reverse-engineering process

### DR-0 — Environment and binary admission

Before interpreting Drake internals:

1. capture the fresh installed-game environment fingerprint;
2. fingerprint `Awaken.ECS.dll` and relevant companion assemblies;
3. bind all static findings to that exact binary set;
4. reject stale or unmatched decompilation evidence for current action claims.

Output: admitted static evidence packet.

### DR-1 — Authoritative type/member inventory

Perform a bounded exhaustive inventory of Drake-relevant types in `Awaken.ECS`.

At minimum map:

- manager/service types;
- renderer/authoring types;
- ECS component types;
- handles and identifiers;
- graphics IDs;
- load/start/completion state holders;
- counters/reference ownership structures;
- conversion entry points;
- unload/teardown entry points;
- reset/disposal paths;
- error/failure paths.

Record exact namespaces, type names, fields, properties, methods, signatures, and static ownership relationships.

Evidence class: `E5 decompiled-binary`.

### DR-2 — Static lifecycle reconstruction

Reconstruct the authoritative static call/data flow for:

```text
request/start
-> state admission
-> asset/load ownership
-> authoring/conversion
-> graphics/entity creation
-> completion publication
-> active ownership
-> unload request
-> reference/counter release
-> component/entity teardown
-> handle/graphics-ID release
-> reset/final state
```

For every transition record:

- caller and callee;
- preconditions;
- state fields read/written;
- counter changes;
- handle creation/destruction;
- graphics-ID creation/destruction;
- component/bitmask changes;
- exception/failure exits;
- whether rollback exists;
- whether repeated start/unload is guarded.

Do not infer runtime ordering solely from method layout.

### DR-3 — Tainted Weapons assumption comparison

Compare the reconstructed owner contract against every Drake-facing Tainted Weapons intervention and historical hypothesis.

Each intervention must receive one disposition:

```text
REQUIRED_BY_OWNER_CONTRACT
COMPATIBLE_BUT_UNNECESSARY
INCORRECT
UNPROVEN_RUNTIME_DEPENDENT
REMOVE_PENDING_VALIDATION
```

This comparison must specifically cover any private loading-start, bitmask, counter, handle, completion, unload, renderer, or presentation intervention.

No source mutation is authorised by this comparison alone.

### DR-4 — Runtime instrumentation design

Using the static owner map, define read-only or bounded instrumentation that can directly observe the required lifecycle without changing the lifecycle being measured.

Required runtime markers should include, where available:

- request identity;
- manager instance identity;
- state before start;
- load-start transition;
- handle identity;
- graphics ID;
- component/bitmask before/after;
- entity identity;
- completion transition;
- FPP/TPP/preview owner identity;
- unload request;
- counter before/after;
- teardown ordering;
- final zero/absent state;
- exception/failure state.

Output must carry the exact environment and binary fingerprints.

### DR-5 — Controlled runtime validation

Static reconstruction becomes action-grade only after controlled validation.

Minimum validation set:

1. vanilla positive control;
2. custom weapon positive control;
3. FPP equip/unequip;
4. TPP equip/unequip;
5. inventory-preview open/close;
6. perspective transition;
7. weapon swap;
8. duplicate/repeated request protection;
9. partial/failure path where safely reproducible;
10. repeated load/unload lifecycle;
11. scene/camera ownership transition where relevant;
12. final teardown/counter symmetry.

Evidence class: `E7 controlled-validation` for action claims.

A single visible weapon is insufficient.

### DR-6 — Reviewed Drake contract

After DR-0 through DR-5, produce a reviewed contract containing:

- exact supported environment tuple;
- authoritative type/member map;
- lifecycle state machine;
- ownership ledger;
- counter/handle/graphics-ID invariants;
- supported FPP/TPP/preview paths;
- failure and terminal states;
- unload/teardown policy;
- known exclusions;
- exact allowed implementation use.

This contract must complete the applicable RH1/RH2/RH3/RH4 review stages and human promotion before it becomes current action authority.

## Importer implementation gate

The Drake presentation adapter must not proceed from ownership discovery alone.

Required gate:

```text
matching Awaken.ECS admitted
-> DR-1 static inventory PASSED
-> DR-2 lifecycle reconstruction PASSED
-> DR-3 assumption comparison PASSED
-> DR-4 instrumentation reviewed
-> DR-5 controlled validation PASSED for claimed scope
-> independent review PASSED
-> human promotion for exact Drake contract
-> presentation-adapter implementation planning
```

If any transition is missing, the presentation adapter remains blocked for that unresolved claim.

## Evidence-lane separation

### `decompilation-static`

May establish:

- exact owner types;
- fields/methods/signatures;
- static call/data relationships;
- candidate lifecycle and ownership invariants.

It may not by itself establish runtime ordering, lifecycle success, perspective behaviour, persistence, compatibility, or performance.

### `internal-game`

May establish observed lifecycle behaviour for the exact captured environment.

It does not replace static ownership evidence or generalise outside the tested matrix.

### `external-research`

May provide Unity/ECS concepts, terminology, tooling practice, or research leads.

It must not substitute for the exact installed `Awaken.ECS` binary or FoA runtime proof.

## Preservation rule

Historical weapon-importer documents that discuss Drake remain preserved as research context. They must not be rewritten to imply that Drake reverse engineering was completed when it was not.

Future summaries must use language equivalent to:

```text
Drake ownership boundary: IDENTIFIED
Authoritative Awaken.ECS reverse engineering: NOT_RUN pending matching binary
Runtime Drake lifecycle proof: NOT_RUN
Presentation adapter authority: NOT_GRANTED
```

Do not shorten this to "Drake researched" or "Drake contract known" unless the required evidence and review stages have actually passed.

## Downstream relationship

This record does not bypass the current ordered weapon-importer governance sequence.

The existing RH3 review of the research-governance reconciliation remains the next governance transition. After the applicable process promotion and fresh environment admission, this Drake process becomes the controlling technical research lane for the presentation-adapter dependency.

No implementation, native mutation, save work, compatibility claim, consumer migration, or release claim is authorised by this document.
