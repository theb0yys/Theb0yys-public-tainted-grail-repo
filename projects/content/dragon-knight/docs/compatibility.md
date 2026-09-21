# Dragon Knight Compatibility

Current status: loader builds, deploys, and live-loads in the local FoA BepInEx install.

Proven methods to adapt:

- `mods/avalon-companions`: companion behavior methods; no dependency by default.
- `mods/avalon-awakened`: asset import/transport methods; no dependency by default.
- `mods/avalon-ai-runtime`: AI architecture and package methods; no dependency by default.

Known future compatibility surfaces:

- BepInEx v5 Mono plugin load order.
- FoA native Addressables or ModService catalogue discovery.
- Actor spawning and `LocationTemplate` ownership.
- Companion ally/faction behavior.
- Combat AI, target selection, death/corpse lifecycle, and rewards.
- Item, weapon, armor, inventory, equipment, crafting, vendor, loot, and localization systems.
- Save/load and missing-content behavior.
- Dragon Knight AI package compatibility.
- Any later integration with Avalon Companions, Avalon Awakened, or Avalon AI Runtime, if explicitly opened by a Dragon Knight decision.

Current compatibility claim:

- Status: Loads.
- Local path: `<local-path>`.
- Loader: BepInEx `5.4.23.3`.
- Unity runtime in log: `v6000.0.41.4645959`.
- Game version: not separately identified beyond the BepInEx title line `Fall of Avalon (17/06/2026 02:27:09)`.
- Scope: Dragon Knight loader only. No visual asset, actor, AI, follower, usable item, save, or release-package compatibility is claimed.
