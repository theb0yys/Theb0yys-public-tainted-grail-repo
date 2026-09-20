# Crafting Looks Stash-Unaware but Native Logic Already Uses the Stash

Use this when crafting or upgrade UI appears to ignore stash ingredients.

Working lesson: [Crafting Was Already Stash-Aware](../../../research/case-studies/storage/crafting-already-stash-aware.md).

## Symptom

The UI or ingredient count makes it look as though crafting can only see the carried inventory.

A naive fix would be:

```text
patch crafting
→ manually merge inventory + stash
→ replace native ingredient consumption
```

Do not do that before checking the owner.

## What research found

The deeper native path already:

- requests Hero Storage;
- combines inventory + stash for recipe ingredient matching;
- combines inventory + stash for upgrade checks;
- can consume from those matched items.

The actual problem/opportunity is **presentation clarity**, not necessarily craftability.

## Diagnostic sequence

1. test whether the native craft/upgrade actually succeeds with ingredients split across inventory and stash;
2. inspect the native matching owner;
3. inspect the displayed count separately;
4. determine whether only the UI projection is misleading;
5. change presentation only if gameplay ownership is already correct.

## Do not

- add a second ingredient pool;
- rewrite consumption because a label is wrong;
- duplicate native storage ownership;
- claim "stash crafting support" before testing the actual craft result.

## Useful fix shape

If the gameplay path is correct:

```text
native inventory + stash truth
→ read-only projection
→ corrected count/status text
```

Keep the native crafting transaction unchanged.

## Evidence boundary

Static/decompiled evidence established that the native crafting/upgrade logic was already stash-aware. The case is a corrective ownership lesson, not a new crafting-system implementation.
