# Infrastructure Component Reference

Use this page when you need the **exact shared component identity** to add a dependency or assembly reference.

Package/install folder names may change. Prefer stable plugin GUIDs, assembly names and public API types over hardcoded filesystem paths.

Before adding a dependency, also check [Public distribution and versioning](distribution-and-versioning.md) for the verified acquisition route and [API stability and capability promotion](api-stability.md) for whether the exact surface is supported, versioned, promoted or gated.

| Component | BepInEx plugin GUID | Primary assembly / contract | Public entry point | Author posture |
| --- | --- | --- | --- | --- |
| FoA Mod Manager | `kane.tgfoa.mod-manager` | `FoAModManager.dll` | `FoAModManager.FoAModManagerApi` | **Author-ready** |
| Tainted Interface | `kane.tgfoa.tainted-interface` | `TaintedInterface.dll` | `TaintedInterface.TaintedInterfaceApi` | **Author-ready visual/UI resources** |
| Avalon Core | `kane.tgfoa.avalon-core` | `AvalonCore.dll` + Core support assemblies | `AvalonCore.Plugin.TrustReports`, `AvalonCore.Plugin.Registry` | **Read-only/discovery baseline** |
| Tainted Framework | `kane.tgfoa.tainted-framework` | host + `Tainted.Abstractions.dll` | capability-specific contracts such as `framework.runtime-report` | **Capability-gated** |
| Avalon AI FoA Host | `kane.tgfoa.avalon-ai-foa-host` | host implementation; packages reference `AvalonAI.Contracts.V2.dll` | `IAvalonAiPackage` and Contracts V2 DTOs | **Package contracts public; host-owned execution** |
| Avalon Contracts | `kane.tgfoa.avalon-contracts` | `AvalonContracts.dll`, `AvalonContracts.Abstractions.dll`, runtime host | `AvalonContractsApi` | **Discovery/readback + lane-specific lifecycle** |
| Tainted Grail Extender | `kane.tgfoa.tainted-grail-extender` | `TGE.FoAHost.BepInEx.dll` + TGE contracts/services | manifest/SDK service contracts | **Advanced/SDK** |
| Tainted Diagnostic Tool | `kane.tgfoa.template-diagnostics` | `TemplateDiagnostics.dll` source assembly | installed tool + CSV/TXT outputs | **Author-ready read-only research** |

## FoA Mod Manager

Reference `FoAModManager.dll` only when you need the direct API.

Ordinary `Config.Bind<T>` settings do **not** require a compile-time manager reference.

Useful API:

- `SetCustomUiScope(...)`
- `SetControllerCursorScope(...)`
- `RegisterControllerAction(...)`
- `RegisterStatusProvider(...)`

## Tainted Interface

Direct consumers reference `TaintedInterface.dll`.

Useful API:

- `TaintedInterfaceApi.GetRenderOnlyStyles()`
- `TaintedInterfaceApi.GetUiTexture(...)`
- `TaintedInterfaceApi.GetIcon(...)`
- `TaintedInterfaceApi.GetItemIcon(...)`
- catalog/descriptor APIs.

Feature gameplay state remains consumer-owned.

## Avalon Core

A direct runtime consumer typically references:

- `AvalonCore.dll`
- `AvalonCore.Abstractions.dll`
- `AvalonCore.Trust.dll`

and declares a hard dependency on `kane.tgfoa.avalon-core`.

Treat the baseline as discovery/read-only unless the exact later capability is explicitly promoted.

## Tainted Framework

Do not reference the host merely to obtain a generic “game SDK”.

For diagnostics, the useful contract is typically in `Tainted.Abstractions.dll`.

Every runtime-facing service needs its own public consumer contract and maturity statement.

## Avalon AI Runtime

Third-party AI package code should reference **Contracts**, not the BepInEx host:

`AvalonAI.Contracts.V2.dll`

The installed FoA host is identified by:

`kane.tgfoa.avalon-ai-foa-host`

Package code should not depend on the host assembly, Rabbit, GOAP, Blaze or FoA/game assemblies.

## Avalon Contracts

The BepInEx host identity is:

`kane.tgfoa.avalon-contracts`

Provider/consumer code works through `AvalonContractsApi` and the public contract DTOs.

Do not treat the existence of lifecycle methods as generic authority to mutate provider/game state.

## Tainted Grail Extender / FOA-SDK

TGE's BepInEx host identity is:

`kane.tgfoa.tainted-grail-extender`

External tools should use the authenticated loopback SDK/service contracts.

Public SDK repository:

<https://github.com/theb0yys/FOA-SDK>

## Diagnostic Tool

The stable BepInEx GUID is:

`kane.tgfoa.template-diagnostics`

The visible product name can change independently from the stable plugin/config identity.

Use output files as evidence inputs, not as mutation approval.
