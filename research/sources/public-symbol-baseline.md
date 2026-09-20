# Public FoA Symbol Baseline

> **Snapshot:** 2026-09-20  
> **Purpose:** public-only inventory of high-value FoA symbols that a mod author would otherwise have to gather across public source repositories.  
> **Boundary:** this is provenance/evidence intake, not a claim that every publicly visible symbol is a supported API or remains unchanged across game builds.

This baseline deliberately excludes generic Unity, BepInEx and Harmony knowledge. It records FoA-specific hooks, runtime-access patterns, services, events, templates and useful members demonstrated by public FoA source.

## Source snapshots

| ID | Public source | Snapshot |
| --- | --- | --- |
| S1 | Questline — `AR-Questline/merlin-workshop` | `073bdab3e09d6adad5003339fc49b021738d71e6` |
| S2 | jonanoj — `jonanoj/FallOfAvalonMods` | `a5c361b87e287733f8962f941e8d76f919dda6f8` |
| S3 | Grailwright — `keenanselbee/grailwright` | `663c29d82044f76f0f1b24b7174888ddee81d662` |
| S4 | apodworny — `apodworny/FallOfAvalonMods` | `cf167112eb2d93c3a1714399b57647392d3d1ed5` |

S1 is Questline's official public Merlin-compatible source surface. S2, S3 and S4 are public mod/source projects. Evidence lanes remain distinct: Merlin source exposure is not automatically shipped-runtime equivalence, while working mod source demonstrates a use against the author's tested runtime/build but is not automatically cross-version proof.

## Hook and lifecycle inventory

