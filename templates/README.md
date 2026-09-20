# Project Templates

Starter projects for Tainted Grail: The Fall of Avalon mods.

The templates contain source and project structure only. They expect you to provide your own local game, loader, framework, or Merlin files and do not redistribute proprietary game assemblies or commercial assets.

## Named mod-family templates

[31 cross-runtime mod-family templates](mod-families/README.md) provide starting points for existing mod families such as Avalon AI FoA Host, Avalon Cheat Panel, Avalon Stash, FoA Mod Manager, Tainted Interface, Tainted Performance, Wyrd Hunt, Tainted Music, and others.

Each named template includes:

- shared starter code appropriate to that mod family;
- a Mono/BepInEx 5 host;
- an IL2CPP/BepInEx 6 host;
- Tainted Framework runtime integration where needed;
- notes explaining which parts are shared and which parts still need a verified FoA-specific hook or owner.

The shared starter code deliberately avoids game-specific types until you add the exact FoA integration your feature requires.

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

## Dual-runtime and mixed workflows

- [Tainted Framework dual-runtime consumer](hybrid/tainted-framework-consumer/) — shared feature code with thin Mono and IL2CPP hosts.
- [Mono + Merlin](hybrid/mono-merlin/) — keeps Merlin-authored content and BepInEx runtime code as separate parts of one project.

## Merlin

- [Merlin overlay](merlin/basic/) — starter content layered into the official Merlin Workshop project.

See the [Template Source-Family Map](SOURCE-MAP.md) for provenance.

A template is a starting point, not proof that a feature works in the current game build. Use [examples](../examples/README.md) for focused code demonstrations and [tooling](../platform/README.md) for shared infrastructure.
