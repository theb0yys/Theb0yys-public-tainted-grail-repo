# Tainted Framework Dual-Runtime Consumer

Starter for mods that share feature logic across **Mono/BepInEx 5** and **IL2CPP/BepInEx 6** while using Tainted Framework as the common runtime dependency.

```text
shared/SharedFeature.cs
        | compiled into both hosts
        |
   +----+----+
   |         |
 mono/     il2cpp/
 host       host
```

The host owns loader-specific entry points and references. Shared feature code stays free of BepInEx host classes and receives the runtime kind explicitly.

## Build Mono

```powershell
dotnet build .\mono\TaintedFrameworkConsumer.Mono.csproj -c Release \
  -p:GameRoot="C:\Path\To\Tainted Grail FoA" \
  -p:TaintedFrameworkDir="C:\Path\To\TaintedFramework"
```

## Build IL2CPP

```powershell
dotnet build .\il2cpp\TaintedFrameworkConsumer.IL2CPP.csproj -c Release \
  -p:GameRoot="C:\Path\To\Tainted Grail FoA" \
  -p:TaintedFrameworkDir="C:\Path\To\TaintedFramework"
```

Expected framework input: `Tainted.Abstractions.dll`.

This starter demonstrates the host boundary only. A Tainted Framework capability is consumer-ready only when its public tooling contract says so. See [Tainted Framework tooling](../../../tooling/tainted-framework/README.md).

If shared feature logic needs per-frame or IMGUI callbacks on IL2CPP, combine this pattern with the [IL2CPP registered-behaviour UI starter](../../il2cpp/ui/runtime-overlay/).
