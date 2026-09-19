# Hook and Lifecycle Surface Catalogue

> **Reference page.** This catalogue records researched intervention/observation surfaces from the working repository. A row may be **proven**, **source-inspected**, **diagnostic-only**, or **blocked/partial**. A listed method is not automatically the recommended target for a new mod.

## What this system is

A useful FoA "hook" can be:

- a Harmony Prefix/Postfix;
- a native event;
- a Model/Element lifecycle callback;
- a service/template readiness boundary;
- a scene transition callback;
- a save-completion observation;
- a normal owner API that is safer than patching.

Choose the owner/lifecycle first, then choose the hook.

## Who owns it in FoA

Harmony does not own the behavior it patches. The native type/method/event does.

For every surface below, treat the native owner and its lifecycle as the real contract.

## Important identities, types, and methods

### Core lifecycle and registries

| Surface | Kind | Best-known use | Status / warning |
|---|---|---|---|
| `Model.Events.BeforeFullyInitialized` / `AfterFullyInitialized` | Native event | Observe Model readiness | Official lifecycle surface; prefer over polling where appropriate |
| `Model.Events.BeforeDiscarded` / `BeingDiscarded` / `AfterDiscarded` | Native event | Cleanup/ownership observation | Official lifecycle surface; listener owner/cleanup still matters |
| `Model.Events.AfterElementsCollectionModified` | Native event | React to Element membership changes | Official lifecycle surface |
| `World.EventSystem.ListenTo("*", ...)` | Native event | Observe an event from every source | Powerful; use stable listener owner and bounded scope |
| `TemplatesLoader.set_FinishedLoading(bool)` | Harmony Postfix / readiness boundary | Retry custom-template registration after native template loading | Source-inspected across multiple item/weapon consumers; private lifecycle, patch-sensitive |
| `TemplatesProvider.AllLoaded` | Readiness check | Guard exact template lookup | Native provider contract; calling too early is explicitly rejected |
| `TemplatesProvider.Get<T>(guid)` | Owner API | Resolve exact loaded template | Prefer normal lookup after any registration |
| `World.Add(model/item/...)` | Owner API | Enter FoA MVC/runtime ownership | Native lifecycle path, not a Harmony hook |
| `Model.Discard()` | Owner API | Native Model teardown | Prefer over simulating cleanup with raw Unity destruction |
| `View.Discard()` | Owner API | View/world/event/asset teardown | `Destroy(view.gameObject)` is not equivalent |

### Items, inventory, merchants, crafting

| Target | Patch/route | What it tells us | Evidence boundary |
|---|---|---|---|
| `Shop.OpenShop()` | Prefix / lifecycle observation | Shop is entering stock-open/decompression flow | Source-inspected; some restock mutations remain runtime-unverified |
| `ShopUI.OnFullyInitialized()` | Prefix | Proven custom-item insertion point after stock decompression and before original UI list capture | Bounded runtime-visible custom-item proof |
| `LockpickingInteraction.ConsumePickHP(float)` | Prefix | Exact lockpick durability-consumption point | Source-inspected custom lockpick implementation; feature-specific runtime proof separate |
| `ContainerUI.TakeItemFromContainer(...)` | Postfix | Observe item successfully taken from a container | Source-inspected Wyrd Hunt path; do not generalise to loot generation ownership |
| `PickItemAction.OnStart` | Postfix | Observe/route a world pickup action | Source-inspected; world-pickup semantics differ from item registration |
| `VReadablePopupUI.OnSteal` | Postfix | Observe successful readable theft action | Source-inspected |
| `HandcraftingTemplate.Recipes` getter | Harmony getter patch | Runtime proof access/append experiments | Validation/proof tooling; not durable recipe-registration proof |
| `AlchemyTemplate.Recipes` getter | Harmony getter patch | Runtime proof recipe visibility/append experiments | Validation/proof tooling; persistence separate |
| `HeroRecipes.LearnRecipe(IRecipe)` | Owner API / diagnostic surface | Native persistent-learning mutation path for an existing loaded recipe | Source-inspected; live save/reload validation still pending |
| `Crafting.AfterCreate(Item)` | Native/diagnostic event surface | Observe successful craft output | Read-only diagnostic use in research |

### Player, stats, combat, interactions

