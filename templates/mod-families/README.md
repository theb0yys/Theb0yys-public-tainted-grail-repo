# Named Mod-Family Templates

These starter projects are based on patterns used by the repository's existing mod families.

Each template contains:

- `shared/Feature.cs` with portable starter logic for that kind of mod;
- a Mono/BepInEx 5 host;
- an IL2CPP/BepInEx 6 host;
- Tainted Framework integration for identifying the active runtime.

The shared code deliberately avoids FoA, Unity, and Harmony types. Add game-specific references and hooks in the runtime host once you know the exact FoA integration your feature needs.

Available templates:

- [Avalon AI FoA Host](avalon-ai-foa-host/)
- [Avalon Cheat Panel](avalon-cheat-panel/)
- [Avalon Stash](avalon-stash/)
- [CarryWeightTweaks](carry-weight-tweaks/)
- [Dungeon Exit Marker](dungeon-exit-marker/)
- [Easy Avalon](easy-avalon/)
- [FoA Mod Manager](foa-mod-manager/)
- [Hold to Steal](hold-to-steal/)
- [Immersive Backgrounds](immersive-backgrounds/)
- [Immersive Footsteps](immersive-footsteps/)
- [Immersive HUD](immersive-hud/)
- [Immersive Progression](immersive-progression/)
- [Jump Higher](jump-higher/)
- [Lockpicking Reforged](lockpicking-reforged/)
- [Magic Tweaks](magic-tweaks/)
- [Multi-Pin Map Notes](multi-pin-map-notes/)
- [No Fall Damage](no-fall-damage/)
- [Origins of Avalon](origins-of-avalon/)
- [Rich Merchant](rich-merchant/)
- [StaminaControl](stamina-control/)
- [Tainted Combat](tainted-combat/)
- [Tainted Interface](tainted-interface/)
- [Tainted Performance](tainted-performance/)
- [Wyrd Hunt](wyrd-hunt/)
- [Avalon Companions](avalon-companions/)
- [Tainted Music](tainted-music/)
- [Tainted Diagnostic Tool](tainted-diagnostic-tool/)
- [Better Bonfire Menu](better-bonfire-menu/)
- [Merchant Stock Tweaks](merchant-stock-tweaks/)
- [Tainted Core](tainted-core/)
- [Avalon Human Companions](avalon-human-companions/)

These are starter projects, not copies of released mods and not proof that every feature works on every game build.
