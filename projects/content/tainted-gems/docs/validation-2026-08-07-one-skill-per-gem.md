# Tainted Gems 0.1.7 One Skill Per Gem Validation

## Scope

Version `0.1.7` changes active skill/perk Tainted Gems so each descriptor has one visible skill/perk and one non-neutral runtime effect lane. The `0.1.6` store split remains unchanged: 42 skill variants, 42 perk variants, 42 inert trinket variants, one generic valuable gem, and one generic low-value junk gem.

## Static Validation

- Read `AGENTS.md`, `docs/engineering-process.md`, `docs/code-review-standard.md`, `mods/tainted-gems/docs/research.md`, `mods/tainted-gems/docs/design.md`, and `mods/tainted-gems/docs/validation-plan.md` before editing.
- Confirmed `PluginVersion`, assembly version, and file version are `0.1.7`.
- Confirmed startup logging reports `gameplayPerks=socketed-skill-and-perk-tainted-gems-one-effect-each`.
- Confirmed active item descriptions use `{SkillName}. {EffectDescription}` and no longer list the full silent-sneak, jump, and trade bundle.
- Confirmed each active descriptor maps to exactly one effect enum value: `SilentSneak`, `JumpLaunch`, `ShopDiscount`, `SellPremium`, `ShopSurcharge`, or `SellPenalty`.
- Confirmed each effect enum value controls one non-neutral runtime lane:
  - `SilentSneak`: only `SilentSneak=true`
  - `JumpLaunch`: only `JumpVelocityMultiplier>1`
  - `ShopDiscount`: only `ShopBuyPriceMultiplier<1`
  - `SellPremium`: only `HeroSellPriceMultiplier>1`
  - `ShopSurcharge`: only `ShopBuyPriceMultiplier>1`
  - `SellPenalty`: only `HeroSellPriceMultiplier<1`
- Confirmed trinket, generic valuable, and generic junk descriptors remain inert and excluded from attached-gem gameplay lookup.

## Build Validation

Command:

```powershell
dotnet build 'mods/tainted-gems/src/TaintedGems.csproj' -c Release -p:FoAGameRoot='<local-path>
```

Result:

- Passed with 0 warnings and 0 errors.
- Output DLL: `mods/tainted-gems/src/bin/Release/netstandard2.1/TaintedGems.dll`

## Package Validation

- Package: `mods/tainted-gems/release/TaintedGems-0.1.7-embedded.zip`
- Package contents: `TaintedGems.dll` only.
- Package size: `14806250` bytes.
- DLL size: `17219584` bytes.
- Embedded bundle input size: `17138090` bytes.
- Package SHA-256: `8C7AC9AF2266EEF53EE71746E77A37A70E11548E9DE6DD6EDECFDE0586AB83AA`
- DLL SHA-256: `C37A022D5A823D17267349953C189921EBAF89D39FF467C4D15E079963C494E7`
- Embedded AssetBundle SHA-256: `356AE8426D3A3ED60CA324EAF1F73FDE310D8C971DC658C70B9C750FD8619103`

## Runtime Validation Still Needed

- Launch the game with the deployed `0.1.7` DLL.
- Confirm the log shows `Tainted Gems 0.1.7` and `gameplayPerks=socketed-skill-and-perk-tainted-gems-one-effect-each`.
- Open an Avalon shop and confirm the skill/perk cards each show one skill/perk and one effect sentence only.
- Confirm the store still reports `candidateCount=128` and `perOpenLimit=128`.
- Socket representative gems from each effect lane and confirm only that lane applies.

## Live Deployment Validation

- Deployed `mods/tainted-gems/src/bin/Release/netstandard2.1/TaintedGems.dll` to `<local-path>`.
- Built DLL SHA-256: `C37A022D5A823D17267349953C189921EBAF89D39FF467C4D15E079963C494E7`
- Live deployed DLL SHA-256: `C37A022D5A823D17267349953C189921EBAF89D39FF467C4D15E079963C494E7`
- Live deployed DLL size: `17219584` bytes.
