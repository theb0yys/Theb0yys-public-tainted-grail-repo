# Mono Templates

Starter projects for Mono builds using BepInEx 5.

- [Basic plug-in](basic/)
- [Harmony](harmony/README.md)
- [Audio](audio/README.md)
- [Combat](combat/README.md)
- [Items](items/README.md)
- [Magic](magic/README.md)
- [Rendering](rendering/README.md)
- [UI](ui/README.md)

Pick the template closest to what you are building, then replace the placeholder integration with the exact FoA type or hook your feature needs.

If one feature must support both Mono and IL2CPP through Tainted Framework, start with the [dual-runtime framework consumer](../hybrid/tainted-framework-consumer/) so the shared feature code is not duplicated.
