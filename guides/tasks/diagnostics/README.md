# Debugging and Diagnostics

- [Build performance telemetry without blaming a plugin](build-non-causal-performance-telemetry.md)

> **Reference page.** Use this when a known process fails and you need to identify which layer stopped working.

## What this system is

Diagnostics should answer **which stage failed**, not generate the largest possible log.

A useful chain is:

~~~text
loader
→ plug-in
→ native target/service readiness
→ identity resolution
→ registration
→ runtime object creation
→ owner insertion
→ presentation
→ behavior
→ persistence
~~~

## Who owns it in FoA

Each diagnostic should be attached to the owner being tested:

- BepInEx for plug-in load;
- Harmony for patch installation/target resolution;
- `TemplatesProvider` for template lookup;
- template registrar/loader for custom registration;
- `World` for live object creation;
- `HeroItems` / `Stock` for acquisition;
- UI owner for presentation;
- save system for persistence.

## Important identities, types, and methods

Useful evidence fields:

- game version/build;
- Mono or IL2CPP;
- BepInEx version/build;
- plug-in GUID/version;
- target assembly hash when patch-sensitive;
- exact type/method signature;
- exact native/custom GUID;
- phase marker;
- before/after counts;
- expected owner;
- observed result.

## Where it exists in the lifecycle

Log once at meaningful boundaries.

Examples:

- plug-in loaded;
- templates ready;
- source GUID resolved;
- custom GUID registered;
- provider re-resolved custom GUID;
- `World.Add` returned a valid item;
- stock count before/after;
- UI item-list sees the item;
- cleanup executed.

Avoid per-frame logging unless the task is specifically a bounded diagnostic probe.

## How we interact with it

Debug from the earliest failed invariant.

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

## Why this route

Your successful and failed item/shop experiments show that several different failures can produce the same visible symptom.

A missing shop item may be:

- failed registration;
- unresolved GUID;
- wrong stock lifecycle;
- stale UI snapshot;
- duplicate guard;
- invalid category/presentation.

Stage markers prevent speculative fixes.

## What goes wrong

Diagnostic anti-patterns:

- logging every frame;
- dumping proprietary data or user paths;
- changing several subsystems before reproducing again;
- rerunning the same failed check without changing the prerequisite;
- treating a build as runtime proof;
- treating a visible object as ownership/registration proof;
- diagnosing from the last exception when an earlier warning established the real failure.

## How to verify

A good diagnostic pass should leave:

- one reproducible trigger;
- exact environment/version;
- first failing stage;
- expected vs observed invariant;
- smallest relevant log excerpt;
- whether the failure is source/static, load, runtime, UI, save or compatibility.

## Current proof boundary

This page describes the repository's debugging discipline. Exact diagnostic markers differ by mod/process. Use [Testing and Evidence Status](../../../sources/evidence-standard.md) when recording formal proof states.
