# Tooling and Shared Infrastructure

Use this section when your mod needs shared tooling that another project already provides.

Start here for common settings/UI handling, shared visual resources, diagnostics, framework services, AI integration, cross-mod contracts, or external SDK tooling. The tables and component pages below tell you what each project currently supports and what is still restricted or research-only.

## Start here: what do you need?

| I need to… | Use | Current posture |
| --- | --- | --- |
| expose BepInEx settings | [FoA Mod Manager](components/foa-mod-manager/README.md) | **Author-ready** |
| register controller commands or runtime status | [FoA Mod Manager](components/foa-mod-manager/README.md) | **Author-ready** |
| open a custom screen without every mod owning cursor/freeze logic | [FoA Mod Manager](components/foa-mod-manager/custom-ui-scope.md) | **Author-ready** |
| use common styles, icons or semantic UI assets | [Tainted Interface](components/tainted-interface/README.md) | **Author-ready** |
| find real GUIDs/templates/recipes/spawners/runtime context | [Tainted Diagnostic Tool](components/diagnostic-tool/README.md) | **Author-ready, read-only** |
| discover shared capability/provider metadata | [Avalon Core](components/avalon-core/README.md) | **Read-only/discovery** |
| consume a concrete shared runtime service | [Tainted Framework](components/tainted-framework/README.md) | **Only named promoted surfaces** |
| author AI behaviour that composes with other AI | [Avalon AI Runtime](components/avalon-ai-runtime/README.md) | **Package contracts + single host** |
| publish/query cross-mod contract data | [Avalon Contracts](components/avalon-contracts/README.md) | **Read-only/provider-first; lifecycle lane-specific** |
| run an external local development client | [Tainted Grail Extender](components/tainted-grail-extender/README.md) | **Advanced/SDK** |

For exact GUIDs/assemblies/API entry points, use the [Component reference](ecosystem/component-reference.md).

For hard/soft dependency and packaging rules, use [Dependency and packaging](ecosystem/dependency-and-packaging.md).

For recommended combinations, use [Author stacks](ecosystem/author-stacks.md).

For copyable workflows, use [Integration recipes](recipes/README.md).

For full source projects, use [Mono infrastructure examples](../examples/mono/infrastructure/README.md).

## Default recommendation

For most user-facing mods:

```text
BepInEx
+ FoA Mod Manager (settings / commands / shared UI scope)
+ Tainted Interface (only if you need shared visual resources)
```

Add Core, Framework, AI Runtime, Contracts or TGE **only when the feature actually needs that owner**.

## The ownership rule

**Do not reimplement a shared owner locally when the ecosystem already has a reviewed owner.**

Examples:

- do not write another cursor/freeze manager when FoA Mod Manager owns the shared scope;
- do not hardcode raw UI asset-pack paths when Tainted Interface exposes semantic IDs;
- do not ship a second AI scheduler for actors governed by Avalon AI Runtime;
- do not scan provider internals when Core/Contracts expose a supported discovery surface;
- do not guess native GUIDs when the Diagnostic Tool can collect evidence.

The opposite is also true:

**Do not take a dependency just because a framework exists.**

If a Framework/Core capability is not explicitly promoted for consumers, treat it as unavailable.

## Maturity vocabulary

- **Author-ready** — intended for ordinary real mod consumers.
- **Read-only/discovery** — query/metadata/diagnostics only.
- **Capability-gated** — shared owner exists; only specifically promoted surfaces may be consumed.
- **Advanced/SDK** — external process or high-authority integration requiring stronger version/security discipline.
- **Blocked/research** — architecture may exist, but mod authors should not build against it yet.
