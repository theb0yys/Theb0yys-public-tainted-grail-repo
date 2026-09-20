# Better Bonfire Menu Public Template

Use this starter when you want to build a bonfire service menu without rebuilding the services themselves. Shared code can own menu/configuration rules; runtime hosts should route actions to the exact native bonfire, shop, crafting, storage, and UI owners.

Source family: `better-bonfire-menu`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
