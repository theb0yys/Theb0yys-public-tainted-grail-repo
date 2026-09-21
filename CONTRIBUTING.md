# Contributing

This repository is a public, source-only Tainted Grail: The Fall of Avalon modding project.

## Where contributions go

| Question | Area |
| --- | --- |
| How do I reproduce a complete item, weapon, armour, or creature import process? | `pipelines/` |
| How do I learn or perform a bounded task? | `guides/` |
| How does the game own or execute it? | `knowledge/systems/` |
| What reusable modding capability applies? | `knowledge/mechanics/` |
| What exact identifier, hook, service or version fact do I need? | `knowledge/reference/` |
| Is the behaviour still being investigated? | `research/investigations/` |
| What did real mod work prove? | `research/case-studies/` |
| Where is upstream provenance? | `research/sources/` |
| What shared infrastructure should a mod consume? | `platform/` |
| Where is minimal runnable source? | `examples/` |
| Where is a starter project? | `templates/` |

See [Repository taxonomy](contributing/taxonomy.md), [Pipeline documentation](contributing/authoring/pipelines.md), and [Evidence standards](contributing/evidence-standards.md). Update the existing explanation for a topic instead of creating a second competing version.

## Public boundary

Do not contribute proprietary game content, binaries, generated interop assemblies, executables, archives, bulk decompiled source, secrets, private paths, personal data, or code from private projects unless you have the right to publish it.

## Examples

Keep `examples/` predictable:

    examples/<mono|il2cpp|merlin|hybrid>/<domain>/<mechanism>/

## Pull requests

State what changed, which runtime or authoring setup it targets (Mono, IL2CPP, Merlin, or hybrid), what you actually tested, any local dependencies used, and why the contribution is safe to redistribute.

For pipeline changes, state which stage changed and whether its status is PASSED, FAILED, PARTIAL, BLOCKED, NOT_RUN, or NOT_APPLICABLE. Do not use one stage as evidence for a later stage.

The public-surface CI check helps catch material that should not be redistributed. Passing it does not prove game, editor, or runtime behaviour.

## Licensing

Original software code and software artifacts contributed to this repository are licensed under the **Apache License 2.0** unless a more specific notice applies. Original documentation and research material are licensed under **CC BY 4.0** unless a more specific notice applies.

See [LICENSE](LICENSE) and [LICENSE-DOCS](LICENSE-DOCS).

Do not assume contribution or inclusion in this repository relicenses third-party, upstream, mirrored, proprietary, or trademarked material. Contributors must have authority to publish their contribution under the applicable repository licence, or preserve and identify the controlling separate terms.
