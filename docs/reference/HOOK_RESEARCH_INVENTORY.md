# Hook Research Inventory

> **Reference/research page.** This is a broader inventory of Harmony targets found in the maintainer's working repository. Entries here mean **the target exists in inspected source and has been used for a bounded purpose**. They are not blanket recommendations and do not automatically inherit the validation level of the mod that used them.

## What this system is

The curated [Hook Catalogue](HOOK_CATALOGUE.md) contains the smaller set of hooks whose lifecycle meaning has already been distilled into reusable reference guidance.

This page is the wider discovery layer: exact native types/methods, patch shape, and the question each hook was used to answer.

## Who owns it in FoA

Harmony never becomes the owner. The native type/method listed below remains the owner of the lifecycle being observed or modified.

## Important identities, types, and methods

### Templates and registration

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `TemplatesLoader` | `set_FinishedLoading(bool)` | Postfix | Retry custom item/weapon registration after native templates report ready |

### Items, pickups, containers and theft

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `PickItemAction` | `OnStart` | Postfix in observation path; Prefix in gated theft path | Observe or gate world-item pickup |
| `ContainerUI` | `TakeItemFromContainer` | Postfix observation; Prefix guard in theft path | Observe returned item or gate illegal container taking |
| `ContainerUI` | `TakeAllItems` | Prefix | Gate bulk illegal container action |
| `VReadablePopupUI` | `OnSteal` | Postfix observation; Prefix guard | Observe/gate readable theft |
| `Pickable` | `DefaultActionName` getter | Postfix | Adjust interaction text only for bounded target |
| `Pickable` | `StartInteraction` | Prefix | Gate interaction at native interaction entry |
| `Regrowable` | `DefaultActionName` getter | Postfix | Adjust interaction text for bounded target |
| `Regrowable` | `StartInteraction` | Prefix | Gate interaction |
| `HeroInteraction` | `StartInteraction` | Prefix | Central interaction authorization point used by a bounded theft-safety path |
| `Prompt` | `Tap` | Prefix | Gate prompt tap dispatch |
| `Prompt` | `Hold` | Prefix | Rewrite/gate hold prompt behavior |
| `HeroInteractionHoldUI` | `Handle` | Prefix | Observe hold lifecycle before native dispatch |

### Lockpicking

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `LockpickingInteraction` | `ConsumePickHP(float)` | Prefix | Conditionally suppress lockpick durability consumption |

### Inventory stats and equipment-derived gameplay

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `ItemStat` | `ModifiedValue` getter | Postfix | Apply scoped equipped-item stamina-cost modifiers |
| `ItemSet` | `ApplyStats` | Prefix | Runtime/stat application path with separate Mono/IL2CPP signatures |

### Prices and economy

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `DefaultPriceProvider` | `SellPrice` | Postfix | Adjust merchant-to-hero price after native calculation |
| `HeroPriceProvider` | `SellPrice` | Postfix | Adjust hero-sale price after native calculation |
| `HeroPriceProvider` | `GetStolenModifier` | Postfix | Modify stolen-item price modifier in a bounded crime/economy path |
| `Item` | `ExactPrice` getter | Postfix | Observe/adjust exact item price |
| `Item` | `ExactBuyPrice` getter | Postfix | Observe/adjust exact buy price |
| `SFenceAllItems` | `FenceCost` getter | Postfix | Adjust fence-all-items cost |
| `Shop` | `OpenShop` | Prefix/observation depending route | Merchant open/restock lifecycle |
| `ShopUI` | `OnFullyInitialized` | Prefix | Proven custom-stock insertion before original item-list snapshot |

### Crafting and recipes

The read-only recipe-access proof used multiple hooks to map the full UI/data path:

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `HandcraftingTemplate` | `Recipes` getter | Postfix | Observe handcraft recipe enumeration |
| `AlchemyTemplate` | `Recipes` getter | Postfix | Observe alchemy recipe enumeration |
| `CookingTemplate` | `Recipes` getter | Postfix | Observe cooking recipe enumeration |
| `Crafting` | `Recipes` getter | Postfix | Observe active crafting recipe set |
| `Crafting` | `AfterViewSpawned` | Postfix | Observe crafting UI lifecycle |
| `RecipeGridUI` | constructor taking recipes | Postfix | Observe recipe list at UI construction |
| `RecipeGridUI` | `Refresh` | Postfix | Observe refresh path |
| `RecipeTabContents` | `AfterViewSpawned` | Postfix | Observe tab lifecycle |
| `RecipeTabContents` | `Refresh` | Postfix | Observe tab refresh |
| `RecipeSlot` | constructor | Postfix | Observe individual recipe slot construction |

These were used as a **read-only mapping lane**. The existence of these hooks does not mean recipe registration/persistence is solved.

### Combat, damage and death

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `HealthElement` | `TakeDamage` | Prefix | Observe incoming damage before native processing |
| `NpcElement` | `DeathNonCriticalFunctions` | Postfix | Observe NPC death lifecycle after the native non-critical death work |
| `ProficiencyEventListener` | `XPGainEvent` | Postfix | Observe proficiency XP gains |

