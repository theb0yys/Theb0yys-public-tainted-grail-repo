# Gate 2 Validation Record

Gate 2 adds the configurable Mono/BepInEx 5/Harmony runtime tracer.

This record separates repository/source validation from runtime proof.

## Acceptance checks

| Check | State | Evidence / limit |
| --- | --- | --- |
| Gate 2 diff contains only runtime-tracer implementation/integration | **PASSED** | Compare against Gate 1 head before promotion. |
| Public-surface guard | **IN_PROGRESS** | Must pass on the final Gate 2 branch head. |
| No configured target installs no patch | **PASSED (source review)** | `General.Enabled=false` by default; enabled + no target returns before Harmony creation. |
| Exact target identity / overload ambiguity fails closed | **PASSED (source review)** | Assembly, full declaring type and declared method are resolved explicitly; ambiguous overloads require `ParameterTypeNames`. |
| Explicit zero-parameter overload selection | **PASSED (source review)** | `ParameterTypeNames=<none>` selects an exact zero-parameter signature. |
| Observation does not mutate arguments/result/control flow | **PASSED (source review)** | Prefix/postfix/finalizer use read-only injections; finalizer does not replace the exception. |
| Re-entry suppression / output bounds / rate limit | **PASSED (source review)** | Callback depth guard, bounded renderer and fixed-window limiter are implemented. |
| Harmony 2 manual patch and patch-info API shape | **PASSED (API review)** | Implementation matches Harmony 2 documented `Patch(...)`, `GetPatchInfo(...)` and prefix/postfix/finalizer ownership inspection. |
| BepInEx 5 config/log API shape | **PASSED (API review)** | Implementation matches BepInEx 5 documented `Config.Bind`, `ConfigDescription`, acceptable ranges and `ManualLogSource`. |
| Compile against actual local BepInEx/Harmony/Unity references | **BLOCKED** | Requires a local Mono/BepInEx 5 FoA installation; no proprietary/local reference set is available in this execution environment. |
| Self-owned runtime tracer smoke test | **NOT_RUN** | Requires loading the built plug-in in a Mono/BepInEx 5 runtime. |
| Real FoA target runtime observation | **NOT_RUN** | Must follow static owner/identity investigation and an explicitly selected safe target. |
| Persistence validation | **NOT_APPLICABLE** | Gate 2 tracer is observation-only and makes no persistence claim. |
| IL2CPP parity | **NOT_APPLICABLE** | Separate later gate. |

## Runtime promotion requirement

Gate 2 is not runtime-proven until all of the following are recorded from one identified environment:

1. exact game version/build;
2. Mono/BepInEx 5 lane;
3. successful local build;
4. `targetResolution=PASSED` for the self-owned target;
5. `patchInstallation=PASSED`;
6. `selfTestRuntimeObservation=PASSED`;
7. unchanged self-test result (`Ping(41) == 42`);
8. the relevant bounded log excerpt, sanitized before publication if retained.

A later real-game observation must be recorded separately and must not be inferred from the self-test.

## Gate state

**PARTIAL**

Implementation and source/API review are present. Runtime promotion is blocked on an installed Mono/BepInEx 5 game environment.