| Symbol | Kind | Publicly demonstrated role | Lane | Source | Canonical intake |
| --- | --- | --- | --- | --- | --- |
| `Hero.OnFullyInitialized` | lifecycle hook | hero-dependent setup; event listener install; cache/status refresh | Mono | S2 — `DamageNumbers/HeroPatch.cs`, `HigherWeightLimit/HeroPatch.cs`, others | hooks + player |
| `HeroRPGStats.AfterHeroFullyInitialized` | lifecycle hook | stat-owner readiness before native stat tweaks | Mono | S2 — `HigherWeightLimit/HeroRPGStatsPatch.cs`, `HigherSummonLimit/HeroStatsPatch.cs` | hooks + player |
| `HeroItems.OnRestore` | restore hook | valid restored hero-items owner acquisition | IL2CPP native | public IL2CPP mod evidence already reviewed | hooks + runtime access |
| `HealthElement.OnDamage(Damage)` | Prefix target | mutate incoming damage before native handling | Mono | S2 — `CustomDifficulty/DamagePatch.cs` | hooks |
| `HealthElement.TakeDamage(Damage)` | hook family | completed character-damage observation/sidecars | public mod | previously reviewed public source | hooks |
| `HealthElement.OnDeathEvents` | hook family | death/terminal sidecars | public mod | previously reviewed public source | hooks |
| `Hero.ChangingStatWealth` | Postfix target | alter positive wealth change while retaining `ChangeReason` context | Mono | S2 — `CustomDifficulty/CoinMultipliers/HeroPatch.cs` | hooks + types |
| `ProficiencyStats.TryAddXP` | Prefix target / method | scale or award native proficiency XP | Mono | S2 — `CustomDifficulty/ExpMultipliers/ProficiencyStatsPatch.cs` | hooks + types |
| `NpcTemplate.GetExpReward` | Postfix target | alter NPC kill-XP result | Mono | S2 — `CustomDifficulty/ExpMultipliers/NpcTemplatePatch.cs` | hooks + types |
| `Objective.ExperiencePoints` | getter hook | alter objective XP result | Mono | S2 — `CustomDifficulty/ExpMultipliers/ObjectivePatch.cs` | hooks + types |
| `Quest.ExperiencePoints` | getter hook | alter quest XP result | Mono | S2 — `CustomDifficulty/ExpMultipliers/QuestPatch.cs` | hooks + types |
| `Difficulty.ManaUsage` | getter hook | effective difficulty mana-use multiplier | Mono | S2 — `CustomDifficulty/DifficultyPatch.cs` | hook gap closed in this pass |
| `Difficulty.StaminaUsage` | getter hook | effective difficulty stamina-use multiplier | Mono | S2 — `CustomDifficulty/DifficultyPatch.cs` | hook gap closed in this pass |
| `Difficulty.MaxEnemiesAttacking` | getter hook | effective simultaneous-attacker limit | Mono | S2 — `CustomDifficulty/DifficultyPatch.cs` | hook gap closed in this pass |
| `FallDamageUtil.DealFallDamage` | Prefix target | alter fall damage before application | Mono | S2 — `FallDamageControl/FallDamageUtilPatch.cs` | hooks + types |
| `Item.Weight` | getter hook | override effective item weight | Mono | S2 — `WeightControl/ItemWeightPatch.cs` | hooks + types |
| `ItemEquip.EquipmentType` | getter hook | override effective handedness/equipment type | Mono | S2 — `DualTwoHanded/ItemEquipPatch.cs` | hooks + types |
| `ItemsSorting.Compare(Item, Item)` | Prefix target | inject equipped/loadout priority into native item sorting | Mono | S2 — `ImprovedInventory/Inventory/Sorting/Equippable/ItemsSortingPatch.cs` | hook gap closed in this pass |
| `MapUI.AfterViewSpawned` | Postfix target | map-view-ready boundary | Mono | S2 — `FastTravelAlways/MapUIPatch.cs` | hooks |
| `VHeroHUD.AfterFullyInitialized` | Postfix target | hero HUD ready for child UI attachment/repositioning | Mono | S4 — `ProficiencyHud/VHeroHUD_Patch.cs`, `RepositionHud/VHeroHUD_Patch.cs` | hook gap closed in this pass |
| `VHeroKeys.Handle(UIEvent)` | Prefix target | hero input dispatch before native action handling | Mono | S4 — `HotkeyQuickslots/VHeroKeys_Patch.cs` | hook gap closed in this pass |
| `VCEnemyHealthBar.StartPointing(Location)` | Postfix target | enemy-target HUD acquires a Location and can resolve its NPC element | Mono | S4 — `DisplayEnemyLevels/VCEnemyHealthBar_Patch.cs` | baseline only; domain-specific UI |
| `VCEnemyBars.UpdateHP(Location)` | Postfix target | enemy-bar refresh with Location → NPC access | Mono | S4 — `ViewEnemyHealthAndStamina/VCEnemyBars_Patch.cs` | baseline only; domain-specific UI |
| `HeroStorageUI.OnFullyInitialized` | Postfix target | storage UI ready for prompt extension | Mono | S2 — `ImprovedStorage/HeroStoragePatch.cs` | hooks + types |
| `PContainerUI.OnFullyInitialized` | Postfix target | pickup/container UI ready | Mono | S2 — `ImprovedInventory/Loot/PContainerOneTimePatch.cs` | hooks + types |
| `PContainerElement.CacheVisualElements` | Postfix target | extend cached row visual tree | Mono | S2 — `ImprovedInventory/Loot/PContainerElementPatch.cs` | hooks + types |
| `PContainerElement.SetData` | Postfix target | populate extended row UI from current `Item` | Mono | S2 — `ImprovedInventory/Loot/PContainerElementPatch.cs` | hooks + types |
| `ItemTooltipFooterComponent.SetupCounters` | Postfix target | augment item-tooltip counters | Mono | S2 — `ImprovedInventory/Inventory/ItemTooltipFooterComponentSetupCountersPatch.cs` | hooks + types |
| `Prompt(...)` hold/tap constructor | Prefix target | inspect/alter native hold prompt semantics | Mono | S2 — `LessHoldTime/PromptHoldPatch.cs` | hooks + types |
| `HeroCameraShakes.MeleeSlowDownTime` | Prefix target | suppress selected melee slow-motion calls | Mono | S2 — `LessSlowMotion/LessSlowDownPatches.cs` | hooks |
| `HeroCameraShakes.RangedSlowDownTime` | Prefix target | suppress selected ranged slow-motion calls | Mono | S2 — `LessSlowMotion/LessSlowDownPatches.cs` | hooks |
| `SlowDownTime.OnInitialize` | Postfix target | observe/reduce other slowdown sources | Mono | S2 — `LessSlowMotion/LessSlowDownPatches.cs` | hooks |
| `HeroWyrdNightEdge.Execute` | Prefix target | Wyrd-night edge presentation intervention | Mono | S2 — `DisableNightGlow/HeroWyrdNightEdgePatch.cs` | hooks |
| `HeroOffHandCutOff.ApplyAnimPP` | Prefix + Postfix target | hand-cutoff presentation/late lifecycle point | Mono | S2 — `HandRegrow/HeroOffHandCutOffPatch.cs` | hook gap closed in this pass |
| `ReInput.MappingHelper.UserAssignableMapCategories` | getter Postfix | expose/filter user-assignable Rewired map categories | Mono | S2 — `CustomKeybinds/MappingHelperPatch.cs` | hook gap closed in this pass |
| `ModEntryUI.ToggleActive` | Prefix target | guard BepInEx entries from unsupported in-game toggling | Mono | S2 — `BepInExModManager/ModManagerPatch.cs` | hook gap closed in this pass |
| `ModManagerUI.InitializeModEntries` | Postfix target | inject BepInEx entries into native mod-manager UI | Mono | S2 — same file | hook gap closed in this pass |
| `ModManagerUI.ChangeModPosition` | Prefix target | guard unsupported BepInEx load-order changes | Mono | S2 — same file | hook gap closed in this pass |
| `AudioCore.DetermineMusicToPlay` | Prefix target | observe/alter combat-level input to music selection | Mono | S2 — `PersistentBgm/AudioCorePatch.cs` | hook gap closed in this pass |
| `WyrdnessAudioProvider.IsPlayerWithinZone` | Prefix target | override Wyrdness ambience zone result | Mono | S2 — `PersistentBgm/WyrdnessAudioProviderPatch.cs` | hook gap closed in this pass |
| `Crafting.get_Recipes` | native hook | inspect/extend station recipe content | IL2CPP native | previously reviewed public IL2CPP source | hooks |
| `PickItemAction.OnStart` | Prefix/action hook | narrow loose-world theft action | Mono | previously reviewed public mod source | hooks |
| `ContainerUI.TakeItemFromContainer` | action hook | narrow container item-take action | Mono | previously reviewed public mod source | hooks |
| `ContainerUI.TakeAllItems` | action hook | narrow container take-all action | Mono | previously reviewed public mod source | hooks |
| `VReadablePopupUI.OnSteal` | action hook | narrow readable theft action | Mono | previously reviewed public mod source | hooks |

