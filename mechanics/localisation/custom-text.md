<!-- Canonical Wave 5 mechanic page split from docs/reference/LOCALISATION_BABEL.md. -->
# Localisation and Questline Babel — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/localisation/README.md) first for ownership, identities, lifecycle, and proof scope.

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

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/localisation/README.md).
