# Native Item Registrar Ownership

The shared registrar exists as a **framework ownership direction**, not a universal “register anything” API.

A mature registrar must centralize:

- template readiness;
- source-template resolution;
- GUID/name ownership;
- cloning/validation;
- definition hashing;
- idempotency;
- collision rejection;
- native map insertion;
- provider readback;
- receipts;
- persistence/missing-package policy.

## Author rule

Do not create a second consumer-local registrar if the shared registrar is promoted for your exact lane.

Until the relevant runtime/persistence gates are explicitly ready, use the public [item mechanics](../../../knowledge/mechanics/items/register-custom-template.md) as evidence/architecture guidance rather than assuming a framework call is safe.