## Runtime-access inventory

| Access pattern | Resolves / exposes | Lane | Source | Canonical intake |
| --- | --- | --- | --- | --- |
| `Hero.Current` | current hero | Merlin + Mono | S1 numerous files; S2 multiple mods | runtime access + player |
| `World.Any<Hero>()` | null-checkable current hero model | Mono | S2 — `FallDamageControl/FallDamageUtilPatch.cs` | runtime access |
| `World.Any<CachedHeroData>()` | active borrowed/cached hero-development context | Mono | S3 — `tools/shared/BorrowedHeroContext.cs` | not promoted: niche context |
| `World.Only<GameRealTime>()` | singleton world model | Merlin | S1 — `Assets/Code/Main/Analytics/MapAnalytics.cs` | runtime access |
| `World.Services.Get<T>()` | registered service | Merlin + Mono | S1 + S2 | runtime access + services |
| `model.TryGetElement<T>()` | owned MVC element | Merlin + Mono | S1 `SkillRole.cs`; S2 `HeroRPGStatsPatch.cs` | runtime access |
| `Location.TryGetElement<T>()` | element attached to a runtime Location | Mono | S4 — enemy HUD mods resolve `NpcElement` from `Location` | runtime-access gap closed in this pass |
| `TemplatesProvider.Get<T>(guid)` | typed native template by GUID | Mono | S2 — `WeightControl/HeroPatch.cs`, `UnlimitedOriginPotions/HeroPatch.cs` | templates + runtime access |
| `TemplateReference.TryGet<T>()` | typed template behind reference | Mono | S2 — `HigherWeightLimit/HeroPatch.cs` | templates + runtime access |
| `ActorRef.Get()` | actor from actor reference | Merlin | S1 — `Assets/Code/Main/Stories/Actors/ActorRef.cs` | runtime access |
| `Services.Get<ViewHosting>().OnMainCanvas()` | main UI canvas host | Merlin | S1 — `Assets/Code/MVC/ViewHosting.cs` consumers | runtime access + services |
| `SceneService.ActiveSceneRef` | current active-scene reference | Merlin | S1 — `MapAnalytics.cs`, `Sketch.cs` | runtime access + services |
| `SceneService.IsAdditiveScene` | additive-scene state | Merlin | S1 — `Assets/Code/Main/AI/States/Flee/StateFlee.cs` | services |
| `SceneService.ActiveSceneLoadTime` | active scene load timing | Merlin | S1 — `MapAnalytics.cs` | services |
| `World.SpawnView<T>(...)` | spawn MVC view for model/element | Mono | S2 — `BepInExModManager/ModManagerPatch.cs` | type inventory; no new category |
| `Hero.Actor` → `DefinedActor.Hero.Retrieve()` | actor identity backing hero | Merlin | S1 — `Assets/Code/Main/Heroes/Hero.cs` | type inventory |
| `hero.Inventory` | hero character-inventory surface | Merlin | S1 — `Assets/Code/Main/Heroes/Items/ItemUtils.cs` | existing inventory system |

