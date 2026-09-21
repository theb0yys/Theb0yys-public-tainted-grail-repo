# Weapon Stage 1 — Native Archetype and Source Profile

## Objective

Choose one exact native rigid-weapon family whose full item/equip/combat/presentation contract can be preserved for the first custom weapon.

## Procedure

1. Identify the target game/runtime build.
2. Select one native weapon archetype, preferably a simple rigid one-handed weapon for the first proof.
3. Record the complete source ItemTemplate profile:
   - GUID/name;
   - abstract-template lineage;
   - attachment groups;
   - ItemEquipSpec;
   - representation references;
   - animation/controller profile;
   - combat profile;
   - audio/trail/finisher/hit-stop references;
   - perspective-specific presentation assumptions.
4. Record the native hand/equip chain expected for this archetype.
5. Do not derive combat geometry or equip behaviour from the imported render mesh.
6. Mark every field that will remain inherited for the first proof.

## Output

A weapon source profile that can be compared against the custom clone.

## Validation gate

PASSED only when the exact source identity resolves and the expected ItemEquip/combat/presentation contract is recorded for the claimed build.

## Does not prove

No custom weapon exists yet. This stage does not prove registration, equip, presentation, combat, preview, or persistence.

## Next

Proceed to [custom ItemTemplate registration](../02-template-registration/README.md).
