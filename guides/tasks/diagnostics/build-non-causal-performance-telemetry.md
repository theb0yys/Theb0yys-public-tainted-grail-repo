# Build Performance Telemetry Without Blaming a Plugin

**Evidence status: PARTIAL.** The diagnostic model is established; this is not a validated causal profiler for every source of frame-time cost.

Working lineage: [Telemetry Without Blame](../../../research/case-studies/performance/telemetry-without-blame.md).

## Goal

Collect useful performance context without making unsupported causal claims.

The central rule is:

> Plugin presence is context, not causality.

## Collect

Useful observations include:

- frame timing;
- GC/allocation pressure;
- loaded mod stack;
- scene/context;
- bounded timestamps/markers.

## Keep proof levels separate

```text
plugin present
≠ correlated with spike
≠ isolated as contributor
≠ proven cause
```

A good diagnostic workflow keeps two lanes open:

- native/engine baseline;
- mod-stack isolation.

## Process

1. capture baseline;
2. capture loaded stack as metadata;
3. record timing/allocation symptoms;
4. reproduce;
5. disable/isolate one variable at a time;
6. compare controlled runs;
7. only use causal language when isolation supports it.

## Avoid

- "Mod X caused this" because it was loaded;
- per-frame giant log dumps;
- invasive instrumentation that creates the hitch;
- mixing user hardware/environment differences into one conclusion.

## Verification

A useful telemetry tool should prove:

- stable sampling overhead;
- bounded storage/logging;
- timestamps align with observed spikes;
- loaded-stack capture is accurate;
- disabling telemetry returns to baseline overhead;
- reports distinguish observation from attribution.

## Current proof boundary

The epistemic/diagnostic design is established. Exact performance overhead, sampling accuracy and causal isolation still require runtime validation per implementation.
