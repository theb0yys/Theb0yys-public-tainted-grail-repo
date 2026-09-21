# 0013 - Dragon Knight Custom Boss Move-Set Source

Status: accepted source-proof update

Date: 2026-08-02

## Context

The base Dragon Knight Unity package contains the Iron and Fire character-with-weapon prefabs and high-quality material/texture source, but its imported controller has zero states and the only usable imported clip is `Pose_Check`. That package alone cannot prove custom attacks, a phase transition animation, root-motion candidates, or combat animation mapping.

The user supplied a separate dedicated boss move-set source:

`<local-path>`

Read-only inventory found `MCB.zip`, an `MCB` folder, 93 FBX files, 32 Maya source files, and `SM_Greatsword.fbx`.

## Decision

The Dragon Knight Unity retarget/render proof must use the supplied `MCB` boss move-set FBX clips as the custom animation source. Generic one-handed humanoid placeholder clips are no longer the proof source for Dragon Knight boss attacks or phase presentation.

The proof builder must:

- require `-dragonKnightBossMoveSetRoot`;
- copy/import `MCB` FBX clips only;
- require key Iron/phase-1, phase-transform, Fire/phase-2, and root-motion proof clips;
- force Humanoid import for MCB FBXs and require Humanoid source avatars;
- build proof controller states for one-handed Iron, one-handed Fire, phase transform, and root-motion candidates;
- assign the resulting controller to the Dragon Knight Iron/Fire humanoid prefabs in the proof prefab;
- keep runtime AI, movement, attacks, damage, hitboxes, save access, game deployment, and roaming untouched.

`SM_Greatsword.fbx` is excluded from this proof. The Dragon Knight boss remains a one-handed sword boss using the Dragon Knight sword prefab.

## Consequences

The previous generated retarget/render proof manifest was stale because it was built from generic third-party one-handed clips. The current generated manifest now records `DRAGON_KNIGHT_RETARGET_RENDER_PROOF_PASS` for the supplied MCB boss move-set source.

This decision does not authorize runtime attacks, animation events, hitboxes, damage, movement, AI action dispatch, companion protection, real phase combat, saves, items, or roaming.

## Retarget Evidence Note

A Unity rerun showed that direct `CopyFromOther` import from the Dragon Knight avatar onto the MCB FBXs is not valid because the MCB files use an `SK_Mannequin` hierarchy that does not match the Dragon Knight avatar hierarchy. The proof path therefore uses Unity Humanoid retargeting from each MCB source avatar into a controller assigned to the Dragon Knight prefab, rather than copying the Dragon Knight avatar into the MCB imports.
