# Tainted Gems Research

## User instruction

Current task instruction authorizes a new mod called `tainted gems`, using the known asset importer route and known world placement route, with the gem asset source at:

```text
<local-path>
```

Follow-up instruction on 2026-08-06 requires world placement to happen naturally without user input. Version `0.1.3` therefore changes Tainted Gems from the original hotkey proof trigger to automatic runtime placement near `Hero.Current`, while keeping the same embedded AssetBundle, ground projection, renderer-bounds alignment, and no-scene-edit boundary.

Follow-up instruction on 2026-08-06 requires custom UI for the Tainted Gems release. Version `0.1.4` adds 42 custom UI sprites rendered from the imported gem FBX prefabs and source textures during the Unity bundle build, then binds runtime clone templates to those sprites through a mod-scoped `ShareableSpriteReference` prefix.

Follow-up instruction on 2026-08-07 requires every gem to have a unique skill/perk, no sound when sneaking, better prices, higher jumping, and a mix of adaptive and maladaptive effects. Version `0.1.5` keeps the existing clone/shop/world-placement route and adds runtime metadata plus Harmony patches for attached Tainted Gems.

Follow-up instruction on 2026-08-07 requires the store to keep perk, skill, and trinket versions, while world placement becomes a generic valuable gem route. Version `0.1.6` therefore splits the store catalogue into skill, perk, trinket, generic valuable, and generic low-value junk entries, while keeping socketed gameplay only on skill/perk variants.

Follow-up instruction on 2026-08-07 requires each gem to have one skill instead of reusing several combined effects across all of them. Version `0.1.7` therefore keeps the existing skill/perk/trinket store split but changes active skill/perk descriptors so each attached Tainted Gem has exactly one non-neutral runtime effect lane and one visible skill/perk name.

Follow-up instruction on 2026-08-16 requires one effect variable per two gems rather than repeating the same few labels endlessly. Version `0.1.8` therefore uses an explicit 42-entry table over the base gem ordinals: every two base gems share one named variable, each skill variant is the adaptive/positive side, and each perk variant is the maladaptive/negative side.

Follow-up instruction on 2026-08-16 requires the custom UI icons to persist in the crafting attachment menu. Version `0.1.9` therefore extends the custom icon route from `ShareableSpriteReference.RegisterAndSetup` to the direct `SpriteReference.SetSprite` path used by FoA's relic attachment screen.

Follow-up instruction on 2026-08-16 requires socketed relic effects to work on the items they are slotted into, with reduced cost called out as not working on weapons. Version `0.1.10` therefore adds a researched weapon stamina-cost lane for equipped weapon item stats while preserving the one-effect-per-skill-or-perk contract.

## Process sources read

- Root `AGENTS.md`: research-first workflow, protected-file rule, small scoped changes, final reporting format.
- `docs/INDEX.md`: game-knowledge read order.
- `docs/codex-workflow.md`: repository workflow and validation discipline.
- `docs/engineering-process.md`: research/design/implementation/validation gates and live DLL freshness rule.
- `docs/foa-modding-environment.md`: Mono/BepInEx v5/netstandard2.1 environment and no-game-DLL commit boundary.
- `docs/mod-lifecycle.md`: decompile target requirement for behavior changes.
- `docs/code-review-standard.md`: version bump requirement for release behavior changes and validation expectations.
- `codex/skills/tainted-grail-foa-modding/SKILL.md`: item/economy lane guidance and FoA build validation expectations.
- `codex/skills/tainted-grail-foa-evidence-gate/SKILL.md`: evidence gate before mutation.
- `docs/research/frameworks/tainted-content-import-process-2026-08-05.md`: current content-import framework constraints.
- `docs/research/frameworks/tainted-content-import-validation-matrix-2026-08-05.md`: content import validation matrix.
- `docs/research/game-knowledge/CONTENT-ADDITION-WORKFLOW.md`: content addition workflow.
- `mods/tainted-framework/docs/decisions/0012-native-item-registrar-ownership.md`: item registrar ownership boundary.

## Route sources read

