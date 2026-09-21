# Decision 0002: DK2 Visual AssetBundle Proof

Date: 2026-08-01

Status: accepted for a visual-only Dragon Knight proof.

## Evidence

- The user supplied Fab library screenshots showing Dragon Knight saved in the user's library and the listing name `Dragon Knight`, publisher `Naked Singularity Studio - Fantasy`, and advertised `2-PHASES BOSS`, `IRON AND FIRE`, and `UE4 + UE5 SKELETON`.
- Local source path is `<local-path>`.
- Unity `6000.0.64f1` imported `dragonknight_2022333f1.unitypackage` and produced `mods/dragon-knight/docs/generated-dragon-knight-unity-inspection-report.json`.
- The import report records 102 imported Dragon Knight assets, 8 prefabs, Fire/Iron character prefabs, Fire/Iron character-with-weapon prefabs, Fire/Iron sword prefabs, zero missing scripts on the prefabs, and no gameplay actor components.
- The import report records one imported animation clip, `Pose_Check`, and zero controller states in `Dragon_Knight_Controller.controller`.

## Decision

DK2 may build a Windows AssetBundle from Unity `6000.0.64f1` containing only:

- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Iron_Weapon.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire_Weapon.prefab`

The BepInEx plugin may load that bundle from its own plugin folder and instantiate one visual-only prefab on explicit hotkey request.

## Limits

This decision does not authorize:

- native actor construction;
- `LocationTemplate` or `NpcTemplate` registration;
- AI;
- attacks;
- custom combat behavior;
- follower mechanics;
- usable items;
- inventory grants;
- save writes;
- roaming or population integration;
- treating `Pose_Check` as a combat animation.

The Iron and Fire prefabs may be used as visual phase placeholders only. Real two-phase combat needs a later decision after attack animation clips, phase state rules, native combat mapping, and health/transition policy are proven.
