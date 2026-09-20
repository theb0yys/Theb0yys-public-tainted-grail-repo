---
document_type: mechanic
scope: runtime recipe construction versus persistent learning
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: MIXED
  persistence: LIVE_SAVE_RELOAD_PENDING_FOR_EXISTING_RECIPE_ADAPTER
last_verified: 2026-09-20
---

# Runtime Recipe vs Persistent Recipe

FoA exposes several different recipe problems. Keep them separate.

## Runtime recipe construction

A proof route can construct an `AlchemyRecipe` from existing identities and append it to an `AlchemyTemplate` recipe enumeration.

That demonstrates runtime composition only.

## Runtime known-recipe shim

A separate proof can treat only the runtime proof recipe as known without writing `HeroRecipes`.

That is explicitly **not persistence**.

## Persistent learning of an existing recipe

A project adapter:

```text
requires explicit mutation gates
→ resolves an existing loaded IRecipe
→ obtains HeroRecipes
→ skips already-learned recipe
→ records authorization receipt
→ calls HeroRecipes.LearnRecipe(recipe)
```

The private decision required a throwaway-save save/reload check before any runtime-persistence claim.

## Do not collapse these

```text
runtime recipe object
≠ runtime known shim
≠ persistent learning of existing recipe
≠ persistent custom recipe registration
```
