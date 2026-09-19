# Recipe — Skill Caps and Progression

**Category:** progression / skills  
**Source-path evidence:** LOAD_EVIDENCED  
**Drop-in public mini-mod:** deliberately not provided

The owner-side Skills Uncapped path reached plugin-load/config evidence, but feature-level cap/XP/save testing remained pending.

A correct skill-cap mod is more than changing one number. The inspected path has to coordinate:

- `ProficiencyStats.ProficiencyStatsWrapper.Initialize`;
- `ProficiencyStats.TryAddXP`;
- `ProficiencyStats.GetProgressToNextLevel`;
- each resolved `LimitedStat` upper limit;
- FoA's stored per-skill XP dictionary;
- level-up XP rewards and notifications;
- optional lowering of an already-over-cap current skill;
- save persistence after intentional lowering.

## Why there is no 30-line example

If you only raise `LimitedStat._upperLimit` but leave the native XP path assuming a different limit, UI progress, stored XP and leveling behavior can disagree.

If you replace `TryAddXP`, you now own enough of the native progression algorithm that mistakes can affect saves.

## Safe learning path

1. Start read-only: resolve `ProfStatType.HeroProficiencies` and log current values/limits.
2. On a disposable save, change **one** skill cap.
3. Verify XP gain below, at and above the old 100 limit.
4. Verify progress UI.
5. Save/reload.
6. Only then add runtime config reapply.
7. Treat "lower current skills to cap" as a separate destructive feature with an explicit opt-in.

Do not copy the full owner-side implementation as a generic snippet. Its value is the proven dependency map, not a promise that arbitrary progression rewrites are safe.
