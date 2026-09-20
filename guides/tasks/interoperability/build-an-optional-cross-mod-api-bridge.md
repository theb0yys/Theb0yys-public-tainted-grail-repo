# Build an Optional Cross-Mod API Bridge

Use this guide when one mod wants to call another mod without compiling against its implementation or reflecting into its private state.

Working lineage: [Fail-Closed Cross-Mod API Bridge](../../../research/case-studies/gameplay/cross-mod-api.md).

## Runnable source

Start from the minimal public example: [Optional cross-mod API example](../../../examples/mono/infrastructure/optional-cross-mod-api/README.md). Build it unchanged first, confirm the documented log/result, then make one change at a time.

## What you will build

```text
consumer wants optional feature
→ resolve provider public API type
→ require exact public members
→ check provider IsAvailable
→ call bounded public method
→ receive success/failure
→ only then commit consumer-side cost/cooldown
```

The proven example is **Wyrd Decoy → Wyrd Hunt**.

## Step 1 — define a small public provider contract

The provider should expose only the supported surface.

The working bridge required members equivalent to:

- `TryReduceThreat(float, string)`;
- `TrySuppressThreatGain(float, string)`;
- `CurrentThreat`;
- `IsAvailable`.

Do not expose internal fields just because the consumer wants access.

## Step 2 — resolve the public API type at runtime

The consumer in the working path resolves:

```text
WyrdHunt.WyrdHuntApi, WyrdHunt
```

Then it looks for the exact required public members.

If the type or any required member is missing, mark the bridge unavailable.

Do not fall back to private reflection.

## Step 3 — require provider availability

Even if the type exists, call the public availability signal before attempting the action.

```text
API type found
+ required members found
+ IsAvailable == true
→ bridge usable
```

Anything else means unavailable.

## Step 4 — keep the call transactional

If the consumer spends a resource, applies a cooldown or commits state, do it **after** provider success.

The proven Decoy order is:

```text
verify carried resource
→ attempt provider action
→ provider succeeds
→ consume resource
→ start cooldown
```

Do not consume first and hope the external operation works.

## Step 5 — fail closed

If:

- provider mod is absent;
- API type changed;
- required method is absent;
- provider is unavailable;
- call returns failure;
- reflection invocation throws;

then report the optional feature unavailable/failed and leave consumer gameplay state uncommitted.

## Step 6 — log contract status, not internals

Useful diagnostics:

- provider found/not found;
- API contract compatible/incompatible;
- provider available/unavailable;
- call succeeded/failed.

Do not dump private provider state.

## Verification checklist

1. consumer works normally with provider absent;
2. provider present resolves exact API type;
3. required members are found;
4. `IsAvailable` gates use correctly;
5. successful provider call produces intended effect;
6. consumer resource is spent only after success;
7. failed call does not spend resource/start cooldown;
8. incompatible provider contract fails closed;
9. consumer never patches provider internals.

## Evidence boundary

**Proven:** Wyrd Decoy calling Wyrd Hunt through a small public static API without compile-time coupling, with user/log confirmation and transactional resource use.

**Not claimed:** universal plugin discovery, arbitrary private reflection compatibility or automatic API-version migration.
