---
document_type: system
scope: FoA fireplace/bonfire native service ownership
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
last_verified: 2026-09-20
---

# Bonfire and Fireplace Services

Use this page when you want to add a **bonfire convenience action** without rebuilding the gameplay system behind it.

FoA already has native entry points for many services. In most cases, the safest mod is a better way to reach those services—not a replacement implementation.

## Native bonfire actions

Useful researched routes include:

| Player-facing action | Native route | Important condition / note |
| --- | --- | --- |
| Open Stash | `FireplaceUI.OpenHeroStorage()` | Opens `Hero.Current.Storage` through the native path |
| Cooking | `FireplaceUI.CookAction()` | Opens the native bonfire cooking UI |
| Alchemy | `FireplaceUI.AlchemyAction()` | Opens the native alchemy UI |
| Handcrafting | `FireplaceUI.HandcraftingAction()` | Native availability depends on bonfire crafting progression |
| Rest | `FireplaceUI.GoToSleepAction()` | Opens the native rest flow |
| Level Up | `FireplaceUI.LevelUpAction()` | Opens the native character progression UI |
| Save | `FireplaceUI.SaveGame()` | Native save guards remain authoritative |
| Fast Travel | `WyrdRepellingFireplaceUI.FastTravel()` | Native upgraded/open-world conditions apply |
| Recall Pet | `WyrdRepellingFireplaceUI.RecallPet()` | Only valid when a pet is actually left behind |
| Identify Items | `GemsUI.OpenIdentifyUI()` | Opens native identify UI |
| Sharpen / Upgrade | native Gems UI Sharpening tab | Native gear-service flow |
| Manage Relics | `GemsUITabType.GemManagement` | Opens native relic/gem management |
| Transmogrify | `GemsUITabType.Transmogrify` | Native transmog UI; pricing policy is a separate concern |
| Gear Care | `GemsUITabType.WeightReduction` | Verified armour weight-reduction service, **not** proven durability repair |

## Arrow Crafting is a convenience entry, not recipe injection

A dedicated Arrow Crafting button can route to native handcrafting when the bonfire crafting upgrade is available.

That does **not** mean the mod:

- adds new arrow recipes;
- force-learns recipes;
- bypasses crafting progression;
- replaces the native crafting transaction.

It is only another way to open the existing handcrafting service.

## Merchant access is loaded-shop reuse

A bonfire menu can enumerate currently loaded `Shop` Models and open a selected live shop with `Shop.OpenShop()`.

That is not the same as creating a universal bonfire merchant.

The loaded-shop route does **not** create:

- new shop templates;
- new merchant Locations;
- custom stock pools;
- rare-item injection;
- buyback changes;
- quest/unique-item handling.

Merchant profiles are UI filters over shops the game already has loaded.

## Preserve native availability rules

The upgraded bonfire already decides when several actions are valid.

Examples found in the inspected code include:

- handcrafting availability from `Hero.Current.Development.BonfireCraftingLevel`;
- Fast Travel only for the upgraded bonfire/open-world context;
- Save Game only under the relevant native save restriction/guard state;
- Recall Pet only when `PetUtils.HasPetBeenLeftBehind()` is true.

A convenience menu can show an unavailable action as disabled and explain why, but it should not force the native action through an invalid state.

## Opening a native service is not the same as owning it

A useful host-menu flow is:

~~~text
bonfire menu
→ player selects convenience entry
→ check native precondition
→ call native service
→ native UI/gameplay owns the transaction
→ native service closes
→ return to the bonfire host
~~~

The host menu owns navigation and presentation.

The game still owns storage, crafting, progression, saving, travel, shops, and gear-service behavior.

## UI hosting is a separate problem

Knowing the correct service method does not prove that a custom replacement submenu has correct:

- input ownership;
- focus;
- controller navigation;
- disabled-state behavior;
- Back/Cancel handling;
- service-return restoration;
- teardown.

See [Better Bonfire Menu case study](../../../research/case-studies/bonfire/native-service-reuse.md) for the versioned history showing how those were discovered separately.

## Evidence limits

The service mappings above are supported by source/decompilation work.

Specific UI-host implementations have their own runtime proof. Do not promote "the native service opens" into "the replacement submenu is fully validated."
