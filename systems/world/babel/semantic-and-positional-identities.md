# Babel Semantic and Positional Identities

Use this page when adding or resolving localized text.

Canonical overview: [Babel Localisation](README.md).

## Two identity shapes

Babel exposes both:

```text
semantic key
→ lookup map
→ corpus index
```

and:

```text
LocalizationEntryId
→ direct corpus index
```

The numeric ID is positional.

## Why fabricated numeric IDs are unsafe

A numeric index only has meaning relative to the exact compiled corpus ordering.

Therefore:

- the same number is not a globally stable semantic identity;
- inserting/removing/reordering corpus entries can change meaning;
- a mod should not invent a numeric index and assume it maps to its own text.

## Public mod preference

Where supported, prefer:

- a semantic identifier;
- explicit fallback text;
- a clearly owned mod localization layer.

A `LocString` fallback is useful for display resilience, but fallback text is not the same as registering a new native Babel translation entry.

## LightLocString caution

`LightLocString` has compact numeric identity without the same fallback behavior.

Use it only when the required native compiled identity is known.

## Verification

Test:

1. semantic key/identity;
2. current language;
3. fallback behavior;
4. missing-key behavior;
5. language switch/reload;
6. Story/UI consumer where used;
7. exact build/corpus scope for numeric identities.
