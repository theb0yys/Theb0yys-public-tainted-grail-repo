# Origins of Avalon Public Template

Use this starter for Origins of Avalon-style content or gameplay features that need a shared core with separate runtime integration.

Keep portable rules and data in the shared project, and isolate exact FoA hooks, services, or content access in the Mono and IL2CPP hosts.

## Reusable mechanisms

- origin selection
- growth/progression provider
- journey state
- shared UI integration

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/OriginsOfAvalon.Mono.csproj
il2cpp/Plugin.cs
il2cpp/OriginsOfAvalon.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
