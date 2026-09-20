# Shared Infrastructure Runtime Compatibility

Checked: **2026-09-20**

This page answers a narrower question than the general compatibility guide:

> Which shared infrastructure components are intended for Mono, IL2CPP, or both, and what may a mod author safely claim?

Runtime support, API stability and feature validation are separate.

## Runtime lanes

- **Mono** — FoA Mono runtime with BepInEx 5.
- **IL2CPP** — FoA IL2CPP runtime with BepInEx 6 / Il2CppInterop.
- **Cross-runtime contract** — shared API/contract intended to keep feature logic independent from the loader-specific host.

## Current public support matrix

| Component | Mono | IL2CPP | Author interpretation |
| --- | --- | --- | --- |
| FoA Mod Manager | **PUBLICLY SUPPORTED** | **PUBLICLY SUPPORTED** | Nexus supplies runtime-specific Main files and states the same user-facing feature set is supported on both. If your mod consumes a direct API, validate the exact API calls on every runtime you claim. |
| Tainted Interface | **PUBLICLY SUPPORTED** | **PUBLICLY SUPPORTED** | Shared semantic UI/resources are distributed for both runtimes. Runtime-specific installation remains package-owned; consumers should stay on the public semantic API. |
| Tainted Core / Avalon Core | **PUBLICLY SUPPORTED** | **PUBLICLY SUPPORTED** | Install the complete Main package matching the runtime. Do not infer IL2CPP package details from the older Mono-oriented source-side release manifest. |
| Tainted Framework | **PUBLICLY SUPPORTED** | **PUBLICLY SUPPORTED** | This is the common cross-runtime dependency layer. Shared contracts stay runtime-neutral; Mono and IL2CPP hosts handle loader/runtime differences behind that boundary. |
| Avalon AI FoA Host | **PUBLICLY SUPPORTED** | **PUBLICLY SUPPORTED** | The public distribution advertises both runtimes and uses Tainted Framework for IL2CPP. Detailed technical text remains more Mono-oriented, so validate the exact runtime-specific Main package and package/contract combination you ship against. |
| Tainted Diagnostic Tool | **PUBLICLY SUPPORTED** | **PUBLICLY SUPPORTED** | Install the Main file matching the runtime. Diagnostic output is evidence only and does not become gameplay authority. |
| FOA-SDK | **NOT A BepInEx RUNTIME PACKAGE** | **NOT A BepInEx RUNTIME PACKAGE** | Public pre-alpha source/authoring platform. Its existence does not prove either runtime adapter is release-ready. |

Avalon Contracts and the Tainted Grail Extender host are intentionally not expanded here while their standalone public distribution remains outside the current author-ready surface.

## Tainted Framework is the cross-runtime boundary

For cross-runtime feature mods, the preferred shape is:

```text
feature/domain logic
    ↓
Tainted Framework shared contract
    ↓
runtime-specific framework host
    ├─ Mono / BepInEx 5
    └─ IL2CPP / BepInEx 6 + Il2CppInterop
```

Do not duplicate loader-specific branches throughout the whole feature when the promoted framework contract already owns that distinction.

Also do not assume every framework internal service is cross-runtime consumer-ready. Runtime support and capability promotion are separate. See [API stability and capability promotion](api-stability.md).

## Compatibility claim levels

Use the narrowest claim supported by your evidence.

### Publicly supported

The current public distribution states that the component is supplied for that runtime.

This is enough to tell users which package family to obtain. It is **not** proof that your own integration works.

### Build validated

Your consumer compiles against the intended runtime/package references.

This does not prove the plugin loads.

### Loader validated

The packaged consumer and dependency load together under the intended BepInEx/runtime lane.

This does not prove the feature path executes.

### Runtime proven

The exact integration path executes correctly in the target runtime.

### Compatibility tested

The same advertised behavior has been separately tested across every runtime/build combination you claim.

Do not turn a Mono runtime test plus an IL2CPP build into a cross-runtime feature claim.

## What a mod should record

For every published runtime claim, record:

- game build/version;
- Mono or IL2CPP;
- BepInEx version/backend;
- feature-mod version/commit;
- infrastructure package version actually installed;
- public contract/API used;
- whether the dependency is hard or optional;
- build result;
- loader result;
- exact feature result;
- cleanup/unregister result;
- persistence result if relevant;
- packaged-release result;
- anything not tested.

## Dependency declarations

### Hard dependency

Use a hard dependency when the advertised feature cannot function correctly without the shared owner.

```csharp
[BepInDependency(
    "kane.tgfoa.tainted-framework",
    BepInDependency.DependencyFlags.HardDependency)]
```

Only use a minimum version when you have evidence for the lower bound. Do not write `latest` as a compatibility contract.

### Optional integration

If the base feature works correctly without the dependency, keep the integration isolated and fail closed when the dependency/API is unavailable.

Do not use reflection to disguise a real hard dependency.

## Runtime-specific code

Some feature work will still require runtime-specific implementation.

Keep that code at the narrowest boundary possible:

```text
shared feature logic
  ↓
shared contract/service
  ↓
small Mono adapter | small IL2CPP adapter
```

Do not let runtime-specific BepInEx, Unity or Il2CppInterop types leak into shared contracts unless the contract explicitly requires them.

## Example evidence boundary

The current `examples/mono/infrastructure/` projects are **Mono integration examples**. They demonstrate dependency shape and API usage; they do not by themselves prove IL2CPP compatibility.

An IL2CPP example should only be added when its exact references, loader path and validation state are available. Do not mechanically rename a Mono project and call it cross-runtime.

## Related references

- [Public distribution and versioning](distribution-and-versioning.md)
- [API stability and capability promotion](api-stability.md)
- [Dependency and packaging](dependency-and-packaging.md)
- [General compatibility validation](../../how-to/compatibility/validation.md)