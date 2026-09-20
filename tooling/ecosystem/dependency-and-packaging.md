# Shared Infrastructure Dependency and Packaging Rules

The shared infrastructure should behave like **installed platform dependencies**, not libraries copied into every mod zip.

## Required integration: use a real dependency

If your advertised feature requires the infrastructure to function correctly:

- reference the public assembly directly;
- declare the matching BepInEx hard dependency;
- keep the reference `Private=false` / copy-local disabled;
- fail load rather than silently running a broken half-feature.

Example:

```csharp
[BepInDependency(
    "kane.tgfoa.mod-manager",
    BepInDependency.DependencyFlags.HardDependency)]
```

Use the exact GUID from the [component reference](component-reference.md).

## Optional integration: do not accidentally make it hard

If the integration is only an enhancement:

```text
base mod works alone
+ shared infrastructure present
→ enhancement enabled
```

Do not place direct references to optional infrastructure types throughout always-loaded feature code.

Prefer one of:

1. a small reflection bridge;
2. a separate optional integration assembly;
3. a provider-neutral shared contract that is already part of the required stack.

Recipe: [Optional infrastructure bridge](../recipes/optional-infrastructure.md).

## Do not redistribute duplicate shared DLLs

A feature release should normally **not** include second copies of:

- `FoAModManager.dll`;
- `TaintedInterface.dll`;
- `AvalonCore.dll` and support assemblies;
- Tainted Framework host/contracts;
- Avalon AI host/runtime;
- Avalon Contracts host;
- TGE host.

Ship your feature assembly and declare/document the dependency.

Duplicate shared DLLs create:

- version ambiguity;
- load-order problems;
- two packages claiming the same owner;
- difficult support reports;
- accidental downgrades.

## MSBuild pattern

```xml
<Reference Include="FoAModManager">
  <HintPath>$(FoAModManagerDir)\FoAModManager.dll</HintPath>
  <Private>false</Private>
</Reference>
```

Use configurable local build paths. Do not commit a developer-machine absolute path.

## Version discipline

For every direct infrastructure dependency record:

- verified public acquisition route;
- installed package version;
- plugin GUID;
- assembly/API surface;
- minimum contract/API version where one exists;
- Mono/IL2CPP applicability;
- what happens when incompatible;
- teardown/unregister behaviour.

Keep package version, BepInEx plugin version, assembly/file version and API-contract version separate. See [Public distribution and versioning](distribution-and-versioning.md).

If a component has no verified standalone public distribution, do not create a new third-party hard dependency on it and do not vendor private/shared DLLs as a workaround.

Use [API stability and capability promotion](api-stability.md) to confirm that the exact member/capability is an author-facing contract.

Use [Runtime compatibility](runtime-compatibility.md) before claiming Mono/IL2CPP parity. Tainted Framework is the common cross-runtime boundary, but your feature still needs claim-fit testing on every runtime you advertise.

Do not infer compatibility from the DLL loading successfully.

## Lifecycle discipline

If you register something, unregister it.

Examples:

- controller actions;
- status providers;
- contract providers;
- custom UI scopes;
- AI package/actor ownership where the host contract requires release;
- extension/service subscriptions.

Cleanup is part of the integration contract.
