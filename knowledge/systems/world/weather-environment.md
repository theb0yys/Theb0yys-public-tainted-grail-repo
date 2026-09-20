# Weather and Environment

Use this page when you want to change **weather, rain/fog/storms, sky, water response, environmental audio, or time-dependent world presentation**.

The key rule is:

> Weather is not one renderer or one boolean. It is state consumed by several systems.

## Separate weather state from its consumers

A useful model is:

~~~text
time / region / biome / shelter context
→ weather state
→ renderer/provider
→ sky
→ water
→ fog / precipitation / lighting
→ audio
→ optional gameplay/AI consumers
~~~

If several mods independently write these systems, they can easily disagree.

## Keep one weather truth

A custom weather system should expose one stable state/snapshot that downstream consumers can read.

For example:

~~~text
weather state = LightRain

visual provider → renders rain
sky system      → adjusts sky
water           → reacts separately
audio           → plays local/native ambience
gameplay        → reads approved state if needed
~~~

Do not let each presentation subsystem invent its own weather state.

## FoA/native concerns

Research separates:

- game time/day-night;
- native `WeatherController`;
- weather audio/biome context;
- water/ocean sampling;
- sky/skybox ownership;
- region/biome/shelter context;
- downstream gameplay/AI.

Useful researched surfaces include:

- native day/night state;
- `WeatherController`;
- weather-audio biome/context;
- `WaterSurfaceSampler`;
- water raycast/layer ownership.

## External weather providers

Optional packages such as Weather Maker can supply rendering capability without becoming the owner of all FoA weather semantics.

Keep external providers optional where practical.

They should not automatically take ownership of:

- game time;
- quest/weather logic;
- sky state;
- water truth;
- AI behavior.

## Use exact provider profiles

If a provider has distinct profiles/assets for Rain, Snow, Hail, Sleet, Storm, or custom precipitation, use the exact validated profile.

Do not silently substitute a generic effect because a family has not been validated yet.

The research found contradictory provider-version assumptions until exact source/runtime/bundle identity was checked, so provider version matters.

## Scene/context changes

A clean lifecycle is:

~~~text
scene/world ready
→ determine context
→ select weather state
→ emit state change
→ consumers apply
→ scene/context changes
→ consumers reapply or release
~~~

A scene transition can invalidate presentation/resource ownership even when the weather state itself stays logically the same.

## Common mistakes

- several mods independently writing weather state;
- a visual provider accidentally controlling game time;
- sky, water, and weather sharing hidden mutable state;
- assuming provider version from a folder name;
- substituting unvalidated generic visuals for an unsupported family;
- calling a material/VFX proof a complete weather-system proof.

## How to verify

Check:

1. exact weather-state owner;
2. time/region/biome/shelter inputs;
3. selected state;
4. exact provider/profile;
5. state-change event;
6. precipitation/fog/visual application;
7. sky response;
8. water response;
9. audio response;
10. gameplay/AI consumers if used;
11. scene/context transition;
12. provider failure/disable fallback;
13. cleanup/restoration;
14. performance;
15. persistence only if weather state is deliberately durable.

## Evidence limits

The working project has extensive source/live evidence for its weather-provider integration and separation of consumers.

This page uses that work to explain the design and validation rules. It does not require or universally endorse one third-party weather package for FoA mods.
