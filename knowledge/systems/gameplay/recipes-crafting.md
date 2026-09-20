# Recipes and Crafting

Use this page when you are working with recipe identity, learned recipes, station recipe lists, ingredient consumption, or crafting UI. Those are separate native responsibilities and should not be treated as one registration step.

This page documents the native owners around recipes and crafting.

## Native ownership

Keep these concepts separate:

- recipe identity;
- learned-recipe state;
- station/runtime recipe enumeration;
- ingredient ownership and consumption;
- crafting UI.

A mod should use the native owner for each stage instead of treating a visible recipe row as complete registration.

## Existing recipes

For existing loaded recipes, use the native recipe identity and the game's normal learning/crafting owners.

## Crafting interaction

Let the native station and inventory systems own:

- availability;
- ingredient checks;
- quantity changes;
- item creation;
- UI refresh.

Do not reimplement those transactions in a parallel mod-owned inventory model.

## Rule

Recipe identity, learning, crafting and persistence are separate owners. Only publish a custom-content path when the complete lifecycle is established.
