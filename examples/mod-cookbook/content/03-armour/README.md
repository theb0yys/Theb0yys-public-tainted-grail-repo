# Content Example — Simple Armour

**Authoring path:** Merlin Workshop  
**Evidence:** STATIC_CONFIRMED  
**This public recipe:** NOT_RUN

Ordinary wearable armour is an ItemTemplate specialization, not a separate standalone armour database.

## Minimal route

1. Create ItemTemplate.
2. Select the correct armour inheritance/weight family.
3. Add ItemEquipSpec.
4. Choose an EquipmentType:
   - Cuirass
   - Helmet
   - Gauntlets
   - Greaves
   - Boots
   - Back
5. Add a mod-owned equipment representation for the NPC/body family you are testing.
6. Add ItemStatsAttachment.
7. Set only a small armor value for the first test.
8. Test equip, unequip, appearance and stat effect.
9. Add requirements/weight/polish afterward.

## Example training profile

~~~text
slot = Cuirass
armor = 5
armorGain = 0
weight = modest test value on ItemTemplate
no stat requirements
one representation only
~~~

These numbers are illustrative, not balance guidance.

Keep the worn representation separate from a drop/pickup representation.

See docs/pipelines/ARMOUR.md.
