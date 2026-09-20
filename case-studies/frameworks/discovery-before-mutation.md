---
document_type: case
scope: Avalon Core read-only public baseline
evidence:
  runtime: LIVE_LOAD_VALIDATED_READ_ONLY
last_verified: 2026-09-20
---

# Discovery Before Mutation

Avalon Core deliberately established a public consumer baseline before exposing gameplay execution.

A sample consumer proved:

- BepInEx dependency/load order;
- read-only trust report access;
- declarative descriptor registration;
- adapter/capability queries;
- service-contract compatibility;
- fail-closed guardrails;
- zero runtime-mutation capabilities.

## Lesson

A shared framework becomes easier to trust when discovery/version/error semantics are proven before mutation is added.
