# Project Templates

Source-only starters for Tainted Grail: The Fall of Avalon mod authors. These templates reference locally supplied game, loader, framework, and toolkit files; they do not redistribute proprietary assemblies or commercial assets.

## Named mod-family templates

- [31 cross-runtime mod-family templates](mods/README.md) — named starters for Avalon AI FoA Host, Avalon Cheat Panel, Avalon Stash, CarryWeightTweaks, Dungeon Exit Marker, Easy Avalon, FoA Mod Manager, Hold to Steal, Immersive Backgrounds, Immersive Footsteps, Immersive HUD, Immersive Progression, Jump Higher, Lockpicking Reforged, Magic Tweaks, Multi-Pin Map Notes, No Fall Damage, Origins of Avalon, Rich Merchant, StaminaControl, Tainted Combat, Tainted Interface, Tainted Performance, Wyrd Hunt, Avalon Companions, Tainted Music, Tainted Diagnostic Tool, Better Bonfire Menu, Merchant Stock Tweaks, Tainted Core, and Avalon Human Companions.

Each named template is self-contained with:
- **real shared starter logic** for that feature family;
- a Mono/BepInEx 5 host;
- an IL2CPP/BepInEx 6 host;
- Tainted Framework runtime-kind integration;
- source-family/mechanism notes.

The shared code deliberately stays free of game-specific types until you add the exact verified FoA owner/hook required by the feature.

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

See [Template Source-Family Map](SOURCE-MAP.md) for provenance. Templates are starting points, not runtime, persistence, compatibility, or release proof. Use [examples](../examples/README.md) for mechanism demonstrations and [tooling](../tooling/README.md) for shared infrastructure contracts.
