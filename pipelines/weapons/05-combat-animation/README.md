# Weapon Stage 5 — Combat, Animation, Audio, and Hit Geometry

## Objective

Prove that the custom weapon still uses the intended native combat contract after its identity and presentation were changed.

## Rule

Render bounds are not the melee hitbox. Preserve the source combat owner and geometry unless a separately evidenced change is being made.

## Procedure

1. Keep the source archetype's CharacterWeapon/combat profile for the first proof.
2. Exercise the normal attack route.
3. Verify:
   - correct hand/weapon attachment during attack;
   - expected animation/controller events;
   - native sweep/hit detection;
   - damage/hit response;
   - trail/VFX ownership where applicable;
   - audio events;
   - finisher/hit-stop behaviour where applicable.
4. Confirm the custom presentation remains aligned with the native combat owner.
5. Test repeated equip/attack/unequip cycles to catch stale state.

## Check

Confirm that the native combat path produces the expected attack/hit behaviour and no custom presentation shortcut has become a second combat owner.

## Before continuing

One attack does not cover every move set, every perspective, inventory preview, scene transition, save/load, or another weapon archetype.

## Next

Proceed to [perspectives and preview](../06-perspectives-preview/README.md).
