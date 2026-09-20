# Hook Catalogue

Hooks are only one segment of a complete mechanic. A method target does not prove owner, lifecycle, downstream success, cleanup or compatibility.

## Evidence-scoped private-corpus hook candidates

| Native target | Patch | Evidence state | Compatibility risk | Public use |
| --- | --- | --- | --- | --- |
| `LockpickingInteraction.ConsumePickHP(float)` | Prefix | Source inspected | High | Narrow lockpick durability guard |
| `Shop.OpenShop` | Prefix | Source inspected; owner runtime-unverified | High | Restock normal `RestockableStock` before shop UI |
| `VCCharacterMagicVFX.CastingBegun` | Postfix | Source inspected | High | Player-owned mod VFX overlay |
| `TemplatesLoader.set_FinishedLoading(bool)` | Postfix | Source inspected across several consumers | **Critical** | Retry/readiness boundary for template registration |
| concrete `CloudService.EndSave(string)` providers | Postfix | Source inspected + decompiled target research | High | Observe slot IDs; **not** generic durable-success semantics |

## Public working hook catalogue

| Native surface | Patch | Publicly demonstrated use | Notes |
| --- | --- | --- | --- |
| `Hero.OnFullyInitialized` | Postfix | initialize mod state after hero/gameplay infrastructure is available | Public mods use this to set up event listeners, refresh hero-dependent state and clear per-save caches |
| `HeroRPGStats.AfterHeroFullyInitialized` | Postfix | apply stat tweaks after hero RPG stats are ready | Public mods resolve `Hero.Current`, `TweakSystem` and hero stats here |
| `HealthElement.OnDamage(Damage)` | Prefix | modify raw damage before downstream handling | Public difficulty mod excludes `AliveLocation` targets to avoid altering mining/woodcutting-style damage |
| `HealthElement.TakeDamage(Damage)` | Existing public hook family | observe completed character damage for UI/VFX/audio sidecars | Different lifecycle position from `OnDamage`; do not treat the two as interchangeable |
| `HealthElement.OnDeathEvents` | Existing public hook family | attach terminal character presentation/cleanup sidecars | Death boundary rather than damage boundary |
| `Hero.ChangingStatWealth` | Postfix | adjust incoming positive wealth changes while preserving change context | Public mod distinguishes trade from reward context |
| `ProficiencyStats.TryAddXP` | Prefix | scale proficiency XP before native application | `ProfStatType` and XP amount are directly available |
| `NpcTemplate.GetExpReward` | Postfix | modify kill-XP result | Return-value patch point |
| `Objective.ExperiencePoints` getter | Postfix | modify objective XP result | Return-value patch point |
| `Quest.ExperiencePoints` getter | Postfix | modify quest XP result | Return-value patch point |
| `Difficulty.ManaUsage` getter | Postfix | alter effective mana-use multiplier | Difficulty owner; return-value patch |
| `Difficulty.StaminaUsage` getter | Postfix | alter effective stamina-use multiplier | Difficulty owner; return-value patch |
| `Difficulty.MaxEnemiesAttacking` getter | Postfix | alter simultaneous-attacker limit | Difficulty owner; return-value patch |
| `FallDamageUtil.DealFallDamage` | Prefix | alter fall damage before application | Public mod also resolves the hero at this point |
| `Item.Weight` getter | Prefix | override effective item weight by item category/state | Getter suppression can fully replace the returned weight |
| `ItemEquip.EquipmentType` getter | Postfix | alter effective equipment handedness/type | Public mod caches decisions and clears them when hero/stats are reinitialized |
| `ItemsSorting.Compare(Item, Item)` | Prefix | inject equipped/loadout priority into native item sorting | Prefix may return false after supplying its own comparison result |
| `MapUI.AfterViewSpawned` | Postfix | run after map UI view creation | Public mod calls `AllowFastTravel()` here |
| `VHeroHUD.AfterFullyInitialized` | Postfix | attach/reposition mod-owned HUD children after the hero HUD is ready | Public HUD mods use this instead of plugin-load timing |
| `VHeroKeys.Handle(UIEvent)` | Prefix | intercept hero input dispatch before native action handling | Public quickslot mod handles selected `UIKeyDownAction` names and returns false only for owned actions |
| `HeroStorageUI.OnFullyInitialized` | Postfix | extend storage prompts after storage UI initialization | Public mod accesses current storage tab and prompt collection |
| `PContainerUI.OnFullyInitialized` | Postfix | alter pickup/container UI after initialization | Public mod unpatches its one-shot UI change afterward |
| `PContainerElement.CacheVisualElements` | Postfix | add cached visual elements to container rows | UI Toolkit surface |
| `PContainerElement.SetData` | Postfix | populate row UI from the current `Item` | Useful after custom row elements have been cached |
| `ItemTooltipFooterComponent.SetupCounters` | Postfix | augment item tooltip counters | Receives `IItemDescriptor` |
| `Prompt` constructor taking key/name/press-type/action/position/control-scheme/hold-time | Prefix | inspect or alter native prompt hold/tap behavior | Exact constructor signature matters |
| `ReInput.MappingHelper.UserAssignableMapCategories` getter | Postfix | expose/filter Rewired categories available to users | Public mod uses `MapCategories` and filters debug bindings |
| `HeroCameraShakes.MeleeSlowDownTime` / `RangedSlowDownTime` | Prefix | suppress selected kill-camera slow-motion calls | Public mod intentionally suppresses the original method |
| `SlowDownTime.OnInitialize` | Postfix | detect/shorten other slowdown sources | Different owner from hero-camera entry points |
| `HeroWyrdNightEdge.Execute` | Prefix | alter Wyrd-night edge presentation behavior | Presentation-specific hook |
| `HeroOffHandCutOff.ApplyAnimPP` | Prefix + Postfix | suppress cut-off visual and run hand-regrow logic at a later safe point | Public mod reports earlier restore/init boundaries were too early for its removal operation |
| `AudioCore.DetermineMusicToPlay` | Prefix | inspect/alter combat-level input to music selection | Audio owner; preserves original by default |
| `WyrdnessAudioProvider.IsPlayerWithinZone` | Prefix | override Wyrdness ambience zone result | Can fully replace return value |
| `ModEntryUI.ToggleActive` | Prefix | prevent unsupported in-game toggling of injected BepInEx entries | Integration-specific but reusable |
| `ModManagerUI.InitializeModEntries` | Postfix | add BepInEx entries to native mod-manager UI | Uses `World.SpawnView` for injected rows |
| `ModManagerUI.ChangeModPosition` | Prefix | prevent unsupported BepInEx load-order changes | Integration-specific |
| `HeroItems.OnRestore` | IL2CPP native hook | capture a valid restored `HeroItems` instance | Public native implementation replaced an unsafe generic lookup/field-offset approach |
| `Crafting.get_Recipes` | IL2CPP native hook | observe/extend station recipe content | Native pointer lifetime must be validated |
| `PickItemAction.OnStart` | Public Harmony hook | guard illegal loose-world pickup while preserving native transfer | Theft/action owner |
| `ContainerUI.TakeItemFromContainer`, `ContainerUI.TakeAllItems` | Public Harmony hooks | guard native container theft/take-all | Container action owners |
| `VReadablePopupUI.OnSteal` | Public Harmony hook | guard native readable steal action | Readable-specific action |
| `FMODManager.PlayOneShot(...)` filtered to `VHeroFootsteps` | Public Harmony hook | replace only hero footstep playback | Broad audio method requires narrow caller/source filtering |