## Services inventory

| Service | Publicly demonstrated member/use | Lane | Source | Intake |
| --- | --- | --- | --- | --- |
| `TemplatesProvider` | `Get<T>(guid)` | Mono | S2 | services/templates |
| `TweakSystem` | `Tweak(...)`, `AddTweak(...)` | Mono | S2 | services/types |
| `SceneService` | `ActiveSceneRef`, `ActiveSceneLoadTime`, `IsAdditiveScene` | Merlin | S1 | services |
| `ViewHosting` | `OnMainCanvas()` | Merlin | S1 | services |
| `ActorsRegister` | `GetActor(ActorRef)` | Merlin | S1 — `ActorRef.cs` | services |
| `NpcGrid` | `GetHearingNpcs(position, range)` | Merlin | S1 — `Assets/Code/Main/Fights/AINoises.cs` | services |
| `DroppedItemSpawner` | `DroppedItemsParent` | Merlin | S1 — `Assets/Code/Main/Heroes/Items/ItemRigidbody.cs` | services |
| `UnityUpdateProvider` | `RegisterLocationSpawner(...)` | Merlin | S1 — `Assets/Code/Main/Locations/VLocationSpawner.cs` | services |
| `GameConstants` | gem-cost fields | Merlin | S1 — `Assets/Code/Main/Locations/Gems/GemUtils.cs` | services |
| `GameplayMemory` | `Context()` | Merlin | S1 — `Assets/Code/MVC/Utils/LastOpenWorldUtils.cs` | services |
| `LargeFilesStorage` | `ForceRemoveFile(...)` | Merlin | S1 — `Sketch.cs` | services |
| `IdStorage` | `NextIdFor(model)` | Merlin | S1 — `Assets/Code/Main/Locations/LocationCreator.cs` | useful type; service page can add later |
| `CombatDirector` | static helpers resolve service before enemy-list mutation | Merlin | S1 — `Assets/Code/Main/AI/Combat/CombatDirector.cs` | existing AI/system owner |

## Events inventory

| Event surface | Public role | Lane | Source | Intake |
| --- | --- | --- | --- | --- |
| `World.EventSystem.ListenTo(...)` | global subscription | Mono | S2 — `DamageNumbers/HeroPatch.cs` | events |
| `World.EventSystem.RemoveListener(...)` | listener cleanup | Mono | S2 — same | events |
| `EventSelector.AnySource` | source-independent subscription selector | Mono | S2 | events |
| `HealthElement.Events.OnDamageDealt` | completed damage outcome | Mono | S2 | events |
| `Hero.ListenTo(...)` | model-scoped subscription | Merlin | S1 — `HeroFoV.cs` | events |
| `Hero.Events.HeroSprintingStateChanged` | sprint-state change | Merlin | S1 — `HeroFoV.cs` | events |
| `Hero.Events.HideWeapons` / `ShowWeapons` | weapon visibility events | Merlin | S1 — `VHeroKeys.cs` | events |
| `ItemsUI.Events.ItemsCollectionChanged` | inventory UI collection changed | Mono | S2 — `ImprovedStorage/HeroStoragePatch.cs` | events |
| `World.EventSystem.Trigger(source, event, payload)` | global event trigger | Merlin | S1 — `VJailUI.cs` | events |
| `Target.Trigger(...)` | model/event trigger | Merlin | S1 — `VHeroKeys.cs` | events |

## Templates and useful members

| Symbol | Role | Lane | Source | Intake |
| --- | --- | --- | --- | --- |
| `CommonReferences.Get` | common native references | Merlin + Mono | S1 + S2 | templates/types |
| `CommonReferences.Get.OverEncumbranceStatus` | over-encumbrance status reference | Mono | S2 | existing weight/status material |
| `StatusTemplate` | status definition | Mono | S2 | templates |
| `ItemTemplate` | item definition; public source uses `ChangeQuantity` | Mono | S2 | templates/types |
| `NpcTemplate` | NPC definition; `GetExpReward` | Mono | S2 | templates/types |
| `CraftingTemplate.recipes` | station recipe references | IL2CPP native | reviewed public source | templates/types |
| `TemplateReference[]` | observed container behind `CraftingTemplate.recipes` | IL2CPP native | reviewed public source | types |
| `HeroStats.EncumbranceLimit` | hero encumbrance stat | Mono | S2 | player/types |
| `HeroStats.ArmorWeightMultiplier` | armor-weight stat | Mono | S2 | player/types |
| `HeroStats.SummonLimit` | summon-limit stat | Mono | S2 | player/types |
| `Hero.AliveStats.Health` / `MaxHealth` | current/max health | Mono | S2 | player/types |
| `CharacterStatuses.FirstFrom(...)` / `RemoveStatus(...)` | query/remove typed status | Mono | S2 | existing status system |
| `Item.MoveTo(IInventory, int)` | native stack/item transfer | Mono | S2 | types |
| `HeroStorageTabUI.SelectItem(...)` | storage transfer action | Mono | S2 | types |
| `ItemsUI.FullRefresh()` | explicit UI refresh after mutation | Mono | S2 | types |
| `ReInput.MappingHelper.MapCategories` | all input map categories | Mono | S2 | input/types |
| `MapUI.AllowFastTravel()` | enable native map fast-travel affordance | Mono | S2 | world/map already canonical |

