# Prototype a Session-Only Survival System

**Evidence status: PARTIAL.** The staged session model is established; durable persistence is deliberately not part of this first guide.

Working lineage: [Session First, Persistence Later](../../../research/case-studies/survival/session-first.md).

## Goal

Test whether a survival mechanic is fun and technically stable **before** inventing a save schema.

## Staged path

```text
observe native rest/movement/damage/status/environment
→ session-only fatigue model
→ passive overlay
→ optional non-saved stat effects
→ food/rest/environment integration
```

## Process

1. choose one session variable, e.g. fatigue;
2. define exact native observations that increase/decrease it;
3. keep the value in mod-owned session memory;
4. expose a read-only overlay;
5. optionally apply one reversible non-saved native tweak;
6. reset cleanly on new session/reload.

## Do not start with

- save migration;
- native save-domain changes;
- dozens of needs/meters;
- permanent debuffs;
- broad food/rest rewrites.

## Verification

Measure:

- event counts;
- accumulation rate;
- rest recovery;
- scene/session reset;
- overlay accuracy;
- non-saved tweak cleanup;
- no duplicate subscriptions.

## Current proof boundary

This teaches the staged architecture. Persistence, migration and a complete balance/feel matrix remain outside the current proof.
