# IL2CPP Templates

IL2CPP/BepInEx 6 starters are grouped by functional domain.

- [Basic plug-in](basic/)
- [Harmony](harmony/README.md)
- [Audio](audio/README.md)
- [Diagnostics / performance](diagnostics/README.md)
- [UI](ui/README.md)

For a mod intended to ship on both Mono and IL2CPP through Tainted Framework, use the [dual-runtime framework consumer](../hybrid/tainted-framework-consumer/). Keep shared feature logic outside the host-specific entry points.
