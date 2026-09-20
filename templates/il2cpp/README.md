# IL2CPP Templates

Starter projects for IL2CPP builds using BepInEx 6.

- [Basic plug-in](basic/)
- [Harmony](harmony/README.md)
- [Audio](audio/README.md)
- [Diagnostics and performance](diagnostics/README.md)
- [UI](ui/README.md)

Pick the template closest to what you are building, then add only the IL2CPP-specific interop and game references the feature actually needs.

For a feature that supports both Mono and IL2CPP through Tainted Framework, use the [dual-runtime framework consumer](../hybrid/tainted-framework-consumer/) and keep shared feature logic outside the runtime-specific host projects.
