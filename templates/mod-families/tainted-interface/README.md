# Tainted Interface Public Template

Source family: `Tainted Interface`

This is a public, source-only starter distilled from the Tainted Interface mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

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
