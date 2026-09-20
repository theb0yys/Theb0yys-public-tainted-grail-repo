# Extend Crime Semantics Without Replacing Native Bounty

Use CrimeUtils.AddBounty and TemporaryBounty reporting as observation/intervention points while keeping FoA's crime owner authoritative.

Working lineage: [Preserve Native Bounty, Extend Semantic Truth](../../../research/case-studies/crime/preserve-native-bounty.md).

## Bounty route

The maintained Crime and Consequences implementation patches:

~~~text
Awaken.TG.Main.Fights.Factions.Crimes.CrimeUtils.AddBounty
~~~

with a prefix/postfix.

Before native AddBounty runs, you can read:

~~~csharp
float currentBounty = CrimeUtils.Bounty(template);
~~~

and, if your mod intentionally changes the amount, adjust only the incoming value argument.

The working implementation composes:

~~~text
incoming bounty
× configured bounty multiplier
× current wanted-heat multiplier
× optional disguise multiplier
→ native CrimeUtils.AddBounty continues
~~~

It does not create a second bounty store.

## Observe the native result afterward

The AddBounty postfix runs after FoA has committed the bounty.

Use that point to:

- read the new native bounty;
- update mod-owned wanted/incident metadata;
- evaluate optional downstream reactions;
- emit UI/diagnostic state.

The native CrimeOwnerTemplate remains the legal owner.

## Immediate witness reporting

FoA temporary witness/report state exposes:

~~~text
TemporaryBounty.RegisterCrime
→ TemporaryBounty.GuardApplyCrimes()
~~~

The maintained implementation can call GuardApplyCrimes after RegisterCrime when its immediate-report feature is enabled.

If you only want semantic tracking, observe this route and record the report/witness context without forcing it.

## Sidecar incident records

For richer semantics, create a mod-owned record such as:

~~~text
incidentId
native crime owner/template identity
native bounty before
native bounty after
event kind
witness/report context
game/session timestamp
optional mod categories
~~~

That record supplements the native legal state.

Do not use the incident object as the thing that decides whether the hero is wanted.

## Useful native state remains native

Keep these on FoA's side:

- CrimeUtils.Bounty(...);
- CrimeOwnerTemplate;
- TemporaryBounty;
- witness/report application;
- prison punishment;
- native guard/crime search state.

Your semantic layer can consume them and add presentation/history.

## Example: wanted-heat tiers

A clean extension is:

~~~text
read current native bounty
→ map to mod-owned heat tier
→ let CrimeUtils.AddBounty commit native amount
→ read final native bounty
→ update heat label/history
~~~

The tier is your interpretation. The bounty value is FoA's state.
