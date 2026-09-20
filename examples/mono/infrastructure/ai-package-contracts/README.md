# Avalon AI V2 Package Contract Example

This is an **inert provider-neutral package library**, not a BepInEx plugin and not an AI host.

It demonstrates the minimum `IAvalonAiPackage` shape while declaring no goals/actions/capabilities.

## Build

Point `AvalonAiContractsDir` at the directory containing `AvalonAI.Contracts.V2.dll`:

```powershell
dotnet build .\AiPackageContracts.csproj -c Release \
  -p:AvalonAiContractsDir="C:\path\to\AvalonAI"
```

The important boundary is visible in the project itself: it references only the provider-neutral Contracts assembly.

A real package adds reviewed goals/actions/blackboard declarations and is then integrated through the single Avalon AI Runtime/FoA host path.
