# Named Mod-Family Templates

These 31 public templates correspond to the current cross-runtime first-party mod families being used as source/reference material.

Every family now contains:
- `shared/Feature.cs` with **actual portable starter logic** for that family rather than a mechanism-name placeholder;
- a Mono/BepInEx 5 host/project;
- an IL2CPP/BepInEx 6 host/project;
- a hard Tainted Framework dependency for the shared runtime-kind boundary.

The shared layer intentionally avoids FoA/Unity/Harmony types until the author selects an exact verified owner. Add those references in the runtime host that needs them rather than spreading loader/runtime differences throughout feature logic.

- [Avalon AI FoA Host](avalon-ai-foa-host/) — source family `avalon-ai-runtime`
- [Avalon Cheat Panel](avalon-cheat-panel/) — source family `avalon-cheat-panel`
- [Avalon Stash](avalon-stash/) — source family `Avalon Stash`
- [CarryWeightTweaks](carry-weight-tweaks/) — source family `carry-weight-tweaks`
- [Dungeon Exit Marker](dungeon-exit-marker/) — source family `dungeon-exit-helper`
- [Easy Avalon](easy-avalon/) — source family `easy-avalon`
- [FoA Mod Manager](foa-mod-manager/) — source family `foa-mod-manager`
- [Hold to Steal](hold-to-steal/) — source family `hold-to-steal`
- [Immersive Backgrounds](immersive-backgrounds/) — source family `immersive-backgrounds`
- [Immersive Footsteps](immersive-footsteps/) — source family `immersive-footsteps`
- [Immersive HUD](immersive-hud/) — source family `always-show-hud`
- [Immersive Progression](immersive-progression/) — source family `immersive-progression`
- [Jump Higher](jump-higher/) — source family `jump-higher`
- [Lockpicking Reforged](lockpicking-reforged/) — source family `lockpicking-reforged`
- [Magic Tweaks](magic-tweaks/) — source family `magic-tweaks`
- [Multi-Pin Map Notes](multi-pin-map-notes/) — source family `multi-pin-map-notes`
- [No Fall Damage](no-fall-damage/) — source family `no-fall-damage`
- [Origins of Avalon](origins-of-avalon/) — source family `origins-of-avalon`
- [Rich Merchant](rich-merchant/) — source family `rich-merchant`
- [StaminaControl](stamina-control/) — source family `stamina-action-control`
- [Tainted Combat](tainted-combat/) — source family `Tainted Combat`
- [Tainted Interface](tainted-interface/) — source family `Tainted Interface`
- [Tainted Performance](tainted-performance/) — source family `tainted-performance`
- [Wyrd Hunt](wyrd-hunt/) — source family `wyrd-hunt`
- [Avalon Companions](avalon-companions/) — source family `avalon-companions`
- [Tainted Music](tainted-music/) — source family `tainted-music`
- [Tainted Diagnostic Tool](tainted-diagnostic-tool/) — source family `Tainted-Diagnostic Tool`
- [Better Bonfire Menu](better-bonfire-menu/) — source family `better-bonfire-menu`
- [Merchant Stock Tweaks](merchant-stock-tweaks/) — source family `merchant-stock-tweaks`
- [Tainted Core](tainted-core/) — source family `avalon-core`
- [Avalon Human Companions](avalon-human-companions/) — source family `avalon-human-companions`

These are starter projects, not copies of release packages or runtime proof.
