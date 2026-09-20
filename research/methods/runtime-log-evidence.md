---
document_type: investigation
scope: loader, plugin, game and runtime log evidence
last_verified: 2026-09-20
---

# Runtime Log Evidence

Logs are evidence of recorded events in one environment. They are not a general proof that a feature is correct.

Separate the log source before drawing a conclusion.

## BepInEx loader log

Useful for establishing:

- loader startup;
- plugin discovery/load attempts;
- dependency/load-order errors;
- patching or initialization exceptions emitted through the loader.

It does not by itself prove that a gameplay target executed or that the user-visible effect is correct.

## Plugin-owned log messages

Useful for establishing:

- a known plugin code path executed;
- configuration/state observed by that plugin;
- a bounded probe saw a specific event;
- feature-level fail-closed decisions.

A message saying "patch installed" is only as trustworthy as the check that produced it. Prefer logging the exact resolved target and explicit installation result.

## Native game / Unity logs

Useful for establishing:

- native/game-side exceptions or warnings;
- Unity lifecycle/resource failures;
- messages emitted independently of the mod;
- correlation with a tested interaction.

A stack trace or warning can narrow the failure stage but does not automatically identify causality.

## Harmony diagnostics

Useful for establishing:

- target resolution;
- patch installation attempts;
- patch-chain composition where explicitly inspected.

Installation is not invocation.

Keep these claims separate:

```text
target resolved
≠ patch installed
≠ target invoked
≠ downstream behaviour correct
```

## Minimum run record

For reusable evidence, record:

- game build/version;
- Mono or IL2CPP lane;
- loader/plugin version where relevant;
- exact feature/target under test;
- start/end time or bounded test interval;
- expected observation;
- actual observation;
- log source;
- conclusion and evidence state.

## Sanitization before publication

Logs often contain material that does not belong in a public repository.

Before publishing any excerpt, remove or replace:

- user profile names;
- absolute local paths;
- Steam/library locations;
- machine names;
- account identifiers;
- save paths;
- credentials/tokens;
- unrelated mod lists or diagnostics;
- proprietary content not needed for the claim.

Prefer the smallest excerpt necessary to support the conclusion. Do not publish full logs by default.

## Interpreting absence

"No matching log line" is usually **PARTIAL**, not proof that the code never ran, unless the instrumentation is known to cover the relevant path and logging itself is verified.

## Result states

Use:

- **PASSED** when the intended bounded observation is present and the instrumentation itself is established;
- **FAILED** when the observed event contradicts the expected bounded result;
- **PARTIAL** when the log narrows the question but does not establish the whole claim;
- **BLOCKED** when the required log source is unavailable or unusable;
- **NOT_RUN** when the test was not performed;
- **NOT_APPLICABLE** when that log source does not participate in the tested lane.
