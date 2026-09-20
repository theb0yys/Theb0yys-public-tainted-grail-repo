# Tainted Diagnostic Tool Public Template

Use this starter when you are building a diagnostics tool that should inspect both Mono and IL2CPP without mutating gameplay. Keep report/query logic shared and put exact runtime access in the corresponding host.

Source family: `Tainted-Diagnostic Tool`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- runtime dumps
- template diagnostics
- evidence receipts
- framework runtime reports

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/TaintedDiagnosticTool.Mono.csproj
il2cpp/Plugin.cs
il2cpp/TaintedDiagnosticTool.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
