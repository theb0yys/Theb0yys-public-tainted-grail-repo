# DK4 Cromlech Encounter Geometry

Date: 2026-08-02

Status: accepted encounter-geometry evidence. This note does not authorize runtime source, actor spawning, movement, attacks, AI, phase combat, item registration, save writes, or native shrine interaction.

## User Direction

- Encounter location is Ancient Cromlech / Stonehenge in Horns of the South.
- The player must be able to see the Dragon Knight from the edge, watch, and get curious without immediately starting the fight.
- The boss may use that edge distance to stand, turn, and get ready.
- Combat must be contained inside the Stonehenge structure around the circular arena.
- Existing shrine/player activation objects must not be used as the encounter trigger.

## Live Dump Evidence

Source dump:

`<local-path>`

Runtime snapshot:

- Scene: `Horns of the South [CampaignMap_HOS]`
- Hero edge coords: `-1808.181|82.621|-2931.862`
- Hero in combat: `false`

Relevant `cromlech_target_proof.csv` rows:

- `AltarInteract`
  - `Location.ID`: `CM_Stonehenge_0_2258891627046429048_1`
  - position: `-1792.661|79.942|-2912.844`
  - hero distance from edge dump: `24.692`
  - use: measurement/scene proof only; rejected as encounter trigger.
- `Spec_Discovery_Small_Stonecircle`
  - `Location.ID`: `CM_Stonehenge_0_428019505_1`
  - position: `-1793.15|78.393|-2911.98`
  - hero distance from edge dump: `25.28`
  - use: measurement/scene proof only; rejected as encounter trigger.
- `DruidFocus`
  - `Location.ID`: `CM_Stonehenge_0_1470083556330758694_1`
  - position: `-1792.949|80.489|-2913.684`
  - hero distance from edge dump: `23.811`
  - use: measurement/scene proof only.

Calculated horizontal distances from the edge dump hero position:

- edge to `AltarInteract`: `24.55m`
- edge to `Spec_Discovery_Small_Stonecircle`: `24.92m`
- edge to `DruidFocus`: `23.72m`

## Encounter Geometry Decision

Use a Dragon Knight-owned arena center near the Cromlech altar/center. Do not use `AltarInteract`, `Spec_Discovery_Small_Stonecircle`, discovery radius, prompt visibility, or line-of-sight as the trigger.

Recommended staged radii:

- Outer wake radius: `25m`
  - Boss may stand, turn toward the player, and ignite/ready the sword.
  - No combat start.
  - No health bar.
  - No attacks, movement pursuit, damage, or phase behavior.
- Inner fight-start radius: `5m`
  - Equivalent to a 10m-wide central circle.
  - Crossing this inner ring starts the actual boss fight after the later actor/AI/combat gates authorize it.
- Arena soft leash radius: `22m`
  - Preferred movement/combat space stays inside the standing-stone ring.
- Arena hard leash radius: `25m`
  - Boss must not pursue past the measured edge of the Stonehenge structure.

## Remaining Gate

This geometry evidence does not close DK4 by itself. DK4 source remains blocked until the live diagnostic proves Dragon Knight actor observation, exact actor `Location.ID`, host ownership/activation, cleanup, and no-save behavior.
