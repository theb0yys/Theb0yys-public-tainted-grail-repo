# Human One-Session Native Ally Proof

Date: 2026-06-15

## Scope

Prove the smallest safe native ally behavior for one plugin-owned human proof actor. This is a throwaway-save proof only. It does not approve converting existing NPCs, persistent followers, recruitment, dialogue, inventory, equipment, quest behavior, or a human command panel.

## Evidence read

- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/research/human-npc-proof-step-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/safe-proof-spawn-target-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-actor-scanner-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`

## Creature companion reference

Avalon Companions makes plugin-owned creature candidates non-hostile by mirroring the native `NpcAllyPetVariant` path:

1. Spawn only a reviewed one-session candidate after an explicit command.
2. Mark the spawned `Location` as `MarkedNotSaved=true`.
3. Require a spawned `NpcElement`.
4. Apply `NpcElement.OverrideFaction(Hero.Current.GetFactionTemplateForSummon(), FactionOverrideContext.Summon)`.
5. Add `new NpcHeroPetAlly(Hero.Current)`.
6. Track the actor only in memory and discard it if setup fails.

Its defend behavior stays inside the native ally lane by calling `NpcHeroPetAlly.EnterCombat()` only when `Hero.Current.PossibleAttackers` already contains live attackers. It does not choose arbitrary targets or force hostility.

## Decision

Avalon Human Companions may add one disabled-by-default proof path that applies the same native summon-faction plus `NpcHeroPetAlly` marker to a plugin-owned, one-session human proof spawn.

The proof must:

- require `Research.ResearchModeOnly=false`,
- require `HumanAllyProof.EnableOneSessionAllyProof=true`,
- require an explicit hotkey,
- require a reviewed `Target.TemplateGuid`,
- block templates without `NpcAttachment`,
- block `NpcAttachment.IsUnique=true`,
- block spawned `NpcElement.IsUnique=true`,
- mark the spawned `Location` not saved immediately,
- discard the actor if native ally setup fails,
- allow only one ally proof per session by default,
- provide an explicit dismiss hotkey,
- log that no recruitment, dialogue, story, crime, quest, custom target selection, or persistence behavior was applied.

## Approved first target

The only approved first target remains the already reviewed safe proof spawn target:

- Template: `Spec_Enemy_Generic_Tier1_Outlaw_1H`
- GUID: `2bd34a05d1e1fb94f9770b9ee7f23be2`
- Evidence: regular, non-abstract, repetitive NPC attachment, `NpcUnique=false`, and vanilla spawner usage.

The old safe proof spawn is expected to attack because it applies no ally marker. The new proof exists only to test whether the same native ally marker that works for creature candidates can make this plugin-owned human proof actor non-hostile.

## Not approved

- Converting existing wild, hostile, civilian, named, unique, quest, story, boss, challenge, tutorial, or scene-critical NPCs.
- Persistent human companions.
- Human recruitment or roster state.
- Custom follow, wait, defend, attack, or target selection logic.
- Human command panel or native interaction action.
- Dialogue, affinity, romance, inventory, equipment, leveling, or quest integration.
- Save data, transition reconstruction, or quit/relaunch reconstruction.
- Editing vanilla templates, story graphs, serialized interaction lists, crime state, faction tables, or global relations.

## Validation needed

Use a throwaway save and abandon it after testing.

1. Reload before testing so any old hostile proof-spawn outlaw is gone.
2. Configure the reviewed target GUID.
3. Set `Research.ResearchModeOnly=false`.
4. Set `HumanAllyProof.EnableOneSessionAllyProof=true`.
5. Set `HumanAllyProof.AllyProofHotkey` to an unreserved key.
6. Press the hotkey once.
7. Confirm the log says the native summon-faction plus `NpcHeroPetAlly` marker was applied.
8. Confirm the actor does not immediately attack the player.
9. Press `HumanAllyProof.DismissAllyProofHotkey` and confirm the actor is discarded.
10. Do not continue normal play from this proof save.

If the actor still attacks after the log says the native ally marker was applied, the next step is not custom combat AI. The next step is to inspect scanner CSVs and local `TG.Main.dll` hostility/faction surfaces for humanoid-specific antagonism, possible attackers, and combat-state cleanup.
