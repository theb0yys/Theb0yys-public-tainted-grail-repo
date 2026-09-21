# Tainted Gems 0.1.10 Weapon Cost Effect Validation

## Scope

The 2026-08-16 user request requires socketed relic effects to work on the items they are slotted into. The named failure was reduced cost not working on weapons.

## Evidence read

- `mods/tainted-gems/docs/design.md`: active gameplay scans `EquipmentSlotType.All` and applies one non-neutral effect lane per skill/perk gem.
- `mods/tainted-gems/docs/research.md`: prior allowed active lanes were silent sneaking, jump launch, shop-buy prices, and hero-sell prices.
- `mods/tainted-gems/src/Patches/TaintedGemsGameplayPatch.cs`: equipped item scan already reads attached `GemAttached` elements across `EquipmentSlotType.All`.
- `mods/tainted-gems/src/Patches/TaintedGemsShopPatch.cs`: the 42-entry paired effect table previously mapped `Discount Grease` and `Final Wink` to shop-buy price effects.
- Local `TG.Main.dll` decompilation:
  - `Awaken.TG.Main.Heroes.Items.EquipmentSlotType.All` includes weapon/loadout slots such as `MainHand`, `OffHand`, `Quiver`, `Throwable`, `AdditionalMainHand`, and `AdditionalOffHand`.
  - `Awaken.TG.Main.Heroes.Items.Weapons.ItemStats` creates `ItemStat` values for light attack, heavy attack, heavy-hold, bow draw, item hold, push, block stamina-cost multiplier, and parry stamina cost.
  - `Awaken.TG.Main.Heroes.Items.ItemStatType` exposes those weapon stamina-cost stat type instances.
  - `Awaken.TG.Main.Heroes.Stats.ItemStat.ModifiedValue` is the item stat value route for hero-owned items.
  - `HeroAnimatorSubstateMachine`, melee attack states, bow states, and `HeroBlock` consume those item stat values for weapon stamina spending.

## Change validated

- Added `WeaponCostDiscount` and `WeaponCostSurcharge` as active effect lanes.
- Added `ItemStaminaCostMultiplier` to descriptor metadata and active perk aggregation.
- Changed `Discount Grease` and `Final Wink` paired variables to weapon stamina-cost effects.
- Added an `ItemStat.ModifiedValue` postfix that applies the multiplier only to the current hero's equipped item stamina-cost stat types.
- Added `Awaken.Utility.dll` as a compile reference because `ItemStatType` derives from the game's RichEnum base type.

## Static checks

Command:

```powershell
$text = Get-Content -LiteralPath 'mods\tainted-gems\src\Patches\TaintedGemsShopPatch.cs' -Raw
$rows = [regex]::Matches($text, 'new\("([^"\r\n]+)", TaintedGemSkillEffect\.([^,]+), [^,]+, "[^"]+", "[^"]+", TaintedGemSkillEffect\.([^,]+),')
```

Result:

- Effect rows: `42`
- Effect variables: `21`
- Bad pair counts: none
- Weapon-cost rows: `4`
- Weapon-cost variables: `Discount Grease`, `Final Wink`

Changed tracked paths stayed under `mods/tainted-gems`.

## Build

The first build attempt failed before compile because this online worktree did not contain the ignored cooked AssetBundle at:

```text
mods/tainted-gems/src/bin/Release/netstandard2.1/assets/tainted_gems.bundle
```

The existing local cooked bundle was copied from the original checkout. Bundle SHA-256:

```text
356AE8426D3A3ED60CA324EAF1F73FDE310D8C971DC658C70B9C750FD8619103
```

Command:

```powershell
dotnet build 'mods\tainted-gems\src\TaintedGems.csproj' -c Release -p:FoAGameRoot='<local-path>
```

Result:

- Build succeeded.
- Warnings: `0`
- Errors: `0`
- Local validation DLL version: `0.1.10.0`
- Local validation DLL SHA-256 before commit: `6297211CB60536A68B66FD743E9E388D4D691640A6144E70CE34E2E21E6870B3`

## Not validated

- Runtime weapon stamina-cost spending and tooltip changes were not checked in-game in this validation step.
- Runtime relic attachment menu icon pixels were not rechecked in-game in this validation step.

## Live deployment

Initial deployment was blocked while FoA was still running:

- Process: `Fall of Avalon`
- PID: `62036`
- Path: `<local-path>`

After the process exited, the live DLL was backed up and replaced.

- Backup path: `<local-path>`
- Backup SHA-256: `99A8CECE9CED888FAFD74D43148BDA2BE39CF325CE275416CADF87D8407DC3D3`
- Deployed path: `<local-path>`
- Deployed version: `0.1.10.0`
- Deployed product version: `0.1.10+f2b33a0cac7cf4992c81189d02048206c0bc262f`
- Deployed SHA-256: `2F04464C86FBD59DBA80A228076C8620C0251524CD1C70618DAF7E66C7684CCE`

## Runtime load check

After FoA was launched, `BepInEx/LogOutput.log` confirmed:

- `[Info   :   BepInEx] Loading [Tainted Gems 0.1.10]`
- `Tainted Gems custom icon patch armed; shareableSpriteReferenceOverloads=2; spriteReferenceSetSpriteOverloads=2; spriteReferenceReleaseOverloads=1; route=rendered-imported-gem-prefab-sprites.`
- `Tainted Gems 0.1.10 loaded... gameplayPerks=forty-two-entry-paired-variable-table-one-positive-skill-one-negative-perk-per-two-base-gems-with-equipped-weapon-stamina-cost-lane`

No shop/template registration or active weapon-cost effect markers were present in the checked log slice.
