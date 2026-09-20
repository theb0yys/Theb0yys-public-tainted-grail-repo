# Tooling and Shared Infrastructure

This section is for **things a mod author can actually use**.

It is the bridge between ordinary FoA mod code and the shared infrastructure maintained in the wider mod ecosystem.

## Start here: what do you need?

| I need to… | Use |
| --- | --- |
| expose settings, controller actions, mod status, or modal UI ownership | [FoA Mod Manager](foa-mod-manager/README.md) |
| share visual styles, semantic textures/icons, or common UI resources | [Tainted Interface](tainted-interface/README.md) |
| discover trusted/shared capabilities without taking runtime authority | [Avalon Core](avalon-core/README.md) |
| consume shared runtime-facing framework services | [Tainted Framework](tainted-framework/README.md) |
| author an AI behaviour package that runs through the single AI host | [Avalon AI Runtime](avalon-ai-runtime/README.md) |
| expose or consume contract/provider data across mods | [Avalon Contracts](avalon-contracts/README.md) |
| build an extension or external local SDK client | [Tainted Grail Extender](tainted-grail-extender/README.md) |
| find real IDs, templates, recipes, spawners or runtime context | [Tainted Diagnostic Tool](diagnostic-tool/README.md) |

For the whole stack, see the [ecosystem map](ecosystem/README.md).

For copyable multi-tool workflows, use [integration recipes](recipes/README.md).

## The rule

**Do not reimplement shared ownership locally when the ecosystem already has a reviewed owner.**

Examples:

- do not write another cursor/freeze manager when FoA Mod Manager already owns the shared UI scope;
- do not hardcode raw UI asset paths when Tainted Interface exposes semantic IDs;
- do not create a second AI scheduler when Avalon AI Runtime owns the single host;
- do not make a feature mod scan shared providers when Avalon Core / Avalon Contracts already expose discovery contracts;
- do not guess native GUIDs when the Diagnostic Tool can collect evidence.

## Maturity matters

Some infrastructure is ready for direct consumer use; some is intentionally narrow or gated.

Every page in this section labels the public posture as one of:

- **Author-ready** — intended for real mod consumers now.
- **Read-only/discovery** — safe for diagnostics/query/metadata, not runtime mutation.
- **Capability-gated** — a shared owner exists, but only named surfaces are promoted.
- **Advanced/SDK** — useful, but requires tighter compatibility/security/version discipline.

Do not infer support from the existence of a DLL, class, descriptor, or private implementation.
