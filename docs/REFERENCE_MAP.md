# Reference Library

This is the repository's **lookup layer**, not a learning path.

If you are learning from the beginning, use the **[repository front page](../README.md)** and follow the learning pages it links to. You do **not** need to read the files in `docs/` in order.

Use this page when you already know the question you are trying to answer.

## Runtime and loader lookup

### [Runtime Guide](RUNTIME_GUIDE.md)

Use when you need to answer questions such as:

- Is this installation Mono or IL2CPP?
- Which BepInEx lane belongs with it?
- Which local assemblies should an IL2CPP project reference?
- What changes after generated interop becomes necessary?

### [Debugging](DEBUGGING.md)

Use when something that previously should have worked is not loading, patching, resolving, or surviving a game update.

It is organized by failure symptom rather than by learning stage.

## Mod-design lookup

### [Mod Architecture](MOD_ARCHITECTURE.md)

Use when a working experiment is becoming large enough that you need clearer boundaries between:

- plug-in startup;
- configuration;
- patches;
- reusable services;
- diagnostics;
- cleanup.

This is architectural guidance, not a starter template you must reproduce exactly.

## Testing-status lookup

### [Testing and Evidence Status](EVIDENCE.md)

Use when you need the exact meanings of repository status labels or need to distinguish:

- source inspection;
- build results;
- plug-in load;
- editor execution;
- in-game behaviour;
- packaged-release validation.

Beginners do not need to memorize these labels.

## Content-authoring lookup

### [FoA Authoring Pipelines](pipelines/README.md)

Use when you are already working on Merlin Workshop content and need the detailed contract for a specific content type:

- [Items](pipelines/ITEMS.md)
- [Weapons](pipelines/WEAPONS.md)
- [Armour](pipelines/ARMOUR.md)
- [Creatures / NPCs / Kandra](pipelines/CREATURES_KANDRA.md)

The learning path introduces these pages when the detail becomes useful. They are intentionally denser than the tutorials.

## Old start-page compatibility

### [Start Here](START_HERE.md)

This path is retained for old bookmarks. It only routes readers to the current front door, learning pages, or references; it is not a second curriculum.

## External technical sources

These are source/upstream projects used by the repository's reference material:

- Merlin Workshop / Tainted Grail authoring toolkit: https://github.com/theb0yys/merlin-workshop
- BepInEx Tainted Grail loader work: https://github.com/theb0yys/BepInEx-Tainted-Grail
- BepInEx upstream: https://github.com/BepInEx/BepInEx
- HarmonyX upstream: https://github.com/BepInEx/HarmonyX

For the current Merlin Workshop source snapshot and authoring-status boundary, see [FoA Authoring Pipelines](pipelines/README.md).
