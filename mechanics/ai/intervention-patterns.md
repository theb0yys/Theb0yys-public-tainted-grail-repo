<!-- Canonical Wave 5 mechanic page split from docs/reference/AI_PERCEPTION_BEHAVIOR.md. -->
# AI, Perception, Combat Pressure, and Native Behavior Ownership — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/ai/README.md) first for ownership, identities, lifecycle, and the current technical proof boundary.

## How we interact with it

### Modify the native fact/parameter closest to the desired behavior

Examples from the research:

- existing guard pursuit refresh → `NpcAI.AlertStack.NewPoi(...)`;
- target-loss duration → `NpcAIDistancesUtils`;
- attack cadence for a reviewed creature profile → native behavior cooldown result;
- whether a melee behavior requires a combat slot → exact behavior getter;
- global simultaneous attack pressure → difficulty/combat-director inputs.

Do not replace perception, movement and combat together when one owner already exposes the needed control.

### Preserve native group alert/search when it already exists

Research found `StateAlert` and `StateAlertWander` already perform nearby-friendly alert/movement behavior.

A mod should not duplicate group alerts or search movement merely because it can.

### Distinguish observation from command authority

A blackboard/GOAP/project framework can know facts or choose a desired action without thereby owning:

- native movement;
- damage;
- animation;
- target selection;
- actor creation/destruction.

Each command needs an explicit executor/owner contract.

## Why this route

Several AI investigations found the "obvious" replacement was unnecessary.

### Guard pursuit

A private crime helper ultimately refreshed the native alert stack. The better integration point is the public/native alert owner, not reflection into the wrapper.

### Lost-target pursuit

Native `NpcAI.ExitCombat` already includes lost-view delay and close-range rules.

Use the native search/disengagement timer; do not add a parallel generic search timer.

### Search-for-criminal state

`StateSearchForCriminal.Update` was empty after the preceding investigation state, which exposed a **specific gap/boundary** where bounded search movement could be added without replacing perception or combat.

This is the correct use of a gap: extend the missing behavior, do not seize the whole AI stack.

## What goes wrong

### Replacing the whole AI because one behavior is weak

Creates ownership conflicts over movement, perception, combat, animations, death and cleanup.

### Template/name taxonomy treated as behavior proof

An NPC template name does not prove its active AI package, spawn route or runtime state.

### Duplicating native alerts

Can multiply pressure and create runaway group reactions.

### Omniscient post-LOS pursuit

Refreshing a visible-target pursuit path after line-of-sight is lost can accidentally create impossible knowledge.

The crime research explicitly separates visual escape, pursuit escape, report escape and legal escape.

### Combat-slot removal generalized across all creatures

Creature-specific tuning patches are evidence for those profiles, not a universal AI rule.

### Third-party AI framework attached to native actors without ownership transfer

If native and external systems both drive movement/animation/targeting, state can fight every frame.

## How to verify

For an AI modification:

1. exact actor/template/profile;
2. current native state;
3. perception/target owner;
4. movement owner;
5. combat owner;
6. exact parameter/hook changed;
7. behavior before/after;
8. line-of-sight/target-loss behavior;
9. combat enter/exit;
10. group alerts;
11. death/disable/scene cleanup;
12. no duplicate driver;
13. save impact remains as designed.

For profile-specific tuning, test only the approved exact actors first.

## Evidence boundary

This Wave 5 split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/ai/README.md).
