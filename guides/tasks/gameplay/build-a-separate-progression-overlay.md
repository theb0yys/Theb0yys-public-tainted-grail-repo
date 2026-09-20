# Build a Separate Progression Overlay

**Evidence status: PARTIAL.** Research establishes that visible talent-group labels do not map one-to-one to native proficiencies. The separate overlay model still needs full runtime/persistence validation.

Working lineage: [Visible Talent Group ≠ Native Proficiency](../../../research/case-studies/progression/visible-tree-vs-proficiency.md).

## Problem

A UI group named "Daggers", "Critical Hits" or "Attack Speed" does not prove there is a hidden vanilla XP/proficiency store with the same identity.

## Safe design

When native practice/event evidence exists but no matching native progression owner exists:

```text
observe confirmed native practice
→ update mod-owned branch/progression state
→ render mod-owned overlay
```

Do not invent a fake vanilla proficiency and write it into unrelated native stats.

## Process

1. define your own progression identity namespace;
2. document which native events contribute;
3. keep accumulation mod-owned;
4. render the overlay distinctly;
5. add persistence only after the session model is proven.

## Verification

Prove:

- correct native activity increments the intended branch;
- unrelated activity does not;
- vanilla talents/stats are not mutated accidentally;
- UI clearly distinguishes mod progression;
- reset/disable behaviour is defined.

## Current proof boundary

The taxonomy correction and separate-overlay architecture are established. End-to-end runtime and durable persistence remain partial.
