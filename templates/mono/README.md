# Mono Templates

Mono/BepInEx 5 starters:

- [Basic plug-in](basic/)
- [Harmony patterns](harmony/README.md) — basic lifecycle, result postfix, and action guard.
- [Audio replacement gate](audio/replacement-gate/)
- [Combat observers](combat/README.md) — damage and death observation.
- [Illegal pickup guard](items/illegal-pickup-guard/)
- [Magic projectile speed](magic/projectile-speed/)
- [Skybox ownership](rendering/skybox-ownership/)
- [Runtime UI overlay](ui/runtime-overlay/)

For a mod intended to ship on both Mono and IL2CPP through Tainted Framework, start with the [dual-runtime framework consumer](../hybrid/tainted-framework-consumer/) instead of duplicating feature code.
