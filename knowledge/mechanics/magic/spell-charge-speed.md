---
document_type: mechanic
scope: heavy magic charge-speed runtime scaling
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  persistence: NONE
last_verified: 2026-09-20
---

# Spell Charge Speed

`HeroStats.SpellChargeSpeed` is a native `LimitedStat` created by `HeroStatsWrapper.Initialize`.

The heavy magic charge state reads it for animation-state speed.

## Route

```text
HeroStatsWrapper.Initialize
→ attach non-saved SpellChargeSpeed tweak
→ MagicHeavyChargeLoop reads modified stat
```

This is preferable to rewriting animation-event timing.

## Boundary

The strongest evidence is for heavy charge timing.

Light-cast timing may still depend on animation events/FSM transitions that do not map one-to-one to this stat.

Do not market a heavy-charge stat tweak as universal spell cast-speed control without separate testing.
