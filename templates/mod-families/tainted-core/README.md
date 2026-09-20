# Tainted Core's Public Template

Use this starter for shared core infrastructure that must provide the same high-level capability across both runtimes.

Keep contracts and portable logic common, and isolate loader/runtime-specific discovery, registration, and FoA access in the host projects.

## Reusable mechanisms

- adapter registry
- trust reports
- capability contracts
- read-only consumer discovery

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/TaintedCore.Mono.csproj
il2cpp/Plugin.cs
il2cpp/TaintedCore.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