- `mods/food-drink-asset-proof/docs/research.md`: validates the shop route, including `Shop.OpenShop`, `ShopUI.OnFullyInitialized`, runtime clone registration, and `World.Add` before `Stock.AddItem`.
- `mods/food-drink-asset-proof/docs/design.md`: documents the hotkey world placement and shop mutation proof behavior.
- `mods/food-drink-asset-proof/src/Plugin.cs`: bundle load and screen-center ground placement implementation.
- `mods/food-drink-asset-proof/src/Patches/VendorStockProbePatch.cs`: runtime clone registration and decompressed shop-stock insertion implementation.
- `mods/realistic-longsword/tools/unity/Assets/Editor/RealisticLongswordBundleBuilder.cs`: external source copy, Unity import configuration, material/prefab generation, AssetBundle build, and manifest route.
- `mods/evil-greatsword-moveset/src/Plugin.cs`: mod-scoped `ShareableSpriteReference` custom sprite route for Unity UI image binding.

## Gem evidence

The external gem source folder contains two FBX source sets and texture sets:

- `diamond_gem_shape_set_1_collectibles/source/Diamond_Shapes.fbx`
- `diamond_gem_shape_set_1_collectibles/textures/Diamond_Master_AlphaN.jpg`
- `diamond_gem_shape_set_1_collectibles/textures/Diamond_Master_Roughness.jpeg`
- `diamond_gem_shape_set_1_collectibles/textures/Diamond_Shapes_Diamond_Master_BaseColor.jpeg`
- `diamond_gem_shape_set_1_collectibles/textures/Diamond_Shapes_Diamond_Master_Normal.png`
- `diamond_gem_shape_set_2_collectibles/source/Diamond_Shape2.fbx`
- `diamond_gem_shape_set_2_collectibles/textures/Diamond_Master2_Alpha.jpeg`
- `diamond_gem_shape_set_2_collectibles/textures/Diamond_Master2_BaseColor.jpeg`
- `diamond_gem_shape_set_2_collectibles/textures/Diamond_Master2_Roughness.jpeg`
- `diamond_gem_shape_set_2_collectibles/textures/Diamond_Shape2_Diamond_Master2_Normal.png`

The runtime dump includes 42 named, non-fake native weapon gem rows with `item:gem`. These are used as native clone sources for the runtime inventory/shop identities. The two first proven entries are:

- `037d430b700551c49924a38348ec0d6f`, `ItemTemplate_Gem_Tierable_Weapon_GarnetOfFog`, display name `Garnet of Fog`
- `a8a01e2728a4d0f4189089f89e0b5f6b`, `ItemTemplate_Gem_Tierable_Weapon_CrimsonShard`, display name `Crimson Shard`

Version `0.1.1` expands the descriptor set to the remaining named `item:gem` rows from the same dump query, excluding fake/test rows and blank names.

## Gameplay effect evidence

Local `TG.Main.dll` decompilation for version `0.1.5` confirms:

