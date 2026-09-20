# Performance

Use this page when a FoA mod touches **Update loops, broad scans, frequent Harmony hooks, UI refresh, AI/navigation, reflection, logging, or file/asset work**.

The goal is not to declare something "slow" from source alone. It is to identify code that scales badly, then measure it in a controlled scenario.

## What source review can tell you

Static/source inspection can show:

- how often a loop can run;
- whether work scales with actors, items, scene objects, or UI elements;
- repeated reflection;
- obvious allocations;
- logging/string formatting in a hot callback;
- unbounded queues or scans;
- whether caching/throttling exists.

It cannot tell you the FPS cost on a player's machine without measurement.

## Broad World scans

`World.All<T>()` enumerates registered Models of a type.

Good uses include:

- startup discovery;
- one-shot indexing;
- diagnostics;
- genuinely bounded queries.

Be cautious about putting a full `World.All<T>()` scan in every `Update` or common event callback.

When the state changes on a known event, react to the event instead of rediscovering the entire world repeatedly.

## Unity object/hierarchy searches

Repeated scene-wide searches, `Find*Object*`, or repeated `GetComponentsInChildren` calls can become expensive in frequently executed code.

Prefer:

~~~text
discover
→ cache
→ invalidate on known lifecycle change
→ rediscover
~~~

Do not cache blindly if the object's lifetime is uncertain.

## Use the native query when it matches

If FoA already maintains a narrow service for the question you are asking, prefer that over rebuilding a global scan.

For example, Questline source exposes `NpcGrid.GetHearingNpcs(position, range)` for the hearing system's spatial query.

That does not make `NpcGrid` a universal proximity API; it shows why you should use the native owner when its semantics match.

## Frequent Harmony hooks

Treat commonly executed targets as hot until you understand their invocation rate.

Examples include:

- `HealthElement.OnDamage`
- `HealthElement.TakeDamage`
- `FMODManager.PlayOneShot(...)`
- HUD/update refresh methods
- movement/update methods

A hot patch should usually:

1. exit immediately when the feature does not apply;
2. avoid repeated reflection;
3. avoid unbounded LINQ/enumeration;
4. avoid file I/O;
5. avoid routine log formatting;
6. cap diagnostics.

Prefix vs Postfix is not the performance question. The amount of work and call frequency are.

## Polling

Polling is fine when no useful event exists, but keep it cheap and bounded.

A common pattern is:

~~~text
Update
→ cheap flag/timestamp check
→ return on most frames
→ narrow lookup at an interval
→ stop/back off after success
~~~

Inspected FoA mods use this for template readiness, Hero readiness, music context, HUD reference discovery, and settings/render checks.

If the polling interval affects responsiveness, document it.

## Caching and invalidation

Useful cache examples include:

- reflection metadata resolved once;
- `ItemEquip` decisions cleared when Hero/stat state is rebuilt;
- UI component/`RectTransform` references cached with throttled retry;
- textures/styles/assets initialized once on first use.

A cache is only correct when you know what invalidates it.

## Reflection

Reflection is often fine during startup, patch registration, or an occasional compatibility bridge.

Repeated `AccessTools`, `GetMethod`, `GetField`, assembly scanning, or delegate creation inside a damage/audio/UI/update hot path should be treated as a performance risk until measured.

Private reflection is also a compatibility risk even if its runtime cost is small.

## Logging and diagnostics

Logging can become the most expensive part of an otherwise tiny hook.

Prefer:

- startup/registration logs;
- log-once failures;
- config-gated verbose diagnostics;
- bounded ring buffers or row caps;
- rate-limited samples;
- explicit report/export actions.

Do not continuously write CSV/JSON/log files from combat, rendering, or per-frame callbacks.

## UI

For HUD and custom UI:

- only draw/rebuild while visible;
- cache styles/textures/components;
- keep hierarchy discovery out of ordinary paint/update paths;
- refresh from native events where practical;
- separate one-time discovery from per-frame geometry.

Per-frame layout work can be legitimate. Measure it instead of mixing discovery, asset loading, and reflection into the same path.

## AI and navigation

Pathfinding/repath cost and per-frame actor eligibility checks can scale with actor count and navigation complexity.

Use the game's normal AI/navigation lifecycle where possible.

There is no universal "safe FoA repath interval" documented here; choose one from representative measurement.

## File and asset work

Avoid synchronous file I/O or expensive asset decoding in gameplay hot loops.

Prefer:

- startup or on-demand load;
- bounded asset counts;
- cached runtime objects;
- explicit cleanup/release;
- report writes outside ordinary gameplay updates.

File size on disk does not tell you live memory cost.

## How to measure a real regression

For a material performance claim:

1. use the same save, scene, graphics settings, and workload;
2. capture a baseline with the target feature disabled;
3. capture with only that feature enabled where practical;
4. capture the normal mod stack with diagnostics disabled;
5. record low-overhead owned-code counters such as invocation/scan/repath rates;
6. use an external timeline/method profiler for reproducible spikes or leaks;
7. record the CPU/frame/allocation/GC/memory metrics relevant to the issue;
8. rerun after the optimization.

Do not compare materially different scenarios and call the difference a mod regression.

## Performance monitors

A bounded monitor can prove what happened during the sampled window.

It cannot by itself prove:

- which native FoA subsystem caused the issue;
- which loaded mod caused the issue;
- that one hardware/save/settings result applies to everyone;
- the exact method responsible without attribution/profiling evidence.

## Evidence limits

This page combines Questline public source, public FoA mod source, and internal source/performance review.

Most guidance here describes **cost shape and scaling risk**, not measured game-wide performance.

No repository-wide FPS, frame-time, memory, or allocation budget is asserted.

See [Public FoA Symbol Baseline](../../../research/sources/public-symbol-baseline.md) and [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
