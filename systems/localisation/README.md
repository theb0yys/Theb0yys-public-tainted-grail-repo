<!-- Canonical Wave 5 native-system page split from docs/reference/LOCALISATION_BABEL.md. -->
# Localisation and Questline Babel

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/localisation/custom-text.md).

## What this system is

Questline's localisation stack is layered:

~~~text
Unity Localization String Tables + LocString authoring
→ Questline Babel corpus/ID bake
→ per-language Babel payloads
→ languages.arch
→ BabelManager / selected provider
→ semantic-key or numeric-index lookup
→ LocString / LightLocString consumers
~~~

Unity Localization remains part of authoring/tooling.

Questline Babel owns the normal compiled runtime text database used by much of the game.

## Who owns it in FoA

Important owners/types include:

- `LocString`
- `OptionalLocString`
- `LightLocString`
- `LocalizationEntryId`
- `ILocalizationManager`
- `BabelManager`
- Babel providers
- Unity String Table collections
- Story/Babel compilation pipeline
- individual consumer caches such as `Item.SetupTexts()`

## Important identities, types, and methods

### LocString

Carries:

- semantic `ID`;
- optional `IdOverride`;
- literal `Fallback`.

Runtime uses the final semantic key to query Babel and falls back to the literal string when translation is empty.

### Important rule

~~~csharp
(LocString)"My Mod Item"
~~~

does **not** register a Babel term.

It creates a LocString whose fallback is the literal text while semantic IDs remain empty.

### LightLocString

Contains only a compiled numeric `LocalizationEntryId`.

It has no literal fallback.

### Babel numeric IDs

They are positional corpus indexes, not hashes.

Zero is invalid; valid IDs encode global corpus position.

That makes private numeric ID allocation dangerous.

## Where it exists in the lifecycle

Authoring and runtime are different:

~~~text
semantic authoring key
→ Unity table/corpus
→ Babel bake
→ numeric corpus position / language blobs
→ archive
→ BabelManager loads selected provider
→ consumer resolves string
→ some consumers cache the result
~~~

For example, an `Item` can resolve template text into cached token text during initialization.

A provider/language change alone does not prove all already-live UI/items refresh.

## Current proof boundary

**Partial path proven:** Babel architecture, semantic/numeric lookup model, literal fallback behavior, global positional ID constraint, archive/runtime relationship.

**Safe now:** package-owned semantic identities plus source-language fallback and offline translation manifests.
