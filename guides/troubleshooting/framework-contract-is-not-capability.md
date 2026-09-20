---
document_type: troubleshooting
scope: shared framework surface exists but requested gameplay action is unavailable
last_verified: 2026-09-20
---

# Contract Exists but the Capability Is Not Ready

A descriptor, interface, manifest or service ID proves that a **contract surface exists**.

It does not automatically prove:

- provider implementation;
- runtime activation;
- installed-game compatibility;
- save safety;
- consumer migration;
- release readiness.

## Diagnose

1. Identify the contract ID/version.
2. Identify the provider plugin/version.
3. Check advertised capability/readiness/evidence metadata.
4. Check whether execution is actually enabled.
5. Check exact runtime/build compatibility.
6. Check required safety gates.
7. Fail closed if any required layer is absent.

Do not bypass a blocked shared provider by duplicating its private mutation path in the consumer.
