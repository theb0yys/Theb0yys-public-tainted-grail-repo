---
document_type: system
scope: FoA fireplace/bonfire native service ownership
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
last_verified: 2026-09-20
---

# Bonfire / Fireplace Native Services

FoA already owns a large set of bonfire services through `FireplaceUI`, `WyrdRepellingFireplaceUI` and their views.

## Native actions

Verified service entry points include:

| Capability | Native owner/action |
| --- | --- |
| Stash | `FireplaceUI.OpenHeroStorage()` → `Hero.Current.Storage.Open()` |
| Cooking | `FireplaceUI.CookAction()` |
| Alchemy | `FireplaceUI.AlchemyAction()` |
| Handcrafting | `FireplaceUI.HandcraftingAction()` |
| Rest | `FireplaceUI.GoToSleepAction()` |
| Level up | `FireplaceUI.LevelUpAction()` |
| Save | `FireplaceUI.SaveGame()` with native save guards |
| Fast travel | `WyrdRepellingFireplaceUI.FastTravel()` |
| Recall pet | `WyrdRepellingFireplaceUI.RecallPet()` |
| Sharpen/upgrade | `GemsUI.OpenSharpeningUI()` |
| Identify | `GemsUI.OpenIdentifyUI()` |
| Armour weight reduction | `GemsUI.OpenGemsUI(WeightReduction)` |
| Relic/gem management | `GemsUITabType.GemManagement` |
| Transmog | `GemsUITabType.Transmogrify` |

## Native preconditions remain authoritative

The upgraded bonfire view already decides when some actions are available, e.g. open-world fast travel, crafting level, survival save restriction, or left-behind pet state.

A community menu should call the real action only when the real precondition is satisfied—not simulate the outcome independently.
