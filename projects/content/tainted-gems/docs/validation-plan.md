# Tainted Gems Validation Plan

## Static validation

1. Confirm protected-file classes are untouched.
2. Build `mods/tainted-gems/src/TaintedGems.csproj` against the local FoA install.
3. Confirm no raw gem source files were copied into the repo.
4. Confirm plugin and assembly versions match the release version after behavior changes.
5. Confirm all 128 Tainted Gem store descriptors resolve to stable custom GUIDs and custom icon addresses.
6. Confirm only skill and perk descriptors are included in the attached-gem gameplay lookup.
7. Confirm each skill/perk descriptor has exactly one non-neutral gameplay lane.
8. Confirm the 42 base gem rows are grouped into 21 named variables, with each variable used by exactly two base gems.
9. Confirm each active skill variant is adaptive/positive and each matching perk variant is maladaptive/negative.
10. Confirm weapon-cost descriptors map to only the equipped item stamina-cost lane and do not also apply shop-buy, hero-sell, jump, or silent-sneak effects.

## AssetBundle validation

1. Run `TaintedGemsBundleBuilder.Build` in Unity batchmode with `-gemSourceRoot` pointing to the external gem folder.
2. Confirm the manifest marker is `TAINTED_GEMS_BUNDLE_PASS`.
3. Confirm `uiIconCount=42` and `uiIconSource=rendered-imported-gem-prefabs` in the manifest.
4. Confirm all `TaintedGemsIcon_*.png` files exist in the Unity project, have nonzero alpha pixels, have one significant connected component each, and are not shader-error magenta.
5. Confirm `tainted_gems.bundle` exists in the bundle output folder.
6. Build `TaintedGems.dll` and confirm the generated bundle was embedded from `mods/tainted-gems/src/bin/Release/netstandard2.1/assets/tainted_gems.bundle`.
7. Confirm no `mods/tainted-gems/assets` folder is required for release.

## Runtime validation

1. Launch FoA with BepInEx and `TaintedGems.dll`.
2. Confirm the plugin log reports `source=embedded:TaintedGems.Assets.tainted_gems.bundle` and both prefab names.
3. Confirm both generic valuable gem display prefabs auto-place near the hero without pressing a key and the log reports `placement=auto-hero-ground-generic-valuable-gem`.
4. Confirm `WorldPlacement.AllowHotkeyPlacement=false`, `WorldPlacement.PlacementHotkey=None`, and `WorldPlacement.SpawnScaleMultiplier=0.35` in the default release config path.
5. Open an Avalon shop and confirm log lines show `TaintedGemsShopStock added` before the shop item-list setup route.
6. Confirm the log reports `Tainted Gems custom icon patch armed`.
7. Confirm the log reports nonzero `shareableSpriteReferenceOverloads` and `spriteReferenceSetSpriteOverloads` in the custom icon patch marker.
8. Confirm the shop UI shows the inserted 128-entry Tainted Gem store batch with custom icons, subject to any runtime clone validation skips logged by the plugin.
9. Confirm the relic attachment menu shows the same custom Tainted Gem icons for attached and previewed relic slots, not white placeholder squares.
10. Confirm the shop batch includes 42 skill variants, 42 perk variants, 42 inert trinket variants, one generic valuable gem, and one generic low-value junk gem.
11. Confirm skill/perk item cards show unique `Adaptive Skill - ...`, `Maladaptive Skill - ...`, `Adaptive Perk - ...`, or `Maladaptive Perk - ...` text with one effect sentence only.
12. Confirm trinket, generic valuable, and generic junk item cards say they are inert and do not claim socketed gameplay.
13. Confirm Tainted Gem buy prices are rebalanced from the clone price fields.
14. Socket a silent-sneak Tainted Gem into equipped gear, crouch, and confirm footstep sound plus crouch movement noise are suppressed.
15. Socket a jump-launch Tainted Gem and confirm launch height is higher than the same character without an active Tainted Gem.
16. Socket adaptive and maladaptive trade Tainted Gems and confirm only the assigned shop-buy or hero-sell price lane adjusts in the expected direction after native price calculation.
17. Socket adaptive and maladaptive weapon-cost Tainted Gems into equipped weapons and confirm weapon stamina cost tooltip values plus light/heavy attack, heavy-hold, bow draw/hold, push, block, and parry stamina costs change in the expected direction.
18. Confirm the same weapon-cost Tainted Gems do not alter shop-buy or hero-sell prices unless a separate trade gem is also socketed.
19. Confirm trinket, generic valuable, and generic junk entries do not activate attached-gem gameplay effects.
20. Confirm no scene files, saves, loot tables, containers, recipes, native templates on disk, or runtime-authored `SkillGraph` assets were mutated.
