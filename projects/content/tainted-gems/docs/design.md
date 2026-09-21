# Tainted Gems Design

## Scope

`Tainted Gems` has two scoped behaviors:

- import the external gem pack into a cooked AssetBundle and embed that bundle into the release DLL
- add Tainted Gem item stock to Avalon shops through the proven runtime shop route
- give socketed Tainted Gems one unique adaptive/maladaptive skill or perk each through runtime clone metadata and attached-gem gameplay patches
- split the store into skill, perk, trinket, generic valuable, and generic low-value junk entries while keeping world placement as a generic valuable gem display

## Asset route

The Unity builder copies from the external source root into a Unity project under `Assets/TaintedGems`, configures the FBX and texture imports, creates textured gem materials for the active Unity render pipeline, filters the world prefabs down to single-gem renderers, renders 42 custom UI sprites from the imported gem prefabs, builds `tainted_gems.bundle`, and writes a JSON manifest.

The repo stores the builder and mod code. The cooked bundle is generated under `mods/tainted-gems/src/bin/Release/netstandard2.1/assets/tainted_gems.bundle` and embedded into `TaintedGems.dll` at build time. Raw source assets remain external, and no loose `mods/tainted-gems/assets` folder is required for release.

## World placement route

The plugin loads embedded resource `TaintedGems.Assets.tainted_gems.bundle` with `AssetBundle.LoadFromMemory` by default. `assets/tainted_gems.bundle` remains a plugin-relative fallback path for debugging. Version `0.1.3` places both configured prefabs automatically near the hero as soon as `Hero.Current`, prefab loading, and a valid ground projection are available. Version `0.1.6` treats this automatic world placement as a generic valuable gem display, not as the perk/skill/trinket item variants. The automatic route uses the hero facing direction, side-by-side offsets, downward ground projection, renderer-bounds ground alignment, and the release scale default `WorldPlacement.SpawnScaleMultiplier=0.35`. If hero placement is not available yet, the plugin retries and can fall back to the old screen-center ground raycast without requiring player input.

`WorldPlacement.AllowHotkeyPlacement` defaults to `false` and `WorldPlacement.PlacementHotkey` defaults to `None`. The old hotkey route remains an optional debug path only.

This placement is a runtime visual placement proof for generic valuable gems. It is not saved to scene files and it does not create pickable world items.

## Custom UI route

The bundle contains one sprite per Tainted Gem identity using the `TaintedGemsIcon_` prefix. The sprites are rendered in Unity from single imported gem renderers and their source textures; they are not procedural placeholders.

Custom clone templates receive `ShareableSpriteReference` addresses under `mod://kane.tgfoa.tainted-gems/icon/`. `TaintedGemsIconPatch` intercepts that prefix on `ShareableSpriteReference.RegisterAndSetup` overloads and on direct `SpriteReference.SetSprite` calls for `UnityEngine.UI.Image` and `UnityEngine.UIElements.VisualElement`, loads the matching sprite from the embedded bundle, and leaves all native icon references on the original game route. The direct `SpriteReference.SetSprite` route covers FoA's relic attachment menu, where `GemSlotUI` passes the attached or previewed gem icon through `VGemSlotUI.SetGemSprite` instead of calling `ShareableSpriteReference.RegisterAndSetup`.

## Shop route

The plugin registers custom runtime clone templates once the template loader reports finished. Each clone inherits native gameplay behavior, native drop/pickable references, and template components from its source gem, then receives a stable mod GUID, Tainted Gem display name, and custom icon reference. The embedded gem pack supplies the custom world visual prefabs and the custom shop/inventory UI sprites.

When a shop opens, the patch waits until stock is decompressed and runs from a `ShopUI.OnFullyInitialized()` prefix before item-list setup. It creates each item with `World.Add(new Item(template, quantity))` and inserts it into a decompressed `RestockableStock` with `AddItem(..., allowStacking: true)`.

