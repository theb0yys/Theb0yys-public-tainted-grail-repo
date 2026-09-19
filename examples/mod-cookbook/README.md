# Tainted Grail Modding Cookbook

This cookbook is organised by the kind of mod you want to build.

Only working, reusable paths belong here. Internal research notes, unfinished hook ideas and untested procedures are not part of the cookbook.

## Sections

| Section | Working patterns |
| --- | --- |
| [Gameplay](gameplay/README.md) | projectile tuning, theft guards, character damage and death hooks |
| [Graphics](graphics/README.md) | runtime visual ownership patterns |
| [Visual effects](visual-effects/README.md) | damage/death VFX sidecars |
| [Audio](audio/README.md) | hero footstep replacement |
| [UI & HUD](ui-hud/README.md) | runtime overlay ownership |
| [Systems](systems/README.md) | Harmony guards/postfixes and reusable ownership rules |

For the reusable implementation shapes shared across several mod types, see [Proven path examples](../proven-paths/README.md).

## Runtime lane

The game-target examples in this cookbook use the **Mono / BepInEx 5** path.

Build against your own local game references. Do not redistribute game DLLs, Unity DLLs, BepInEx binaries or game assets.

## Build pattern

~~~powershell
dotnet build .\Example.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Each recipe explains the game owner it relies on and the current limitation that matters to the modder.
