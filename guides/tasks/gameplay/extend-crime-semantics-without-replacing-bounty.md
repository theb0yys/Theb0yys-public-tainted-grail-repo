# Extend Crime Semantics Without Replacing Native Bounty

**Evidence status: PARTIAL.** Native crime/witness/bounty ownership is established; the richer incident/report/case layer is still a first-phase passive/session-only design.

Working lineage: [Preserve Native Bounty, Extend Semantic Truth](../../../research/case-studies/crime/preserve-native-bounty.md).

## Goal

Add richer semantic tracking around crime without replacing FoA's authoritative legal state.

Keep native:

- crime entry;
- witness reactions;
- pending reporting;
- bounty storage;
- `CrimeUtils.AddBounty`.

Add mod-owned interpretation around that path.

## Architecture

```text
native crime happens
→ native witness/report/bounty flow remains authoritative
→ mod observes event/context
→ mod records incident/witness/report/case metadata
→ overlay/diagnostics consume mod-owned semantic state
```

## Process

1. Observe one exact native crime event.
2. Record only mod-owned metadata.
3. Do not rewrite the native bounty amount/state.
4. Keep incident IDs separate from native identities.
5. Start session-only.
6. Add persistence only after the semantic model is stable.

## Why

A native system can be authoritative yet still not model every concept your mod wants.

That does not justify duplicating its authoritative state.

## Verification

Prove:

- native bounty still changes normally;
- witnesses/reporting still behave natively;
- mod incident record matches the observed event;
- disabling the semantic layer leaves bounty/legal state intact;
- no duplicate incidents from one native event.

## Current proof boundary

The owner split is established. Durable case files, saved witness attribution, migration and a full runtime matrix remain unproven.
