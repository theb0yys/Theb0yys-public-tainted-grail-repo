# IL2CPP Templates

IL2CPP/BepInEx 6 starters:

- [Basic plug-in](basic/)
- [Harmony patterns](harmony/README.md) — basic patching and result postfix.
- [Audio replacement gate](audio/replacement-gate/)
- [Frame/performance sampler](diagnostics/frame-sampler/)
- [Runtime UI overlay](ui/runtime-overlay/)

For a mod intended to ship on both Mono and IL2CPP through Tainted Framework, use the [dual-runtime framework consumer](../hybrid/tainted-framework-consumer/). Keep shared feature logic outside the host-specific entry points.
