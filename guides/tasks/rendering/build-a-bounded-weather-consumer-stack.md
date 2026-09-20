# Build a Bounded Weather Consumer Stack

Use this guide when several mods or subsystems need to react to one weather state without all writing weather independently.

Working lineage: [Rain / Day Owner-Stack Validation](../../../research/case-studies/weather/rain-day-owner-stack.md).  
Canonical system: [Weather, Environment, Sky, Water, and World-State Ownership](../../../knowledge/systems/world/weather-environment.md).

## What you will build

```text
one weather-truth owner
→ publishes one bounded state
→ sky consumer reads state and owns sky mutation
→ water consumer reads state and owns water mutation
→ other consumers may read the same state
→ no consumer rewrites the truth owner
```

The accepted live proof used a Rain/Day stack across Tainted Weather, Tainted Skybox and Immersive Water.

## Step 1 — choose one semantic truth owner

Exactly one component/mod should answer:

> What is the current approved weather state for this stack?

Consumers should not independently derive and write competing states.

For the proven stack, Weather remained the semantic source.

## Step 2 — publish a small snapshot/plan

Expose only the state consumers need.

Example:

```text
weather family: Rain
time bucket: Day
provider/context metadata
version/source identity
```

Do not expose mutable renderer internals as the contract.

## Step 3 — let each consumer own its own output

Examples:

- sky consumer owns Unity skybox mutation;
- water consumer owns water-surface profile mutation;
- weather owner does not directly mutate those outputs merely because it knows the state.

That separation makes cleanup and compatibility tractable.

## Step 4 — apply exact consumer policies

A sky consumer can map `Rain + Day` to one reviewed sky profile.

A water consumer can map the same semantic state to one reviewed water policy such as `GreenShallows`.

Those mappings are consumer policy—not the definition of weather truth itself.

## Step 5 — make ownership observable

Log one bounded record per state/application change:

```text
weather source published Rain
→ sky consumer selected/applied profile X
→ water consumer accepted plan/applied profile Y
```

Avoid per-frame spam.

## Step 6 — define fallback and cleanup

If a consumer is missing or fails:

- weather truth should remain valid;
- other consumers should continue independently;
- the failed consumer should restore only what it owns.

Do not let one missing visual consumer invalidate the semantic state.

## Verification checklist

1. exactly one weather truth owner publishes the state;
2. sky consumer reads but does not redefine that truth;
3. water consumer reads but does not redefine that truth;
4. each consumer mutates only its own surface;
5. disabling one consumer does not corrupt the others;
6. scene/context transitions reapply/release as intended;
7. no last-writer-wins flicker occurs.

The accepted live run established the bounded Rain/Day ownership handshake and later screenshot evidence supported the active sky/weather presentation.

## Evidence boundary

**Proven:** bounded Rain/Day semantic ownership and separate sky/water consumer application in the observed stack.

**Not claimed:** every weather family, every time bucket, all provider presets, all restore paths, water visual acceptance for that screenshot, or universal performance.
