---
document_type: case
scope: Tainted Performance evidence model
runtime: il2cpp
evidence:
  source: SOURCE_REVIEWED
  overhead: OUTSTANDING_IN_BASELINE_RESEARCH
last_verified: 2026-09-20
---

# Telemetry Without Blame

Tainted Performance's strongest reusable design decision is epistemic rather than graphical:

**plugin presence is context, not causality.**

The monitor can record frame timing, GC/allocation pressure and the loaded stack while keeping two investigation lanes open:

- native/engine baseline;
- mod-stack isolation.

## Lesson

A diagnostic tool becomes less trustworthy when it jumps from “these things were present” to “this thing caused the frame drop”.

Keep observation, correlation, controlled isolation and causality as distinct proof levels.
