# Localisation and Questline Babel

> **Reference page.** FoA localisation is not just a string field and it is not safe to invent native Babel numeric IDs.

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

## How we interact with it

### For a first runtime custom item

A literal fallback can make the item readable without pretending it has full native multilingual registration.

Say so explicitly.

### For durable package-owned localisation identity

Use stable semantic IDs owned by the mod/package.

Do not fabricate native numeric IDs.

### Keep voice separate

FMOD voice/audio resources are a separate resource domain even when associated with localized dialogue.

### Treat hot language switching as its own lifecycle

Native PC language changes can trigger application exit/restart in the inspected architecture.

Do not assume every custom UI automatically refreshes in place.

## Why this route

Babel research corrected a common false assumption:

> assigning a string to `LocString` is not native localisation registration.

It also discovered a more dangerous one:

> Babel's compact numeric IDs are global positional indexes, so rebuilding or privately allocating them can make existing compiled `LightLocString` values resolve to the **wrong text**, not merely fail.

That makes "just add another numeric entry" an unsafe generic modding strategy.

## What goes wrong

- treating fallback text as registered Babel localisation;
- inventing global `LocalizationEntryId` values;
- rebuilding base Babel corpus with changed ordering;
- assuming semantic keys are case-insensitive/normalized;
- renaming/moving authoring context and assuming generated keys remain stable;
- assuming live items/tooltips retranslate after provider swap;
- bundling voice under the text-localisation model;
- claiming IL2CPP equivalence without evidence.

## How to verify

For custom text, record:

- semantic ID if one exists;
- fallback text;
- ownership namespace;
- collision check;
- current language behavior;
- whether the consumer caches resolved text;
- restart/hot-switch behavior if claimed;
- missing translation behavior;
- exact runtime lane/build.

For true multilingual support, verify at least two language payloads and the consumer refresh lifecycle.

## Current proof boundary

**Partial path proven:** Babel architecture, semantic/numeric lookup model, literal fallback behavior, global positional ID constraint, archive/runtime relationship.

**Safe now:** package-owned semantic identities plus source-language fallback and offline translation manifests.

