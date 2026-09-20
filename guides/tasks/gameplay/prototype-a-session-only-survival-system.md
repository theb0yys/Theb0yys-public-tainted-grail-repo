# Prototype a Session-Only Survival System

Build the survival model from native activity observations and runtime-only StatTweak effects. Do not start by changing FoA save serialization.

Working lineage: [Session First, Persistence Later](../../../research/case-studies/survival/session-first.md).

## Runnable source

Start from the buildable example: [Session survival](../../../examples/mono/gameplay/session-survival/README.md). Build it unchanged first, confirm the documented behavior, then make one change at a time.


## Movement input

The maintained Tainted Survival implementation observes:

~~~text
ProficiencyEventListener.XPGainEvent
~~~

with a Harmony prefix.

The prefix receives:

- ProfStatType proficiencyToLevel;
- BaseXPType targetXPType;
- float xpGainParameter.

It filters to movement sources, respects ProficiencyGainBlockerModel, reads whether Hero.Current is in combat, then calls the session model:

~~~csharp
FatigueDryRunModel.RecordMovement(
    proficiencyName,
    sourceName,
    xpGainParameter,
    heroInCombat);
~~~

This reuses activity FoA has already classified instead of polling player velocity every frame.

## Rest input

Observe:

~~~text
RestPopupUI.SkipWeatherTime(...)
~~~

with a postfix.

The working signature supplies:

~~~text
Hero hero
GameRealTime gameRealTime
float hourValue
bool isSafelyResting
~~~

Convert hourValue to minutes and pass it to:

~~~csharp
FatigueDryRunModel.RecordRest(
    minutes,
    isSafelyResting,
    heroPresent);
~~~

Do not replace the native rest action.

## Other useful session observations

The same model can consume bounded events for:

- wound/damage shock;
- food/preparation;
- alcohol;
- disease/curse pressure;
- weather/night pressure;
- camp-preparedness services.

Keep each input as an observation that updates one mod-owned fatigue state.

## Session state

A small implementation only needs:

~~~text
current fatigue value
current fatigue stage
temporary buffers/recovery state
last relevant event timestamps
~~~

Reset/rebind when the hero/session changes.

Do not persist the first version.

## Apply runtime effects through current stat owners

The working implementation patches both:

~~~text
CharacterStats.CharacterStatsWrapper.Initialize
HeroStats.HeroStatsWrapper.Initialize
~~~

CharacterStats effects target:

- SprintCostMultiplier;
- StaminaUsageMultiplier.

HeroStats effects target:

- EncumbranceLimit;
- SpellChargeSpeed.

Each effect is a mod-owned runtime StatTweak attached to the current hero/stat instance.

When the fatigue stage or config changes:

~~~text
Hero.Current
→ current CharacterStats/HeroStats
→ discard/update owned tweaks
→ apply multipliers for current fatigue stage
~~~

When the system is disabled, reapply neutral values/remove the owned tweaks.

## Overlay

Render the current fatigue value/stage from the mod-owned session state.

The overlay should read the model; it should not become the model.

## A clean first feature

A useful first survival slice is:

~~~text
movement practice increases fatigue
→ safe rest reduces fatigue
→ current stage affects sprint/action stamina
→ HUD shows current stage
→ reload resets session state
~~~

That is already enough to test the mechanic without inventing a save format.
