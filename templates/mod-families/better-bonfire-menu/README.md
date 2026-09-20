# Better Bonfire Menu Public Template

Use this starter for a bonfire-services menu or similar hub UI.

It provides shared feature structure plus separate runtime hosts for connecting to native bonfire services and UI. Reuse the game's service owners rather than reimplementing storage, crafting, saving, travel, or merchant transactions.

## Reusable mechanisms

- bonfire services menu
- native submenu integration
- stash/craft/merchant service bridges
- shared UI

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/BetterBonfireMenu.Mono.csproj
il2cpp/Plugin.cs
il2cpp/BetterBonfireMenu.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
