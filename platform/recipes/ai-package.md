# Recipe: AI Package Through Avalon AI Runtime

The package describes **AI intent/policy**. The host owns live scheduling/execution.

An inert provider-neutral source example is available at [examples/mono/infrastructure/ai-package-contracts](../../examples/mono/infrastructure/ai-package-contracts/README.md).

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

## Runtime flow

```text
feature/provider owns domain truth
→ package exposes provider-neutral AI declarations
→ Runtime registers/evaluates package
→ single host collects approved observations
→ host dispatches only reviewed capabilities
→ native owner executes
```

Do not start your own scheduler for the same actor or call FoA internals directly from package policy.
