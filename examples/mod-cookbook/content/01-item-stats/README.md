# Content Example — Item Stats

**Authoring path:** Merlin Workshop  
**Evidence:** STATIC_CONFIRMED  
**This public recipe:** NOT_RUN

This is the concrete content-authoring equivalent of the code examples.

## Goal

Create one mod-owned equippable item and give it ItemStatsAttachment values without copying a vanilla asset.

## 1. Create the item

In Merlin:

~~~text
Assets -> TG Data -> Miscellaneous -> Item
~~~

Create a prefab with a unique mod-owned identity.

Choose the correct abstract/inheritance family before tuning stats. Category behavior is derived from inheritance; it is not just a label.

## 2. Add ItemStatsAttachment

ItemStatsAttachment is the stat surface for equippable items.

The inspected authoring contract exposes, when applicable:

### Stamina
- lightAttackStaminaCost
- heavyAttackStaminaCost
- heavyAttackHoldCostPerTick
- blockStaminaCostMultiplier
- parryStaminaCost
- pushStaminaCost
- ranged draw/hold costs

### Weapon damage
- minDamage
- maxDamage
- damageGain
- heavyAttackDamageMultiplier
- pushDamageMultiplier
- backStabDamageMultiplier
- armorPenetration
- damageIncreasePerCharge
- critical / weak-spot / sneak multipliers
- damageType and damageSubTypes

Damage subtype percentages should add to 100%.

### Armour/blocking
- armor
- armorGain
- blockDamageReductionPercent
- blockAngle
- blockGain

### Force/poise
- forceDamage
- ragdollForce
- poiseDamage
- poise multipliers

### Magic
- lightCastManaCost
- heavyCastManaCost
- hold mana costs
- magicHeldSpeedMultiplier

## Training values

For a first test weapon, start boring:

~~~text
lightAttackStaminaCost = 10
heavyAttackStaminaCost = 25
minDamage = 10
maxDamage = 15
heavyAttackDamageMultiplier = 2
blockDamageReductionPercent = 50
blockAngle = 45
forceDamage = 1
poiseDamage = 1
npcDamageMultiplier = 1
~~~

These are **training values, not balance recommendations**. Change one field at a time after the item resolves and equips.

## 3. Validate in layers

1. template resolves;
2. item appears where your test path exposes it;
3. equipment category is correct;
4. equip representation works if applicable;
5. baseline attack/block works;
6. then change one stat and compare.

Do not interpret a working prefab preview as game-runtime stat validation.
