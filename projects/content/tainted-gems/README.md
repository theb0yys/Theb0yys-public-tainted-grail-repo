# Tainted Gems

BepInEx mod for importing the gem asset pack from the known external asset storage route, automatically placing its cooked prefabs in the world, and inserting gem stock into opened Avalon shops through the proven pre-item-list shop route.

This is a scoped runtime mod. It does not edit scenes, save files, loot tables, containers, recipes, or vanilla templates on disk.

## Embedded AssetBundle

Release `0.1.10` embeds `tainted_gems.bundle` inside `TaintedGems.dll`. The normal deployed layout is:

```text
BepInEx/plugins/TaintedGems/
  TaintedGems.dll
```

`Bundle.PreferEmbedded` defaults to `true`. `Bundle.RelativePath` remains as an optional fallback for local debugging.

Default prefab names:

- `TaintedGems_DiamondShapeSet1`
- `TaintedGems_DiamondShapeSet2`

The embedded bundle also contains 42 custom UI sprites named with the `TaintedGemsIcon_` prefix. These sprites are rendered from the imported gem FBX prefabs and their source textures during the Unity bundle build.

Default world placement:

- `WorldPlacement.AutoPlaceOnWorldReady = true`: place both generic valuable gem display prefabs near the hero as soon as a playable world ground hit is available.
- `WorldPlacement.AllowHotkeyPlacement = false`: no user input is required for release placement.
- `WorldPlacement.PlacementHotkey = None`: optional debug placement remains unbound by default.
- `WorldPlacement.SpawnScaleMultiplier = 0.35`: use the filtered gem-renderer prefabs as a smaller world prop instead of the full source sheet scale.

World placement is visual-only generic valuable gem display. It does not create pickable world items.

## Shop Stock

The shop patch follows the validated food/drink route:

- register runtime clone templates after `TemplatesLoader.FinishedLoading`
- wait for `Shop.OpenShop()` to decompress stock
- add items from a `ShopUI.OnFullyInitialized()` prefix before the shop item-list snapshot is built
- create with `World.Add(new Item(template, quantity))`
- insert with `RestockableStock.AddItem(..., allowStacking: true)`

Default stock entries are the 42 named, non-fake native weapon gem rows from the runtime dump, cloned into stable `Tainted` item identities. Each clone receives a `mod://kane.tgfoa.tainted-gems/icon/...` icon reference resolved from the embedded bundle by the custom UI patch in shop/inventory UI and FoA's relic attachment menu. The first proven entries remain `Tainted Garnet Shard` and `Tainted Crimson Cluster`.

`ShopStock.TargetShopGuid` is blank by default, so any opened shop with decompressed restockable stock can receive the full store set once per runtime session. Set it to a specific shop template GUID to narrow placement. Version `0.1.6` treats previous two-entry and 42-entry default configs as stale values and uses the full 128-entry store batch without editing the config file.

Version `0.1.6` store stock contains:

- 42 skill variants using the original `7a9e...` clone GUIDs
- 42 perk variants using new `7a9f...` clone GUIDs
- 42 inert trinket variants using new `7aa0...` clone GUIDs
- 1 generic valuable gem
- 1 generic low-value junk gem

## Gameplay Perks

Version `0.1.10` gives the 42 base gems an explicit 42-entry paired effect table. Every two base gems share one named effect variable, for 21 variables total, and each active gem still has one visible skill/perk plus one active effect lane. The active gameplay perks apply only when a skill or perk Tainted Gem clone is attached to equipped gear:

- silent-sneak gems mute footstep audio and crouch movement noise while sneaking
- jump gems can increase or reduce jump launch through the existing jump multiplier hook
- adaptive trade gems improve either shop-buy or hero-sell prices
- maladaptive trade gems worsen either shop-buy or hero-sell prices
- weapon-cost gems reduce or increase equipped weapon stamina costs, including melee attack, heavy-hold, bow draw/hold, push, block, and parry stamina-cost item stats

Skill variants are the positive/adaptive side of the pair. Perk variants are the negative/maladaptive side of the same pair variable. Trinket, generic valuable, and generic junk entries are inert sellable/collectible items. Each active skill/perk item contributes only its one assigned effect; it no longer lists or applies the full silent-sneak, jump, and trade bundle on every gem. The perks are runtime Harmony hooks keyed by attached skill/perk Tainted Gem template GUIDs. They do not mutate scenes, saves, native templates on disk, loot tables, containers, recipes, or runtime-authored `SkillGraph` assets.

## Build

Build the plugin:

```powershell
dotnet build mods/tainted-gems/src/TaintedGems.csproj -c Release -p:FoAGameRoot="<local-path>"
```

Build the AssetBundle through the same wrapper shape as the existing asset route:

```powershell
mods/tainted-gems/tools/Build-TaintedGemsBundle.ps1
```

The external raw gem assets are not tracked in this repo. The cooked bundle is produced by the Unity wrapper under `mods/tainted-gems/src/bin/Release/netstandard2.1/assets/tainted_gems.bundle` and embedded into the DLL at build time. No `mods/tainted-gems/assets` folder is required.
