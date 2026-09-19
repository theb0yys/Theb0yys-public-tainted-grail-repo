# Fail-Closed Cross-Mod API Bridge

Wyrd Decoy → Wyrd Hunt demonstrates a clean optional dependency pattern.

## Working implementation lineage

Wyrd Decoy calls Wyrd Hunt's public static API without compiling against Wyrd Hunt. The decoy route was confirmed working by user report and log evidence.

## Discovery

The consumer resolves the provider type at runtime:

~~~text
WyrdHunt.WyrdHuntApi, WyrdHunt
~~~

Then it requires the exact public members it needs:

- `TryReduceThreat(float, string)`
- `TrySuppressThreatGain(float, string)`
- `CurrentThreat`
- `IsAvailable`

If any required member is absent, the bridge returns unavailable instead of reaching into provider internals.

## Call flow

~~~text
consumer request
→ resolve provider API
→ check provider IsAvailable
→ call required public methods
→ read result/state
→ report success/failure
~~~

## Transactional resource use

Wyrd Decoy optionally requires a carried item. It consumes that item **only after** the Wyrd Hunt API reports that the decoy effect succeeded.

That ordering matters:

~~~text
verify resource
→ attempt provider action
→ provider succeeds
→ consume resource
→ start cooldown
~~~

Do not consume the item first and hope the external action works.

## Rule

Cross-mod integration should use a small public contract, not reflection into private fields or Harmony patches against another mod's implementation.
