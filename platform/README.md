# Tooling and Shared Infrastructure

This section covers shared tools and libraries that FoA mod authors can actually use.

Use it when your mod needs something that is already provided by another project—for example shared settings UI, controller commands, common visual assets, diagnostics, AI integration, or framework services.

```text
your mod
  ├─ settings, commands, shared UI handling → FoA Mod Manager
  ├─ shared styles and visual assets        → Tainted Interface
  ├─ capability/provider discovery          → Avalon Core
  ├─ shared runtime services                → Tainted Framework
  ├─ AI integration                         → Avalon AI Runtime
  ├─ contracts and provider data            → Avalon Contracts
  ├─ external/local SDK development         → Tainted Grail Extender + FOA-SDK
  └─ game-data inspection                   → Tainted Diagnostic Tool
```

## What do you need?

| I need to… | Use | Ready for |
| --- | --- | --- |
| expose BepInEx settings | [FoA Mod Manager](components/foa-mod-manager/README.md) | normal mod use |
| register controller commands or show runtime status | [FoA Mod Manager](components/foa-mod-manager/README.md) | normal mod use |
| open a custom screen with shared cursor/freeze handling | [FoA Mod Manager](components/foa-mod-manager/custom-ui-scope.md) | normal mod use |
| use common styles, icons, or semantic UI assets | [Tainted Interface](components/tainted-interface/README.md) | normal mod use |
| find real GUIDs, templates, recipes, spawners, or runtime context | [Tainted Diagnostic Tool](components/diagnostic-tool/README.md) | read-only diagnostics |
| discover shared capability/provider metadata | [Avalon Core](components/avalon-core/README.md) | read-only discovery |
| use a named shared runtime service | [Tainted Framework](components/tainted-framework/README.md) | only documented consumer APIs |
| author AI behaviour that must cooperate with other AI mods | [Avalon AI Runtime](components/avalon-ai-runtime/README.md) | package contracts and shared host |
| publish or query cross-mod contract data | [Avalon Contracts](components/avalon-contracts/README.md) | read-only/provider-first integration |
| run an external local development client | [Tainted Grail Extender](components/tainted-grail-extender/README.md) | advanced SDK work |

For exact GUIDs, assemblies, and API entry points, see the [Component reference](ecosystem/component-reference.md).

For dependency and packaging rules, see [Dependency and packaging](ecosystem/dependency-and-packaging.md).

For recommended combinations of tools, see [Author stacks](ecosystem/author-stacks.md).

For copyable integration workflows, see [Integration recipes](recipes/README.md).

For complete source examples, see [Mono infrastructure examples](../examples/mono/infrastructure/README.md).

## A sensible default

Most user-facing mods should start with:

```text
BepInEx
+ FoA Mod Manager, when shared settings/commands/UI handling are useful
+ Tainted Interface, only when shared visual resources are useful
```

Add Avalon Core, Tainted Framework, Avalon AI Runtime, Avalon Contracts, or Tainted Grail Extender only when the feature actually needs what that project provides.

## Do not duplicate shared infrastructure

If a shared project already provides a supported solution, use it instead of creating another incompatible copy inside your mod.

Examples:

- use FoA Mod Manager's shared cursor/freeze handling instead of writing another global manager;
- use Tainted Interface semantic asset IDs instead of hard-coding asset-pack paths;
- use Avalon AI Runtime for actors it already manages instead of shipping a second competing scheduler;
- use supported Core/Contracts discovery APIs instead of scanning another provider's internals;
- use the Diagnostic Tool to discover native IDs instead of guessing GUIDs.

The reverse also matters: **do not add a dependency just because a framework exists.**

If a capability is not documented as available to consumers, treat it as unavailable.

## Maturity labels

- **Normal mod use** — intended for ordinary mod authors.
- **Read-only** — safe for queries, metadata, or diagnostics; not for mutation.
- **Documented APIs only** — the project exists, but only specifically documented consumer APIs should be used.
- **Advanced/SDK** — external-process or higher-authority integration that needs stronger version and security discipline.
- **Research only** — not ready to build against yet.
