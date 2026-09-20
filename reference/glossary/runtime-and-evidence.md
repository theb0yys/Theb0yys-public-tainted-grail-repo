# Runtime and Evidence Terms

## Runtime lanes

**Mono** — FoA managed runtime lane used with BepInEx 5.

**IL2CPP** — FoA IL2CPP runtime lane used with BepInEx 6 / Il2CppInterop.

**Merlin** — authoring/content lane based on Questline's official Merlin's Workshop Unity project.

**Hybrid / dual-runtime** — a mod or starter that deliberately spans more than one lane while keeping the lane-specific host/authoring boundaries explicit.

## Validation terms

**Static/source inspected** — a claim supported by source, metadata, decompilation or repository inspection without live target execution.

**Build validated** — the intended project/source compiled successfully for the stated target.

**Loader validated** — the packaged plugin/dependency loaded successfully under the stated loader/runtime.

**Runtime proven** — the exact claimed behavior was directly observed in the target runtime.

**Persistence proven** — the exact durable behavior survived the stated save/load/restart sequence.

**Compatibility tested** — the claimed behavior was separately validated on every stated build/runtime combination.

**Release validated** — the actual packaged release artifact, not merely the development tree, was tested.

See [Public Evidence Standard](../../sources/evidence-standard.md).

## Failure-state terms

**Fail closed** — when prerequisites/evidence are missing, the mod or process refuses the unsafe/unsupported action.

**Fail open** — for a replacement/interception path, failure leaves the native behavior running rather than suppressing it. This is useful for bounded replacement hooks such as audio.

**Blocked** — a required prerequisite/gate is unresolved for the exact operation being discussed.

**Not run** — the validation/action was not executed.

**Partial** — only part of the stated claim has supporting evidence; the unsupported remainder must stay explicit.
