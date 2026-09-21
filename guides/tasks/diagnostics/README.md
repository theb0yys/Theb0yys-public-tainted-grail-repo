# Debugging and Diagnostics

- [Build performance telemetry without blaming a plugin](build-non-causal-performance-telemetry.md)

> **Reference page.** Use this when a known process fails and you need to identify which layer stopped working.

## What diagnostics should answer

Diagnostics should answer **which stage failed**, not generate the largest possible log.

A useful chain is:

~~~text
loader
→ plug-in
→ native target/service readiness
→ identity resolution
→ registration
→ runtime object creation
→ game-system insertion
→ presentation
→ behavior
→ persistence
~~~

Attach each diagnostic to the system being tested:

- BepInEx for plug-in load;
- Harmony for patch installation/target resolution;
- `TemplatesProvider` for template lookup;
- template registrar/loader for custom registration;
- `World` for live object creation;
- `HeroItems` / `Stock` for acquisition;
- UI system for presentation;
- save system for persistence.

## Useful evidence

Record:

- game version/build;
- Mono or IL2CPP;
- BepInEx version/build;
- plug-in GUID/version;
- target assembly hash when patch-sensitive;
- exact type/method signature;
- exact native/custom GUID;
- phase marker;
- before/after counts;
- expected system;
- observed result.

Log once at meaningful boundaries, such as plug-in load, template readiness, registration, object creation, UI presentation, and cleanup.

Avoid per-frame logging unless you are running a short targeted probe.

## Debug from the earliest failed invariant

Example:

~~~text
custom item missing from shop
1. did custom GUID resolve?
2. did Item creation succeed?
3. did RestockableStock contain it?
4. was insertion before the UI snapshot?
5. did the UI classify/render it?
~~~

Do not jump directly to icon/UI code if registration never succeeded.

A missing shop item may come from failed registration, an unresolved GUID, the wrong stock lifecycle, a stale UI snapshot, a duplicate guard, or invalid category/presentation data. Stage markers help distinguish them.

## Avoid these diagnostic mistakes

- logging every frame;
- dumping proprietary data or user paths;
- changing several subsystems before reproducing again;
- rerunning the same failed check without changing the prerequisite;
- treating a build as runtime proof;
- treating a visible object as proof that it was registered in the correct system;
- diagnosing from the last exception when an earlier warning established the real failure.

## What a useful diagnostic pass leaves behind

- one reproducible trigger;
- exact environment/version;
- first failing stage;
- expected vs observed invariant;
- smallest relevant log excerpt;
- whether the failure is source/static, load, runtime, UI, save or compatibility.

## Scope

Exact diagnostic markers vary by mod and process. Use [Testing and Evidence Status](../../../sources/evidence-standard.md) when recording formal evidence.
