# Tainted Gems Changelog

## 0.1.10

- Added a socketed weapon stamina-cost effect lane for equipped weapons.
- Changed the `Discount Grease` and `Final Wink` paired variables to reduce or increase equipped weapon stamina costs instead of shop-buy prices.
- Applied weapon-cost effects through the equipped item `ItemStat.ModifiedValue` route for melee, bow, push, block, and parry stamina-cost stats.

## 0.1.9

- Extended the custom icon route to direct `SpriteReference.SetSprite` calls used by the relic attachment menu.
- Kept native icon references on the original FoA loader and only redirected Tainted Gems `mod://kane.tgfoa.tainted-gems/icon/...` addresses.

## 0.1.8

- Replaced the repeated modulo effect loop with a 42-entry base-gem effect table.
- Added 21 named effect variables, one per two base gems, with a positive adaptive skill side and a negative maladaptive perk side.
- Kept active mechanics limited to the researched safe runtime channels: silent sneaking, jump multiplier, shop-buy multiplier, and hero-sell multiplier.
- Added negative jump multipliers through the existing jump multiplier hook so jump variables have both positive and negative sides.

## 0.1.7

- Changed active skill/perk Tainted Gems so each gem has one visible skill/perk and one non-neutral effect lane.
- Removed the repeated full silent-sneak, jump, and trade bundle from every active gem description.
- Kept the `0.1.6` shop split: 42 skill variants, 42 perk variants, 42 inert trinkets, one generic valuable gem, and one generic low-value junk gem.
- Built release package `TaintedGems-0.1.7-embedded.zip`.

## 0.1.6

- Split shop stock into the full 128-entry Tainted Gems store set: 42 skill variants, 42 perk variants, 42 inert trinket variants, one generic valuable gem, and one generic low-value junk gem.
- Kept active socketed gameplay on the skill/perk variants only, with trinket, generic valuable, and generic junk entries remaining inert sellable/collectible items.
- Changed automatic world placement wording and runtime route to use the embedded generic valuable gem display prefabs without requiring player input.
- Kept previous two-entry and 42-entry shop config values working by treating them as stale defaults and using the full 128-entry batch at runtime.
- Built release package `TaintedGems-0.1.6-embedded.zip`.

## 0.1.5

- Added unique adaptive/maladaptive skill and perk descriptions for all 42 Tainted Gem clone identities.
- Tuned cloned gem `basePrice`, `buyPrice`, and price-level values for better shop pricing.
- Added active socketed-gem gameplay hooks for silent sneaking, higher jump launch, and adaptive/maladaptive trade multipliers.
- Kept the existing embedded AssetBundle, custom icon, automatic world-placement, and shop stock routes.
- Built release package `TaintedGems-0.1.5-embedded.zip`.

## 0.1.4

- Added embedded custom UI sprites for all 42 Tainted Gem clone identities.
- Rendered the UI sprites from single imported gem renderers and source textures during the Unity bundle build.
- Added a single-component icon capture gate so item icons do not fall back to full multi-gem prefab sheets.
- Filtered automatic world-placement prefabs down to single-gem renderers and reduced release placement scale to `0.35`.
- Added a mod-scoped `ShareableSpriteReference` icon route for `mod://kane.tgfoa.tainted-gems/icon/...`.
- Built release package `TaintedGems-0.1.4-embedded.zip`.

## 0.1.3

- Changed world placement from manual hotkey proof behavior to automatic hero-ground placement.
- Set `WorldPlacement.AllowHotkeyPlacement=false` and `WorldPlacement.PlacementHotkey=None` as release defaults.
- Kept embedded AssetBundle loading and the 42-entry Tainted Gem shop batch.
- Documented that shop/inventory icons remain inherited native gem icons in this release.
- Built release package `TaintedGems-0.1.3-embedded.zip`.

## 0.1.2

- Embedded `tainted_gems.bundle` into `TaintedGems.dll`.
- Added embedded-first bundle loading with plugin-relative bundle fallback.
- Added `Bundle.PreferEmbedded`, default `true`.
- Kept the 42-entry Tainted Gem shop batch from `0.1.1`.
- Built release package `TaintedGems-0.1.2-embedded.zip`.

## 0.1.1

- Expanded shop stock from the two proven Tainted Gem entries to 42 named native weapon gem clone entries.
- Updated live shop config to `TemplatesPerOpen = 42`.

## 0.1.0

- Added initial Tainted Gems plugin.
- Added AssetBundle world placement route.
- Added first two shop entries: `Tainted Garnet Shard` and `Tainted Crimson Cluster`.
