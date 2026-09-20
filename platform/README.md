# Tooling and Shared Infrastructure

Shared tools and services for Tainted Grail mods. Start with the smallest set that solves your problem.

```text
your mod
  ├─ settings / controller / UI scope → FoA Mod Manager
  ├─ shared visual resources         → Tainted Interface
  ├─ read-only capability discovery  → Avalon Core
  ├─ shared runtime service          → Tainted Framework
  ├─ AI package                      → Avalon AI Runtime
  ├─ contract/provider integration   → Avalon Contracts
  ├─ external/local SDK client       → Tainted Grail Extender + FOA-SDK
  └─ game-data diagnostics           → Tainted Diagnostic Tool
```

## Start here: what do you need?

| I need to… | Use | Status |
| --- | --- | --- |
| detect my FoA runtime, scaffold, build, install or tail logs | [Developer tools](developer-tools/README.md) | **Ready** |
| expose BepInEx settings | [FoA Mod Manager](components/foa-mod-manager/README.md) | **Ready** |
| register controller commands or runtime status | [FoA Mod Manager](components/foa-mod-manager/README.md) | **Ready** |
| open a custom screen without every mod owning cursor/freeze logic | [FoA Mod Manager](components/foa-mod-manager/custom-ui-scope.md) | **Ready** |
| use common styles, icons or semantic UI assets | [Tainted Interface](components/tainted-interface/README.md) | **Ready** |
| find real GUIDs/templates/recipes/spawners/runtime context | [Tainted Diagnostic Tool](components/diagnostic-tool/README.md) | **Ready; read-only** |
| discover shared capability/provider metadata | [Avalon Core](components/avalon-core/README.md) | **Discovery only** |
| consume a concrete shared runtime service | [Tainted Framework](components/tainted-framework/README.md) | **Documented APIs only** |
| author AI behaviour that composes with other AI | [Avalon AI Runtime](components/avalon-ai-runtime/README.md) | **Package contracts + single host** |
| publish/query cross-mod contract data | [Avalon Contracts](components/avalon-contracts/README.md) | **Provider/read-only; lifecycle support varies** |
| run an external local development client | [Tainted Grail Extender](components/tainted-grail-extender/README.md) | **Advanced SDK** |

For a practical Windows setup/build loop, use the [Developer tools](developer-tools/README.md).

For exact GUIDs, assemblies, and API entry points, use the [Component reference](ecosystem/component-reference.md).

For hard/soft dependency and packaging rules, use [Dependency and packaging](ecosystem/dependency-and-packaging.md).

For recommended combinations, use [Author stacks](ecosystem/author-stacks.md).

For copyable workflows, use [Integration recipes](recipes/README.md).

For full source projects, use [Mono infrastructure examples](../examples/mono/infrastructure/README.md).

## Recommended starting point

For most user-facing mods:

```text
BepInEx
+ FoA Mod Manager (settings / commands / shared UI scope)
+ Tainted Interface (only if you need shared visual resources)
```

Add Core, Framework, AI Runtime, Contracts, or TGE only when your feature needs one of their documented capabilities.

## Avoid duplicating shared infrastructure

Use an existing shared service when it already solves the problem:

- use FoA Mod Manager for shared cursor/freeze handling instead of writing another manager;
- use Tainted Interface semantic IDs instead of hardcoding raw UI asset-pack paths;
- use Avalon AI Runtime for actors that participate in its shared AI model instead of shipping a second scheduler;
- use supported Core/Contracts discovery APIs instead of scanning provider internals;
- use the Diagnostic Tool to collect native GUIDs instead of guessing them.

Also avoid unnecessary dependencies. If a Framework/Core capability is not explicitly documented for consumers, treat it as unavailable.
