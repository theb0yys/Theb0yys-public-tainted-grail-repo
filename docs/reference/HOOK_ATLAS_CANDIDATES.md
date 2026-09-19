# Source-Located Hook Atlas

> **Research atlas.** These targets were located in working-repository source as Harmony patches or explicit integration targets. A listing here means **"this is a real place to investigate"**, not **"this is the recommended production hook."**

Use [Hook Catalogue](HOOK_CATALOGUE.md) for the smaller curated set with stronger lifecycle interpretation.

## What this system is

This atlas helps answer:

> Has the repository already found a concrete FoA type/method near the behavior I am researching?

Before reuse, inspect:

- native owner;
- method signature;
- lifecycle;
- patch kind;
- runtime lane;
- original implementation's scope;
- evidence state;
- cleanup/performance risk.

## Content, templates, items, crafting

| Type / method | Seen in working source for | Immediate caution |
|---|---|---|
| `TemplatesLoader.FinishedLoading` setter | custom item/template registration retry | Private/native lifecycle; patch-sensitive |
| `SkillInitialization` targets | custom item native-effect work | Exact overload/semantic target must be re-read |
| `ItemUtils.FillSimilarItemsDataList(...)` | custom bottle/recipe-equivalence integration | Similar-item semantics are not general item registration |
| `ItemIconComponent.Refresh` | custom item icon replacement | UI presentation only |
| `Item.Use` | medicine/consumable observation and cooldown gating | High-value gameplay action; classify owner/item carefully |
| `HeroRecipes.LearnRecipe(IRecipe)` | recipe-learning diagnostics / persistent existing-recipe route | Durable claim requires save validation |
| `HeroRecipes.IsLearned` | runtime recipe-access proof | Can alter visibility/known state without persistence |
| `HandcraftingTemplate.Recipes` getter | runtime recipe-access proof | Getter patch is not durable registration |
| `AlchemyTemplate.Recipes` getter | runtime recipe append/access proof | Runtime collection mutation ≠ save-safe custom recipe |
| `Shop.OpenShop` | merchant restock/custom stock | Lifecycle differs from UI snapshot boundary |
| `ShopUI.OnFullyInitialized` | custom item insertion before item-list capture | Stronger bounded merchant insertion boundary |

## Inventory, world pickup, containers, interactions

| Type / method | Seen for | Caution |
|---|---|---|
| `PickItemAction.OnStart` | world pickup observation | Pickup execution ≠ item definition/loot generation |
| `ContainerUI.TakeItemFromContainer` | container-loot observation | UI transfer hook, not loot-table authority |
| `VReadablePopupUI.OnSteal` | readable theft observation | Presentation/action surface, not full crime lifecycle |
| `HeroInteraction.StartInteraction` | theft/interaction safety | Must map prompt/input/interaction owner |
| `Prompt.Tap` | prompt action boundary | Not a universal interaction dispatcher |
| `Pickable.StartInteraction` | world-action diagnostics | Source-located research hook |
| `Regrowable.StartInteraction` | harvest/regrowable diagnostics | Source-located research hook |

## Crime, theft, bounty, pricing

| Type / method | Seen for | Caution |
|---|---|---|
| `IllegalActionTracker.AddWatchingNpc` | crime-awareness changes | Crime ownership/state implications |
| `IllegalActionTracker.OnHeroCrouchToggled` | stealth/crime awareness | High-level state hook, not all detection |
| `NpcCrimeReactions.Pickpocketing` | pickpocket reaction changes | NPC/crime-specific |
| `CommitCrime.Murder(IWithCrimeNpcValue)` | murder consequence observation/mutation | Story/faction/save impact possible |
| `CrimeUtils.AddBounty` | bounty changes | Persistent economy/crime state |
| `HeroPriceProvider.GetStolenModifier` | stolen-goods pricing | Economy semantics need validation |
| `Item.ExactPrice` getter | price observation/modification | Hot/broad getter; scope tightly |
| `TradeUtils.TryTrade` | trade practice/telemetry | Full transaction owner should remain native |

## Combat, stats, stamina, AI

| Type / method | Seen for | Caution |
|---|---|---|
| `HealthElement.TakeDamage` | damage diagnostics/cheats | Central combat hook; high-frequency/high-risk |
| `HeroStats.HeroStatsWrapper.Initialize` | hero stat tweaks, carry weight, crime stealth | Good stat-init point; avoid duplicate tweaks |
| `CharacterStats.CharacterStatsWrapper.Initialize` | stamina/combat stat tweaks | Add non-saved tweaks rather than template mutation when appropriate |
| `ItemSet.ApplyStats` | background/stat integration | Mono/IL2CPP route differences exist |
| `ItemStat.ModifiedValue` getter | equipped weapon cost effects | Broad hot getter; verify stat/owner/equipped state |
| `CombatEnemyBehaviourBase.get_CooldownDuration` | creature-specific attack cadence tuning | Creature/profile-specific, not universal AI rule |
| `MeleeAttackBehaviour.get_RequiresCombatSlot` | creature attack-slot behavior | Can alter combat concurrency/AI balance |
| `NpcAIDistancesUtils.LoseTargetDelayByDistanceToLastIdlePoint` | target-loss tuning | AI state/lifetime sensitive |
| `NpcElement.DeathNonCriticalFunctions` | death observation | Death/corpse lifecycle continues beyond hook |
| `ProficiencyEventListener.HeroParriedDamageCallback` | parry diagnostics | Observation point; stat owner may be safer for tuning |