- `ItemTemplate` exposes runtime price fields `basePrice`, `overrideBuyPrice`, `buyPrice`, and `priceLevelMultiplier`; cloned Tainted Gem templates can set cheaper/rebalanced shop prices without mutating native templates on disk.
- `ItemTemplate` stores description through private serialized `OptionalLocString description`; runtime clones can set a visible custom description with `new OptionalLocString((LocString)text, true)`.
- `GemAttached` stores the attached gem `ItemTemplate` and initializes its visible description from `Template.Description`; socketed Tainted Gems can be identified by the attached gem template GUID.
- `GemSlotUI.OnInitialize()` passes attached relic icons through `_gemAttached.Template.IconReference().Get()` and `GemSlotUI.SetPreviewGemItem(...)` passes preview relic icons through `PreviewGemItem.Icon.Get()`. Both call `VGemSlotUI.SetGemSprite(SpriteReference)`, and that view calls `SpriteReference.SetSprite(UnityEngine.UI.Image)` directly instead of using `ShareableSpriteReference.RegisterAndSetup`.
- `HeroItems` and the existing repository pattern `hero.HeroItems?.EquippedItem(EquipmentSlotType.X)` expose equipped items; scanning `EquipmentSlotType.All` can find attached Tainted Gems without inventory/save mutation.
- `HumanoidMovementBase.Jump()` sets `VHeroController.verticalVelocity`; a postfix can multiply the launch velocity for higher jumping while leaving the native jump trigger and stamina checks intact.
- `HumanoidMovementBase.MakeMovementSound(...)` emits crouch thief noise and AI footstep noise, `VHeroFootsteps.FootStep(int isCrouching)` plays FMOD footstep audio, and `ThieveryNoise.MakeNoiseByHero(...)` sends crouch noise to NPC reactions; prefixes can suppress sneaking sound/noise while a Tainted Gem silent-sneak perk is active.
- `DefaultPriceProvider.SellPrice(...)` handles shop sale prices to the hero and `HeroPriceProvider.SellPrice(...)` handles hero sell prices; postfixes can apply adaptive/maladaptive trade multipliers after native price calculation.
- `EquipmentSlotType.All` includes weapon and loadout slots such as `MainHand`, `OffHand`, `Quiver`, `Throwable`, `AdditionalMainHand`, and `AdditionalOffHand`, so the existing equipped-item scan can find socketed gems on weapons as well as armor and accessory slots.
- `ItemStats.ItemStatsWrapper.Initialize(...)` constructs weapon stamina-cost `ItemStat` instances with `ItemStatType.LightAttackCost`, `HeavyAttackCost`, `HeavyAttackHoldCostPerTick`, `DrawBowCostPerTick`, `HoldItemCostPerTick`, `PushStaminaCost`, `BlockStaminaCostMultiplier`, and `ParryStaminaCost`.
- `ItemStat.ModifiedValue` is the item-stat value route for hero- or shop-owned items; `Stat` also implicitly converts to `ModifiedValue`, so consumers that cast an `ItemStat` to `float` still pass through the same value route.
- `HeroAnimatorSubstateMachine.LightAttackCost`, `HeavyAttackCost`, and `PushCost` multiply equipped weapon `ItemStats` costs by `HeroStats.ItemStaminaCostMultiplier`; melee attack states subtract those values from hero stamina.
- `BowPull` reads `StatsItemStats.DrawBowCostPerTick.ModifiedValue` and `BowHold` reads hold cost before continuous stamina drain.
- `HeroBlock` reads `HoldItemCostPerTick.ModifiedValue` for block hold cost and `BlockStaminaCostMultiplier` for stamina damage while blocking.
- Existing item-template evidence includes regular valuable templates such as `ItemTemplate_Valuable_RoughDiamond` (`5373347b55faaf543964e09a965a7ea4`) with regular classification flags and jewelry pickup/drop audio. Version `0.1.6` uses that researched regular valuable prototype for generic valuable, generic low-value junk, and inert trinket clones.

No native route was found or adopted for authoring new VisualScript `SkillGraph` assets at runtime. Version `0.1.5` therefore implements "skill/perk" as per-gem clone metadata shown in the item/gem description plus active runtime gameplay hooks keyed by attached Tainted Gem template GUIDs.

## Allowed implementation

- Cook external FBX/textures into a Windows AssetBundle through a Unity editor builder.
- Load the release bundle from an embedded DLL resource, with plugin-relative `AssetBundle.LoadFromFile` retained only as fallback.
- Place the resulting prefabs automatically near the hero after a valid world ground projection is available, with screen-center placement retained only as an automatic fallback/debug route.
- Register runtime clone item templates from existing native gem templates.
- Insert the gem stock into decompressed `RestockableStock` before the shop UI item-list snapshot.
- Render custom UI sprites from imported gem prefabs and source textures during the Unity bundle build.
- Bind custom clone template icon references to the embedded bundle sprite assets through a mod-scoped icon address prefix.
- Resolve custom icon references through the direct `SpriteReference.SetSprite` path used by FoA's relic attachment menu.
- Assign each runtime skill/perk clone a unique adaptive or maladaptive skill/perk description, tuned price fields, and stable custom effect metadata.
- Apply one active attached-gem effect per skill/perk clone through the researched Harmony lanes for sneaking sound/noise, jump launch velocity, shop-buy prices, player-sale prices, or equipped weapon stamina-cost item stats.
- Assign active skill/perk effects through a 42-entry base-gem table, grouped into 21 named variables with one positive skill side and one negative perk side per two base gems.
- Apply weapon stamina-cost effects through `ItemStat.ModifiedValue` only for the current hero's equipped weapon stamina-cost stat types, without mutating native item templates, item save values, or hero stats.
- Add shop/store variants for skill, perk, and trinket versions, plus generic valuable and generic low-value junk gems.
- Keep generic valuable/junk/trinket entries inert by cloning a regular valuable prototype and excluding them from attached-gem gameplay lookup.
- Keep raw vendor assets out of the repo.
- Keep the cooked release bundle embedded in the plugin DLL for release packaging.

## Not implemented in this step

- Scene persistence.
- Save-file mutation.
- Loot table mutation.
- Container mutation.
- Recipe integration.
- New runtime-authored VisualScript `SkillGraph` assets.
- Native template, save, scene, loot table, container, or recipe mutation on disk.
