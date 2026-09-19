# Babel Localisation

## What it is

**Babel** is Questline's compiled runtime localisation system layered on top of Unity Localization authoring data.

Unity String Tables provide the editor/source table side. Questline's build process compiles those strings into Babel's runtime corpus and language payloads.

## Build/runtime flow

~~~text
Unity String Tables + LocString authoring
→ Babel corpus/ID bake
→ per-language payloads
→ languages.arch
→ BabelManager
→ selected language provider
→ semantic key or numeric index lookup
→ LocString / LightLocString consumers
~~~

## Main runtime concepts

- `BabelManager`
- `LocString`
- `LightLocString`
- `LocalizationEntryId`
- semantic string keys
- streamed/preloaded language providers

Primary assembly: `Awaken.Babel.dll`.

## ID model

`LocalizationEntryId` is positional. A valid numeric ID represents an index in the compiled corpus.

That means the **ordering of the complete compiled corpus matters**. Numeric IDs are not independent hashes that mods can safely fabricate.

## LocString behaviour

A `LocString` can contain:

- semantic ID;
- fallback text.

If Babel does not resolve a semantic ID, `LocString` can return its fallback.

A `LightLocString` contains only the compact numeric identity and has no equivalent source-language fallback.

## Runtime lookup

Babel supports:

~~~text
semantic key
→ key-to-index map
→ language provider

numeric LocalizationEntryId
→ direct index
→ language provider
~~~

## Modding relevance

For public mods, semantic identifiers plus explicit fallback text are the useful safe-facing model.

Do not treat a literal `(LocString)"text"` conversion as native translation registration; that produces fallback text, not a new compiled Babel entry.

## Related systems

- [Story Graphs](../story-graphs/README.md)
- [Serialization and archives](../serialization-archives/README.md)
- [Runtime orchestration](../runtime-orchestration/README.md)