The default `ShopStock.TargetShopGuid` is blank so every opened shop that exposes decompressed restockable stock can receive the Tainted Gem store batch once per runtime session. Configuration can narrow this to one shop GUID. Version `0.1.6` builds 128 store entries: 42 skill variants, 42 perk variants, 42 inert trinket variants, one generic valuable gem, and one generic low-value junk gem. Version `0.1.6` treats previous `TemplatesPerOpen=2` and `TemplatesPerOpen=42` defaults as stale config values and uses all 128 entries without editing the config file.

If a clone template cannot validate, the default config allows fallback to the native source gem template so the shop stock route still has a valid item template.

## Gameplay perk route

Version `0.1.5` keeps the native gem template clone as the item identity boundary and adds mod-owned metadata to each cloned Tainted Gem descriptor. The metadata derives a unique visible skill name, perk name, adaptive/maladaptive disposition, tuned `basePrice`/`buyPrice` values, silent-sneak flag, jump launch multiplier, shop-buy multiplier, and hero-sell multiplier for that clone GUID.

Version `0.1.7` narrows the active descriptor contract to one skill per skill/perk item. Each active descriptor maps to exactly one non-neutral runtime effect lane: silent sneaking, jump launch, shop-buy discount, hero-sell premium, shop-buy surcharge, or hero-sell penalty. Descriptions name only that one skill/perk effect instead of listing the full silent-sneak, jump, and trade bundle on every gem.

Version `0.1.8` replaces the repeated modulo lane assignment with a full 42-entry base-gem table. The table is keyed by the existing stable custom GUID ordinal, so each base gem has an explicit row. Rows are grouped in pairs: every two base gems share one named effect variable, the skill variant is adaptive/positive, and the perk variant is maladaptive/negative. The variables use only the already researched safe runtime channels: silent sneaking, jump multiplier, shop-buy multiplier, and hero-sell multiplier.

Version `0.1.10` adds an equipped weapon stamina-cost lane for cost variables that previously had no weapon stat target. The lane still follows the one-effect-per-skill-or-perk contract: a socketed gem assigned the weapon-cost effect contributes only an item stamina-cost multiplier, not a bundled trade or jump effect. The modifier is applied through `ItemStat.ModifiedValue` only for the current hero's equipped items and only for weapon stamina-cost stat types: light attack, heavy attack, heavy attack hold, bow draw, item hold, push, block stamina-cost multiplier, and parry.

The clone registration route writes the visible custom text into the cloned template `description` field with native `OptionalLocString`, so unattached and attached gem cards show the Tainted Gem skill/perk instead of only the inherited native description. It also writes the tuned price fields on the clone, not on the native source template.

Version `0.1.6` keeps active gameplay only on the store skill and perk variants. Existing `7a9e...` clone GUIDs remain the skill variants for compatibility. New `7a9f...` clone GUIDs are perk variants. New `7aa0...` clone GUIDs are inert trinket variants cloned from the regular valuable prototype, and the `7aa1...` entries are generic valuable and generic low-value junk gems. Trinket, valuable, and junk entries use the gem icons but are excluded from attached-gem gameplay lookup.

Active gameplay checks are runtime-only. `TaintedGemsGameplay` scans `Hero.Current.HeroItems.EquippedItem(...)` across `EquipmentSlotType.All`, reads attached `GemAttached` elements, and matches attached gem template GUIDs against the Tainted Gem skill/perk descriptor map. The result is cached once per Unity frame.

Harmony patches apply only when at least one attached Tainted Gem is active:

- crouched footstep FMOD and crouch movement noise are suppressed only by gems assigned the silent-sneak effect
- jump launch velocity is multiplied after the native jump method runs only by gems assigned the jump-launch effect
- player shop-buy and hero-sell prices are adjusted after native price providers calculate the base value only by gems assigned the matching trade effect
- equipped weapon stamina costs are multiplied at the item stat value route only by gems assigned the weapon-cost effect

Adaptive skill gems grant one beneficial effect. Maladaptive perk gems grant one harmful effect. Multiple attached Tainted Gems can stack through bounded aggregate multipliers, but each attached gem contributes only its one assigned effect lane. Trinket, valuable, and junk variants are inert sellable/collectible items. No scenes, saves, native templates on disk, loot tables, containers, recipes, or runtime-authored `SkillGraph` assets are mutated.
