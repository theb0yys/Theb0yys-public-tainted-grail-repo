# Research: Wolf/Bear Native Defend Assist

Date: 2026-06-15
Scope: add defend response when the hero is attacked without adding custom target selection or custom combat AI.

## Evidence read

- `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md`
- `mods/avalon-companions/docs/research/actor-control-proof-design-2026-06-14.md`
- `mods/avalon-companions/docs/research/pet-and-summon-targets-2026-06-14.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- Local decompilation with `ilspycmd` 10.1.0.8386 against `<local-path>`

## Decompiled targets

- `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly`
  - `FindTarget()` iterates `Ally.PossibleAttackers`, adds those attackers as possible combat targets, recalculates target, and can enter combat with the selected target.
  - `UnityUpdate()` calls `FindTarget()` on a timer and can enter combat when a current target exists.
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon`
  - Public `EnterCombat()` removes summon invisibility and calls the inherited native target-selection path.
  - `OnAllyEnteredCombat()` already calls `EnterCombat()`.
- `Awaken.TG.Main.Character.ICharacter`
  - Exposes `PossibleAttackers`, the same relation collection used by `NpcAlly.FindTarget()`.
- `Awaken.TG.Main.Heroes.Hero`
  - Implements `ICharacter` and exposes `PossibleAttackers`.

## Decision

Avalon Companions may add a lightweight native defend assist for plugin-owned wolf/bear one-session candidates:

1. Run only for managed `CreatureCandidateLocations`.
2. Run automatically only when `Companions.EnableNativeDefendAssist=true`.
3. Check `Hero.Current.PossibleAttackers`.
4. If a live attacker exists, find managed candidates with `NpcHeroPetAlly`.
5. Call `NpcHeroPetAlly.EnterCombat()`.

This does not choose or force a target directly. The native `NpcAlly`/`NpcHeroSummon` path still reads the hero's attackers and decides the combat target.

2026-06-15 command-mode update: the plugin-owned command panel may expose an explicit `Defend` mode for the same managed wolf/bear candidates. This mode uses the same guarded helper above: it only prompts `NpcHeroPetAlly.EnterCombat()` when `Hero.Current.PossibleAttackers` already contains a live attacker. It does not add custom target selection, arbitrary nearby target scans, forced hostility, persistence, or a new defend hotkey. `Companions.EnableNativeDefendAssist` remains the automatic fallback gate outside explicit `Defend` mode.

## Boundary

This approves automatic attack response and explicit panel `Defend` mode only for Avalon-owned one-session wolf/bear candidates that already passed native ally setup.

This does not approve:

- custom manual attack commands or defend hotkeys,
- custom target selection,
- forcing attacks against arbitrary nearby actors,
- town/civilian aggression logic,
- wild creature conversion,
- persistence or save/load behavior,
- humanoid companions.

## Validation still needed

- Throwaway-save smoke test: summon wolf, let an enemy attack the hero, confirm the wolf attacks that enemy.
- Throwaway-save smoke test: summon bear, let an enemy attack the hero, confirm the bear attacks that enemy.
- Confirm no repeated log spam.
- Confirm dismiss still removes the candidate cleanly during or after combat.
- Confirm town/civilian behavior before adding attack-assist commands, custom target selection, or defend hotkeys.
