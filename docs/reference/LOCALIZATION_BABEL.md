# Localisation and Questline Babel

> **Reference page.** Use this when adding names, descriptions, dialogue text, UI text, or reasoning about multilingual support.

## What this system is

FoA uses Unity Localization for authoring/source tables, then Questline's **Babel** system for the normal compiled runtime text database.

High-level path:

~~~text
Unity String Tables + LocString authoring
→ Questline Babel bake
→ per-language payloads
→ languages.arch
→ BabelManager/provider
→ LocString / LightLocString consumers
~~~

This is not merely "Unity Localization at runtime."

## Who owns it in FoA

Important owners/types include:

- `LocString`;
- `OptionalLocString`;
- `LightLocString`;
- `LocalizationEntryId`;
- `ILocalizationManager`;
- `BabelManager`;
- Babel providers;
- editor-side Babel baker/tooling.

Unity still owns locale/source-table infrastructure. Questline owns the compiled corpus, runtime lookup, and Story text baking.

## Important identities, types, and methods

### LocString

A `LocString` carries:

- semantic ID;
- optional override ID;
- literal fallback.

A direct cast from string:

~~~csharp
(LocString)"Example"
~~~

creates fallback text. It **does not register a Babel term**.

### LightLocString

`LightLocString` stores only a numeric `LocalizationEntryId`.

It has no literal fallback and is tightly coupled to the compiled Babel corpus ordering used when it was baked.

### Babel numeric IDs

The researched IDs are positional/global-index based rather than content hashes.

That makes fabricated numeric IDs dangerous.

## Where it exists in the lifecycle

### Authoring

~~~text
LocString semantic key
→ Unity String Table source
→ Babel global corpus/order
→ locale payloads
→ languages.arch
~~~

### Runtime

~~~text
semantic key or numeric ID
→ BabelManager/provider
→ selected-language string
→ LocString fallback if semantic lookup is empty
~~~

Items can cache resolved text into runtime token text when initialized.

Therefore changing the provider/locale does not automatically prove every already-live item/UI element refreshes.

## How we interact with it

### Use semantic mod-owned keys

A custom item can have:

- stable package-owned semantic key;
- explicit fallback text.

Do not fabricate global numeric Babel IDs.

### Treat fallback-only text honestly

A custom `LocString` created from a raw string can be readable in the source language while still having **no registered native translation entry**.

### Keep localisation identity separate from display text

The displayed English text is not the durable localization identity.

### Do not rebuild/replace the base corpus casually

Story `LightLocString` values and other compiled data can depend on exact global positions.

## Why this route

The research found a hard compatibility constraint:

> rebuilding or privately reordering the base Babel corpus can cause existing numeric IDs to resolve to the wrong text, not merely missing text.

That is much more dangerous than an ordinary missing-key fallback.

## What goes wrong

### Raw string cast described as localisation registration

It is fallback text only.

### Fabricated LocalizationEntryId

Can point at an unrelated corpus position.

### Base `languages.arch` replacement

Risks changing global corpus compatibility.

### Hot language switching assumed to refresh all live UI

Some objects cache resolved text. A provider switch alone is not proof of complete live refresh.

### Display string used as content identity

Localized text can change and collide.

## How to verify

For mod text, verify:

1. stable semantic key;
2. no collision with another mod/package key;
3. fallback displays;
4. intended localization provider resolves translated value if translation support exists;
5. item/UI refresh path works for live objects;
6. language switch/restart behavior is understood;
7. no native Babel numeric ID is fabricated.

## Current proof boundary

Fallback `LocString` use and the Babel architecture are well supported by source research.

A generic native Babel extension API for arbitrary mod terms is **not** currently proven.

Base `languages.arch` mutation, private global ID allocation, hot multilingual Story compilation, and IL2CPP equivalence remain outside the proven public process.
