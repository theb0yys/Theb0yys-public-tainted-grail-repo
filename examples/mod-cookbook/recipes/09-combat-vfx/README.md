# Recipe — Combat VFX Sidecars

**Category:** combat VFX  
**Source-path evidence:** RUNTIME_EVIDENCED  
**This public recipe:** NOT_RUN

This recipe describes the smallest safe progression from the Character Damage Observer example to a custom hit-effect system.

It intentionally does not copy the maintainer's blood/VFX implementation or asset catalog.

## Proven ownership seam

The useful runtime seam is:

~~~text
HealthElement.TakeDamage(Damage)
~~~

Use a postfix when the effect should happen after native damage handling.

## Stage 1 — filter character targets

Before spawning anything:

1. require the damaged target or HealthElement parent model to implement ICharacter;
2. reject mining, digging, harvest/resource-node and other non-character health targets;
3. decide whether damage-over-time and blocked/parried hits belong in your feature;
4. keep the native damage path untouched.

This guard is essential. Broad damage hooks can otherwise make rocks, ore and dig targets emit character effects.

## Stage 2 — resolve a world position

Prefer evidence in this order:

1. damage PositionPure when available;
2. non-zero damage Position;
3. resolved target/model world position;
4. health owner's world position;
5. fail closed if no meaningful position exists.

If your effect needs facing, prefer a meaningful damage direction and normalize it.

Do not treat Vector3.zero as a valid combat impact location.

## Stage 3 — use mod-owned effects

Good first choices:

- generated ParticleSystem;
- a small mod-owned prefab;
- a legally redistributable cooked asset bundle;
- a sidecar light or decal owned entirely by the plug-in.

Do not rewrite a global native VFX table merely to prove a local hit effect.

## Stage 4 — cap everything

Add:

- global spawns per second;
- per-target spawns per second;
- maximum active instances;
- lifetime;
- cleanup on unload/scene transition where applicable.

A damage hook can be hot. An uncapped effect layer will become a performance problem quickly.

## Stage 5 — preserve native behavior

Preferred additive shape:

~~~text
native damage
    ↓
native VFX/audio remain intact
    ↓
your bounded sidecar effect
~~~

If your feature eventually replaces native effects, prove the replacement independently and keep a fail-open/fallback path.

## Death effects are a separate lifecycle

Treat living hit, terminal hit, death event and already-dead corpse hit as separate cases.

Do not assume a normal hit hook is a complete death/corpse lifecycle.

## Avoid damage-critical target-resolution patches

Do not patch deep target-resolution logic merely to attach VFX.

The maintainer's proven VFX work explicitly keeps damage target resolution untouched.

## Validation ladder

1. observer logs only character hits;
2. one generated effect appears on one controlled hit;
3. resource nodes remain unaffected;
4. rapid hits respect caps;
5. effect lifetime/cleanup works;
6. death behavior is tested separately;
7. scene reload/unload leaves no owned objects behind;
8. package contains only redistributable assets.

That preserves the working path without publishing the finished VFX design.
