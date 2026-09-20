# Build a Separate Practice/Progression Overlay

FoA already has character level, proficiencies, RPG stats and talent trees. If you want another progression layer, build it from observed practice instead of pretending every visible talent group is a hidden vanilla proficiency.

Working lineage: [Visible Talent Group ≠ Native Proficiency](../../../research/case-studies/progression/visible-tree-vs-proficiency.md).

## Record actual practice

The maintained Immersive Progression implementation funnels observed activity into:

~~~csharp
PracticeLedger.Record(
    target,
    source,
    sourceParameter,
    sourceMultiplier,
    heroInCombat,
    contextTags,
    xp);
~~~

The ledger groups records by target, source, context and theme.

Existing recording routes include:

- native proficiency XP through ProficiencyStats;
- damage-related proficiency context;
- trade transactions;
- recipe learning;
- fishing catches;
- medicine/buff item use;
- player-sourced status application;
- world/mining actions.

Use native events that already occurred. Do not increment a branch every Update frame.

## Keep a mod-owned ledger

A minimal overlay needs:

~~~text
practice bucket key
event count
accumulated practice/XP
theme/category
current session hero identity
~~~

The maintained ledger clears/rebinds when the tracked hero session changes.

## Convert practice at a deliberate boundary

Immersive Progression uses safe rest as one conversion boundary.

Rest is observed through:

~~~text
RestPopupUI.SkipWeatherTime(...)
~~~

and the ledger calls:

~~~csharp
PracticeLedger.SummarizeAndClearAfterRest(
    minutes,
    isSafelyResting,
    heroPresent,
    shouldLog);
~~~

That gives you a natural point to summarize recent practice, award mod-owned branch progress, or feed a carefully selected native proficiency route.

## Visible talent labels are presentation categories

The Character Sheet shows groups such as:

~~~text
Two Handed
One Handed
Attack Speed
Critical Hits
Daggers
Crafting & Trading
Armor
...
~~~

Do not assume each label maps to a ProfStatType.

Map observed native sources into your own stable branch IDs.

## Native talent spending remains native

FoA already checks normal talent/RPG-stat spend availability through:

~~~text
TalentTreeBase.IsUpgradeAvailable
~~~

which requires active FireplaceUI context for the ordinary Character Sheet flow.

A separate progression overlay does not need to replace that gate.

## Rendering the overlay

Render:

- branch name;
- accumulated practice;
- rank/progress;
- recent contributing activities;
- any mod-owned unlock currency.

Rebuild from your ledger/state rather than storing native UI objects.

## Optional native effects

If a branch applies a gameplay effect, use an already identified native stat owner and a non-saved runtime tweak.

For example, existing branch work maps weapon-practice ranks to weapon-specific HeroStats attack-speed stats rather than creating a fake vanilla proficiency.

Keep the branch state and the native runtime effect as two separate pieces.
