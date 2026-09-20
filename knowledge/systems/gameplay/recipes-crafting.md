# Recipes and Crafting

Use this page when you need to **learn a recipe, expose a recipe at a station, change crafting availability, or reason about recipe persistence**.

The key rule is:

> Recipe definition, learned-recipe state, station availability, crafting transaction, and UI are separate native responsibilities.

## Keep these stages separate

~~~text
recipe identity
→ learned/known state
→ station recipe list
→ ingredient check
→ ingredient consumption
→ output creation
→ UI refresh
→ persistence
~~~

A recipe row appearing in a UI does not prove the player has learned it or that crafting will succeed correctly.

## Existing recipes

For an existing loaded recipe:

- resolve the exact native recipe identity;
- use the native `HeroRecipes`/learning path when you want the Hero to learn it;
- let the native station and inventory systems perform crafting.

`HeroRecipes.LearnRecipe(IRecipe)` is the important existing-recipe learning surface documented elsewhere.

That operation is different from registering a genuinely new recipe definition.

## Runtime station recipe lists

Research also includes runtime recipe-enumeration/append experiments.

Those can prove:

- a station sees a recipe;
- a UI can display it;
- the runtime list can be extended.

They do not automatically prove persistent learning or save-safe custom recipe registration.

## Let the native crafting transaction own crafting

Keep native systems responsible for:

- whether the station can craft it;
- ingredient availability;
- quantities;
- consumption;
- output creation;
- inventory changes;
- UI refresh.

Do not maintain a second mod-owned inventory/ingredient truth.

## A useful implementation sequence

For a new crafting feature:

1. prove the recipe identity;
2. prove the relevant station can enumerate it;
3. prove ingredient references resolve;
4. prove output references resolve;
5. let the native craft transaction run;
6. verify inventory changes;
7. then test learned-state/save behavior separately if required.

## Common mistakes

- "recipe is visible" = "recipe is registered correctly";
- "station can enumerate it" = "Hero learned it";
- runtime recipe append = persistent recipe registration;
- UI row injection = native crafting transaction;
- directly editing inventory quantities instead of letting the native craft operation own them;
- assuming custom recipe persistence from one current-session test.

## How to verify

Check:

1. exact recipe identity;
2. exact crafting station/template;
3. ingredient identities and counts;
4. output identity/count;
5. learned-state requirements;
6. station enumeration;
7. craft availability;
8. native ingredient consumption;
9. output creation;
10. UI refresh;
11. repeated craft behavior;
12. save/load if learned/custom state is meant to persist.

## Evidence limits

Existing-recipe learning and several runtime station/enumeration paths are source-backed.

A single general-purpose, save-safe custom recipe registration process is not yet established for every recipe/station family.
