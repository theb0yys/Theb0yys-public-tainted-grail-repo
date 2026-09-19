# Crime, Stealth, Bounty, Guards, and Consequences

> **Reference page.** Use this when modifying theft, witnesses, bounty, guard pursuit, jail, stolen-item pricing, pickpocketing, or stealth pressure.

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

A theft/pickpocket path can enter earlier through interaction/UI/action owners. See [Interactions and Usables](INTERACTIONS_USABLES.md).

## How we interact with it

### Prefer native crime authority over a parallel wanted system

If the goal is stronger consequences:

- increase or reinterpret native bounty;
- react to native witnessed-crime events;
- use native guard/intervention ownership;
- use native jail/payment/confiscation owners.

Do not start by spawning a custom bounty-hunter system unless the native population/spawn path is separately proven.

### Refresh existing guard pursuit through the public/native AI surface

Research found that private `CrimeReactionUtils.CallGuard(NpcElement)` does not itself spawn an actor or write persistence. It refreshes an existing guard through the native alert stack.

Later AI research preferred the public/native alert-stack route directly instead of reflecting the private wrapper.

That gives a general rule:

> If a private helper is only forwarding into a public owner API, integrate at the public owner.

### Use non-saved stat tweaks for reversible stealth tuning

Where the behavior is genuinely a `HeroStats` multiplier, use scoped non-saved `StatTweak` patterns instead of writing save-backed stats or editing item templates.

### Read equipment for disguise-like logic; do not mutate it just to classify the player

The research identifies read-only helmet/equipment access and item names/tags that can support contextual modifiers.

The classification itself must remain explicit and reviewable.

## Why this route

The research found a lot of native crime infrastructure already exists:

- bounties;
- witnesses;
- guard calls;
- prison;
- confiscation;
- fences;
- pickpocketing;
- lockpicking;
- crouch stealth.

The safest first improvements therefore reuse the native stack rather than duplicating it.

The bounty-hunter research is an important counterexample: candidate templates existed, but repeated diagnostics found **zero loaded vanilla spawner references** for those bounty-hunter candidates. That is not enough evidence to invent a production spawn route.

## What goes wrong

### Template taxonomy mistaken for spawn authority

Finding `Spec_Enemy_Generic_Tier4_BountyHunter*` templates does not prove how or where the game safely spawns them.

### Private helper reflected when the actual public owner is known

Adds unnecessary compatibility risk.

### Crime UI behavior mistaken for crime authority

A prompt or stolen-item label is presentation/input. The crime owner is deeper in the native crime stack.

### Stolen price changes treated as item-template changes

Pricing can be changed at the pricing-owner layer without mutating item definitions.

### Detection pressure increased through too many variables at once

The research produced user feedback that one proof pass made detection pressure "insanely high." That is evidence to tune bounded native multipliers, not to add yet more overlapping detection systems.

### Persistent consequences changed without save validation

Bounty, jail, reputation and progression-adjacent crime state can be save-visible. Durable changes need explicit save/reload proof.

## How to verify

For crime changes, validate separately:

1. exact `CrimeOwnerTemplate`;
2. illegal action classification;
3. witness/temporary crime path;
4. bounty before/after;
5. existing guard reaction;
6. moving-hero pursuit/intervention;
7. payment/jail/confiscation behavior;
8. stolen-item merchant behavior if touched;
9. stealth/pickpocket modifiers if touched;
10. leave/return and save/load when persistent;
11. no unintended actor spawning or duplicate guards.

## Current proof boundary

Native crime/bounty/guard/jail/pricing ownership is strongly mapped by decompilation and implementation research.

A generic custom bounty-hunter spawn system is **not** supported by the current evidence. The safe public lesson is to reuse native owner-scoped crime/guard behavior until a separate population/spawner route is proven.
