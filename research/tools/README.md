# Research Tools

Source-only tools that support evidence collection and validation.

- [Runtime tracer](runtime-tracer/README.md) — configurable Mono/BepInEx 5/Harmony method observation.
- [Symbol anchors](symbol-anchors/README.md) — extract source-declared Harmony targets and verify them against local Mono assemblies.
- [Harmony runtime audit](harmony-runtime-audit/README.md) — inspect the live Harmony patch table and owner IDs in Mono/BepInEx 5.

Each tool answers a bounded technical question. Keep symbol identity, patch installation, invocation, feature behavior, persistence and release claims separate; see the repository's [evidence standards](../../contributing/evidence-standards.md).
