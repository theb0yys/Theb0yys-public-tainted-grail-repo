# Types and Members Reference

Exact lookup for important FoA types and members.

- [Hero and Character stat surfaces](hero-character-stats.md) — exact inspected Mono stat/member inventory and base-vs-modified save semantics This is intentionally selective: include a member when knowing it saves meaningful FoA-specific source hunting or reverse engineering.

## Lifecycle and owner types

| Type / member | Publicly demonstrated relevance | Evidence boundary |
| --- | --- | --- |
| `Hero.Current` | Current hero access | Questline public source + public Mono mods |
| `Hero.OnFullyInitialized` | Hero/gameplay initialization boundary used for listeners, cache resets and hero-dependent setup | Public Mono mods |
| `HeroRPGStats.AfterHeroFullyInitialized` | Stat-owner readiness boundary | Public Mono mods |
| `HeroItems.OnRestore` | Reliable IL2CPP-native availability point for hero item owner | Public native IL2CPP mod |
| `Hero.TryGetElement<T>()` | Resolve hero-owned MVC elements such as `ArmorWeight` | Questline/public Mono source |
| `World.Any<T>()` | Null-checkable lookup for a world model such as `Hero` | Public Mono mod |
| `World.Only<T>()` | Resolve a single expected world model | Questline public source |
| `World.Services.Get<T>()` | Resolve registered services | Questline public source + public Mono mods |
| `World.All<T>()` | Broad enumeration of registered models | Preserved developer lifecycle documentation |
| `model.Element<T>()` / `model.Elements<T>()` | Access one/all related Elements | Preserved developer lifecycle documentation |
| `element.ParentModel` | Resolve Element owner | Preserved developer lifecycle documentation |
| `World.View<T>(model)` | Resolve an associated View | Preserved developer lifecycle documentation |
| `Model.MarkedNotSaved` / `IsNotSaved` | Explicit non-saved Model state; save preparation skips normal save callback for not-saved Models | Exact Mono static inspection |

## Templates, stats and progression

| Type / member | Publicly demonstrated relevance | Evidence boundary |
| --- | --- | --- |
| `TemplatesProvider.Get<T>(guid)` | Resolve a native template by GUID | Public Mono mods |
| `TemplateReference.TryGet<T>()` | Resolve a typed template from a template reference | Public Mono mod |
| `TemplatesProvider.GetAllOfType<T>()` | Enumerate loaded templates of a requested type | Exact Mono static inspection |
| `SaveWriter.WriteTemplate<T>()` | Persist a template relationship as its GUID | Exact Mono static inspection |
| `SaveReader.ReadTemplate<T>()` | Restore a saved template through `TemplatesUtil.Load<T>()` | Exact Mono static inspection |
| `TemplatesUtil.Load<T>()` | Resolve a saved GUID through `TemplatesProvider` | Exact Mono static inspection |
| `CommonReferences.Get` | Public source of common game template references | Public Mono mods / Questline source |
| `TweakSystem.Tweak(...)` / `AddTweak(...)` | Build and register native stat tweaks | Public Mono mods |
| `StatTweak.Add` / `StatTweak.Multi` | Publicly used additive/multiplicative stat tweak constructors | Public Mono mods |
| `ProficiencyStats.TryAddXP` | Native proficiency XP entry point | Public Mono mods |
| `NpcTemplate.GetExpReward` | Native NPC kill-XP result | Public Mono mod |
| `Quest.ExperiencePoints` | Quest XP property | Public Mono mod |
| `Objective.ExperiencePoints` | Objective XP property | Public Mono mod |
| `Hero.ChangingStatWealth` | Wealth-change hook carrying `HookResult<IWithStats, Stat.StatChange>` | Public Mono mod |

## Items and inventory

| Type / member | Publicly demonstrated relevance | Evidence boundary |
| --- | --- | --- |
| `HeroRecipes.LearnRecipe` | Learn existing/native recipe | Public native IL2CPP mod |
| `KnownItems` | Existing known-item state for startup reconstruction | Public Mono + IL2CPP mod behavior |
| `CraftingTemplate.recipes` | Recipe template references used to inspect station recipe content | Public IL2CPP implementation; stale native pointers observed |
| `TemplateReference[]` | Container type observed behind `CraftingTemplate.recipes` | Same IL2CPP evidence boundary |
| `Item.Weight` | Effective item weight property | Public Mono mod |
| `Item.MoveTo(IInventory, int)` | Native item transfer between inventories | Public storage mod |
| `Item.Quantity` | Quantity used for stack transfer | Public storage mod |
| `ItemEquip.EquipmentType` | Effective equipment-type/handedness property | Public Mono mod |
| `HeroStorageUI.OnFullyInitialized` | Storage UI readiness point | Public Mono mod |
| `HeroStorageTabUI.SelectItem(...)` | Native storage transfer selection path | Public storage mod |
| `ItemsUI.Events.ItemsCollectionChanged` | Event used before explicit UI refresh after batch item movement | Public storage mod |
| `ItemsUI.FullRefresh()` | Explicit inventory UI refresh after mutation | Public storage mod |

## Combat, interaction and UI

