---
document_type: mechanic
scope: cleanup before FoA native title-screen process termination
runtime: mono
evidence:
  static: DECOMPILED
  runtime: PROJECT_SPECIFIC_VALIDATION
last_verified: 2026-09-20
---

# Native Title-Screen Exit Teardown

FoA's inspected title-screen exit path is unusually important for plugin cleanup.

`TitleScreenUI.Exit` calls a Windows helper that terminates the current process directly.

That means relying only on Unity/BepInEx `OnDestroy` for critical listener/owned-actor cleanup may leave a verification gap.

## Project pattern

A plugin that owns resources needing deterministic pre-exit cleanup can patch the **exact native zero-argument title Exit** with a void Harmony Prefix:

```text
native menu Exit called
→ plugin attempts owned teardown
→ plugin logs/flushes completion
→ original native Exit body continues
→ process termination remains game-owned
```

## Rules

- never skip/replace the original Exit;
- patch only the exact reviewed native entry;
- remove only your own patch;
- make teardown idempotent/reentrant-safe;
- keep `OnDestroy` as fallback;
- do not claim this covers every OS/crash/forced-termination route.

This is lifecycle engineering, not a reason to intercept arbitrary process exit globally.
