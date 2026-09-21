# Research: Wolf/Bear Native Ally Marker

Date: 2026-06-15
Scope: make Avalon-owned wolf and bear one-session candidates allied to the player without adding custom behavior logic.

## Evidence read

- `mods/avalon-companions/docs/research/pet-and-summon-targets-2026-06-14.md`
- `mods/avalon-companions/docs/research/actor-control-proof-design-2026-06-14.md`
- `mods/avalon-companions/docs/research/wolf-bear-diagnostic-review-2026-06-14.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- Local decompilation with `ilspycmd` 10.1.0.8386 against `<local-path>`

## Decompiled targets

- `Awaken.TG.Main.Locations.Pets.Variants.NpcAllyPetVariant`
  - `OnSpawned()` gets the spawned `NpcElement`, calls `OverrideFaction(PetOwner.GetFactionTemplateForSummon(), FactionOverrideContext.Summon)`, and adds `new NpcHeroPetAlly(PetOwner)`.
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroPetAlly`
  - Public constructor: `NpcHeroPetAlly(Hero owner)`.
  - Derives from `NpcHeroSummon`.
  - `DestroyOnRest=false`.
  - `Type=CharacterLimitedLocationType.None`.
  - `LimitForCharacter(...)` returns `1`.
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon`
  - Derives from `NpcAlly`.
  - Marks the NPC as a hero summon, removes search/pickpocket actions, handles portal/fast-travel/long-teleport repositioning, and prevents friendly fire from the hero when the player setting allows it.
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly`
  - Stores the ally reference, follows/patrols near the ally, can enter combat with the ally's attackers, and resets its ally faction override on discard.
- `Awaken.TG.Main.AI.Utils.SummonUtils`
  - `InitializeSummon(...)` applies the owner summon faction and adds `NpcHeroSummon` for hero-owned summons.
- `Awaken.TG.Main.Fights.Factions.FactionOverrideContext`
  - Contains `Summon` and `Ally` contexts.

## Decision

Avalon Companions may apply the same native marker path used by `NpcAllyPetVariant` to plugin-owned wolf and bear one-session creature candidates:

1. Spawn only after explicit player command.
2. Mark the spawned `Location` not saved.
3. Require the spawned location to expose `NpcElement`.
4. Apply `NpcElement.OverrideFaction(Hero.Current.GetFactionTemplateForSummon(), FactionOverrideContext.Summon)`.
5. Add `new NpcHeroPetAlly(Hero.Current)` to that `NpcElement`.
6. Track the location only in memory for current-session recall, catch-up, and dismiss.

If the spawned location has no `NpcElement` or the native ally marker fails, the spawned location must be discarded immediately. Do not leave an unallied wolf or bear in the world.

## Boundary

This approves only native player ally ownership for the two reviewed one-session creature candidates:

- `Spec_AnimalWolf` / `9086dee514edc644b9b55890d885db3f`
- `Spec_AnimalBear` / `c45508309b84907429f83d1361918fc2`

This does not approve:

- custom defend commands,
- custom combat target selection,
- custom follow/pathing behavior beyond existing catch-up recall,
- global faction or relation changes,
- persistence or save/load support,
- wild creature conversion,
- humanoid companions,
- unnamed template expansion.

2026-06-15 defend updates: `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md` supersedes only the native attack-response and explicit panel `Defend` mode for these same plugin-owned one-session candidates after the ally marker succeeds. Custom combat target selection, attack commands, defend hotkeys, persistence, save/load support, wild creature conversion, humanoid companions, and template expansion remain blocked.

## Validation still needed

- Throwaway-save smoke test: summon wolf, confirm it is not hostile to the player, dismiss it, and check BepInEx log.
- Throwaway-save smoke test: summon bear, confirm it is not hostile to the player, dismiss it, and check BepInEx log.
- Check that the panel status reports allied summon success or a failed ally conversion.
- Check town/civilian behavior before enabling any future custom target selection, attack-assist command, or defend hotkey.
