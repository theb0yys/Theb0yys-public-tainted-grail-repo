# Content Example — Simple Weapon

**Authoring path:** Merlin Workshop  
**Evidence:** STATIC_CONFIRMED  
**This public recipe:** NOT_RUN

A FoA weapon is an item plus weapon-specific specialization.

## Minimal route

1. Create ItemTemplate.
2. Select the correct abstract weapon family.
3. Add ItemEquipSpec.
4. Set EquipmentType, such as OneHanded or TwoHanded.
5. Add ItemStatsAttachment.
6. Provide a mod-owned weapon representation prefab.
7. On the representation prefab, satisfy the Weapon component contract:
   - WeaponType;
   - collider;
   - left-handed state where applicable.
8. Keep world drop/pickup representation separate from equipped representation.
9. Test equip/unequip before combat tuning.

## First practice weapon

Use a placeholder model you own.

Suggested learning target:

~~~text
logical type: one-handed practice sword
equipment type: OneHanded
damage: low training values
no custom animation changes
no custom hitbox system
no special effects
~~~

The point is to prove:

~~~text
ItemTemplate
 -> inheritance
 -> ItemEquipSpec
 -> representation prefab
 -> Weapon component
 -> ItemStatsAttachment
 -> vanilla equip/combat lifecycle
~~~

Do not start by replacing animations, hit reactions, VFX and combat rules simultaneously.

See docs/pipelines/WEAPONS.md for the full static-confirmed contract.
