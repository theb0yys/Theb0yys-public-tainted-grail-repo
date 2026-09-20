# Tainted Diagnostic Tool Public Template

Use this starter for a read-only diagnostics tool that needs to inspect both Mono and IL2CPP builds.

Keep reporting and query logic shared; runtime hosts should own the exact object access and interop needed to collect evidence safely.

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
