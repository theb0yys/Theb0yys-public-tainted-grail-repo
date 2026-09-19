# Contributing

This repository is a public, clean-room Tainted Grail modding knowledge and example repository.

## Mandatory documentation architecture

Substantial technical documentation must follow:

- [Documentation Architecture](contributing/authoring/DOCUMENTATION_ARCHITECTURE.md)
- [Private-to-Public Publication Workflow](contributing/authoring/PUBLICATION_WORKFLOW.md)
- [Migration Manifest](contributing/authoring/MIGRATION_MANIFEST.md)

Do not invent a new document structure for each contribution. Choose the correct document archetype and preserve canonical ownership/evidence boundaries.

## Before opening an issue

Use the repository issue forms for public-safe bug reports and improvement proposals.

Before submitting:
- reduce a problem to the smallest useful reproduction;
- state the runtime/content lane and exact versions when relevant;
- share only relevant redacted diagnostics;
- search for an existing report covering the same problem.

Do **not** put sensitive vulnerabilities, credentials, personal data, unredacted private paths, or proprietary game content into a public issue. Follow [SECURITY.md](SECURITY.md).

## Good contributions

- corrections to setup/runtime documentation;
- native-system explanations backed by inspectable evidence;
- bounded mechanic/capability pages;
- investigation or troubleshooting material that preserves reasoning;
- small reusable C# examples;
- starter templates using locally supplied references;
- diagnostics that do not expose private paths or proprietary content;
- CI checks that keep the repository source-only.

## Do not contribute

- game assets, extracted commercial content, localization dumps, maps, audio, textures, models, scenes, asset bundles, or saves;
- game DLLs, Unity DLLs, BepInEx binaries, generated interop assemblies, executables, archives, or compiled plug-ins;
- bulk decompiled game source;
- secrets, API keys, tokens, signing material, private paths, or personal data;
- private implementation source without an explicit publication decision;
- claims that a runtime/build works unless evidence and scope are stated;
- duplicated technical truth when a canonical system/mechanic/reference page already exists.

## Pull requests

Keep each pull request focused. Explain:

1. what the change adds or fixes;
2. the document archetype or code/example role;
3. runtime lane: Mono, IL2CPP, both, or documentation-only;
4. what was actually tested;
5. which canonical owner/system/mechanic the change depends on;
6. whether local game/BepInEx files were used as references;
7. why the contribution is safe to redistribute.

Do not describe static inspection as runtime proof.

For documentation changes, confirm that:
- long conceptual material is linked rather than duplicated;
- evidence lanes are stated honestly;
- cleanup/persistence/compatibility boundaries are not implied;
- legacy redirects remain valid when a canonical page moved.

## Examples and templates

Examples should teach one concept at a time. Prefer neutral names and self-contained demonstrations over code lifted from a real mod.

A public example does **not** inherit runtime status from a private implementation with a similar mechanism.

## Licence changes

This repository does not currently include a general licence.

Do not add, replace, or reinterpret the repository licence through an ordinary contribution. Licence selection is an explicit maintainer decision.
