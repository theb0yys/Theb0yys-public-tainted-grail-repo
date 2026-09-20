# Hero and Character Stat Surfaces

> **Evidence scope:** exact Mono static inspection for Steam build `24270691`, `TG.Main.dll` SHA-256 `749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982`.  
> **Use:** lookup and research planning. Presence of a stat does not grant ownership or imply that modifying it is compatible with other mods or safe for saves.

## HeroRPGStats

The inspected `HeroRPGStats : Element<Hero>` exposes the base RPG attributes:

- `Strength`
- `Dexterity`
- `Spirituality`
- `Perception`
- `Endurance`
- `Practicality`

Its `OnInitialize()` initializes its wrapper and registers `AfterHeroFullyInitialized` against the parent Hero's fully-initialized callback.

## CharacterStats

The inspected `CharacterStats : Element<ICharacter>` exposes:

- `Level`
- `Stamina`, `MaxStamina`, `StaminaRegen`, `StaminaUsageMultiplier`, `SprintCostMultiplier`
- `Mana`, `MaxMana`, `ManaUsageMultiplier`, `ManaRegen`, `ManaRegenPercentage`
- `ManaShield`, `ManaShieldRetaliation`, `MeleeRetaliation`
- `MovementSpeedMultiplier`
- `Strength`, `StrengthLinear`
- `IncomingDamage`, `IncomingHealing`
- `ConsumableHealingBonus`, `PotionHealingBonus`
- `Resistance`, `LifeSteal`
- `BuffStrength`, `BuffDuration`, `DebuffStrength`, `DebuffDuration`
- `MeleeDamageMultiplier`, `OneHandedMeleeDamageMultiplier`, `TwoHandedMeleeDamageMultiplier`, `UnarmedMeleeDamageMultiplier`
- `RangedDamageMultiplier`
- `MagicStrength`
- `HoldBlockCostReduction`
- `DeflectPrecision`

## HeroStats

The inspected `HeroStats : Element<Hero>` exposes a broad hero-specific stat surface including:

### Progression and points

- `TalentPoints`
- `CatalystTalentPoints`
- `SarrasMageTalentPoints`
- `SarrasRogueTalentPoints`
- `SarrasWarriorTalentPoints`
- `BaseStatPoints`
- `XPForNextLevel`
- `XP`

### Attack and cast speed

- `AttackSpeed`
- `BowDrawSpeed`
- `OneHandedLightAttackSpeed`
- `OneHandedHeavyAttackSpeed`
- `TwoHandedLightAttackSpeed`
- `TwoHandedHeavyAttackSpeed`
- `DualHandedLightAttackSpeed`
- `DualHandedHeavyAttackSpeed`
- `FistLightAttackSpeed`
- `FistHeavyAttackSpeed`
- `BlockPrepareSpeed`
- `SpellChargeSpeed`

### Movement and survival

- `MoveSpeed`
- `SprintSpeed`
- `CrouchSpeedMultiplier`
- `SwimSpeed`
- `BlockingMovementMultiplier`
- `JumpHeight`
- `EncumbranceLimit`
- `ArmorWeightMultiplier`
- `OxygenLevel`
- `OxygenUsage`
- `FallDamageMultiplier`

### Dash and stamina feel

- `DashStamina`
- `DashSpeed`
- `DashCostMultiplier`
- `MaxDashOptimalCounter`
- `DashRegenDurationMultiplier`
- `DashIFramesBonus`
- `ItemStaminaCostMultiplier`
- `StaminaDepletedTimeMultiplier`
- `DualWieldHeavyAttackCostMultiplier`

### Stealth, crime and interaction

- `FootstepsNoisiness`
- `VisibilityMultiplier`
- `NoiseMultiplier`
- `CrouchNoiseMultiplier`
- `CrouchVisibilityMultiplier`
- `LockpickDamageMultiplier`
- `LockpickToleranceMultiplier`
- `TheftHoldTimeModifier`
- `PickpocketHoldTimeModifier`
- `PickpocketRecoveryChance`

### Combat modifiers

- `DamageNullifier`
- `CriticalChance`
- `CriticalDamageMultiplier`
- `SneakDamageMultiplier`
- `BackStabDamageMultiplier`
- `MeleeSneakDamageMultiplier`
- `WeakSpotDamageMultiplier`
- `MeleeWeakSpotDamageMultiplier`
- `MeleeCriticalChance`
- `RangedCriticalChance`
- `MagicCriticalChance`
- `ParryStaminaDamageMultiplier`
- `ParryWindowBonus`
- `BlockingStaminaDamageMultiplier`
- `MinimumHeavyDamageAdd`
- `MaximumHeavyDamageAdd`
- `OutgoingBuildupMultiplier`
- `IncomingBuildupMultiplier`

### Wyrd, ranged and summoning

- `MaxWyrdSkillDuration`
- `WyrdSkillDuration`
- `WyrdWhispers`
- `WyrdMemoryShards`
- `ArrowRetrievalChance`
- `BowSwayMultiplier`
- `AimSensitivityMultiplier`
- `SummonsManaDrainMultiplier`
- `SummonLimit`
- `MaxHealthReservation`
- `MaxManaReservation`

### Economy and crafting

- `LootChanceMultiplier`
- `AdditionalScrapChance`
- `EquipmentLevelBonus`
- `CookingLevelBonus`
- `AlchemyLevelBonus`
- `UpgradeDiscount`
- `CraftingRequirementModifier`
- `PrisonPenaltyMultiplier`
- `ArmorPenaltyMultiplier`
- `FenceSellBonusMultiplier`
- `CraftingSkillBonus`
- `FishingMeanMultiplier`

### Equipment timing

- `EquipWeaponActionCooldown`

## Stat value semantics

The inspected `Stat` contract distinguishes persistent/base state from tweak-derived state:

- `BaseValue` is the mutable base value;
- `ModifiedValue` resolves the tweak-calculated result or falls back to `BaseValue`;
- `ValueForSave` returns `BaseValue`;
- `SetTo(...)` mutates `BaseValue` and recalculates;
- `IncreaseBy(...)` delegates to `SetTo(BaseValue + amount, ...)`;
- `BaseInt` / `ModifiedInt` use ceiling conversion.

`LimitedStat` preserves the same save-base distinction while applying configured limits.

**Implication:** changing a base stat and applying a temporary `StatTweak` are materially different operations.

## StatTweak and TweakSystem

The inspected `StatTweak : Element<Model>, ITweaker` exposes:

- `Add(...)`
- `AddPreMultiply(...)`
- `Multi(...)`
- `Override(...)`
- runtime constructor taking target stat, modifier, optional priority, operation and optional parent model;
- `SetModifier(...)`;
- `SwapModifier(...)`.

Observed operation types:

- `Add`
- `Multi`
- `Override`
- `AddPreMultiply`

Observed tweak priorities:

- `PreSet`
- `AddPreMultiply`
- `Multiply`
- `Add`
- `Override`

`TweakSystem` indexes tweaks by stat owner/type, recalculates from `stat.BaseValue`, and removes tweaks attached to or created by a discarded model.

## Evidence boundary

This is a type/member inventory, not a recommendation to mutate every listed stat. Ownership, compatibility and save behavior remain feature-specific.

See [Player / Hero](../../systems/gameplay/player.md), [Types and Members](README.md), and [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
