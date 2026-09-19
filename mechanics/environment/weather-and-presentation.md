<!-- Canonical Wave 5 mechanic page split from docs/reference/WEATHER_ENVIRONMENT.md. -->
# Weather, Environment, Sky, Water, and World-State Ownership — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/environment/README.md) first for ownership, identities, lifecycle, and proof scope.

## How we interact with it

### Keep one truth owner

A mod may own a custom weather policy, but its consumers should receive one stable state/snapshot rather than each reading/writing unrelated provider internals.

### Separate state from rendering provider

For example:

- weather state can say "LightRain";
- Weather Maker can be one rendering provider;
- skybox can be another consumer;
- water can respond separately;
- audio can use local/native FMOD logic.

This allows a provider to be changed without redefining gameplay truth.

### Make external providers optional when possible

A rendering package should not automatically become authority over:

- FoA game time;
- native quest/weather semantics;
- skybox ownership;
- water ownership;
- AI state.

### Use exact validated provider profiles

Do not treat "Rain", "Snow", "Hail" or "Storm" as interchangeable string labels if the provider has exact profiles/assets/states.

## Why this route

The weather research went through many staged gates precisely because **provider capability is not the same as integration authority**.

It also found contradictory version assumptions for Weather Maker and corrected them through exact identity/fingerprint admission.

That produces a general rule:

> Third-party capability must be version-identified and fitted into FoA ownership; it does not become the owner because it has more features.

## What goes wrong

### Several mods write weather independently

Creates last-writer-wins behavior, flicker, stale state and impossible downstream semantics.

### Weather provider owns game time by accident

Can interfere with quests, rest, day/night systems or scene state.

### Skybox, water and weather share hidden mutable state

Makes cleanup and replacement impossible to reason about.

### Provider version assumed from folder/name

The weather research explicitly found version claims that were contradicted until exact source/runtime/bundle identity was established.

### "Unsupported family" replaced by generic visual

Can hide the fact that a real provider path exists but simply lacks a validated profile.

### Material/effect proof called weather-system proof

Loading/applying rain/fog material effects does not prove scheduler, state, world context or persistence.

## How to verify

For environment integration:

1. exact weather truth owner;
2. current time/region/biome/shelter inputs;
3. chosen state;
4. provider/profile identity;
5. weather event emitted;
6. visual application;
7. sky consumer;
8. water consumer;
9. audio consumer;
10. gameplay/AI consumers if any;
11. context/scene transition;
12. provider disable/failure fallback;
13. cleanup/restoration;
14. performance;
15. save/persistence only if the weather state is meant to be durable.

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/environment/README.md).
