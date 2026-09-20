---
document_type: framework
scope: Avalon Awakened creature resolver boundary
runtime: mono
evidence:
  static: SOURCE_INSPECTED
last_verified: 2026-09-20
---

# Avalon Awakened Creature Resolver Boundary

The shared creature-companion code can resolve a creature `LocationTemplate` through an Avalon Awakened project API before invoking the native spawn path.

The consumer verifies:

- resolver type exists;
- expected public static method exists;
- expected by-reference signature matches;
- invocation succeeds before the result is trusted.

## Important classification

This is a **project API boundary**, not a native FoA resolver contract.

Document it under tooling/frameworks so consumers do not mistake a shared project abstraction for Questline's underlying native API.

The native actor lifecycle begins only after the resolved template is handed to the FoA-owned spawn/location path.
