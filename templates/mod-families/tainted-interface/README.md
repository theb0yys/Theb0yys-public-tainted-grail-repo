# Tainted Interface Public Template

Use this starter for shared UI resources or presentation infrastructure across both runtimes.

Keep semantic resource/catalog logic common, and put runtime-specific registration or FoA integration in the Mono and IL2CPP hosts.

## Reusable mechanisms

- shared UI styling
- semantic icon catalogue
- custom UI scope
- inventory/UI extension surfaces

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/TaintedInterface.Mono.csproj
il2cpp/Plugin.cs
il2cpp/TaintedInterface.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
