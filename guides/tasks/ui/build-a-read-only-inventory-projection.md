# Build a Read-Only Inventory Projection

**Evidence status: PARTIAL.** Static ownership and a first read-only source slice are established; runtime host/focus/cleanup proof and mutation adapters remain separate.

Working lineage: [Inventory Suite: Presentation Without a Second Inventory](../../../research/case-studies/ui/inventory-truth-boundary.md).

## Goal

Build a richer inventory UI that **projects** FoA inventory truth instead of becoming a second inventory system.

Native owners include:

- `HeroItems`;
- `Item`;
- `HeroLoadout`;
- native item actions;
- Character Sheet lifecycle.

## Process

```text
read native inventory/loadout
→ project rows/workspaces/search
→ select/filter locally
→ display native truth
```

Do not implement equip/use/transfer in the first slice.

## UI ownership

Your custom UI may own:

- search;
- sort;
- selection;
- workspace layout;
- presentation state.

It should not own:

- item identity;
- equipped state;
- quantity;
- native action legality.

## Runtime proof still required

Test:

- open/close;
- focus/cursor;
- inventory refresh while open;
- stale-row handling;
- scene/load transition;
- plugin disable cleanup;
- no duplicate native state.

## Current proof boundary

Read-only ownership correctness is established. Mutation actions and complete UI host/input lifecycle remain separate proof lanes.
