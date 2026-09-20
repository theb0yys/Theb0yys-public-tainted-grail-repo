# Dual-Runtime Host Boundary

This example shows the **mechanism** behind a Mono + IL2CPP mod that shares feature logic while keeping loader/runtime entry points separate.

Copyable starter: [Tainted Framework dual-runtime consumer](../../templates/hybrid/tainted-framework-consumer/).

## Shape

```text
shared feature logic
        │
        ├─ Mono host    → BepInEx 5 / BaseUnityPlugin
        └─ IL2CPP host → BepInEx 6 / BasePlugin
```

The shared layer should depend on runtime-neutral/public contracts wherever possible.

The host layer owns:

- BepInEx entry point;
- runtime-specific references;
- loader lifecycle;
- Il2CppInterop types when required;
- registration of Unity behaviours on IL2CPP when required;
- runtime-specific teardown.

## Why Tainted Framework fits here

Tainted Framework is the common cross-runtime dependency boundary.

A shared feature can consume a promoted framework contract while the runtime-specific framework host handles Mono/IL2CPP differences behind that contract.

Do not assume that every internal framework service is promoted merely because both runtimes are supported.

See [Framework API stability](../../tooling/ecosystem/api-stability.md).

## Example decision

If a feature only needs the runtime kind:

```csharp
internal static string Describe(TaintedRuntimeKind runtimeKind)
{
    return $"Feature active. runtime={runtimeKind}";
}
```

The Mono and IL2CPP hosts supply the runtime value; the shared class does not import loader-specific base classes.

## Evidence boundary

A shared source file compiling into two projects proves source reuse only.

Cross-runtime feature compatibility requires separate build, loader and runtime evidence for each lane.

See [Runtime compatibility](../../tooling/ecosystem/runtime-compatibility.md).
