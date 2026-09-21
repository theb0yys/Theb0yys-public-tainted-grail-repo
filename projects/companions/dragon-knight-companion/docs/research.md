# Research

## Dragon Knight Boss Asset Source

- `mods/dragon-knight/src/Plugin.cs`
  - `BundleFileName = "dragonknight_visuals"`
  - Iron visual prefab: `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Iron_Weapon.prefab`
  - Fire visual prefab: `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire_Weapon.prefab`
  - Loads the bundle with `AssetBundle.LoadFromFile`.

- `mods/dragon-knight/src/DK4/DragonKnightDk4ActorObserver.cs`
  - Resolves the Dragon Knight `LocationTemplate` by GUID.
  - Validates the `RepetitiveNpcAttachment` and `NpcTemplate` GUID before spawn.
  - Prepares the template with an `ARAssetReference` bootstrap visual.

- `mods/dragon-knight/src/DK4/DragonKnightDk4DiagnosticHost.cs`
  - Attaches the Dragon Knight visual as a render overlay under the NPC controller.
  - Disables overlay colliders.
  - Hides native Unity and Kandra renderers while keeping overlay renderers enabled.

## Companion Mechanics Source

- `mods/avalon-human-companions/src/Plugin.cs`
  - Uses `LocationTemplate.SpawnLocation` near `Hero.Current`.
  - Marks spawned locations `MarkedNotSaved = true`.
  - Rejects missing `NpcElement` and unique NPCs.
  - Applies `OverrideFaction(hero.GetFactionTemplateForSummon(), FactionOverrideContext.Summon)`.
  - Adds `new NpcHeroPetAlly(hero)`.
  - Recalls with `Location.MoveAndRotateTo(..., teleport: true)`.
  - Defend prompts use `NpcHeroPetAlly.EnterCombat()` when the hero already has attackers.
  - Dismiss discards the runtime location after marking it not saved.

## FoA Mod Manager Settings Source

- `mods/foa-mod-manager/docs/mod-author-guide.md`
  - The manager discovers loaded BepInEx v5 Mono plugins and normal `Config.Bind` entries.
  - Optional setting metadata tags provide cleaner labels, categories, ordering, and hidden entries.
  - Custom IMGUI screens should use `FoAModManagerApi.SetCustomUiScope` while open.

- `mods/foa-mod-manager/src/Plugin.cs`
  - Reads setting metadata tags by reflection from `ConfigDescription.Tags`.
  - Recognizes `DisplaySection`, `DisplayName`, `ChoiceLabels`, `SectionOrder`, `Order`, and `Hidden`.

- Existing feature bridge pattern:
  - `mods/avalon-human-companions/src/FoAModManagerBridge.cs`
  - `mods/origins-of-avalon/src/FoAModManagerBridge.cs`
  - `mods/better-bonfire-menu/src/FoAModManagerBridge.cs`
