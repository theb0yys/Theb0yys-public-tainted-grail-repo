# Project Templates

Use this section when you want a **starter project you can copy and adapt**.

Templates give you project structure, loader setup, and starter logic. They are not proof that the final feature works in your game build; you still need to connect the template to the exact FoA owner, hook, service, or runtime API your feature requires.

All templates are source-only. They expect you to provide your own local game, loader, framework, or Merlin files and do not redistribute proprietary game assemblies or commercial assets.

## Named mod-family templates

[31 cross-runtime mod-family templates](mod-families/README.md) provide starting points based on existing FoA mod families such as Better Bonfire Menu, Tainted Performance, Avalon Stash, Immersive HUD, Magic Tweaks, Wyrd Hunt, and others.

Each named starter includes:

- shared feature logic for that kind of mod;
- a Mono/BepInEx 5 host;
- an IL2CPP/BepInEx 6 host;
- shared runtime-kind handling where needed;
- notes about the source pattern it was derived from and what still needs a verified FoA integration.

The shared code deliberately avoids hard-wiring game-specific types until the feature selects the exact FoA integration it needs.

## Mono / BepInEx 5

[Mono templates](mono/README.md) include:

- basic plug-in;
- Harmony patterns;
- audio replacement;
- combat observation;
- item interaction guards;
- magic projectile tuning;
- rendering ownership;
- runtime UI.

## IL2CPP / BepInEx 6

[IL2CPP templates](il2cpp/README.md) include:

- basic plug-in;
- Harmony patterns;
- runtime UI;
- diagnostics/performance sampling;
- audio replacement.

## Dual-runtime and mixed workflows

- [Tainted Framework dual-runtime consumer](hybrid/tainted-framework-consumer/) — shared feature code with thin Mono and IL2CPP hosts.
- [Mono + Merlin](hybrid/mono-merlin/) — Merlin-authored content and BepInEx runtime code kept as separate parts of one project.

## Merlin

- [Merlin overlay](merlin/basic/) — an owned content root designed to be layered into the official Merlin Workshop project.

See the [Template Source-Family Map](SOURCE-MAP.md) when you need provenance for a starter.

Use [Examples](../examples/README.md) when you only need to see one technique, and [Tooling and Shared Infrastructure](../platform/README.md) when the project should consume an existing shared service instead of implementing it locally.
