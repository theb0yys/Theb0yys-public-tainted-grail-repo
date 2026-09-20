# Recipe: AI Package Through Avalon AI Runtime

The package describes **AI intent/policy**. The host owns live scheduling/execution.

## Package assembly boundary

Reference `AvalonAI.Contracts.V2` / the current public Contracts assembly.

Do **not** reference the FoA host, Rabbit, GOAP, Blaze or game assemblies from the package.

The V2 package shape is:

```csharp
public sealed class MyPackage : IAvalonAiPackage
{
    public AvalonAiPackageManifest Manifest { get; }
    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies { get; }
    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions { get; }
    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions { get; }
}
```

The manifest declares stable package identity, required Runtime API, goals/actions, required capabilities, blackboard namespace/schema/keys, actor roles, cadence and optional persistent/procedure requirements.

## Runtime flow

```text
feature/provider owns domain truth
→ package exposes provider-neutral AI declarations
→ Runtime registers/evaluates package
→ single host collects approved observations
→ host dispatches only reviewed capabilities
→ native owner executes
```

## Package rules

- return no proposal / no applicable goal when prerequisites are missing;
- use stable IDs;
- declare only the blackboard/capabilities you need;
- do not start your own scheduler for the same actor;
- do not call FoA internals directly from package policy;
- do not reach into another package's private state.
