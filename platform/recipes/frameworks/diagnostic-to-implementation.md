# Recipe: Diagnostic Evidence to Implementation

Use this when you know **what you want**, but not the exact FoA identity/owner.

```text
question
→ load relevant save/scene
→ collect read-only diagnostic dump
→ identify exact GUID/type/scene/owner/context
→ record unknowns
→ inspect canonical system/lifecycle
→ choose the smallest mechanic
→ implement
→ prove terminal runtime success
→ prove cleanup
→ persistence/compatibility validation when relevant
```

## Example: custom item

```text
item_templates.csv
→ exact native source template GUID
→ inspect item/template ownership
→ choose registration mechanic
→ verify provider lookup
→ separately verify presentation and save restoration
```

## Example: creature/actor

```text
creature_templates.csv + spawner_refs.csv
→ exact template evidence
→ actor/location lifecycle research
→ prove one bounded temporary spawn
→ prove cleanup
→ do not turn the dump into an arbitrary spawn allowlist
```

The Diagnostic Tool answers **what exists / what was loaded**.

The system/mechanic research answers **what owns the behaviour and how far the evidence goes**.
