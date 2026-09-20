---
document_type: mechanic
scope: bounded in-game performance telemetry without gameplay mutation
runtime: il2cpp
evidence:
  source: SOURCE_REVIEWED
  runtime: PARTIAL_UI_EVIDENCE
  overhead: NOT_MEASURED_IN_CURRENT_RESEARCH_BASELINE
last_verified: 2026-09-20
---

# Bounded Performance Sampling

A performance monitor can collect useful evidence without patching gameplay or assigning blame.

## Safe observation set

Tainted Performance is designed around bounded/public counters such as:

- frame duration samples;
- optional Unity CPU/GPU frame timing;
- managed allocation counters;
- GC collection counters;
- scene/display/quality/VSync/target-FPS context;
- loaded BepInEx plugin inventory as **context only**.

## Hot-path rule

Per-frame collection should remain scalar and bounded.

Expensive work belongs off the normal sample path:

- plugin enumeration;
- report formatting;
- file I/O;
- derived chart/statistics work;
- large object scans.

## Report semantics

A report may say:

- this sampled window existed;
- p95/p99/worst/1%-low metrics were observed;
- CPU/GPU timing coverage existed or did not;
- GC/allocation pressure was present;
- these plugins were loaded.

It may **not** say a particular native subsystem or mod caused the issue without profiler/owned-code timer/controlled isolation evidence.
