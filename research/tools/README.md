# Research Tools

Source-only tools that support evidence collection and validation.

- [Runtime tracer](runtime-tracer/README.md) — configurable Mono/BepInEx 5/Harmony method observation.
- [Symbol anchors](symbol-anchors/README.md) — extract source-declared Harmony targets and verify them against local Mono assemblies.
- [Harmony runtime audit](harmony-runtime-audit/README.md) — inspect the live Harmony patch table and owner IDs in Mono/BepInEx 5.

Each tool answers a bounded technical question. Keep symbol identity, patch installation, invocation, feature behavior, persistence and release claims separate; see the repository's [evidence standards](../../contributing/evidence-standards.md).

## Runtime-lane scope

Symbol-anchor extraction is source-based and covers Mono, IL2CPP, hybrid and unspecified source lanes. Symbol verification can inspect Mono managed assemblies or generated IL2CPP interop assemblies through the runtime filter.

The current in-process runtime tracer and Harmony live-audit hosts target Mono / BepInEx 5. Their core Harmony concepts transfer to IL2CPP, but the BepInEx 6 IL2CPP host lifecycle and interop behavior require separate host implementations rather than treating the Mono binaries as cross-runtime tools.

