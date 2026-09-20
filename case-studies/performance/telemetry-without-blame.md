# Telemetry Without Blame

Tainted Performance's strongest reusable design decision is epistemic rather than graphical:

**plugin presence is context, not causality.**

The monitor can record frame timing, GC/allocation pressure and the loaded stack while keeping two investigation lanes open:

- native/engine baseline;
- mod-stack isolation.

## Lesson

A diagnostic tool becomes less trustworthy when it jumps from “these things were present” to “this thing caused the frame drop”.

Keep observation, correlation, controlled isolation and causality as distinct proof levels.