### Movement, audio and hero presentation

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `VHeroFootsteps` | `FootStep` | Prefix | Suppress bounded footstep audio |
| `ThieveryNoise` | `MakeNoiseByHero` | Prefix | Suppress bounded crouch/noise emission |
| `HumanoidMovementBase` | `MakeMovementSound` | Prefix | Gate movement-sound path |
| `HumanoidMovementBase` | `Jump` | Postfix | Adjust launch result after native jump logic |

### Dialogue, choices and story

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `Story` | `OfferChoice` | Prefix | Observe story-choice offer before native processing |
| `VChoice` | `Select(Choice)` | Prefix + Postfix | Observe player choice selection lifecycle |
| `Choice` | constructor `(ChoiceConfig, Story)` | Postfix on IL2CPP path | Attach/prepare hover-information state when getter patch shape differs |
| `Choice` | `HoverInfos` getter | Postfix on Mono path | Extend/observe native choice hover information |

The different Mono/IL2CPP hook shapes are a concrete reminder that the two runtime lanes are not interchangeable.

### New game, rest, travel and discovery

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `NewGameLoading` | `OnComplete` | Postfix | Observe new-game loading completion |
| `RestPopupUI` | `SkipWeatherTime` | Postfix | Observe rest/time-advance completion/context |
| `Hero` | `WalkThroughPortal` | Postfix | Observe portal travel lifecycle |
| `Hero` | `ArrivedAtPortal` | Postfix | Observe post-portal arrival |
| `WyrdRepellingFireplaceUI` | `FastTravel` | Prefix | Observe native fireplace fast-travel entry |
| `Portal.FastTravel` | `To(Hero, Portal, bool, bool)` | Prefix | Observe fast-travel dispatch/context |
| `LocationDiscovery` | `UnityUpdate` | Prefix + Postfix state capture | Detect discovery-state transition rather than just method invocation |

### Bonfire UI

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `VFireplaceUI` | `OnInitialize` | Postfix | Enter/extend native fireplace UI only after native initialization |
| `FireplaceUI` | `Close(bool)` | Prefix | Intercept bounded native back/close behavior |

### Map fog

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `MapUI` | `ToggleFogOfWar` | Prefix | Override fog toggle in bounded mod |
| `FogOfWar` | `CreateMaskTexture` | Prefix | Substitute fog mask creation result |
| `FogOfWar` | `IsPositionRevealed` | Prefix | Override reveal query |

These are direct behavior overrides, not merely observations, so compatibility and downstream assumptions matter more than for diagnostic Postfixes.

### Save lifecycle

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| concrete Steam/NoCloud/Debug/GOG cloud service types | `EndSave(string)` | Postfix | Observe a completed native save-slot write |

This is an observation seam. It is **not** evidence of a general mod-owned serialization registrar.

### Rendering / terrain research

| Native type | Method | Patch | Observed use |
|---|---|---|---|
| `MedusaBrgRenderer` | `SetRenderers` | Harmony patch in terrain-replacement research | Intervene at renderer-definition application for a bounded terrain/material path |

This is a proprietary rendering surface and should be treated as high-risk/version-sensitive until its full owner/lifecycle is mapped.

## Where it exists in the lifecycle

The inventory itself demonstrates a pattern:

~~~text
getter/constructor hooks
→ useful for observing data/UI construction

Prefix hooks
→ useful when a decision must happen before native mutation

Postfix hooks
→ useful when native state/result must exist first

Prefix + Postfix
→ useful for before/after transition comparison
~~~

That is a heuristic for choosing patch shape, not a substitute for target research.

## How we interact with it

Before promoting any inventory row into the curated catalogue:

1. inspect the owning type/method;
2. establish the before/after lifecycle;
3. inspect all relevant overloads;
4. identify whether the hook observes, modifies input, modifies output, or suppresses original behavior;
5. check Mono/IL2CPP differences;
6. inspect failure history;
7. run claim-fit validation.

## Why this route

A large raw hook list without lifecycle meaning would reproduce the exact problem this repository is trying to solve.

The inventory therefore preserves discovery breadth while the curated catalogue preserves reusable understanding.

## What goes wrong

A concrete failure from the working repository:

> An experimental patch on `HeroItems.Add` installed during plug-in startup caused save-load initialization failures and was removed.

That failure teaches several things:

- a central method is not automatically a safe hook;
- Harmony patch installation itself occurs during a sensitive startup/lifecycle period;
- low-level owner methods may execute while the game is reconstructing state;
- a diagnostic hook can destabilize the exact lifecycle it is attempting to observe.

The safer answer was **not** “patch `HeroItems.Add` differently until it stops crashing.” The route was abandoned and the research moved to a safer, more specific owner/hook.

## How to verify

A hook is ready for the curated catalogue only when the page can explain:

- exact type/signature;
- why Prefix/Postfix is correct;
- state before and after;
- what native owner remains responsible;
- compatibility/version risk;
- actual observation/validation;
- cleanup/unpatch behavior;
- explicit non-claims.

## Current proof boundary

This inventory is broader than the curated hook catalogue. Source presence proves that these targets were used in the working repository; it does not automatically prove that each is generally safe or current across all builds.
