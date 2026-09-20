# Contributing

This repository is a public, source-only Tainted Grail: The Fall of Avalon modding knowledge base and toolkit.

## Where should a contribution go?

| If your contribution answers… | Put it in… |
| --- | --- |
| How do I learn or perform this task? | `guides/` |
| How does the game itself work? | `knowledge/systems/` |
| How can a mod accomplish this specific change? | `knowledge/mechanics/` |
| What exact ID, type, method, hook, service, or version do I need? | `knowledge/reference/` |
| Is this behavior still being investigated? | `research/investigations/` |
| What did a real modding problem teach us? | `research/case-studies/` |
| Where did this information come from? | `research/sources/` |
| What shared tool or framework API should a mod use? | `platform/` |
| Is this a small focused code example? | `examples/` |
| Is this a reusable starter project? | `templates/` |

See [Repository taxonomy](contributing/taxonomy.md) and [Evidence standards](contributing/evidence-standards.md) for the detailed rules.

Do not maintain several current explanations of the same technical claim. Keep one main explanation and link to it.

## Public boundary

Do not contribute proprietary game content, binaries, generated interop assemblies, executables, archives, bulk decompiled source, secrets, private paths, personal data, or code copied from private projects unless it has been deliberately reviewed and rewritten for public release.

## Examples

Keep example paths predictable:

```text
examples/<mono|il2cpp|merlin|hybrid>/<what-it-does>/<specific-technique>/
```

For example, the first folder tells the reader which environment the code targets; the next tells them what kind of problem it solves.

## Pull requests

State:

- what changed;
- which runtime/tooling environment it targets;
- what you actually tested;
- which local dependencies were required;
- what remains untested;
- why the contribution is safe to redistribute.

The public-surface guard is a redistribution safety check. It does not prove that the code works in the game or editor.

## Licence status

No general repository licence is included yet. Public visibility does not itself grant redistribution or relicensing rights.
