# Native Bonfire Services

A bonfire expansion should route users into the game's existing service methods instead of rebuilding storage, cooking, alchemy or level-up logic.

## Working implementation lineage

Better Bonfire Menu 0.6.3 was deployed and the added/restored service entries and service grid were reported working in game.

## Native owners

Useful `FireplaceUI` service methods include:

- `OpenHeroStorage()`
- `CookAction()`
- `AlchemyAction()`
- native level-up/rest/service flows owned by the fireplace UI

The working UI path attaches from `VFireplaceUI.OnInitialize`.

## Working UI pattern

~~~text
VFireplaceUI initializes
→ attach one Services entry
→ build service choices from native button styling
→ service selected
→ call the existing FireplaceUI action
→ native service screen owns its own transaction/lifecycle
→ return to bonfire/services UI
~~~

The working implementation cloned the initialized native Level Up button for service rows rather than inventing a visually unrelated button system.

## Availability

Keep an unavailable service visible but non-interactable when the native prerequisite is absent.

Examples from the working implementation:

- Recall Pet requires an applicable left-behind pet;
- Arrow Crafting only opens the native crafting route when the vanilla prerequisite exists.

That keeps the mod from bypassing native progression gates.

## Rule

The menu may own **routing and presentation**. The native service still owns its gameplay operation.
