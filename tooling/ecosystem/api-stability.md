# API Stability and Capability Promotion

This page defines what **public**, **supported**, **versioned** and **gated** mean in this repository.

The goal is to prevent a common failure mode:

> a type exists in a DLL, therefore it must be a supported mod-author API.

That conclusion is not valid.

## Stability labels

### VERSIONED PUBLIC CONTRACT

A named, explicitly versioned contract intended for third-party authors.

A consumer can bind to that contract version without depending on host implementation internals.

Current clear example:

- `AvalonAI.Contracts.V2`

A versioned contract still has runtime and capability requirements. Versioning does not grant execution authority.

### SUPPORTED PUBLIC SURFACE

Documented, author-facing API intended for real consumers.

It is supported for the documented use, but this label alone does **not** promise indefinite binary compatibility or semantic-version guarantees.

### PROMOTED CAPABILITY

A shared framework owns the implementation, but only a named capability has been promoted for consumers.

The rest of the framework remains gated.

### GATED

The implementation, type, method, capability or lifecycle may exist, but authors must not depend on it as a public contract yet.

### INTERNAL

Implementation detail. Do not reference it from feature mods.

## Current capability map

| Component | Public/supported surface | Stability state | Still gated / not promised |
| --- | --- | --- | --- |
| FoA Mod Manager | normal BepInEx config discovery; `FoAModManagerApi.SetCustomUiScope`; `SetControllerCursorScope`; controller-action registration/unregistration; status-provider registration/unregistration; documented read-only scope state | **SUPPORTED PUBLIC SURFACE** | manager internals, raw input implementation, undocumented native-settings internals, any method not in the author docs |
| Tainted Interface | `TaintedInterfaceApi` render-only styles; semantic texture/icon/item-icon lookup; documented catalog/descriptor APIs; supported shared UI-scope integration | **SUPPORTED PUBLIC SURFACE** | raw embedded-pack paths/payload layout, arbitrary full-pack access as a compatibility contract, generic Main/Pause Menu registration |
| Avalon Core | `Plugin.TrustReports`; documented registry/discovery; capability/descriptor/version/readiness queries | **SUPPORTED PUBLIC SURFACE — READ-ONLY/DISCOVERY** | private adapters, direct gameplay execution, arbitrary scene/spawn/save mutation, treating capability discovery as execution permission |
| Tainted Framework | exact promoted services such as `framework.runtime-report` under their documented consumer boundary | **PROMOTED CAPABILITY** | every other internal service/capability unless separately promoted; framework host as a generic FoA SDK |
| Avalon AI Runtime | `AvalonAI.Contracts.V2`; `IAvalonAiPackage`; Manifest / GoalPolicies / GoalDefinitions / ActionDefinitions DTO contract | **VERSIONED PUBLIC CONTRACT** | host implementation, Rabbit/GOAP/Blaze internals, package-to-FoA direct calls, package-owned global scheduling/execution |
| Avalon Contracts | `AvalonContractsApi.DiscoverHost()`; explicit provider registration/unregistration; documented catalog/state/evidence/preview/validation readback | **SUPPORTED CONTRACT SHAPE; DISTRIBUTION NOT YET STANDALONE** | generic lifecycle mutation authority; provider gameplay ownership; lane-specific command routes not individually promoted |
| Tainted Grail Extender | documented extension manifests and authenticated loopback SDK/service contracts | **ADVANCED CONTRACT SHAPE; HOST DISTRIBUTION NOT YET STANDALONE** | generic remote/admin server behavior, secret exposure, undocumented host internals, broad runtime authority |
| Tainted Diagnostic Tool | installed read-only dump/evidence workflows and documented output interpretation | **AUTHOR-READY TOOL** | feature-mod compile-time dependency, mutation API, treating diagnostic rows as gameplay approval |

## Supported does not mean forever-stable ABI

A method should be called stable only when the owning component publishes a compatibility promise or an explicit versioned contract.

Therefore:

- FoA Mod Manager's listed API is **supported**, not automatically an eternal ABI.
- Tainted Interface's semantic APIs are **supported**, but the physical embedded payload is not the contract.
- Avalon Core discovery is **supported**, while mutation remains gated.
- Tainted Framework is **capability-by-capability**.
- Avalon AI Contracts V2 is explicitly versioned.
- Avalon Contracts lifecycle authority remains lane-specific.
- TGE is advanced and distribution-gated at the host level.

## Consumer rule

A feature mod may depend on a surface only when all of these are true:

1. the component can be obtained through a verified route;
2. the exact surface appears in the public author documentation;
3. the surface's posture permits the intended operation;
4. the mod declares the real dependency when the feature cannot work without it;
5. the tested package/contract version is recorded;
6. cleanup/unregistration is implemented when the contract requires it.

If #1 fails, distribution is blocked.

If #2 or #3 fails, API use is gated.

If #4 is hidden behind reflection, the dependency model is wrong.

## Reflection rule

Reflection is acceptable for **optional enhancement-only integration** when:

```text
base feature works correctly without dependency
dependency absent -> bridge does nothing
dependency present + compatible -> enhancement activates
```

Reflection is not a way to consume gated APIs or hide a hard semantic dependency.

See [Optional infrastructure bridge](../recipes/optional-infrastructure.md).

## Promoting a new capability

Before adding a new API to this page as supported:

1. identify the owning component;
2. identify the exact public contract/type/member;
3. establish the minimum package or contract version;
4. state Mono/IL2CPP scope;
5. document failure behavior when absent/incompatible;
6. document lifecycle/cleanup;
7. provide a copyable consumer route when appropriate;
8. validate the consumer against the intended packaged distribution;
9. update the distribution map if a new package is required.

A source type existing is not enough.

A build passing is not enough.

A host loading is not enough.

Promotion is a consumer contract decision backed by the appropriate evidence.