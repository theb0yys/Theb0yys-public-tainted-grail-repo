# Build Robust Game Changes

Advanced modding is not just doing more complicated things. It is controlling more failure modes with better evidence.

## IL2CPP

For IL2CPP, debug in this order:

1. native loader entry;
2. BepInEx chainloader startup;
3. interop generation/loading;
4. plug-in discovery;
5. target type resolution;
6. gameplay behaviour.

Start from an established lower-layer path before adding the advanced patch.

Generated interop state can change after game updates. A source-compatible plug-in can still fail because the runtime representation changed.

## Patch design

Prefer the smallest stable method that represents the behaviour you need.

Before changing a result, understand what the original method guarantees:

- state transitions;
- collection consistency;
- callbacks/events;
- side effects;
- save-facing state.

A patch that produces the expected immediate result but breaks an invariant can fail later somewhere unrelated.

Fail closed when assumptions are wrong. Disable the feature with a useful error instead of guessing.

## Mod conflicts

Assume other mods may patch the same method.

Where practical:

- avoid replacing entire methods;
- preserve original behaviour;
- keep patches narrow;
- log enough identity/version information to diagnose conflicts.

## Diagnostics

A serious mod should make it easy to determine:

- mod version;
- game/runtime environment;
- active features;
- missing dependencies;
- first meaningful failure.

## Packaging and release

Ship only what users actually need.

Do not include:

- local build caches;
- game binaries;
- generated interop assemblies unless redistribution is clearly authorized and actually required;
- saves;
- private paths;
- credentials;
- unrelated tools.

Before calling a release working, test the actual packaged artifact through the intended install/load path.

## Content pipelines

Advanced content work should still be staged.

The repository currently separates:

- items;
- weapons;
- armour;
- creatures/NPCs.

Read how-to/README.md and the relevant pipeline document.

Keep evidence levels separate:

- static/source confirmation;
- editor validation;
- game runtime validation;
- packaging/release validation.

Do not turn one level into another by wording.

## When to build shared systems

Move to [Build Reusable Mod Systems](../reusable-systems/README.md) when you have repeated problems across multiple features or mods and can identify a real shared contract worth maintaining.