| Target | Patch | Use observed in repository | Status / warning |
|---|---|---|---|
| `HealthElement.TakeDamage(...)` | Prefix | Damage diagnostics/modification | Source-inspected; high-frequency/combat-sensitive |
| `ItemStat.ModifiedValue` getter | Postfix | Modify/observe equipped item stat values | Used by Tainted Gems for scoped stamina-cost effects; hot-path caution |
| `ItemSet.ApplyStats` | Prefix/Postfix depending lane | Observe/apply item-set stat behavior | Source-inspected; Mono/IL2CPP implementation differences exist |
| `HeroInteraction.StartInteraction(...)` | Patch | Interaction authorization/timing | Source-inspected Hold-to-Steal path; input/UI ownership must be understood |
| `Prompt.Tap(...)` | Patch | Prompt interaction input boundary | Source-inspected; do not treat as universal interaction owner |
| `ProficiencyEventListener.XPGainEvent(...)` | Postfix | Observe direct proficiency XP gain | Source-inspected diagnostics/gameplay integration |
| `RestPopupUI.SkipWeatherTime(...)` | Postfix/diagnostic | Observe rest/time/weather transition context | Source-inspected diagnostic route |
| `Hero.WalkThroughPortal(...)` | Diagnostic patch | Observe portal/travel exposure context | Source-inspected; normal scene-travel owner is deeper Portal/ScenePreloader path |

### NPC, death, world actor lifecycle

| Target/surface | Patch/route | Use | Status / warning |
|---|---|---|---|
| `NpcElement.DeathNonCriticalFunctions(...)` | Postfix | Observe NPC death after critical native death work | Source-inspected Wyrd Hunt hook |
| `LocationTemplate.SpawnLocation(...)` | Owner API | Construct a native Location from a resolved template | Source-inspected one-session companion path; not generic population authority |
| `Location.MarkedNotSaved = true` | Lifecycle policy | Keep controlled proof/session actor out of save ownership | Source-inspected in companion/creature proofs |
| `Location.Discard()` | Owner API | Explicit native location teardown | Required in one-session actor cleanup paths |
| `NpcDummy` + `Corpse` transition | State/lifecycle observation | Accept native death/corpse handoff | Source-inspected companion/creature lifecycle, not a Harmony hook |

### Dialogue, story, quests, choices

| Target | Patch | Use | Status / warning |
|---|---|---|---|
| `Story.OfferChoice(...)` | Prefix | Observe choice being offered | Source-inspected Smart Save Backups |
| `VChoice.Select(Choice)` | Prefix | Observe player choice selection | Source-inspected Immersive Backgrounds |
| `Choice.HoverInfos` getter (Mono) | Getter patch | Modify/read choice hover information | Source-inspected; IL2CPP route differs |
| `Choice(ChoiceConfig, Story)` constructor (IL2CPP lane) | Constructor patch | Alternative choice-info integration point | Source-inspected branch-specific solution |
| Quest completion targets resolved dynamically | Harmony TargetMethods | Observe completion of multiple quest-related methods | Source-inspected; exact target set must be re-read before reuse |
| `NewGameLoading.OnComplete()` | Postfix | Act after native new-game loading completes | Source-inspected Origins path; not a generic save/load completion hook |

### Economy and item pricing

| Target | Patch | Use | Status / warning |
|---|---|---|---|
| `HeroPriceProvider.GetStolenModifier(...)` | Patch | Crime/economy pricing behavior | Source-inspected |
| `Item.ExactPrice` getter | Patch | Item price observation/modification | Source-inspected; economy implications require separate validation |
| Native price-provider SellPrice methods | Postfix patterns | Adaptive/maladaptive trade multipliers | Decompile/source-backed Tainted Gems path; exact provider semantics matter |

### Audio and presentation

| Target | Patch | Use | Status / warning |
|---|---|---|---|
| `VHeroFootsteps.FootStep(...)` | Prefix | Suppress/replace hero footstep audio for scoped conditions | Source-inspected/runtime-evidenced patterns elsewhere; avoid broad permanent suppression |
| `VCCharacterMagicVFX.CastingBegun` | Postfix | Attach an overlay when native magic casting begins | Source-inspected; template-name family mapping is heuristic, not native VFX ownership proof |
| `VFireplaceUI.OnInitialize` | Postfix | Add/modify bonfire UI services entry | Source-inspected; full UI input/focus/cleanup path required for reusable UI claims |

### Map and fog

