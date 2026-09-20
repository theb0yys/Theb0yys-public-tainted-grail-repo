# Recipe: Read-Only Avalon Core Discovery

Use this when your mod wants shared ecosystem metadata without taking execution authority.

## Dependency

```csharp
[BepInDependency(
    AvalonCore.Plugin.PluginGuid,
    BepInDependency.DependencyFlags.HardDependency)]
```

## Trust-report read

```csharp
AvalonCore.HostTrustReportSnapshot snapshot =
    AvalonCore.Plugin.TrustReports;

if (snapshot.WouldMutateRuntime)
{
    // This integration expects read-only state. Fail closed.
    return;
}
```

## Exact engine lookup

```csharp
if (AvalonCore.Plugin.Registry == null ||
    !AvalonCore.Plugin.Registry.TryGet(
        "adapter-registry",
        out AdapterRegistryEngine? registry) ||
    registry == null)
{
    // action=none
    return;
}
```

Then query the **exact documented capability/contract/version/readiness** you need.

If execution is required, identify the named executor/service owner.

Do not reflect into provider internals because discovery said “missing” or “blocked”.