| Type / member | Publicly demonstrated relevance | Evidence boundary |
| --- | --- | --- |
| `HealthElement.OnDamage(Damage)` | Incoming-damage mutation point | Public Mono mod |
| `HealthElement.TakeDamage(Damage)` | Public working completed-damage hook family | Existing public hook evidence |
| `FallDamageUtil.DealFallDamage` | Fall-damage entry point | Public Mono mod |
| `Prompt` constructor with `KeyBindings`, name, `IButton.PressType`, action, position, control scheme and hold time | Native prompt creation surface | Public Mono mod |
| `MapUI.AfterViewSpawned` | Map view creation boundary | Public Mono mod |
| `VHeroHUD.AfterFullyInitialized` | hero-HUD readiness boundary for attaching/repositioning child UI | Public Mono mods |
| `VHeroKeys.Handle(UIEvent)` | hero input dispatch surface receiving UI events/actions | Public Mono mod |
| `Location.TryGetElement<T>()` | resolve an element attached to a runtime Location; public examples resolve `NpcElement` | Public Mono mods |
| `PContainerUI.OnFullyInitialized` | Container/pickup UI initialization | Public Mono mod |
| `PContainerElement.CacheVisualElements` | Container-row visual cache construction | Public Mono mod |
| `PContainerElement.SetData` | Container-row item binding | Public Mono mod |
| `ItemTooltipFooterComponent.SetupCounters` | Item-tooltip footer population | Public Mono mod |
| `HeroCameraShakes.MeleeSlowDownTime` / `RangedSlowDownTime` | Kill-camera slowdown entry points | Public Mono mod |
| `SlowDownTime.OnInitialize` | Generic slowdown initialization | Public Mono mod |

## Services and events

| Type / member | Publicly demonstrated relevance | Evidence boundary |
| --- | --- | --- |
| `SceneService.ActiveSceneRef` | Current active scene reference | Questline public source |
| `ActorsRegister.GetActor(ActorRef)` | Resolve actor identity/reference to actor | Questline public source |
| `World.EventSystem.ListenTo` | Subscribe to MVC/game events | Public Mono mod |
| `World.EventSystem.RemoveListener` | Remove a registered listener | Public Mono mod |
| `EventSelector.AnySource` | Listen independent of one source model | Public Mono mod |
| `HealthElement.Events.OnDamageDealt` | Damage-dealt event consumed by a public mod | Public Mono mod |
| `Hero.Events.HeroSprintingStateChanged` | Hero sprint-state event used by Questline code | Questline public source |
| `ViewHosting.OnMainCanvas()` | Main-canvas host lookup | Questline public source |

## Settings, input, audio and integration

| Type / member | Publicly demonstrated relevance | Evidence boundary |
| --- | --- | --- |
| `Difficulty.ManaUsage` | effective difficulty mana-use multiplier | Public Mono mod |
| `Difficulty.StaminaUsage` | effective difficulty stamina-use multiplier | Public Mono mod |
| `Difficulty.MaxEnemiesAttacking` | effective simultaneous-attacker limit | Public Mono mod |
| `ItemsSorting.Compare(Item, Item)` | native inventory item comparison surface | Public Mono mod |
| `ReInput.MappingHelper.UserAssignableMapCategories` | user-assignable Rewired category list | Public Mono mod |
| `ReInput.MappingHelper.MapCategories` | all available Rewired map categories used to construct filtered assignable lists | Public Mono mod |
| `AudioCore.DetermineMusicToPlay` | native music selection receives combat level | Public Mono mod |
| `WyrdnessAudioProvider.IsPlayerWithinZone` | Wyrdness ambience-zone predicate | Public Mono mod |
| `ModManagerUI.InitializeModEntries` | native mod-manager entry initialization | Public Mono integration mod |
| `ModEntryUI.ToggleActive` | native mod-entry toggle action | Public Mono integration mod |
| `World.SpawnView<T>(...)` | spawn a view for an MVC model/element; used when injecting mod-manager rows | Public Mono integration mod |
| `Hero.Actor` | Questline source resolves hero actor through `DefinedActor.Hero.Retrieve()` | Questline public Merlin source |

## Public namespaces worth knowing

Questline's public Merlin Workshop source and public Mono mods expose game-facing namespaces including:

- `Awaken.TG.Main.Heroes`
- `Awaken.TG.Main.Character`
- `Awaken.TG.Main.Heroes.Stats`
- `Awaken.TG.Main.Heroes.Items`
- `Awaken.TG.Main.Locations`
- `Awaken.TG.Main.Fights`
- `Awaken.TG.Main.Stories.Quests`
- `Awaken.TG.Main.Saving`
- `Awaken.TG.Main.AI`
- `Awaken.TG.Main.Templates`
- `Awaken.TG.MVC`
- `Awaken.TG.MVC.Events`
- `Awaken.TG.MVC.Domains`

Namespace presence is a navigation aid, not proof that every contained type is a supported mod API.

## Entry format

When a type/member is promoted into this reference, record where known:

- assembly and namespace;
- full type/member name and signature;
- static or instance ownership;
- relevant runtime/build scope;
- known callers or consumers;
- observable side effects;
- lifecycle constraints;
- evidence source and maturity.

Do not turn this area into a bulk decompilation archive. Follow the repository's public-source boundary.

Private/internal evidence-derived entries are scoped in [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
