# Localisation Consumer Boundaries

> **Consumer/modding page.** The canonical Babel architecture, compiled corpus model, and runtime lookup flow live in [Babel Localisation](babel/README.md).

Use this page when a mod needs to **display or consume text safely** without pretending to own the native Babel compilation pipeline.

## Safe-facing identities

Prefer a stable package/mod-owned semantic identifier when the integration surface supports one.

Keep these distinct:

- semantic string identity;
- literal fallback text;
- compiled positional `LocalizationEntryId`;
- voice/audio identity.

Do not fabricate a native numeric Babel ID. The canonical Babel page explains why the numeric form is tied to compiled corpus position.

## `LocString` fallback behavior

A literal conversion such as:

```csharp
(LocString)"My Mod Item"
```

does not register a new native Babel term.

It provides fallback text while the semantic identity remains absent.

That can be useful for a first runtime proof when the requirement is simply “the custom item must be readable”, but the documentation should call it a fallback rather than multilingual integration.

## Consumer caching matters

Some runtime consumers resolve template/localisation data during initialization and retain a cached result.

Therefore:

```text
language provider changed
≠
every live consumer refreshed
```

If a feature claims runtime language switching, test the actual consumer lifecycle rather than only proving the provider changed.

## Story/dialogue relationship

Story text uses Babel/localisation identities, but text localisation and Story execution remain separate owners.

- Story architecture: [Story Graphs](story-graphs/README.md)
- Runtime Story/choice behavior: [Story, Quests, Dialogue, and Choices](story-quest-dialogue.md)

Voice/FM0D resources are another separate domain.

## Practical mod policy

For public mods:

1. use stable semantic names under a mod-owned namespace where a supported route exists;
2. keep a readable source-language fallback when appropriate;
3. do not allocate native positional IDs privately;
4. keep localisation manifests/source translations separate from game-extracted language payloads;
5. test the concrete UI/item/dialogue consumer that displays the text;
6. record whether a restart or object reconstruction is required for language changes.

## Failure patterns

- calling fallback text “registered localisation”;
- treating semantic keys and numeric indexes as interchangeable;
- assuming semantic-key normalization/case rules without evidence;
- assuming live UI/items retranslate automatically;
- combining voice replacement with text localisation ownership;
- promoting Mono consumer behavior to IL2CPP without a separate test.

## Proof boundary

The canonical architecture and ID model are documented under [Babel Localisation](babel/README.md).

This page owns only the mod-consumer implications: fallback usage, semantic identity discipline, consumer refresh behavior, and claim boundaries.
