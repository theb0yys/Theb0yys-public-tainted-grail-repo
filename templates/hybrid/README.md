# Hybrid / Dual-Runtime Templates

Templates that share feature code across runtimes or combine separate authoring and runtime paths.

- [Named mod-family templates](../mod-families/README.md) — cross-runtime starters for the named mod families.
- [Tainted Framework dual-runtime consumer](tainted-framework-consumer/) — one shared feature source, thin Mono/BepInEx 5 host, and thin IL2CPP/BepInEx 6 host.
- [Mono + Merlin](mono-merlin/) — Merlin-authored content plus a separate runtime plug-in.

A shared codebase does not make every API cross-runtime automatically. Keep loader, Unity, game-interop, and lifecycle differences in the host/adaptor layer.
