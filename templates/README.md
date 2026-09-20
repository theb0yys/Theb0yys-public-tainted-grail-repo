# Project Templates

Source-only starters for Tainted Grail: The Fall of Avalon mod authors. These templates reference locally supplied game, loader, framework, and toolkit files; they do not redistribute proprietary assemblies or commercial assets.

## Mono / BepInEx 5

- [Mono templates](mono/README.md)
  - basic plug-in
  - Harmony patterns
  - audio replacement gate
  - combat observers
  - item interaction guard
  - magic projectile tuning
  - skybox ownership
  - runtime UI overlay

## IL2CPP / BepInEx 6

- [IL2CPP templates](il2cpp/README.md)
  - basic plug-in
  - Harmony patterns
  - runtime UI overlay
  - frame/performance sampler
  - audio replacement gate

## Dual-runtime / framework-backed

- [Tainted Framework dual-runtime consumer](hybrid/tainted-framework-consumer/) — shared feature source with thin Mono and IL2CPP hosts.
- [Mono + Merlin](hybrid/mono-merlin/) — separate Merlin-authored content and BepInEx runtime lifecycles.

## Merlin

- [Merlin overlay](merlin/basic/) — owned content root layered into the official Merlin Workshop project.

Templates are starting points, not runtime, persistence, compatibility, or release proof. Use [examples](../examples/README.md) for mechanism demonstrations and [tooling](../tooling/README.md) for shared infrastructure contracts.
