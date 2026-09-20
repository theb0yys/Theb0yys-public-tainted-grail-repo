# Contributing

This repository is a public, source-only Tainted Grail: The Fall of Avalon modding platform.

## Canonical owners

| Question | Area |
| --- | --- |
| How do I learn or perform this task? | `guides/` |
| How does the game own or execute it? | `knowledge/systems/` |
| What reusable modding capability applies? | `knowledge/mechanics/` |
| What exact identifier, hook, service or version fact do I need? | `knowledge/reference/` |
| Is the behaviour still being investigated? | `research/investigations/` |
| What did real mod work prove? | `research/case-studies/` |
| Where is upstream provenance? | `research/sources/` |
| What shared infrastructure should a mod consume? | `platform/` |
| Where is minimal runnable source? | `examples/` |
| Where is a starter project? | `templates/` |

See [Repository taxonomy](contributing/taxonomy.md) and [Evidence standards](contributing/evidence-standards.md). Do not maintain parallel current explanations.

## Public boundary

Do not contribute proprietary game content, binaries, generated interop assemblies, executables, archives, bulk decompiled source, secrets, private paths, personal data, or code copied from private projects without an explicit decision to publish it.

## Examples

Keep `examples/` predictable:

```text
examples/<mono|il2cpp|merlin|hybrid>/<domain>/<mechanism>/
```

## Pull requests

State what changed, the runtime/authoring lane, what was actually tested, local dependencies used, and why the contribution is safe to redistribute.

The public-surface guard is a redistribution backstop; it does not prove game/editor/runtime behaviour.

## Licence status

No general repository licence is included yet. Public visibility does not itself grant redistribution or relicensing rights.
