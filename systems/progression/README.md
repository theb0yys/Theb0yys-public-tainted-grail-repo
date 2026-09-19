<!-- Canonical Wave 5 native-system page split from docs/reference/PROGRESSION_SKILLS.md. -->
# Skills, Progression, XP, Talents, and Reversible Stat Growth

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/progression/tuning-and-effects.md).

## What this system is

FoA separates:

- action/event context that decides *why* progression occurs;
- the save-affecting XP/progression sink;
- multiplier stats;
- talent/spend gates;
- gameplay stats that can often be adjusted with reversible runtime tweaks.

A safe mod should know which layer it is changing.

## Who owns it in FoA

Important owners include:

- `ProficiencyEventListener` — maps gameplay context into proficiency XP events;
- `ProficiencyStats` — proficiency XP/state owner;
- `HeroMultStats` — progression multipliers;
- `Talent` / talent-tree owners — skill/perk spending;
- `HeroStats` / `CharacterStats` — gameplay stat effects;
- `StatTweak` — reversible runtime modifiers;
- native fireplace/UI ownership for ordinary spending availability.

## Important identities, types, and methods

Researched surfaces include:

- `ProficiencyStats.TryAddXP(ProfStatType, float)` — save-affecting proficiency XP sink;
- `ProficiencyEventListener.XPGainEvent(...)` — context mapping before the sink;
- `HeroMultStats.HeroMultStatsWrapper.Initialize(...)`;
- `HeroMultStats.ProfMultiplier`;
- `HeroMultStats.KillExpMultiplier`;
- `HeroMultStats.ExpMultiplier`;
- `Talent.AcquireNextTemporaryLevel`;
- `Talent.ApplyTemporaryLevels`;
- `RestPopupUI.SkipWeatherTime(...)` and hero before/after-rest events;
- `CharacterStatuses.AddStatus(...)`;
- `Hero.Current.HeroID` as a stable hero identity used by project-owned sidecar research.

## Where it exists in the lifecycle

### Proficiency XP

~~~text
gameplay action/context
→ ProficiencyEventListener maps source/category
→ multipliers
→ ProficiencyStats.TryAddXP
→ saved progression state
~~~

### Reversible progression effect

~~~text
native HeroStats/CharacterStats initializes
→ mod adds non-saved StatTweak
→ native gameplay consumer reads ModifiedValue
→ mod updates/removes tweak when rank/config changes
~~~

### Talent spending

~~~text
native spend gate available
→ temporary acquisition
→ confirm/apply
→ saved progression owner commits state
→ refund/respec path may later reverse it
~~~

The confirm/cancel/refund/respec lifecycle must be mapped before replacing the spend gate.

## Current proof boundary

The XP sink/context/multiplier and many stat-owner mappings are source/decompile-backed.

Keep progression changes on the native XP, proficiency and talent-spending paths documented above.
