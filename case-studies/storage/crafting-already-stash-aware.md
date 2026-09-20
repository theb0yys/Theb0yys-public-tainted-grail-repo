# Crafting Was Already Stash-Aware

Avalon Stash began from a user-visible ingredient-count problem.

Decompilation showed the deeper logic already did the right thing:

- crafting requested Hero Storage;
- recipe ingredient matching combined inventory + stash;
- upgrade checks combined inventory + stash;
- native consumption could spend from those matched items.

The real defect/opportunity was **presentation clarity**, not craftability.

## Lesson

Before implementing “make system X use stash”, inspect whether the game already owns that behaviour and only the UI is misleading.
