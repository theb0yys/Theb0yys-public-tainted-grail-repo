# Validation Plan

## Build

Run:

```powershell
dotnet build .\mods\dragon-knight-companion\src\DragonKnightCompanion.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"
```

## Runtime Checks

- Start a save with BepInEx enabled.
- Install this companion mod beside the Dragon Knight boss mod, or place `dragonknight_visuals` beside `DragonKnightCompanion.dll`.
- Press the configured summon hotkey.
- Press the configured panel hotkey and confirm the command screen opens and closes.
- Use the command screen buttons to summon/swap, recall, toggle follow/defend, switch visual variant, save config, and dismiss.
- Open FoA Mod Manager and confirm `Dragon Knight Companion` appears with `General`, `Screen`, `Controls`, `Summon`, `Follow`, `Defend`, `Visual`, and `Advanced` settings categories.
- Confirm the actor spawns near the hero and uses the Dragon Knight visual.
- Confirm recall moves the actor near the hero.
- Confirm defend prompt does not fire unless the hero has live attackers.
- Confirm dismiss removes the runtime actor.
- Confirm a save/load cycle does not persist the summoned companion.