## Public runtime/event access used alongside hooks

Not every useful interception point is a Harmony patch:

- `World.EventSystem.ListenTo(EventSelector.AnySource, HealthElement.Events.OnDamageDealt, ...)` is used publicly after `Hero.OnFullyInitialized`.
- `Hero.ListenTo(Hero.Events.HeroSprintingStateChanged, ...)` appears in Questline's public source.
- `World.EventSystem.RemoveListener(listener)` is the corresponding cleanup path shown by a public mod.

See [Events](../events/README.md).

## Publicly documented bad or hazardous targets

| Surface | Observed problem | Lesson |
| --- | --- | --- |
| broad `HeroItems.Add` patch installed during `Plugin.Awake()` / `Harmony.PatchAll` | public mod report says it could interfere with `HeroItems` initialization and prevent saves loading | a semantically relevant method can still be the wrong lifecycle intervention |
| generic `TryGetElement<HeroItems>` plus field-offset pointer recovery | public IL2CPP implementation replaced it after the approach proved broken/dangerous | prefer a lifecycle point that hands you the valid owner |
| direct traversal of `CraftingTemplate.recipes` native pointers | stale/freed entries caused an access violation in a public IL2CPP implementation | native pointer/type checks do not prove memory lifetime |
| `HeroOffHandCutOff.OnRestored`, `OnInitialized`, and `Hero.OnFullyInitialized` for one removal operation | public hand-regrow mod reports all were too early and caused an "Element wasn't fully initialized" failure | even broadly useful lifecycle hooks may be too early for a specific child-element operation |
| broad audio hooks without caller/source filtering | public working footstep replacement relies on filtering to `VHeroFootsteps` | a technically valid broad hook may still be too wide for compatibility |

## Provenance

See [Public FoA Symbol Baseline](../../../research/sources/public-symbol-baseline.md) for source repositories, snapshot SHAs and intake status.

## Rules

- Patch the narrow native owner for the behaviour being changed.
- Preserve the original path unless the mod intentionally owns that calculation/action.
- Revalidate private/reflection targets after game updates.
- A hook firing is not terminal success.
- Keep Mono, IL2CPP-native and Merlin evidence lanes explicit; do not silently treat their access mechanisms as interchangeable.
