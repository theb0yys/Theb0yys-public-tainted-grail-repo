# Recipes and Crafting: Three Different Integration Lanes

> **Reference/process page.** Recipe work must distinguish **learning an existing recipe**, **appending a runtime-only recipe**, and **registering genuinely new persistent recipe content**.

## What this system is

Crafting is station- and recipe-owner specific.

The three important lanes are:

~~~text
A. existing recipe learn
B. runtime recipe append
C. custom persistent recipe registration
~~~

Evidence for one lane does not prove another.

## Who owns it in FoA

Important native owners include:

- `IRecipe` / `BaseRecipe`;
- station templates such as `AlchemyTemplate` and `HandcraftingTemplate`;
- `HeroRecipes` — learned recipe ownership/persistence;
- crafting UI/grid/tab owners;
- item templates for ingredients/outcomes;
- native inventory/crafting transaction owner for actual consumption/output.

## Important identities, types, and methods

Researched surfaces include:

- `HeroRecipes.LearnRecipe(IRecipe)`
- `HeroRecipes.IsLearned`
- `HeroRecipes` serialization of learned recipes as template references
- `AlchemyTemplate.Recipes`
- `HandcraftingTemplate.Recipes`
- `BaseRecipe` as a `Template`/`MonoBehaviour`
- `TemplateReference` for ingredient/outcome refs
- `RecipeTabType.Contains(IRecipe)`
- `RecipeGridUI`
- `RecipeSlot`
- `Crafting.AfterCreate(Item)`

## Where it exists in the lifecycle

## Lane A — Learn an existing loaded recipe

~~~text
exact IRecipe resolves
→ Hero.Current / HeroRecipes ready
→ check already learned
→ HeroRecipes.LearnRecipe(recipe)
→ RecipeLearned event/UI
→ native save writes template reference
~~~

**Current state:** source/native path is understood; generalized live disposable-save reload proof remains pending in the cited implementation.

## Lane B — Runtime-only recipe append

A bounded validation prototype proved:

~~~text
plugin-owned GameObject
→ runtime AlchemyRecipe component
→ existing item TemplateReferences
→ append through AlchemyTemplate.Recipes getter path
→ station/UI sees recipe
→ runtime-known/display shims as needed
→ do NOT write to HeroRecipes.knownRecipes
~~~

Why the GameObject matters: `BaseRecipe` is a Unity `MonoBehaviour`/Template, not a plain data object.

**Current state:** runtime append is a useful proof technique, not persistent recipe registration.

## Lane C — Custom persistent recipe registration

Requires additional proof for:

- stable custom recipe identity;
- template registration;
- station membership;
- localisation/icon/assets;
- learning/discovery;
- craft execution;
- save/load;
- missing-mod behavior;
- duplicate/conflict handling;
- removal/migration.

**Current state:** generic safe custom persistent recipe registration remains unproven.

## How we interact with it

### Existing recipe

Prefer the native `HeroRecipes.LearnRecipe` path once the exact existing recipe has been resolved and the mutation/save gate is intentional.

### Runtime proof recipe

Keep it explicitly runtime-only.

Use existing registered item templates as ingredient/outcome refs.

Do not put an unregistered runtime recipe into saved `HeroRecipes`.

### UI validation

Verify the recipe exists **and** can be found in the active tab.

The research showed that a valid appended recipe can be hidden or look broken because:

- outcome flags do not match the selected tab predicate;
- it appears only under `All` or `Other`;
- the outcome icon is missing;
- an uncraftable slot is rendered at low alpha;
- native ordering places it behind another slot.

UI visibility is not the same as registration.

## Why this route

A major recipe lesson came from a runtime proof that **worked structurally** while appearing missing or blank in UI.

That forced the investigation to separate:

- station membership;
- learned state;
- tab filtering;
- ordering;
- icon availability;
- craftability/alpha;
- persistence.

Without that separation, a UI symptom could be misdiagnosed as recipe-registration failure.

## What goes wrong

### Runtime recipe permanently learned without registration

On reload, the saved template reference may not resolve.

### Getter append called "registration"

Appending to a station's runtime enumeration is not the same as registering a durable `BaseRecipe` template.

### Item flags ignored

Recipe tab routing often derives from outcome characteristics. A recipe can exist but not appear in the expected filtered tab.

### Missing icon mistaken for missing recipe

The slot can exist while looking blank/indistinct.

### UI shims mistaken for gameplay truth

Forcing tab inclusion, known-state or alpha for validation proves UI reachability, not durable recipe semantics.

## How to verify

### Existing recipe learn

Verify:

1. exact `IRecipe` identity;
2. unlearned state;
3. `LearnRecipe` call;
4. learned/UI state;
5. craft station visibility if applicable;
6. disposable save/restart/load when persistence is claimed;
7. duplicate learn behavior.

### Runtime append

Verify:

1. exact station owner;
2. recipe object lifetime;
3. ingredient/outcome TemplateReferences;
4. no duplicate append;
5. station enumeration;
6. tab/filter placement;
7. icon/slot;
8. crafting execution if authorized;
9. helper disable/restart does not require unresolved saved template.

### Custom persistent recipe

Requires all of the above plus registration, missing-mod, migration and coexistence proof.

## Current proof boundary

**Source/runtime evidence exists** for existing recipe resolution/learning paths and a bounded runtime-only Alchemy recipe append proof.

**Not yet a generic public guarantee:** safe custom persistent recipe registration, arbitrary station mutation/removal, custom recipe save migration, or uninstall-safe custom recipe content.
