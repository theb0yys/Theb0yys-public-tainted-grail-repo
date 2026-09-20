---
document_type: system
scope: native Story dialogue entry and canonical state ownership
runtime: mono
evidence:
  static: CURRENT_BINARY_STATIC_PACKET
last_verified: 2026-09-20
---

# Native Story / Dialogue Ownership

The inspected native dialogue route uses Story identity rather than a generic free-form dialogue object.

## Entry path

```text
DialogueAction
→ StoryBookmark
→ Story
→ VDialogue
```

Native entry identity includes Story GUID plus bookmark/chapter context.

Native Story lifecycle also owns cleanup when the Story/session is discarded.

## Canonical state remains native

Dialogue may read or affect game state, but canonical truth for these domains stays in FoA owners:

- quests/objectives;
- Story flags / GameplayMemory;
- inventory;
- factions/reputation;
- rewards;
- actors/world state;
- native save truth.

A third-party dialogue engine variable with the same name is not automatically canonical game state.

## Migration principle

Any external dialogue system should route only exact reviewed bindings.

Unmigrated/ambiguous/unsupported conversations should remain native Story-owned.