| Target | Patch | Use | Status / warning |
|---|---|---|---|
| `MapUI.ToggleFogOfWar(...)` | Harmony patch | Map fog behavior | Source-inspected No Map Fog implementation |
| `FogOfWar.CreateMaskTexture(...)` | Harmony patch | Fog-mask generation behavior | Source-inspected; renderer/UI version risk |

### Save lifecycle

| Target/surface | Patch/route | Meaning | Status / warning |
|---|---|---|---|
| `LoadSave.CanAutoSave()` | Owner API/readiness | Native autosave guard | Decompile/source researched |
| `LoadSave.Save(SaveSlot, bool)` | Owner API / possible capture boundary | Native save request | Does not itself mean disk write succeeded |
| `LoadSave.QuickSave()` | Owner API | Native quicksave request | Native guards still apply |
| concrete cloud service `EndSave(string)` | Postfix | Strong completed-slot-write observation | Source-inspected/decompiled; does not provide custom serialization domain |
| `SaveInProgressHandle.MarkSucceeded` | Researched lifecycle candidate | Strong native success milestone for future sidecar coordination | Static candidate; controlled runtime/crash proof not complete |
| `LoadSave.LoadSaveSlotToCache(...)` | Researched load-stage candidate | Save data staged before restoration | Static candidate; not a promoted general hook |
| `SceneLifetimeEvents.Events.AfterSceneStoriesExecuted` | Researched post-load candidate | Late point after scene stories execute | Static candidate for sidecar apply; not yet a proven generic persistence process |

### Scene and travel lifecycle

| Target/surface | Kind | Meaning | Status / warning |
|---|---|---|---|
| `SceneService.LoadSceneAsync(...)` | Owner API | Native Addressables scene load | Strong static/source contract |
| `SceneService.SceneLoaded(...)` | Owner callback | Scene object announced loaded | Strong static/source contract |
| `SceneService.SceneInitialized(...)` | Owner callback | Scene initialization complete | Strong static/source contract |
| `SceneService.UnloadSceneAsync(...)` | Owner API | Native scene unload | Strong static/source contract |
| `Portal.MapChangeTo` / `ScenePreloader.ChangeMap` | Native travel chain | Player map/scene transition ownership | Source-inspected; custom external scene empirical gate remains NOT_RUN |
| `AdditiveScene.Start()` | Native scene lifecycle | Additive scene reports into SceneService lifecycle | Source/static established |

## Where it exists in the lifecycle

The catalogue should be read as a set of **state boundaries**, not method names.

A hook is useful only when the state you need is guaranteed at that point.

## How we interact with it

Before reusing a catalogue row:

1. read the original owner/process;
2. verify the exact game/runtime lane;
3. resolve exact type/signature;
4. identify state before/after;
5. decide whether a native event/API is safer than Harmony;
6. define cleanup;
7. define the exact claim the hook will prove.

## Why this route

Repository history repeatedly shows that apparently similar hooks can have very different ownership semantics.

Examples:

- portal-related methods are not all scene-transition owners;
- a shop-open hook is not the same as the pre-UI-snapshot insertion boundary;
- a save request is not the same as completed write;
- a recipe getter patch is not persistent recipe learning;
- a VFX cast hook is not exact spell/VFX identity ownership.

## What goes wrong

- selecting hooks by class/method name resemblance;
- copying a hook without the surrounding lifecycle;
- choosing a hot getter for work that should be event-driven;
- forgetting branch-specific Mono/IL2CPP differences;
- treating diagnostic-only patches as production mutation APIs;
- using a source-inspected target without runtime/version revalidation;
- failing to clean up listeners, objects, patches or assets.

## How to verify

Every production/reusable hook record should eventually contain:

- runtime lane;
- game build;
- target assembly and hash where practical;
- fully qualified type;
- exact signature;
- patch/event kind;
- lifecycle preconditions;
- invocation evidence;
- downstream effect/observation;
- cleanup;
- failure behavior;
- compatibility risk;
- proof state.

## Current proof boundary

This catalogue is intentionally broader than the original promoted mechanics index, but it preserves evidence distinctions. Many rows are **source-inspected integration examples**, not promoted universal recommendations.

Use [Golden Rules](GOLDEN_RULES.md), [MVC Lifecycle](MVC_MODELS_ELEMENTS_EVENTS.md), and [Scene/Service/Template Lifecycle](SCENES_SERVICES_TEMPLATES.md) before choosing a target.
