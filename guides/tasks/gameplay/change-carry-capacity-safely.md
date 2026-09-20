# Change Carry Capacity Safely

**Evidence status: PARTIAL.** The stale-owner bug and corrected reattachment strategy are established; a complete load/transition matrix is not recorded here.

Working lineage: [Carry Tweak Must Follow the Current Stat Instance](../../../research/case-studies/stats/carry-stale-tweak.md).

## Problem

A tweak object can still exist while no longer affecting gameplay because FoA rebuilt the underlying stat instance.

Research found that `HeroStatsWrapper.Initialize` can rebuild `EncumbranceLimit`.

## Process

```text
hero stats initialized/reinitialized
→ get current EncumbranceLimit instance
→ discard old mod helper/tweak
→ create fresh non-saved tweak
→ attach tweak to current stat
```

Do not cache one stat reference forever.

## Rules

- keep the tweak non-saved unless persistence is deliberately designed;
- track which stat instance the tweak belongs to;
- on reinitialization, clean up the old helper;
- attach only to the current owned stat;
- avoid duplicate stacking.

## Verification

Test:

- baseline carry capacity;
- tweak applies;
- hero/stat reinitialization;
- old tweak is not left attached to stale state;
- new stat receives exactly one tweak;
- save/load/scene transitions do not accumulate duplicates;
- disabling the mod returns to baseline after recreation/reload as appropriate.

## Current proof boundary

The owner replacement and reattachment correction are established. Full transition/save validation for the public implementation still needs to be run.
