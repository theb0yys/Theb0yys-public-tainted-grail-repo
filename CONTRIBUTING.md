# Contributing

This repository is a public, source-only Tainted Grail: The Fall of Avalon modding knowledge base.

## Put knowledge in one canonical place

| Question | Area |
| --- | --- |
| How do I learn this in order? | `learn/` |
| How do I perform a task? | `how-to/` |
| How does the game own/execute it? | `systems/` |
| What exact identifier/type/hook do I need? | `reference/` |
| What did a real working mod prove? | `case-studies/` |
| Where is the minimal runnable code? | `examples/` |
| Where do official/upstream references live? | `sources/` |
| Where is a starter project? | `templates/` |

Do not maintain parallel current explanations. Link to the canonical owner.

## Good contributions

- setup/runtime corrections;
- source-only C# examples;
- exact identities, hooks or architecture notes with clear runtime/version scope;
- reproducible procedures;
- case studies from working mods;
- public-safe diagnostics and CI checks.

## Do not contribute

- game assets, extracted commercial content, localization dumps, maps, audio, textures, models, scenes, asset bundles or saves;
- game DLLs, Unity DLLs, BepInEx binaries, generated interop assemblies, executables, archives or compiled plug-ins;
- bulk decompiled game source;
- secrets, tokens, signing material, private paths or personal data;
- code copied from private projects without an explicit decision to publish that exact code;
- unscoped claims that a runtime/build works.

## Example layout

Keep `examples/` shallow and predictable:

```text
examples/<mono|il2cpp|merlin|hybrid>/<domain>/<mechanism>/
```

For Mono and IL2CPP examples, use lowercase kebab-case domain/mechanism folders, do not use numeric ordering prefixes, and do not repeat the runtime in child names. Project files should use concise PascalCase mechanism names rather than `*Template.csproj` or `*Example.csproj` suffixes. Add a domain folder only when it contains a public-safe example.

## Pull requests

State what changed, the Mono/IL2CPP/Merlin/hybrid/documentation lane, what was actually tested, local dependencies used, and why the contribution is safe to redistribute.

The public-surface guard is a redistribution backstop; it does not prove game/editor/runtime behaviour.

## Licence status

No general repository licence is included yet. Public visibility does not itself grant redistribution or relicensing rights.
