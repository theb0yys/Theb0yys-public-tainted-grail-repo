# Project Templates

Source-only starters for Tainted Grail: The Fall of Avalon mod authors. These templates reference locally supplied game, loader, framework, and toolkit files; they do not redistribute proprietary assemblies or commercial assets.

## Named mod-family templates

- [31 selected cross-runtime starter families](mod-families/README.md) — reusable starters distilled from a subset of the larger development workspace.

These 31 are **not** the complete mod inventory. See the [106-project example catalogue](../examples/mod-catalog/README.md) for the full current project set and the pattern each project demonstrates.

Each named template includes shared starter logic, a Mono/BepInEx 5 host, an IL2CPP/BepInEx 6 host, Tainted Framework runtime-kind integration, and source-family/mechanism notes.

The shared code stays free of game-specific types until you add the exact verified FoA system or hook required by the feature.

## Mono / BepInEx 5

- [Basic plug-in](mono/basic/)
- [Harmony patterns](mono/harmony/README.md)
- [Audio replacement gate](mono/audio/replacement-gate/)
- [Combat observers](mono/combat/README.md)
- [Illegal pickup guard](mono/items/illegal-pickup-guard/)
- [Magic projectile speed](mono/magic/projectile-speed/)
- [Skybox ownership](mono/rendering/skybox-ownership/)
- [Runtime UI overlay](mono/ui/runtime-overlay/)

See [all Mono templates](mono/README.md).

## IL2CPP / BepInEx 6

- [Basic plug-in](il2cpp/basic/)
- [Harmony patterns](il2cpp/harmony/README.md)
- [Runtime UI overlay](il2cpp/ui/runtime-overlay/)
- [Frame/performance sampler](il2cpp/diagnostics/frame-sampler/)
- [Audio replacement gate](il2cpp/audio/replacement-gate/)

See [all IL2CPP templates](il2cpp/README.md).

## Dual-runtime / framework-backed

- [Tainted Framework dual-runtime consumer](hybrid/tainted-framework-consumer/) — shared feature source with thin Mono and IL2CPP hosts.
- [Mono + Merlin](hybrid/mono-merlin/) — separate Merlin-authored content and BepInEx runtime lifecycles.

## Merlin

- [Merlin overlay](merlin/basic/) — owned content root layered into the official Merlin Workshop project.

See [Template Source-Family Map](SOURCE-MAP.md) for provenance. Templates are starting points, not runtime, persistence, compatibility, or release proof. Use [examples](../examples/README.md) for working mechanisms and [tooling](../platform/README.md) for shared infrastructure.
