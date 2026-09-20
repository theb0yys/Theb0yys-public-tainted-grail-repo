---
document_type: framework
scope: declarative adapter/capability discovery
last_verified: 2026-09-20
---

# Adapter and Capability Contracts

A reusable adapter registry should describe **what a provider claims to offer** separately from **whether execution is currently permitted**.

Useful capability metadata includes:

- provider plugin identity;
- contract ID;
- contract version;
- operating mode;
- evidence level;
- validation level;
- approval/readiness state;
- save impact;
- default-enabled state;
- required safety gates.

## Versioning

Service contracts should have stable IDs and explicit major/minor compatibility rules.

A practical policy:

- additive compatible fields/behaviour → minor version;
- changed meaning, removed fields or changed defaults → new major version or compatibility shim.

## Mutation guardrail

A descriptor may describe a future mutation capability while keeping it:

- disabled by default;
- blocked/not-ready;
- non-executable through the registry.

This prevents “the registry can see it” from becoming “the host may run it”.
