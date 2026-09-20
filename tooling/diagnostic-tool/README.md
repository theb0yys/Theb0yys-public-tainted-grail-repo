# Tainted Diagnostic Tool

**Posture: Author-ready read-only research tool**

**Obtain:** https://www.nexusmods.com/taintedgrailthefallofavalon/mods/182

**Version signal:** the current engineering source line is `0.4.56`; verify the exact public package installed before declaring a minimum version.

**Stability boundary:** this is an installed evidence-collection tool, not a feature-mod compile-time library. Its output is evidence to review, not mutation authority.

See [distribution/versioning](../ecosystem/distribution-and-versioning.md) and [API stability](../ecosystem/api-stability.md).

Use it before guessing.

The tool writes timestamped CSV/TXT evidence from the loaded game runtime.

It does **not**:

- spawn actors;
- grant items;
- learn recipes;
- mutate saves;
- patch gameplay;
- decide that a candidate is safe.

## Common questions

- What is the real item/template/recipe GUID?
- Which recipes belong to this crafting station?
- Which creature/template is referenced by this loaded spawner?
- What route/patrol/world context is loaded here?
- Which proficiency/source XP rows exist?
- Which actor authority refs are visible?
- Which knowledge gaps remain?

Start with:

- [First dump](first-dump.md)
- [Find identities](find-identities.md)