## Movement, rest, exposure, world transitions

| Type / method | Seen for | Caution |
|---|---|---|
| `RestPopupUI.SkipWeatherTime` | rest/time/weather diagnostics | UI entry into broader world-time lifecycle |
| `Hero.WalkThroughPortal` | portal exposure diagnostics | Not the actual full scene-travel owner |
| `HumanoidMovementBase.Jump` | scoped jump multiplier | Post-action state manipulation; owner checks needed |
| `HumanoidMovementBase.MakeMovementSound` | scoped movement-sound suppression | AI noise/audio ownership also exists |
| movement methods such as slide/jump/heavy-hold | stamina-cost correction experiments | Action-specific, verify native cost already applied |

## Progression and skills

| Type / method | Seen for | Caution |
|---|---|---|
| `ProficiencyEventListener.XPGainEvent` | XP diagnostics | Observation does not define progression ownership |
| `ProficiencyStats.TryAddXP` | cap/XP changes | Persistent progression impact |
| `ProficiencyStatsWrapper.Initialize` | cap initialization | Save/progression semantics |
| `HeroMultStatsWrapper.Initialize` | progression multiplier tweaks | Broad player progression |
| `Talent.AcquireNextTemporaryLevel` | spend/progression diagnostics | Persistent/temporary-level semantics |
| `Talent.ApplyTemporaryLevels` | progression commit diagnostics | Mutation/persistence boundary |

## Story, dialogue, choices, new game

| Type / method | Seen for | Caution |
|---|---|---|
| `Story.OfferChoice` | choice offer observation | Offer ≠ selected/handled consequence |
| `VChoice.Select(Choice)` | player choice selection | Need downstream Story continuation/state proof |
| `Choice.HoverInfos` getter (Mono) | choice hover presentation | Presentation only |
| `Choice(ChoiceConfig, Story)` constructor (IL2CPP route) | choice info setup | Branch-specific adaptation |
| `TitleScreenUtils.StartNewGame` | origin/background setup | New-game persistent state implications |
| `NewGameLoading.OnComplete` | post-new-game action | Not generic save-load completion |

## UI, HUD, map

| Type / method | Seen for | Caution |
|---|---|---|
| `VFireplaceUI.OnInitialize` | bonfire/campfire service/skill entries | Full input/focus/close path still required |
| `MapUI.ToggleFogOfWar` | map fog behavior | UI/map-specific |
| `FogOfWar.CreateMaskTexture` | fog mask generation | Renderer/map implementation risk |
| `VHeroHUD.UpdateCanvasGroups` | HUD visibility | Visual layer only |
| `VCSelectedQuickSlot.UpdateIcon` | selected quickslot visibility | Presentation only |
| hero storage UI initialization targets | stash summary/entry integration | Native UI owner must remain valid |

## Audio and sensory behavior

| Type / method | Seen for | Caution |
|---|---|---|
| `VHeroFootsteps.FootStep` | footstep suppression/replacement | Audio + stealth noise are separate systems |
| `ThieveryNoise.MakeNoiseByHero` | sneaking noise suppression | AI reaction route separate from FMOD audio |
| native character-hand audio targets | weapon/item audio replacement | Exact target varies by hand/item implementation |
| `VCCharacterMagicVFX.CastingBegun` | spell VFX overlay | VFX family heuristic ≠ native spell identity |

## Rendering / world presentation

| Type / method | Seen for | Caution |
|---|---|---|
| `MedusaBrgRenderer.SetRenderers` | world texture/material replacement experiments | Medusa is manager/data-oriented; global renderer patch is high risk |
| character creator initialization/gender paths | body/tattoo/render inventory research | Character creation/presentation lifecycle-specific |
| native weapon/equip load targets in Tainted Weapons | Drake prototype integration | Requires registered-weapon-only scoping and resource lifetime |

## Save and lifecycle

The smaller [Hook Catalogue](HOOK_CATALOGUE.md) contains the better-researched save hooks, including:

- concrete cloud-service `EndSave(string)`;
- `LoadSave.Save`/guards;
- candidate `SaveInProgressHandle.MarkSucceeded`;
- load-stage and post-scene-story candidates.

Do not infer a custom serializer from a save hook.

## How to use this atlas

For any target above:

1. locate the original source file;
2. inspect exact patch signature and conditions;
3. find the native owner/callers;
4. identify what state is guaranteed at the hook;
5. look for runtime evidence;
6. inspect known failures;
7. check Mono/IL2CPP differences;
8. choose a narrower native event/API if available.

## Golden warning

A source-located Harmony target is **evidence that somebody found a place to intervene**, not evidence that the target is stable, complete, safe, or the correct abstraction for your new mod.
