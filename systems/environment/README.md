<!-- Canonical Wave 5 native-system page split from docs/reference/WEATHER_ENVIRONMENT.md. -->
# Weather, Environment, Sky, Water, and World-State Ownership

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/environment/weather-and-presentation.md).

## What this system is

A visible "weather system" is usually several owners working together:

~~~text
weather truth/state
→ scheduler/context
→ native/external weather renderer
→ sky/skybox presentation
→ water/ocean response
→ lighting/fog/precipitation/material effects
→ audio
→ downstream AI/gameplay consumers
~~~

These layers should not all write the same state independently.

## Who owns it in FoA

Research identifies separate native/project ownership concerns for:

- FoA game time/day-night;
- native weather state/controller;
- weather visual provider/runtime;
- sky/skybox renderer;
- water/ocean truth and sampling;
- weather audio;
- world/region/biome/shelter context;
- downstream AI/gameplay consumers.

The working weather architecture intentionally assigns one **weather-truth owner** and lets other systems consume its state rather than each system inventing weather independently.

## Important identities, types, and methods

Research/source surfaces include:

- native day/night system;
- native `WeatherController`;
- native weather-audio biome/context systems;
- water sampling via `WaterSurfaceSampler`;
- water raycast/layer ownership;
- weather-event/state APIs;
- optional Weather Maker runtime/provider integration;
- separate skybox and water consumer contracts.

Weather Maker research also found first-class precipitation paths for:

- rain;
- snow;
- hail;
- sleet;
- custom precipitation.

That means unsupported/unfinished weather families should not be silently routed through arbitrary visual fallbacks if a real provider path exists but has not yet been validated.

## Where it exists in the lifecycle

A clean environment stack is:

~~~text
world/scene available
→ determine region/biome/shelter/time context
→ weather authority selects state
→ state-change event
→ visual/audio/water/sky consumers plan/apply
→ gameplay consumers read approved state
→ scene/context changes
→ consumers reapply/release
~~~

## Current proof boundary

The working project has extensive gated source/live evidence for its weather-provider stack and consumer separation.

This handbook uses that research to teach ownership and validation rules, not to declare a particular third-party weather package a required FoA modding dependency.
