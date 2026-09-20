# Hybrid / Dual-Runtime Templates

Hybrid templates keep distinct runtime or authoring lifecycles explicit instead of mixing them in one opaque project.

- [Tainted Framework dual-runtime consumer](tainted-framework-consumer/) — one shared feature source, thin Mono/BepInEx 5 host, thin IL2CPP/BepInEx 6 host.
- [Mono + Merlin](mono-merlin/) — Merlin-authored content plus a separate runtime plug-in.

A shared codebase does not make every API cross-runtime automatically. Keep loader, Unity, game-interop, and lifecycle differences in the host/adaptor layer.