## Known public hazards

| Surface | Public observation | Intake |
| --- | --- | --- |
| broad `HeroItems.Add` patch during plugin startup | reported to interfere with hero-item initialization/save loading | hooks |
| generic IL2CPP `TryGetElement<HeroItems>` + field-offset recovery | replaced after proving unsafe/broken | hooks + runtime access |
| direct native traversal of `CraftingTemplate.recipes` | stale/freed pointers observed | hooks + runtimes/templates |
| `HeroOffHandCutOff.OnRestored`, `OnInitialized`, and even `Hero.OnFullyInitialized` for one hand-regrow operation | public mod reports these were still too early for `RemoveElementsOfType`; `ApplyAnimPP` was late enough | lifecycle/hooks |
| UI data mutation after native list/visual snapshot | requires explicit refresh or an earlier owner hook | hooks/lifecycle |
| broad `FMODManager.PlayOneShot` interception | working public use depends on filtering to hero-footstep source | hooks |

## Intake status

The public baseline is now substantially represented by these canonical pages:

- [Hooks](../../knowledge/reference/hooks/README.md)
- [Runtime access](../../knowledge/reference/runtime-access/README.md)
- [Types and members](../../knowledge/reference/types/README.md)
- [Services](../../knowledge/reference/services/README.md)
- [Events](../../knowledge/reference/events/README.md)
- [Templates](../../knowledge/reference/templates/README.md)
- [Player / Hero system](../../knowledge/systems/gameplay/player.md)
- [Runtime lanes](../../knowledge/reference/runtimes/README.md)

Items marked as niche remain in this baseline without being promoted into general reference prose. That keeps public provenance complete without turning every observed game member into a recommended modding API.

## Promotion rule

A public symbol should move from this baseline into canonical `knowledge/` when at least one of these is true:

1. it is a broadly reusable runtime-access or lifecycle boundary;
2. it is a concrete hook repeatedly useful across mod categories;
3. it establishes an owner/service/event/template relationship a modder would otherwise need to rediscover;
4. it documents a proven hazard that changes safe implementation choices.

Public visibility alone is not sufficient.


## Configuration evidence

Public FoA mod source establishes several recurring ecosystem conventions:

- BepInEx `ConfigFile` / `ConfigEntry<T>` is the normal configuration surface in public Mono mods.
- Multiple public projects temporarily set `SaveOnConfigSet = false` while binding their full setting set, call `Save()` once, then restore automatic saving.
- Public mods use enum entries for modes, `AcceptableValueRange<T>` for bounded numeric values, named sections, and separate language/display-name entries for mod-owned UI text.
- Public Grailwright source demonstrates schema-versioned configuration, backup/reset behavior, preservation of compatible settings and bounded values.

These patterns support the [Configuration](../../knowledge/mechanics/configuration/README.md) page; they do not imply that every setting can be safely applied live.

## Performance evidence

Public FoA source exposes several high-value performance-shaping surfaces:

- `World.All<T>()` is a broad registered-model enumeration path and appears in Questline source for debug, startup/indexing, one-shot cleanup and selected gameplay queries.
- `NpcGrid.GetHearingNpcs(position, range)` provides a native spatial NPC-query route for its specific hearing use case.
- public mods demonstrate lifecycle caches instead of repeated recalculation (for example cached `ItemEquip` handedness decisions cleared on hero/stat reinitialization);
- public mods use event/hook-driven work for damage, UI, inventory and interaction changes rather than requiring broad per-frame scans;
- Questline exposes services such as `RecurringActions` and `UnityUpdateProvider` for bounded scheduled/update ownership in relevant native systems.

These are source-visible design facts, not measured performance rankings. Exact cost and causality require runtime profiling.
