# Contributing

This repository is a public, clean-room starter for Tainted Grail modding.

## Before opening an issue

Use the repository issue forms for public-safe bug reports and improvement proposals.

Before submitting:

- reduce a problem to the smallest useful reproduction;
- state the runtime/content lane and exact versions when relevant;
- share only the relevant redacted diagnostics;
- search for an existing report covering the same problem.

Do **not** put a sensitive vulnerability, credential, personal data, unredacted private path, or proprietary game content into a public issue. Follow [SECURITY.md](SECURITY.md) instead.

## Good contributions

- corrections to setup/runtime documentation;
- small reusable C# examples;
- starter templates that depend only on locally supplied references;
- diagnostics that do not expose private paths or proprietary content;
- CI checks that keep the repository source-only;
- documentation for reproducible, legal mod-development workflows.

## Do not contribute

- game assets, extracted commercial content, localization dumps, maps, audio, textures, models, scenes, asset bundles, or saves;
- game DLLs, Unity DLLs, BepInEx binaries, generated interop assemblies, executables, archives, or compiled plug-ins;
- bulk decompiled game source;
- secrets, API keys, tokens, signing material, private paths, or personal data;
- code copied from private projects without an explicit decision to publish that exact code;
- claims that a runtime/build works unless the evidence and scope are stated.

## Pull requests

The pull-request template is a checklist, not ceremony: it exists to keep public claims, redistribution boundaries, and validation scope explicit.

Keep each pull request focused. Explain:

1. what the change adds or fixes;
2. which runtime lane it applies to: Mono, IL2CPP, both, or documentation-only;
3. what you actually tested;
4. whether the change references any local game/BepInEx files;
5. why the contribution is safe to redistribute.

Do not describe static inspection as runtime proof.

Before submitting, run the applicable repository checks. The hosted public-surface guard is a backstop; it does not prove game/editor behaviour.

## Licence changes

This repository does not currently include a general licence.

Do not add, replace, or reinterpret the repository licence through an ordinary contribution. Licence selection is an explicit maintainer decision because it changes the legal terms under which repository material may be reused.

## Examples and templates

Examples should teach one concept at a time. Prefer neutral names and self-contained demonstrations over code lifted from a real mod.

Game-specific patches are acceptable only when they are small, well explained, independently authored, and do not require redistributing proprietary game content.
