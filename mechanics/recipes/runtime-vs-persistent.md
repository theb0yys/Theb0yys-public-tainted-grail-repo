---
document_type: mechanic
scope: runtime recipe construction versus persistent learning
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: MIXED
  persistence: PARTIAL
last_verified: 2026-09-20
---

# Runtime Recipe vs Persistent Recipe

FoA exposes two importantly different recipe problems.

## Runtime construction

A validation/prototype route can construct an `AlchemyRecipe` from existing identities and append it to an `AlchemyTemplate` recipe enumeration.

That demonstrates runtime composition. It does **not** imply that the recipe is learned or durable.

## Persistent learning of an existing recipe

A separate route resolves an existing loaded `IRecipe`, obtains `HeroRecipes`, skips already-learned entries and calls `HeroRecipes.LearnRecipe(IRecipe)`.

Private evidence explicitly kept live save/reload validation separate.

## Rule

Do not use a runtime “known recipe” shim as proof of persistent learning, and do not use successful `LearnRecipe` source inspection as proof of generic custom-recipe persistence.
