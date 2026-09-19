# Tainted Grail Modding Cookbook

This cookbook is organized by **what you are trying to build**, not by example number.

The numbered example folders are retained as stable implementation units so existing links do not break. They are no longer the primary navigation model.

## Choose a section

| Section | Use it for |
| --- | --- |
| [Gameplay](gameplay/README.md) | player rules, combat, magic, items, inventory, economy, interactions, companions, mounts, NPC AI and world encounters |
| [Graphics](graphics/README.md) | camera comfort, post-processing, fog, visibility, culling, sky and weather rendering |
| [Visual effects](visual-effects/README.md) | combat VFX, blood/death presentation, spell effects, weapon trails and helper lighting |
| [Audio](audio/README.md) | footsteps, music, weapon/magic SFX and world audio |
| [UI & HUD](ui-hud/README.md) | native HUD changes, custom HUDs, menus, overlays and input/cursor ownership |
| [Systems](systems/README.md) | Harmony patterns, lifecycle, persistence, diagnostics, cross-mod APIs, asset loading and validation |
| [Research](research/README.md) | candidate hooks, unvalidated examples and explicit evidence gates |

For the working-mod lineage behind the strongest reusable mechanisms, see [Proven mechanics index](systems/PROVEN_MECHANICS.md).

## Evidence classes

This repository keeps two separate questions visible:

1. **How strong is the underlying mechanism evidence?**
2. **Has this exact public rewrite been executed?**

The formal labels are defined in [Testing and Evidence Status](../../docs/EVIDENCE.md).

- **RUNTIME_EVIDENCED** — the underlying mechanism produced useful observed runtime behaviour in the stated scope.
- **LOAD_EVIDENCED** — load/registration was observed, but useful feature behaviour was not fully established.
- **SOURCE_BUILD_EVIDENCED** — source path plus build evidence, without useful live feature proof.
- **SOURCE_CONFIRMED** — concrete source path exists, without a completed build/load/feature result.
- **STATIC_CONFIRMED** — editor/toolkit authoring contract confirmed from source.
- **NOT_RUN** — the exact public example or recipe was not executed.
- **NOT_PROVEN** — the claimed adaptation is not established.

A rewritten public example can therefore be **NOT_RUN** even when its underlying mechanism is **RUNTIME_EVIDENCED**.

## Cookbook classes

### Runtime-backed pattern

A mechanism has useful live evidence from a working implementation. The public rewrite still keeps its own exact execution status.

### Advanced pattern

The underlying path has useful runtime evidence, but the mechanism carries larger lifecycle, persistence, actor-ownership, save, performance or compatibility requirements.

### Research candidate

The target is useful for investigation, but the current evidence ceiling is source/build/load-only or otherwise incomplete. Research candidates are not presented as working gameplay recipes.

## Runtime lane

Most game-target code examples in this cookbook were investigated on the **Mono / BepInEx 5** lane.

If your installed game is IL2CPP, start with [First IL2CPP Game Change](../il2cpp-first-game-change/README.md), inspect your local generated interop, verify the current type/member signatures, and adapt the mechanism instead of assuming a Mono patch shape is portable.

## Build rule

A typical Mono example builds against local references:

~~~powershell
dotnet build .\Example.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Do not redistribute game assemblies, BepInEx binaries, generated interop assemblies, game assets or other proprietary content.

## Where the old numbers went

Examples such as `04-magic-projectile-speed-mono` and `23-character-damage-observer-mono` remain at their existing paths for compatibility. Each section links to those implementation folders by subject.

The number is an identifier, not a maturity level or category.
