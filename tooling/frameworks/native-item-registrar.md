---
document_type: framework
scope: shared native item registrar ownership
runtime: mono
evidence:
  static: CONTRACT_OWNERSHIP_ACCEPTED
  runtime: PROMOTION_BLOCKED
  persistence: BLOCKED
last_verified: 2026-09-20
---

# Native Item Registrar Ownership

The framework decision establishes **who should own the registrar contract**, not that a public production registrar is universally ready.

## Shared owner responsibilities

The registrar lane should centralize:

- template readiness;
- source-template resolution;
- profile validation;
- GUID/name ownership;
- cloning;
- field/attachment validation;
- definition hashing;
- idempotency;
- collision rejection;
- native map insertion;
- receipts;
- save-restoration availability;
- missing-package/migration policy.

## Explicitly separate gates

Contract ownership does not by itself authorize:

- public registrar API;
- consumer migration;
- runtime mutation;
- native map insertion;
- item grants;
- merchant/recipe mutation;
- save writes;
- live deployment;
- release/compatibility claims.

This separation prevents “we have an API shape” from being mistaken for “the game integration is proven”.
