# Performance Reference

Exact lookup for performance-sensitive FoA modding concerns.

This page records **known scaling surfaces and evidence-backed engineering patterns**. It does not assign a performance culprit or invent an FPS/frame-time budget without measurement.

## Evidence classes

Keep these separate:

| Evidence | Can establish |
| --- | --- |
| source/static inspection | work performed, loop/scanning shape, allocations visible in source, reflection location, cache/throttle design, scaling dimensions |
| bounded runtime counters | invocation rates, owned-code timing/allocation counters in the captured scenario |
| controlled baseline comparison | measured delta for the exact save/settings/scenario |
| external timeline/method profiler | method/thread/native timing attribution within the captured run |

A performance report showing low FPS does **not** by itself prove which FoA subsystem or mod caused it.

## FoA-specific high-risk scaling surfaces

### Broad MVC enumeration

`World.All<T>()` enumerates registered models of a requested type.

It is useful for:

- startup discovery;
- one-shot indexing;
- diagnostics;
- bounded operations where a full set is actually required.

Do not turn a one-shot `World.All<T>()` discovery into an unconditional per-frame scan without measuring the resulting scaling behavior.

Questline source itself uses broad enumeration in debug/startup/one-shot and some gameplay paths; that establishes the API and its semantics, not a universal performance verdict.

### Unity object and hierarchy discovery

Repeated scene-wide object searches or repeated `GetComponentsInChildren`/hierarchy discovery are high-risk when placed in `Update`, rendering, UI-refresh or combat paths.

Prefer:

~~~text
discover once
→ cache exact owner/reference
→ invalidate on the owner's lifecycle boundary
→ rediscover only when invalid
~~~

When a reference can genuinely change every frame, measure before assuming a cache is valid.

### Actor and spatial queries

Avoid replacing a native spatial owner with a global actor scan when the native service already represents the query you need.

Questline public source exposes `NpcGrid.GetHearingNpcs(position, range)` for its hearing query. That does not make `NpcGrid` a generic proximity API, but it demonstrates the broader principle: use the narrow native owner when its semantics match instead of rebuilding a global search.

### Hot Harmony targets

Treat patches on frequently executed native surfaces as hot until their call rate is understood.

Common examples in the FoA mod corpus include:

- `HealthElement.OnDamage`;
- `HealthElement.TakeDamage`;
- audio playback surfaces such as `FMODManager.PlayOneShot(...)`;
- HUD/update refresh methods;
- movement/update methods.

A hot patch should normally:

1. exit immediately when the feature does not apply;
2. avoid repeated reflection/type discovery;
3. avoid unbounded LINQ/enumeration;
4. avoid file I/O;
5. avoid normal-play log formatting/spam;
6. bound any diagnostic capture.

The fact that a patch is a Prefix/Postfix is not itself a performance problem; the work and invocation frequency are what matter.

## Event-first and lifecycle-first alternatives

The FoA MVC/event system gives several alternatives to repeated discovery:

- model/event listeners;
- `Model.Events` lifecycle events;
- subsystem-specific events;
- initialization/restore hooks;
- scene/domain readiness events.

For state that changes on identifiable native transitions, prefer reacting to that transition over polling the whole world to rediscover the same fact.

Polling remains appropriate when no trustworthy event exists, but it should be bounded and throttled.

## Throttled polling

Useful FoA mod patterns include:

~~~text
Update
→ cheap timestamp/flag guard
→ only at interval
→ narrow lookup
→ stop or back off after success
~~~

Examples from inspected project code include:

- retrying template/hero readiness at a multi-second interval rather than performing inventory/reflection work every frame;
- throttled music/context classification;
- throttled HUD/status reference discovery;
- timestamp-gated rendering/settings checks.

Record the polling interval as part of the feature contract when it materially affects responsiveness or cost.

## Caching

Cache data whose invalidation boundary is known.

Public/private FoA examples include:

- cached `ItemEquip` classification cleared when hero/stat initialization invalidates the result;
- cached reflection `MethodInfo`/API delegates resolved once rather than per invocation;
- cached UI `RectTransform`/component references with throttled retry when missing;
- cached asset/style/texture setup initialized once on first use.

A cache without an invalidation story can trade CPU cost for stale-state bugs.

## Reflection

Reflection is usually acceptable for patch/setup or occasional compatibility bridges when performed once and cached.

Repeated `AccessTools`, `GetMethod`, `GetField`, assembly scanning or delegate creation inside a hot native callback should be treated as a static performance risk until measured.

Also treat private reflected targets as a compatibility risk independently of performance.

## Logging and diagnostics

Logging can dominate a hot path even when the gameplay calculation is trivial.

Useful patterns:

- startup/patch-registration logs only during normal play;
- log-once failure suppression;
- config-gated verbose diagnostics;
- bounded in-memory rings/row caps;
- rate-limited state samples;
- export/report formatting only on explicit request or bounded incident capture.

Do not continuously serialize CSV/JSON or write files from combat, rendering or per-frame callbacks.

## UI work

For custom HUD/UI:

- draw/rebuild only while the surface is visible;
- cache styles/textures/components;
- avoid hierarchy searches during ordinary paint/update passes;
- refresh on native data/event changes when possible;
- separate one-time discovery from per-frame geometry updates.

A visible UI may legitimately do per-frame layout/geometry work; measure that path rather than moving expensive discovery into it by accident.

## Navigation and AI

Repeated path calculation/repath and per-frame actor eligibility scans are high-risk scaling dimensions because their cost grows with actor/path complexity.

Use the owning AI/navigation system's normal update/repath boundaries where possible and measure representative actor counts before setting an update frequency.

No generic FoA-safe repath interval is established here.

## File and asset work

Avoid synchronous disk I/O or expensive asset decoding in gameplay hot loops.

Prefer:

- startup/on-demand loading;
- bounded asset counts;
- cached decoded/runtime objects;
- explicit release/cleanup;
- report/export writes outside ordinary gameplay updates.

Asset bundle size on disk is not the same as live memory cost; measure runtime memory separately.

## Profiling protocol

For a material performance claim:

1. capture the same save/settings/scenario with the target mod/feature disabled;
2. capture with only that feature enabled where practical;
3. capture the normal full mod stack with diagnostics disabled;
4. record low-overhead owned-code counters such as invocation/scan/repath/log rates;
5. escalate to an external timeline/method profiler for reproducible spikes or leaks;
6. record CPU/frame timing, managed allocation/GC behavior, scene-transition spikes and long-session memory trend as relevant;
7. rerun after an optimization to prove the delta.

Do not compare runs that materially differ in scene, game branch, graphics settings or workload and call the difference a mod regression.

## Performance-monitor reports

A bounded monitor can establish that a sampled window had particular frame/allocation/GC characteristics.

It cannot, without further attribution evidence:

- prove a native FoA subsystem caused the issue;
- prove a loaded mod caused the issue merely because it was present;
- generalize one save/hardware/settings capture to all players;
- replace a method/native profiler.

## Current proof boundary

The page combines Questline public source, public FoA mod source and internal source/performance review evidence. Most entries are **cost-shape and engineering-risk conclusions**, not measured game-wide performance claims.

No repository-wide FPS, frame-time, memory or allocation budget is asserted.

See [Public FoA Symbol Baseline](../../../research/sources/public-symbol-baseline.md) and [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
