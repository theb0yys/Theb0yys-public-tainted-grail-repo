---
document_type: mechanic
scope: selecting a FoA intervention seam
game_build: mixed
runtime: both
evidence:
  static: SUPPORTED
  runtime: MIXED
last_verified: 2026-09-20
---

# Intervention Selection

The safest recurring FoA pattern is **native-owner first**.

Do not begin with “what method can I patch?” Begin with:

1. What exact thing or state am I trying to change?
2. Which game system owns that state or transition?
3. When is that owner ready?
4. Which downstream system consumes the result?
5. What cleanup/restoration does the native path normally perform?

Then choose the smallest seam that preserves those owners.

## Practical matrix

- [Intervention seam selection matrix](seam-selection-matrix.md)

## Three common shapes

### Adjust a result

Let native logic run, then modify a bounded returned value.

Good examples include price or velocity calculations.

### Guard an action

Check a mod-owned condition before allowing the normal native action.

Good examples include conditional pickup/theft handling.

### Attach a sidecar

Observe a native lifecycle and add mod-owned presentation, telemetry or optional behaviour without replacing the native owner.

Good examples include damage VFX, receipts or audio overlays.

## Stop conditions

Do not escalate to a broader patch when:

- the current failure belongs to a different owner;
- the target is not ready yet;
- the downstream consumer never sees your result;
- the visible symptom is presentation-only;
- cleanup/restoration is unknown;
- static evidence exists but runtime behaviour has not been proven.

Use [Finding the native owner](../../investigate/finding-the-native-owner.md) when ownership is unclear.
