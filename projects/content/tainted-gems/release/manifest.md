# Tainted Gems Release Manifest

## 0.1.10 Weapon Cost Effects

- Plugin ID: `kane.tgfoa.tainted-gems`
- Plugin DLL: `TaintedGems.dll`
- Release package: not built in this validation step
- Release package SHA-256: not generated
- DLL SHA-256: not finalized in this validation step
- Embedded AssetBundle resource: `TaintedGems.Assets.tainted_gems.bundle`
- Embedded AssetBundle SHA-256: `356AE8426D3A3ED60CA324EAF1F73FDE310D8C971DC658C70B9C750FD8619103`
- Gem stock count: `128`
- Custom UI sprite count: `42`
- Runtime route: embedded bundle load, filtered automatic hero-ground generic valuable gem display placement at scale `0.35`, optional unbound debug hotkey fallback, and pre-shop-item-list stock insertion.
- UI icon route: mod-scoped `ShareableSpriteReference` and direct `SpriteReference.SetSprite` addresses resolved to embedded sprites rendered from single imported gem renderers, including the relic attachment menu.
- Store route: 42 skill variants, 42 perk variants, 42 inert trinket variants, one generic valuable gem, and one generic low-value junk gem.
- Gameplay route: each socketed skill/perk Tainted Gem has one unique visible skill/perk and one non-neutral effect lane from the 42-entry paired-variable table, including the equipped weapon stamina-cost lane. Trinket, generic valuable, and generic junk entries are inert.

## Deploy

Copy `TaintedGems.dll` to:

```text
BepInEx/plugins/TaintedGems/TaintedGems.dll
```

No loose AssetBundle is required for release `0.1.10`.
