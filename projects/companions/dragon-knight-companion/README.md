# Dragon Knight Companion

Standalone runtime companion mod that reuses the Dragon Knight boss visual AssetBundle and the one-session companion mechanics proven in the existing companion mods.

## Scope

- Uses the boss mod AssetBundle name `dragonknight_visuals`.
- Uses the boss mod Dragon Knight iron/fire prefab asset paths.
- Spawns the Dragon Knight location template as a runtime-only companion.
- Applies the native summon faction plus `NpcHeroPetAlly` ownership marker.
- Supports summon/swap, recall, defend prompt, and dismiss.
- Adds a hotkey-opened command screen for companion control.
- Exposes manager-friendly settings metadata for FoA Mod Manager.
- Keeps spawned companion locations marked not saved.

AI package integration is intentionally left out for now; native ally defend is used until custom AI logic is relevant.

## Runtime Asset Lookup

The plugin checks for the visual bundle in this order:

1. Any already-loaded `dragonknight_visuals` AssetBundle.
2. The Dragon Knight Companion plugin folder.
3. A sibling boss plugin folder named `DragonKnight`, `dragon-knight`, or `DragonKnightBoss`.

The boss mod files are not modified by this companion mod.

## Default Controls

- `PageDown`: open or close the Dragon Knight companion screen.
- `KeypadPlus`: summon or swap the companion.
- `KeypadEnter`: recall the active companion.
- `Keypad0`: toggle follow/defend.
- `KeypadMinus`: dismiss the active companion.

## FoA Mod Manager Settings

The mod exposes its BepInEx settings to FoA Mod Manager with friendly categories:

- `General`: enabled toggle.
- `Screen`: command screen hotkey.
- `Controls`: summon, recall, defend, and dismiss hotkeys.
- `Summon`: placement distance and right offset.
- `Follow`: catch-up recall controls.
- `Defend`: native defend assist.
- `Visual`: iron/fire variant and native renderer hiding.
- `Advanced`: tick rate and native template preparation.
