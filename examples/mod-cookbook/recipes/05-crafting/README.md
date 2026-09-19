# Recipe — Crafting / Alchemy Recipes

**Category:** crafting  
**Source-path evidence:** SOURCE_CONFIRMED validation prototype  
**Shipping-ready persistent recipe registration:** NOT_PROVEN

The owner workspace contains a validation-only runtime recipe append prototype.

Its source demonstrates these concrete pieces:

- create an `AlchemyRecipe` on a mod-owned runtime `GameObject`;
- assign a unique GUID;
- assign outcome through `TemplateReference`;
- create ingredient rows from template GUIDs/counts;
- append the recipe to a selected `AlchemyTemplate` recipe list;
- optionally provide runtime-only display/known-recipe shims.

## Why this stays a recipe, not a drop-in mod

The proof source explicitly does **not** establish safe persistent recipe registration or save integration.

A runtime recipe that appears in a grid is not automatically:

- learned/persisted correctly;
- available after reload;
- compatible with every crafting station;
- safe to add/remove mid-save.

## Safe development order

1. Use only existing item templates for outcome/ingredients.
2. Create one runtime-only test recipe.
3. Prove it appears in the intended station.
4. Prove ingredient consumption and output quantity.
5. Remove all UI shims and test native visibility rules.
6. Research persistent registration separately.
7. Only then decide whether the recipe should survive reload/save.

Do not reuse the validation proof's concrete item GUIDs as public defaults.
