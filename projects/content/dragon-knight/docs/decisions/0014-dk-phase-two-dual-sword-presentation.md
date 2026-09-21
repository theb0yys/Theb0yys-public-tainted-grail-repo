# 0014 - Dragon Knight Phase Two Dual-Sword Presentation

Status: accepted render-proof update

Date: 2026-08-02

## Context

The Dragon Knight asset package supplies separate Fire and Iron Dragon Knight sword prefabs. The current retarget/render proof uses the Iron character-with-weapon prefab for phase 1 and the Fire character-with-weapon prefab for phase 2. The user proposed that phase 2 can use both swords.

## Decision

Phase 1 remains a single-sword Iron presentation.

Phase 2 may present as dual-sword:

- mainhand source: `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire_Weapon.prefab`;
- offhand source: `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/WEAPON/SM_DragonKnight_Sword_Iron.prefab`;
- offhand attach target: the Fire phase humanoid `hand_l` bone.

`SM_Greatsword.fbx` from the MCB move-set source remains excluded. The Dragon Knight boss proof remains based on Dragon Knight one-handed sword prefabs.

## Evidence

Unity `6000.0.64f1` reran the retarget/render proof and passed with marker `DRAGON_KNIGHT_RETARGET_RENDER_PROOF_PASS`.

The generated manifest records:

- `phaseTwoDualSword.enabled=true`;
- offhand object `Phase2_Offhand_IronSword`;
- left-hand path `Fire_Phase_2_DualSword/root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l`;
- offhand renderer count `1`;
- mesh renderer count `1`;
- skinned renderer count `0`;
- local position `0|0|0`;
- local rotation `0|0|0|1`;
- local scale `1|1|1`;
- `phaseTwoDualSword.pass=true`.

Manifest SHA-256: `AA7DD7B69AD052B7AF67BA24394E53A5606DA27D46BDC5BC3C5C9DF778421AAA`.

Bundle SHA-256: `18A140AAD734EC835A48CFED98F8A5A2C5F2D50D0B9ACAE5FA8E04616E019A22`.

## Boundary

This is presentation proof only. It does not authorize or prove runtime dual-wield combat, weapon sockets, hitboxes, damage windows, animation events, AI action dispatch, movement, companion protection, real phase combat, items, saves, cleanup, roaming, or release packaging.
