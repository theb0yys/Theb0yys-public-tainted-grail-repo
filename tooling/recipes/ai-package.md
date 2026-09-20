# Recipe: AI Package Through Avalon AI Runtime

```text
feature/provider owns domain truth
→ package references AvalonAI.Contracts
→ package declares manifest/goals/actions/blackboard
→ Runtime registers/evaluates package
→ single host collects observation
→ host dispatches only reviewed action capability
→ native owner executes
```

## Do not

- reference the host implementation from the package;
- call FoA internals directly from package policy;
- ship a competing scheduler for the same actor;
- reach into Rabbit/GOAP/Blaze internals from third-party package code.

The package describes decisions. The host owns execution authority.
