# Runtime Tracer — Gate 2

A source-only, configurable **Mono / BepInEx 5 / Harmony** runtime observation tool.

Its purpose is narrow: after static investigation identifies an exact method, this plug-in can establish whether that method resolves, whether the tracer patches it, and whether the target is observed executing in one recorded environment.

It does not establish persistence, broad compatibility, semantic equivalence after a game update, or release readiness.

## Runtime support

| Lane | State |
| --- | --- |
| Mono / BepInEx 5 | **IMPLEMENTED — runtime validation still required on an installed game** |
| IL2CPP | **NOT_APPLICABLE in Gate 2** |

IL2CPP tracer parity is a later, separately validated gate.

## Safety model

The tracer is deliberately fail-closed:

- `General.Enabled=false` by default;
- no configured target means **no Harmony patch**;
- the target assembly, full declaring type and method must resolve exactly;
- an overloaded method requires an exact parameter signature;
- only methods declared on the configured type are selected;
- constructors and open generic methods are not supported;
- no target is guessed when identity is ambiguous;
- the tracer observes only—it does not change arguments, results, control flow or exceptions;
- log callbacks suppress re-entry;
- logs are rate-limited;
- arbitrary object `ToString()` implementations are not invoked;
- argument and result capture are off by default;
- values are bounded before logging;
- the plug-in writes no custom files.

BepInEx may create its normal configuration and log files.

## Build

Use only references from your own local Mono/BepInEx 5 installation:

```powershell
dotnet build RuntimeTracer.csproj -c Release -p:FoAGameRoot="<GameRoot>"
```

Or set `TAINTED_GRAIL_FOA_ROOT` and omit the MSBuild property.

Do not copy BepInEx, Harmony, Unity or game DLLs into this repository.

## First run: self-owned smoke target

Before tracing a game method, use the plug-in's own test method.

Generated BepInEx configuration:

```ini
[General]
Enabled = true

[SelfTest]
Enabled = true

[Capture]
TraceArguments = true
TraceResult = true
```

This patches only:

```text
TGCommunity.RuntimeTracer::
TGCommunity.RuntimeTracer.SelfTestTarget.Ping(System.Int32)->System.Int32
```

and invokes it once.

A successful runtime should report:

```text
targetResolution=PASSED
patchInstallation=PASSED
selfTestRuntimeObservation=PASSED
```

That proves the tracer's own Mono/Harmony observation path in that environment. It does not prove any FoA game method.

## Configure a game target

Disable self-test and configure an exact identity:

```ini
[General]
Enabled = true

[SelfTest]
Enabled = false

[Target]
AssemblyName = TG.Main
TypeName = Example.Namespace.ExampleOwner
MethodName = ExampleMethod
ParameterTypeNames = System.Int32;System.String

[Capture]
TraceArguments = false
TraceResult = false
```

The names above are placeholders, not Tainted Grail facts.

Use `ParameterTypeNames` when the method is overloaded. The list is semicolon-separated so assembly-qualified names can still contain commas. For an explicitly selected zero-parameter overload, set:

```ini
ParameterTypeNames = <none>
```

If the method name is overloaded and no signature is supplied, resolution fails rather than choosing one candidate.

## Evidence states

Startup distinguishes:

```text
targetResolution=PASSED / FAILED
patchInstallation=PASSED / FAILED
runtimeObservation=NOT_RUN
```

Once a configured target is observed, the session summary becomes:

```text
runtimeObservation=PASSED
invocationCount=<n>
```

This means the target executed while this tracer was installed in that environment.

It still does not prove:

- that the target is the correct semantic owner;
- that downstream visible behaviour is correct;
- persistence/save safety;
- compatibility with another game build;
- compatibility with every other mod;
- release readiness.

## Argument/result capture

Capture is opt-in.

The renderer records scalar values only for bounded safe categories such as strings, primitives, enums, GUIDs and timestamps. Unknown objects are represented by type name instead of calling arbitrary `ToString()` code.

Arrays are represented by type and length rather than enumerated.

Targets containing values that cannot safely flow through Harmony's boxed observation injections have that capture mode disabled automatically.

## Hot methods

Do not casually trace frame/update loops.

`Limits.MaxEventsPerMinute` bounds logged invocations per one-minute window. Invocation counting continues while output is suppressed.

Start with a narrow interaction that can be triggered manually.

## Exceptions

The finalizer observes exceptions but does not replace or suppress them. Exception logging records the exception type and a bounded message; it intentionally does not dump stack traces by default.

## Public evidence

Before publishing excerpts, follow [Runtime log evidence](../../methods/runtime-log-evidence.md). Local logs may still contain machine or game information and must be reviewed before publication.

## Gate 2 validation status

See [VALIDATION.md](VALIDATION.md) for the gated validation record.


Repository/public-surface validation can prove the source is public-safe. Actual tracer runtime proof requires an installed Mono/BepInEx 5 game environment and remains a separate runtime evidence lane.
