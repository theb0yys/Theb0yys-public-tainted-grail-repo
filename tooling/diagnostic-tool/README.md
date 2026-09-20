# Tainted Diagnostic Tool

**Posture: Author-ready read-only research tool**

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
