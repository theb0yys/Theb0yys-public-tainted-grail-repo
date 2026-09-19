<!-- Canonical Wave 5 native-system page split from docs/reference/CRIME_STEALTH_BOUNTY.md. -->
# Crime, Stealth, Bounty, Guards, and Consequences

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/crime/intervention-patterns.md).

## What this system is

FoA already has a native crime stack. A mod does not need to invent a parallel wanted system just to increase or reshape crime pressure.

The researched native chain includes:

~~~text
illegal action / witness
→ native crime classification
→ witnessed/temporary crime state
→ CrimeOwnerTemplate-owned bounty
→ guard/vigilante reaction
→ pursuit / intervention
→ payment, confiscation, jail, prison consequences
~~~

Stealth, pickpocket, lockpicking, disguise-like modifiers, and stolen-item pricing intersect this stack but have their own native owners.

## Who owns it in FoA

Important owners include:

- `CrimeOwnerTemplate` — owner-scoped bounty/crime authority;
- `CrimeUtils` — bounty operations;
- `TemporaryBounty` — witnessed/delayed crime application;
- `CrimeReactionUtils` — guard response;
- `GuardIntervention` — intervention/pursuit behavior;
- `CrimePenalties` — bounty payment, jail, confiscation;
- `NpcCrimeReactions` — NPC crime responses such as pickpocketing;
- `HeroPriceProvider` — stolen-item merchant pricing;
- `HeroStats` — stealth/theft/pickpocket/lockpick multipliers.

## Important identities, types, and methods

Researched exact surfaces include:

- `CrimeUtils.AddBounty(CrimeOwnerTemplate, float, out Change<float>)`
- `CrimeUtils.Bounty`
- `CrimeUtils.HasBounty`
- `CrimeUtils.ClearBounty`
- `TemporaryBounty.RegisterCrime`
- `TemporaryBounty.GuardApplyCrimes`
- `CrimeReactionUtils.CallGuardsToHero(CrimeOwnerTemplate)`
- private `CrimeReactionUtils.CallGuard(NpcElement)`
- `GuardIntervention.Update(float)`
- `CrimePenalties.PayBounty(...)`
- `CrimePenalties.GoToPrisonPeacefully(...)`
- `CrimePenalties.GoToPrisonFromCombat(...)`
- `HeroPriceProvider.GetStolenModifier(IMerchant, Item)`
- `NpcCrimeReactions.Pickpocketing(float, Item)`

Useful `HeroStats` include:

- `CrouchNoiseMultiplier`
- `CrouchVisibilityMultiplier`
- `CrouchSpeedMultiplier`
- `LockpickToleranceMultiplier`
- `LockpickDamageMultiplier`
- `TheftHoldTimeModifier`
- `PickpocketHoldTimeModifier`
- `PickpocketRecoveryChance`
- `SneakDamageMultiplier`
- `MeleeSneakDamageMultiplier`

## Where it exists in the lifecycle

A simplified native crime flow is:

~~~text
action classified illegal
→ witness/temporary crime recorded
→ owner-scoped bounty applied
→ nearby/native guard path reacts
→ guard targets or follows hero
→ intervention / dialogue / combat
→ pay bounty / jail / confiscation / clear
~~~

A theft/pickpocket path can enter earlier through interaction/UI/action owners. See [Interactions and Usables](../../docs/reference/INTERACTIONS_USABLES.md).

## Current proof boundary

Native crime/bounty/guard/jail/pricing ownership is strongly mapped by decompilation and implementation research.

A generic custom bounty-hunter spawn system is **not** supported by the current evidence. The safe public lesson is to reuse native owner-scoped crime/guard behavior until a separate population/spawner route is proven.
